---
title: Cross-Milo Contract Design Adoption Analysis
status: draft
type: analysis
created: 2025-10-21
tags: [architecture, plugins, contracts, tier-design]
---

# Cross-Milo Contract Design Adoption for Lablab-Bean

## Executive Summary

This document analyzes the cross-milo project's tier 1 and tier 2 contract design patterns and provides recommendations for adopting these patterns in lablab-bean. The cross-milo architecture provides a proven model for clean separation between contracts (what to do), infrastructure (how to do it), and implementations (platform-specific realizations).

## Cross-Milo Architecture Overview

### Three-Tier Design Pattern

```
TIER 1: cross-milo (Contracts)
├── Purpose: Define service interfaces and event contracts
├── Contains: Service interfaces, event definitions, data models
├── Dependencies: Minimal (.NET only)
└── Examples: IRegistry, IEventBus, IService interfaces

TIER 2: plugin-manoi (Infrastructure)
├── Purpose: Plugin loading and event routing infrastructure
├── Contains: Registry implementation, EventBus, source generators
├── Dependencies: Tier 1 contracts
└── Examples: ServiceRegistry, EventBus implementation, plugin lifecycle

TIER 3+: winged-bean plugins (Implementations)
├── Purpose: Implement services in platform-specific ways
├── Contains: Concrete service implementations
├── Dependencies: Tier 1 contracts, Tier 2 infrastructure
└── Examples: NAudioService (Console), UnityAudioService (Unity)
```

## Key Design Patterns from Cross-Milo

### 1. Foundation Contracts (Tier 1 Base)

**Location**: `CrossMilo.Contracts/`

**Core Interfaces**:
- `IRegistry` - Service registration and resolution
- `IEventBus` - Publish-subscribe event communication
- `ServiceMetadata` - Service metadata (priority, name, version)
- Attributes - `[RealizeService]`, `[SelectionStrategy]`

**Key Features**:
```csharp
// IRegistry.cs
public interface IRegistry
{
    void Register<TService>(TService implementation, int priority = 0) where TService : class;
    void Register<TService>(TService implementation, ServiceMetadata metadata) where TService : class;
    TService Get<TService>(SelectionMode mode = SelectionMode.HighestPriority) where TService : class;
    IEnumerable<TService> GetAll<TService>() where TService : class;
    bool IsRegistered<TService>() where TService : class;
    bool Unregister<TService>(TService implementation) where TService : class;
    void UnregisterAll<TService>() where TService : class;
    ServiceMetadata? GetMetadata<TService>(TService implementation) where TService : class;
}

// IEventBus.cs
public interface IEventBus
{
    Task PublishAsync<T>(T eventData) where T : class;
    void Subscribe<T>(Func<T, Task> handler) where T : class;
}
```

### 2. Domain Contract Pattern (Tier 1 Services)

**Organizational Structure**:
```
CrossMilo.Contracts.{DomainName}/
├── {SupportingTypes}.cs          # Root namespace
└── Services/
    ├── Interfaces/
    │   └── IService.cs           # Generic IService within domain
    └── Service.cs                 # ProxyService implementation
```

**Example: Audio Service**

```csharp
// CrossMilo.Contracts.Audio/Services/Interfaces/IService.cs
namespace Plate.CrossMilo.Contracts.Audio.Services;

public interface IService
{
    void Play(string clipId, AudioPlayOptions? options = null);
    void Stop(string clipId);
    void StopAll();
    void Pause(string clipId);
    void Resume(string clipId);
    float Volume { get; set; }
    bool IsPlaying(string clipId);
    Task<bool> LoadAsync(string clipId, CancellationToken cancellationToken = default);
    void Unload(string clipId);
}

// CrossMilo.Contracts.Audio/Services/Service.cs
[RealizeService(typeof(IService))]
[SelectionStrategy(SelectionMode.HighestPriority)]
public partial class ProxyService : IService
{
    private readonly IRegistry _registry;

    public ProxyService(IRegistry registry)
    {
        _registry = registry;
    }

    // Source generator implements all interface methods
}
```

**Benefits of Generic IService Naming**:
- ✅ Consistent structure across all 17 contract projects
- ✅ Simplified naming within namespace context
- ✅ Better scalability (easy to add multiple services per domain)
- ✅ Namespace clarity: `Plate.CrossMilo.Contracts.Audio.Services.IService`

### 3. Event-Driven Communication

**Event Naming Convention**: `{Subject}{Action}Event`

