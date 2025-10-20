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

### 🔧 Development Mode (Recommended for Development)

Start the full stack with hot reload for rapid development:

```bash
# One-command development stack with hot reload
task dev-stack

# Or step by step:
task build              # Build .NET components (one-time)
task dev-stack          # Start dev stack with hot reload
task dev-status         # Check status
task dev-logs           # View logs
task dev-stop           # Stop the stack
```

**What runs in development mode:**
- 🌐 **Astro Dev Server** - http://localhost:3000 (hot reload enabled)
- 🔌 **PTY Terminal Backend** - TypeScript watch mode
- 💻 **Console App** - .NET development mode

**Key Features:**
- ✨ Astro hot reload - instant UI updates
- 🔄 TypeScript watch mode for terminal backend
- 🚀 Fast iteration without rebuilding
- 📦 Uses local PM2 (no global installation needed)

### 🚀 Production Build & Run

Build and run the complete stack from versioned artifacts:

```bash
# One-command build and run
task release-and-run

# Or step by step:
task build-release       # Build versioned artifacts
task stack-run          # Start the stack
task stack-status       # Check status
task stack-logs         # View logs
```

**Quick Script (Windows):**
```powershell
.\build\scripts\build-and-run.ps1
```

The stack includes:
- 🌐 **Web App** - http://localhost:3000 (bundled Astro)
- 💻 **Console App** - Terminal.Gui TUI
- 🎮 **Windows App** - SadConsole GUI

See [docs/RELEASE.md](docs/RELEASE.md) for complete release documentation.

### 🛠️ Manual Component Development

#### Task Automation

```bash
task install              # Install dependencies
task pre-commit-install   # Install pre-commit hooks
task check               # Run all checks
```

#### Website (Web Terminal)

```bash
cd website
pnpm install
pnpm dev                 # Start dev server at http://localhost:3000
```

#### .NET Console App (Terminal.Gui)

```bash
cd dotnet/console-app/LablabBean.Console
dotnet run
```

#### .NET Windows App (SadConsole)

```bash
cd dotnet/windows-app/LablabBean.Windows
dotnet run
```

## Available Tasks

View all available tasks:

```bash
task --list
```

### Development Stack Tasks

- `task dev-stack` - Start development stack with hot reload
- `task dev-stop` - Stop development stack
- `task dev-restart` - Restart development stack
- `task dev-status` - Show development stack status
- `task dev-logs` - View development logs (live)
- `task dev-delete` - Delete development stack from PM2

### Release & Production Stack Tasks

- `task build-release` - Build complete release with versioned artifacts
- `task release-and-run` - Build and start the full stack (one command)
- `task stack-run` - Start production stack from versioned artifacts
- `task stack-stop` - Stop all PM2 processes
- `task stack-restart` - Restart production stack
- `task stack-status` - Show PM2 stack status
- `task stack-logs` - View all logs (live)
- `task stack-logs-web` - View web app logs only
- `task stack-logs-console` - View console app logs only
- `task stack-monit` - Open PM2 monitoring dashboard
- `task stack-delete` - Delete all PM2 processes
- `task list-versions` - List all available versioned artifacts
- `task show-version` - Show current version
- `task test-web` - Run Playwright tests
- `task test-full` - Build, start, test, and report (complete test workflow)

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
