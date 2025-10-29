# Lablab Bean

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Node.js](https://img.shields.io/badge/Node.js-18+-339933?style=flat-square&logo=node.js&logoColor=white)](https://nodejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0+-3178C6?style=flat-square&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Astro](https://img.shields.io/badge/Astro-4.0+-FF5D01?style=flat-square&logo=astro&logoColor=white)](https://astro.build/)
[![Task](https://img.shields.io/badge/Task-Automation-29BEB0?style=flat-square&logo=task&logoColor=white)](https://taskfile.dev/)
[![Pre-commit](https://img.shields.io/badge/Pre--commit-Enabled-FAB040?style=flat-square&logo=pre-commit&logoColor=black)](https://pre-commit.com/)
[![License](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey?style=flat-square)](https://github.com/your-username/lablab-bean)

> A multi-platform development toolkit featuring task automation, web terminal, and cross-platform console applications with event-driven plugin architecture.

## ✨ Key Features

### 🏗️ Core Platform

- **Task Automation**: Powered by [Task](https://taskfile.dev) with comprehensive workflow management
- **Event-Driven Architecture**: High-performance plugin system (1.1M+ events/sec)
- **Cross-Platform**: Windows, Linux, macOS support
- **Modern .NET**: .NET 8 with Microsoft.Extensions ecosystem

### 🌐 Applications

- **Web Terminal**: Astro.js + xterm.js + node-pty for browser-based terminal
- **Console App**: Terminal.Gui v2 TUI application
- **Windows App**: SadConsole ASCII graphics application
- **Media Player**: Terminal-based video/audio player with FFmpeg + Braille rendering

### 🎮 Gameplay Systems

- **Complete RPG Suite**: Quest, NPC, Progression, Spell, Merchant, Boss, and Hazards systems
- **Arch ECS**: High-performance entity component system (5M+ entities/sec)
- **AI Integration**: Semantic Kernel for dynamic content generation

### 🛠️ Developer Tools

- **Spec-Kit**: Template-based code generation and specifications
- **Pre-commit Hooks**: Automated code quality checks
- **Reactive Programming**: ReactiveUI, System.Reactive, R3

## Project Structure

```
lablab-bean/
├── docs/                    # Documentation
│   ├── plugins/            # Plugin development guides
│   └── specs/              # Feature specifications
├── templates/              # Code generation templates
│   ├── entity/            # Entity templates (monsters, items)
│   └── docs/              # Documentation templates
├── git-hooks/              # Custom Git hooks
├── plugins/                # Plugin implementations
│   ├── examples/          # Example plugins
│   └── */                 # Plugin projects
├── website/                # Node.js workspace (pnpm)
│   ├── apps/web/          # Astro.js web app with terminal
│   └── packages/terminal/ # Terminal backend (node-pty)
└── dotnet/                # .NET 8 solution
    ├── framework/         # Shared libraries & contracts
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

## 🚀 Quick Start

### Prerequisites

- [Task](https://taskfile.dev) - `winget install Task.Task` or `brew install go-task/tap/go-task`
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) and [pnpm 8+](https://pnpm.io/)

### One-Command Setup

```bash
# Initialize project and install dependencies
task init

# Start development stack with hot reload
task dev-stack
```

Visit <http://localhost:3000> for the web terminal interface.

### Development Workflow

```bash
task dev-stack          # Start dev stack with hot reload
task dev-status         # Check status
task dev-logs           # View logs
task dev-stop           # Stop the stack
```

### Production Deployment

```bash
task release-and-run    # Build and start production stack
```

**Available Applications:**

- 🌐 **Web Terminal** - <http://localhost:3000>
- 💻 **Console App** - Terminal.Gui TUI
- 🎮 **Windows App** - SadConsole GUI

See [docs/guides/development.md](docs/guides/development.md) for detailed setup instructions.

### Manual Component Development

```bash
# Task automation
task install && task pre-commit-install && task check

# Website (Web Terminal)
cd website && pnpm install && pnpm dev

# .NET Console App
cd dotnet/console-app/LablabBean.Console && dotnet run

# .NET Windows App
cd dotnet/windows-app/LablabBean.Windows && dotnet run
```

## 🔌 Plugin Architecture

High-performance event-driven plugin system with **1.1M+ events/sec** throughput and **0.003ms** latency.

### Quick Plugin Creation

```bash
cd plugins/
dotnet new classlib -n YourPlugin -f net8.0
# Implement IPlugin interface - see docs/plugins/event-driven-development.md
```

### Key Features

- **Event Bus**: Pub-sub messaging with `IEventBus`
- **Loose Coupling**: Plugins communicate via events, not direct references
- **Service Contracts**: Platform-independent interfaces
- **Priority-Based Selection**: Multiple implementations with priority

**Documentation**: [Event-Driven Development Guide](docs/plugins/event-driven-development.md) | [Architecture Spec](specs/007-tiered-contract-architecture/spec.md)

## 🎮 Gameplay Systems

Complete RPG suite built on **Arch ECS** (5M+ entities/sec) with AI-powered content generation.

### Available Systems

| System | Features | Documentation |
|--------|----------|---------------|
| **Quest** | AI-generated quests, objectives, rewards, chains | [README](dotnet/plugins/LablabBean.Plugins.Quest/README.md) |
| **NPC** | 10 unique NPCs, dialogue trees, reputation system | [README](dotnet/plugins/LablabBean.Plugins.NPC/README.md) |
| **Progression** | Level 1-20, stat growth, class specializations | [README](dotnet/plugins/LablabBean.Plugins.Progression/README.md) |
| **Spells** | 15 spells across Fire/Ice/Lightning schools | [README](dotnet/plugins/LablabBean.Plugins.Spells/README.md) |
| **Merchant** | 3 merchant types, 50+ items, dynamic pricing | [README](dotnet/plugins/LablabBean.Plugins.Merchant/README.md) |
| **Boss** | 5 unique bosses, multi-phase encounters | [README](dotnet/plugins/LablabBean.Plugins.Boss/README.md) |
| **Hazards** | Environmental traps and obstacles | [README](dotnet/plugins/LablabBean.Plugins.Hazards/README.md) |

### Integration Example

```csharp
questService.StartQuest(playerEntity, "fetch_herbs");
spellService.CastSpell(playerEntityId, spellId, targetEntityId);
merchantService.BuyItem(playerEntity, merchantEntity, itemId, quantity);
```

## 📋 Available Tasks

```bash
task --list  # View all available tasks
```

### Essential Commands

| Command | Description |
|---------|-------------|
| `task init` | Initialize project and install dependencies |
| `task dev-stack` | Start development stack with hot reload |
| `task release-and-run` | Build and start production stack |
| `task check` | Run all validation checks |
| `task format` | Format all files |

### Development Stack

- `task dev-stack/stop/restart/status/logs` - Development stack management
- `task stack-run/stop/restart/status/logs` - Production stack management

### Code Quality

- `task pre-commit-install/run/update` - Pre-commit hook management
- `task dotnet:format/analyze/fix/check` - .NET code quality
- `task validate-yaml/markdown` - File validation

### Spec-Kit

- `task speck-init` - Initialize spec-kit directories
- `task speck-generate` - Generate from templates

## 🪝 Code Quality

Automated pre-commit hooks ensure code quality across all languages:

**Configured Hooks**: General checks, Markdown/YAML formatting, Python (Ruff/mypy), .NET (format/Roslynator), Shell (shellcheck/shfmt), Web (Prettier), Security (Gitleaks)

```bash
task pre-commit-install  # Install hooks
task pre-commit-run      # Run manually
```

## 📝 Spec-Kit Integration

Template-based code generation and standardized specifications.

### Quick Start

```bash
task speck-init                                    # Initialize spec-kit
code docs/guides/spec-kit-quickstart.md           # 5-minute guide
```

### Available Templates

- `templates/entity/monster.tmpl` - Monster classes with stats, AI, behavior
- `templates/docs/spec-template.tmpl` - Feature specifications

### Available Specifications

- `docs/specs/dungeon-generation-system.md` - Dungeon generation (implemented)
- `docs/specs/monster-template-example.md` - Template usage guide

**Documentation**: [Quick Start](docs/guides/spec-kit-quickstart.md) | [Complete Guide](docs/guides/spec-kit-utilization.md)

## 🔄 Development Workflow

```bash
# 1. Make changes to your files
# 2. Format and validate
task format && task check

# 3. Commit (pre-commit hooks run automatically)
git commit -m "Your changes"

# 4. Generate from templates (optional)
task speck-generate TEMPLATE=model OUTPUT=my-model.md
```

## 🏗️ Architecture

### Applications

| Component | Tech Stack | Features |
|-----------|------------|----------|
| **Web Terminal** | Astro.js, React, xterm.js, node-pty | Browser-based terminal, WebSocket communication |
| **Console App** | Terminal.Gui v2, ReactiveUI | Cross-platform TUI, keyboard shortcuts |
| **Windows App** | SadConsole, MonoGame | Retro ASCII graphics, roguelike UI |

### Framework Libraries

- **LablabBean.Core** - Domain models, interfaces, business logic
- **LablabBean.Infrastructure** - DI, logging (Serilog), configuration
- **LablabBean.Reactive** - ReactiveUI ViewModels, System.Reactive, R3

**Documentation**: [Website README](website/README.md) | [.NET README](dotnet/README.md)

## ⚙️ Configuration

Project configuration in `.lablab-bean.yaml`:

```yaml
project:
  name: lablab-bean
  version: 0.1.0

speck:
  enabled: true
  templates_dir: ./templates
  output_dir: ./generated

pre_commit:
  enabled: true
  auto_install: true
```

## 📚 Documentation

Structured documentation system with YAML front-matter and automatic validation.

### Quick Links

- **Getting Started**: [Quick Start](docs/QUICKSTART.md) | [Developer Setup](docs/QUICKSTART-DEV.md)
- **Development**: [Development Guide](docs/guides/development.md) | [Testing](docs/guides/testing.md) | [Debugging](docs/guides/debugging.md)
- **Architecture**: [System Architecture](docs/ARCHITECTURE.md) | [Project Organization](docs/ORGANIZATION.md)
- **Spec-Kit**: [Quick Start](docs/guides/spec-kit-quickstart.md) | [Complete Guide](docs/guides/spec-kit-utilization.md)

### Structure

```
docs/
├── guides/     # How-to guides and tutorials
├── specs/      # Feature specifications
├── findings/   # Research and analysis
└── archive/    # Historical documentation
```

**Validation**: `python scripts/validate_docs.py` | **Navigation**: [docs/README.md](docs/README.md)

## 🤖 AI Agent Support

Structured instructions for AI coding assistants in `.agent/` directory.

**Available**: [Claude](CLAUDE.md) → [.agent/adapters/claude.md](.agent/adapters/claude.md)
**Coming Soon**: GitHub Copilot, Windsurf

**Documentation**: [.agent/README.md](.agent/README.md)

## 🤝 Contributing

```bash
task pre-commit-install              # Install hooks
# Make your changes
task check                          # Run validation
python scripts/validate_docs.py     # Validate docs
git commit -m "Your changes"        # Commit (hooks run automatically)
```

**AI Assistants**: Read [CLAUDE.md](CLAUDE.md) for AI coding guidelines
**Guidelines**: [docs/CONTRIBUTING.md](docs/CONTRIBUTING.md)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 💬 Support

- **Documentation**: [docs/README.md](docs/README.md)
- **Issues**: [GitHub Issues](https://github.com/your-username/lablab-bean/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-username/lablab-bean/discussions)
