using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OneTypePerFile;

public class TypeAnalyzer
{
    private readonly bool _verbose;

    public TypeAnalyzer(bool verbose)
    {
        _verbose = verbose;
    }

    public async Task<List<FileViolation>> AnalyzeAsync(string path, string[] excludePatterns)
    {
        var violations = new List<FileViolation>();
        var csFiles = GetCSharpFiles(path, excludePatterns);

        if (_verbose)
        {
            Console.WriteLine($"Analyzing {csFiles.Count} C# files...");
        }

        foreach (var file in csFiles)
        {
            var fileViolation = await AnalyzeFileAsync(file);
            if (fileViolation != null)
            {
                violations.Add(fileViolation);
            }
        }

        return violations;
    }

    private async Task<FileViolation?> AnalyzeFileAsync(string filePath)
    {
        var code = await File.ReadAllTextAsync(filePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();

        var types = new List<TypeInfo>();

        // Find all top-level type declarations
        var typeDeclarations = root.DescendantNodes()
            .Where(n => n.Parent is BaseNamespaceDeclarationSyntax or CompilationUnitSyntax)
            .Where(n => n is TypeDeclarationSyntax or DelegateDeclarationSyntax or EnumDeclarationSyntax)
            .ToList();

        if (typeDeclarations.Count <= 1)
        {
            return null; // No violation
        }

        // Extract usings
        var usings = root.DescendantNodes()
            .OfType<UsingDirectiveSyntax>()
            .Select(u => u.ToFullString().Trim())
            .ToList();

        // Get namespace
        var namespaceDecl = root.DescendantNodes()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault();

        string? namespaceName = namespaceDecl?.Name.ToString();

        foreach (var typeDecl in typeDeclarations)
        {
            var typeName = GetTypeName(typeDecl);
            var kind = GetTypeKind(typeDecl);
            var line = tree.GetLineSpan(typeDecl.Span).StartLinePosition.Line + 1;

            // Extract the full source including XML doc comments
            var fullSource = ExtractTypeWithDocComments(typeDecl, root);

            types.Add(new TypeInfo
            {
                Name = typeName,
                Kind = kind,
                Line = line,
                FullSource = fullSource,
                Usings = usings,
                Namespace = namespaceName
            });
        }

        return new FileViolation
        {
            FilePath = filePath,
            Types = types
        };
    }

    private string ExtractTypeWithDocComments(SyntaxNode typeDecl, SyntaxNode root)
    {
        var triviaList = typeDecl.GetLeadingTrivia();
        var docComments = triviaList
            .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                       t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            .ToList();

        if (docComments.Any())
        {
            var docCommentsText = string.Join(Environment.NewLine, docComments.Select(d => d.ToFullString()));
            return docCommentsText + typeDecl.ToFullString();
        }

        return typeDecl.ToFullString();
    }

    private string GetTypeName(SyntaxNode node)
    {
        return node switch
        {
            TypeDeclarationSyntax type => type.Identifier.Text,
            DelegateDeclarationSyntax del => del.Identifier.Text,
            EnumDeclarationSyntax en => en.Identifier.Text,
            _ => "Unknown"
        };
    }

    private string GetTypeKind(SyntaxNode node)
    {
        return node switch
        {
            ClassDeclarationSyntax => "class",
            InterfaceDeclarationSyntax => "interface",
            StructDeclarationSyntax => "struct",
            RecordDeclarationSyntax record => record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) ? "record struct" : "record",
            EnumDeclarationSyntax => "enum",
            DelegateDeclarationSyntax => "delegate",
            _ => "type"
        };
    }

    private List<string> GetCSharpFiles(string path, string[] excludePatterns)
    {
        var files = new List<string>();

        if (File.Exists(path) && path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return new List<string> { path };
        }

        if (!Directory.Exists(path))
        {
            Console.WriteLine($"Path not found: {path}");
            return files;
        }

        var allFiles = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);

        foreach (var file in allFiles)
        {
            var normalizedPath = file.Replace('\\', '/');
            if (!ShouldExclude(normalizedPath, excludePatterns))
            {
                files.Add(file);
            }
        }

        return files;
    }

    private bool ShouldExclude(string path, string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            // Simple glob matching for common patterns
            var normalizedPattern = pattern.Replace('\\', '/').Replace("**/", "");
            var normalizedPath = path.Replace('\\', '/');

            if (normalizedPath.Contains(normalizedPattern.TrimEnd('*').TrimStart('*')))
            {
                return true;
            }
        }

        return false;
    }
}
