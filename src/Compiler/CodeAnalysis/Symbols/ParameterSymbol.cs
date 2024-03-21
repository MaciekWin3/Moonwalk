﻿namespace Compiler.CodeAnalysis.Symbols
{
    public sealed class ParameterSymbol : VariableSymbol
    {
        public ParameterSymbol(string name, TypeSymbol type) : base(name, isReadOnly: true, type)
        {
        }

        public override SymbolKind Kind => SymbolKind.Parameter;
    }
}