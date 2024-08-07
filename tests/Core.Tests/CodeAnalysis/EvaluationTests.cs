﻿using Core.CodeAnalysis;
using Core.CodeAnalysis.Symbols;
using Core.CodeAnalysis.Syntax;
using FluentAssertions;
using NUnit.Framework;

namespace Core.Tests.CodeAnalysis
{
    [TestFixture]
    public class EvaluationTests
    {
        [Test]
        [TestCase("1", 1)]
        [TestCase("+1", 1)]
        [TestCase("-1", -1)]
        [TestCase("~1", -2)]
        [TestCase("14 + 12", 26)]
        [TestCase("12 - 3", 9)]
        [TestCase("4 * 2", 8)]
        [TestCase("9 / 3", 3)]
        [TestCase("(10)", 10)]
        [TestCase("12 == 3", false)]
        [TestCase("3 == 3", true)]
        [TestCase("12 != 3", true)]
        [TestCase("3 != 3", false)]
        [TestCase("3 < 4", true)]
        [TestCase("6 < 4", false)]
        [TestCase("3 <= 4", true)]
        [TestCase("3 <= 3", true)]
        [TestCase("5 <= 4", false)]
        [TestCase("8 > 7", true)]
        [TestCase("8 > 9", false)]
        [TestCase("8 >= 7", true)]
        [TestCase("8 >= 8", true)]
        [TestCase("8 >= 9", false)]
        [TestCase("5 > 4", true)]
        [TestCase("1 | 2", 3)]
        [TestCase("1 | 0", 1)]
        [TestCase("1 & 3", 1)]
        [TestCase("1 & 0", 0)]
        [TestCase("1 ^ 0", 1)]
        [TestCase("0 ^ 1", 1)]
        [TestCase("1 ^ 3", 2)]
        [TestCase("false == false", true)]
        [TestCase("true == false", false)]
        [TestCase("false != false", false)]
        [TestCase("true != false", true)]
        [TestCase("true && true", true)]
        [TestCase("false && false", false)]
        [TestCase("false | false", false)]
        [TestCase("false | true", true)]
        [TestCase("true | false", true)]
        [TestCase("true | true", true)]
        [TestCase("false & false", false)]
        [TestCase("false & true", false)]
        [TestCase("true & false", false)]
        [TestCase("true & true", true)]
        [TestCase("false ^ false", false)]
        [TestCase("true ^ false", true)]
        [TestCase("false ^ true", true)]
        [TestCase("true ^ true", false)]
        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("!true", false)]
        [TestCase("!false", true)]
        [TestCase("var a = 10", 10)]
        [TestCase("\"test\"", "test")]
        [TestCase("\"te\"\"st\"", "te\"st")]
        [TestCase("\"test\" == \"test\"", true)]
        [TestCase("\"test\" != \"test\"", false)]
        [TestCase("\"test\" == \"abc\"", false)]
        [TestCase("\"test\" != \"abc\"", true)]
        [TestCase("{ var a = 10 a * a}", 100)]
        [TestCase("!false", true)]
        [TestCase("{ var a = 0 (a = 10) * a }", 100)]
        [TestCase("{ var a = 0 if a == 0 a = 10 a }", 10)]
        [TestCase("{ var a = 0 if a == 5 a = 10 a }", 0)]
        [TestCase("{ var a = 0 if a == 0 a = 10 else a = 20 a }", 10)]
        [TestCase("{ var a = 0 if a == 1 a = 10 else a = 20 a }", 20)]
        [TestCase("{ var i = 10 var result = 0 while i > 0 { result = result + i i = i - 1} result }", 55)]
        [TestCase("{ var result = 0 for i in 1..10 { result = result + i } result }", 55)]
        [TestCase("{ var a = 10 for i in 1..(a = a - 1) { } a }", 9)]
        [TestCase("{ var i = 0 while i < 5 { i = i + 1 if i == 5 continue } i }", 5)]
        public void EvaluatorComputesCorrectValues(string text, object expectedValue)
        {
            AssertValue(text, expectedValue);
        }

        [Test]
        public void EvaluatorVariableDeclarationReportsRedeclaration()
        {
            // Arrange
            var text = @"
                {
                    var x = 10
                    var y = 100
                    {
                        var x = 10
                    }
                    var [x] = 10
                }
            ";

            // Act & Assert
            AssertDiagnostics(text, "'x' is already declared.");
        }

