namespace Compiler.CodeAnalysis.Syntax
{
    internal class Lexer
    {
        private readonly string text;
        private readonly DiagnosticBag diagnostics = new DiagnosticBag();
        private int position;
        private int start;
        private SyntaxKind kind;
        private object? value;
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
            start = position;
            kind = SyntaxKind.InvalidToken;
            value = null!;

            switch (Current)
            {
                case '\0':
                    kind = SyntaxKind.EndOfFileToken;
                    break;
                case '+':
                    kind = SyntaxKind.PlusToken;
                    position++;
                    break;
                case '-':
                    kind = SyntaxKind.MinusToken;
                    position++;
                    break;
                case '*':
                    kind = SyntaxKind.StarToken;
                    position++;
                    break;
                case '/':
                    kind = SyntaxKind.SlashToken;
                    position++;
                    break;
                case '(':
                    kind = SyntaxKind.OpenParenthesisToken;
                    position++;
                    break;
                case ')':
                    kind = SyntaxKind.CloseParenthesisToken;
                    position++;
                    break;
                case '&':
                    if (Lookahead == '&')
                    {
                        kind = SyntaxKind.AmpersandAmpersandToken;
                        position += 2;
                        break;
                    }
                    break;
                case '|':
                    if (Lookahead == '|')
                    {
                        kind = SyntaxKind.PipePipeToken;
                        position += 2;
                        break;
                    }
                    break;
                case '=':
                    if (Lookahead != '=')
                    {
                        kind = SyntaxKind.EqualsToken;
                        position++;
                    }
                    else
                    {
                        position += 2;
                        kind = SyntaxKind.EqualsEqualsToken;
                    }
                    break;
                case '!':
                    if (Lookahead != '=')
                    {
                        kind = SyntaxKind.BangToken;
                        position++;
                    }
                    else
                    {
                        kind = SyntaxKind.BangEqualsToken;
                        position += 2;
                    }
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    ReadNumberToken();
                    break;
                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    ReadWhitespace();
                    break;
                default:
                    if (char.IsLetter(Current))
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else if (char.IsWhiteSpace(Current))
                    {
                        ReadWhitespace();
                    }
                    else
                    {
                        diagnostics.ReportBadNumber(position, Current);
                        position++;
                    }
                    break;
            }

            var length = position - start;
            var text = SyntaxFacts.GetText(kind);
            text ??= this.text.Substring(start, length);

            return new SyntaxToken(kind, start, text, value);

        }

        private void ReadNumberToken()
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

            this.value = value;
            kind = SyntaxKind.NumberToken;
        }

        private void ReadWhitespace()
        {
            while (char.IsWhiteSpace(Current))
            {
                Next();
            }
            kind = SyntaxKind.WhitespaceToken;
        }

        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current))
            {
                Next();
            }
            int length = position - start;
            string tokenText = text.Substring(start, length);
            kind = SyntaxFacts.GetKeywordKind(tokenText);
        }
    }
}
