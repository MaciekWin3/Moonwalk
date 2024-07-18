using Core.CodeAnalysis.Symbols;

namespace Core.CodeAnalysis.Binding
{
    internal class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression(TypeSymbol type, BoundExpression expression)
        {
            Type = type;
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
        public BoundExpression Expression { get; }
        public override TypeSymbol Type { get; }
    }
}