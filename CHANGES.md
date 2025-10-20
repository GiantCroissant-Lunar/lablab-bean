# Recent Changes - File Organization & Testing

**Date**: 2025-10-20  
**Summary**: Reorganized project files and added Playwright testing with versioned artifact integration

## 🎯 Overview

This update reorganizes the project structure for better maintainability and adds comprehensive end-to-end testing with Playwright. All build outputs, logs, and test results are now organized in versioned artifact directories.

## 📁 File Organization Changes

### Scripts → `build/scripts/`

**Moved:**
- `build-and-run.ps1` → `build/scripts/build-and-run.ps1`

**Purpose**: Centralize all build and automation scripts

### Documentation → `docs/`

**Updated:**
- `docs/QUICKSTART.md` - Updated with new structure
- `docs/RELEASE.md` - Complete rewrite with testing

**New:**
- `docs/ORGANIZATION.md` - Project structure documentation
- `docs/TESTING.md` - Comprehensive testing guide

**Purpose**: All documentation in one place

### Website Configuration → `website/`

**Moved:**
- `ecosystem.config.js` → `website/ecosystem.production.config.js`

**New:**
- `website/playwright.config.ts` - Playwright configuration
- `website/tests/web-terminal.spec.ts` - E2E tests

**Purpose**: Keep website-related configs together

## 🏗️ Versioned Artifact Structure

### New Directory Layout

```
build/_artifacts/<version>/
├── publish/           # Built applications
│   ├── console/      # Self-contained .NET console app
│   ├── windows/      # Self-contained .NET Windows app
│   └── website/      # Built Astro website
├── logs/             # PM2 runtime logs (NEW)
│   ├── web-out.log
│   ├── web-error.log
│   ├── console-out.log
│   └── console-error.log
├── test-results/     # Playwright test artifacts (NEW)
│   ├── screenshots/
│   ├── videos/
│   └── traces/
├── test-reports/     # Test reports (NEW)
│   ├── html/
│   ├── results.json
│   └── junit.xml
└── version.json      # Build metadata (UPDATED)
```

### Benefits

- ✅ All outputs tied to specific version
- ✅ Easy to compare results across versions
- ✅ Clean separation of concerns
- ✅ CI/CD friendly
- ✅ Historical data preserved

## 🧪 Playwright Testing

### New Test Infrastructure

**Configuration:**
- `website/playwright.config.ts` - Versioned output configuration
- Multiple reporters: HTML, JSON, JUnit
- Multi-browser support: Chromium, Firefox, WebKit

**Tests:**
- `website/tests/web-terminal.spec.ts` - Comprehensive web terminal tests

**Coverage:**
- Terminal visibility and interaction
- WebSocket connections
- Command execution
- Terminal history
- Multiple sessions
- Responsive design

### New Test Commands

```bash
task test-install      # Install Playwright browsers
task test-web          # Run tests
task test-web-ui       # Interactive UI mode
task test-web-headed   # Headed mode (see browser)
task test-web-debug    # Debug mode
task test-report       # View HTML report
task test-full         # Complete workflow
```

## 🔧 Build System Updates

### `build/nuke/Build.cs`

**Added:**
- `LogsDirectory` - Path to versioned logs
- `TestResultsDirectory` - Path to test results
- `TestReportsDirectory` - Path to test reports

**Updated:**
- `Release` target creates directory structure
- Enhanced `version.json` with directory info
- Better logging of artifact locations

### `website/package.json`

**Added:**
- `@playwright/test` dependency
- Test scripts: `test`, `test:ui`, `test:headed`, `test:debug`, `test:report`

## 📋 Task Automation Updates

### New Tasks

**Testing:**
- `test-install` - Install Playwright browsers
- `test-web` - Run Playwright tests
- `test-web-ui` - Run tests in UI mode
- `test-web-headed` - Run tests in headed mode
- `test-web-debug` - Debug tests
- `test-report` - Show test report
- `test-full` - Complete test workflow

### Updated Tasks

**Stack Management:**
- `stack-run` - Now uses `website/ecosystem.production.config.js`
- `stack-stop` - Updated config reference
- `stack-restart` - Updated config reference
- `stack-delete` - Updated config reference

All stack tasks now run from `website/` directory.

## 📚 Documentation Updates

### `README.md`

**Updated:**
- Script path: `.\build\scripts\build-and-run.ps1`
- RELEASE.md link: `docs/RELEASE.md`
- Added test commands to task list

### `docs/RELEASE.md`

**Complete Rewrite:**
- Updated directory structure
- Added testing section
- Updated all file paths
- Added test workflow examples
- CI/CD integration examples

### `docs/QUICKSTART.md`

**Updated:**
- New prerequisites
- Updated quick start steps
- Current project structure

### New Documentation

**`docs/ORGANIZATION.md`:**
- Complete project structure
- File location reference
- Versioned artifacts structure
- Migration notes
- Best practices

**`docs/TESTING.md`:**
- Complete testing guide
- Test commands and workflows
- Writing tests
- Debugging techniques
- CI/CD integration
- Troubleshooting

