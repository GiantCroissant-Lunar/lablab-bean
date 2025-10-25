---
title: ReleaseConsole Artifact Verification Integration - Handover
date: 2025-10-24
author: GitHub Copilot
type: handover
status: completed
tags: [nuke, build, verification, plugins, release]
version: 0.0.4-144
---

# ReleaseConsole Artifact Verification Integration - Handover

## Purpose

Document the integration of artifact verification into the ReleaseConsole pipeline, re-enabling game plugins, and the current verification status of the console application.

## Session Date

2025-10-24 (20:00 - 21:00 UTC+8)

## What Was Completed

### 1. Artifact Verification Integration

**File**: `build/nuke/Build.cs:659`

Wired `VerifyArtifact` target into the `ReleaseConsole` dependency chain:

```csharp
Target ReleaseConsole => _ => _
    .DependsOn(PrintVersion, PublishConsole, VerifyArtifact, GenerateReports)
    .Executes(() => { ... });
```

**Pipeline Flow**:

1. `PrintVersion` - Display version and build info
2. `PublishConsole` - Build and publish console app + plugins
3. `VerifyArtifact` - Runtime verification of published plugins → generates health reports
4. `GenerateReports` - Generate build/session/plugin metrics
5. `ReleaseConsole` - Complete

### 2. Re-enabled Game Plugins

**File**: `build/nuke/Build.cs:462, 557`

**Before** (excluded 4 plugins):

```csharp
&& !n.Contains("/LablabBean.Plugins.Progression/", ...)
&& !n.Contains("/LablabBean.Plugins.Boss/", ...)
&& !n.Contains("/LablabBean.Plugins.Merchant/", ...)
&& !n.Contains("/LablabBean.Plugins.Quest/", ...)
```

**After** (only exclude Merchant - has build errors):

```csharp
&& !n.Contains("/LablabBean.Plugins.Merchant/", ...)
```

**Reason for exclusion**:

- **Merchant**: Has compilation error - missing `LablabBean.Plugins.NPC` namespace reference

### 3. Updated Verification Command

**File**: `build/nuke/Build.cs:684`

Removed `--exclude merchant` flag since Merchant is no longer published.

## Verification Results

### Build Metrics

- **Version**: 0.0.4-144
- **Build Date**: 2025-10-24
- **Configuration**: Debug
- **Target**: ReleaseConsole

### Plugin Statistics

#### Before Changes

- **Plugins with manifests**: 8
- **Published**: 8
- **Verified**: 8 running, 0 failed (100%)

#### After Changes

- **Total plugin projects**: 33
- **Plugins with manifests**: 11 (game-facing plugins)
- **Published**: 32 (Merchant excluded)
- **Verified**: 11 running, 0 failed (100%)
- **Infrastructure plugins**: 21 (no manifest needed)

#### Verified Plugins (11 total)

| Plugin | Version | Load Time | Memory | Status | Notes |
|--------|---------|-----------|--------|--------|-------|
| Boss System | 1.0.0 | 4ms | 24 KB | Running | ✨ NEW - Re-enabled |
| Diagnostic Firebase | 1.0.0 | 14ms | 24 KB | Running | Full initialization |
| Diagnostic OpenTelemetry | 1.0.0 | 6ms | 38 KB | Running | Full initialization |
| Diagnostic Sentry | 1.0.0 | 6ms | 16 KB | Running | Full initialization |
| Hazards System | 1.0.0 | 1ms | 24 KB | Running | Stub implementation |
| Inventory System | 1.0.0 | 6ms | 32 KB | Running | Full initialization |
| NPC System | 1.0.0 | 4ms | 24 KB | Running | Passive mode (no World service) |
| Progression System | 1.0.0 | 5ms | 32 KB | Running | ✨ NEW - Passive mode |
| Quest System | 1.0.0 | 5ms | 24 KB | Running | ✨ NEW - Passive mode |
| Spells System | 1.0.0 | 1ms | 24 KB | Running | Stub implementation |
| Status Effects System | 1.0.0 | 5ms | 32 KB | Running | Full initialization |

**Summary**:

- **Total Load Time**: 0.13s
- **Total Memory**: 0.29 MB
- **Average Load Time**: 5ms per plugin
- **Success Rate**: 100%

### Artifact Reports Generated

Location: `build/_artifacts/0.0.4-144/test-reports/`

**New Reports** (from VerifyArtifact):

- `plugin-health-artifact-0.0.4-144-0.0.4-144-20251024-124238.json`
- `plugin-health-artifact-0.0.4-144-0.0.4-144-20251024-124238.html`
- `plugin-health-artifact-0.0.4-144-0.0.4-144-20251024-124238.csv`

**Existing Reports** (from GenerateReports):

- `build-metrics-{Version}-{BuildNumber}-{Timestamp}.html/csv`
- `session-analytics-{Version}-{BuildNumber}-{Timestamp}.html/csv`
- `plugin-metrics-{Version}-{BuildNumber}-{Timestamp}.html/csv`
- `windows-session-{Version}-{BuildNumber}-{Timestamp}.html/csv`