**Event Pattern**:
```csharp
public record EntitySpawnedEvent(
    Guid EntityId,
    EntityType Type,
    Position Position,
    DateTimeOffset Timestamp
)
{
    // Convenience constructor without timestamp
    public EntitySpawnedEvent(Guid entityId, EntityType type, Position position)
        : this(entityId, type, position, DateTimeOffset.UtcNow)
    {
    }
}
```

**Usage Pattern**:
```csharp
// Publisher (in game service)
await _eventBus.PublishAsync(new EntitySpawnedEvent(entityId, EntityType.Enemy, position));

// Subscriber (in analytics plugin)
eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
{
    await _analyticsService.Track("EntitySpawned", new { evt.EntityId, evt.Type });
});
```

### 4. Source Generator Pattern (Tier 1 → Tier 2 Bridge)

**Purpose**: Automatically generate proxy service implementations that delegate to `IRegistry`

**How It Works**:
1. Developer marks partial class with `[RealizeService(typeof(IService))]`
2. Developer specifies `[SelectionStrategy(SelectionMode.HighestPriority)]`
3. Source generator creates implementation:
   ```csharp
   public partial class ProxyService : IService
   {
       public void Play(string clipId, AudioPlayOptions? options = null)
       {
           var implementation = _registry.Get<IService>(SelectionMode.HighestPriority);
           implementation.Play(clipId, options);
       }
       // ... similar for all methods
   }
   ```

**Benefits**:
- ✅ Automated delegation to registry
- ✅ No manual delegation code needed
- ✅ Consistent behavior across all services
- ✅ Supports multiple implementations per interface

### 5. Selection Modes

```csharp
public enum SelectionMode
{
    HighestPriority,  // Get implementation with highest priority
    One,              // Expect exactly one implementation
    All               // Get all implementations (use GetAll instead)
}
```

## Current Lablab-Bean Architecture

### Existing Plugin System

**Location**: `dotnet/framework/LablabBean.Plugins.Contracts/`

**Current Interfaces**:
```csharp
// IRegistry.cs (GOOD: Already aligned!)
public interface IRegistry
{
    void Register<TService>(TService implementation, ServiceMetadata metadata) where TService : class;
    void Register<TService>(TService implementation, int priority = 100) where TService : class;
    TService Get<TService>(SelectionMode mode = SelectionMode.HighestPriority) where TService : class;
    IEnumerable<TService> GetAll<TService>() where TService : class;
    bool IsRegistered<TService>() where TService : class;
    bool Unregister<TService>(TService implementation) where TService : class;
}

// IPlugin.cs
public interface IPlugin
{
    string Id { get; }
    string Name { get; }
    string Version { get; }
    Task InitializeAsync(IPluginContext context, CancellationToken ct = default);
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}
```

**Status**:
- ✅ `IRegistry` interface is already nearly identical to cross-milo!
- ✅ `ServiceMetadata` pattern already implemented
- ✅ `SelectionMode` pattern already implemented
- ❌ Missing: `IEventBus` interface
- ❌ Missing: Domain-specific contract projects (Audio, Config, Scene, etc.)
- ❌ Missing: Proxy service pattern with source generators
- ❌ Missing: Event-driven communication patterns

## Adoption Roadmap for Lablab-Bean

### Phase 1: Add IEventBus Foundation (Tier 1)

**Goal**: Add event bus contract to `LablabBean.Plugins.Contracts`

**Tasks**:
1. Create `IEventBus.cs` in `LablabBean.Plugins.Contracts/`
2. Implement `EventBus` in `LablabBean.Plugins.Core/`
3. Register EventBus in plugin host bootstrap
4. Add comprehensive XML documentation

**Implementation**:
```csharp
// LablabBean.Plugins.Contracts/IEventBus.cs
namespace LablabBean.Plugins.Contracts;

/// <summary>
/// Event bus for inter-plugin communication using publish-subscribe pattern.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publish an event to all subscribers.
    /// </summary>
    Task PublishAsync<T>(T eventData) where T : class;

    /// <summary>
    /// Subscribe to events of a specific type.
    /// </summary>
    void Subscribe<T>(Func<T, Task> handler) where T : class;
}
```

### Phase 2: Create Domain Contract Projects (Tier 1)

**Goal**: Create domain-specific contract assemblies following cross-milo pattern

**Recommended Domains for Lablab-Bean**:
1. `LablabBean.Contracts.Game` - Game loop, entity management, turn processing
2. `LablabBean.Contracts.UI` - UI rendering, input handling, viewport management
3. `LablabBean.Contracts.Scene` - Scene/level management, camera, viewport
4. `LablabBean.Contracts.Config` - Configuration management
5. `LablabBean.Contracts.Analytics` - (Optional) Analytics tracking
6. `LablabBean.Contracts.Diagnostics` - (Optional) Error tracking, logging

