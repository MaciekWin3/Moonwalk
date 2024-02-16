﻿namespace Compiler.CodeAnalysis.Symbols
{
    public sealed class VariableSymbol
    {
        internal VariableSymbol(string name, bool isReadOnly, Type type)
        {
            Name = name;
            IsReadOnly = isReadOnly;
            Type = type;
        }
        public string Name { get; }
        public bool IsReadOnly { get; }
        public Type Type { get; }

        public override string ToString() => Name;
    }
}