## Console App Runtime Test

### What Was Verified ✅

1. **Plugin Loading**: All 11 plugins discovered and loaded successfully
2. **Clean Startup**: No crashes during initialization
3. **Graceful Shutdown**: Responded to 'Q' key, showed quit confirmation dialog, exited cleanly
4. **Plugin Unloading**: All plugins unloaded gracefully on shutdown

### What Was NOT Verified ❌

1. **TUI Rendering**: Visual confirmation that the game interface renders properly
2. **Interactive Gameplay**: Player movement, combat, inventory interaction
3. **Game Map Display**: Dungeon generation and display
4. **UI Responsiveness**: Full keyboard/mouse interaction testing

**Note**: Console output showed plugin loading logs and shutdown sequences, but **no screen recording or screenshot was captured** to prove the TUI renders correctly.

### Expected Warnings (Not Errors)

Some plugins run in "passive mode" when the World service is not available:

- **NPC Plugin**: "World service not found; NPC plugin will initialize in passive mode"
- **Progression Plugin**: "World service not found; Progression plugin will initialize in passive mode"
- **Quest Plugin**: "World service not found; initializing in passive mode"

These are **expected** in console verification mode - these plugins need a full game world to be fully functional.

## How to Run

### Build and Verify

```powershell
# Full release console with verification
dotnet run --project build/nuke/Build.csproj --target ReleaseConsole -configuration Release

# Verification only (requires existing publish)
dotnet run --project build/nuke/Build.csproj --target VerifyArtifact -configuration Debug
```

### Run from Artifact

```powershell
# Navigate to versioned artifact
cd build/_artifacts/0.0.4-144/publish/console

# Run console app (starts game TUI)
.\LablabBean.Console.exe

# List loaded plugins
.\LablabBean.Console.exe plugins list

# Verify plugins runtime
.\LablabBean.Console.exe plugins verify --paths plugins --output verify.json

# Generate plugin health report
.\LablabBean.Console.exe report plugin --output report.html --data verify.json --format html
```

## Known Issues

### 1. Merchant Plugin - Build Error

**Status**: Excluded from publish
**Error**: `error CS0234: The type or namespace name 'NPC' does not exist in the namespace 'LablabBean.Plugins'`
**File**: `dotnet/plugins/LablabBean.Plugins.Merchant/MerchantPlugin.cs:2`
**Fix Required**: Add proper project reference to `LablabBean.Plugins.NPC`

### 2. Passive Mode Plugins

**Status**: Working as designed
**Plugins Affected**: NPC, Progression, Quest, Spells
**Reason**: Console verification mode doesn't initialize full World service
**Impact**: Plugins load successfully but some features unavailable until World is provided

### 3. Plugin Manifest Coverage

**Status**: By design
**Details**: Only 11 of 33 plugins have `plugin.json` manifests

- **Game-facing plugins** (11): Have manifests, verified by VerifyArtifact
- **Infrastructure plugins** (21): No manifest needed (ConfigManager, Serialization, Reporting, etc.)
- **Excluded** (1): Merchant (build error)

## Next Session Plan

### Priority 1: Visual Verification

- [ ] Install and configure asciinema or screen recording tool
- [ ] Record console app session showing:
  - TUI rendering
  - Player movement
  - Inventory interaction
  - Map display
  - Quit functionality
- [ ] Capture recording to `docs/demos/` or project website

### Priority 2: Merchant Plugin Fix

- [ ] Add `LablabBean.Plugins.NPC` project reference to Merchant
- [ ] Verify Merchant builds successfully
- [ ] Re-enable Merchant in Build.cs exclusion filter
- [ ] Test Merchant plugin loads and runs

### Priority 3: World Service Policy

**Decision needed**: Should verification mode provide a minimal World service?

**Option A**: Provide minimal World (stub implementation)

- **Pros**: More plugins fully functional, better verification coverage
- **Cons**: Additional mock infrastructure needed

**Option B**: Keep passive mode

- **Pros**: Clean separation, no mock complexity
- **Cons**: Some plugins can't demonstrate full functionality

**Recommendation**: Keep passive mode for now, document which features require World

### Priority 4: Contract Probes Enhancement

- [ ] Expand ContractProbes to test more plugin interfaces
- [ ] Add more PASS/FAIL badges to HTML reports
- [ ] Show which contracts each plugin implements
- [ ] Display probe results in verification report

### Priority 5: CI/CD Integration (Optional)

- [ ] Wire VerifyArtifact into GitHub Actions workflow
- [ ] Publish verification reports as build artifacts
- [ ] Fail build if plugin verification fails
- [ ] Adjust Playwright tests to target latest versioned test-reports

## File Changes Summary

### Modified Files