**Project Structure**:
```
LablabBean.Contracts.Game/
├── LablabBean.Contracts.Game.csproj
├── GameEvents.cs                    # Event definitions
├── GameModels.cs                    # Supporting types (EntitySnapshot, etc.)
└── Services/
    ├── Interfaces/
    │   └── IService.cs              # Game service interface
    └── Service.cs                    # ProxyService (if using source generator)
```

**Example: Game Service Contract**:
```csharp
// LablabBean.Contracts.Game/Services/Interfaces/IService.cs
namespace LablabBean.Contracts.Game.Services;

/// <summary>
/// Core game service for dungeon crawler mechanics.
/// </summary>
public interface IService
{
    // Game loop
    Task StartGameAsync(CancellationToken ct = default);
    Task ProcessTurnAsync(CancellationToken ct = default);
    void PauseGame();
    void ResumeGame();

    // Entity management
    Task<Guid> SpawnEntityAsync(EntityType type, Position position);
    Task MoveEntityAsync(Guid entityId, Direction direction);
    Task<bool> AttackAsync(Guid attackerId, Guid defenderId);

    // Game state
    GameState GetState();
    IReadOnlyList<EntitySnapshot> GetEntities();

    // Events
    event EventHandler<GameStateChangedEventArgs>? StateChanged;
}

// LablabBean.Contracts.Game/GameEvents.cs
namespace LablabBean.Contracts.Game;

public record EntitySpawnedEvent(
    Guid EntityId,
    EntityType Type,
    Position Position,
    DateTimeOffset Timestamp
)
{
    public EntitySpawnedEvent(Guid entityId, EntityType type, Position position)
        : this(entityId, type, position, DateTimeOffset.UtcNow) { }
}

public record EntityMovedEvent(
    Guid EntityId,
    Position OldPosition,
    Position NewPosition,
    DateTimeOffset Timestamp
)
{
    public EntityMovedEvent(Guid entityId, Position oldPos, Position newPos)
        : this(entityId, oldPos, newPos, DateTimeOffset.UtcNow) { }
}

public record CombatEvent(
    Guid AttackerId,
    Guid DefenderId,
    int Damage,
    bool DefenderKilled,
    DateTimeOffset Timestamp
)
{
    public CombatEvent(Guid attackerId, Guid defenderId, int damage, bool killed)
        : this(attackerId, defenderId, damage, killed, DateTimeOffset.UtcNow) { }
}
```

### Phase 3: Implement Source Generator (Optional, Tier 2)

**Goal**: Automate proxy service generation (following cross-milo pattern)

**Benefits**:
- Eliminates manual delegation code
- Ensures consistency across all services
- Easier to maintain and extend

**Tasks**:
1. Create `LablabBean.SourceGenerators.Proxy` project
2. Implement Roslyn source generator
3. Add `[RealizeService]` and `[SelectionStrategy]` attributes
4. Generate proxy implementations at compile time

**Alternative (Simpler)**:
- Manual proxy services (no source generator)
- Less automation but simpler to implement initially

### Phase 4: Migrate Existing Services to Contract Pattern

**Goal**: Refactor existing services to use domain contracts

**Current Services to Migrate**:
1. Game loop logic → `LablabBean.Contracts.Game.Services.IService`
2. UI rendering → `LablabBean.Contracts.UI.Services.IService`
3. Input handling → `LablabBean.Contracts.UI.Input.IService`

**Migration Pattern**:
```csharp
// Before: Direct implementation in plugin
public class GamePlugin : IPlugin
{
    public Task InitializeAsync(IPluginContext context, CancellationToken ct)
    {
        // Game logic directly in plugin
    }
}

// After: Service contract + implementation
// 1. Define contract in LablabBean.Contracts.Game
public interface IService
{
    Task ProcessTurnAsync();
}

// 2. Implement in plugin
public class DungeonGameService : IService
{
    private readonly IEventBus _eventBus;

    public DungeonGameService(IRegistry registry)
    {
        _eventBus = registry.Get<IEventBus>();
    }

    public async Task ProcessTurnAsync()
    {
        // ... game logic ...
        await _eventBus.PublishAsync(new TurnProcessedEvent());
    }
}

// 3. Register in plugin
public class GamePlugin : IPlugin
{
    public Task InitializeAsync(IPluginContext context, CancellationToken ct)
    {
        var service = new DungeonGameService(context.Registry);
        context.Registry.Register<IService>(service, priority: 200);
        return Task.CompletedTask;
    }
}
```

