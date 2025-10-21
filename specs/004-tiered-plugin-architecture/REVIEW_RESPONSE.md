# Review Response: Critical Issues Fixed

**Date**: 2025-10-21T17:10:00+08:00
**Review Document**: `specs/SPEC_REVIEW_004_005_006.md`
**Status**: ✅ **CRITICAL ISSUES RESOLVED**

---

## Changes Applied

### ✅ Issue 1: IPlugin Interface Redesign (CRITICAL)

**Problem**: Direct `IServiceCollection` exposure violated ALC isolation.

**Fix Applied**:
- Updated `contracts/IPlugin.cs` to use `IPluginContext` pattern
- Changed signature: `Initialize(IServiceCollection, ...)` → `InitializeAsync(IPluginContext, ...)`
- Created `IPluginContext` interface with `IRegistry`, `IConfiguration`, `ILogger`, `IPluginHost`
- Updated `quickstart.md` with corrected example using `context.Registry.Register<T>()`

**Files Modified**:
- `specs/004-tiered-plugin-architecture/contracts/IPlugin.cs`
- `specs/004-tiered-plugin-architecture/quickstart.md`
- `specs/004-tiered-plugin-architecture/data-model.md`

---

### ✅ Issue 2: Missing IRegistry Contract (CRITICAL)

**Problem**: Cross-ALC service registry pattern was completely missing from contracts.

**Fix Applied**:
- Created `contracts/IRegistry.cs` with priority-based service registration
- Added `SelectionMode` enum (One, HighestPriority, All)
- Added `ServiceMetadata` class for priority, name, version
- Updated FR-006 in `spec.md` to reference IRegistry
- Updated `data-model.md` to include IRegistry interface

**Files Created**:
- `specs/004-tiered-plugin-architecture/contracts/IRegistry.cs` (NEW)

**Files Modified**:
- `specs/004-tiered-plugin-architecture/spec.md`
- `specs/004-tiered-plugin-architecture/data-model.md`

---

### ✅ Issue 3: Incomplete Manifest Schema (MEDIUM)

**Problem**: Manifest lacked multi-profile support, capabilities, priority, load strategy.

**Fix Applied**:
- Expanded `PluginManifest` with:
  - `Dictionary<string, string> EntryPoint` (multi-profile entry points)
  - `List<string> Capabilities` (feature flags)
  - `List<string> SupportedProfiles` (Console, Unity, SadConsole, etc.)
  - `int Priority` (default: 100)
  - `string? LoadStrategy` (eager, lazy, explicit)
  - `List<string> TargetProcesses` and `TargetPlatforms`
- Kept legacy `EntryAssembly` and `EntryType` for backward compatibility
- Updated FR-001 in `spec.md` to reference expanded schema

**Files Modified**:
- `specs/004-tiered-plugin-architecture/contracts/PluginManifest.cs`
- `specs/004-tiered-plugin-architecture/spec.md`

---

### ✅ Issue 4: Dependency Resolution Unclear (MEDIUM)

**Problem**: Algorithm choice and cycle detection details were vague.

**Fix Applied**:
- Updated FR-003 in `spec.md` with explicit requirements:
  - **Algorithm**: Kahn's topological sort
  - **Hard deps**: Missing → exclude plugin, log ERROR
  - **Soft deps**: Missing → log WARNING, load with reduced features
  - **Cycle detection**: `sorted.Count != pluginCount` → throw `InvalidOperationException`
  - **Version selection**: Highest semantic version per plugin ID (NuGet.Versioning)

**Files Modified**:
- `specs/004-tiered-plugin-architecture/spec.md` (FR-003)

---

### ✅ Issue 5: Updated Key Entities

**Problem**: Key entities list didn't reflect new contracts.

**Fix Applied**:
- Added `IPluginContext`, `IRegistry`, `ServiceMetadata` to key entities
- Clarified `IPluginHost` and `IPlugin` in the list
- Updated spec.md and data-model.md

**Files Modified**:
- `specs/004-tiered-plugin-architecture/spec.md`

---

## Remaining Work (Non-Blocking)

### 🟡 Hot Reload Implementation Details (P2)
**Status**: Deferred to implementation phase
**Reason**: P2 feature; algorithm is clear from PluginManoi reference

### 🟡 Metrics Specification (P2)
**Status**: FR-009 marked as optional P2, OpenTelemetry recommended
**Reason**: Not critical for MVP

### 🟢 Multi-Profile Support (LOW)
**Status**: Manifest schema supports it; spec documents it
**Reason**: Can be tested with Console + SadConsole in Phase 4

### 🟢 Contracts Assembly Creation (Implementation)
**Status**: Spec contracts updated; actual projects deferred to implementation
**Reason**: Will be created in implementation phase with correct `netstandard2.1` targeting

---

## Spec Readiness Status

### Spec 004: Tiered Plugin Architecture
**Status**: ✅ **READY FOR IMPLEMENTATION**
**Critical Blockers Resolved**: 2/2
- ✅ IPlugin interface redesigned with IPluginContext
- ✅ IRegistry contract added
- ✅ Manifest schema expanded
- ✅ Dependency resolution algorithm specified

**Estimated Implementation Time**: 16-20 hours (contracts + core + loader + demo)

---

### Spec 005: Inventory Plugin Migration
**Status**: ⚠️ **DEPENDENT ON 004**
**Next Actions**:
1. Wait for 004 implementation to complete
2. Add manifest example with ECS plugin dependency
3. Define event schema (IObservable<InventoryChangedEvent>)

**Estimated Ready Time**: 3-4 hours after 004 complete

---

### Spec 006: Status Effects Plugin Migration
**Status**: ⚠️ **DEPENDENT ON 004**
**Next Actions**:
1. Wait for 004 implementation to complete
2. Add manifest example and turn integration strategy
3. Define event schema (IObservable<EffectAppliedEvent>)

**Estimated Ready Time**: 3-4 hours after 004 complete

---

## Implementation Order

1. **Phase 1: Contracts Assembly** (4h)
   - Create `LablabBean.Plugins.Contracts` (netstandard2.1)
   - Implement IPlugin, IPluginContext, IRegistry, IPluginHost, manifest models

2. **Phase 2: Core & Registry** (6h)
   - Create `LablabBean.Plugins.Core` (netstandard2.1)
   - Implement Registry, PluginRegistry, dependency resolver

3. **Phase 3: Host Loader** (4h)
   - Add `PluginLoaderHostedService` to Console and Windows hosts
   - Implement ALC loading, lifecycle orchestration

4. **Phase 4: Demo Plugin** (4h)
   - Create `LablabBean.Plugins.DungeonGame`
   - E2E test: discover → load → start → stop

5. **Phase 5: Migrations** (Deferred to 005/006 specs)
   - Inventory plugin
   - Status Effects plugin

---

## Summary

**Critical Issues**: 2/2 resolved ✅
**Medium Issues**: 2/2 resolved ✅
**Low Issues**: Documented and deferred ✅

**Implementation Readiness**: ✅ **READY TO PROCEED**

All blocking architectural issues have been addressed. The contracts now follow the PluginManoi/WingedBean patterns correctly with:
- ALC-safe IPluginContext boundary
- Cross-ALC IRegistry for service registration
- Multi-profile manifest schema
- Explicit dependency resolution algorithm

---

**Next Step**: Begin Phase 1 (Create Contracts Assembly) when approved.
