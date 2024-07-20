namespace Core.CodeAnalysis.Syntax.Expressions
{
    public sealed class NameExpressionSyntax : ExpressionSyntax
    {
        public SyntaxToken IdentifierToken { get; }

        public override SyntaxKind Kind => SyntaxKind.NameExpression;

        public NameExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken)
            : base(syntaxTree)
        {
            IdentifierToken = identifierToken;
        }
    }
}
