using Core.CodeAnalysis.Syntax;
using NUnit.Framework;

namespace Core.Tests.CodeAnalysis.Syntax
{
    internal sealed class AssertingEnumerator : IDisposable
    {
        private readonly IEnumerator<SyntaxNode> enumerator;
        private bool hasErrors;

        public AssertingEnumerator(SyntaxNode node)
        {
            enumerator = Flatten(node).GetEnumerator();
        }

        private bool MarkFailed()
        {
            hasErrors = true;
            return false;
        }

        public void Dispose()
        {
            if (!hasErrors)
            {
                Assert.That(enumerator.MoveNext(), Is.False);
            }

            enumerator.Dispose();
        }

        private static IEnumerable<SyntaxNode> Flatten(SyntaxNode node)
        {
            var stack = new Stack<SyntaxNode>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                var n = stack.Pop();
                yield return n;

                foreach (var child in n.GetChildren().Reverse())
                {
                    stack.Push(child);
                }
            }
        }

        public void AssertNode(SyntaxKind kind)
        {
            try
            {
                Assert.That(enumerator.MoveNext(), Is.True);
                Assert.That(enumerator.Current.Kind, Is.EqualTo(kind));
                Assert.That(enumerator.Current, Is.Not.InstanceOf<SyntaxToken>());
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }

        public void AssertToken(SyntaxKind kind, string text)
        {
            try
            {
                Assert.Multiple(() =>
                {
                    Assert.That(enumerator.MoveNext(), Is.True);
                    Assert.That(enumerator.Current.Kind, Is.EqualTo(kind));
                });
                var token = enumerator.Current;
                Assert.Multiple(() =>
                {
                    Assert.That(token, Is.InstanceOf(typeof(SyntaxToken)));
                    Assert.That(((SyntaxToken)token).Text, Is.EqualTo(text));
                });
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }
    }
}
