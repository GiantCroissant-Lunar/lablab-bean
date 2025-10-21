# OneTypePerFile - Quick Usage Guide

## Quick Start

### Check for Violations

```bash
cd git-hooks/dotnet/OneTypePerFile/OneTypePerFile
dotnet run -- --path ../../../../dotnet --mode check --verbose
```

### Fix All Violations

```bash
cd git-hooks/dotnet/OneTypePerFile/OneTypePerFile
dotnet run -- --path ../../../../dotnet --mode fix --verbose
```

## Common Scenarios

### 1. Check Specific Directory

```bash
dotnet run -- --path /path/to/your/project --mode check
```

### 2. Check Single File

```bash
dotnet run -- --path /path/to/YourFile.cs --mode check
```

### 3. Fix with Custom Exclusions

```bash
dotnet run -- --path ../../../../dotnet --mode fix --exclude "**/obj/**" "**/bin/**" "**/Generated/**"
```

### 4. Get JSON Output for CI

```bash
dotnet run -- --path ../../../../dotnet --mode check --json > violations.json
```

### 5. Use in Pre-commit Hook

The tool is already integrated! Just run:

```bash
../../one-type-per-file-check
```

Or add to `.pre-commit-config.yaml` (see main README).

## Understanding the Output

### Check Mode (Verbose)

```
Analyzing 135 C# files...
✗ Found 43 file(s) with multiple types:

  D:\path\to\Events.cs
    - record ConfigChangedEvent (line 10)
    - record ConfigReloadedEvent (line 33)

Total: 157 types in 43 files

Run with --mode fix to automatically split these files
```

### Fix Mode (Verbose)

```
Splitting: D:\path\to\Events.cs
  Kept ConfigChangedEvent in Events.cs
  Moved ConfigReloadedEvent to ConfigReloadedEvent.cs
✓ Files split successfully
```

### JSON Output

```json
{
  "totalFiles": 43,
  "totalTypes": 157,
  "violations": [
    {
      "filePath": "D:\\path\\to\\Events.cs",
      "types": [
        {
          "name": "ConfigChangedEvent",
          "kind": "record",
          "line": 10
        },
        {
          "name": "ConfigReloadedEvent",
          "kind": "record",
          "line": 33
        }
      ]
    }
  ]
}
```

## What Gets Detected

- ✅ Multiple classes in one file
- ✅ Multiple interfaces in one file
- ✅ Multiple structs in one file
- ✅ Multiple records in one file
- ✅ Multiple enums in one file
- ✅ Multiple delegates in one file
- ✅ Mixed types (e.g., class + interface)
- ❌ Nested types (intentionally ignored - kept with parent)

## Best Practices

### Before Fixing

1. **Commit your current work** - Fix mode modifies files!
2. **Run check mode first** - See what will change
3. **Review the output** - Make sure it's what you expect

### When Fixing

```bash
# 1. Commit current work
git add .
git commit -m "chore: prepare for one-type-per-file fix"

# 2. Run the fix
cd git-hooks/dotnet/OneTypePerFile/OneTypePerFile
dotnet run -- --path ../../../../dotnet --mode fix --verbose

# 3. Review changes
git status
git diff

# 4. Build and test
cd ../../../../dotnet
dotnet build
dotnet test

# 5. Commit the changes
git add .
git commit -m "refactor: split files to one type per file"
```

### After Fixing

- Update `.csproj` files if needed (usually auto-detected by VS/Rider)
- Check that all tests pass
- Verify the build succeeds
- Update any documentation that references file names

## Troubleshooting

### Tool doesn't find any files

- Check the `--path` argument is correct
- Verify `.cs` files exist in that directory
- Check exclusion patterns aren't too broad

### Build fails after fixing

- Make sure all `.csproj` files are updated
- Check for missing `using` directives
- Verify namespace declarations are correct

### "File already exists" warning

The tool won't overwrite existing files. Either:
- Delete the existing file first
- Rename it manually
- Use a different name for the new type

## Integration with CI/CD

### GitHub Actions

```yaml
- name: Check One Type Per File
  run: |
    cd git-hooks/dotnet/OneTypePerFile/OneTypePerFile
    dotnet run -- --path ../../../../dotnet --mode check --json
```

### GitLab CI

```yaml
code-quality:
  script:
    - cd git-hooks/dotnet/OneTypePerFile/OneTypePerFile
    - dotnet run -- --path ../../../../dotnet --mode check
```

### Azure Pipelines

```yaml
- script: |
    cd git-hooks/dotnet/OneTypePerFile/OneTypePerFile
    dotnet run -- --path $(Build.SourcesDirectory)/dotnet --mode check
  displayName: 'Check One Type Per File'
```

## Examples

See the [Examples directory](./Examples) for sample input/output scenarios (if available).

## Need Help?

- Read the [full README](./README.md)
- Check the [main git-hooks README](../../README.md)
- Review the source code for advanced usage
