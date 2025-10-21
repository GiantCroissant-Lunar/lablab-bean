# OneTypePerFile - C# Code Quality Tool

A Roslyn-based tool that enforces the "one type per file" convention in C# codebases.

## Overview

This tool detects and optionally fixes violations where a single C# file contains multiple type declarations (classes, interfaces, structs, records, enums, delegates).

## Features

- **Accurate Detection**: Uses Roslyn (C# compiler API) for precise syntax analysis
- **Smart Splitting**: Preserves XML documentation, usings, and namespace declarations
- **Two Modes**:
  - `check` - Exit with code 1 if violations found (perfect for CI/git hooks)
  - `fix` - Automatically split files with multiple types
- **Flexible**: Supports both block-scoped and file-scoped namespaces
- **Configurable**: Exclude patterns for obj/bin folders and other directories
- **Rich Output**: Colored console output or JSON for CI integration

## Installation

No installation required! The tool is part of the git-hooks infrastructure.

## Usage

### Command Line

```bash
cd git-hooks/dotnet/OneTypePerFile/OneTypePerFile

# Check for violations (exit code 1 if found)
dotnet run -- --path ../../../../dotnet --mode check

# Check with verbose output
dotnet run -- --path ../../../../dotnet --mode check --verbose

# Fix violations by splitting files
dotnet run -- --path ../../../../dotnet --mode fix --verbose

# Output as JSON
dotnet run -- --path ../../../../dotnet --mode check --json
```

### Options

- `--path <path>` - Path to directory or solution to analyze (default: current directory)
- `--mode <check|fix>` - Mode: 'check' exits with code 1, 'fix' splits files (default: check)
- `--exclude <patterns>` - Patterns to exclude (default: `**/obj/**`, `**/bin/**`, `**/node_modules/**`)
- `--verbose` - Enable verbose output (default: false)
- `--json` - Output results as JSON (default: false)

### Git Hook Integration

The tool is integrated via the `one-type-per-file-check` script in the git-hooks directory.

**Manual run:**
```bash
./git-hooks/one-type-per-file-check
```

**Add to pre-commit:**
Add to `.pre-commit-config.yaml`:

```yaml
repos:
  - repo: local
    hooks:
      - id: one-type-per-file
        name: One Type Per File Check
        entry: ./git-hooks/one-type-per-file-check
        language: script
        pass_filenames: false
```

### Task Integration

You can also add task commands for easier usage:

```bash
# In Taskfile.yml or package.json scripts
task one-type-check   # Run check
task one-type-fix     # Fix violations
```

## How It Works

### Detection

1. Scans all `.cs` files in the specified path
2. Parses each file using Roslyn's `CSharpSyntaxTree`
3. Identifies top-level type declarations (excludes nested types)
4. Reports files with 2+ types

### Fixing

1. Identifies which type should stay in the original file (matches filename or first type)
2. Creates new files for each additional type
3. Preserves:
   - XML documentation comments
   - Using directives
   - Namespace declarations (file-scoped or block-scoped)
   - Original formatting and indentation

### Example

**Before (Events.cs):**
```csharp
using System;

namespace MyApp.Events;

/// <summary>Config changed event</summary>
public record ConfigChangedEvent(string Key, object Value);

/// <summary>Config reloaded event</summary>
public record ConfigReloadedEvent(DateTime Timestamp);
```

**After fixing:**

**ConfigChangedEvent.cs:**
```csharp
using System;

namespace MyApp.Events;

/// <summary>Config changed event</summary>
public record ConfigChangedEvent(string Key, object Value);
```

**ConfigReloadedEvent.cs:**
```csharp
using System;

namespace MyApp.Events;

/// <summary>Config reloaded event</summary>
public record ConfigReloadedEvent(DateTime Timestamp);
```

## Exit Codes

- `0` - Success (no violations found in check mode, or fix completed)
- `1` - Violations found (check mode only)

## Limitations

- **Nested types**: Kept with parent type (intentional - this is usually desired)
- **Partial classes**: Each partial declaration is treated separately
- **Generic constraints**: Preserved as-is
- **File conflicts**: Won't overwrite existing files when fixing

## Development

### Build

```bash
cd git-hooks/dotnet/OneTypePerFile/OneTypePerFile
dotnet build
```

### Test

```bash
# Run on the main dotnet directory
dotnet run -- --path ../../../../dotnet --mode check --verbose
```

### Dependencies

- .NET 8.0
- Microsoft.CodeAnalysis.CSharp 4.8.0
- System.CommandLine 2.0.0-beta4

## License

Part of the Lablab-Bean project.

## See Also

- [JetBrains InspectCode Documentation](https://www.jetbrains.com/help/resharper/InspectCode.html)
- [ReSharper Command Line Tools](https://www.jetbrains.com/help/resharper/ReSharper_Command_Line_Tools.html)
- [Roslyn API Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)
