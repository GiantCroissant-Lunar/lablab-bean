---
doc_id: DOC-2025-00027
title: File Organization Migration Guide
doc_type: guide
status: active
canonical: true
created: 2025-10-20
tags: [migration, file-organization, testing, playwright]
summary: >
  Guide for migrating to the new file organization and testing structure
  with Playwright and versioned artifacts.
---

# Migration Guide

Guide for migrating to the new file organization and testing structure.

## Overview

This guide helps you migrate from the old structure to the new organized structure with Playwright testing and versioned artifacts.

## Quick Migration Checklist

- [ ] Update script references to `build/scripts/`
- [ ] Update PM2 config references to `website/ecosystem.production.config.js`
- [ ] Update log paths to versioned directories
- [ ] Install Playwright browsers: `task test-install`
- [ ] Update CI/CD pipelines
- [ ] Update documentation links

## File Location Changes

### Scripts

| Old Location | New Location |
|--------------|--------------|
| `./build-and-run.ps1` | `./build/scripts/build-and-run.ps1` |

**Action Required:**

```bash
# Update any scripts that call:
.\build-and-run.ps1

# To:
.\build\scripts\build-and-run.ps1

# Or use task:
task release-and-run
```

### Documentation

| Old Location | New Location |
|--------------|--------------|
| `./QUICKSTART.md` | `./docs/QUICKSTART.md` |
| `./RELEASE.md` | `./docs/RELEASE.md` |
| N/A | `./docs/ORGANIZATION.md` (new) |
| N/A | `./docs/TESTING.md` (new) |

**Action Required:**

```bash
# Update documentation links from:
[RELEASE.md](RELEASE.md)

# To:
[docs/RELEASE.md](docs/RELEASE.md)
```

### Configuration

| Old Location | New Location |
|--------------|--------------|
| `./ecosystem.config.js` | `./website/ecosystem.production.config.js` |
| N/A | `./website/playwright.config.ts` (new) |

**Action Required:**

```bash
# Update PM2 commands from:
pm2 start ecosystem.config.js

# To:
cd website
pm2 start ecosystem.production.config.js

# Or use task:
task stack-run
```

### Logs

| Old Location | New Location |
|--------------|--------------|
| `./logs/` | `./build/_artifacts/<version>/logs/` |

**Action Required:**

- Update log monitoring scripts
- Use `task stack-logs` for live logs
- Check versioned directories for historical logs

## Command Changes

### Stack Management

**Old Commands:**

```bash
pm2 start ecosystem.config.js
pm2 stop ecosystem.config.js
pm2 restart ecosystem.config.js
pm2 delete ecosystem.config.js
```

**New Commands:**

```bash
task stack-run
task stack-stop
task stack-restart
task stack-delete
```

### Testing (New)

```bash
# Install browsers (first time)
task test-install

# Run tests
task test-web

# View reports
task test-report

# Complete workflow
task test-full
```

## Directory Structure Changes

### Old Structure

```
lablab-bean/
├── build/
│   ├── nuke/
│   └── _artifacts/
│       └── <version>/
│           └── publish/
├── logs/                    # Root level
├── build-and-run.ps1       # Root level
├── ecosystem.config.js     # Root level
├── QUICKSTART.md           # Root level
├── RELEASE.md              # Root level
└── ...
```

### New Structure

```
lablab-bean/
├── build/
│   ├── nuke/
│   ├── scripts/            # NEW: Scripts directory
│   │   └── build-and-run.ps1
│   └── _artifacts/
│       └── <version>/
│           ├── publish/
│           ├── logs/       # NEW: Versioned logs
│           ├── test-results/  # NEW: Test artifacts
│           └── test-reports/  # NEW: Test reports
├── docs/                   # NEW: Documentation directory
│   ├── QUICKSTART.md
│   ├── RELEASE.md
│   ├── ORGANIZATION.md     # NEW
│   ├── TESTING.md          # NEW
│   └── MIGRATION.md        # NEW (this file)
├── website/
│   ├── ecosystem.production.config.js  # MOVED
│   ├── playwright.config.ts            # NEW
│   └── tests/                          # NEW
│       └── web-terminal.spec.ts
└── ...
```

## Updating Scripts

### PowerShell Scripts

**Old:**

```powershell
# Call build script
.\build-and-run.ps1

# Start PM2
pm2 start ecosystem.config.js
```

**New:**

```powershell
# Call build script
.\build\scripts\build-and-run.ps1

# Or use task
task release-and-run

# Start PM2
task stack-run
```

### Bash Scripts

**Old:**

```bash
#!/bin/bash
./build-and-run.ps1
pm2 start ecosystem.config.js
```

**New:**

```bash
#!/bin/bash
./build/scripts/build-and-run.ps1
# Or
task release-and-run
task stack-run
```

## Updating CI/CD

### GitHub Actions

**Old:**

```yaml
- name: Build and run
  run: .\build-and-run.ps1

- name: Start stack
  run: pm2 start ecosystem.config.js
```

**New:**

```yaml
- name: Build release
  run: task build-release

- name: Start stack
  run: task stack-run

- name: Install Playwright
  run: task test-install

- name: Run tests
  run: task test-web

- name: Upload test reports
  uses: actions/upload-artifact@v3
  if: always()
  with:
    name: test-reports
    path: build/_artifacts/*/test-reports/
```

