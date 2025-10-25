# Lablab Bean .NET

.NET 8 solution with multiple applications and shared framework libraries.

## Structure

```
dotnet/
├── framework/                    # Shared libraries
│   ├── LablabBean.Core/         # Core domain models and interfaces
│   ├── LablabBean.Infrastructure/ # Infrastructure services (DI, Logging, Config)
│   ├── LablabBean.Reactive/     # Reactive programming utilities
│   ├── LablabBean.AI.Core/      # Intelligent avatar core abstractions
│   ├── LablabBean.AI.Actors/    # Akka.NET actor implementations
│   └── LablabBean.AI.Agents/    # Semantic Kernel AI agents
├── plugins/                      # Gameplay plugins (ECS-based)
│   ├── LablabBean.Plugins.Quest/      # Quest system with AI generation
│   ├── LablabBean.Plugins.NPC/        # NPC dialogue and reputation
│   ├── LablabBean.Plugins.Progression/ # Leveling and stat growth
│   ├── LablabBean.Plugins.Spells/     # Magic system (15 spells)
│   ├── LablabBean.Plugins.Merchant/   # Trading and shops
│   ├── LablabBean.Plugins.Boss/       # Boss encounters
│   └── LablabBean.Plugins.Hazards/    # Environmental hazards
├── console-app/                  # Terminal.Gui v2 console application
│   └── LablabBean.Console/
└── windows-app/                  # SadConsole Windows application
    └── LablabBean.Windows/
```

## Prerequisites

- .NET 8 SDK
- Windows (for SadConsole app)

## Framework Libraries

### LablabBean.Core

Core domain models, interfaces, and business logic.

**Features:**

- Domain models
- Service interfaces
- Command/Query interfaces

### LablabBean.Infrastructure

Infrastructure services and cross-cutting concerns.

**Features:**

- Dependency injection setup
- Logging with Serilog
- Configuration management
- Microsoft.Extensions.* integration

**Packages:**

- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Logging
- Serilog

### LablabBean.Reactive

Reactive programming utilities and ViewModels.

**Features:**

- ReactiveUI ViewModels
- System.Reactive extensions
- ObservableCollections support
- MessagePipe integration
- R3 (Reactive Extensions v3)

**Packages:**

- System.Reactive
- ReactiveUI
- ReactiveUI.Fody
- ObservableCollections (Cysharp)
- MessagePipe (Cysharp)
- R3 (Cysharp)

### LablabBean.AI.Core

Core abstractions for intelligent avatar system.

**Features:**

- ECS bridge components (AkkaActorRef, SemanticAgent, IntelligentAI)
- Avatar models (AvatarContext, AvatarState, AvatarMemory, AvatarRelationship)
- AI decision models
- Event definitions (AIThoughtEvent, AIBehaviorChangedEvent)

### LablabBean.AI.Actors

Akka.NET actor implementations for intelligent avatars.

**Features:**

- BossActor and EmployeeActor with Akka.Persistence
- Message-driven architecture
- EventBus bridge for ECS integration
- State serialization and snapshots

**Packages:**

- Akka 1.5.35
- Akka.Hosting
- Akka.Persistence.Sql

### LablabBean.AI.Agents

Semantic Kernel AI agents for decision-making.

**Features:**

- BossIntelligenceAgent with OpenAI integration
- EmployeeIntelligenceAgent for NPC behaviors
- YAML-based personality system
- Chat-based AI decision making

**Packages:**

- Microsoft.SemanticKernel 1.25.0
- Microsoft.SemanticKernel.Agents.Core
- Microsoft.SemanticKernel.Connectors.OpenAI

## Gameplay Plugins

The solution includes 7 gameplay plugins built on **Arch ECS** (Entity Component System), providing a complete dungeon crawler experience.

### 🎮 Plugin Overview

| Plugin | Description | Key Features |
|--------|-------------|--------------|
| **Quest** | Quest management | AI generation, objectives, rewards, quest chains |
| **NPC** | Interactive NPCs | Dialogue trees, reputation, 10 unique NPCs |
| **Progression** | Character growth | Leveling (1-20), stat growth, ability unlocks |
| **Spells** | Magic system | 15 spells across 3 schools (Fire, Ice, Lightning) |
| **Merchant** | Trading | 3 merchants, 50+ items, dynamic pricing |
| **Boss** | Epic encounters | 5 bosses, multi-phase battles, special mechanics |
| **Hazards** | Environmental dangers | Traps, damage-over-time, triggered effects |

### 📖 Plugin Documentation

Each plugin has comprehensive documentation:

- [Quest System README](plugins/LablabBean.Plugins.Quest/README.md)
- [NPC System README](plugins/LablabBean.Plugins.NPC/README.md)
- [Progression README](plugins/LablabBean.Plugins.Progression/README.md)
- [Spells README](plugins/LablabBean.Plugins.Spells/README.md)
- [Merchant README](plugins/LablabBean.Plugins.Merchant/README.md)
- [Boss README](plugins/LablabBean.Plugins.Boss/README.md)
- [Hazards README](plugins/LablabBean.Plugins.Hazards/README.md)

