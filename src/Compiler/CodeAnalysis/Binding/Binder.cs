using Compiler.CodeAnalysis.Lowering;
using Compiler.CodeAnalysis.Symbols;
using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Syntax.Expressions;
using Compiler.CodeAnalysis.Text;
using Compiler.Tests.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag diagnostics = new();
        public FunctionSymbol Function { get; }
        public DiagnosticBag Diagnostics => diagnostics;

        private Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> loopStack = new();
        private int labelCounter;
        private BoundScope scope;

        public Binder(BoundScope parent, FunctionSymbol function)
        {
            scope = new BoundScope(parent);
            Function = function;

            if (function is not null)
            {
                foreach (var p in function.Parameters)
                {
                    scope.TryDeclareVariable(p);
                }
            }
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
        {
            var parentScope = CreateParentScope(previous);
            var binder = new Binder(parentScope, function: null!);

            foreach (var function in syntax.Members.OfType<FunctionDeclarationSyntax>())
            {
                binder.BindFunctionDeclaration(function);
            }

            var statements = ImmutableArray.CreateBuilder<BoundStatement>();

            foreach (var globalStatement in syntax.Members.OfType<GlobalStatementSyntax>())
            {
                var statement = binder.BindStatement(globalStatement.Statement);
                statements.Add(statement);
            }

            var functions = binder.scope.GetDeclaredFunctions();
            var variables = binder.scope.GetDeclaredVariables();
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous is not null)
            {
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);
            }

            return new BoundGlobalScope(previous!, diagnostics, functions, variables, statements.ToImmutable());
        }

        public static BoundProgram BindProgram(BoundGlobalScope globalScope)
        {
            var parentScope = CreateParentScope(globalScope);

            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            var scope = globalScope;
            while (scope is not null)
            {
                foreach (var function in scope.Functions)
                {
                    var binder = new Binder(parentScope, function);
                    var body = binder.BindStatement(function.Declaration?.Body ?? null!);
                    var loweredBody = Lowerer.Lower(body);
                    functionBodies.Add(function, loweredBody);

                    diagnostics.AddRange(binder.Diagnostics);
                }

                scope = scope.Previous;
            }
            var statement = Lowerer.Lower(new BoundBlockStatement(globalScope.Statements));

            return new BoundProgram(diagnostics.ToImmutable(), functionBodies.ToImmutable(), statement);
        }

        private void BindFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            var seenParameterNames = new HashSet<string>();
            foreach (var parameterSyntax in syntax.Parameters)
            {
                var parameterName = parameterSyntax.Identifier.Text;
                var parameterType = BindTypeClause(parameterSyntax.Type);
                if (!seenParameterNames.Add(parameterName))
                {
                    diagnostics.ReportParameterAlreadyDeclared(parameterSyntax.Span, parameterName);
                }
                else
                {
                    var parameter = new ParameterSymbol(parameterName, parameterType);
                    parameters.Add(parameter);
                }
            }

            var type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;

            if (type != TypeSymbol.Void)
            {
                diagnostics.XXX_ReportFunctionsAreUnsupported(syntax.Type.Span);
            }

            var function = new FunctionSymbol(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax);
            if (!scope.TryDeclareFunction(function))
            {
                diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Span, function.Name);
            }
        }

        private static BoundScope CreateParentScope(BoundGlobalScope previous)
        {
            var stack = new Stack<BoundGlobalScope>();
            while (previous is not null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            var parent = CreateRootScope();

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);

                foreach (var f in previous.Functions)
                {
                    scope.TryDeclareFunction(f);
                }

                foreach (var v in previous.Variables)
                {
                    scope.TryDeclareVariable(v);
                }

                parent = scope;
            }

            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            var result = new BoundScope(null!);
            foreach (var f in BuiltinFunctions.GetAll())
            {
                result.TryDeclareFunction(f);
            }
            return result;
        }

        private BoundStatement BindErrorStatement()
        {
            return new BoundExpressionStatement(new BoundErrorExpression());
        }

        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.BlockStatement => BindBlockStatement((BlockStatementSyntax)syntax),
                SyntaxKind.VariableDeclaration => BindVariableDeclaration((VariableDeclarationSyntax)syntax),
                SyntaxKind.ExpressionStatement => BindExpressionStatement((ExpressionStatementSyntax)syntax),
                SyntaxKind.IfStatement => BindIfStatement((IfStatementSyntax)syntax),
                SyntaxKind.WhileStatement => BindWhileStatement((WhileStatementSyntax)syntax),
                SyntaxKind.ForStatement => BindForStatement((ForStatementSyntax)syntax),
                SyntaxKind.BreakStatement => BindBreakStatement((BreakStatementSyntax)syntax),
                SyntaxKind.ContinueStatement => BindContinueStatement((ContinueStatementSyntax)syntax),
                _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
            };
        }

        private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            scope = new BoundScope(scope);

            foreach (var statementSyntax in syntax.Statements)
            {
                var statement = BindStatement(statementSyntax);
                statements.Add(statement);
            }

            scope = scope.Parent;

            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            var isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
            var type = BindTypeClause(syntax.TypeClause);
            var initializer = BindExpression(syntax.Initializer);
            var variableType = type ?? initializer.Type;
            var variable = BindVariable(syntax.Identifier, isReadOnly, variableType);
            var convertedInitializer = BindConversion(syntax.Initializer.Span, initializer, variableType);
            return new BoundVariableDeclaration(variable, convertedInitializer);
        }

        private TypeSymbol BindTypeClause(TypeClauseSyntax syntax)
        {
            if (syntax is null)
            {
                return null!;
            }

            var type = LookupType(syntax.Identifier.Text);
            if (type is null)
            {
                diagnostics.ReportUndefinedType(syntax.Identifier.Span, syntax.Identifier.Text);
                //return TypeSymbol.Error;
            }

            return type!;
        }

        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var thenStatement = BindStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseClause is null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, thenStatement, elseStatement!);
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
        }

        private BoundStatement BindForStatement(ForStatementSyntax syntax)
        {
            var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
            var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

            scope = new BoundScope(scope);

            var variable = BindVariable(syntax.Identifier, isReadOnly: true, TypeSymbol.Int);
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

            scope = scope.Parent;
            return new BoundForStatement(variable, lowerBound, upperBound, body, breakLabel, continueLabel);
        }

        private BoundStatement BindLoopBody(StatementSyntax body, out BoundLabel breakLabel, out BoundLabel continueLabel)
        {
            labelCounter++;
            breakLabel = new BoundLabel($"break{labelCounter}");
            continueLabel = new BoundLabel($"continue{labelCounter}");

            loopStack.Push((breakLabel, continueLabel));
            var boundBody = BindStatement(body);
            loopStack.Pop();
            return boundBody;
        }

        private BoundStatement BindBreakStatement(BreakStatementSyntax syntax)
        {
            if (loopStack.Count == 0)
            {
                diagnostics.ReportInvalidBreakOrContinue(syntax.Keyword.Span, syntax.Keyword.Text);
                return BindErrorStatement();
            }

            var breakLabel = loopStack.Peek().BreakLabel;
            return new BoundGotoStatement(breakLabel);
        }

        private BoundStatement BindContinueStatement(ContinueStatementSyntax syntax)
        {
            if (loopStack.Count == 0)
            {
                diagnostics.ReportInvalidBreakOrContinue(syntax.Keyword.Span, syntax.Keyword.Text);
                return BindErrorStatement();
            }

            var continueLabel = loopStack.Peek().ContinueLabel;
            return new BoundGotoStatement(continueLabel);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            var expression = BindExpression(syntax.Expression, canBeVoid: true);
            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType)
        {
            return BindConversion(syntax, targetType);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            var result = BindExpressionInternal(syntax);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                diagnostics.ReportExpressionMustHaveValue(syntax.Span);
                return new BoundErrorExpression();
            }

            return result;
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax syntax) =>
            syntax.Kind switch
            {
                SyntaxKind.ParenthesizedExpression => BindParenthesizedExpression(((ParenthesizedExpressionSyntax)syntax)),
                SyntaxKind.LiteralExpression => BindLiteralExpression((LiteralExpressionSyntax)syntax),
                SyntaxKind.NameExpression => BindNameExpression((NameExpressionSyntax)syntax),
                SyntaxKind.AssignmentExpression => BindAssignmentExpression((AssignmentExpressionSyntax)syntax),
                SyntaxKind.UnaryExpression => BindUnaryExpression((UnaryExpressionSyntax)syntax),
                SyntaxKind.BinaryExpression => BindBinaryExpression((BinaryExpressionSyntax)syntax),
                SyntaxKind.CallExpression => BindCallExpression((CallExpressionSyntax)syntax),
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
            if (syntax.IdentifierToken.IsMissing)
            {
                // This means the token was inserted by the parser. We already
                // reported error so we can just return an error expression.
                return new BoundErrorExpression();
            }

            if (!scope.TryLookupVariable(name, out var variable))
            {
                diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundErrorExpression();
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);

            if (!scope.TryLookupVariable(name, out var variable))
            {
                diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return boundExpression;
            }

            if (variable.IsReadOnly)
            {
                diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);
            }

            var convertedExpression = BindConversion(syntax.Expression.Span, boundExpression, variable.Type);
            return new BoundAssignmentExpression(variable, convertedExpression);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);

            if (boundOperand.Type == TypeSymbol.Error)
            {
                return new BoundErrorExpression();
            }

            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
            if (boundOperator is null)
            {
                diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
                return new BoundErrorExpression();
            }
            return new BoundUnaryExpression(boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);

            if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
            {
                return new BoundErrorExpression();
            }

            var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperator is null)
            {
                diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return new BoundErrorExpression();
            }
            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }

        private BoundExpression BindCallExpression(CallExpressionSyntax syntax)
        {
            if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is TypeSymbol type)
            {
                return BindConversion(syntax.Arguments[0], type, allowExplicit: true);
            }

            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (var argument in syntax.Arguments)
            {
                boundArguments.Add(BindExpression(argument));
            }

            if (!scope.TryLookupFunction(syntax.Identifier.Text, out var function))
            {
                diagnostics.ReportUndefinedFunction(syntax.Identifier.Span, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (syntax.Arguments.Count != function.Parameters.Length)
            {
                TextSpan span;
                if (syntax.Arguments.Count > function.Parameters.Length)
                {
                    SyntaxNode firstExceedingNode;
                    if (function.Parameters.Length > 0)
                    {
                        firstExceedingNode = syntax.Arguments.GetSeparator(function.Parameters.Length - 1);
                    }
                    else
                    {
                        firstExceedingNode = syntax.Arguments[0];
                    }
                    var lastExceedingArgument = syntax.Arguments[^1];
                    span = TextSpan.FromBounds(firstExceedingNode.Span.Start, lastExceedingArgument.Span.End);
                }
                else
                {
                    span = syntax.CloseParenthesisToken.Span;
                }
                diagnostics.ReportWrongArgumentCount(span, function.Name, function.Parameters.Length, syntax.Arguments.Count);
                return new BoundErrorExpression();
            }


            bool hasErrors = false;
            for (int i = 0; i < syntax.Arguments.Count; i++)
            {
                var argument = boundArguments[i];
                var parameter = function.Parameters[i];

                if (argument.Type != parameter.Type)
                {
                    if (argument.Type != TypeSymbol.Error)
                    {
                        diagnostics.ReportWrongArgumentType(syntax.Arguments[i].Span, parameter.Name, parameter.Type, argument.Type);
                    }
                    hasErrors = true;
                }
            }
            if (hasErrors)
            {
                return new BoundErrorExpression();
            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false)
        {
            var expression = BindExpression(syntax);
            return BindConversion(syntax.Span, expression, type, allowExplicit);
        }

        private BoundExpression BindConversion(TextSpan diagnosticSpan,
            BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
        {
            var conversion = Conversion.Classify(expression.Type, type);

            if (!conversion.Exists)
            {
                if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                {
                    diagnostics.ReportCannotConvert(diagnosticSpan, expression.Type, type);
                }
                return new BoundErrorExpression();
            }

            if (!allowExplicit && conversion.IsExplicit)
            {
                diagnostics.ReportCannotConvertImplicitly(diagnosticSpan, expression.Type, type);
            }

            if (conversion.IsIdentity)
            {
                return expression;
            }

            return new BoundConversionExpression(type, expression);
        }

        private VariableSymbol BindVariable(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
        {
            var name = identifier.Text ?? "?";
            var declare = !identifier.IsMissing;
            var variable = Function is null
                ? (VariableSymbol)new GlobalVariableSymbol(name, isReadOnly, type)
                : new LocalVariableSymbol(name, isReadOnly, type);

            if (declare && !scope.TryDeclareVariable(variable))
            {
                diagnostics.ReportSymbolAlreadyDeclared(identifier.Span, name);
            }

            return variable;
        }

        private TypeSymbol LookupType(string name)
        {
            return name switch
            {
                "bool" => TypeSymbol.Bool,
                "int" => TypeSymbol.Int,
                "string" => TypeSymbol.String,
                _ => null!,
            };
        }
    }
}
