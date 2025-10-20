# Development Guide

This guide covers the development workflow for Lablab Bean, including hot reload setup and best practices.

## Overview

Lablab Bean supports two distinct workflows:

1. **Development Mode** - Fast iteration with hot reload
2. **Production Mode** - Versioned artifacts for deployment

## Development Mode

### Architecture

Development mode uses PM2 to orchestrate multiple processes with hot reload:

```
┌─────────────────────────────────────────────────────────┐
│                    PM2 Process Manager                   │
│                    (Local, not global)                   │
├─────────────────────────────────────────────────────────┤
│                                                           │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────┐ │
│  │  Astro Dev      │  │  PTY Terminal   │  │ Console │ │
│  │  Server         │  │  Backend        │  │  App    │ │
│  │                 │  │                 │  │         │ │
│  │  Hot Reload ✨  │  │  TS Watch 🔄    │  │ .NET 🏃 │ │
│  │  Port: 3000     │  │  (auto-build)   │  │  Dev    │ │
│  └─────────────────┘  └─────────────────┘  └─────────┘ │
│                                                           │
└─────────────────────────────────────────────────────────┘
```

### Quick Start

```bash
# 1. Install dependencies (first time only)
cd website
pnpm install

# 2. Build .NET components (first time or after changes)
task build

# 3. Start development stack
task dev-stack

# The stack is now running:
# - Web: http://localhost:3000 (hot reload)
# - PTY: TypeScript watch mode
# - Console: .NET development mode
```

### Development Commands

| Command | Description |
|---------|-------------|
| `task dev-stack` | Start development stack with hot reload |
| `task dev-stop` | Stop development stack |
| `task dev-restart` | Restart development stack |
| `task dev-status` | Show stack status |
| `task dev-logs` | View live logs from all processes |
| `task dev-delete` | Remove all processes from PM2 |

### Hot Reload Behavior

#### Astro (Web App)
- **Auto-reload**: Yes ✅
- **Trigger**: File changes in `website/apps/web/src/`
- **Speed**: Instant (HMR)
- **No rebuild needed**: Changes reflect immediately

#### PTY Terminal Backend
- **Auto-rebuild**: Yes ✅
- **Trigger**: File changes in `website/packages/terminal/src/`
- **Speed**: Fast (TypeScript incremental compilation)
- **Restart needed**: Astro dev server picks up changes automatically

#### Console App (.NET)
- **Auto-rebuild**: No ❌
- **Manual rebuild**: `task build-dotnet`
- **Restart needed**: `task dev-restart` after rebuild

### Configuration Files

#### Development Ecosystem Config
**File**: `ecosystem.development.config.js`

```javascript
module.exports = {
  apps: [
    {
      name: 'lablab-web-dev',
      script: 'pnpm',
      args: 'dev',
      cwd: path.join(__dirname, 'website', 'apps', 'web'),
      // Astro handles hot reload
    },
    {
      name: 'lablab-pty-dev',
      script: 'pnpm',
      args: 'dev',
      cwd: path.join(__dirname, 'website', 'packages', 'terminal'),
      // TypeScript watch mode
    },
    {
      name: 'lablab-console-dev',
      script: 'dotnet',
      args: 'run',
      cwd: path.join(__dirname, 'dotnet', 'console-app', 'LablabBean.Console'),
      // .NET development mode
    }
  ]
};
```

### PM2 Usage

PM2 is installed **locally** in the workspace, not globally. All commands use `pnpm pm2`:

```bash
# View status
pnpm pm2 status

# View logs
pnpm pm2 logs

# Monitor dashboard
pnpm pm2 monit

# Stop all
pnpm pm2 stop all

# Delete all
pnpm pm2 delete all
```

### Workflow Examples

#### Typical Development Session

```bash
# Morning: Start the stack
task dev-stack

# Work on Astro components
# Edit website/apps/web/src/pages/index.astro
# Changes appear instantly in browser

# Work on terminal backend
# Edit website/packages/terminal/src/index.ts
# TypeScript recompiles automatically

# Lunch break: Check status
task dev-status

# Afternoon: Continue working
# No need to restart anything

# End of day: Stop the stack
task dev-stop
```

#### Working on .NET Console App

```bash
# Start dev stack
task dev-stack

# Make changes to .NET code
# Edit dotnet/console-app/LablabBean.Console/Program.cs

# Rebuild .NET components
task build-dotnet

# Restart only the console app
pnpm pm2 restart lablab-console-dev

# Or restart entire stack
task dev-restart
```

#### Debugging

```bash
# View logs from all processes
task dev-logs

# View logs from specific process
pnpm pm2 logs lablab-web-dev
pnpm pm2 logs lablab-pty-dev
pnpm pm2 logs lablab-console-dev

# Monitor in dashboard
task dev-status
pnpm pm2 monit
```

