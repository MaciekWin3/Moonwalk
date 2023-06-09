﻿using Compiler.CodeAnalysis.Binding;
using Compiler.CodeAnalysis.Syntax;

namespace Compiler.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundExpression Root;
        private readonly Dictionary<VariableSymbol, object> variables = new();

        public Evaluator(BoundExpression root, Dictionary<VariableSymbol, object> variables)
        {
            Root = root;
            this.variables = variables;
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

            if (node is BoundVariableExpression v)
            {
                return variables[v.Variable];
            }

            if (node is BoundAssignmentExpression a)
            {
                var value = EvaluateExpression(a.Expression);
                variables[a.Variable] = value;
                return value;
            }

            if (node is BoundUnaryExpression u)
            {
                var operand = EvaluateExpression(u.Operand);

                return u.Op.Kind switch
                {
                    BoundUnaryOperatorKind.Identity => (int)operand,
                    BoundUnaryOperatorKind.Negation => -(int)operand,
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
                BoundBinaryOperatorKind.Equals => Equals(left, right),
                BoundBinaryOperatorKind.NotEquals => !Equals(left, right),
                _ => throw new NotSupportedException($"Error: Unexpected binary operator {syntaxKind}")
            };
    }
}
