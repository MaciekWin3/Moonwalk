using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Syntax.Expressions;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly Dictionary<VariableSymbol, object> variables = new();
        private readonly DiagnosticBag diagnostics = new();
        public DiagnosticBag Diagnostics => diagnostics;
        public Binder(Dictionary<VariableSymbol, object> variables)
        {
            this.variables = variables;
        }

        public BoundExpression BindExpression(ExpressionSyntax syntax) =>
            syntax.Kind switch
            {
                SyntaxKind.ParenthesizedExpression => BindParenthesizedExpression(((ParenthesizedExpressionSyntax)syntax)),
                SyntaxKind.LiteralExpression => BindLiteralExpression((LiteralExpressionSyntax)syntax),
                SyntaxKind.NameExpression => BindNameExpression((NameExpressionSyntax)syntax),
                SyntaxKind.AssignmentExpression => BindAssignmentExpression((AssignmentExpressionSyntax)syntax),
                SyntaxKind.UnaryExpression => BindUnaryExpression((UnaryExpressionSyntax)syntax),
                SyntaxKind.BinaryExpression => BindBinaryExpression((BinaryExpressionSyntax)syntax),
                _ => throw new Exception($"Unexpected syntax {syntax.Kind}")
            };

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
        {
            return BindExpression(syntax.Expression);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var variable = variables.Keys.FirstOrDefault(v => v.Name == name);

            if (variable is null)
            {
                diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundLiteralExpression(0);
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);
            var existingVariable = variables.Keys.FirstOrDefault(v => v.Name == name);
            if (existingVariable is not null)
            {
                variables.Remove(existingVariable);
            }
            var variable = new VariableSymbol(name, boundExpression.Type);
            variables[variable] = null!;

            return new BoundAssignmentExpression(variable, boundExpression);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
            if (boundOperator is null)
            {
                diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
                return boundOperand;
            }
            return new BoundUnaryExpression(boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);
            var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperator is null)
            {
                diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return boundLeft;
            }
            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }
    }
}
