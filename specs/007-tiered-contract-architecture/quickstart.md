# Quickstart: Event-Driven Plugin Development

**Feature**: 007-tiered-contract-architecture
**Audience**: Plugin developers
**Goal**: Create your first event-driven plugin in under 30 minutes

## Overview

This guide walks you through creating an analytics plugin that tracks game events without directly depending on the game plugin. You'll learn:

1. How to subscribe to events via `IEventBus`
2. How to implement service contracts
3. How to register services with `IRegistry`
4. Best practices for event-driven plugin architecture

## Prerequisites

- .NET 8 SDK installed
- Basic understanding of C# async/await
- Familiarity with the lablab-bean plugin system

## Example 1: Analytics Plugin (Event Subscriber)

### Step 1: Create Plugin Project

```bash
cd plugins/
dotnet new classlib -n LablabBean.Plugins.Analytics -f net8.0
cd LablabBean.Plugins.Analytics
```

### Step 2: Add References

```xml
<!-- LablabBean.Plugins.Analytics.csproj -->
<ItemGroup>
  <ProjectReference Include="..\..\dotnet\framework\LablabBean.Plugins.Contracts\LablabBean.Plugins.Contracts.csproj" />
  <ProjectReference Include="..\..\dotnet\framework\LablabBean.Contracts.Game\LablabBean.Contracts.Game.csproj" />
</ItemGroup>
```

**Note**: Reference only the **contracts** assemblies, not the implementations. This ensures loose coupling.

### Step 3: Implement Plugin

```csharp
using LablabBean.Contracts.Game.Events;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Analytics;

public class AnalyticsPlugin : IPlugin
{
    private ILogger? _logger;
    private IEventBus? _eventBus;
    private int _entitySpawnCount;
    private int _combatEventCount;

    public string Name => "Analytics Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context)
    {
        _logger = context.Logger;
        _eventBus = context.Registry.Get<IEventBus>();

        // Subscribe to game events
        _eventBus.Subscribe<EntitySpawnedEvent>(OnEntitySpawned);
        _eventBus.Subscribe<CombatEvent>(OnCombat);
        _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

        _logger.LogInformation("Analytics plugin initialized");
        return Task.CompletedTask;
    }

    private Task OnEntitySpawned(EntitySpawnedEvent evt)
    {
        _entitySpawnCount++;
        _logger?.LogInformation(
            "Entity spawned: {Type} at ({X}, {Y}). Total spawns: {Count}",
            evt.EntityType, evt.Position.X, evt.Position.Y, _entitySpawnCount);
        return Task.CompletedTask;
    }

    private Task OnCombat(CombatEvent evt)
    {
        _combatEventCount++;
        _logger?.LogInformation(
            "Combat: {Attacker} → {Target}, Damage: {Damage}, Hit: {Hit}, Kill: {Kill}. Total combats: {Count}",
            evt.AttackerId, evt.TargetId, evt.DamageDealt, evt.IsHit, evt.IsKill, _combatEventCount);
        return Task.CompletedTask;
    }

    private Task OnGameStateChanged(GameStateChangedEvent evt)
    {
        _logger?.LogInformation(
            "Game state changed: {Old} → {New}. Reason: {Reason}",
            evt.OldState, evt.NewState, evt.Reason ?? "N/A");
        return Task.CompletedTask;
    }

    public Task ShutdownAsync()
    {
        _logger?.LogInformation(
            "Analytics summary - Spawns: {Spawns}, Combats: {Combats}",
            _entitySpawnCount, _combatEventCount);
        return Task.CompletedTask;
    }
}
```

### Step 4: Register Plugin

Add to `plugins/plugin-manifest.json`:

```json
{
  "plugins": [
    {
      "name": "Analytics",
      "assembly": "LablabBean.Plugins.Analytics.dll",
      "type": "LablabBean.Plugins.Analytics.AnalyticsPlugin",
      "enabled": true
    }
  ]
}
```

### Step 5: Build and Run

```bash
dotnet build
cd ../../
dotnet run --project dotnet/console/LablabBean.Console
```

**Expected Output**:

```
[Analytics Plugin] Entity spawned: player at (5, 5). Total spawns: 1
[Analytics Plugin] Entity spawned: goblin at (10, 8). Total spawns: 2
[Analytics Plugin] Combat: <player-id> → <goblin-id>, Damage: 5, Hit: True, Kill: False. Total combats: 1
```

---

## Example 2: Custom Game Service (Service Provider)

### Step 1: Create Plugin Project

```bash
cd plugins/
dotnet new classlib -n LablabBean.Plugins.CustomGame -f net8.0
cd LablabBean.Plugins.CustomGame
```

### Step 2: Add References

