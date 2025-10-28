# Roslynator Integration

This document describes the Roslynator code analysis integration in the Lablab-Bean project.

## Overview

[Roslynator](https://josefpihrt.github.io/docs/roslynator/) is a collection of 500+ analyzers, refactorings and fixes for C#. It's integrated into our pre-commit hooks to ensure consistent code quality.

## Integration Points

### Pre-commit Hook

The `roslynator-check` hook runs automatically on every commit for .NET files (`.cs`, `.csproj`, `.sln`):

- **Location**: `git-hooks/checks/dotnet/roslynator-check`
- **Trigger**: When .NET files are staged for commit
- **Action**: Runs `dotnet roslynator analyze --severity-level info`
- **Auto-install**: Installs Roslynator CLI globally if not present

### Task Commands

Manual execution via Task:

```bash
# Analyze code
task dotnet:analyze

# Fix issues automatically
task dotnet:fix

# Run both format and analysis
task dotnet:check
```

### Configuration

Roslynator uses the project's existing `.editorconfig` and MSBuild properties for configuration.

## Usage

### Automatic (Recommended)

Roslynator runs automatically on every commit. If issues are found:

1. Review the analysis output
2. Fix issues manually or run `task dotnet:fix`
3. Stage the fixed files and commit again

### Manual Analysis

```bash
# Analyze current code
task dotnet:analyze

# Fix issues automatically where possible
task dotnet:fix
```

### Advanced Usage

For more advanced scenarios, use the Roslynator CLI directly:

```bash
cd dotnet

# Analyze with custom severity
dotnet roslynator analyze --severity-level warning

# Fix specific analyzers
dotnet roslynator fix --analyzer RCS1001

# List available analyzers
dotnet roslynator list-analyzers
```

## Resources

- **Roslynator Documentation**: <https://josefpihrt.github.io/docs/roslynator/>
- **CLI Commands**: <https://josefpihrt.github.io/docs/roslynator/cli/category/commands/>
- **Analyzers List**: <https://josefpihrt.github.io/docs/roslynator/analyzers/>

## Troubleshooting

### Installation Issues

If Roslynator fails to install automatically:

```bash
# Install manually
dotnet tool install -g roslynator.dotnet.cli

# Verify installation
dotnet tool list -g | grep roslynator
```

### Analysis Failures

If analysis fails:

1. Ensure the .NET solution builds successfully: `task build`
2. Check for compilation errors: `task test`
3. Review Roslynator output for specific issues

### Performance

For large solutions, analysis may take time. Consider:

- Running analysis on specific projects: `dotnet roslynator analyze MyProject.csproj`
- Using severity filters: `--severity-level warning` (instead of `info`)