### Phase 5: Add Event-Driven Communication

**Goal**: Enable loose coupling between plugins via events

**Use Cases**:
1. **Analytics Plugin** - Subscribe to game events, track metrics
2. **Diagnostics Plugin** - Subscribe to error events, log issues
3. **UI Plugin** - Subscribe to game state changes, update display

**Example**:
```csharp
// In GamePlugin (publisher)
public class DungeonGameService : IService
{
    private readonly IEventBus _eventBus;

    public async Task SpawnEnemyAsync(Guid id, Position pos)
    {
        // ... spawn enemy ...
        await _eventBus.PublishAsync(new EntitySpawnedEvent(id, EntityType.Enemy, pos));
    }
}

// In AnalyticsPlugin (subscriber)
public class AnalyticsPlugin : IPlugin
{
    public Task InitializeAsync(IPluginContext context, CancellationToken ct)
    {
        var eventBus = context.Registry.Get<IEventBus>();

        eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
        {
            // Track entity spawn in analytics
            Console.WriteLine($"Entity spawned: {evt.EntityId} at {evt.Position}");
        });

        return Task.CompletedTask;
    }
}
```

## Comparison: Cross-Milo vs Lablab-Bean

| Aspect | Cross-Milo | Lablab-Bean (Current) | Recommendation |
|--------|------------|----------------------|----------------|
| **Tier 1 Base** | IRegistry, IEventBus | IRegistry only | ✅ Add IEventBus |
| **Domain Contracts** | 17 contract assemblies | None (monolithic) | ✅ Create Game, UI, Scene contracts |
| **Event System** | EventBus with typed events | None | ✅ Implement event-driven pattern |
| **Service Selection** | Priority-based, SelectionMode | ✅ Already implemented | ✅ Keep current approach |
| **Proxy Services** | Source generator | None | ⚠️ Optional (consider manual first) |
| **Namespace Pattern** | `Plate.CrossMilo.Contracts.{Domain}` | `LablabBean.Plugins.Contracts` | ✅ Adopt domain namespaces |
| **IService Naming** | Generic `IService` per domain | Specific names | ✅ Consider generic naming |

## Architecture Principles to Adopt

### 1. Three-Tier Layer Definition

```
TIER 1: LablabBean.Contracts.* (Contracts)
└─ Purpose: Define service interfaces and event contracts
└─ Contains: Event definitions, service interfaces, data models
└─ Dependencies: Minimal (.NET only)

TIER 2: LablabBean.Plugins.* (Infrastructure)
└─ Purpose: Plugin loading and event routing
└─ Contains: IEventBus, EventBus implementation, plugin lifecycle, IRegistry
└─ Dependencies: Tier 1 contracts

TIER 3+: LablabBean.Game.*, LablabBean.UI.* (Implementations)
└─ Purpose: Implement services in platform-specific ways
└─ Contains: Concrete implementations as plugins
└─ Dependencies: Tier 1 contracts, Tier 2 infrastructure
```

### 2. Event-Driven Communication

**When to Use Events**:
- ✅ Analytics tracking
- ✅ Diagnostics logging
- ✅ State synchronization across plugins
- ✅ Notification of state changes

**When NOT to Use Events**:
- ❌ Direct service calls (use `IRegistry.Get<T>()`)
- ❌ Request/response patterns (use direct method calls)
- ❌ Tightly-coupled operations within same plugin

### 3. Runtime vs Edit-Time Mode Support

All contracts should support both:
- **Runtime Mode**: Application running for end-users
- **Edit-Time Mode**: Application being authored/configured

Different implementations can exist for each mode using same contracts.

### 4. Host ≠ UI Framework

Separate concerns:
- **Host**: Application runtime (Console, Unity, Godot)
- **UI Framework**: Rendering library (Terminal.Gui, ImGui, SadConsole)
- **Game Logic**: Platform-agnostic contracts

## Implementation Priority

### High Priority (Phase 1-2)
1. ✅ **Add IEventBus to LablabBean.Plugins.Contracts**
   - Essential for event-driven architecture
   - Enables loose coupling
   - Low complexity, high value

2. ✅ **Create LablabBean.Contracts.Game**
   - Core domain for dungeon crawler
   - Contains most critical service contracts
   - Foundation for all other contracts

3. ✅ **Create LablabBean.Contracts.UI**
   - UI rendering contracts
   - Input handling contracts
   - Viewport management

### Medium Priority (Phase 3-4)
4. **Create LablabBean.Contracts.Scene**
   - Level/dungeon management
   - Camera and viewport
   - Scene transitions

