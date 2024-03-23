using Compiler.CodeAnalysis.Symbols;
using System.Collections.Immutable;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class BoundCallExpression : BoundExpression
    {
        public BoundCallExpression(FunctionSymbol function, ImmutableArray<BoundExpression> arguments)
        {
            Function = function;
            Arguments = arguments;
        }
        public override TypeSymbol Type => Function.Type;

        public override BoundNodeKind Kind => BoundNodeKind.CallExpression;

        public FunctionSymbol Function { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }
    }
}
