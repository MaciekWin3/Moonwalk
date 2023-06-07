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
                _ => SyntaxKind.IdentifierToken
            };

        public static string GetText(SyntaxKind kind) =>
            kind switch
            {
                SyntaxKind.PlusToken => "+",
                SyntaxKind.MinusToken => "-",
                SyntaxKind.StarToken => "*",
                SyntaxKind.SlashToken => "/",
                SyntaxKind.BangToken => "!",
                SyntaxKind.EqualsToken => "=",
                SyntaxKind.AmpersandAmpersandToken => "&&",
                SyntaxKind.PipePipeToken => "||",
                SyntaxKind.EqualsEqualsToken => "==",
                SyntaxKind.BangEqualsToken => "!=",
                SyntaxKind.OpenParenthesisToken => "(",
                SyntaxKind.CloseParenthesisToken => ")",
                SyntaxKind.FalseKeyword => "false",
                SyntaxKind.TrueKeyword => "true",
                _ => null!,
            };
    }
}
