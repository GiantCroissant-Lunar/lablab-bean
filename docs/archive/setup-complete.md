# Setup Complete - Lablab Bean

## What Was Added

### 1. NUKE Build System ✅

**Location:** `build/nuke/`

**Features:**
- Reusable build components following NUKE best practices
- Components: `IClean`, `IRestore`, `ICompile`, `ITest`, `IPublish`
- Main build script with all targets

**Usage:**
```bash
# Via Task
task nuke-build          # Build solution
task nuke-clean          # Clean solution
task nuke-test           # Run tests
task nuke-publish        # Publish all apps
task nuke-publish-console # Publish console app only
task nuke-publish-windows # Publish windows app only

# Direct
cd build/nuke
dotnet run -- Compile
dotnet run -- Publish
```

**Components:**
- `IClean.cs` - Clean build artifacts
- `IRestore.cs` - Restore NuGet packages
- `ICompile.cs` - Compile solution
- `ITest.cs` - Run tests
- `IPublish.cs` - Publish applications

### 2. PM2 Process Manager ✅

**Location:** `website/ecosystem.config.js`

**Features:**
- Manages entire stack (web + console TUI)
- Automatic restart on failure
- Log aggregation
- Process monitoring

**Configuration:**
```javascript
{
  apps: [
    { name: 'lablab-web', script: 'pnpm dev' },
    { name: 'lablab-console-tui', script: 'dotnet run' }
  ]
}
```

**Usage:**
```bash
# Via Task
task stack-start    # Start all services
task stack-stop     # Stop all services
task stack-status   # Show status
task stack-logs     # View logs

# Via pnpm
cd website
pnpm stack:start
pnpm stack:stop
pnpm pm2:logs
pnpm pm2:monit
```

### 3. Enhanced Taskfile ✅

**New Task Categories:**

**NUKE Build:**
- `nuke-build` - Build with NUKE
- `nuke-clean` - Clean with NUKE
- `nuke-test` - Test with NUKE
- `nuke-publish` - Publish with NUKE
- `nuke-publish-console` - Publish console app
- `nuke-publish-windows` - Publish windows app

**.NET Tasks:**
- `dotnet-build` - Build solution
- `dotnet-clean` - Clean solution
- `dotnet-restore` - Restore packages
- `dotnet-format` - Format code
- `dotnet-run-console` - Run console app
- `dotnet-run-windows` - Run windows app

**Website/PM2:**
- `website-install` - Install dependencies
- `website-dev` - Start dev server
- `website-build` - Build website
- `stack-start` - Start entire stack
- `stack-stop` - Stop stack
- `stack-status` - Show status
- `stack-logs` - View logs

**JetBrains CLI:**
- `jb-inspect` - Code inspection (slow)
- `jb-cleanup` - Code cleanup

### 4. Enhanced Git Hooks ✅

**Location:** `git-hooks/`

**New Hooks:**
- `gitleaks-check` - Detect secrets/credentials
- `yaml-lint` - Lint YAML files
- `markdown-lint` - Lint Markdown files
- `dotnet-format-check` - Check .NET formatting

**Prerequisites:**
```bash
# Windows
winget install gitleaks

# Python tools
pip install yamllint

# Node tools
npm install -g markdownlint-cli

# .NET tools (included with SDK)
dotnet tool install -g dotnet-format
```

**Integration with pre-commit:**
```yaml
repos:
  - repo: local
    hooks:
      - id: gitleaks
        entry: ./git-hooks/gitleaks-check
      - id: yaml-lint
        entry: ./git-hooks/yaml-lint
      - id: markdown-lint
        entry: ./git-hooks/markdown-lint
      - id: dotnet-format
        entry: ./git-hooks/dotnet-format-check
```

### 5. Interactive TUI App ✅

**Location:** `dotnet/console-app/LablabBean.Console/`

**New Features:**
- **Three-panel layout:**
  - Left: Action list with emojis
  - Center: Output/content view
  - Right: Details panel
- **Interactive menu system**
- **Keyboard shortcuts** (Ctrl+N, Ctrl+O, F5, F6, etc.)
- **Mouse support** - Click actions
- **Real-time output** - Logs with timestamps
- **File operations** - New, Open, Save
- **Build integration** - Mock build output
- **Test runner** - Mock test execution

**New Files:**
- `Models/MenuAction.cs` - Menu action model
- `Services/IMenuService.cs` - Menu service interface
- `Services/MenuService.cs` - Menu service implementation
- `Views/InteractiveWindow.cs` - Main interactive window

**Features:**
```
┌─────────────────────────────────────────────────────────┐
│ File  Edit  View  Build  Help                          │
├──────────┬──────────────────────┬──────────────────────┤
│ Actions  │ Output               │ Details              │
│          │                      │                      │
│ 📄 New   │ [15:30:45] Building │ Build the project    │
│ 📂 Open  │ [15:30:46] Success  │                      │
│ 💾 Save  │                      │ Shortcut: F6         │
│ 🔨 Build │                      │                      │
│ 🧪 Tests │                      │ Compiles project     │
│ 📊 Logs  │                      │ and shows output.    │
│ 🔄 Refresh│                     │                      │
│ ℹ️  About│                      │                      │
│ ❌ Exit  │                      │                      │
└──────────┴──────────────────────┴──────────────────────┘
│ ^Q Quit  F5 Refresh  F6 Build  F1 Help                │
└─────────────────────────────────────────────────────────┘
```

