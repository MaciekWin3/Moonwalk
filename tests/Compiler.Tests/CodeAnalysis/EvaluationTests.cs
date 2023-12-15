using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Syntax;
using FluentAssertions;
using NUnit.Framework;

namespace Compiler.Tests.CodeAnalysis
{
    [TestFixture]
    public class EvaluationTests
    {
        [Test]
        [TestCase("1", 1)]
        [TestCase("+1", 1)]
        [TestCase("-1", -1)]
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
        [TestCase("false == false", true)]
        [TestCase("true == false", false)]
        [TestCase("false != false", false)]
        [TestCase("true != false", true)]
        [TestCase("true && true", true)]
        [TestCase("false && false", false)]
        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("!true", false)]
        [TestCase("!false", true)]
        [TestCase("var a = 10", 10)]
        [TestCase("{ var a = 10 a * a}", 100)]
        [TestCase("!false", true)]
        [TestCase("{ var a = 0 (a = 10) * a }", 100)]
        [TestCase("{ var a = 0 if a == 0 a = 10 a }", 10)]
        [TestCase("{ var a = 0 if a == 5 a = 10 a }", 0)]
        [TestCase("{ var a = 0 if a == 0 a = 10 else a = 20 a }", 10)]
        [TestCase("{ var a = 0 if a == 1 a = 10 else a = 20 a }", 20)]
        [TestCase("{ var i = 10 var result = 0 while i > 0 { result = result + i i = i - 1} result }", 55)]
        [TestCase("{ var result = 0 for i in 1..10 { result = result + i } result }", 55)]
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
            AssertDiagnostics(text, "Variable 'x' is already declared.");
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
            AssertDiagnostics(text, "Cannot convert type 'System.Int32' to 'System.Boolean'.");
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
            AssertDiagnostics(text, "Cannot convert type 'System.Int32' to 'System.Boolean'.");
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
            AssertDiagnostics(text, "Cannot convert type 'System.Boolean' to 'System.Int32'.");
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
            AssertDiagnostics(text, "Cannot convert type 'System.Boolean' to 'System.Int32'.");
        }

        [Test]
        public void EvaluatorUnaryExpressionReportsUndefined()
        {
            // Arrange
            var text = "[+]true";

            // Act & Assert
            AssertDiagnostics(text, "Unary operator '+' is not defined for type 'System.Boolean'.");
        }

        [Test]
        public void EvaluatorBinaryExpressionReportsUndefined()
        {
            // Arrange
            var text = "10 [+] true";

            // Act & Assert
            AssertDiagnostics(text, "Binary operator '+' is not defined for types 'System.Int32' and 'System.Boolean'.");
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
            var text = @"[]";

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
            AssertDiagnostics(text, "Cannot convert type 'System.Boolean' to 'System.Int32'.");
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
                var actualSpan = result.Diagnostics[i].Span;
                expectedSpan.Should().Be(actualSpan);
            }
        }
    }
}
