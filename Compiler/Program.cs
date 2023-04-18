// Repl

using Compiler.CodeAnalysis;

while (true)
{
    bool showTree = false;
    Console.Write("> ");
    var line = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(line) || line == "exit()")
    {
        return;
    }

    if (line == "showTree()")
    {
        showTree = !showTree;
        Console.WriteLine(showTree ? "Showing parse trees." : "Not showing parse trees");
        continue;
    }

    if (line == "cls()")
    {
        Console.Clear();
        continue;
    }

    var syntaxTree = SyntaxTree.Parse(line);

    var color = Console.ForegroundColor;

    if (showTree)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        PrettyPrint(syntaxTree.Root);
        Console.ForegroundColor = color;
    }

    if (!syntaxTree.Diagnostics.Any())
    {
        var e = new Evaluator(syntaxTree.Root);
        var result = e.Evaluate();
        Console.WriteLine(result);
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        foreach (var diagnostic in syntaxTree.Diagnostics)
        {
            Console.WriteLine(diagnostic);
        }
        Console.ForegroundColor = color;
    }
}

static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
{
    var marker = isLast ? "└──" : "├──";
    Console.Write(indent);
    Console.Write(marker);
    Console.Write(node.Kind);

    if (node is SyntaxToken t && t.Value is not null)
    {
        Console.Write(" ");
        Console.Write(t.Value);
    }

    Console.WriteLine();
    indent += isLast ? "    " : "│   ";


    var lastChild = node.GetChildren().LastOrDefault();

    foreach (var child in node.GetChildren())
    {
        PrettyPrint(child, indent, child == lastChild);
    }
}
