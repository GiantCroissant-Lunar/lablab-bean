---
doc_id: DOC-2025-00066
title: Contract Architecture Gap Analysis - Lablab-Bean vs Cross-Milo
doc_type: finding
status: draft
canonical: true
created: 2025-10-21
tags: [architecture, contracts, gaps, cross-milo-comparison]
summary: Analysis of gaps between lablab-bean's current contract implementation and cross-milo reference architecture
---

# Contract Architecture Gap Analysis

## Executive Summary

This document analyzes the gaps between lablab-bean's current contract implementation and the comprehensive cross-milo reference architecture. While lablab-bean has started implementing the tiered contract pattern, significant gaps remain in both contract assemblies and infrastructure components.

## Current State Comparison

### Contract Assemblies

| Contract Domain | Cross-Milo | Lablab-Bean | Status |
|----------------|------------|-------------|--------|
| **Base Contracts** | CrossMilo.Contracts | LablabBean.Plugins.Contracts | ✅ Partial (missing IEventBus) |
| **Game** | ❌ N/A | ✅ LablabBean.Contracts.Game | ✅ Present |
| **UI** | CrossMilo.Contracts.UI | ✅ LablabBean.Contracts.UI | ✅ Present |
| **Analytics** | ✅ CrossMilo.Contracts.Analytics | ❌ Missing | ❌ Gap |
| **Audio** | ✅ CrossMilo.Contracts.Audio | ❌ Missing | ❌ Gap |
| **Capability** | ✅ CrossMilo.Contracts.Capability | ❌ Missing | ❌ Gap |
| **Config** | ✅ CrossMilo.Contracts.Config | ❌ Missing | ❌ Gap |
| **Diagnostics** | ✅ CrossMilo.Contracts.Diagnostics | ❌ Missing | ❌ Gap |
| **GOAP** | ✅ CrossMilo.Contracts.Goap | ❌ Missing | ❌ Gap |
| **Hosting** | ✅ CrossMilo.Contracts.Hosting | ❌ Missing | ❌ Gap |
| **Input** | ✅ CrossMilo.Contracts.Input | ❌ Missing | ❌ Gap |
| **Recorder** | ✅ CrossMilo.Contracts.Recorder | ❌ Missing | ❌ Gap |
| **Resilience** | ✅ CrossMilo.Contracts.Resilience | ❌ Missing | ❌ Gap |
| **Resource** | ✅ CrossMilo.Contracts.Resource | ❌ Missing | ❌ Gap |
| **Scene** | ✅ CrossMilo.Contracts.Scene | ❌ Missing | ❌ Gap |
| **Terminal** | ✅ CrossMilo.Contracts.Terminal | ❌ Missing | ❌ Gap |

**Summary**:

- Cross-Milo: 15 contract assemblies
- Lablab-Bean: 2 contract assemblies (Game, UI) + 1 base (Plugins.Contracts)
- **Gap: 13 missing contract domains**

### Infrastructure Components (Tier 2)

| Component | Cross-Milo | Lablab-Bean | Status |
|-----------|------------|-------------|--------|
| **IRegistry** | ✅ CrossMilo.Contracts | ✅ LablabBean.Plugins.Contracts | ✅ Complete |
| **IEventBus** | ✅ CrossMilo.Contracts | ❌ Missing | ❌ Critical Gap |
| **ServiceRegistry** | ✅ PluginManoi.Core | ✅ LablabBean.Plugins.Core | ✅ Complete |
| **EventBus Implementation** | ✅ PluginManoi.Core | ❌ Missing | ❌ Critical Gap |
| **Source Generator (Proxy)** | ✅ CrossMilo.SourceGenerators.Proxy | ❌ Missing | ❌ Major Gap |
| **SelectionMode Enum** | ✅ CrossMilo.Contracts | ✅ LablabBean.Plugins.Contracts | ✅ Complete |
| **ServiceMetadata** | ✅ CrossMilo.Contracts | ✅ LablabBean.Plugins.Contracts | ✅ Complete |
| **Attributes** | ✅ RealizeService, SelectionStrategy | ❌ Missing | ❌ Major Gap |

**Summary**:

