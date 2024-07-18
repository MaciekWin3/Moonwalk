namespace Core.CodeAnalysis.Syntax
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

                SyntaxKind.AmpersandToken => 2,
                SyntaxKind.AmpersandAmpersandToken => 2,

                SyntaxKind.PipeToken => 1,
                SyntaxKind.PipePipeToken => 1,
                SyntaxKind.HatToken => 1,
                _ => 0,
            };

        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind) =>
            kind switch
            {
                SyntaxKind.PlusToken => 6,
                SyntaxKind.MinusToken => 6,
                SyntaxKind.BangToken => 6,
                SyntaxKind.TildeToken => 6,
                _ => 0,
            };

        public static SyntaxKind GetKeywordKind(string text) =>
            text switch
            {
                "break" => SyntaxKind.BreakKeyword,
                "continue" => SyntaxKind.ContinueKeyword,
                "true" => SyntaxKind.TrueKeyword,
                "false" => SyntaxKind.FalseKeyword,
                "let" => SyntaxKind.LetKeyword,
                "return" => SyntaxKind.ReturnKeyword,
                "var" => SyntaxKind.VarKeyword,
                "if" => SyntaxKind.IfKeyword,
                "else" => SyntaxKind.ElseKeyword,
                "while" => SyntaxKind.WhileKeyword,
                "for" => SyntaxKind.ForKeyword,
                "func" => SyntaxKind.FunctionKeyword,
                "in" => SyntaxKind.InKeyword,
                _ => SyntaxKind.IdentifierToken
            };

        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
        {
            var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                if (kind.GetUnaryOperatorPrecedence() > 0)
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
                if (kind.GetBinaryOperatorPrecedence() > 0)
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
                SyntaxKind.TildeToken => "~",
                SyntaxKind.AmpersandToken => "&",
                SyntaxKind.AmpersandAmpersandToken => "&&",
                SyntaxKind.PipeToken => "|",
                SyntaxKind.PipePipeToken => "||",
                SyntaxKind.HatToken => "^",
                SyntaxKind.EqualsEqualsToken => "==",
                SyntaxKind.BangEqualsToken => "!=",
                SyntaxKind.OpenParenthesisToken => "(",
                SyntaxKind.CloseParenthesisToken => ")",
                SyntaxKind.OpenBraceToken => "{",
                SyntaxKind.CloseBraceToken => "}",
                SyntaxKind.ColonToken => ":",
                SyntaxKind.CommaToken => ",",
                SyntaxKind.DotToken => ".",
                SyntaxKind.DotDotToken => "..",
                SyntaxKind.BreakKeyword => "break",
                SyntaxKind.ContinueKeyword => "continue",
                SyntaxKind.ElseKeyword => "else",
                SyntaxKind.FalseKeyword => "false",
                SyntaxKind.ForKeyword => "for",
                SyntaxKind.FunctionKeyword => "func",
                SyntaxKind.TrueKeyword => "true",
                SyntaxKind.IfKeyword => "if",
                SyntaxKind.LetKeyword => "let",
                SyntaxKind.ReturnKeyword => "return",
                SyntaxKind.InKeyword => "in",
                SyntaxKind.WhileKeyword => "while",
                SyntaxKind.VarKeyword => "var",
                _ => null!,
            };
    }
}