## 🔄 Migration Guide

### For Developers

**Old Way:**
```bash
.\build-and-run.ps1
pm2 start ecosystem.config.js
```

**New Way:**
```bash
.\build\scripts\build-and-run.ps1
task stack-run  # Uses website/ecosystem.production.config.js
```

### For CI/CD

**Old Artifacts:**
```
build/_artifacts/<version>/publish/
logs/  # Root level
```

**New Artifacts:**
```
build/_artifacts/<version>/
├── publish/
├── logs/
├── test-results/
└── test-reports/
```

### File References

Update any scripts or documentation that reference:
- `build-and-run.ps1` → `build/scripts/build-and-run.ps1`
- `ecosystem.config.js` → `website/ecosystem.production.config.js`
- `RELEASE.md` → `docs/RELEASE.md`
- `logs/` → `build/_artifacts/<version>/logs/`

## ⚠️ Breaking Changes

### PM2 Configuration

**Old:** `ecosystem.config.js` (root)  
**New:** `website/ecosystem.production.config.js`

**Impact:** Update any PM2 commands or scripts

**Fix:**
```bash
# Old
pm2 start ecosystem.config.js

# New
cd website
pm2 start ecosystem.production.config.js
# or
task stack-run
```

### Log Locations

**Old:** `logs/` (root)  
**New:** `build/_artifacts/<version>/logs/`

**Impact:** Log monitoring scripts need updating

**Fix:** Use `task stack-logs` or check versioned directory

### Script Locations

**Old:** `.\build-and-run.ps1`  
**New:** `.\build\scripts\build-and-run.ps1`

**Impact:** Direct script invocations need updating

**Fix:** Update paths or use `task release-and-run`

## ✅ Verification Steps

After pulling these changes:

1. **Install Playwright browsers:**
   ```bash
   task test-install
   ```

2. **Build release:**
   ```bash
   task build-release
   ```

3. **Start stack:**
   ```bash
   task stack-run
   ```

4. **Run tests:**
   ```bash
   task test-web
   ```

5. **View reports:**
   ```bash
   task test-report
   ```

## 📊 Benefits

### Organization
- ✅ Logical file structure
- ✅ Clear separation of concerns
- ✅ Easy to navigate
- ✅ Scalable for growth

### Testing
- ✅ Comprehensive E2E tests
- ✅ Multiple test reporters
- ✅ Easy debugging
- ✅ CI/CD ready

### Versioning
- ✅ All outputs versioned
- ✅ Historical data preserved
- ✅ Easy comparison
- ✅ Audit trail

### Developer Experience
- ✅ Clear documentation
- ✅ Simple commands
- ✅ Quick workflows
- ✅ Better debugging

## 🚀 Quick Start (New Users)

```bash
# 1. Install Playwright
task test-install

# 2. Build and run
task release-and-run

# 3. Run tests
task test-web

# 4. View results
task test-report
```

## 📖 Documentation Index

- **[README.md](README.md)** - Main project documentation
- **[docs/QUICKSTART.md](docs/QUICKSTART.md)** - Quick start guide
- **[docs/RELEASE.md](docs/RELEASE.md)** - Release & deployment
- **[docs/ORGANIZATION.md](docs/ORGANIZATION.md)** - Project structure
- **[docs/TESTING.md](docs/TESTING.md)** - Testing guide
- **[Taskfile.yml](Taskfile.yml)** - All available tasks

## 🤝 Contributing

When adding new features:

1. **Scripts** → Place in `build/scripts/`
2. **Documentation** → Place in `docs/`
3. **Tests** → Place in `website/tests/`
4. **Configs** → Place in appropriate directory

See [docs/ORGANIZATION.md](docs/ORGANIZATION.md) for details.

## 📝 Changelog

### Added
- Playwright testing infrastructure
- Versioned test results and reports
- Versioned log directories
- `docs/ORGANIZATION.md`
- `docs/TESTING.md`
- `website/playwright.config.ts`
- `website/tests/web-terminal.spec.ts`
- `website/ecosystem.production.config.js`
- Multiple test commands in Taskfile

### Changed
- Moved `build-and-run.ps1` to `build/scripts/`
- Moved `RELEASE.md` to `docs/`
- Updated `docs/QUICKSTART.md`
- Updated `README.md` with new paths
- Updated all stack tasks to use new config
- Enhanced `Build.cs` with test directories
- Updated `version.json` structure

### Removed
- Root-level `logs/` directory (now versioned)
- Root-level `ecosystem.config.js` (moved to website/)
- Root-level `QUICKSTART.md` (now in docs/)
- Root-level `RELEASE.md` (now in docs/)

## 🎯 Next Steps

1. Review new documentation in `docs/`
2. Update any custom scripts with new paths
3. Run `task test-install` to set up Playwright
4. Try `task test-full` to test the complete workflow
5. Check versioned artifacts in `build/_artifacts/`

---

**Questions?** Check [docs/ORGANIZATION.md](docs/ORGANIZATION.md) or run `task --list`
