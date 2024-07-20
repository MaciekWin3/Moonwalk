using Core.CodeAnalysis;
using Core.CodeAnalysis.Syntax;
using Core.IO;

namespace Compiler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: mwc <source-paths>");
                return;
            }

            if (args.Length > 1)
            {
                Console.Error.WriteLine("Error: Only one file can be compiled at a time.");
                return;
            }

            var path = args.Single();

            if (!File.Exists(path))
            {
                Console.WriteLine($"error: file '{path}' doesn't exist");
                return;
            }

            var syntaxTree = SyntaxTree.Load(path);

            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate([]);

            if (!result.Diagnostics.Any())
            {
                if (result.Value is not null)
                {
                    Console.WriteLine(result.Value);
                }
            }
            else
            {
                Console.Error.WriteDiagnostics(result.Diagnostics);
            }
        }
    }
}