```xml
<ItemGroup>
  <ProjectReference Include="..\..\dotnet\framework\LablabBean.Plugins.Contracts\LablabBean.Plugins.Contracts.csproj" />
  <ProjectReference Include="..\..\dotnet\framework\LablabBean.Contracts.Game\LablabBean.Contracts.Game.csproj" />
</ItemGroup>
```

### Step 3: Implement Game Service

```csharp
using LablabBean.Contracts.Game.Events;
using LablabBean.Contracts.Game.Models;
using LablabBean.Contracts.Game.Services;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.CustomGame;

public class CustomGameService : IService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly Dictionary<Guid, EntitySnapshot> _entities = new();
    private GameState _gameState;

    public CustomGameService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus;
        _logger = logger;
        _gameState = new GameState(
            GameStateType.NotStarted,
            TurnNumber: 0,
            PlayerEntityId: null,
            CurrentLevel: 1,
            StartTime: DateTimeOffset.UtcNow
        );
    }

    public async Task StartGameAsync(GameStartOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting game with difficulty: {Difficulty}", options.Difficulty);

        var oldState = _gameState.State;
        _gameState = _gameState with { State = GameStateType.Running };

        await _eventBus.PublishAsync(new GameStateChangedEvent(oldState, GameStateType.Running, "Game started"));
    }

    public async Task<Guid> SpawnEntityAsync(string entityType, Position position)
    {
        var entityId = Guid.NewGuid();
        var entity = new EntitySnapshot(
            Id: entityId,
            Type: entityType,
            Position: position,
            Health: 100,
            MaxHealth: 100,
            Properties: new Dictionary<string, object>()
        );

        _entities[entityId] = entity;

        await _eventBus.PublishAsync(new EntitySpawnedEvent(entityId, entityType, position));

        _logger.LogInformation("Spawned {Type} at ({X}, {Y})", entityType, position.X, position.Y);
        return entityId;
    }

    public async Task<bool> MoveEntityAsync(Guid entityId, Position newPosition)
    {
        if (!_entities.TryGetValue(entityId, out var entity))
            return false;

        var oldPosition = entity.Position;
        _entities[entityId] = entity with { Position = newPosition };

        await _eventBus.PublishAsync(new EntityMovedEvent(entityId, oldPosition, newPosition));

        return true;
    }

    public async Task<CombatResult> AttackAsync(Guid attackerId, Guid targetId)
    {
        if (!_entities.TryGetValue(attackerId, out var attacker))
            throw new InvalidOperationException($"Attacker {attackerId} not found");

        if (!_entities.TryGetValue(targetId, out var target))
            throw new InvalidOperationException($"Target {targetId} not found");

        // Simple combat logic
        var damage = Random.Shared.Next(5, 15);
        var isHit = Random.Shared.Next(100) < 80; // 80% hit chance
        var actualDamage = isHit ? damage : 0;

        var newHealth = Math.Max(0, target.Health - actualDamage);
        var isKill = newHealth == 0;

        _entities[targetId] = target with { Health = newHealth };

        await _eventBus.PublishAsync(new CombatEvent(attackerId, targetId, actualDamage, isHit, isKill));

        return new CombatResult(actualDamage, isHit, isKill, SpecialEffect: null);
    }

    public Task ProcessTurnAsync(CancellationToken cancellationToken = default)
    {
        _gameState = _gameState with { TurnNumber = _gameState.TurnNumber + 1 };
        return Task.CompletedTask;
    }

    public GameState GetGameState() => _gameState;

    public IReadOnlyCollection<EntitySnapshot> GetEntities() => _entities.Values.ToList();
}
```

### Step 4: Register Service in Plugin

```csharp
public class CustomGamePlugin : IPlugin
{
    public string Name => "Custom Game";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context)
    {
        var eventBus = context.Registry.Get<IEventBus>();
        var gameService = new CustomGameService(eventBus, context.Logger);

        // Register with priority 200 (higher than default 100)
        context.Registry.Register<LablabBean.Contracts.Game.Services.IService>(
            gameService,
            new ServiceMetadata
            {
                Priority = 200,
                Name = "CustomGameService",
                Version = "1.0.0"
            }
        );

        context.Logger.LogInformation("Custom game service registered");
        return Task.CompletedTask;
    }

    public Task ShutdownAsync() => Task.CompletedTask;
}
```

---

## Example 3: Reactive UI Plugin (Event Subscriber + Service Provider)

### Step 1: Create Plugin Project

```bash
cd plugins/
dotnet new classlib -n LablabBean.Plugins.ReactiveUI -f net8.0
cd LablabBean.Plugins.ReactiveUI
```

### Step 2: Add References

