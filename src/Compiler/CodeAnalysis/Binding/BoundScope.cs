using Compiler.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        private readonly Dictionary<string, VariableSymbol> variables = new();

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
        }

        public BoundScope Parent { get; }

        public bool TryDeclare(VariableSymbol variable)
        {
            if (variables.ContainsKey(variable.Name))
            {
                return false;
            }

            variables.Add(variable.Name, variable);
            return true;
        }

        public bool TryLookup(string name, out VariableSymbol variable)
        {
            if (variables.TryGetValue(name, out variable!))
            {
                return true;
            }

            if (Parent is null)
            {
                return false;
            }

            return Parent.TryLookup(name, out variable);
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
        {
            return variables.Values.ToImmutableArray();
        }
    }
}
//39:00