**Keyboard Shortcuts:**
- `Ctrl+Q` - Quit
- `Ctrl+N` - New File
- `Ctrl+O` - Open File
- `Ctrl+S` - Save File
- `Ctrl+T` - Run Tests
- `Ctrl+L` - View Logs
- `F1` - Help
- `F5` - Refresh
- `F6` - Build

**Works with xterm.js:**
The TUI app runs in a PTY session and can be displayed in the browser via xterm.js. All interactions (keyboard, mouse) work through the terminal.

## Quick Start

### 1. Install Dependencies

```bash
# Install PM2
cd website
pnpm install

# Restore NUKE build
cd ../build/nuke
dotnet restore
```

### 2. Start the Stack

```bash
# Option 1: Via Task
task stack-start

# Option 2: Via pnpm
cd website
pnpm stack:start
```

This starts:
- Web server (http://localhost:3000)
- Console TUI app (in PTY)

### 3. Access in Browser

Open http://localhost:3000 to see the TUI app running in xterm.js!

### 4. Build with NUKE

```bash
task nuke-build
task nuke-publish
```

### 5. Run Individually

```bash
# Console TUI
task dotnet-run-console

# Windows App
task dotnet-run-windows

# Website only
task website-dev
```

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                      Browser                            │
│  ┌──────────────────────────────────────────────────┐   │
│  │  xterm.js Terminal Emulator                      │   │
│  │  (displays TUI, handles input)                   │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                         │ WebSocket
                         ▼
┌─────────────────────────────────────────────────────────┐
│              Node.js Server (Astro)                     │
│  ┌──────────────────────────────────────────────────┐   │
│  │  WebSocket Server + Terminal Manager             │   │
│  │  (node-pty)                                      │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                         │ PTY
                         ▼
┌─────────────────────────────────────────────────────────┐
│         .NET Console App (Terminal.Gui)                 │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Interactive TUI with 3-panel layout             │   │
│  │  • Actions (left)                                │   │
│  │  • Output (center)                               │   │
│  │  • Details (right)                               │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

## Development Workflow

### 1. Make Changes

```bash
# Edit code in dotnet/console-app/LablabBean.Console/
```

### 2. Format Code

```bash
task dotnet-format
```

### 3. Build

```bash
task nuke-build
```

### 4. Test

```bash
task nuke-test
```

### 5. Run

```bash
# Local testing
task dotnet-run-console

# Full stack
task stack-start
```

### 6. Commit

```bash
git add .
git commit -m "feat: add new feature"
# Pre-commit hooks run automatically
```

## PM2 Commands

```bash
# Start
pnpm stack:start

# Stop
pnpm stack:stop

# Restart
pnpm pm2:restart

# Logs
pnpm pm2:logs

# Monitor
pnpm pm2:monit

# Status
pnpm stack:status
```

## NUKE Build Targets

```bash
# Clean
dotnet run --project build/nuke/Build.csproj -- Clean

# Restore
dotnet run --project build/nuke/Build.csproj -- Restore

# Compile
dotnet run --project build/nuke/Build.csproj -- Compile

# Test
dotnet run --project build/nuke/Build.csproj -- Test

# Publish
dotnet run --project build/nuke/Build.csproj -- Publish

# Publish Console
dotnet run --project build/nuke/Build.csproj -- PublishConsole

# Publish Windows
dotnet run --project build/nuke/Build.csproj -- PublishWindows
```

## Git Hooks

### Run Manually

```bash
# Gitleaks
./git-hooks/gitleaks-check

# YAML Lint
./git-hooks/yaml-lint

# Markdown Lint
./git-hooks/markdown-lint

# .NET Format
./git-hooks/dotnet-format-check
```

### Via Task

```bash
# Format .NET code
task dotnet-format

# JetBrains inspection (slow)
task jb-inspect

# JetBrains cleanup
task jb-cleanup
```

## Troubleshooting

### PM2 Issues

```bash
# Kill all PM2 processes
pm2 kill

# Restart
pnpm stack:start
```

### NUKE Build Issues

```bash
# Clean and rebuild
task nuke-clean
task nuke-build
```

### Terminal.Gui Issues

```bash
# Check logs
cat dotnet/console-app/LablabBean.Console/logs/lablab-bean-*.log
```

## Next Steps

1. **Customize the TUI** - Add more actions and features
2. **Add Authentication** - Secure the web terminal
3. **Persist Sessions** - Save terminal sessions
4. **Add Themes** - Customize colors and appearance
5. **Plugin System** - Extend functionality

## Summary

✅ **NUKE Build** - Reusable components for .NET builds
✅ **PM2** - Process management for full stack
✅ **Enhanced Tasks** - Comprehensive task automation
✅ **Git Hooks** - gitleaks, yaml-lint, markdown-lint, dotnet-format
✅ **Interactive TUI** - 3-panel layout with keyboard shortcuts
✅ **xterm.js Integration** - TUI works in browser

The project is now a complete development toolkit with build automation, process management, code quality tools, and an interactive TUI that works both locally and in the browser! 🎉