### GitLab CI

**Old:**

```yaml
script:
  - .\build-and-run.ps1
  - pm2 start ecosystem.config.js
```

**New:**

```yaml
script:
  - task build-release
  - task stack-run
  - task test-install
  - task test-web
artifacts:
  when: always
  paths:
    - build/_artifacts/*/test-reports/
  reports:
    junit: build/_artifacts/*/test-reports/junit.xml
```

## Updating Documentation

### Internal Links

**Old:**

```markdown
See [RELEASE.md](RELEASE.md) for details.
Run `.\build-and-run.ps1` to start.
```

**New:**

```markdown
See [docs/RELEASE.md](docs/RELEASE.md) for details.
Run `.\build\scripts\build-and-run.ps1` or `task release-and-run` to start.
```

### File References

Update any references to:

- `ecosystem.config.js` → `website/ecosystem.production.config.js`
- `logs/` → `build/_artifacts/<version>/logs/`
- Documentation files → `docs/`

## Updating Monitoring Scripts

### Log Monitoring

**Old:**

```powershell
Get-Content logs\web-out.log -Wait
```

**New:**

```powershell
# Use task command
task stack-logs-web

# Or direct path
$version = "0.1.0-alpha.1"
Get-Content "build\_artifacts\$version\logs\web-out.log" -Wait
```

### Log Rotation Scripts

**Old:**

```bash
# Rotate logs in logs/
find logs/ -name "*.log" -mtime +7 -delete
```

**New:**

```bash
# Logs are now versioned, no rotation needed
# Old versions can be archived or deleted as needed
```

## Testing Setup

### First Time Setup

```bash
# 1. Install Playwright browsers
task test-install

# 2. Verify installation
cd website
pnpm exec playwright --version
```

### Running Tests

```bash
# Basic test run
task test-web

# Interactive UI mode
task test-web-ui

# Headed mode (see browser)
task test-web-headed

# Debug mode
task test-web-debug
```

### Viewing Results

```bash
# Open HTML report
task test-report

# Check test results directory
ls build/_artifacts/*/test-results/

# Check test reports directory
ls build/_artifacts/*/test-reports/
```

## Common Issues

### Issue: PM2 can't find config

**Error:**

```
Error: ecosystem.config.js not found
```

**Solution:**

```bash
# Use task command
task stack-run

# Or change directory
cd website
pm2 start ecosystem.production.config.js
```

### Issue: Logs not found

**Error:**

```
logs/ directory not found
```

**Solution:**

```bash
# Logs are now versioned
# Use task command
task stack-logs

# Or check versioned directory
ls build/_artifacts/*/logs/
```

### Issue: Script not found

**Error:**

```
build-and-run.ps1 not found
```

**Solution:**

```bash
# Use new path
.\build\scripts\build-and-run.ps1

# Or use task
task release-and-run
```

### Issue: Playwright not installed

**Error:**

```
Executable doesn't exist at ...
```

**Solution:**

```bash
task test-install
```

## Rollback Plan

If you need to rollback temporarily:

1. **Keep old ecosystem.config.js:**

   ```bash
   # Copy from website/
   cp website/ecosystem.production.config.js ecosystem.config.js
   ```

2. **Use old commands:**

   ```bash
   pm2 start ecosystem.config.js
   ```

3. **Update later:**

   ```bash
   # When ready, switch back
   task stack-run
   ```

## Verification

After migration, verify everything works:

```bash
# 1. Build release
task build-release

# 2. Start stack
task stack-run

# 3. Check status
task stack-status

# 4. Run tests
task test-web

# 5. View logs
task stack-logs

# 6. View test report
task test-report

# 7. Stop stack
task stack-stop
```

## Getting Help

If you encounter issues:

1. Check [docs/ORGANIZATION.md](ORGANIZATION.md) for file locations
2. Check [docs/TESTING.md](TESTING.md) for testing help
3. Check [docs/RELEASE.md](RELEASE.md) for deployment help
4. Run `task --list` to see all available commands

## Timeline

**Immediate Actions:**

- [ ] Update script paths
- [ ] Update PM2 config references
- [ ] Install Playwright browsers

**Within 1 Week:**

- [ ] Update CI/CD pipelines
- [ ] Update monitoring scripts
- [ ] Update team documentation

**Within 1 Month:**

- [ ] Remove old log monitoring
- [ ] Archive old logs
- [ ] Train team on new structure

## Benefits After Migration

✅ **Better Organization** - Logical file structure
✅ **Versioned Outputs** - All artifacts tied to version
✅ **Comprehensive Testing** - E2E tests with Playwright
✅ **Better Debugging** - Multiple test modes
✅ **CI/CD Ready** - Structured artifacts
✅ **Historical Data** - Preserved in versioned directories

## Next Steps

1. Complete migration checklist
2. Run verification steps
3. Update team documentation
4. Train team on new commands
5. Update CI/CD pipelines

---

**Questions?** See [docs/ORGANIZATION.md](ORGANIZATION.md) or run `task --list`
