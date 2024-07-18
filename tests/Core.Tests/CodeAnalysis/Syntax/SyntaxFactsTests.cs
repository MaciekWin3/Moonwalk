using Compiler.CodeAnalysis.Syntax;
using FluentAssertions;
using NUnit.Framework;

namespace Compiler.Tests.CodeAnalysis.Syntax
{
    [TestFixture]
    public class SyntaxFactsTests
    {
        [Test]
        [TestCaseSource(nameof(GetSyntaxKindData))]
        public void SyntaxFactGetTextRoundTrips(SyntaxKind kind)
        {
            // Arrange
            var text = SyntaxFacts.GetText(kind);
            if (text is null)
            {
                return;
            }

            // Act
            var tokens = SyntaxTree.ParseTokens(text);
            var token = tokens.First();

            // Assert
            token.Kind.Should().Be(kind);
            token.Text.Should().Be(text);
        }

        private static IEnumerable<object[]> GetSyntaxKindData()
        {
            var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                yield return new object[] { kind };
            }
        }
    }
}