- ✅ Complete: IRegistry, ServiceRegistry, SelectionMode, ServiceMetadata (4/8)
- ❌ Missing: IEventBus, EventBus implementation, Source Generator, Attributes (4/8)

## Detailed Gap Analysis

### 1. Missing Base Contracts (Tier 1)

#### IEventBus Interface

**Status**: ❌ **Critical Gap**

**What's Missing**:

```csharp
// Location: LablabBean.Plugins.Contracts/IEventBus.cs (DOES NOT EXIST)
public interface IEventBus
{
    Task PublishAsync<T>(T eventData) where T : class;
    void Subscribe<T>(Func<T, Task> handler) where T : class;
}
```

**Impact**:

- No event-driven communication between plugins
- Plugins must use direct dependencies or polling
- Cannot implement analytics, diagnostics, or other cross-cutting concerns

**Priority**: 🔴 **P0 - Blocking**

---

### 2. Missing Source Generator (Tier 2)

#### CrossMilo.SourceGenerators.Proxy

**Status**: ❌ **Major Gap**

**What's Missing**:

- Roslyn source generator that automatically generates proxy service implementations
- `[RealizeService(typeof(IService))]` attribute
- `[SelectionStrategy(SelectionMode.HighestPriority)]` attribute
- Automatic delegation to `IRegistry.Get<T>()`

**Example of Generated Code** (from cross-milo):

```csharp
// Developer writes this:
[RealizeService(typeof(IService))]
[SelectionStrategy(SelectionMode.HighestPriority)]
public partial class ProxyService : IService
{
    private readonly IRegistry _registry;
    public ProxyService(IRegistry registry) => _registry = registry;
}

// Source generator creates this:
public partial class ProxyService
{
    public void Play(string clipId, AudioPlayOptions? options = null)
    {
        var implementation = _registry.Get<IService>(SelectionMode.HighestPriority);
        implementation.Play(clipId, options);
    }

    // ... all other interface methods
}
```

**Impact**:

- Developers must manually write proxy delegation code
- Increased boilerplate and maintenance burden
- Inconsistent proxy patterns across services
- Higher chance of bugs in delegation logic

**Priority**: 🟡 **P2 - Important** (Can use manual proxies initially)

---

### 3. Missing Domain Contract Assemblies

#### High Priority Contracts (Needed for Dungeon Crawler)

##### 3.1 LablabBean.Contracts.Scene

**Status**: ❌ **High Priority Gap**

**Cross-Milo Equivalent**: `CrossMilo.Contracts.Scene`

**What It Provides**:

```csharp
// Scene service interface
public interface IService
{
    void Initialize();
    Viewport GetViewport();
    CameraViewport GetCameraViewport();
    void SetCamera(Camera camera);
    void UpdateWorld(IReadOnlyList<EntitySnapshot> snapshots);
    void Run();
    event EventHandler<SceneShutdownEventArgs>? Shutdown;
}

// Supporting types
public record SceneLoadedEvent(string SceneId, DateTimeOffset Timestamp);
public record SceneUnloadedEvent(string SceneId, DateTimeOffset Timestamp);
public record Camera(Position Position, float Zoom);
public record Viewport(int Width, int Height);
```

**Why Needed**:

- Dungeon level/scene management
- Camera and viewport control
- World rendering coordination
- Scene transitions (entering/exiting dungeons)

**Priority**: 🔴 **P1 - Critical for dungeon crawler**

---

##### 3.2 LablabBean.Contracts.Input

**Status**: ❌ **High Priority Gap**

**Cross-Milo Equivalent**: `CrossMilo.Contracts.Input`

**What It Provides**:

```csharp
// Input router (scope-based input handling)
namespace Input.Router;
public interface IService<TInputEvent> where TInputEvent : class
{
    IDisposable PushScope(Scope.IService<TInputEvent> scope);
    void Dispatch(TInputEvent inputEvent);
    Scope.IService<TInputEvent>? Top { get; }
}

// Input mapper (logical actions from raw input)
namespace Input.Mapper;
public interface IService
{
    void Map(RawKeyEvent rawKey);
    bool TryGetAction(string actionName, out Action action);
}

// Supporting types
public record RawKeyEvent(ConsoleKey Key, ConsoleModifiers Modifiers);
public record InputActionTriggeredEvent(string Action, DateTimeOffset Timestamp);
```

