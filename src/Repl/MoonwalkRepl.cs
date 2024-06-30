using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Symbols;
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
                var isIdentifier = token.Kind == SyntaxKind.IdentifierToken;
                var isNumber = token.Kind == SyntaxKind.NumberToken;
                var isString = token.Kind == SyntaxKind.StringToken;

                if (isKeyword)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                else if (isIdentifier)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }
                else if (isNumber)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else if (isString)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                }
                else
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

            var lastTwoLinesAreBlank = text.Split(Environment.NewLine).Reverse().Take(2).All(string.IsNullOrWhiteSpace);

            if (lastTwoLinesAreBlank)
            {
                return true;
            }

            var syntaxTree = SyntaxTree.Parse(text);

            if (syntaxTree.Root.Members.Last().GetLastToken().IsMissing)
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
                if (result.Value is not null)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                }
                previous = compilation;
            }
            else
            {
                foreach (var diagnostic in result.Diagnostics.OrderBy(diag => diag.Span, new TextSpanComparer()))
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
