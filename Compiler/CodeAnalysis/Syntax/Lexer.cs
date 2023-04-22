namespace Compiler.CodeAnalysis.Syntax
{
    internal class Lexer
    {
        private readonly string text;
        private int postion;
        private List<string> diagnostics = new();
        public IEnumerable<string> Diagnostics => diagnostics;

        public Lexer(string text)
        {
            this.text = text;
        }

        private char Current => postion >= text.Length ? '\0' : text[postion];

        private void Next()
        {
            postion++;
        }

        public SyntaxToken Lex()
        {
            // <numbers>
            // + = * / ( )
            // <whitespace>

            if (postion >= text.Length)
            {
                return new SyntaxToken(SyntaxKind.EndOfFileToken, postion, "\0", null!);
            }

            if (char.IsDigit(Current))
            {
                int start = postion;
                while (char.IsDigit(Current))
                {
                    Next();
                }
                int length = postion - start;
                string tokenText = text.Substring(start, length);
                if (!int.TryParse(tokenText, out int value))
                {
                    diagnostics.Add($"Error: the number {text} isn't valid Int32.");
                }

                return new SyntaxToken(SyntaxKind.NumberToken, start, tokenText, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                int start = postion;
                while (char.IsWhiteSpace(Current))
                {
                    Next();
                }
                int length = postion - start;
                string tokenText = text.Substring(start, length);
                return new SyntaxToken(SyntaxKind.WhitespaceToken, start, tokenText, null!);
            }

            switch (Current)
            {
                case '+':
                    return new SyntaxToken(SyntaxKind.PlusToken, postion++, "+", null!);
                case '-':
                    return new SyntaxToken(SyntaxKind.MinusToken, postion++, "-", null!);
                case '*':
                    return new SyntaxToken(SyntaxKind.StarToken, postion++, "*", null!);
                case '/':
                    return new SyntaxToken(SyntaxKind.SlashToken, postion++, "/", null!);
                case '(':
                    return new SyntaxToken(SyntaxKind.OpenParenthesisToken, postion++, "(", null!);
                case ')':
                    return new SyntaxToken(SyntaxKind.CloseParenthesisToken, postion++, ")", null!);
            }

            diagnostics.Add($"Error: bad character in input: '{Current}'");
            return new SyntaxToken(SyntaxKind.InvalidToken, postion++, text.Substring(postion - 1, 1), null!);
        }
    }
}