**Why Needed**:

- Keyboard input handling for dungeon navigation
- Modal input scopes (menus, dialogs, in-game)
- Action mapping (arrow keys → move north, 'i' → inventory)
- Platform-agnostic input abstraction

**Priority**: 🔴 **P1 - Critical for dungeon crawler**

---

##### 3.3 LablabBean.Contracts.Config

**Status**: ❌ **High Priority Gap**

**Cross-Milo Equivalent**: `CrossMilo.Contracts.Config`

**What It Provides**:

```csharp
public interface IService
{
    string? Get(string key);
    T? Get<T>(string key);
    IConfigSection GetSection(string key);
    void Set(string key, string value);
    bool Exists(string key);
    Task ReloadAsync(CancellationToken ct = default);
    event EventHandler<ConfigChangedEventArgs>? ConfigChanged;
}

// Supporting types
public record ConfigChangedEvent(string Key, string? OldValue, string? NewValue, DateTimeOffset Timestamp);
```

**Why Needed**:

- Game settings (difficulty, graphics, keybindings)
- Plugin configuration
- Runtime config reload
- Config change notifications via events

**Priority**: 🟠 **P2 - Important**

---

##### 3.4 LablabBean.Contracts.Audio

**Status**: ❌ **Medium Priority Gap**

**Cross-Milo Equivalent**: `CrossMilo.Contracts.Audio`

**What It Provides**:

```csharp
public interface IService
{
    void Play(string clipId, AudioPlayOptions? options = null);
    void Stop(string clipId);
    void StopAll();
    void Pause(string clipId);
    void Resume(string clipId);
    float Volume { get; set; };
    bool IsPlaying(string clipId);
    Task<bool> LoadAsync(string clipId, CancellationToken ct = default);
    void Unload(string clipId);
}

// Supporting types
public record AudioPlayOptions(float Volume = 1.0f, bool Loop = false);
```

**Why Needed**:

- Sound effects (combat, movement, item pickup)
- Background music for dungeons
- Audio feedback for UI interactions
- Platform-agnostic audio (Terminal.Gui has no audio, SadConsole/Unity do)

**Priority**: 🟡 **P3 - Nice to have**

---

#### Medium Priority Contracts (Cross-Cutting Concerns)

##### 3.5 LablabBean.Contracts.Analytics

**Status**: ❌ **Medium Priority Gap**

**Cross-Milo Equivalent**: `CrossMilo.Contracts.Analytics`

**What It Provides**:

```csharp
public interface IService
{
    Task Track(string eventName, object? properties = null);
    Task Identify(string userId, object? traits = null);
    Task Flush(CancellationToken ct = default);
    void SetTrackingEnabled(bool enabled);
}

// Supporting types
public record AnalyticsEventTrackedEvent(string EventName, DateTimeOffset Timestamp);
```

**Why Needed**:

- Player behavior tracking
- Game metrics (deaths, level completion times)
- A/B testing support
- Usage analytics

**Priority**: 🟡 **P3 - Nice to have**

---

##### 3.6 LablabBean.Contracts.Diagnostics

**Status**: ❌ **Medium Priority Gap**

**Cross-Milo Equivalent**: `CrossMilo.Contracts.Diagnostics`

**What It Provides**:

```csharp
public interface IService
{
    void CaptureException(Exception ex, object? context = null);
    void AddBreadcrumb(string message, string category, object? data = null);
    void SetUser(string userId, string? email = null);
    Task FlushAsync(CancellationToken ct = default);
}

// Supporting types
public record ErrorCapturedEvent(Exception Error, DateTimeOffset Timestamp);
```

**Why Needed**:

- Error tracking and reporting
- Debug breadcrumbs
- Performance monitoring
- Crash analytics

**Priority**: 🟡 **P3 - Nice to have**

---

##### 3.7 LablabBean.Contracts.Resource

**Status**: ❌ **Medium Priority Gap**

