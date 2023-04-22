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

        public int Evaluate()
        {
            return EvaluateExpression(Root);
        }

        private int EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression n)
            {
                return (int)n.Value;
            }

            if (node is BoundUnaryExpression u)
            {
                var operand = EvaluateExpression(u.Operand);

                return u.OperatorKind switch
                {
                    BoundUnaryOperatorKind.Negation => operand,
                    BoundUnaryOperatorKind.Identity => -operand,
                    _ => throw new Exception($"Error: Unexpected unary operator {u.OperatorKind}")
                };
            }

            if (node is BoundBinaryExpression b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);
                return CalculateExpression(b.OperatorKind, left, right);
            }

            throw new Exception($"Error: Unexpected node {node.Kind}");
        }

        private static int CalculateExpression(BoundBinaryOperatorKind syntaxKind, int left, int right) =>
            syntaxKind switch
            {

                BoundBinaryOperatorKind.Addition => left + right,
                BoundBinaryOperatorKind.Subtraction => left - right,
                BoundBinaryOperatorKind.Multiplication => left * right,
                BoundBinaryOperatorKind.Division => left / right,
                _ => throw new NotSupportedException($"Error: Unexpected binary operator {syntaxKind}")
            };
    }
}
