using Microsoft.CodeAnalysis.CSharp;

namespace OneTypePerFile;

public class TypeFileFixer
{
    private readonly bool _verbose;

    public TypeFileFixer(bool verbose)
    {
        _verbose = verbose;
    }

    public async Task FixViolationsAsync(List<FileViolation> violations)
    {
        foreach (var violation in violations)
        {
            await FixFileAsync(violation);
        }
    }

    private async Task FixFileAsync(FileViolation violation)
    {
        if (_verbose)
        {
            Console.WriteLine($"Splitting: {violation.FilePath}");
        }

        var directory = Path.GetDirectoryName(violation.FilePath);
        if (directory == null)
        {
            Console.WriteLine($"Cannot determine directory for: {violation.FilePath}");
            return;
        }

        // Determine which type should stay in the original file
        // Strategy: Keep the type whose name matches the file name
        var originalFileName = Path.GetFileNameWithoutExtension(violation.FilePath);
        var typeToKeep = violation.Types.FirstOrDefault(t => t.Name == originalFileName)
                         ?? violation.Types.First(); // Fallback to first type

        var typesToMove = violation.Types.Where(t => t != typeToKeep).ToList();

        // Create new files for moved types
        foreach (var type in typesToMove)
        {
            await CreateNewFileForTypeAsync(type, directory);
        }

        // Update the original file to contain only the kept type
        await UpdateOriginalFileAsync(violation.FilePath, typeToKeep);

        if (_verbose)
        {
            Console.WriteLine($"  Kept {typeToKeep.Name} in {Path.GetFileName(violation.FilePath)}");
            foreach (var type in typesToMove)
            {
                Console.WriteLine($"  Moved {type.Name} to {type.Name}.cs");
            }
        }
    }

    private async Task CreateNewFileForTypeAsync(TypeInfo type, string directory)
    {
        var fileName = $"{type.Name}.cs";
        var filePath = Path.Combine(directory, fileName);

        // Check if file already exists
        if (File.Exists(filePath))
        {
            Console.WriteLine($"Warning: File already exists, skipping: {filePath}");
            return;
        }

        var content = BuildFileContent(type);
        await File.WriteAllTextAsync(filePath, content);
    }

    private async Task UpdateOriginalFileAsync(string filePath, TypeInfo typeToKeep)
    {
        var content = BuildFileContent(typeToKeep);
        await File.WriteAllTextAsync(filePath, content);
    }

    private string BuildFileContent(TypeInfo type)
    {
        var lines = new List<string>();

        // Add usings
        if (type.Usings.Any())
        {
            foreach (var usingDirective in type.Usings)
            {
                lines.Add(usingDirective);
            }
            lines.Add(""); // Blank line after usings
        }

        // Add namespace and type
        if (!string.IsNullOrEmpty(type.Namespace))
        {
            // Check if the original source uses file-scoped namespace
            var tree = CSharpSyntaxTree.ParseText(type.FullSource);
            var root = tree.GetRootAsync().Result;
            var hasFileScopedNamespace = root.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax>()
                .Any();

            if (hasFileScopedNamespace)
            {
                lines.Add($"namespace {type.Namespace};");
                lines.Add("");
                lines.Add(type.FullSource.Trim());
            }
            else
            {
                lines.Add($"namespace {type.Namespace}");
                lines.Add("{");
                lines.Add(IndentCode(type.FullSource.Trim(), 1));
                lines.Add("}");
            }
        }
        else
        {
            // No namespace
            lines.Add(type.FullSource.Trim());
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private string IndentCode(string code, int levels)
    {
        var indent = new string(' ', levels * 4);
        var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        return string.Join(Environment.NewLine, lines.Select(line =>
        {
            if (string.IsNullOrWhiteSpace(line))
                return line;
            return indent + line;
        }));
    }
}
