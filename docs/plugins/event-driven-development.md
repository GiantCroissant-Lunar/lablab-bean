# Event-Driven Plugin Development

**Version**: 1.0.0
**Last Updated**: 2025-10-21
**Audience**: Plugin developers

## Overview

The lablab-bean plugin system supports **event-driven architecture** through the `IEventBus` interface. This enables plugins to communicate without direct dependencies, creating a loosely coupled, reactive system.

## Quick Start

Create your first event-driven plugin in under 30 minutes:

### 1. Create Plugin Project

```bash
cd plugins/
dotnet new classlib -n YourPlugin -f net8.0
```

### 2. Add References

```xml
<ItemGroup>
  <ProjectReference Include="..\..\dotnet\framework\LablabBean.Plugins.Contracts\LablabBean.Plugins.Contracts.csproj" />
  <ProjectReference Include="..\..\dotnet\framework\LablabBean.Contracts.Game\LablabBean.Contracts.Game.csproj" />
</ItemGroup>
```

### 3. Implement Plugin

```csharp
using LablabBean.Contracts.Game.Events;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

public class YourPlugin : IPlugin
{
    private ILogger? _logger;
    private IEventBus? _eventBus;

    public string Id => "your-plugin-id";
    public string Name => "Your Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _eventBus = context.Registry.Get<IEventBus>();

        // Subscribe to events
        _eventBus.Subscribe<EntitySpawnedEvent>(OnEntitySpawned);

        _logger.LogInformation("Plugin initialized");
        return Task.CompletedTask;
    }

    private Task OnEntitySpawned(EntitySpawnedEvent evt)
    {
        _logger?.LogInformation("Entity spawned: {Type} at ({X}, {Y})",
            evt.EntityType, evt.Position.X, evt.Position.Y);
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct = default) => Task.CompletedTask;
}
```

## Core Concepts

### Event Bus

The `IEventBus` provides publish-subscribe messaging:

```csharp
// Subscribe to events
eventBus.Subscribe<EntityMovedEvent>(async evt =>
{
    // Handle event
    await ProcessMovement(evt);
});

// Publish events
await eventBus.PublishAsync(new EntitySpawnedEvent(id, "goblin", position));
```

**Key Features**:

- **Sequential Execution**: Subscribers execute in order (predictable)
- **Error Isolation**: One subscriber's exception doesn't affect others
- **Thread-Safe**: Concurrent publishing supported
- **Performance**: 1.1M+ events/second, 0.003ms latency

### Service Contracts

Define platform-independent service interfaces:

```csharp
// Get service from registry
var gameService = context.Registry.Get<LablabBean.Contracts.Game.Services.IService>();

// Use service
await gameService.StartGameAsync(new GameStartOptions("Normal", 12345, "Player"));
var entityId = await gameService.SpawnEntityAsync("player", new Position(5, 5));
```

**Priority-Based Selection**:

```csharp
// Register with priority
context.Registry.Register<IService>(
    myService,
    new ServiceMetadata { Priority = 200, Name = "MyService", Version = "1.0.0" }
);

// Higher priority wins (200 > 100)
var service = context.Registry.Get<IService>(SelectionMode.HighestPriority);
```

## Example Patterns

### Pattern 1: Analytics Plugin (Event Subscriber)

Track game events without direct game plugin dependency:

```csharp
public class AnalyticsPlugin : IPlugin
{
    private int _spawnCount;
    private int _combatCount;

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        var eventBus = context.Registry.Get<IEventBus>();

        eventBus.Subscribe<EntitySpawnedEvent>(evt =>
        {
            _spawnCount++;
            return Task.CompletedTask;
        });

        eventBus.Subscribe<CombatEvent>(evt =>
        {
            _combatCount++;
            return Task.CompletedTask;
        });

        return Task.CompletedTask;
    }
}
```

### Pattern 2: Game Service Provider

Implement game mechanics with event publishing:

```csharp
public class GameService : LablabBean.Contracts.Game.Services.IService
{
    private readonly IEventBus _eventBus;

    public async Task<Guid> SpawnEntityAsync(string entityType, Position position)
    {
        var entityId = Guid.NewGuid();
        // ... spawn logic ...

        // Publish event
        await _eventBus.PublishAsync(new EntitySpawnedEvent(entityId, entityType, position));

        return entityId;
    }
}
```

### Pattern 3: Reactive UI Plugin

Update UI automatically when game state changes:

