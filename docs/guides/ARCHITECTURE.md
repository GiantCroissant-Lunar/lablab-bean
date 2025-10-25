---
doc_id: DOC-2025-00018
title: Lablab Bean Architecture
doc_type: reference
status: active
canonical: true
created: 2025-10-20
tags: [architecture, overview, structure]
summary: >
  Multi-platform development toolkit architecture including task automation,
  web terminal, .NET console/windows apps, and shared framework.
---

# Lablab Bean Architecture

## Overview

Lablab Bean is a multi-platform development toolkit consisting of:

1. **Task Automation** - Taskfile + pre-commit hooks
2. **Web Terminal** - Astro.js + xterm.js + node-pty
3. **.NET Console App** - Terminal.Gui v2
4. **.NET Windows App** - SadConsole
5. **Shared Framework** - Reusable .NET libraries

## Project Structure

```
lablab-bean/
├── .pre-commit-config.yaml      # Pre-commit hooks configuration
├── Taskfile.yml                  # Task automation
├── .lablab-bean.yaml             # Project configuration
├── docs/                         # Documentation
│   ├── ARCHITECTURE.md           # This file
│   ├── CONTRIBUTING.md           # Contribution guidelines
│   ├── QUICKSTART.md             # Quick start guide
│   ├── QUICKSTART-DEV.md         # Developer quick start
│   ├── RELEASE.md                # Release documentation
│   └── guides/                   # How-to guides
├── git-hooks/                    # Custom Git hooks
│   ├── README.md
│   ├── pre-commit
│   ├── commit-msg
│   └── pre-push
├── website/                      # Node.js monorepo (pnpm)
│   ├── pnpm-workspace.yaml
│   ├── package.json
│   ├── apps/
│   │   └── web/                  # Astro.js web application
│   │       ├── src/
│   │       │   ├── pages/        # Astro pages
│   │       │   ├── layouts/      # Layouts
│   │       │   ├── components/   # React components
│   │       │   └── server.ts     # WebSocket server setup
│   │       ├── astro.config.mjs
│   │       └── package.json
│   └── packages/
│       └── terminal/             # Terminal backend package
│           ├── src/
│           │   ├── index.ts
│           │   ├── types.ts
│           │   ├── manager.ts    # PTY session management
│           │   └── server.ts     # WebSocket server
│           └── package.json
└── dotnet/                       # .NET 8 solution
    ├── LablabBean.sln
    ├── Directory.Build.props     # Common build properties
    ├── Directory.Packages.props  # Central package management
    ├── framework/                # Shared libraries
    │   ├── LablabBean.Core/
    │   │   ├── Models/
    │   │   └── Interfaces/
    │   ├── LablabBean.Infrastructure/
    │   │   └── Extensions/
    │   └── LablabBean.Reactive/
    │       ├── ViewModels/
    │       └── Extensions/
    ├── console-app/              # Terminal.Gui application
    │   └── LablabBean.Console/
    │       ├── Services/
    │       ├── Views/
    │       ├── Program.cs
    │       └── appsettings.json
    └── windows-app/              # SadConsole application
        └── LablabBean.Windows/
            ├── UI/
            ├── Program.cs
            ├── RootScreen.cs
            └── appsettings.json
```

## Technology Stack

### Website (Node.js)

| Technology | Version | Purpose |
|-----------|---------|---------|
| Node.js | 18+ | Runtime |
| pnpm | 8+ | Package manager |
| Astro.js | 4.x | SSR framework |
| React | 18.x | UI components |
| xterm.js | 5.x | Terminal emulator |
| node-pty | 1.x | PTY bindings |
| ws | 8.x | WebSocket server |
| Tailwind CSS | 3.x | Styling |

### .NET Applications

| Technology | Version | Purpose |
|-----------|---------|---------|
| .NET | 8.0 | Runtime |
| Terminal.Gui | 2.0-pre | TUI framework |
| SadConsole | 10.x | ASCII graphics |
| ReactiveUI | 20.x | MVVM framework |
| System.Reactive | 6.x | Reactive extensions |
| R3 | 1.x | Reactive v3 |
| ObservableCollections | 3.x | Observable collections |
| MessagePipe | 1.x | Messaging |
| Serilog | 3.x | Logging |
| Microsoft.Extensions.* | 8.x | DI, Config, Hosting |

## Architecture Patterns

### Website Architecture

```
┌─────────────────────────────────────────┐
│           Browser (Client)              │
│  ┌───────────────────────────────────┐  │
│  │  Astro Page (SSR)                 │  │
│  │  ┌─────────────────────────────┐  │  │
│  │  │  Terminal Component (React) │  │  │
│  │  │  ┌───────────────────────┐   │  │  │
│  │  │  │  xterm.js             │   │  │  │
│  │  │  └───────────────────────┘   │  │  │
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
              │ WebSocket
              ▼
┌─────────────────────────────────────────┐
│         Node.js Server                  │
│  ┌───────────────────────────────────┐  │
│  │  Astro SSR                        │  │
│  │  ┌─────────────────────────────┐  │  │
│  │  │  WebSocket Server           │  │  │
│  │  │  ┌───────────────────────┐   │  │  │
│  │  │  │  Terminal Manager     │   │  │  │
│  │  │  │  ┌─────────────────┐  │   │  │  │
│  │  │  │  │  node-pty (PTY) │  │   │  │  │
│  │  │  │  └─────────────────┘  │   │  │  │
│  │  │  └───────────────────────┘   │  │  │
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
              │
              ▼
        Shell (PowerShell/Bash)
```

