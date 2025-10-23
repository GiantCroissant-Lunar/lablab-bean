# Spec Review: Tiered Plugin Architecture & Migrations

**Review Date**: 2025-10-21
**Reviewer**: Claude (cross-referenced with CrossMilo, PluginManoi, WingedBean)
**Specs Under Review**:

- `specs/004-tiered-plugin-architecture`
- `specs/005-inventory-plugin-migration`
- `specs/006-status-effects-plugin-migration`

---

## Executive Summary

The specs demonstrate **strong architectural alignment** with the reference projects. The other agent has correctly identified the key patterns from CrossMilo (contracts), PluginManoi (loader/registry), and WingedBean (plugin migration). However, there are **critical gaps and misalignments** that need to be addressed before implementation.

**Overall Assessment**: ✅ **APPROVE WITH REQUIRED CHANGES**

---

## Spec 004: Tiered Plugin Architecture

### ✅ Strengths

1. **Correct Tiered Separation**
   - Contracts on netstandard2.1 ✓
   - Hosts/plugins on net8.0 ✓
   - Matches CrossMilo pattern exactly

2. **Comprehensive Lifecycle**
   - Initialize → Start → Stop sequence ✓
   - Matches WingedBean's OnActivateAsync pattern
   - State machine aligns with PluginManoi

3. **Dependency Resolution**
   - Hard vs Soft dependencies ✓
   - Topological sort mentioned ✓
   - Matches PluginManoi's RFC-0005

4. **AssemblyLoadContext Awareness**
   - Hot reload with collectible ALC ✓
   - Memory leak prevention ✓
   - Matches PluginManoi implementation

### ⚠️ Critical Issues

#### Issue 1: **IPlugin Interface Mismatch**

**Current Contract** (`specs/004-tiered-plugin-architecture/contracts/IPlugin.cs`):

```csharp
public interface IPlugin
{
    void Initialize(IServiceCollection services, IConfiguration configuration, IPluginHost host);
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
```

**Problem**: This exposes `IServiceCollection` directly to plugins, which **violates ALC isolation boundaries**.

**Reference Pattern** (WingedBean/PluginManoi):

```csharp
public interface IPlugin
{
    Task OnActivateAsync(IRegistry registry, CancellationToken ct);
    Task OnDeactivateAsync(CancellationToken ct);
}
```

**Why This Matters**:

- `IServiceCollection` is a **build-time** DI container
- Plugins should use `IRegistry` for **runtime** service registration
- Registry supports priority-based selection across ALCs
- `IServiceCollection` doesn't support cross-ALC type identity

**Recommended Fix**:

```csharp
public interface IPlugin
{
    string Id { get; }
    string Name { get; }
    string Version { get; }

    Task InitializeAsync(IPluginContext context, CancellationToken ct = default);
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}

public interface IPluginContext
{
    IRegistry Registry { get; }
    IConfiguration Configuration { get; }
    ILogger Logger { get; }
    IPluginHost Host { get; }
}
```

**Severity**: 🔴 **CRITICAL** - Blocks plugin isolation

---

#### Issue 2: **Missing IRegistry Definition**

**Gap**: Spec mentions `IPluginRegistry` (FR-005) but doesn't specify **service registry** pattern.

**Reference Pattern** (PluginManoi/CrossMilo):

```csharp
public interface IRegistry
{
    void Register<TService>(TService implementation, int priority = 0) where TService : class;
    TService Get<TService>(SelectionMode mode = SelectionMode.HighestPriority) where TService : class;
    IEnumerable<TService> GetAll<TService>() where TService : class;
    bool IsRegistered<TService>() where TService : class;
}

public enum SelectionMode
{
    One,              // Exactly one required (throws if multiple)
    HighestPriority, // Return highest priority
    All              // Use GetAll()
}
```

**Why This Matters**:

- **Type identity across ALCs**: Registry uses runtime type matching, not compile-time references
- **Priority-based selection**: Multiple implementations can coexist (e.g., console vs SadConsole UI)
- **Decoupling**: Plugins don't reference each other directly

**Recommended Action**: Add `IRegistry` to contracts alongside `IPluginRegistry`.

**Severity**: 🔴 **CRITICAL** - Core architectural component missing

