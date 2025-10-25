---
doc_id: DOC-2025-00041
title: Windows 'nul' File Prevention
doc_type: guide
status: draft
canonical: false
created: 2025-10-21
tags: [windows, git, pre-commit, troubleshooting, best-practices]
summary: >
  Solution for preventing Windows 'nul' file creation from command redirections being committed to git.
source:
  author: agent
  agent: claude
  model: sonnet-4.5
---

# Windows 'nul' File Prevention

## Problem

On Windows, a file named `nul` keeps appearing in the project root directory and being accidentally committed to git.

### Root Cause

Windows command-line tools (cmd.exe) interpret `2>nul` differently than Unix shells:

**Unix/Linux/macOS**:

```bash
find . -name "*.json" 2>/dev/null  # Redirects to null device
```

**Windows CMD** (INCORRECT):

```cmd
dir /s /b *.json 2>nul  # Creates a file named 'nul' ❌
```

The Windows `nul` device should be `NUL` or `2>NUL`, but even then, some commands create a file instead of using the device.

## Solution

Three-layer defense implemented:

### 1. `.gitignore` Entry ✅

Added to `.gitignore`:

```gitignore
# Windows command redirection file (created by 2>nul)
nul
```

This prevents `nul` from being tracked by git.

### 2. Pre-commit Hook ✅

Created `git-hooks/prevent-nul-file` that:

- Detects if `nul` file is being committed
- Automatically removes it
- Unstages it from the commit
- Warns the developer with proper command alternatives

**Hook registered in `.pre-commit-config.yaml`**:

```yaml
- id: prevent-nul-file
  name: Prevent Windows nul file
  entry: bash git-hooks/prevent-nul-file
  language: system
  pass_filenames: false
  always_run: true
```

### 3. Developer Education

Use proper alternatives for null redirection:

#### Option A: Use PowerShell (Recommended)

```powershell
# Instead of: dir /s /b *.json 2>nul
Get-ChildItem -Recurse -Filter *.json -ErrorAction SilentlyContinue

# Instead of: find . -name "*.json" 2>nul
Get-ChildItem -Recurse -Filter *.json 2>$null
```

#### Option B: Use Git Bash / WSL

```bash
# Unix-style commands work correctly
find . -name "*.json" 2>/dev/null
ls -la *.md 2>/dev/null
```

#### Option C: Don't Redirect (Simple)

```cmd
# Just accept the error output
dir /s /b *.json
```

## Installation

### For New Developers

The pre-commit hook is automatically installed when running:

```bash
pre-commit install
```

### Manual Cleanup

If `nul` file already exists:

```bash
# Remove the file
rm nul

# Unstage if already added
git reset HEAD nul

# Or use PowerShell
Remove-Item nul -Force
```

## How the Pre-commit Hook Works

When you commit, the hook:

1. **Checks staged files** for `nul`

   ```bash
   git diff --cached --name-only | grep -q '^nul$'
   ```

2. **If found**:
   - Shows error message with alternatives
   - Removes the `nul` file
   - Unstages it
   - Prevents the commit (exit 1)

3. **Always checks working directory**:
   - Even if not staged, removes `nul` if found
   - Shows warning message
   - Allows commit to proceed

### Example Output

**When `nul` is staged**:

```
❌ ERROR: Attempting to commit Windows 'nul' file

The file 'nul' is created by Windows command redirections like:
  dir /s /b *.json 2>nul

🔧 Solutions:
  1. Use PowerShell: Get-ChildItem -Recurse -Filter *.json
  2. Use bash/Git Bash: find . -name '*.json' 2>/dev/null
  3. Don't redirect stderr: dir /s /b *.json

🧹 Auto-cleanup: Removing 'nul' file and unstaging...
✅ Fixed! Please commit again.
```

**When `nul` exists but not staged**:

```
⚠️  WARNING: Found 'nul' file in working directory
🧹 Auto-cleanup: Removing it...
✅ Cleaned up.
```

## Best Practices for Cross-Platform Scripts

### ✅ DO

```bash
# Use bash (Git Bash on Windows)
find . -name "*.json" 2>/dev/null

# Use PowerShell
Get-ChildItem -Recurse -Filter *.json -ErrorAction SilentlyContinue

# Use language-specific tools (Python, Node.js)
python -c "import glob; print(glob.glob('**/*.json', recursive=True))"
```

### ❌ DON'T

```cmd
# These create 'nul' file on Windows
dir /s /b *.json 2>nul
find . -name "*.json" 2>nul  (in cmd.exe)
```

## Why This Happens

Windows has special device names that are **case-insensitive**:

- `NUL` - Null device (like `/dev/null`)
- `CON` - Console
- `PRN` - Printer
- `AUX` - Auxiliary
- `COM1-COM9` - Serial ports
- `LPT1-LPT9` - Parallel ports

However, **lowercase** `nul` in some contexts is interpreted as a filename, especially when:

- Running bash commands in cmd.exe
- Using Git Bash with Windows-style redirections
- Mixing Unix and Windows command syntax

## Related Issues

### Similar Windows Reserved Names

If you see files like these, they're also Windows device name conflicts:

- `con` (Console)
- `prn` (Printer)
- `aux` (Auxiliary)

Add them to `.gitignore` if they appear:

```gitignore
nul
con
prn
aux
```

### Claude Code / AI Agents

When using AI coding assistants, they may generate bash commands that:

1. Work correctly in Unix/Linux/macOS
2. Create `nul` files on Windows

**Solution**: Add this to agent instructions:

```markdown
When writing bash commands for cross-platform use:
- Use `2>/dev/null` only in pure bash scripts
- For Windows compatibility, use PowerShell or omit error redirection
- Never use `2>nul` (creates a file on Windows)
```

## Testing the Hook

### Test 1: Prevent Commit

```bash
# Create nul file
echo "test" > nul

# Try to commit it
git add nul
git commit -m "test"
# ❌ Should be blocked and auto-cleaned
```

### Test 2: Auto-cleanup

```bash
# Create nul file (don't stage)
echo "test" > nul

# Commit something else
git add README.md
git commit -m "test"
# ⚠️  Should show warning and remove nul
```

### Test 3: Verify .gitignore

```bash
# Create nul file
echo "test" > nul

# Check git status
git status
# ✅ Should NOT show nul as untracked
```

## Troubleshooting

### Hook Not Running

```bash
# Reinstall pre-commit hooks
pre-commit uninstall
pre-commit install

# Test specific hook
pre-commit run prevent-nul-file --all-files
```

### Manual Cleanup After Commit

If `nul` was already committed:

```bash
# Remove from git history (use with caution!)
git rm --cached nul
git commit -m "chore: remove Windows nul file"

# Or just remove from latest commit
git rm nul
git commit --amend --no-edit
```

## Files Modified

- **`.gitignore`** - Added `nul` entry
- **`git-hooks/prevent-nul-file`** - Pre-commit hook script
- **`.pre-commit-config.yaml`** - Hook configuration

## See Also

- [Pre-commit Hooks Guide](development.md)
- [Git Best Practices](../CONTRIBUTING.md)
- [Windows Development Setup](project-setup.md)

---

**Prevention Status**: ✅ Active
**Auto-cleanup**: ✅ Enabled
**Protection Level**: 3-layer (gitignore + pre-commit + education)
