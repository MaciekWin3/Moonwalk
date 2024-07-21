using Core.CodeAnalysis;
using Core.CodeAnalysis.Symbols;
using Core.CodeAnalysis.Syntax;
using Core.IO;

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

        [MetaCommand("cls", "Clears the screen")]
        private void EvaluateCls()
        {
            Console.Clear();
        }

        [MetaCommand("reset", "Clears all previous submissions")]
        private void EvaluateReset()
        {
            _previous = null;
            _variables.Clear();
        }

        [MetaCommand("showTree", "Shows the parse tree")]
        private void EvaluateShowTree()
        {
            _showTree = !_showTree;
            Console.WriteLine(_showTree ? "Showing parse trees." : "Not showing parse trees.");
        }

        [MetaCommand("showProgram", "Shows the bound tree")]
        private void EvaluateShowProgram()
        {
            _showProgram = !_showProgram;
            Console.WriteLine(_showProgram ? "Showing bound tree." : "Not showing bound tree.");
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
                Console.Out.WriteDiagnostics(result.Diagnostics);
            }
        }
    }
}
