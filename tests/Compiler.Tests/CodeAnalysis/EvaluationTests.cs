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
        [TestCase("false == false", true)]
        [TestCase("true == false", false)]
        [TestCase("false != false", false)]
        [TestCase("true != false", true)]
        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("!true", false)]
        [TestCase("!false", true)]
        [TestCase("{ var a = 0 (a = 10) * a }", 100)]
        public void SyntaxFactGetTextRoundTrips(string text, object expectedValue)
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
    }
}
