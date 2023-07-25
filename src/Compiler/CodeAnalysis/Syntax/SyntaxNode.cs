using System.Reflection;

namespace Compiler.CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public IEnumerable<SyntaxNode> GetChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
                {
                    SyntaxNode? child = (SyntaxNode?)property.GetValue(this);
                    if (child is not null)
                    {
                        yield return child;
                    }
                }
                else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
                {
                    var children = (IEnumerable<SyntaxNode>)property.GetValue(this)!;
                    foreach (var child in children)
                    {
                        if (child is not null)
                        {
                            yield return child;
                        }
                    }
                }
            }

        }
    }
}
