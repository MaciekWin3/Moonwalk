using Compiler.CodeAnalysis.Syntax.Expressions;

namespace Compiler.CodeAnalysis.Syntax
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax(SyntaxToken forKeyword, SyntaxToken identifier, SyntaxToken inKeyword,
                                  ExpressionSyntax lowerBound, SyntaxToken dotDotToken, ExpressionSyntax upperBound,
                                  StatementSyntax body)
        {
            ForKeyword = forKeyword;
            Identifier = identifier;
            InKeyword = inKeyword;
            LowerBound = lowerBound;
            DotDotToken = dotDotToken;
            UpperBound = upperBound;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.ForStatement;
        public SyntaxToken ForKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken InKeyword { get; }
        public ExpressionSyntax LowerBound { get; }
        public SyntaxToken DotDotToken { get; }
        public ExpressionSyntax UpperBound { get; }
        public StatementSyntax Body { get; }
    }
}
