using Core.CodeAnalysis;
using Core.CodeAnalysis.Syntax;

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

            var text = File.ReadAllText(path);

            var syntaxTree = SyntaxTree.Parse(text);
            var compilation = new Compilation(syntaxTree);
            compilation.Evaluate([]);

        }
    }
}
