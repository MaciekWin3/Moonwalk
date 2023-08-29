// Repl

using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Text;
using System.Text;

bool showTree = false;
var variables = new Dictionary<VariableSymbol, object>();
var textBuilder = new StringBuilder();
Compilation previous = null!;

while (true)
{
    if (textBuilder.Length == 0)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("MoonWalk> ");
        Console.ResetColor();
        Console.Write(textBuilder.ToString());
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("MoonWalk| ");
        Console.ResetColor();
    }
    var input = Console.ReadLine();
    var isBlank = string.IsNullOrWhiteSpace(input);

    if (textBuilder.Length == 0)
    {
        if (isBlank)
        {
            break;
        }
        else if (input == "#showTree")
        {
            showTree = !showTree;
            Console.WriteLine(showTree ? "Showing parse trees." : "Not showing parse trees");
            continue;
        }
        else if (input == "cls()")
        {
            Console.Clear();
            continue;
        }
        else if (input == "#reset")
        {
            previous = null!;
            variables.Clear();
            continue;
        }
    }

    textBuilder.AppendLine(input);
    var text = textBuilder.ToString();

    var syntaxTree = SyntaxTree.Parse(text);

    if (!isBlank && syntaxTree.Diagnostics.Any())
    {
        continue;
    }

    var compilation = previous is null ? new Compilation(syntaxTree) : previous.ContinueWith(syntaxTree);
    var result = compilation.Evaluate(variables);

    var diagnostics = result.Diagnostics;

    if (showTree)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        syntaxTree.Root.WriteTo(Console.Out);
        Console.ResetColor();
    }

    if (!diagnostics.Any())
    {
        Console.WriteLine(result.Value);
        previous = compilation;
    }
    else
    {
        foreach (var diagnostic in diagnostics)
        {
            var lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
            var lineNumber = lineIndex + 1;
            var line = syntaxTree.Text.Lines[lineIndex];
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
    textBuilder.Clear();
}


