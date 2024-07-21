using Core.CodeAnalysis;
using Core.CodeAnalysis.Symbols;
using Core.CodeAnalysis.Syntax;
using Core.IO;

namespace Repl
{
    internal sealed class MoonwalkRepl : Repl
    {
        private static bool loadingSubmission;
        private Compilation? previous;
        private bool showTree;
        private bool showProgram;
        private readonly Dictionary<VariableSymbol, object> variables = new();

        public MoonwalkRepl()
        {
            LoadSubmissions();
        }

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
            previous = null;
            variables.Clear();
            ClearSubmissions();
        }

        [MetaCommand("showTree", "Shows the parse tree")]
        private void EvaluateShowTree()
        {
            showTree = !showTree;
            Console.WriteLine(showTree ? "Showing parse trees." : "Not showing parse trees.");
        }

        [MetaCommand("showProgram", "Shows the bound tree")]
        private void EvaluateShowProgram()
        {
            showProgram = !showProgram;
            Console.WriteLine(showProgram ? "Showing bound tree." : "Not showing bound tree.");
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

            var result = compilation.Evaluate(variables);

            if (!result.Diagnostics.Any())
            {
                if (result.Value is not null)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                }
                previous = compilation;

                SaveSubmission(text);
            }
            else
            {
                Console.Out.WriteDiagnostics(result.Diagnostics);
            }
        }

        private static string GetSubmissionsDirectory()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var submissionsDirectory = Path.Combine(localAppData, "Minsk", "Submissions");
            return submissionsDirectory;
        }

        private void LoadSubmissions()
        {
            var submissionsDirectory = GetSubmissionsDirectory();
            if (!Directory.Exists(submissionsDirectory))
            {
                return;
            }

            var files = Directory.GetFiles(submissionsDirectory).OrderBy(f => f).ToArray();
            if (files.Length == 0)
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Loaded {files.Length} submission(s)");
            Console.ResetColor();

            loadingSubmission = true;

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                EvaluateSubmission(text);
            }

            loadingSubmission = false;
        }

        private static void ClearSubmissions()
        {
            Directory.Delete(GetSubmissionsDirectory(), recursive: true);
        }

        private static void SaveSubmission(string text)
        {
            if (loadingSubmission)
            {
                return;
            }

            var submissionsDirectory = GetSubmissionsDirectory();
            Directory.CreateDirectory(submissionsDirectory);
            var count = Directory.GetFiles(submissionsDirectory).Length;
            var name = $"submission{count:0000}";
            var fileName = Path.Combine(submissionsDirectory, name);
            File.WriteAllText(fileName, text);
        }
    }
}
