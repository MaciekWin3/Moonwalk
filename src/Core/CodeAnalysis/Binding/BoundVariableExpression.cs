using Core.CodeAnalysis.Symbols;

namespace Core.CodeAnalysis.Binding
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public VariableSymbol Variable { get; }
        public override TypeSymbol Type => Variable.Type;
        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
        public BoundVariableExpression(VariableSymbol variable)
        {
            Variable = variable;
        }
    }
}
