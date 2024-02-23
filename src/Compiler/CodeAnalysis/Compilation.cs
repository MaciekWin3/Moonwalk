﻿using Compiler.CodeAnalysis.Binding;
using Compiler.CodeAnalysis.Lowering;
using Compiler.CodeAnalysis.Symbols;
using Compiler.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace Compiler.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope? globalScope;

        public Compilation(SyntaxTree syntaxTree)
            : this(null!, syntaxTree)
        { }

        private Compilation(Compilation previous, SyntaxTree syntaxTree)
        {
            Previous = previous;
            SyntaxTree = syntaxTree;
        }

        public Compilation Previous { get; }
        public SyntaxTree SyntaxTree { get; }

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (globalScope is null)
                {
                    var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope!, SyntaxTree.Root);
                    Interlocked.CompareExchange(ref this.globalScope, globalScope, null);
                }

                return globalScope;
            }
        }

        public Compilation ContinueWith(SyntaxTree syntaxTree)
        {
            return new Compilation(this, syntaxTree);
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var diagnostics = SyntaxTree.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
            {
                return new EvaluationResult(diagnostics, null!);
            }

            var statement = GetStatement();
            var evaluator = new Evaluator(statement, variables);
            var value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }

        public void EmitTree(TextWriter writer)
        {
            var statement = GetStatement();
            statement.WriteTo(writer);
        }

        private BoundBlockStatement GetStatement()
        {
            var result = GlobalScope.Statement;
            return Lowerer.Lower(result);
        }
    }
}
