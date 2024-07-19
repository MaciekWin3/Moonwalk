using Core.CodeAnalysis.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Core.Tests.CodeAnalysis.Text
{
    public class SourceTextTests
    {
        [Test]
        [TestCase(".", 1)]
        [TestCase(".\r\n", 2)]
        [TestCase(".\r\n\r\n", 3)]
        public void SourceText_IncludesLastLine(string text, int expectedLineCount)
        {
            var sourceText = SourceText.From(text);
            expectedLineCount.Should().Be(sourceText.Lines.Length);
        }
    }
}
