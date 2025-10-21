# Custom Git Hooks

This directory contains custom pre-commit hook scripts that can be used alongside or instead of the pre-commit framework.

## Available Hooks

### Standard Hooks
- `pre-commit` - Runs before commit (checks for TODO, console.log)
- `commit-msg` - Validates commit messages (Conventional Commits format)
- `pre-push` - Runs before push (checks for sensitive data)

### Specialized Checks
- `gitleaks-check` - Detects secrets and credentials
- `yaml-lint` - Lints YAML files
- `markdown-lint` - Lints and fixes Markdown files
- `dotnet-format-check` - Checks .NET code formatting
- `python-check` - Runs black, flake8, isort on Python files
- `validate-agent-pointers` - Ensures agent pointer files are in sync

## Prerequisites

Install the required tools:

```bash
# gitleaks (secrets detection)
# Windows: winget install gitleaks
# macOS: brew install gitleaks
# Linux: https://github.com/gitleaks/gitleaks/releases

# yamllint
pip install yamllint

# markdownlint
npm install -g markdownlint-cli

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

## Usage

### Option 1: Use with pre-commit framework

Add to `.pre-commit-config.yaml`:

```yaml
repos:
  - repo: local
    hooks:
      - id: gitleaks
        name: Gitleaks
        entry: ./git-hooks/gitleaks-check
        language: script
        pass_filenames: false
      
      - id: yaml-lint
        name: YAML Lint
        entry: ./git-hooks/yaml-lint
        language: script
        pass_filenames: false
      
      - id: markdown-lint
        name: Markdown Lint
        entry: ./git-hooks/markdown-lint
        language: script
        pass_filenames: false
      
      - id: dotnet-format
        name: .NET Format Check
        entry: ./git-hooks/dotnet-format-check
        language: script
        pass_filenames: false

      - id: python-check
        name: Python Code Quality
        entry: ./git-hooks/python-check
        language: script
        pass_filenames: false

      - id: validate-agent-pointers
        name: Validate Agent Pointers
        entry: ./git-hooks/validate-agent-pointers
        language: script
        pass_filenames: false
```

### Option 2: Direct Git hooks

Copy scripts to `.git/hooks/`:

```bash
cp git-hooks/pre-commit .git/hooks/pre-commit
chmod +x .git/hooks/pre-commit
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
- **yamllint**: Validates YAML syntax and style
- **markdownlint**: Enforces Markdown style guide
- **dotnet-format**: Ensures consistent C# code formatting
- **python-check**: Runs **Ruff** (modern linter + formatter) and optionally **mypy** (type checker)
  - Ruff replaces: black, flake8, isort, pyupgrade, and 50+ other tools
  - 10-100x faster than traditional Python tools
- **validate-agent-pointers**: Ensures `.agent/` pointer files are synchronized
- **JetBrains CLI**: Full code inspection (too slow for pre-commit, use via task)

## Creating Custom Hooks

1. Create a new script in this directory
2. Make it executable: `chmod +x git-hooks/your-hook.sh`
3. Add it to `.pre-commit-config.yaml` or copy to `.git/hooks/`

## Skipping Hooks

If you need to skip hooks temporarily:

```bash
git commit --no-verify
```

**Use with caution!**
