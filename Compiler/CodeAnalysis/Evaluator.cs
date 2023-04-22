using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Syntax.Expressions;

namespace Compiler.CodeAnalysis
{
    public sealed class Evaluator
    {
        public ExpressionSyntax Root { get; }

        public Evaluator(ExpressionSyntax root)
        {
            Root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(Root);
        }

        public static int CalculateExpression(SyntaxKind syntaxKind, int left, int right) =>
            syntaxKind switch
            {
                SyntaxKind.PlusToken => left + right,
                SyntaxKind.MinusToken => left - right,
                SyntaxKind.StarToken => left * right,
                SyntaxKind.SlashToken => left / right,
                _ => throw new NotSupportedException($"Error: Unexpected binary operator {syntaxKind}")
            };

        private int EvaluateExpression(ExpressionSyntax node)
        {
            if (node is LiteralExpressionSyntax n)
            {
                return (int)n.LiteralToken.Value;
            }

            if (node is UnaryExpressionSyntax u)
            {
                var operand = EvaluateExpression(u.Operand);

                if (u.OperatorToken.Kind == SyntaxKind.PlusToken)
                {
                    return operand;
                }

                if (u.OperatorToken.Kind == SyntaxKind.MinusToken)
                {
                    return -operand;
                }

                throw new Exception($"Error: Unexpected unary operator {u.OperatorToken.Kind}");
            }

            if (node is BinaryExpressionSyntax b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);
                return CalculateExpression(b.OperatorToken.Kind, left, right);
            }

            if (node is ParenthesizedExpressionSyntax p)
            {
                return EvaluateExpression(p.Expression);
            }
            throw new Exception($"Error: Unexpected node {node.Kind}");
        }
    }
}
