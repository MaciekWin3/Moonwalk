using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Compiler.Tests.CodeAnalysis.Syntax
{
    [TestFixture]
    public class LexerTests
    {
        [Test]
        public void LexerLexesUnterminatedString()
        {
            // Arrange
            var text = "\"text";

            // Act 
            var tokens = SyntaxTree.ParseTokens(text, out var diagnostics);

            // Assert
            var token = tokens.Should().ContainSingle().Which;
            token.Kind.Should().Be(SyntaxKind.StringToken);
            token.Text.Should().Be("\"text");

            var diagnostic = diagnostics.Should().ContainSingle().Which;
            diagnostic.Span.Should().Be(new TextSpan(0, 1));
            diagnostic.Message.Should().Be("Unterminated string literal.");

        }

        [Test]
        public void LexerCoverAllTokens()
        {
            // Arrange
            var tokenKinds = Enum.GetValues(typeof(SyntaxKind))
                .Cast<SyntaxKind>()
                .Where(k => k.ToString().EndsWith("Keyword") ||
                            k.ToString().EndsWith("Token"));

            // Act 
            var testedTokenKinds = GetTokens()
                .Concat(GetSeparators())
                .Select(t => t.kind);

            var untestedTokenKinds = new SortedSet<SyntaxKind>(tokenKinds);
            untestedTokenKinds.Remove(SyntaxKind.InvalidToken);
            untestedTokenKinds.Remove(SyntaxKind.EndOfFileToken);
            untestedTokenKinds.ExceptWith(testedTokenKinds);

            // Assert
            untestedTokenKinds.Should().BeEmpty();
        }

        [Test]
        [TestCaseSource(nameof(GetTokensData))]
        public void LexerLexesToken(SyntaxKind kind, string text)
        {
            // Act 
            var tokens = SyntaxTree.ParseTokens(text);
            var token = tokens.First();

            // Assert
            token.Kind.Should().Be(kind);
            token.Text.Should().Be(text);
        }

        [Test]
        [TestCaseSource(nameof(GetTokenPairsData))]
        public void LexerLexesTokenPairs(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)
        {
            // Arrange
            var text = t1Text + t2Text;

            // Act 
            var tokens = SyntaxTree.ParseTokens(text).ToArray();

            // Assert
            tokens.Length.Should().Be(2);

            tokens[0].Kind.Should().Be(t1Kind);
            tokens[0].Text.Should().Be(t1Text);

            tokens[1].Kind.Should().Be(t2Kind);
            tokens[1].Text.Should().Be(t2Text);
        }

        [Test]
        [TestCaseSource(nameof(GetTokenPairsWithSeparatorData))]
        public void LexerLexesTokenPairsWithSeparators(SyntaxKind t1Kind, string t1Text,
                                         SyntaxKind separatorKind, string separatorText,
                                         SyntaxKind t2Kind, string t2Text)
        {
            // Arrange
            var text = t1Text + separatorText + t2Text;

            // Act 
            var tokens = SyntaxTree.ParseTokens(text).ToArray();

            // Assert
            tokens.Length.Should().Be(3);

            tokens[0].Kind.Should().Be(t1Kind);
            tokens[0].Text.Should().Be(t1Text);

            tokens[1].Kind.Should().Be(separatorKind);
            tokens[1].Text.Should().Be(separatorText);

            tokens[2].Kind.Should().Be(t2Kind);
            tokens[2].Text.Should().Be(t2Text);
        }

        private static IEnumerable<object[]> GetTokensData()
        {
            foreach (var (kind, text) in GetTokens().Concat(GetSeparators()))
            {
                yield return new object[] { kind, text };
            }
        }

        private static IEnumerable<object[]> GetTokenPairsData()
        {
            foreach (var (t1Kind, t1Text, t2Kind, t2Text) in GetTokenPairs())
            {
                yield return new object[] { t1Kind, t1Text, t2Kind, t2Text };
            }
        }

        private static IEnumerable<object[]> GetTokenPairsWithSeparatorData()
        {
            foreach (var (t1Kind, t1Text, separatorKind, separatorText, t2Kind, t2Text) in GetTokenPairsWithSeparator())
            {
                yield return new object[] { t1Kind, t1Text, separatorKind, separatorText, t2Kind, t2Text };
            }
        }

        private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
        {
            var fixedTokens = Enum.GetValues(typeof(SyntaxKind))
                .Cast<SyntaxKind>()
                .Select(k => (kind: k, text: SyntaxFacts.GetText(k)))
                .Where(t => t.text is not null);

            var dynamicTokens = new[]
            {
                (SyntaxKind.NumberToken, "1"),
                (SyntaxKind.NumberToken, "123"),
                (SyntaxKind.IdentifierToken, "a"),
                (SyntaxKind.IdentifierToken, "abc"),
                (SyntaxKind.StringToken, "\"Test\""),
                (SyntaxKind.StringToken, "\"Te\"\"st\""),
            };

            return fixedTokens.Concat(dynamicTokens);
        }

        private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
        {
            return new[]
            {
                (SyntaxKind.WhitespaceToken, " "),
                (SyntaxKind.WhitespaceToken, "  "),
                (SyntaxKind.WhitespaceToken, "\r"),
                (SyntaxKind.WhitespaceToken, "\n"),
                (SyntaxKind.WhitespaceToken, "\r\n"),
                (SyntaxKind.WhitespaceToken, "\t"),
            };
        }

        private static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind)
        {
            var t1IsKeyword = t1Kind.ToString().EndsWith("Keyword");
            var t2IsKeyword = t2Kind.ToString().EndsWith("Keyword");
            return (t1Kind, t2Kind) switch
            {
                (SyntaxKind.IdentifierToken, SyntaxKind.IdentifierToken) => true,
                var (_, _) when t1IsKeyword && t2IsKeyword => true,
                (_, SyntaxKind.IdentifierToken) when t1IsKeyword => true,
                (SyntaxKind.IdentifierToken, _) when t2IsKeyword => true,
                (SyntaxKind.NumberToken, SyntaxKind.NumberToken) => true,
                (SyntaxKind.StringToken, SyntaxKind.StringToken) => true,
                (SyntaxKind.BangToken, SyntaxKind.EqualsToken) => true,
                (SyntaxKind.BangToken, SyntaxKind.EqualsEqualsToken) => true,
                (SyntaxKind.EqualsToken, SyntaxKind.EqualsToken) => true,
                (SyntaxKind.EqualsToken, SyntaxKind.EqualsEqualsToken) => true,
                (SyntaxKind.LessToken, SyntaxKind.EqualsToken) => true,
                (SyntaxKind.LessToken, SyntaxKind.EqualsEqualsToken) => true,
                (SyntaxKind.GreaterToken, SyntaxKind.EqualsToken) => true,
                (SyntaxKind.GreaterToken, SyntaxKind.EqualsEqualsToken) => true,
                (SyntaxKind.DotToken, SyntaxKind.DotToken) => true,
                (SyntaxKind.DotToken, SyntaxKind.DotDotToken) => true,
                (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandToken) => true,
                (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandAmpersandToken) => true,
                (SyntaxKind.PipeToken, SyntaxKind.PipeToken) => true,
                (SyntaxKind.PipeToken, SyntaxKind.PipePipeToken) => true,
                _ => false
            };
        }

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs()
        {
            foreach (var (t1Kind, t1Text) in GetTokens())
            {
                foreach (var (t2Kind, t2Text) in GetTokens())
                {
                    if (!RequiresSeparator(t1Kind, t2Kind))
                    {
                        yield return (t1Kind, t1Text, t2Kind, t2Text);
                    }
                }
            }
        }

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text,
                            SyntaxKind separatorKind, string separatorText,
                            SyntaxKind t2Kind, string t2Text)> GetTokenPairsWithSeparator()
        {
            foreach (var (t1Kind, t1Text) in GetTokens())
            {
                foreach (var (t2Kind, t2Text) in GetTokens())
                {
                    if (RequiresSeparator(t1Kind, t2Kind))
                    {
                        foreach (var (separatorKind, separatorText) in GetSeparators())
                        {
                            yield return (t1Kind, t1Text, separatorKind, separatorText, t2Kind, t2Text);
                        }
                    }
                }
            }
        }
    }
}