---

#### Issue 3: **Manifest Schema Incomplete**

**Current Manifest** (`PluginManifest.cs`):

```csharp
public sealed class PluginManifest
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public string? EntryAssembly { get; init; }
    public string? EntryType { get; init; }
    public List<PluginDependency> Dependencies { get; init; } = new();
}
```

**Missing Fields** (from PluginManoi RFC schema):

```csharp
public string? Description { get; init; }
public string? Author { get; init; }
public string? License { get; init; }
public Dictionary<string, string> EntryPoint { get; init; } // Multi-profile support
public List<string> Capabilities { get; init; }
public List<string> SupportedProfiles { get; init; }
public int Priority { get; init; } = 100;
public string? LoadStrategy { get; init; } // "eager" | "lazy" | "explicit"
public List<string> TargetProcesses { get; init; } // ["Console", "Unity"]
public List<string> TargetPlatforms { get; init; } // ["Windows", "Linux"]
```

**Why This Matters**:

- **Multi-profile support**: Console, Unity, SadConsole have different entry points
- **Filtering**: Pre-load filtering by process/platform (WingedBean RFC-0006)
- **Load strategy**: Lazy loading for optional plugins
- **Capabilities**: Feature flags for plugin discovery

**Recommended Action**: Expand manifest schema to match PluginManoi RFC-0004.

**Severity**: 🟡 **MEDIUM** - Limits future extensibility

---

#### Issue 4: **Hot Reload Implementation Details Missing**

**Spec Statement** (SC-003):
> "At least one plugin can be hot reloaded 3× in a row without memory growth >10%."

**Gap**: No specification of:

1. **File watching strategy** (manual vs automatic)
2. **Unload verification** (`AssemblyLoadContext.IsCollectible` checks)
3. **GC forcing strategy** (PluginManoi uses 10 cycles with sleep)
4. **Reload trigger** (file change, user command, API call)

**Reference Pattern** (PluginManoi):

```csharp
public async Task UnloadAsync(ILoadedPlugin plugin)
{
    await plugin.DeactivateAsync();
    await _contextProvider.UnloadContextAsync(contextName, waitForUnload: true);

    // Force GC cycles
    for (int i = 0; i < 10 && alc.IsCollectible; i++)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Thread.Sleep(100);
    }
}
```

**Recommended Action**: Add hot reload requirements to FR section.

**Severity**: 🟡 **MEDIUM** - P2 feature, can defer to implementation

---

#### Issue 5: **Dependency Resolution Cycle Detection Unclear**

**Spec Statement** (FR-003):
> "Provide dependency resolution with cycle detection and topological start order."

**Gap**: Spec doesn't specify:

