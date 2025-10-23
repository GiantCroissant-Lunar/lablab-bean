---
doc_id: DOC-2025-00020
title: Project Organization
doc_type: reference
status: active
canonical: true
created: 2025-10-20
tags: [organization, structure, file-layout]
summary: >
  File organization and directory structure of the Lablab Bean project.
---

# Project Organization

This document describes the file organization and structure of the Lablab Bean project.

## Directory Structure

```
lablab-bean/
├── build/
│   ├── _artifacts/              # Build outputs (versioned)
│   │   └── <version>/
│   │       ├── publish/         # Built applications
│   │       ├── logs/            # Runtime logs
│   │       ├── test-results/    # Test artifacts
│   │       ├── test-reports/    # Test reports
│   │       └── version.json     # Build metadata
│   ├── nuke/                    # NUKE build system
│   │   ├── Build.cs
│   │   ├── Build.csproj
│   │   └── Components/
│   └── scripts/                 # Build and automation scripts
│       └── build-and-run.ps1
├── docs/                        # Documentation
│   ├── QUICKSTART.md           # Quick start guide
│   ├── RELEASE.md              # Release & deployment guide
│   ├── ORGANIZATION.md         # This file
│   ├── ARCHITECTURE.md
│   ├── CONTRIBUTING.md
│   └── ...
├── dotnet/                      # .NET solution
│   ├── console-app/
│   ├── windows-app/
│   └── framework/
├── website/                     # Node.js workspace
│   ├── apps/web/               # Astro web app
│   ├── packages/terminal/      # Terminal backend
│   ├── tests/                  # Playwright tests
│   ├── playwright.config.ts    # Playwright configuration
│   ├── ecosystem.config.js     # PM2 dev config
│   ├── ecosystem.production.config.js  # PM2 production config
│   └── package.json
├── git-hooks/                   # Git hooks
├── Taskfile.yml                # Task automation
├── GitVersion.yml              # Version configuration
└── README.md                   # Main documentation
```

## File Locations

### Scripts

- **Build Script**: `build/scripts/build-and-run.ps1`
- **NUKE Build**: `build/nuke/Build.cs`

### Documentation

- **Quick Start**: `docs/QUICKSTART.md`
- **Release Guide**: `docs/RELEASE.md`
- **Organization**: `docs/ORGANIZATION.md`
- **Architecture**: `docs/ARCHITECTURE.md`
- **Contributing**: `docs/CONTRIBUTING.md`

### Configuration

- **Task Automation**: `Taskfile.yml` (root)
- **PM2 Development**: `website/ecosystem.config.js`
- **PM2 Production**: `website/ecosystem.production.config.js`
- **Playwright**: `website/playwright.config.ts`
- **GitVersion**: `GitVersion.yml` (root)

### Tests

- **Playwright Tests**: `website/tests/`
- **Test Configuration**: `website/playwright.config.ts`

## Versioned Artifacts Structure

Each build creates a versioned directory with the following structure:

```
build/_artifacts/<version>/
├── publish/                    # Built applications
│   ├── console/               # Self-contained .NET console app
│   │   ├── LablabBean.Console.exe
│   │   └── ... (dependencies)
│   ├── windows/               # Self-contained .NET Windows app
│   │   ├── LablabBean.Windows.exe
│   │   └── ... (dependencies)
│   └── website/               # Built Astro website
│       ├── server/
│       ├── client/
│       ├── node_modules/
│       └── package.json
├── logs/                      # Runtime logs (PM2)
│   ├── web-out.log
│   ├── web-error.log
│   ├── console-out.log
│   └── console-error.log
├── test-results/              # Playwright test artifacts
│   ├── screenshots/
│   ├── videos/
│   └── traces/
├── test-reports/              # Test reports
│   ├── html/                  # HTML report
│   │   └── index.html
│   ├── results.json           # JSON report
│   └── junit.xml              # JUnit XML report
└── version.json               # Build metadata
```

## Key Files

### Root Level

- **`Taskfile.yml`** - Task automation (primary interface)
- **`README.md`** - Main project documentation
- **`GitVersion.yml`** - Semantic versioning configuration
- **`.gitignore`** - Git ignore patterns

### Build System

- **`build/nuke/Build.cs`** - NUKE build configuration
- **`build/scripts/build-and-run.ps1`** - Quick build & run script

### Website

- **`website/package.json`** - Workspace package configuration
- **`website/playwright.config.ts`** - Test configuration
- **`website/ecosystem.production.config.js`** - Production PM2 config
- **`website/tests/web-terminal.spec.ts`** - Web terminal tests

### Documentation

- **`docs/QUICKSTART.md`** - Get started quickly
- **`docs/RELEASE.md`** - Complete release guide
- **`docs/ORGANIZATION.md`** - This file

## Configuration Files

### PM2 Configuration

**Development** (`website/ecosystem.config.js`):

- Runs from source
- Hot reload enabled
- Development dependencies

**Production** (`website/ecosystem.production.config.js`):

- Runs from versioned artifacts
- Production dependencies only
- Logs to versioned directory

### Playwright Configuration

Located at `website/playwright.config.ts`:

- Test results go to versioned artifacts
- Multiple reporters (HTML, JSON, JUnit)
- Supports multiple browsers
- Can start/stop web server automatically

## Workflow

### Development Workflow

1. Edit code in `dotnet/` or `website/`
2. Run `task dotnet-run-console` or `pnpm dev` in website
3. Test changes locally

### Release Workflow

1. Run `task build-release`
2. Artifacts created in `build/_artifacts/<version>/`
3. Run `task stack-run` to start from artifacts
4. Run `task test-web` to test
5. Check `build/_artifacts/<version>/test-reports/` for results

### Testing Workflow

1. Ensure stack is running: `task stack-status`
2. Run tests: `task test-web`
3. View reports: `task test-report`
4. Debug if needed: `task test-web-debug`

## Migration Notes

### Old Locations → New Locations

- `build-and-run.ps1` → `build/scripts/build-and-run.ps1`
- `QUICKSTART.md` → `docs/QUICKSTART.md` (updated)
- `RELEASE.md` → `docs/RELEASE.md`
- `ecosystem.config.js` → `website/ecosystem.production.config.js`
- `logs/` → `build/_artifacts/<version>/logs/`

### Breaking Changes

- PM2 production config moved to `website/ecosystem.production.config.js`
- All logs now go to versioned artifacts directory
- Test results now go to versioned artifacts directory
- Build script moved to `build/scripts/`

### Updated References

All references have been updated in:

- `README.md`
- `Taskfile.yml`
- `docs/QUICKSTART.md`
- `docs/RELEASE.md`
- `build/scripts/build-and-run.ps1`

## Best Practices

### Adding New Scripts

Place in `build/scripts/` and reference from `Taskfile.yml`

### Adding New Documentation

Place in `docs/` with descriptive name

### Adding New Tests

Place in `website/tests/` with `.spec.ts` extension

### Versioned Artifacts

All build outputs, logs, and test results should go to versioned artifacts directory

## Quick Reference

### Run from Source (Development)

```bash
cd website
pnpm dev
```

### Run from Artifacts (Production)

```bash
task build-release
task stack-run
```

### Run Tests

```bash
task test-web
```

### View Logs

```bash
task stack-logs
```

### View Test Reports

```bash
task test-report
```

## See Also

- [QUICKSTART.md](QUICKSTART.md) - Quick start guide
- [RELEASE.md](RELEASE.md) - Release & deployment guide
- [README.md](../README.md) - Main documentation
- [Taskfile.yml](../Taskfile.yml) - All available tasks
