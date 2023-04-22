namespace Compiler.CodeAnalysis.Binding
{
    internal class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryOperatorKind OperatorKind { get; }
        public BoundExpression Left { get; }
        public BoundExpression Right { get; }
        public override Type Type => Left.Type;
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public BoundBinaryExpression(BoundExpression left, BoundBinaryOperatorKind operatorKind, BoundExpression right)
        {
            Left = left;
            OperatorKind = operatorKind;
            Right = right;
        }
    }
}