### .NET Architecture

```
┌─────────────────────────────────────────┐
│         Applications Layer              │
│  ┌──────────────┐  ┌─────────────────┐  │
│  │  Console App │  │  Windows App    │  │
│  │ (Terminal.Gui)│  │  (SadConsole)   │  │
│  └──────────────┘  └─────────────────┘  │
└─────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│         Framework Layer                 │
│  ┌──────────────────────────────────┐   │
│  │  LablabBean.Reactive             │   │
│  │  (ReactiveUI, Rx, MessagePipe)   │   │
│  └──────────────────────────────────┘   │
│  ┌──────────────────────────────────┐   │
│  │  LablabBean.Infrastructure       │   │
│  │  (DI, Logging, Configuration)    │   │
│  └──────────────────────────────────┘   │
│  ┌──────────────────────────────────┐   │
│  │  LablabBean.Core                 │   │
│  │  (Models, Interfaces)            │   │
│  └──────────────────────────────────┘   │
└─────────────────────────────────────────┘
```

## Design Patterns

### 1. Dependency Injection

All .NET applications use Microsoft.Extensions.DependencyInjection:

```csharp
services.AddLablabBeanInfrastructure(configuration);
services.AddLablabBeanReactive();
services.AddSingleton<IMyService, MyService>();
```

### 2. MVVM with ReactiveUI

ViewModels inherit from `ViewModelBase`:

```csharp
public class MyViewModel : ViewModelBase
{
    [Reactive]
    public string Title { get; set; }

    public ReactiveCommand<Unit, Unit> MyCommand { get; }
}
```

### 3. Reactive Programming

Using System.Reactive and R3:

```csharp
observable
    .Where(x => x > 0)
    .Select(x => x * 2)
    .Subscribe(x => Console.WriteLine(x));
```

### 4. Messaging with MessagePipe

```csharp
// Subscribe
var subscription = subscriber.Subscribe<MyMessage>(msg => {
    // Handle message
});

// Publish
publisher.Publish(new MyMessage());
```

### 5. Configuration

Using Microsoft.Extensions.Configuration:

```csharp
services.Configure<AppSettings>(configuration.GetSection("App"));
```

### 6. Logging

Using Serilog:

```csharp
Log.Information("Application started");
Log.Error(ex, "An error occurred");
```

## Communication Protocols

### WebSocket Protocol (Website)

**Client → Server:**

- Terminal input: Raw string data
- Resize: `{ type: 'resize', cols: number, rows: number }`

**Server → Client:**

- Terminal output: Raw string data (ANSI escape sequences)

### PTY Communication

```
Browser ←→ WebSocket ←→ Terminal Manager ←→ node-pty ←→ Shell
```

## Data Flow

### Website Terminal Flow

1. User types in browser terminal (xterm.js)
2. xterm.js sends data via WebSocket
3. WebSocket server receives data
4. Terminal Manager writes to PTY
5. PTY sends to shell (PowerShell/Bash)
6. Shell processes and outputs
7. PTY captures output
8. Terminal Manager sends via WebSocket
9. Browser receives and renders in xterm.js

### .NET Console App Flow

1. User interacts with Terminal.Gui UI
2. Events trigger ReactiveUI commands
3. Commands execute business logic
4. State changes propagate via Reactive extensions
5. UI updates automatically via bindings

## Security Considerations

### Website

- WebSocket connections should be authenticated
- PTY sessions should be isolated per user
- Input should be sanitized
- Rate limiting on PTY creation

### .NET Apps

- Configuration secrets should use Secret Manager
- Logging should not expose sensitive data
- File operations should validate paths

## Performance

### Website

- PTY sessions are created per WebSocket connection
- Sessions are cleaned up on disconnect
- Terminal output is streamed in real-time

### .NET Apps

- Reactive subscriptions are disposed properly
- ObservableCollections provide high performance
- MessagePipe offers zero-allocation messaging

## Extensibility

### Adding New Framework Library

1. Create project in `dotnet/framework/`
2. Add to solution
3. Reference from applications

### Adding New Application

1. Create project in appropriate directory
2. Add to solution
3. Reference framework libraries
4. Configure DI and services

### Adding New Website Package

1. Create package in `website/packages/`
2. Add to `pnpm-workspace.yaml`
3. Reference from apps

## Testing Strategy

(To be implemented)

- Unit tests for framework libraries
- Integration tests for applications
- E2E tests for website

## Deployment

### Website

- Build: `pnpm build`
- Deploy as Node.js application
- Requires WebSocket support

### .NET Console App

- Publish: `dotnet publish -c Release`
- Cross-platform (Windows, Linux, macOS)

### .NET Windows App

- Publish: `dotnet publish -c Release -r win-x64`
- Windows only (MonoGame requirement)

## Future Enhancements

- Authentication for web terminal
- Multi-user support
- Terminal session persistence
- Plugin system
- Theme customization
- Command history
- Tab completion