```xml
<ItemGroup>
  <ProjectReference Include="..\..\dotnet\framework\LablabBean.Plugins.Contracts\LablabBean.Plugins.Contracts.csproj" />
  <ProjectReference Include="..\..\dotnet\framework\LablabBean.Contracts.Game\LablabBean.Contracts.Game.csproj" />
  <ProjectReference Include="..\..\dotnet\framework\LablabBean.Contracts.UI\LablabBean.Contracts.UI.csproj" />
</ItemGroup>
```

### Step 3: Implement Reactive UI Service

```csharp
using LablabBean.Contracts.Game.Events;
using LablabBean.Contracts.Game.Models;
using LablabBean.Contracts.UI.Models;
using LablabBean.Contracts.UI.Services;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.ReactiveUI;

public class ReactiveUIService : IService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private ViewportBounds _viewport;
    private bool _needsRedraw;

    public ReactiveUIService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus;
        _logger = logger;
        _viewport = new ViewportBounds(new Position(0, 0), Width: 80, Height: 24);
        _needsRedraw = false;
    }

    public Task InitializeAsync(UIInitOptions options, CancellationToken cancellationToken = default)
    {
        _viewport = new ViewportBounds(
            new Position(0, 0),
            options.ViewportWidth,
            options.ViewportHeight
        );

        // Subscribe to game events for reactive updates
        _eventBus.Subscribe<EntityMovedEvent>(OnEntityMoved);
        _eventBus.Subscribe<EntitySpawnedEvent>(OnEntitySpawned);
        _eventBus.Subscribe<CombatEvent>(OnCombat);

        _logger.LogInformation("Reactive UI initialized with viewport {W}x{H}",
            options.ViewportWidth, options.ViewportHeight);

        return Task.CompletedTask;
    }

    private Task OnEntityMoved(EntityMovedEvent evt)
    {
        _logger.LogDebug("Entity moved: {Id} from ({X1},{Y1}) to ({X2},{Y2})",
            evt.EntityId, evt.OldPosition.X, evt.OldPosition.Y,
            evt.NewPosition.X, evt.NewPosition.Y);
        _needsRedraw = true;
        return Task.CompletedTask;
    }

    private Task OnEntitySpawned(EntitySpawnedEvent evt)
    {
        _logger.LogDebug("Entity spawned: {Type} at ({X},{Y})",
            evt.EntityType, evt.Position.X, evt.Position.Y);
        _needsRedraw = true;
        return Task.CompletedTask;
    }

    private Task OnCombat(CombatEvent evt)
    {
        _logger.LogInformation("Combat animation: {Attacker} → {Target}, Damage: {Damage}",
            evt.AttackerId, evt.TargetId, evt.DamageDealt);
        _needsRedraw = true;
        return Task.CompletedTask;
    }

    public Task RenderViewportAsync(ViewportBounds viewport, IReadOnlyCollection<EntitySnapshot> entities)
    {
        // Render logic here (platform-specific)
        _logger.LogDebug("Rendering {Count} entities in viewport", entities.Count);
        return Task.CompletedTask;
    }

    public Task UpdateDisplayAsync()
    {
        if (_needsRedraw)
        {
            _logger.LogDebug("Updating display");
            _needsRedraw = false;
        }
        return Task.CompletedTask;
    }

    public Task HandleInputAsync(InputCommand command)
    {
        _eventBus.PublishAsync(new LablabBean.Contracts.UI.Events.InputReceivedEvent(command));
        return Task.CompletedTask;
    }

    public ViewportBounds GetViewport() => _viewport;

    public void SetViewportCenter(Position centerPosition)
    {
        var oldViewport = _viewport;
        var newTopLeft = new Position(
            centerPosition.X - _viewport.Width / 2,
            centerPosition.Y - _viewport.Height / 2
        );
        _viewport = _viewport with { TopLeft = newTopLeft };

        _eventBus.PublishAsync(new LablabBean.Contracts.UI.Events.ViewportChangedEvent(oldViewport, _viewport));
    }
}
```

---

## Best Practices

### 1. Event Handler Performance

✅ **DO**: Keep event handlers fast and non-blocking

```csharp
eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
{
    // Fast: Just log and return
    _logger.LogInformation("Entity spawned: {Type}", evt.EntityType);
    await Task.CompletedTask;
});
```

❌ **DON'T**: Perform heavy work in event handlers

```csharp
eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
{
    // BAD: Heavy database query blocks other subscribers
    await _database.SaveEntityAsync(evt);
    await Task.Delay(1000); // BAD: Artificial delay
});
```

**Solution**: Offload heavy work to background tasks

```csharp
eventBus.Subscribe<EntitySpawnedEvent>(evt =>
{
    // Queue work for background processing
    _workQueue.Enqueue(evt);
    return Task.CompletedTask;
});
```

### 2. Error Handling