**Cross-Milo Equivalent**: `CrossMilo.Contracts.Resource`

**What It Provides**:

```csharp
public interface IService
{
    Task<T> LoadAsync<T>(string resourceId, CancellationToken ct = default);
    void Unload(string resourceId);
    bool IsLoaded(string resourceId);
    Task PreloadAsync(IEnumerable<string> resourceIds, CancellationToken ct = default);
}

// Supporting types
public record ResourceLoadedEvent(string ResourceId, DateTimeOffset Timestamp);
public record ResourceLoadFailedEvent(string ResourceId, Exception Error, DateTimeOffset Timestamp);
```

**Why Needed**:

- Asset loading (sprites, tiles, data files)
- Resource caching
- Preloading for performance
- Platform-agnostic resource management

**Priority**: 🟠 **P2 - Important** (for asset management)

---

#### Lower Priority Contracts (Advanced Features)

##### 3.8 LablabBean.Contracts.Resilience

**What It Provides**: Retry policies, circuit breakers, timeout handling
**Priority**: 🟢 **P4 - Optional**

##### 3.9 LablabBean.Contracts.Recorder

**What It Provides**: Replay recording/playback (URF format)
**Priority**: 🟢 **P4 - Optional**

##### 3.10 LablabBean.Contracts.Capability

**What It Provides**: Capability evaluation and decision making
**Priority**: 🟢 **P4 - Optional**

##### 3.11 LablabBean.Contracts.Goap

**What It Provides**: Goal-Oriented Action Planning for AI
**Priority**: 🟢 **P4 - Optional**

##### 3.12 LablabBean.Contracts.Hosting

**What It Provides**: Host lifecycle and environment abstraction
**Priority**: 🟢 **P4 - Optional**

##### 3.13 LablabBean.Contracts.Terminal

**What It Provides**: Terminal-specific rendering contracts
**Priority**: 🟢 **P4 - Optional** (may be covered by UI contracts)

---

## Recommended Phased Implementation

### Phase 1: Foundation (Spec 007 - Current)

**Goal**: Event-driven architecture basics

✅ Already in spec:

- Add `IEventBus` to `LablabBean.Plugins.Contracts`
- Implement `EventBus` in `LablabBean.Plugins.Core`
- Update `LablabBean.Contracts.Game` with events
- Update `LablabBean.Contracts.UI` with events

**Priority**: 🔴 **P0**

---

### Phase 2: Core Game Contracts

**Goal**: Essential contracts for dungeon crawler

🆕 Should add to spec:

- `LablabBean.Contracts.Scene` - Level/dungeon management, camera, viewport
- `LablabBean.Contracts.Input` - Input routing, mapping, scopes
- `LablabBean.Contracts.Config` - Configuration management
- `LablabBean.Contracts.Resource` - Asset loading and management

**Priority**: 🔴 **P1**

**Justification**:

- Scene: Required for dungeon navigation and world rendering
- Input: Required for player controls and modal interactions
- Config: Required for game settings and plugin configuration
- Resource: Required for loading dungeon data, sprites, etc.

---

### Phase 3: Source Generator

**Goal**: Reduce boilerplate with automated proxy generation

🆕 New infrastructure:

- `LablabBean.SourceGenerators.Proxy` project
- `[RealizeService]` attribute
- `[SelectionStrategy]` attribute
- Roslyn source generator implementation

**Priority**: 🟠 **P2**

**Benefits**:

- Eliminates manual proxy delegation code
- Ensures consistency across all services
- Reduces bugs from manual delegation
- Improves developer experience

**Alternative**: Can use manual proxy services initially, add generator later

---

### Phase 4: Cross-Cutting Contracts

**Goal**: Analytics, diagnostics, audio

🆕 Optional enhancements:

- `LablabBean.Contracts.Analytics` - Player metrics
- `LablabBean.Contracts.Diagnostics` - Error tracking
- `LablabBean.Contracts.Audio` - Sound effects and music

**Priority**: 🟡 **P3**

---

### Phase 5: Advanced Contracts (Future)

**Goal**: Advanced features as needed

🆕 Optional future work:

