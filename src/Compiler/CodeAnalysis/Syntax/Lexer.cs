namespace Compiler.CodeAnalysis.Syntax
{
    internal class Lexer
    {
        private readonly string text;
        private int position;
        private DiagnosticBag diagnostics = new DiagnosticBag();
        public DiagnosticBag Diagnostics => diagnostics;

        public Lexer(string text)
        {
            this.text = text;
        }

        private char Current => Peek(0);
        private char Lookahead => Peek(1);
        private char Peek(int offset)
        {
            var index = position + offset;
            if (index >= text.Length)
            {
                return '\0';
            }
            return text[index];
        }

        private void Next()
        {
            position++;
        }

        public SyntaxToken Lex()
        {
            // <numbers>
            // + = * / ( )
            // <whitespace>

            if (position >= text.Length)
            {
                return new SyntaxToken(SyntaxKind.EndOfFileToken, position, "\0", null!);
            }

            int start = position;

            if (char.IsDigit(Current))
            {
                while (char.IsDigit(Current))
                {
                    Next();
                }
                int length = position - start;
                string tokenText = text.Substring(start, length);
                if (!int.TryParse(tokenText, out int value))
                {
                    diagnostics.ReportInvalidNumber(new TextSpan(start, length), text, typeof(int));
                }

                return new SyntaxToken(SyntaxKind.NumberToken, start, tokenText, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                while (char.IsWhiteSpace(Current))
                {
                    Next();
                }
                int length = position - start;
                string tokenText = text.Substring(start, length);
                return new SyntaxToken(SyntaxKind.WhitespaceToken, start, tokenText, null!);
            }

            if (char.IsLetter(Current))
            {
                while (char.IsLetter(Current))
                {
                    Next();
                }
                var length = position - start;
                var tokenText = text.Substring(start, length);
                var kind = SyntaxFacts.GetKeywordKind(tokenText);
                return new SyntaxToken(kind, start, tokenText, null!);
            }

            switch (Current)
            {
                case '+':
                    return new SyntaxToken(SyntaxKind.PlusToken, position++, "+", null!);
                case '-':
                    return new SyntaxToken(SyntaxKind.MinusToken, position++, "-", null!);
                case '*':
                    return new SyntaxToken(SyntaxKind.StarToken, position++, "*", null!);
                case '/':
                    return new SyntaxToken(SyntaxKind.SlashToken, position++, "/", null!);
                case '(':
                    return new SyntaxToken(SyntaxKind.OpenParenthesisToken, position++, "(", null!);
                case ')':
                    return new SyntaxToken(SyntaxKind.CloseParenthesisToken, position++, ")", null!);
                case '&':
                    if (Lookahead == '&')
                    {
                        position += 2;
                        return new SyntaxToken(SyntaxKind.AmpersandAmpersandToken, start, "&&", null!);
                    }
                    break;
                case '|':
                    if (Lookahead == '|')
                    {
                        position += 2;
                        return new SyntaxToken(SyntaxKind.PipePipeToken, start, "||", null!);
                    }
                    break;
                case '=':
                    if (Lookahead == '=')
                    {
                        position += 2;
                        return new SyntaxToken(SyntaxKind.EqualsEqualsToken, start, "==", null!);
                    }
                    else
                    {
                        position += 1;
                        return new SyntaxToken(SyntaxKind.EqualsToken, start, "=", null!);
                    }
                case '!':
                    if (Lookahead == '=')
                    {
                        position += 2;
                        return new SyntaxToken(SyntaxKind.BangEqualsToken, start, "!=", null!);
                    }
                    else
                    {
                        position += 1;
                        return new SyntaxToken(SyntaxKind.BangToken, start, "!", null!);
                    }
            }

            diagnostics.ReportBadNumber(position, Current);
            return new SyntaxToken(SyntaxKind.InvalidToken, position++, text.Substring(position - 1, 1), null!);
        }
    }
}
