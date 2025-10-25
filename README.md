# Lablab Bean

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Node.js](https://img.shields.io/badge/Node.js-18+-339933?style=flat-square&logo=node.js&logoColor=white)](https://nodejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0+-3178C6?style=flat-square&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Astro](https://img.shields.io/badge/Astro-4.0+-FF5D01?style=flat-square&logo=astro&logoColor=white)](https://astro.build/)
[![Task](https://img.shields.io/badge/Task-Automation-29BEB0?style=flat-square&logo=task&logoColor=white)](https://taskfile.dev/)
[![Pre-commit](https://img.shields.io/badge/Pre--commit-Enabled-FAB040?style=flat-square&logo=pre-commit&logoColor=black)](https://pre-commit.com/)
[![License](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey?style=flat-square)](https://github.com/your-username/lablab-bean)

A multi-platform development toolkit featuring task automation, web terminal, and cross-platform console applications.

## Features

- 🔨 **Task Automation**: Powered by [Task](https://taskfile.dev)
- 📝 **Spec-Kit**: Template-based code generation and specifications
- 🪝 **Pre-commit Hooks**: Automated code quality checks
- 🌐 **Web Terminal**: Astro.js + xterm.js + node-pty
- 💻 **Console App**: Terminal.Gui v2 TUI application
- 🎮 **Windows App**: SadConsole ASCII graphics application
- ⚛️ **Reactive**: ReactiveUI, System.Reactive, R3
- 🏗️ **Modern .NET**: .NET 8 with Microsoft.Extensions.*
- 🔌 **Event-Driven Plugins**: Loosely coupled plugin architecture with pub-sub messaging (1.1M+ events/sec)

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

- 🌐 **Astro Dev Server** - <http://localhost:3000> (hot reload enabled)
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

- 🌐 **Web App** - <http://localhost:3000> (bundled Astro)
- 💻 **Console App** - Terminal.Gui TUI
- 🎮 **Windows App** - SadConsole GUI

See [RELEASE.md](docs/RELEASE.md) for complete release documentation.

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

## Plugin Development

The project features an **event-driven plugin architecture** with exceptional performance (1.1M+ events/sec, 0.003ms latency).

### Quick Start: Create Your First Plugin

```bash
cd plugins/
dotnet new classlib -n YourPlugin -f net8.0
```

Add references and implement `IPlugin` interface. See the [Event-Driven Development Guide](docs/plugins/event-driven-development.md) for a complete tutorial.

### Example Plugins

- **Analytics Plugin**: Track game events without direct dependencies
- **Mock Game Service**: Provide game mechanics with event publishing
- **Reactive UI**: Auto-update UI on game events (no polling)

See [plugins/examples/](plugins/examples/) for working examples.

### Key Features

- **Event Bus**: Pub-sub messaging with `IEventBus`
- **Service Contracts**: Platform-independent interfaces
- **Priority-Based Selection**: Multiple implementations with priority
- **Loose Coupling**: Plugins communicate via events, not direct references

### Documentation

- **Developer Guide**: [docs/plugins/event-driven-development.md](docs/plugins/event-driven-development.md)
- **Quickstart**: [specs/007-tiered-contract-architecture/quickstart.md](specs/007-tiered-contract-architecture/quickstart.md)
- **Performance**: [specs/007-tiered-contract-architecture/performance-results.md](specs/007-tiered-contract-architecture/performance-results.md)
- **Spec**: [specs/007-tiered-contract-architecture/spec.md](specs/007-tiered-contract-architecture/spec.md)

## Gameplay Plugins

The project includes a comprehensive suite of gameplay plugins built on the **Arch ECS** (Entity Component System) architecture, providing a complete dungeon crawler experience.

### 🎮 Available Gameplay Plugins

#### 1. Quest System (`LablabBean.Plugins.Quest`)

Complete quest management with objectives, rewards, and AI-generated content.

**Features:**

- Quest creation with multiple objective types (kill, collect, interact, explore)
- Quest chains and prerequisites
- Dynamic reward system (XP, gold, items)
- AI-powered quest generation using Semantic Kernel
- Quest state tracking (NotStarted, Active, Completed, Failed, Abandoned)

**Documentation:** [Quest Plugin README](dotnet/plugins/LablabBean.Plugins.Quest/README.md)

#### 2. NPC System (`LablabBean.Plugins.NPC`)

Interactive NPCs with dialogue trees and reputation systems.

**Features:**

- 10 unique NPCs (Guard, Merchant, Blacksmith, etc.)
- Branching dialogue trees with multiple response options
- Reputation tracking affects NPC interactions
- Disposition states (Friendly, Neutral, Hostile, Fearful)
- Quest giver integration

**Documentation:** [NPC Plugin README](dotnet/plugins/LablabBean.Plugins.NPC/README.md)

#### 3. Progression System (`LablabBean.Plugins.Progression`)

Character leveling, stat growth, and ability unlocks.

**Features:**

- Level 1-20 progression with exponential XP curve
- Stat growth (Health, Mana, Strength, Intelligence, Agility, Vitality)
- Ability unlocks every 3 levels
- Class specializations (Warrior, Mage, Rogue)
- Prestige system for endgame

**Documentation:** [Progression Plugin README](dotnet/plugins/LablabBean.Plugins.Progression/README.md)

#### 4. Spell System (`LablabBean.Plugins.Spells`)

Magic system with 15 spells across 3 schools of magic.

**Features:**

- **Fire Magic**: Fireball, Flame Strike, Meteor
- **Ice Magic**: Ice Shard, Frost Nova, Blizzard
- **Lightning Magic**: Lightning Bolt, Chain Lightning, Thunderstorm
- Mana management and regeneration
- Spell cooldowns and costs
- Area-of-effect and single-target spells

**Documentation:** [Spells Plugin README](dotnet/plugins/LablabBean.Plugins.Spells/README.md)

#### 5. Merchant System (`LablabBean.Plugins.Merchant`)

Trading system with dynamic pricing and 3 merchant types.

**Features:**

- 3 merchant types (General, Blacksmith, Apothecary)
- 50+ items across categories (weapons, armor, potions, materials)
- Dynamic pricing based on reputation and item rarity
- Buy/sell/trade mechanics
- Item quality tiers (Common, Uncommon, Rare, Epic, Legendary)

**Documentation:** [Merchant Plugin README](dotnet/plugins/LablabBean.Plugins.Merchant/README.md)

#### 6. Boss System (`LablabBean.Plugins.Boss`)

Epic boss encounters with multi-phase battles and special mechanics.

**Features:**

- 5 unique bosses with distinct mechanics
- Multi-phase encounters (2-3 phases per boss)
- Special abilities and attack patterns
- Enrage mechanics and phase transitions
- Unique loot tables per boss

**Documentation:** [Boss Plugin README](dotnet/plugins/LablabBean.Plugins.Boss/README.md)

#### 7. Hazards System (`LablabBean.Plugins.Hazards`)

Environmental hazards and dungeon obstacles.

**Features:**

- Spike traps, fire traps, poison gas, falling rocks
- Periodic and triggered hazard types
- Damage-over-time effects
- Visual indicators for hazards

**Documentation:** [Hazards Plugin README](dotnet/plugins/LablabBean.Plugins.Hazards/README.md)

### 🏗️ Architecture

All gameplay plugins are built on:

- **Arch ECS**: High-performance entity component system (5M+ entities/sec)
- **Event-Driven**: Loosely coupled via `IEventBus`
- **Service-Oriented**: Clean service interfaces for easy integration
- **Data-Driven**: JSON/YAML configuration for game content

### 📖 Plugin Development Guides

- **Event-Driven Development**: [docs/plugins/event-driven-development.md](docs/plugins/event-driven-development.md)
- **Plugin Architecture**: [specs/007-tiered-contract-architecture/spec.md](specs/007-tiered-contract-architecture/spec.md)
- **ECS Guide**: [Arch ECS Documentation](https://github.com/genaray/Arch)

### 🎯 Integration Example

```csharp
// Start a quest
questService.StartQuest(playerEntity, "fetch_herbs");

// Cast a spell
spellService.CastSpell(playerEntityId, spellId, targetEntityId);

// Trade with merchant
merchantService.BuyItem(playerEntity, merchantEntity, itemId, quantity);

// Talk to NPC
npcService.StartDialogue(playerEntity, npcEntity);
```

All plugins publish events through the `IEventBus` for loose coupling and reactive UIs.

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

Spec-kit provides template-based code generation and standardized specifications. Configuration is in `.lablab-bean.yaml`.

### Quick Start

**New to spec-kit?** Start here:

```bash
# Read the 5-minute quick start guide
code docs/guides/spec-kit-quickstart.md
```

### Initialize Spec-kit

```bash
task speck-init
```

### Create a Specification

```bash
# Copy example specification
copy docs\specs\dungeon-generation-system.md docs\specs\my-feature.md

# Edit your specification
code docs\specs\my-feature.md
```

### Generate Code from Template

```bash
# Copy monster template (manual for now)
copy templates\entity\monster.tmpl MyMonster.cs

# Replace {{.Variables}} with actual values
# See: docs/specs/monster-template-example.md (guide for using templates)
```

### Available Templates

Located in `templates/`:

- **entity/monster.tmpl**: Generate monster classes with stats, AI, and behavior
- **docs/spec-template.tmpl**: Generate feature specifications with standard format

### Available Specifications

Located in `docs/specs/`:

- **dungeon-generation-system.md**: Dungeon generation algorithm (implemented v0.0.2)
- **monster-template-example.md**: How to use monster templates
- **README.md**: Specifications directory guide

### Documentation

- **docs/guides/spec-kit-quickstart.md**: 5-minute quick start guide
- **docs/guides/spec-kit-utilization.md**: Complete strategy and implementation plan
- **docs/specs/**: Feature specifications and examples

### Configuration

Edit `.lablab-bean.yaml` to configure spec-kit templates:

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

## Documentation

This project uses a structured documentation system with organized categories:

### 📁 Documentation Structure

```
docs/
├── README.md                  # Documentation navigation
├── ARCHITECTURE.md            # System architecture
├── CONTRIBUTING.md            # Contribution guidelines
├── ORGANIZATION.md            # Project organization
├── DOCUMENTATION-SCHEMA.md    # Documentation standards
├── QUICK-REFERENCE.md         # Quick reference guide
│
├── guides/                    # How-to guides and tutorials
│   ├── development.md         # Development guide
│   ├── debugging.md           # Debugging guide
│   ├── testing.md             # Testing guide
│   ├── project-setup.md       # Project setup
│   ├── spec-kit-quickstart.md # Spec-kit quick start
│   ├── spec-kit-utilization.md # Spec-kit detailed guide
│   ├── agent-usage.md         # AI agent usage guide
│   ├── pm2-hot-reload-migration.md # PM2 migration
│   └── file-organization-migration.md # File organization migration
│
├── specs/                     # Feature specifications
│   ├── dungeon-generation-system.md
│   ├── dungeon-crawler-features.md
│   └── monster-template-example.md
│
├── findings/                  # Research and analysis
│   └── terminal-gui-pm2-fixes.md
│
├── archive/                   # Historical/superseded docs
│   ├── handover.md
│   ├── setup-complete.md
│   ├── file-organization-changes.md
│   └── ... (older versions)
│
└── index/
    └── registry.json          # Machine-readable doc registry
```

### 🎯 Quick Links

**Getting Started:**

- [Quick Start](docs/QUICKSTART.md) - User quick start
- [Developer Quick Start](docs/QUICKSTART-DEV.md) - Setup development environment
- [Project Setup Guide](docs/guides/project-setup.md) - Detailed setup instructions

**Development:**

- [Development Guide](docs/guides/development.md) - Development workflow
- [Testing Guide](docs/guides/testing.md) - Testing strategy
- [Debugging Guide](docs/guides/debugging.md) - Troubleshooting

**Spec-Kit:**

- [Spec-Kit Quick Start](docs/guides/spec-kit-quickstart.md) - 5-minute intro
- [Spec-Kit Utilization](docs/guides/spec-kit-utilization.md) - Complete guide
- [Feature Specifications](docs/specs/) - All specs

**Architecture:**

- [Architecture](docs/ARCHITECTURE.md) - System architecture
- [Organization](docs/ORGANIZATION.md) - Project structure
- [Contributing](docs/CONTRIBUTING.md) - How to contribute

### 📝 Documentation System Features

- **Schema**: All docs include YAML front-matter ([DOCUMENTATION-SCHEMA.md](docs/DOCUMENTATION-SCHEMA.md))
- **Registry**: Machine-readable doc registry at `docs/index/registry.json`
- **Validation**: Automatic validation with duplicate detection
- **Categories**: Organized into guides/, specs/, findings/, archive/

**Validate documentation:**

```bash
python scripts/validate_docs.py
```

See [docs/README.md](docs/README.md) for complete documentation navigation.

## AI Agent Instructions

This project includes structured instructions for AI coding assistants:

- **Claude Code**: [CLAUDE.md](CLAUDE.md) → [.agent/adapters/claude.md](.agent/adapters/claude.md)
- **GitHub Copilot**: `.github/copilot-instructions.md` (coming soon)
- **Windsurf**: `.windsurf/rules.md` (coming soon)

The `.agent/` directory contains:

- **Base Rules**: Core principles, normative rules, and glossary
- **Adapters**: Agent-specific configurations
- **Meta**: Versioning and governance

See [.agent/README.md](.agent/README.md) for details.

## Contributing

1. Install pre-commit hooks: `task pre-commit-install`
2. Read agent instructions if using AI assistants: [CLAUDE.md](CLAUDE.md)
3. Make your changes
4. Run checks: `task check`
5. Validate documentation: `python scripts/validate_docs.py`
6. Commit your changes (hooks will run automatically)
7. Submit a pull request

## License

[Add your license here]

## Support

[Add support information here]