✅ **DO**: Handle errors gracefully in event handlers

```csharp
eventBus.Subscribe<CombatEvent>(async evt =>
{
    try
    {
        await ProcessCombat(evt);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to process combat event");
        // Don't rethrow - let other subscribers continue
    }
});
```

### 3. Service Registration Priority

Use priority ranges to control service selection:

| Priority Range | Purpose | Example |
|----------------|---------|---------|
| 1000+ | Framework services | Core game engine |
| 500-999 | High-priority plugins | Premium features |
| 100-499 | Standard plugins | Most plugins |
| 1-99 | Fallback implementations | Default/mock services |

```csharp
// High-priority game service (overrides default)
context.Registry.Register<IGameService>(
    new AdvancedGameService(),
    new ServiceMetadata { Priority = 500, Name = "Advanced", Version = "2.0" }
);

// Standard plugin
context.Registry.Register<IAnalyticsService>(
    new AnalyticsService(),
    new ServiceMetadata { Priority = 100, Name = "Analytics", Version = "1.0" }
);
```

### 4. Avoid Circular Event Dependencies

❌ **DON'T**: Create circular event chains

```csharp
// Plugin A
eventBus.Subscribe<EventA>(evt => eventBus.PublishAsync(new EventB()));

// Plugin B
eventBus.Subscribe<EventB>(evt => eventBus.PublishAsync(new EventA()));
// This creates an infinite loop!
```

✅ **DO**: Design events with clear direction

```csharp
// Game plugin publishes events
await eventBus.PublishAsync(new EntityMovedEvent(...));

// UI plugin subscribes (doesn't publish back to game)
eventBus.Subscribe<EntityMovedEvent>(evt => UpdateDisplay(evt));
```

### 5. Testing Event-Driven Plugins

```csharp
[Fact]
public async Task AnalyticsPlugin_TracksEntitySpawns()
{
    // Arrange
    var eventBus = new EventBus(NullLogger<EventBus>.Instance);
    var plugin = new AnalyticsPlugin();
    var context = new MockPluginContext(eventBus);

    await plugin.InitializeAsync(context);

    // Act
    await eventBus.PublishAsync(new EntitySpawnedEvent(
        Guid.NewGuid(),
        "goblin",
        new Position(10, 10)
    ));

    // Assert
    // Verify plugin tracked the spawn (check logs, metrics, etc.)
}
```

---

## Troubleshooting

### Problem: Event not received by subscriber

**Symptoms**: Published event doesn't trigger subscriber handler

**Causes**:

1. Subscriber registered after event was published
2. Type mismatch between published and subscribed event
3. Event bus not registered in DI container

**Solutions**:

```csharp
// 1. Subscribe during InitializeAsync (before game loop starts)
public Task InitializeAsync(IPluginContext context)
{
    var eventBus = context.Registry.Get<IEventBus>();
    eventBus.Subscribe<EntitySpawnedEvent>(OnEntitySpawned); // ✅ Early subscription
    return Task.CompletedTask;
}

// 2. Ensure exact type match
await eventBus.PublishAsync(new EntitySpawnedEvent(...)); // Publisher
eventBus.Subscribe<EntitySpawnedEvent>(handler); // ✅ Exact type match

// 3. Verify event bus registration in ServiceCollectionExtensions
services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<EventBus>());
```

### Problem: Service not found via IRegistry

**Symptoms**: `Get<IService>()` throws `InvalidOperationException`

**Causes**:

1. Service not registered
2. Wrong interface type
3. Plugin not loaded

**Solutions**:

```csharp
// 1. Register service in InitializeAsync
context.Registry.Register<IGameService>(gameService, metadata);

// 2. Use correct interface type
var gameService = context.Registry.Get<LablabBean.Contracts.Game.Services.IService>(); // ✅ Full namespace

// 3. Check plugin manifest
{
  "plugins": [
    { "name": "MyPlugin", "assembly": "MyPlugin.dll", "enabled": true } // ✅ enabled: true
  ]
}
```

---

## Next Steps

1. **Read the spec**: [spec.md](./spec.md) for complete requirements
2. **Review data model**: [data-model.md](./data-model.md) for all events and interfaces
3. **Explore contracts**: [contracts/](./contracts/) for interface definitions
4. **Run examples**: Build and test the analytics plugin
5. **Create your plugin**: Follow the patterns above

## Resources

- **Spec**: [spec.md](./spec.md)
- **Data Model**: [data-model.md](./data-model.md)
- **Contracts**: [contracts/](./contracts/)
- **Cross-Milo Reference**: `docs/_inbox/cross-milo-contract-adoption-analysis.md`

---

**Time to first plugin**: ~30 minutes
**Difficulty**: Intermediate
**Support**: See project README for community links
