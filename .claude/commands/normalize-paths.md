# Normalize Paths Command

## Purpose

Scan the codebase for absolute paths and convert them to relative paths for cross-platform compatibility (Windows/Mac/Linux).

## Context

This project must support development on Windows, macOS, and Linux. Absolute paths like `D:\...` or `/home/...` break cross-platform compatibility and violate **R-CODE-004**.

## Task

You must systematically find and fix absolute paths in the codebase.

### Step 1: Scan Configuration Files

Search for absolute paths in these file types:
- `appsettings.json` files (all environments)
- `*.csproj` files
- `package.json` files
- Any other configuration files (`.env` files, config files, etc.)

Look for patterns:
- Windows absolute paths: `D:\`, `C:\`, etc.
- Unix absolute paths: `/home/`, `/usr/`, `/var/`, etc.
- UNC paths: `\\server\share`

### Step 2: Identify Problematic Paths

For each absolute path found:
1. Note the file location
2. Note the current absolute path
3. Determine the appropriate relative path
4. Consider the execution context (where will this run from?)

### Step 3: Convert to Relative Paths

**Configuration Files:**
- Use `./` for same directory
- Use `../` for parent directories
- Use relative paths from project root when appropriate

**C# Code:**
- Use `Path.Combine()` with relative paths
- Use `Directory.GetCurrentDirectory()` as base
- Never hardcode drive letters or absolute paths

**TypeScript/JavaScript:**
- Use `path.join(__dirname, ...)` for relative paths
- Use `./` and `../` in config files
- Use `process.cwd()` when needed

### Step 4: Report Changes

Create a summary report showing:
1. **Files Modified**: List of files changed
2. **Paths Fixed**: Before/After for each change
3. **Manual Review Needed**: Any paths that need human verification
4. **Warnings**: Paths that might break functionality

### Step 5: Validation

After making changes:
1. Verify the application still builds
2. Check if any paths point to non-existent locations
3. Recommend testing on different platforms if possible

## Output Format

Provide a clear, structured report:

```
## Path Normalization Report

### Summary
- Files scanned: X
- Absolute paths found: Y
- Paths converted: Z
- Manual review needed: N

### Changes Made

#### File: path/to/file.json
**Line X**:
- Before: `D:\lunar-snake\personal-work\yokan-projects\lablab-bean\plugins`
- After: `./plugins`
- Reason: Relative to project root

#### File: path/to/another.csproj
**Line Y**:
- Before: `/home/user/project/libs`
- After: `../libs`
- Reason: Relative to solution directory

### Manual Review Needed

- `file.txt:123`: External path to system resource - verify if needed
- `config.json:45`: Path references user-specific directory

### Recommendations

- Test build on Windows, Mac, and Linux
- Update documentation to specify relative path requirements
- Consider adding validation script to prevent absolute paths in commits
```

## Safety Rules

1. **Never delete paths** - only convert them
2. **Preserve path structure** - maintain directory relationships
3. **Backup approach** - if unsure, mark for manual review
4. **Test critical paths** - verify builds still work
5. **Don't modify**:
   - Paths in documentation (unless they're examples)
   - URLs or URIs
   - Package/module identifiers
   - Git URLs

## Execution

Start by scanning the project for absolute paths, then systematically convert them following the rules above.

Begin now.
