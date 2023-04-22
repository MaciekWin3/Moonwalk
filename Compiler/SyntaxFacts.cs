using Compiler.CodeAnalysis.Syntax;

namespace Compiler
{
    internal static class SyntaxFacts
    {
        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind) =>
            kind switch
            {
                SyntaxKind.StarToken => 4,
                SyntaxKind.SlashToken => 4,
                SyntaxKind.PlusToken => 3,
                SyntaxKind.MinusToken => 3,
                SyntaxKind.AmpersandAmpersandToken => 2,
                SyntaxKind.PipePipeToken => 1,
                _ => 0,
            };

        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind) =>
            kind switch
            {
                SyntaxKind.PlusToken => 5,
                SyntaxKind.MinusToken => 5,
                SyntaxKind.BangToken => 5,
                _ => 0,
            };

        public static SyntaxKind GetKeywordKind(string text) =>
            text switch
            {
                "true" => SyntaxKind.TrueKeyword,
                "false" => SyntaxKind.FalseKeyword,
                _ => SyntaxKind.IndentifierToken
            };
    }
}
