using System.CommandLine;
using OneTypePerFile;

var rootCommand = new RootCommand("Enforces one type per file rule for C# code");

var pathOption = new Option<string>(
    name: "--path",
    description: "Path to directory or solution to analyze",
    getDefaultValue: () => Directory.GetCurrentDirectory()
);

var modeOption = new Option<string>(
    name: "--mode",
    description: "Mode: 'check' (exit code 1 if violations) or 'fix' (split files)",
    getDefaultValue: () => "check"
);

var excludeOption = new Option<string[]>(
    name: "--exclude",
    description: "Patterns to exclude (e.g., '**/obj/**', '**/bin/**')",
    getDefaultValue: () => new[] { "**/obj/**", "**/bin/**", "**/node_modules/**" }
);

var verboseOption = new Option<bool>(
    name: "--verbose",
    description: "Enable verbose output",
    getDefaultValue: () => false
);

var jsonOption = new Option<bool>(
    name: "--json",
    description: "Output results as JSON",
    getDefaultValue: () => false
);

rootCommand.AddOption(pathOption);
rootCommand.AddOption(modeOption);
rootCommand.AddOption(excludeOption);
rootCommand.AddOption(verboseOption);
rootCommand.AddOption(jsonOption);

rootCommand.SetHandler(async (string path, string mode, string[] exclude, bool verbose, bool json) =>
{
    var analyzer = new TypeAnalyzer(verbose);
    var violations = await analyzer.AnalyzeAsync(path, exclude);

    if (json)
    {
        OutputJson(violations);
    }
    else
    {
        OutputConsole(violations, verbose);
    }

    if (mode.ToLower() == "fix")
    {
        var fixer = new TypeFileFixer(verbose);
        await fixer.FixViolationsAsync(violations);
        Console.WriteLine("✓ Files split successfully");
    }
    else if (violations.Any())
    {
        Environment.Exit(1);
    }
}, pathOption, modeOption, excludeOption, verboseOption, jsonOption);

return await rootCommand.InvokeAsync(args);

static void OutputJson(List<FileViolation> violations)
{
    var json = System.Text.Json.JsonSerializer.Serialize(new
    {
        totalFiles = violations.Count,
        totalTypes = violations.Sum(v => v.Types.Count),
        violations = violations.Select(v => new
        {
            filePath = v.FilePath,
            types = v.Types.Select(t => new
            {
                name = t.Name,
                kind = t.Kind,
                line = t.Line
            })
        })
    }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

    Console.WriteLine(json);
}

static void OutputConsole(List<FileViolation> violations, bool verbose)
{
    if (!violations.Any())
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ All files comply with one-type-per-file rule");
        Console.ResetColor();
        return;
    }

    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"✗ Found {violations.Count} file(s) with multiple types:");
    Console.ResetColor();
    Console.WriteLine();

    foreach (var violation in violations)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  {violation.FilePath}");
        Console.ResetColor();

        if (verbose)
        {
            foreach (var type in violation.Types)
            {
                Console.WriteLine($"    - {type.Kind} {type.Name} (line {type.Line})");
            }
            Console.WriteLine();
        }
    }

    Console.WriteLine();
    Console.WriteLine($"Total: {violations.Sum(v => v.Types.Count)} types in {violations.Count} files");
    Console.WriteLine();
    Console.WriteLine("Run with --mode fix to automatically split these files");
}