        [Test]
        public void EvaluatorBlockStatementNoInfiniteLoop()
        {
            // Arrange
            var text = @"
                {
                [)][]
            ";

            var diagnostics = @"
                Unexpected token <CloseParenthesisToken>, expected <IdentifierToken>.
                Unexpected token <EndOfFileToken>, expected <CloseBraceToken>.";

            // Act & Assert
            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void EvaluatorFunctionReturnMissing()
        {
            var text = @"
                func [add](a: int, b: int): int
                {
                }
            ";

            var diagnostics = @"
                Not all code paths return a value.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void EvaluatorInvokeFunctionArgumentsMissing()
        {
            var text = @"
                print([)]
            ";

            var diagnostics = @"
                Function 'print' requires 1 arguments but was given 0.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void EvaluatorInvokeFunctionArgumentsExceeding()
        {
            var text = @"
                print(""Hello""[, "" "", "" world!""])
            ";

            var diagnostics = @"
                Function 'print' requires 1 arguments but was given 3.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void EvaluatorInvokeFunctionArgumentsNoInfiniteLoop()
        {
            var text = @"
                print(""Hi""[[=]][)]
            ";

            var diagnostics = @"
                Unexpected token <EqualsToken>, expected <CloseParenthesisToken>.
                Unexpected token <EqualsToken>, expected <IdentifierToken>.
                Unexpected token <CloseParenthesisToken>, expected <IdentifierToken>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void EvaluatorFunctionParametersNoInfiniteLoop()
        {
            var text = @"
                func hi(name: string[[[=]]][)]
                {
                    print(""Hi "" + name + ""!"" )
                }[]
            ";

            var diagnostics = @"
                Unexpected token <EqualsToken>, expected <CloseParenthesisToken>.
                Unexpected token <EqualsToken>, expected <OpenBraceToken>.
                Unexpected token <EqualsToken>, expected <IdentifierToken>.
                Unexpected token <CloseParenthesisToken>, expected <IdentifierToken>.
                Unexpected token <EndOfFileToken>, expected <CloseBraceToken>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void EvaluatorIfStatementReportsCannotConvert()
        {
            // Arrange
            var text = @"
                {
                    var x = 0
                    if [10]
                        x = 10
                }";

            // Act & Assert
            AssertDiagnostics(text, "Cannot convert type 'int' to 'bool'.");
        }

        [Test]
        public void EvaluatorWhileStatementReportsCannotConvert()
        {
            // Arrange
            var text = @"
                {
                    var x = 0
                    while [10]
                        x = 10
                }";

            // Act & Assert
            AssertDiagnostics(text, "Cannot convert type 'int' to 'bool'.");
        }

        [Test]
        public void EvaluatorForStatementReportsCannotConvertLowerBound()
        {
            // Arrange
            var text = @"
                {
                    var result = 0
                    for i in [false]..10
                        result = result + i
                }";

            // Act & Assert
            AssertDiagnostics(text, "Cannot convert type 'bool' to 'int'.");
        }

        [Test]
        public void EvaluatorForStatementReportsCannotConvertUpperBound()
        {
            // Arrange
            var text = @"
                {
                    var result = 0
                    for i in 1..[true]
                        result = result + i
                }";

            // Act & Assert
            AssertDiagnostics(text, "Cannot convert type 'bool' to 'int'.");
        }

        [Test]
        public void EvaluatorUnaryExpressionReportsUndefined()
        {
            // Arrange
            var text = "[+]true";

            // Act & Assert
            AssertDiagnostics(text, "Unary operator '+' is not defined for type 'bool'.");
        }

        [Test]
        public void EvaluatorBinaryExpressionReportsUndefined()
        {
            // Arrange
            var text = "10 [+] true";

            // Act & Assert
            AssertDiagnostics(text, "Binary operator '+' is not defined for types 'int' and 'bool'.");
        }

        [Test]
        public void EvaluatorNameExpressionReportsUndefined()
        {
            // Arrange
            var text = @"[x] * 10";

            // Act & Assert
            AssertDiagnostics(text, "Variable 'x' doesn't exist.");
        }

        [Test]
        public void EvaluatorNameExpressionReportsNoErrorForInsertedToken()
        {
            // Arrange
            var text = @"1 + []";

            // Act & Assert
            AssertDiagnostics(text, "Unexpected token <EndOfFileToken>, expected <IdentifierToken>.");
        }

        [Test]
        public void EvaluatorAssignmentExpressionReportsUndefined()
        {
            // Arrange
            var text = @"[x] = 10";

            // Act & Assert
            AssertDiagnostics(text, "Variable 'x' doesn't exist.");
        }

        [Test]
        public void EvaluatorAssignmentExpressionReportsCannotAssign()
        {
            // Arrange
            var text = @"
                {
                    let x = 10
                    x [=] 100
                }";

            // Act & Assert
            AssertDiagnostics(text, "Variable 'x' is read-only and cannot be assigned to.");
        }

        [Test]
        public void EvaluatorAssignmentExpressionReportsCannotConvert()
        {
            // Arrange
            var text = @"
                {
                    var x = 10
                    x = [true] 
                }";

            // Act & Assert
            AssertDiagnostics(text, "Cannot convert type 'bool' to 'int'.");
        }

        [Test]
        public void EvaluatorVariablesCanShadowFunctions()
        {
            // Arrange
            var text = @"
                {
                    let print = 42
                    [print](""test"")
                }
            ";

            var diagnostics = @"
                'print' is not a function.
            ";

            // Act & Assert
            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void Evaluator_Void_Function_Should_Not_Return_Value()
        {
            var text = @"
                func test()
                {
                    return [1]
                }
            ";

            var diagnostics = @"
                Since the function 'test' does not return a value the 'return' keyword cannot be followed by an expression.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void Evaluator_Function_With_ReturnValue_Should_Not_Return_Void()
        {
            var text = @"
                func test(): int
                {
                    [return]
                }
            ";

            var diagnostics = @"
                An expression of type 'int' is expected.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void Evaluator_Not_All_Code_Paths_Return_Value()
        {
            var text = @"
                func [test](n: int): bool
                {
                    if (n > 10)
                       return true
                }
            ";

            var diagnostics = @"
                Not all code paths return a value.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void Evaluator_Expression_Must_Have_Value()
        {
            var text = @"
                func test(n: int)
                {
                    return
                }
                let value = [test(100)]
            ";

            var diagnostics = @"
                Expression must have a value.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Theory]
        [TestCase("[break]", "break")]
        [TestCase("[continue]", "continue")]
        public void Evaluator_Invalid_Break_Or_Continue(string text, string keyword)
        {
            var diagnostics = $@"
                The keyword '{keyword}' can only be used inside of loops.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void Evaluator_Invalid_Return()
        {
            var text = @"
                [return]
            ";

            var diagnostics = @"
                The 'return' keyword can only be used inside of functions.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void Evaluator_Parameter_Already_Declared()
        {
            var text = @"
                func sum(a: int, b: int, [a: int]): int
                {
                    return a + b + c
                }
            ";

            var diagnostics = @"
                A parameter with the name 'a' already exists.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void Evaluator_Function_Must_Have_Name()
        {
            var text = @"
                func [(]a: int, b: int): int
                {
                    return a + b
                }
            ";

            var diagnostics = @"
                Unexpected token <OpenParenthesisToken>, expected <IdentifierToken>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void Evaluator_Wrong_Argument_Type()
        {
            var text = @"
                func test(n: int): bool
                {
                    return n > 10
                }
                let testValue = ""string""
                test([testValue])
            ";

            var diagnostics = @"
                Parameter 'n' requires a value of type 'int' but was given a value of type 'string'.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void EvaluatorBadType()
        {
            var text = @"
                func test(n: [invalidtype])
                {
                }
            ";

            var diagnostics = @"
                Type 'invalidtype' doesn't exist.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Test]
        public void EvaluatorAssignmentExpressionReportsNotAVariable()
        {
            var text = @"[print] = 42";

            var diagnostics = @"
                'print' is not a variable.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        private static void AssertValue(string text, object expectedValue)
        {
            // Arrange
            var syntaxTree = SyntaxTree.Parse(text);
            var compilation = new Compilation(syntaxTree);
            var variables = new Dictionary<VariableSymbol, object>();

            // Act
            var result = compilation.Evaluate(variables);

            // Assert
            result.Diagnostics.Should().BeEmpty();
            result.Value.Should().Be(expectedValue);
        }

        private static void AssertDiagnostics(string text, string diagnosticsText)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            var expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticsText);

            if (annotatedText.Spans.Length != expectedDiagnostics.Length)
            {
                throw new Exception("ERROR: Must mark as many spans as there are expected diagnostics");
            }

            for (var i = 0; i < expectedDiagnostics.Length; i++)
            {
                var expectedMessage = expectedDiagnostics[i];
                var actualMessage = result.Diagnostics[i].Message;
                expectedMessage.Should().Be(actualMessage);

                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = result.Diagnostics[i].Location.Span;
                expectedSpan.Should().Be(actualSpan);
            }
        }
    }
}
