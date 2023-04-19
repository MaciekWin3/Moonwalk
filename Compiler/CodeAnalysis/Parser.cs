using Compiler.CodeAnalysis.Expressions;

namespace Compiler.CodeAnalysis
{
    internal sealed class Parser
    {
        private readonly SyntaxToken[] tokens;
        private int position;
        private List<string> diagnostics = new();
        public IEnumerable<string> Diagnostics => diagnostics;
        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();
            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();
                if (token.Kind != SyntaxKind.WhitespaceToken && token.Kind != SyntaxKind.InvalidToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            this.tokens = tokens.ToArray();
            diagnostics.AddRange(lexer.Diagnostics);
        }

        private SyntaxToken Peek(int offset)
        {
            int index = position + offset;
            if (index >= tokens.Length)
            {
                return tokens[tokens.Length - 1];
            }

            return tokens[index];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            position++;
            return current;
        }

        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (Current.Kind == kind)
            {
                return NextToken();
            }

            diagnostics.Add($"Error: Unexpected token<{Current.Kind}>, expected <{kind}>");
            return new SyntaxToken(kind, Current.Position, null!, null!);
        }

        public SyntaxTree Parse()
        {
            var expresion = ParseExpression();
            var enfOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(diagnostics, expresion, enfOfFileToken);
        }

        private ExpressionSyntax ParseExpression(int parentPrecedence = 0)
        {
            var left = ParsePrimaryExpression();
            while (true)
            {
                var precedence = GetBinaryOperatorPrecedence(Current.Kind);
                if (precedence == 0 || precedence <= parentPrecedence)
                {
                    break;
                }

                var operatorToken = NextToken();
                var right = ParseExpression(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);

            }
            return left;
        }

        private static int GetBinaryOperatorPrecedence(SyntaxKind kind) => kind switch
        {
            SyntaxKind.StarToken => 2,
            SyntaxKind.SlashToken => 2,
            SyntaxKind.PlusToken => 1,
            SyntaxKind.MinusToken => 1,
            _ => 0,
        };


        private ExpressionSyntax ParsePrimaryExpression()
        {
            if (Current.Kind == SyntaxKind.OpenParenthesisToken)
            {
                var left = NextToken();
                var expression = ParseExpression();
                var right = MatchToken(SyntaxKind.CloseParenthesisToken);
                return new ParenthesizedExpressionSyntax(left, expression, right);

            }
            var numberToken = MatchToken(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(numberToken);
        }
    }
}
