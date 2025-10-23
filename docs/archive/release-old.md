# Release & Deployment Guide

This guide explains how to build and run the Lablab Bean stack using versioned artifacts.

## Overview

The build system creates versioned artifacts in `build/_artifacts/<version>/` with the following structure:

```
build/_artifacts/
└── <version>/
    ├── publish/            # Built applications
    │   ├── console/       # Console app (self-contained .NET)
    │   ├── windows/       # Windows app (self-contained .NET)
    │   └── website/       # Web app (Astro + Node.js)
    ├── logs/              # Runtime logs (PM2)
    ├── test-results/      # Playwright test artifacts
    ├── test-reports/      # Test reports (HTML, JSON, JUnit)
    └── version.json       # Build metadata
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

### Quick Script (Windows)

```powershell
.\build\scripts\build-and-run.ps1
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

### Testing

- **`task test-install`** - Install Playwright browsers
- **`task test-web`** - Run Playwright tests
- **`task test-web-ui`** - Run tests in UI mode
- **`task test-web-headed`** - Run tests in headed mode
- **`task test-web-debug`** - Run tests in debug mode
- **`task test-report`** - Show test report
- **`task test-full`** - Build, start, test, and report (complete workflow)

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

## Testing

### Running Tests

The project includes Playwright end-to-end tests for the web terminal:

```bash
# Install Playwright browsers (first time only)
task test-install

# Run tests
task test-web

# Run tests with UI
task test-web-ui

# Run tests in headed mode (see browser)
task test-web-headed

# Debug tests
task test-web-debug
```

### Test Reports

Test results are stored in versioned artifacts:

- **Test Results**: `build/_artifacts/<version>/test-results/`
- **Test Reports**: `build/_artifacts/<version>/test-reports/`
  - HTML report: `test-reports/html/index.html`
  - JSON report: `test-reports/results.json`
  - JUnit XML: `test-reports/junit.xml`

View the HTML report:

```bash
task test-report
```

### Complete Test Workflow

```bash
# Build, start, test, and generate report
task test-full
```

This will:

1. Build the release
2. Start the stack
3. Wait for services to be ready
4. Run Playwright tests
5. Generate test reports
6. Show report location

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

Logs are stored in the versioned artifacts directory:

- `build/_artifacts/<version>/logs/web-out.log` - Web app stdout
- `build/_artifacts/<version>/logs/web-error.log` - Web app stderr
- `build/_artifacts/<version>/logs/console-out.log` - Console app stdout
- `build/_artifacts/<version>/logs/console-error.log` - Console app stderr

View logs:

```bash
# All logs (live)
task stack-logs

# Web app only
task stack-logs-web

# Console app only
task stack-logs-console
```

## Build Process

The build process:

1. **Clean** - Removes old build artifacts
2. **Print Version** - Shows GitVersion information
3. **Compile** - Builds .NET solution
4. **Publish All** - Publishes console and windows apps as self-contained
5. **Build Website** - Builds Astro website and installs production dependencies
6. **Create Directories** - Creates logs/, test-results/, test-reports/
7. **Create Version File** - Generates `version.json` with build metadata

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

### Tests failing

1. Ensure stack is running:

   ```bash
   task stack-status
   ```

2. Check web app is accessible:

   ```bash
   curl http://localhost:3000
   ```

3. Run tests in headed mode to see what's happening:

   ```bash
   task test-web-headed
   ```

4. Debug tests:

   ```bash
   task test-web-debug
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
- Uses `website/ecosystem.config.js`

### Production Mode

- Uses `task stack-run` (from root directory)
- Runs from versioned artifacts
- Uses production dependencies only
- Self-contained executables
- Uses `website/ecosystem.production.config.js`

## CI/CD Integration

The versioned artifact system is designed for CI/CD:

```yaml
# Example GitHub Actions
- name: Build Release
  run: task build-release

- name: Run Tests
  run: task test-web

- name: Archive Artifacts
  uses: actions/upload-artifact@v3
  with:
    name: release-${{ github.sha }}
    path: build/_artifacts/

- name: Upload Test Reports
  uses: actions/upload-artifact@v3
  if: always()
  with:
    name: test-reports
    path: build/_artifacts/*/test-reports/
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
  "components": ["console", "windows", "website"],
  "directories": {
    "publish": "publish/",
    "logs": "logs/",
    "testResults": "test-results/",
    "testReports": "test-reports/"
  }
}
```

## File Organization

- **Scripts**: `build/scripts/` - Build and automation scripts
- **Documentation**: `docs/` - All documentation files
- **Website Config**: `website/` - PM2 and Playwright configuration
- **Artifacts**: `build/_artifacts/<version>/` - All versioned outputs

## Next Steps

1. Build your first release: `task build-release`
2. Start the stack: `task stack-run`
3. Open web interface: <http://localhost:3000>
4. Run tests: `task test-web`
5. Monitor with: `task stack-status`
6. View test reports: `task test-report`