```csharp
public class ReactiveUIService : LablabBean.Contracts.UI.Services.IService
{
    private bool _needsRedraw;

    public Task InitializeAsync(UIInitOptions options, CancellationToken ct = default)
    {
        // Subscribe to game events
        _eventBus.Subscribe<EntityMovedEvent>(evt =>
        {
            _needsRedraw = true;
            return Task.CompletedTask;
        });

        _eventBus.Subscribe<CombatEvent>(evt =>
        {
            _needsRedraw = true;
            return Task.CompletedTask;
        });

        return Task.CompletedTask;
    }

    public Task UpdateDisplayAsync()
    {
        if (_needsRedraw)
        {
            // Refresh display
            _needsRedraw = false;
        }
        return Task.CompletedTask;
    }
}
```

## Best Practices

### ✅ DO

**Keep event handlers fast**:

```csharp
eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
{
    _logger.LogInformation("Entity spawned: {Type}", evt.EntityType);
    await Task.CompletedTask; // Fast, non-blocking
});
```

**Handle errors gracefully**:

```csharp
eventBus.Subscribe<CombatEvent>(async evt =>
{
    try
    {
        await ProcessCombat(evt);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to process combat");
        // Don't rethrow - let other subscribers continue
    }
});
```

**Use priority ranges appropriately**:

- **1000+**: Framework services
- **500-999**: High-priority plugins
- **100-499**: Standard plugins
- **1-99**: Fallback implementations

### ❌ DON'T

**Don't create circular event chains**:

```csharp
// BAD: Infinite loop
eventBus.Subscribe<EventA>(evt => eventBus.PublishAsync(new EventB()));
eventBus.Subscribe<EventB>(evt => eventBus.PublishAsync(new EventA()));
```

**Don't perform heavy work in handlers**:

```csharp
// BAD: Blocks other subscribers
eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
{
    await Task.Delay(1000); // BAD
    await _database.SaveAsync(evt); // BAD if slow
});

// GOOD: Offload to background
eventBus.Subscribe<EntitySpawnedEvent>(evt =>
{
    _workQueue.Enqueue(evt); // Fast
    return Task.CompletedTask;
});
```

## Available Events

### Game Events

| Event | When Published | Properties |
|-------|----------------|------------|
| `EntitySpawnedEvent` | Entity created | EntityId, EntityType, Position |
| `EntityMovedEvent` | Entity moves | EntityId, OldPosition, NewPosition |
| `CombatEvent` | Combat occurs | AttackerId, TargetId, DamageDealt, IsHit, IsKill |
| `GameStateChangedEvent` | Game state changes | OldState, NewState, Reason |

### UI Events

| Event | When Published | Properties |
|-------|----------------|------------|
| `InputReceivedEvent` | User input | Command (Type, Key, Metadata) |
| `ViewportChangedEvent` | Camera moves | OldViewport, NewViewport |

## Testing Your Plugin

```csharp
[Fact]
public async Task MyPlugin_ReceivesEvents()
{
    // Arrange
    var eventBus = new EventBus(NullLogger<EventBus>.Instance);
    var registry = new ServiceRegistry();
    registry.Register<IEventBus>(eventBus, new ServiceMetadata { Priority = 1000 });

    var plugin = new MyPlugin();
    var context = new MockPluginContext(registry);

    // Act
    await plugin.InitializeAsync(context);
    await eventBus.PublishAsync(new EntitySpawnedEvent(Guid.NewGuid(), "test", new Position(0, 0)));

    // Assert
    // Verify plugin behavior
}
```

## Performance Characteristics

The event bus has been validated to exceed spec requirements:

- **Latency**: 0.003ms average (3,333x better than 10ms target)
- **Throughput**: 1.1M+ events/second (1,124x better than 1,000/sec target)
- **Memory**: ~239 bytes per event
- **Thread Safety**: Concurrent publishing supported

See [performance-results.md](../../specs/007-tiered-contract-architecture/performance-results.md) for detailed metrics.

## Troubleshooting

### Event not received

**Cause**: Subscriber registered after event published

**Solution**: Subscribe during `InitializeAsync` before game loop starts

### Service not found

**Cause**: Service not registered or wrong interface type

**Solution**:

```csharp
// Use full namespace
var service = context.Registry.Get<LablabBean.Contracts.Game.Services.IService>();
```

### Multiple implementations conflict

**Solution**: Use priority-based selection

```csharp
context.Registry.Register<IService>(service, new ServiceMetadata { Priority = 200 });
```

## Additional Resources

- **Spec**: [spec.md](../../specs/007-tiered-contract-architecture/spec.md)
- **Data Model**: [data-model.md](../../specs/007-tiered-contract-architecture/data-model.md)
- **Quickstart**: [quickstart.md](../../specs/007-tiered-contract-architecture/quickstart.md)
- **Performance**: [performance-results.md](../../specs/007-tiered-contract-architecture/performance-results.md)

---

**Time to first plugin**: ~30 minutes
**Difficulty**: Intermediate
**Questions?**: See project README for community links
