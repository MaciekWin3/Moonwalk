using Compiler.CodeAnalysis.Binding;

namespace Compiler.CodeAnalysis
{
    internal sealed class Evaluator
    {
        public BoundExpression Root { get; }

        public Evaluator(BoundExpression root)
        {
            Root = root;
        }

        public object Evaluate()
        {
            return EvaluateExpression(Root);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression n)
            {
                return n.Value;
            }

            if (node is BoundUnaryExpression u)
            {
                var operand = EvaluateExpression(u.Operand);

                return u.Op.Kind switch
                {
                    BoundUnaryOperatorKind.Negation => (int)operand,
                    BoundUnaryOperatorKind.Identity => -(int)operand,
                    BoundUnaryOperatorKind.LogicalNegation => !(bool)operand,
                    _ => throw new Exception($"Error: Unexpected unary operator {u.Op}")
                };
            }

            if (node is BoundBinaryExpression b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);
                return CalculateExpression(b.Op.Kind, left, right);
            }

            throw new Exception($"Error: Unexpected node {node.Kind}");
        }

        private static object CalculateExpression(BoundBinaryOperatorKind syntaxKind, object left, object right) =>
            syntaxKind switch
            {
                BoundBinaryOperatorKind.Addition => (int)left + (int)right,
                BoundBinaryOperatorKind.Subtraction => (int)left - (int)right,
                BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
                BoundBinaryOperatorKind.Division => (int)left / (int)right,
                BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
                BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
                _ => throw new NotSupportedException($"Error: Unexpected binary operator {syntaxKind}")
            };
    }
}
