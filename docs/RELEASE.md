---
doc_id: DOC-2025-00023
title: Release & Deployment Guide
doc_type: guide
status: active
canonical: true
created: 2025-10-20
tags: [release, deployment, versioning, artifacts]
summary: >
  Build and run the Lablab Bean stack using versioned artifacts.
---

# Release & Deployment Guide

This guide explains how to build and run the Lablab Bean stack using versioned artifacts.

## Overview

The build system creates versioned artifacts in `build/_artifacts/<version>/publish/` with the following structure:

```
build/_artifacts/
└── <version>/
    └── publish/
        ├── console/          # Console app (self-contained .NET)
        ├── windows/          # Windows app (self-contained .NET)
        └── website/          # Web app (Astro + Node.js)
```

## Quick Start

### Build and Run

```bash
# Build the complete release with versioned artifacts
task build-release

# Start the stack from versioned artifacts
task stack-run

# Check status
task stack-status

# View logs
task stack-logs
```

### One Command Workflow

```bash
# Build and start in one command
task release-and-run
```

## Available Commands

### Build Commands

- **`task build-release`** - Build complete release with versioned artifacts
- **`task nuke-version`** - Print GitVersion information
- **`task list-versions`** - List all available versioned artifacts

### Stack Management

- **`task stack-run`** - Start stack from versioned artifacts (production mode)
- **`task stack-stop`** - Stop the stack
- **`task stack-restart`** - Restart the stack
- **`task stack-status`** - Show stack status
- **`task stack-delete`** - Remove stack from PM2

### Monitoring & Logs

- **`task stack-logs`** - Show all logs (live)
- **`task stack-logs-web`** - Show web app logs only
- **`task stack-logs-console`** - Show console app logs only
- **`task stack-monit`** - Open PM2 monitoring dashboard

### Version Management

- **`task show-version`** - Show the current version that will be used
- **`task list-versions`** - List all available versions

## Components

The stack consists of three main components:

### 1. Console App (`lablab-console`)

- **Type**: Self-contained .NET application
- **Location**: `build/_artifacts/<version>/publish/console/`
- **Executable**: `LablabBean.Console.exe`
- **Purpose**: Terminal UI / Console application

### 2. Windows App (`lablab-windows`)

- **Type**: Self-contained .NET WPF application
- **Location**: `build/_artifacts/<version>/publish/windows/`
- **Executable**: `LablabBean.Windows.exe`
- **Purpose**: Desktop GUI application

### 3. Web App (`lablab-web`)

- **Type**: Astro + Node.js application
- **Location**: `build/_artifacts/<version>/publish/website/`
- **Entry Point**: `server/entry.mjs`
- **Port**: 3000
- **Purpose**: Web interface with terminal support

## Version Management

### Using Specific Version

Set the `LABLAB_VERSION` environment variable:

```bash
# Windows PowerShell
$env:LABLAB_VERSION = "0.1.0-alpha.1"
task stack-run

# Windows CMD
set LABLAB_VERSION=0.1.0-alpha.1
task stack-run
```

### Auto-Detection

If `LABLAB_VERSION` is not set, the system automatically uses the latest version from `build/_artifacts/`.

## Logs

Logs are stored in the `logs/` directory:

- `web-out-<version>.log` - Web app stdout
- `web-error-<version>.log` - Web app stderr
- `console-out-<version>.log` - Console app stdout
- `console-error-<version>.log` - Console app stderr

## Build Process

The build process:

1. **Clean** - Removes old build artifacts
2. **Print Version** - Shows GitVersion information
3. **Compile** - Builds .NET solution
4. **Publish All** - Publishes console and windows apps as self-contained
5. **Build Website** - Builds Astro website and installs production dependencies
6. **Create Version File** - Generates `version.json` with build metadata

## Troubleshooting

### No artifacts found

```bash
# Build the release first
task build-release
```

### Check what version will be used

```bash
task show-version
```

### View all available versions

```bash
task list-versions
```

### Stack won't start

1. Check if artifacts exist:

   ```bash
   task list-versions
   ```

2. Check PM2 status:

   ```bash
   pm2 status
   ```

3. View error logs:

   ```bash
   task stack-logs
   ```

### Clean restart

```bash
# Stop and delete from PM2
task stack-delete

# Rebuild
task build-release

# Start fresh
task stack-run
```

## Development vs Production

### Development Mode

- Uses `task stack-start` (from `website/` directory)
- Runs from source with hot reload
- Uses development dependencies

### Production Mode

- Uses `task stack-run` (from root directory)
- Runs from versioned artifacts
- Uses production dependencies only
- Self-contained executables

## CI/CD Integration

The versioned artifact system is designed for CI/CD:

```yaml
# Example GitHub Actions
- name: Build Release
  run: task build-release

- name: Archive Artifacts
  uses: actions/upload-artifact@v3
  with:
    name: release-${{ github.sha }}
    path: build/_artifacts/
```

## Version Information

Each release includes a `version.json` file with:

```json
{
  "version": "0.1.0-alpha.1",
  "fullSemVer": "0.1.0-alpha.1+42",
  "branch": "main",
  "commit": "abc123...",
  "buildDate": "2025-10-20T08:50:00.000Z",
  "components": ["console", "windows", "website"]
}
```

## Next Steps

1. Build your first release: `task build-release`
2. Start the stack: `task stack-run`
3. Open web interface: <http://localhost:3000>
4. Monitor with: `task stack-status`
