namespace Compiler.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind) =>
            kind switch
            {
                SyntaxKind.StarToken => 5,
                SyntaxKind.SlashToken => 5,

                SyntaxKind.PlusToken => 4,
                SyntaxKind.MinusToken => 4,

                SyntaxKind.EqualsEqualsToken => 3,
                SyntaxKind.BangEqualsToken => 3,
                SyntaxKind.LessToken => 3,
                SyntaxKind.LessOrEqualsToken => 3,
                SyntaxKind.GreaterToken => 3,
                SyntaxKind.GreaterOrEqualsToken => 3,

                SyntaxKind.AmpersandAmpersandToken => 2,

                SyntaxKind.PipePipeToken => 1,
                _ => 0,
            };

        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind) =>
            kind switch
            {
                SyntaxKind.PlusToken => 6,
                SyntaxKind.MinusToken => 6,
                SyntaxKind.BangToken => 6,
                _ => 0,
            };

        public static SyntaxKind GetKeywordKind(string text) =>
            text switch
            {
                "true" => SyntaxKind.TrueKeyword,
                "false" => SyntaxKind.FalseKeyword,
                "let" => SyntaxKind.LetKeyword,
                "var" => SyntaxKind.VarKeyword,
                "if" => SyntaxKind.IfKeyword,
                "else" => SyntaxKind.ElseKeyword,
                "while" => SyntaxKind.WhileKeyword,
                "for" => SyntaxKind.ForKeyword,
                "in" => SyntaxKind.InKeyword,
                _ => SyntaxKind.IdentifierToken
            };

        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
        {
            var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                if (GetUnaryOperatorPrecedence(kind) > 0)
                {
                    yield return kind;
                }
            }
        }

        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
        {
            var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                if (GetBinaryOperatorPrecedence(kind) > 0)
                {
                    yield return kind;
                }
            }
        }

        public static string GetText(SyntaxKind kind) =>
            kind switch
            {
                SyntaxKind.PlusToken => "+",
                SyntaxKind.MinusToken => "-",
                SyntaxKind.StarToken => "*",
                SyntaxKind.SlashToken => "/",
                SyntaxKind.BangToken => "!",
                SyntaxKind.EqualsToken => "=",
                SyntaxKind.LessToken => "<",
                SyntaxKind.LessOrEqualsToken => "<=",
                SyntaxKind.GreaterToken => ">",
                SyntaxKind.GreaterOrEqualsToken => ">=",
                SyntaxKind.AmpersandAmpersandToken => "&&",
                SyntaxKind.PipePipeToken => "||",
                SyntaxKind.EqualsEqualsToken => "==",
                SyntaxKind.BangEqualsToken => "!=",
                SyntaxKind.OpenParenthesisToken => "(",
                SyntaxKind.CloseParenthesisToken => ")",
                SyntaxKind.OpenBraceToken => "{",
                SyntaxKind.CloseBraceToken => "}",
                SyntaxKind.DotToken => ".",
                SyntaxKind.DotDotToken => "..",
                SyntaxKind.FalseKeyword => "false",
                SyntaxKind.TrueKeyword => "true",
                SyntaxKind.LetKeyword => "let",
                SyntaxKind.VarKeyword => "var",
                SyntaxKind.IfKeyword => "if",
                SyntaxKind.ElseKeyword => "else",
                SyntaxKind.WhileKeyword => "while",
                SyntaxKind.ForKeyword => "for",
                SyntaxKind.InKeyword => "in",
                _ => null!,
            };
    }
}