- `build/nuke/Build.cs`:
  - Line 165-175: Added Version to report filenames
  - Line 392: Added Version to windows-build-metrics filename
  - Line 459-465: Removed Boss, Progression, Quest from exclusion filter (PublishConsole)
  - Line 554-560: Removed Boss, Progression, Quest from exclusion filter (PublishAll)
  - Line 659: Added VerifyArtifact to ReleaseConsole dependencies
  - Line 666-711: VerifyArtifact target implementation
  - Line 684: Removed `--exclude merchant` flag

### Artifact Structure

```
build/_artifacts/0.0.4-144/
├── publish/
│   ├── console/
│   │   ├── LablabBean.Console.exe
│   │   └── plugins/
│   │       ├── LablabBean.Plugins.Boss/
│   │       ├── LablabBean.Plugins.Diagnostic.Firebase/
│   │       ├── LablabBean.Plugins.Diagnostic.OpenTelemetry/
│   │       ├── LablabBean.Plugins.Diagnostic.Sentry/
│   │       ├── LablabBean.Plugins.Hazards/
│   │       ├── LablabBean.Plugins.Inventory/
│   │       ├── LablabBean.Plugins.NPC/
│   │       ├── LablabBean.Plugins.Progression/
│   │       ├── LablabBean.Plugins.Quest/
│   │       ├── LablabBean.Plugins.Spells/
│   │       ├── LablabBean.Plugins.StatusEffects/
│   │       └── ... (21 infrastructure plugins)
│   └── windows/
├── test-reports/
│   ├── plugin-health-artifact-*.json/html/csv (NEW)
│   ├── build-metrics-*.html/csv
│   ├── session-analytics-*.html/csv
│   └── plugin-metrics-*.html/csv
├── test-results/
├── logs/
└── version.json
```

## Key Metrics

### Plugin Coverage Improvement

- **Before**: 8 plugins verified
- **After**: 11 plugins verified
- **Increase**: +3 plugins (37.5% increase)
- **Re-enabled**: Boss, Progression, Quest

### Build Performance

- **ReleaseConsole Duration**: ~1:24 (84 seconds)
  - Restore: 4s
  - Compile: 11s
  - PublishConsole: 61s
  - VerifyArtifact: 5s
  - GenerateReports: 1s
  - PrintVersion: <1s
  - ReleaseConsole: <1s

### Success Indicators

- ✅ 100% plugin load success rate
- ✅ Zero runtime errors during verification
- ✅ All plugins unload gracefully
- ✅ Verification reports generated successfully
- ✅ Build pipeline completes without errors

## Questions & Decisions

### Q1: Should we add more plugins with manifests?

**Answer**: Not needed. Infrastructure plugins (Serialization, ConfigManager, etc.) don't need manifests - they're internal dependencies, not discoverable game features.

### Q2: Why are some plugins in passive mode?

**Answer**: By design. NPC, Progression, Quest, and Spells need access to the game World to be fully functional. In verification mode (no active game), they initialize but don't perform world-dependent operations.

### Q3: How do we verify TUI actually works?

**Answer**: Need screen recording for next session. Current verification only confirms plugins load and app starts/stops cleanly.

### Q4: Should VerifyArtifact run in CI?

**Answer**: Yes, recommended for next iteration. Ensures every release is verified before deployment.

## References

### Related Documents

- `docs/console-app-handover.md` - Previous console app status (outdated - mentions Diagnostic excluded)
- `build/nuke/REPORTING.md` - NUKE build reporting documentation
- `docs/RELEASE.md` - Release process documentation

### Related PRs/Commits

- Commit: Integration of VerifyArtifact into ReleaseConsole
- Modified: build/nuke/Build.cs (plugin exclusions updated)

### Verification Commands

```powershell
# Check plugin count
Get-ChildItem build\_artifacts\0.0.4-144\publish\console\plugins\*\plugin.json | Measure-Object

# View verification results
Get-Content build\_artifacts\0.0.4-144\test-reports\plugin-health-artifact-*-*.json | ConvertFrom-Json | Select-Object -ExpandProperty plugins | Format-Table Name, State

# Run console from artifact
cd build\_artifacts\0.0.4-144\publish\console
.\LablabBean.Console.exe
```

## Session Notes

### Observations

1. Plugin loading is fast (average 5ms per plugin)
2. Memory footprint is low (0.29 MB total for 11 plugins)
3. Terminal.Gui v2 API integration appears functional based on logs
4. Quit dialog responds to keyboard input (Q key detected)

### Technical Debt

- No visual recording capability configured
- Merchant plugin build error needs investigation
- World service policy for verification mode undefined
- ContractProbes coverage could be expanded

### Success Criteria Met

- ✅ Artifact verification integrated into release pipeline
- ✅ Boss, Progression, Quest plugins re-enabled
- ✅ Plugin verification reports generated automatically
- ✅ Console app starts and shuts down cleanly
- ❌ Visual TUI verification (deferred to next session)

## Handover Complete

**Date**: 2025-10-24 21:00 UTC+8
**Next Session Focus**: Visual verification with screen recording, Merchant plugin fix
**Status**: Build pipeline functional, console app status partially verified
