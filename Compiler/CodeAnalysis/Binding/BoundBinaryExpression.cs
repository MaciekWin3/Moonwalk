namespace Compiler.CodeAnalysis.Binding
{
    internal class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryOperator Op { get; }
        public BoundExpression Left { get; }
        public BoundExpression Right { get; }
        public override Type Type => Left.Type;
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
        {
            Left = left;
            Op = op;
            Right = right;
        }
    }
}
