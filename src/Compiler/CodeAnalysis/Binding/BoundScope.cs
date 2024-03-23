using Compiler.CodeAnalysis.Symbols;
using System.Collections.Immutable;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        private Dictionary<string, VariableSymbol> variables;
        private Dictionary<string, FunctionSymbol> functions;

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
        }

        public BoundScope Parent { get; }

        public bool TryDeclareVariable(VariableSymbol variable)
        {
            if (variables is null)
            {
                variables = new();
            }

            if (variables.ContainsKey(variable.Name))
            {
                return false;
            }

            variables.Add(variable.Name, variable);
            return true;
        }

        public bool TryLookupVariable(string name, out VariableSymbol variable)
        {
            variable = null!;

            if (variables is not null && variables.TryGetValue(name, out variable!))
            {
                return true;
            }

            if (Parent is null)
            {
                return false;
            }

            return Parent.TryLookupVariable(name, out variable);
        }

        public bool TryDeclareFunction(FunctionSymbol function)
        {
            if (functions is null)
            {
                functions = new();
            }

            if (functions.ContainsKey(function.Name))
            {
                return false;
            }

            functions.Add(function.Name, function);
            return true;
        }

        public bool TryLookupFunction(string name, out FunctionSymbol function)
        {
            function = null!;

            if (functions is not null && functions.TryGetValue(name, out function!))
            {
                return true;
            }

            if (Parent is null)
            {
                return false;
            }

            return Parent.TryLookupFunction(name, out function);
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
        {
            if (variables is null)
            {
                return ImmutableArray<VariableSymbol>.Empty;
            }
            return variables.Values.ToImmutableArray();
        }

        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
        {
            if (functions is null)
            {
                return ImmutableArray<FunctionSymbol>.Empty;
            }
            return functions.Values.ToImmutableArray();
        }
    }
}
