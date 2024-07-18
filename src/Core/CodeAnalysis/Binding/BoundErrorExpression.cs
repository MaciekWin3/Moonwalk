using Core.CodeAnalysis.Symbols;

namespace Core.CodeAnalysis.Binding
{
    internal class BoundErrorExpression : BoundExpression
    {
        public override TypeSymbol Type => TypeSymbol.Error;

        public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
    }
}
