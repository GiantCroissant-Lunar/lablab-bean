# Custom Git Hooks

This directory contains organized git hook scripts that can be used alongside or instead of the pre-commit framework.

## Directory Structure

```text
git-hooks/
├── hooks/                      # Main git hook scripts
│   ├── pre-commit              # Runs before commit
│   ├── commit-msg              # Validates commit messages
│   └── pre-push                # Runs before push
├── checks/                     # Individual check scripts
│   ├── general/                # Language-agnostic checks
│   │   ├── gitleaks-check      # Detects secrets and credentials
│   │   ├── yaml-lint           # Lints YAML files
│   │   ├── markdown-lint       # Lints and fixes Markdown files
│   │   ├── prevent-nul-file    # Prevents null file commits
│   │   └── validate-agent-pointers # Ensures agent pointer files are in sync
│   ├── dotnet/                 # .NET specific checks
│   │   ├── dotnet-format-check # Checks .NET code formatting
│   │   ├── one-type-per-file-check # Enforces one type per file for C#
│   │   └── OneTypePerFile/     # Tool implementation
│   └── python/                 # Python specific checks
│       └── python-check        # Runs ruff and mypy on Python files
├── utils/                      # Shared utilities
│   └── common.sh              # Common functions and colors
└── examples/                   # Example configurations
    ├── pre-commit-config.yaml  # Example pre-commit configuration
    └── setup-hooks.sh          # Setup script
```

## Available Hooks

### Standard Hooks (hooks/)

- `pre-commit` - Runs before commit (checks for TODO, console.log)
- `commit-msg` - Validates commit messages (Conventional Commits format)
- `pre-push` - Runs before push (checks for sensitive data)

### General Checks (checks/general/)

- `gitleaks-check` - Detects secrets and credentials
- `prevent-nul-file` - Prevents null file commits
- `validate-agent-pointers` - Ensures agent pointer files are in sync

### .NET Checks (checks/dotnet/)

- `dotnet-format-check` - Checks .NET code formatting
- `one-type-per-file-check` - Enforces one type per file for C# code

### Python Checks (checks/python/)

- `python-check` - Runs ruff and mypy on Python files

## Prerequisites

Install the required tools:

```bash
# gitleaks (secrets detection)
# Windows: winget install gitleaks
# macOS: brew install gitleaks
# Linux: https://github.com/gitleaks/gitleaks/releases

# (YAML and Markdown checks use pre-commit hooks; no separate installs required here)

# dotnet format (included with .NET SDK)
dotnet tool install -g dotnet-format

# Python tools (for python-check)
# Ruff - modern, fast linter and formatter (replaces black, flake8, isort)
pip install ruff

# Optional: Type checking
pip install mypy

# Or install all at once
pip install ruff mypy
```

Note: Markdown and YAML checks are handled via third‑party pre‑commit hooks
configured in `.pre-commit-config.yaml` (markdownlint-cli and pretty-format-yaml).

## Quick Setup

### Automated Setup

```bash
# Run the setup script
./git-hooks/examples/setup-hooks.sh
```

### Manual Setup

#### Option 1: Use with pre-commit framework

Copy the example configuration:

```bash
cp git-hooks/examples/pre-commit-config.yaml .pre-commit-config.yaml
pre-commit install
```

#### Option 2: Direct Git hooks

Copy scripts to `.git/hooks/`:

```bash
cp git-hooks/hooks/pre-commit .git/hooks/pre-commit
cp git-hooks/hooks/commit-msg .git/hooks/commit-msg
cp git-hooks/hooks/pre-push .git/hooks/pre-push
chmod +x .git/hooks/*
```

### Option 3: Run via Task

For slow checks like JetBrains CLI:

```bash
task jb-inspect   # Run JetBrains code inspection
task jb-cleanup   # Run JetBrains code cleanup
```

## Task Commands

```bash
# Format .NET code
task dotnet-format

# Run JetBrains inspection (slow, not in pre-commit)
task jb-inspect

# Run JetBrains cleanup
task jb-cleanup
```

## Notes

- **gitleaks**: Detects hardcoded secrets, API keys, passwords
- **dotnet-format**: Ensures consistent C# code formatting
- **one-type-per-file**: Enforces one type per file for C# (Roslyn-based)
  - Can detect violations (check mode) or automatically split files (fix mode)
  - See [checks/dotnet/OneTypePerFile/README.md](checks/dotnet/OneTypePerFile/README.md) for details
- **python-check**: Runs **Ruff** (modern linter + formatter) and optionally **mypy** (type checker)
  - Ruff replaces: black, flake8, isort, pyupgrade, and 50+ other tools
  - 10-100x faster than traditional Python tools
- **validate-agent-pointers**: Ensures `.agent/` pointer files are synchronized
- **JetBrains CLI**: Full code inspection (too slow for pre-commit, use via task)

## Creating Custom Hooks

1. Create a new script in the appropriate `checks/` subdirectory
2. Use the common utilities from `utils/common.sh` for consistent output
3. Make it executable: `chmod +x git-hooks/checks/category/your-hook`
4. Add it to `.pre-commit-config.yaml` or reference from main hooks
5. Update this README with documentation

### Example Custom Hook

```bash
#!/bin/bash
# Custom check example

set -e

# Source common utilities
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../../utils/common.sh"

# Your check logic here
run_check "My Custom Check" "your-command-here"
```

## Skipping Hooks

If you need to skip hooks temporarily:

```bash
git commit --no-verify
```

**Use with caution!**