## Production Mode

### Architecture

Production mode uses versioned artifacts:

```
┌─────────────────────────────────────────────────────────┐
│                    PM2 Process Manager                   │
├─────────────────────────────────────────────────────────┤
│                                                           │
│  ┌─────────────────┐  ┌─────────────────────────────┐  │
│  │  Astro SSR      │  │  Console App                │  │
│  │  (Bundled)      │  │  (Published .exe)           │  │
│  │                 │  │                             │  │
│  │  Production 🚀  │  │  Production 🚀              │  │
│  │  Port: 3000     │  │  From artifacts             │  │
│  └─────────────────┘  └─────────────────────────────┘  │
│                                                           │
│  Artifacts: build/_artifacts/<version>/publish/          │
└─────────────────────────────────────────────────────────┘
```

### Build Process

```bash
# Build everything
task build-release

# This creates:
# build/_artifacts/<version>/publish/
#   ├── website/          # Bundled Astro app
#   └── console/          # Published .NET app
```

### Production Commands

| Command | Description |
|---------|-------------|
| `task build-release` | Build versioned artifacts |
| `task stack-run` | Start production stack |
| `task stack-stop` | Stop all PM2 processes |
| `task stack-restart` | Restart production stack |
| `task stack-status` | Show stack status |
| `task stack-logs` | View live logs |
| `task release-and-run` | Build and run (one command) |

### Configuration Files

#### Production Ecosystem Config
**File**: `website/ecosystem.production.config.js`

```javascript
module.exports = {
  apps: [
    {
      name: 'lablab-web',
      script: path.join(artifactsPath, 'website', 'server', 'entry.mjs'),
      cwd: path.join(artifactsPath, 'website'),
      env: {
        NODE_ENV: 'production',
        PORT: 3000
      }
    },
    {
      name: 'lablab-console',
      script: path.join(artifactsPath, 'console', 'LablabBean.Console.exe'),
      cwd: path.join(artifactsPath, 'console'),
      env: {
        DOTNET_ENVIRONMENT: 'Production'
      }
    }
  ]
};
```

## Comparison: Development vs Production

| Aspect | Development | Production |
|--------|-------------|------------|
| **Astro** | Dev server (hot reload) | Bundled SSR app |
| **PTY** | TypeScript watch mode | Compiled JavaScript |
| **Console** | `dotnet run` | Published `.exe` |
| **Speed** | Instant updates | Requires rebuild |
| **Artifacts** | Source files | Versioned builds |
| **PM2 Config** | `ecosystem.development.config.js` | `ecosystem.production.config.js` |
| **Use Case** | Active development | Testing/Deployment |

## Best Practices

### 1. Use Development Mode for Active Work

```bash
# ✅ Good: Fast iteration
task dev-stack
# Edit files, see changes instantly

# ❌ Avoid: Slow iteration
task build-release
task stack-run
# Must rebuild for every change
```

### 2. Build .NET Once, Then Iterate on Web

```bash
# Build .NET components once
task build-dotnet

# Start dev stack
task dev-stack

# Now work on Astro/React with hot reload
# No need to rebuild unless .NET changes
```

### 3. Use Production Mode for Testing

```bash
# Before committing, test production build
task build-release
task stack-run

# Run tests
task test-web

# Stop when done
task stack-stop
```

### 4. Clean PM2 State Regularly

```bash
# If processes get stuck
task dev-stop
task stack-stop
pnpm pm2 delete all

# Start fresh
task dev-stack
```

### 5. Monitor Logs During Development

```bash
# Keep logs open in separate terminal
task dev-logs

# Or use PM2 dashboard
pnpm pm2 monit
```

## Troubleshooting

### Port Already in Use

```bash
# Stop all PM2 processes
task dev-stop
task stack-stop

# Or kill specific port (Windows)
netstat -ano | findstr :3000
taskkill /PID <pid> /F
```

### Hot Reload Not Working

```bash
# Restart development stack
task dev-restart

# Or restart specific process
pnpm pm2 restart lablab-web-dev
```

### .NET Changes Not Reflected

```bash
# Rebuild .NET components
task build-dotnet

# Restart console app
pnpm pm2 restart lablab-console-dev
```

### PM2 State Corrupted

```bash
# Nuclear option: delete everything
pnpm pm2 delete all
pnpm pm2 kill

# Start fresh
task dev-stack
```

## IDE Integration

### VS Code

Recommended extensions:
- Astro
- ESLint
- Prettier
- C# Dev Kit

### JetBrains Rider

Use built-in .NET support and configure:
- File watcher for TypeScript
- Astro language support plugin

## Next Steps

- See [RELEASE.md](RELEASE.md) for production deployment
- See [TESTING.md](TESTING.md) for testing workflows
- See [ARCHITECTURE.md](ARCHITECTURE.md) for system design
