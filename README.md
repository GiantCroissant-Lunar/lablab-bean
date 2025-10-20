# Lablab Bean

A multi-platform development toolkit featuring task automation, web terminal, and cross-platform console applications.

## Features

- 🔨 **Task Automation**: Powered by [Task](https://taskfile.dev)
- 🪝 **Pre-commit Hooks**: Automated code quality checks
- 🌐 **Web Terminal**: Astro.js + xterm.js + node-pty
- 💻 **Console App**: Terminal.Gui v2 TUI application
- 🎮 **Windows App**: SadConsole ASCII graphics application
- ⚛️ **Reactive**: ReactiveUI, System.Reactive, R3
- 🏗️ **Modern .NET**: .NET 8 with Microsoft.Extensions.*

## Project Structure

```
lablab-bean/
├── docs/                    # Documentation
├── git-hooks/              # Custom Git hooks
├── website/                # Node.js workspace (pnpm)
│   ├── apps/web/          # Astro.js web app with terminal
│   └── packages/terminal/ # Terminal backend (node-pty)
└── dotnet/                # .NET 8 solution
    ├── framework/         # Shared libraries
    ├── console-app/       # Terminal.Gui app
    └── windows-app/       # SadConsole app
```

## Prerequisites

### For Task Automation
- [Task](https://taskfile.dev) - `winget install Task.Task` or `brew install go-task/tap/go-task`
- [Pre-commit](https://pre-commit.com) - `pip install pre-commit` or `brew install pre-commit`

### For Website
- Node.js 18+
- pnpm 8+

### For .NET Apps
- .NET 8 SDK

## Quick Start

### Task Automation

```bash
task install              # Install dependencies
task pre-commit-install   # Install pre-commit hooks
task check               # Run all checks
```

### Website (Web Terminal)

```bash
cd website
pnpm install
pnpm dev                 # Start dev server at http://localhost:3000
```

### .NET Console App (Terminal.Gui)

```bash
cd dotnet/console-app/LablabBean.Console
dotnet run
```

### .NET Windows App (SadConsole)

```bash
cd dotnet/windows-app/LablabBean.Windows
dotnet run
```

## Available Tasks

View all available tasks:

```bash
task --list
```

### Common Tasks

- `task init` - Initialize the project
- `task install` - Install project dependencies and tools
- `task check` - Run all validation checks
- `task format` - Format all files
- `task clean` - Clean generated files
- `task info` - Show project information

### Pre-commit Tasks

- `task pre-commit-install` - Install pre-commit hooks
- `task pre-commit-run` - Run pre-commit hooks on all files
- `task pre-commit-update` - Update pre-commit hooks

### Speck-kit Tasks

- `task speck-init` - Initialize speck-kit directories
- `task speck-generate` - Generate from template (requires TEMPLATE and OUTPUT variables)

### Validation Tasks

- `task validate-yaml` - Validate YAML files
- `task validate-markdown` - Validate Markdown files

## Pre-commit Hooks

The project uses pre-commit hooks to ensure code quality. The following hooks are configured:

- **General Checks**: trailing whitespace, end-of-file fixer, YAML/JSON validation, large file detection, private key detection
- **Markdown**: markdownlint with auto-fix
- **YAML**: pretty-format-yaml with auto-fix

To manually run all hooks:

```bash
task pre-commit-run
```

## Speck-kit Integration

Speck-kit provides template-based code generation. Configuration is in `.lablab-bean.yaml`.

### Initialize Speck-kit

```bash
task speck-init
```

### Generate from Template

```bash
task speck-generate TEMPLATE=model OUTPUT=user-model.md
```

### Configuration

Edit `.lablab-bean.yaml` to configure speck-kit templates:

```yaml
speck:
  enabled: true
  templates_dir: ./templates
  output_dir: ./generated
  templates:
    api: ./templates/api.tmpl
    model: ./templates/model.tmpl
  default_variables:
    author: Your Name
    license: MIT
```

### Available Templates

- **model.tmpl**: Generate model documentation with fields and validation rules
- **api.tmpl**: Generate API documentation with endpoints and examples

## Development Workflow

1. **Make changes** to your files
2. **Format and check**:
   ```bash
   task format
   task check
   ```
3. **Commit** your changes (pre-commit hooks will run automatically)
4. **Generate from templates** as needed:
   ```bash
   task speck-generate TEMPLATE=model OUTPUT=my-model.md
   ```

## Components

### 1. Website (Node.js + pnpm)

**Tech Stack:**
- Astro.js (SSR framework)
- React (UI components)
- xterm.js (Terminal emulator)
- node-pty (PTY bindings)
- Tailwind CSS

**Features:**
- Web-based terminal in browser
- Real-time WebSocket communication
- Cross-platform shell support (PowerShell/Bash)

[Read more →](website/README.md)

### 2. .NET Console App (Terminal.Gui)

**Tech Stack:**
- Terminal.Gui v2
- ReactiveUI
- Microsoft.Extensions.*
- Serilog

**Features:**
- Cross-platform TUI
- Menu bar & status bar
- File operations
- Keyboard shortcuts

[Read more →](dotnet/README.md)

### 3. .NET Windows App (SadConsole)

**Tech Stack:**
- SadConsole
- MonoGame
- ReactiveUI
- Microsoft.Extensions.*

**Features:**
- Retro ASCII graphics
- Roguelike-style UI
- Reactive updates

[Read more →](dotnet/README.md)

### 4. Framework Libraries

**LablabBean.Core:**
- Domain models
- Interfaces
- Business logic

**LablabBean.Infrastructure:**
- Dependency injection
- Logging (Serilog)
- Configuration

**LablabBean.Reactive:**
- ReactiveUI ViewModels
- System.Reactive
- ObservableCollections
- MessagePipe
- R3

## Configuration

The project uses `.lablab-bean.yaml` for configuration:

```yaml
project:
  name: lablab-bean
  version: 0.1.0
  description: Task automation and template generation toolkit

speck:
  enabled: true
  templates_dir: ./templates
  output_dir: ./generated
  templates:
    api: ./templates/api.tmpl
    model: ./templates/model.tmpl

pre_commit:
  enabled: true
  auto_install: true
```

## Contributing

1. Install pre-commit hooks: `task pre-commit-install`
2. Make your changes
3. Run checks: `task check`
4. Commit your changes (hooks will run automatically)
5. Submit a pull request

## License

[Add your license here]

## Support

[Add support information here]