1. **Algorithm choice** (Kahn's vs DFS)
2. **Cycle error handling** (fail all vs exclude cycle participants)
3. **Version selection strategy** (highest semver vs explicit)

**Reference Pattern** (PluginManoi):

- Uses **Kahn's topological sort**
- **Cycle detection**: If `sorted.Count != total`, throw `InvalidOperationException`
- **Version selection**: Highest semantic version per plugin ID
- **Conflict resolution**: Priority → Version → Alphabetical

**Recommended Action**: Specify algorithm and error handling in FR-003.

**Severity**: 🟡 **MEDIUM** - Implementation clarity

---

#### Issue 6: **Metrics Specification Vague**

**Spec Statement** (FR-009):
> "Metrics: load time per plugin, failures, reload counts."

**Gap**: No specification of:

1. **Metric storage** (in-memory vs persistent)
2. **Metric exposure** (API, logs, telemetry)
3. **Metric format** (plain values vs structured events)

**Reference Pattern** (WingedBean):

- Uses **OpenTelemetry** for structured metrics
- **ActivitySource** for distributed tracing
- **Metrics endpoint** for Prometheus scraping

**Recommended Action**: Defer metrics to P2 or specify concrete format.

**Severity**: 🟢 **LOW** - Non-critical for MVP

---

### 🔧 Recommended Changes to Spec 004

#### Change 1: Update IPlugin Interface

**File**: `specs/004-tiered-plugin-architecture/contracts/IPlugin.cs`

```csharp
public interface IPlugin
{
    string Id { get; }
    string Name { get; }
    string Version { get; }

    Task InitializeAsync(IPluginContext context, CancellationToken ct = default);
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}

public interface IPluginContext
{
    IRegistry Registry { get; }
    IConfiguration Configuration { get; }
    ILogger Logger { get; }
    IPluginHost Host { get; }
}
```

#### Change 2: Add IRegistry Contract

**File**: `specs/004-tiered-plugin-architecture/contracts/IRegistry.cs` (new)

```csharp
public interface IRegistry
{
    void Register<TService>(TService implementation, ServiceMetadata metadata) where TService : class;
    void Register<TService>(TService implementation, int priority = 0) where TService : class;

    TService Get<TService>(SelectionMode mode = SelectionMode.HighestPriority) where TService : class;
    IEnumerable<TService> GetAll<TService>() where TService : class;

    bool IsRegistered<TService>() where TService : class;
    bool Unregister<TService>(TService implementation) where TService : class;
}

public enum SelectionMode
{
    One,              // Exactly one required
    HighestPriority, // Return highest priority
    All              // Use GetAll()
}

public class ServiceMetadata
{
    public int Priority { get; set; } = 100;
    public string? Name { get; set; }
    public string? Version { get; set; }
}
```

#### Change 3: Expand PluginManifest

**File**: `specs/004-tiered-plugin-architecture/contracts/PluginManifest.cs`

Add fields:

```csharp
public Dictionary<string, string> EntryPoint { get; init; } = new();
public List<string> Capabilities { get; init; } = new();
public int Priority { get; init; } = 100;
public string? LoadStrategy { get; init; }
```

#### Change 4: Clarify Dependency Resolution

**File**: `specs/004-tiered-plugin-architecture/spec.md`

Update FR-003:

```markdown
FR-003: Provide dependency resolution using Kahn's topological sort algorithm.
  - Hard dependencies: Missing → plugin excluded from load order, ERROR logged
  - Soft dependencies: Missing → WARNING logged, plugin loaded with reduced features
  - Cycle detection: If sorted.Count != pluginCount, throw InvalidOperationException
  - Version selection: Highest semantic version per plugin ID
```

---

## Spec 005: Inventory Plugin Migration

### ✅ Strengths

1. **Clear Migration Goal**
   - Self-contained plugin ✓
   - Minimal host coupling ✓

2. **Service Pattern**
   - `IInventoryService` in DI ✓
   - Matches WingedBean pattern

3. **Backward Compatibility**
   - References spec-001 ✓

### ⚠️ Issues

#### Issue 1: **Missing Manifest Definition**

**Gap**: Spec doesn't include example `.plugin.json` for inventory plugin.

**Recommended Addition**:

```json
{
  "id": "lablabbean.plugins.inventory",
  "version": "1.0.0",
  "name": "Inventory System",
  "description": "ECS-based inventory with pickup/use/equip",
  "entryPoint": {
    "dotnet": "bin/LablabBean.Plugins.Inventory.dll"
  },
  "dependencies": {
    "plugins": [
      {
        "pluginId": "lablabbean.plugins.ecs",
        "versionRange": "[1.0.0,2.0.0)",
        "flags": "Hard"
      }
    ]
  },
  "exports": {
    "services": [
      {
        "interface": "IInventoryService",
        "implementation": "InventoryService",
        "lifecycle": "singleton"
      }
    ]
  },
  "priority": 75,
  "capabilities": ["inventory", "equipment"]
}
```

**Severity**: 🟡 **MEDIUM** - Needed for implementation clarity

---

#### Issue 2: **ECS Dependency Unclear**

**Spec Statement** (FR-002):
> "Maintain ECS components in plugin assembly"

**Question**: Which ECS implementation?

- Option A: Arch ECS (current project dependency)
- Option B: Plugin-specific ECS abstraction
- Option C: Shared ECS plugin (like WingedBean)

**Reference Pattern** (WingedBean):

- Separate **ArchECS plugin** provides ECS runtime
- Game plugins depend on ECS plugin
- ECS registered with priority 100 (framework level)

**Recommended Clarification**: Specify dependency on shared ECS plugin or inline ECS.

**Severity**: 🟡 **MEDIUM** - Architectural decision

---

#### Issue 3: **Event Hooks Undefined**

**Spec Statement** (FR-003):
> "Event hooks for HUD updates (no direct UI dependency)"

**Gap**: Spec doesn't define event schema or delivery mechanism.

**Reference Pattern** (WingedBean):

- Uses `IObservable<T>` (Rx.NET) for reactive state
- Publishes via `BehaviorSubject<T>`
- UI subscribes and updates reactively

**Recommended Addition**:

```csharp
public interface IInventoryService
{
    IObservable<InventoryChangedEvent> InventoryChanged { get; }
    IObservable<ItemUsedEvent> ItemUsed { get; }

    // ... service methods
}
```

**Severity**: 🟡 **MEDIUM** - Integration contract needed

---

### 🔧 Recommended Changes to Spec 005

#### Change 1: Add Manifest Example

**File**: `specs/005-inventory-plugin-migration/manifest.plugin.json` (new)

(See Issue 1 above for content)

#### Change 2: Clarify ECS Dependency

**File**: `specs/005-inventory-plugin-migration/spec.md`

Add to Requirements:

```markdown
FR-005: Declare hard dependency on shared ECS plugin (lablabbean.plugins.ecs)
  - Plugin discovers ECS via IRegistry.Get<IECSService>()
  - Components registered into shared world
  - Systems added to shared system group
```

#### Change 3: Define Event Schema

**File**: `specs/005-inventory-plugin-migration/events.cs` (new)

```csharp
public record InventoryChangedEvent(EntityId Entity, InventorySnapshot Snapshot);
public record ItemUsedEvent(EntityId Entity, ItemId Item, bool Success);

public interface IInventoryService
{
    IObservable<InventoryChangedEvent> InventoryChanged { get; }
    // ... methods
}
```

---

## Spec 006: Status Effects Plugin Migration

### ✅ Strengths

1. **Service Interface**
   - `IStatusEffectService` with clear APIs ✓

2. **Backward Compatibility**
   - References spec-002 ✓

### ⚠️ Issues

**Same issues as Spec 005**:

1. Missing manifest definition
2. ECS dependency unclear
3. Event hooks undefined

**Additional Issue**: Turn-based timing unclear

**Spec Statement** (FR-001):
> "Plugin tracks active effects per entity and updates durations on turn ticks."

**Question**: How does plugin receive turn tick events?

- Option A: Subscribes to `ITurnManager.TurnCompleted` event
- Option B: Polling via system update
- Option C: Host calls `IStatusEffectService.Tick()` explicitly

**Recommended Clarification**: Specify turn tick integration.

**Severity**: 🟡 **MEDIUM** - Integration contract

---

### 🔧 Recommended Changes to Spec 006

Same pattern as Spec 005, plus:

#### Change 1: Define Turn Integration

**File**: `specs/006-status-effects-plugin-migration/spec.md`

Add to Requirements:

```markdown
FR-004: Subscribe to turn events from turn manager plugin
  - Discovers ITurnManager via IRegistry.Get<ITurnManager>()
  - Subscribes to TurnCompleted event
  - Updates effect durations on each turn
```

---

## Cross-Cutting Recommendations

### 1. **Adopt Multi-Profile Support from Day 1**

**Current State**: Specs assume single console host.

**Future Need**: WingedBean demonstrates Console, Unity, SadConsole, and Web hosts.

**Recommendation**:

- Use `EntryPoint` dictionary in manifest (not single `EntryAssembly`)
- Design contracts to be UI-agnostic
- Test with at least Console + SadConsole from start

**Example**:

```json
{
  "entryPoint": {
    "dotnet.console": "bin/Plugin.Console.dll",
    "dotnet.sadconsole": "bin/Plugin.SadConsole.dll",
    "unity": "bin/Plugin.Unity.dll"
  }
}
```

---

### 2. **Separate Contracts Assembly**

**Current State**: Contracts in spec folders, not referenced by project.

**WingedBean Pattern**:

```
LablabBean.Plugins.Contracts.csproj
├─ TargetFramework: netstandard2.1
├─ IPlugin, IRegistry, IPluginHost
└─ <Private>false</Private> (shared across ALCs)

LablabBean.Game.Contracts.csproj
├─ TargetFramework: netstandard2.1
├─ IInventoryService, IStatusEffectService
└─ <Private>false</Private>
```

**Recommendation**: Create contracts assemblies before implementation starts.

---

### 3. **Add Caching Strategy**

**PluginManoi Pattern** (RFC-0004):

- SHA256 hash of manifest files
- Binary cache per plugin directory
- 10x faster discovery on valid cache

**Recommendation**: Add FR for plugin discovery caching (P2 initially).

---

### 4. **Security Considerations**

**Gap**: Specs don't mention plugin signing or verification.

**PluginManoi Pattern**:

- Optional `security.signatureRequired` in manifest
- SHA256 hash verification
- Publisher trust model

**Recommendation**: Document out-of-scope for MVP, add to future work.

---

## Summary of Required Changes

| Spec | Change | Severity | Effort |
|------|--------|----------|--------|
| 004 | Update IPlugin interface | 🔴 Critical | 2h |
| 004 | Add IRegistry contract | 🔴 Critical | 3h |
| 004 | Expand PluginManifest | 🟡 Medium | 1h |
| 004 | Clarify dependency resolution | 🟡 Medium | 30m |
| 004 | Hot reload details | 🟡 Medium | 1h |
| 005 | Add manifest example | 🟡 Medium | 30m |
| 005 | Clarify ECS dependency | 🟡 Medium | 1h |
| 005 | Define event schema | 🟡 Medium | 1h |
| 006 | Add manifest example | 🟡 Medium | 30m |
| 006 | Define turn integration | 🟡 Medium | 1h |
| All | Create contracts assemblies | 🔴 Critical | 4h |
| All | Multi-profile support | 🟢 Low | 2h |

**Total Estimated Effort**: ~17 hours of spec refinement

---

## Implementation Readiness

### Spec 004 (Tiered Plugin Architecture)

**Status**: ⚠️ **NOT READY**
**Blockers**:

1. IPlugin interface needs redesign (IPluginContext pattern)
2. IRegistry contract missing
3. Contracts assembly not created

**Estimated Time to Ready**: 6-8 hours

---

### Spec 005 (Inventory Migration)

**Status**: ⚠️ **NOT READY**
**Dependencies**:

1. Spec 004 must be complete first
2. ECS plugin strategy needed
3. Event schema required

**Estimated Time to Ready**: 3-4 hours (after 004 complete)

---

### Spec 006 (Status Effects Migration)

**Status**: ⚠️ **NOT READY**
**Dependencies**:

1. Spec 004 must be complete first
2. Same issues as Spec 005
3. Turn manager integration needed

**Estimated Time to Ready**: 3-4 hours (after 004 complete)

---

## Recommended Next Steps

1. **CRITICAL**: Update Spec 004 contracts
   - Redesign IPlugin interface
   - Add IRegistry contract
   - Expand PluginManifest schema

2. **CRITICAL**: Create contracts assemblies
   - `LablabBean.Plugins.Contracts` (netstandard2.1)
   - `LablabBean.Game.Contracts` (netstandard2.1)

3. **HIGH**: Clarify ECS strategy
   - Shared ECS plugin OR inline ECS
   - Document in both 005 and 006

4. **MEDIUM**: Add manifest examples
   - One per migration spec
   - Shows dependency declarations

5. **MEDIUM**: Define event schemas
   - Reactive patterns (IObservable)
   - Document in service contracts

6. **LOW**: Consider multi-profile support
   - EntryPoint dictionary
   - Profile-specific implementations

---

## Final Verdict

**Overall Assessment**: The specs demonstrate excellent understanding of the reference architectures and correctly identify the key patterns. However, **critical contract design issues** prevent immediate implementation.

**Recommendation**:
✅ **APPROVE ARCHITECTURAL DIRECTION**
⚠️ **HOLD IMPLEMENTATION** until contract changes applied

The foundational approach is sound, but the devil is in the details. The IPlugin/IRegistry mismatch and missing contracts assembly would cause significant rework if discovered mid-implementation.

**Estimated Time to Production-Ready Specs**: 12-16 hours of focused refinement.

---

**Reviewed by**: Claude
**Date**: 2025-10-21
**Reference Projects**: CrossMilo, PluginManoi, WingedBean
