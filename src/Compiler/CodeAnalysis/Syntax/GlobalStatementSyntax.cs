using Compiler.CodeAnalysis.Syntax;

namespace Compiler.Tests.CodeAnalysis.Syntax
{
    public sealed class GlobalStatementSyntax : MemberSyntax
    {
        public GlobalStatementSyntax(StatementSyntax statement)
        {
            Statement = statement;
        }

        public StatementSyntax Statement { get; }
        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;

    }
}