### 🏗️ Plugin Architecture

**Tech Stack:**

- **Arch ECS**: High-performance entity component system (5M+ entities/sec)
- **Event-Driven**: Loosely coupled via `IEventBus` (1.1M+ events/sec)
- **Service-Oriented**: Clean service interfaces for integration
- **Data-Driven**: JSON/YAML configuration for game content

**Integration Example:**

```csharp
// Quest System
questService.StartQuest(playerEntity, "fetch_herbs");
questService.CompleteQuest(playerEntity, "fetch_herbs");

// Spell System
spellService.CastSpell(playerEntityId, spellId, targetEntityId);

// Merchant System
merchantService.BuyItem(playerEntity, merchantEntity, itemId, quantity);

// NPC System
npcService.StartDialogue(playerEntity, npcEntity);
npcService.SelectChoice(playerEntity, "help_quest");
```

## Applications

### LablabBean.Console

Cross-platform terminal UI application using Terminal.Gui v2.

**Features:**

- Modern TUI with Terminal.Gui v2
- Menu bar and status bar
- File operations (New, Open, Save)
- Keyboard shortcuts
- Dependency injection
- Logging to file

**Run:**

```bash
cd console-app/LablabBean.Console
dotnet run
```

**Keyboard Shortcuts:**

- `Ctrl+Q` - Quit
- `Ctrl+N` - New file
- `Ctrl+O` - Open file
- `Ctrl+S` - Save file

### LablabBean.Windows

Windows application using SadConsole (roguelike/ASCII graphics).

**Features:**

- Retro ASCII graphics with SadConsole
- MonoGame backend
- Menu bar and status bar
- Reactive UI updates
- Dependency injection
- Logging to file

**Run:**

```bash
cd windows-app/LablabBean.Windows
dotnet run
```

**Keyboard Shortcuts:**

- `ESC` - Exit
- `F1` - Help

## Building

### Build entire solution

```bash
dotnet build
```

### Build specific project

```bash
dotnet build console-app/LablabBean.Console
dotnet build windows-app/LablabBean.Windows
```

### Build in Release mode

```bash
dotnet build -c Release
```

## Running

### Console app

```bash
dotnet run --project console-app/LablabBean.Console
```

### Windows app

```bash
dotnet run --project windows-app/LablabBean.Windows
```

## Publishing

### Console app (self-contained)

```bash
dotnet publish console-app/LablabBean.Console -c Release -r win-x64 --self-contained
dotnet publish console-app/LablabBean.Console -c Release -r linux-x64 --self-contained
dotnet publish console-app/LablabBean.Console -c Release -r osx-x64 --self-contained
```

### Windows app

```bash
dotnet publish windows-app/LablabBean.Windows -c Release -r win-x64 --self-contained
```

## Configuration

Both applications use `appsettings.json` for configuration:

```json
{
  "App": {
    "ApplicationName": "Lablab Bean",
    "Version": "0.1.0",
    "Logging": {
      "MinimumLevel": "Information",
      "EnableFile": true,
      "FilePath": "logs/lablab-bean-.log"
    }
  }
}
```

## Logging

Logs are written to the `logs/` directory with daily rolling:

- Console app: `logs/lablab-bean-.log`
- Windows app: `logs/lablab-bean-windows-.log`

## Dependencies

### Central Package Management

This solution uses Central Package Management (CPM) via `Directory.Packages.props`.

All package versions are managed centrally in `Directory.Packages.props`.

### Key Packages

- **Terminal.Gui** v2.0.0-pre.2 - Modern TUI framework
- **SadConsole** v10.0.3 - ASCII/roguelike game engine
- **ReactiveUI** v20.1.1 - MVVM framework with reactive extensions
- **System.Reactive** v6.0.0 - Reactive Extensions for .NET
- **ObservableCollections** v3.0.1 - High-performance observable collections
- **MessagePipe** v1.8.1 - High-performance messaging library
- **R3** v1.2.8 - Reactive Extensions v3
- **Serilog** v3.1.1 - Structured logging
- **Microsoft.Extensions.*** v8.0.0 - Configuration, DI, Hosting, Logging

## Architecture

### Dependency Flow

```
Applications (Console, Windows)
    ↓
Framework Libraries (Core, Infrastructure, Reactive)
    ↓
NuGet Packages
```

### Design Patterns

- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **MVVM**: ReactiveUI
- **Reactive Programming**: System.Reactive, R3
- **Messaging**: MessagePipe
- **Logging**: Serilog
- **Configuration**: Microsoft.Extensions.Configuration

## Development

### Adding a new framework library

1. Create project in `framework/` directory
2. Add to solution: `dotnet sln add framework/YourProject/YourProject.csproj`
3. Reference from applications as needed

### Adding a new application

1. Create project in appropriate directory (`console-app/` or `windows-app/`)
2. Add to solution: `dotnet sln add your-app/YourApp/YourApp.csproj`
3. Reference framework libraries as needed

## Testing

(To be added)

## License

(To be specified)