- `LablabBean.Contracts.Resilience`
- `LablabBean.Contracts.Recorder`
- `LablabBean.Contracts.Capability`
- `LablabBean.Contracts.Goap`
- `LablabBean.Contracts.Hosting`
- `LablabBean.Contracts.Terminal`

**Priority**: 🟢 **P4 - Add when needed**

---

## Impact on Spec 007

### Current Spec Scope

The current specification (`007-tiered-contract-architecture`) focuses on:

- ✅ IEventBus foundation
- ✅ LablabBean.Contracts.Game (already exists, add events)
- ✅ LablabBean.Contracts.UI (already exists, add events)
- ❌ Missing: Scene, Input, Config, Resource contracts
- ❌ Missing: Source generator

### Recommended Spec Updates

**Option A: Expand Spec 007 (Comprehensive)**

- Include Phase 1 + Phase 2 + Phase 3
- Add Scene, Input, Config, Resource contracts
- Add source generator implementation
- **Pro**: One comprehensive spec, complete architecture
- **Con**: Larger scope, longer implementation time

**Option B: Keep Spec 007 Focused (Incremental)**

- Keep current scope (IEventBus + Game/UI events only)
- Create separate specs for:
  - Spec 008: Core Game Contracts (Scene, Input, Config, Resource)
  - Spec 009: Source Generator for Proxy Services
- **Pro**: Smaller, manageable chunks, faster iterations
- **Con**: Multiple specs to coordinate

**Recommendation**: **Option A** - Expand Spec 007 to include Phase 1 + Phase 2, defer Phase 3 (source generator) to Spec 008

**Rationale**:

- Scene, Input, Config, Resource are all essential for a functional dungeon crawler
- Without these contracts, the event bus has limited value (fewer events to publish)
- Source generator is valuable but can be added later without blocking development
- Comprehensive contract foundation enables parallel plugin development

---

## Summary Table: What to Add

| Component | Current Spec | Recommendation | Priority |
|-----------|-------------|----------------|----------|
| IEventBus | ✅ Included | Keep | P0 |
| EventBus Implementation | ✅ Included | Keep | P0 |
| Game Contract Events | ✅ Included | Keep | P0 |
| UI Contract Events | ✅ Included | Keep | P0 |
| **Scene Contract** | ❌ Missing | **Add to Spec 007** | **P1** |
| **Input Contract** | ❌ Missing | **Add to Spec 007** | **P1** |
| **Config Contract** | ❌ Missing | **Add to Spec 007** | **P1** |
| **Resource Contract** | ❌ Missing | **Add to Spec 007** | **P1** |
| Audio Contract | ❌ Missing | Defer to future spec | P3 |
| Analytics Contract | ❌ Missing | Defer to future spec | P3 |
| Diagnostics Contract | ❌ Missing | Defer to future spec | P3 |
| **Source Generator** | ❌ Missing | **Defer to Spec 008** | **P2** |
| Resilience/Recorder/etc. | ❌ Missing | Defer to future specs | P4 |

---

## Next Steps

1. **Review this gap analysis** with stakeholders
2. **Decide on spec scope**:
   - Option A: Expand Spec 007 to include Scene, Input, Config, Resource
   - Option B: Keep Spec 007 minimal, create follow-up specs
3. **Update Spec 007** based on decision
4. **Run `/speckit.plan`** to create implementation plan
5. **Run `/speckit.tasks`** to generate task list

---

## References

### Cross-Milo Contract Files

- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.Scene/`
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.Input/`
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.Config/`
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.Audio/`
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.Analytics/`
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.Resource/`
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.SourceGenerators.Proxy/`

### Lablab-Bean Current Files

- `dotnet/framework/LablabBean.Contracts.Game/` (exists)
- `dotnet/framework/LablabBean.Contracts.UI/` (exists)
- `dotnet/framework/LablabBean.Plugins.Contracts/IRegistry.cs` (exists)
- `dotnet/framework/LablabBean.Plugins.Core/ServiceRegistry.cs` (exists)

---

**Author**: Claude Code
**Date**: 2025-10-21
**Version**: 1.0
**Related Spec**: [007-tiered-contract-architecture](../specs/007-tiered-contract-architecture/spec.md)