5. **Migrate existing services to contracts**
   - Refactor game loop
   - Refactor UI rendering
   - Refactor input handling

### Low Priority (Phase 5)
6. **Consider source generator** (optional)
   - Automates proxy services
   - Reduces boilerplate
   - Can be added later if needed

7. **Add LablabBean.Contracts.Analytics** (optional)
   - Analytics tracking
   - User metrics

8. **Add LablabBean.Contracts.Diagnostics** (optional)
   - Error tracking
   - Performance monitoring

## Migration Strategy

### Step 1: Add IEventBus (No Breaking Changes)
```csharp
// 1. Add to LablabBean.Plugins.Contracts/IEventBus.cs
// 2. Implement in LablabBean.Plugins.Core/EventBus.cs
// 3. Register in PluginHost bootstrap
```

### Step 2: Create First Contract Assembly
```bash
# Create new project
dotnet new classlib -n LablabBean.Contracts.Game -f net8.0

# Add to solution
dotnet sln add dotnet/framework/LablabBean.Contracts.Game

# Add reference to contracts
cd dotnet/framework/LablabBean.Contracts.Game
dotnet add reference ../LablabBean.Plugins.Contracts
```

### Step 3: Gradually Migrate Services
- Start with one service at a time
- Keep existing implementations working
- Use side-by-side migration (old + new)
- Remove old implementations when safe

### Step 4: Update Documentation
- Document contract patterns
- Update plugin development guide
- Add event-driven examples
- Update CLAUDE.md with new patterns

## Benefits of Adoption

### For Developers
- ✅ **Clear contracts** - Well-defined interfaces
- ✅ **Loose coupling** - Plugins don't depend on each other
- ✅ **Event-driven** - React to state changes without polling
- ✅ **Testable** - Easy to mock services and events

### For Architecture
- ✅ **Separation of concerns** - Contracts vs infrastructure vs implementation
- ✅ **Platform independence** - Same contracts work on Console, Unity, Godot
- ✅ **Extensibility** - Easy to add new services and events
- ✅ **Maintainability** - Changes to implementations don't affect contracts

### For Cross-Platform Support
- ✅ **Console app** - Uses Terminal.Gui implementation
- ✅ **Windows app** - Uses SadConsole implementation
- ✅ **Unity (future)** - Uses Unity-specific implementations
- ✅ **All use same contracts** - `LablabBean.Contracts.*`

## Risks and Mitigations

| Risk | Mitigation |
|------|-----------|
| Breaking existing plugins | Side-by-side migration, version contracts |
| Over-engineering | Start minimal (IEventBus + Game contracts only) |
| Source generator complexity | Skip source generators initially, add later if needed |
| Event bus performance | Use async handlers, consider batching if needed |
| Documentation drift | Update docs in same PR as contract changes |

## File References

### Cross-Milo Reference Files
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts/IRegistry.cs`
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts/IEventBus.cs`
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.Audio/Services/Interfaces/IService.cs`
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.Audio/Services/Service.cs`
- `ref-projects/cross-milo/docs/EVENT-BUS-INTEGRATION.md`
- `ref-projects/cross-milo/docs/ARCHITECTURE_HOST_VS_UI.md`

### Lablab-Bean Current Files
- `dotnet/framework/LablabBean.Plugins.Contracts/IRegistry.cs`
- `dotnet/framework/LablabBean.Plugins.Contracts/IPlugin.cs`
- `dotnet/framework/LablabBean.Plugins.Core/ServiceRegistry.cs`

## Next Steps

1. **Review this analysis** with team/stakeholders
2. **Decide on adoption scope**:
   - Full adoption (all phases)
   - Minimal adoption (IEventBus + Game contracts only)
   - Phased approach (start small, expand later)
3. **Create implementation tasks** in todo list
4. **Start with Phase 1**: Add IEventBus foundation

## Conclusion

The cross-milo tier 1 and tier 2 contract design provides a proven architecture for:
- Clean separation of concerns
- Event-driven communication
- Platform independence
- Extensible plugin system

Lablab-bean's existing `IRegistry` implementation is already well-aligned with cross-milo patterns. Adding `IEventBus` and domain-specific contract assemblies will complete the foundation for a robust, cross-platform dungeon crawler architecture.

**Recommendation**: Start with **Phase 1** (IEventBus) and **Phase 2** (Game contracts) to gain immediate benefits with minimal disruption to existing code.

---

**Author**: Claude Code
**Date**: 2025-10-21
**Version**: 1.0
**Status**: Draft for review
