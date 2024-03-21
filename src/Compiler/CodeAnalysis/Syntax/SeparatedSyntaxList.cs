using System.Collections;
using System.Collections.Immutable;

namespace Compiler.CodeAnalysis.Syntax
{
    public abstract class SeparatedSyntaxList
    {
        public abstract ImmutableArray<SyntaxNode> GetWithSeparators();
    }

    public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList, IEnumerable<T>
        where T : SyntaxNode
    {
        public SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodesAndSeparators)
        {
            this.nodesAndSeparators = nodesAndSeparators;
        }

        private readonly ImmutableArray<SyntaxNode> nodesAndSeparators;
        public int Count => (nodesAndSeparators.Length + 1) / 2;
        public T this[int index] => (T)nodesAndSeparators[index * 2];

        public SyntaxToken GetSeparator(int index)
        {
            if (index == Count - 1)
            {
                return null!;
            }

            return (SyntaxToken)nodesAndSeparators[index * 2 + 1];
        }

        public override ImmutableArray<SyntaxNode> GetWithSeparators() => nodesAndSeparators;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
