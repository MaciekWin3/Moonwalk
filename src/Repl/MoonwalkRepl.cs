﻿using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Text;

namespace Repl
{
    internal sealed class MoonwalkRepl : Repl
    {
        private Compilation? previous;
        private bool showTree;
        private bool showProgram;
        private readonly Dictionary<VariableSymbol, object> _variables = new();

        protected override void RenderLine(string line)
        {
            var tokens = SyntaxTree.ParseTokens(line);
            foreach (var token in tokens)
            {
                var isKeyword = token.Kind.ToString().EndsWith("Keyword");
                var isNumber = token.Kind == SyntaxKind.NumberToken;

                if (isKeyword)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                else if (!isNumber)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                Console.Write(token.Text);
                Console.ResetColor();
            }
        }

        protected override void EvaluateMetaCommand(string input)
        {
            switch (input)
            {
                case "#showTree":
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing parse trees." : "Not showing parse trees.");
                    break;
                case "#showProgram":
                    showProgram = !showProgram;
                    Console.WriteLine(showProgram ? "Showing bound tree." : "Not showing bound tree.");
                    break;
                case "#cls":
                    Console.Clear();
                    break;
                case "#reset":
                    previous = null;
                    _variables.Clear();
                    break;
                default:
                    base.EvaluateMetaCommand(input);
                    break;
            }
        }

        protected override bool IsCompleteSubmission(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            var syntaxTree = SyntaxTree.Parse(text);

            if (syntaxTree.Diagnostics.Any())
            {
                return false;
            }

            return true;
        }

        protected override void EvaluateSubmission(string text)
        {
            var syntaxTree = SyntaxTree.Parse(text);

            var compilation = previous is null
                                ? new Compilation(syntaxTree)
                                : previous.ContinueWith(syntaxTree);

            if (showTree)
            {
                syntaxTree.Root.WriteTo(Console.Out);
            }

            if (showProgram)
            {
                compilation.EmitTree(Console.Out);
            }

            var result = compilation.Evaluate(_variables);

            if (!result.Diagnostics.Any())
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(result.Value);
                Console.ResetColor();
                previous = compilation;
            }
            else
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    var lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
                    var line = syntaxTree.Text.Lines[lineIndex];
                    var lineNumber = lineIndex + 1;
                    var character = diagnostic.Span.Start - line.Start + 1;

                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($"({lineNumber}, {character}): ");
                    Console.WriteLine(diagnostic);
                    Console.ResetColor();

                    var prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                    var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

                    var prefix = syntaxTree.Text.ToString(prefixSpan);
                    var error = syntaxTree.Text.ToString(diagnostic.Span);
                    var suffix = syntaxTree.Text.ToString(suffixSpan);

                    Console.Write("    ");
                    Console.Write(prefix);

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(error);
                    Console.ResetColor();

                    Console.Write(suffix);

                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }
    }
}