using Compiler.CodeAnalysis.Symbols;
using System.Collections.Immutable;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        private Dictionary<string, Symbol>? symbols;

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
        }

        public BoundScope Parent { get; }

        public bool TryDeclareVariable(VariableSymbol variable) => TryDeclareSymbol(variable);
        public bool TryDeclareFunction(FunctionSymbol function) => TryDeclareSymbol(function);
        private bool TryDeclareSymbol<TSymbol>(TSymbol symbol) where TSymbol : Symbol
        {
            if (symbols is null)
            {
                symbols = new();
            }
            else if (symbols.ContainsKey(symbol.Name))
            {
                return false;
            }

            symbols.Add(symbol.Name, symbol);
            return true;
        }

        public bool TryLookupVariable(string name, out VariableSymbol variable) => TryLookupSymbol(name, out variable);
        public bool TryLookupFunction(string name, out FunctionSymbol function) => TryLookupSymbol(name, out function);

        private bool TryLookupSymbol<TSymbol>(string name, out TSymbol symbol) where TSymbol : Symbol
        {
            symbol = null!;
            if (symbols is not null && symbols.TryGetValue(name, out var declaredSymbol))
            {
                if (declaredSymbol is TSymbol matchingSymbol)
                {
                    symbol = matchingSymbol;
                    return true;
                }
                return false;
            }

            if (Parent is null)
            {
                return false;
            }

            return Parent.TryLookupSymbol(name, out symbol);
        }


        public ImmutableArray<VariableSymbol> GetDeclaredVariables() => GetDeclaredSymbols<VariableSymbol>();
        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions() => GetDeclaredSymbols<FunctionSymbol>();

        private ImmutableArray<TSymbol> GetDeclaredSymbols<TSymbol>() where TSymbol : Symbol
        {
            if (symbols is null)
            {
                return ImmutableArray<TSymbol>.Empty;
            }
            return symbols.Values.OfType<TSymbol>().ToImmutableArray();
        }
    }
}
