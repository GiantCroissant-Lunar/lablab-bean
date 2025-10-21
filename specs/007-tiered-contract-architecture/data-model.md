# Data Model: Tiered Contract Architecture

**Date**: 2025-10-21  
**Feature**: 007-tiered-contract-architecture  
**Phase**: 1 - Design & Contracts

## Overview

This document defines the data models, events, and service interfaces for the tiered contract architecture. All entities follow the cross-milo tier 1 and tier 2 patterns with clear separation between contracts (what) and implementations (how).

## Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│ TIER 1: Domain Contracts (LablabBean.Contracts.*)          │
│ - Service interfaces (IService)                             │
│ - Event definitions (record types)                          │
│ - Data models (immutable types)                             │
│ - Dependencies: LablabBean.Plugins.Contracts only           │
└─────────────────────────────────────────────────────────────┘
                            ▲
                            │
┌─────────────────────────────────────────────────────────────┐
│ TIER 2: Infrastructure (LablabBean.Plugins.*)               │
│ - IRegistry (service registration)                          │
│ - IEventBus (event publishing)                              │
│ - ServiceRegistry (implementation)                          │
│ - EventBus (implementation)                                 │
└─────────────────────────────────────────────────────────────┘
                            ▲
                            │
┌─────────────────────────────────────────────────────────────┐
│ TIER 3+: Plugin Implementations                             │
│ - Concrete service implementations                          │
│ - Platform-specific code (Terminal.Gui, SadConsole, etc.)  │
│ - Dependencies: Tier 1 contracts + Tier 2 infrastructure   │
└─────────────────────────────────────────────────────────────┘
```

## Core Infrastructure (Tier 2)

### IEventBus Interface

**Assembly**: `LablabBean.Plugins.Contracts`  
**Namespace**: `LablabBean.Plugins.Contracts`

```csharp
/// <summary>
/// Event bus for publish-subscribe communication between plugins.
/// Enables loose coupling via asynchronous event delivery.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publish an event to all registered subscribers.
    /// Executes subscribers sequentially (not in parallel).
    /// Exceptions in subscribers are logged but do not affect other subscribers.
    /// </summary>
    /// <typeparam name="T">Event type (must be a class)</typeparam>
    /// <param name="eventData">Event data to publish</param>
    /// <returns>Task that completes when all subscribers have been notified</returns>
    Task PublishAsync<T>(T eventData) where T : class;

    /// <summary>
    /// Subscribe to events of a specific type.
    /// Handler will be invoked asynchronously when events are published.
    /// </summary>
    /// <typeparam name="T">Event type to subscribe to</typeparam>
    /// <param name="handler">Async handler function</param>
    void Subscribe<T>(Func<T, Task> handler) where T : class;
}
```

**Design Notes**:
- Generic type constraint `where T : class` ensures events are reference types
- Sequential execution (not parallel) ensures predictable ordering
- No unsubscribe mechanism in initial version (subscribers live for app lifetime)
- Thread-safe for concurrent publishing from multiple plugins

---

## Game Domain Contracts (Tier 1)

### Assembly: LablabBean.Contracts.Game

**Target Framework**: net8.0  
**Dependencies**: `LablabBean.Plugins.Contracts`  
**Namespace Root**: `LablabBean.Contracts.Game`

### Game Service Interface

**Namespace**: `LablabBean.Contracts.Game.Services`

```csharp
/// <summary>
/// Game service contract for dungeon crawler mechanics.
/// Platform-independent interface for game loop, entity management, and turn processing.
/// </summary>
public interface IService
{
    /// <summary>
    /// Start a new game session.
    /// </summary>
    /// <param name="options">Game start options (difficulty, seed, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when game is initialized</returns>
    Task StartGameAsync(GameStartOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a single game turn.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when turn processing is done</returns>
    Task ProcessTurnAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Spawn an entity at the specified position.
    /// Publishes EntitySpawnedEvent on success.
    /// </summary>
    /// <param name="entityType">Type of entity to spawn</param>
    /// <param name="position">World position</param>
    /// <returns>Entity ID of spawned entity</returns>
    Task<Guid> SpawnEntityAsync(string entityType, Position position);

    /// <summary>
    /// Move an entity to a new position.
    /// Publishes EntityMovedEvent on success.
    /// </summary>
    /// <param name="entityId">Entity to move</param>
    /// <param name="newPosition">Target position</param>
    /// <returns>True if move succeeded, false if blocked</returns>
    Task<bool> MoveEntityAsync(Guid entityId, Position newPosition);

    /// <summary>
    /// Execute an attack from attacker to target.
    /// Publishes CombatEvent with results.
    /// </summary>
    /// <param name="attackerId">Attacking entity</param>
    /// <param name="targetId">Target entity</param>
    /// <returns>Combat result (damage dealt, hit/miss, etc.)</returns>
    Task<CombatResult> AttackAsync(Guid attackerId, Guid targetId);

    /// <summary>
    /// Get current game state snapshot.
    /// </summary>
    /// <returns>Immutable game state</returns>
    GameState GetGameState();

    /// <summary>
    /// Get all entities as immutable snapshots.
    /// </summary>
    /// <returns>Collection of entity snapshots</returns>
    IReadOnlyCollection<EntitySnapshot> GetEntities();
}
```

### Game Events

**Namespace**: `LablabBean.Contracts.Game.Events`

```csharp
/// <summary>
/// Published when an entity is spawned in the game world.
/// </summary>
public record EntitySpawnedEvent(
    Guid EntityId,
    string EntityType,
    Position Position,
    DateTimeOffset Timestamp
)
{
    public EntitySpawnedEvent(Guid entityId, string entityType, Position position)
        : this(entityId, entityType, position, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Published when an entity moves to a new position.
/// </summary>
public record EntityMovedEvent(
    Guid EntityId,
    Position OldPosition,
    Position NewPosition,
    DateTimeOffset Timestamp
)
{
    public EntityMovedEvent(Guid entityId, Position oldPosition, Position newPosition)
        : this(entityId, oldPosition, newPosition, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Published when combat occurs between entities.
/// </summary>
public record CombatEvent(
    Guid AttackerId,
    Guid TargetId,
    int DamageDealt,
    bool IsHit,
    bool IsKill,
    DateTimeOffset Timestamp
)
{
    public CombatEvent(Guid attackerId, Guid targetId, int damageDealt, bool isHit, bool isKill)
        : this(attackerId, targetId, damageDealt, isHit, isKill, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Published when game state changes (pause, resume, game over, etc.).
/// </summary>
public record GameStateChangedEvent(
    GameStateType OldState,
    GameStateType NewState,
    string? Reason,
    DateTimeOffset Timestamp
)
{
    public GameStateChangedEvent(GameStateType oldState, GameStateType newState, string? reason = null)
        : this(oldState, newState, reason, DateTimeOffset.UtcNow)
    {
    }
}
```

### Game Models

**Namespace**: `LablabBean.Contracts.Game.Models`

```csharp
/// <summary>
/// Immutable snapshot of an entity's state.
/// </summary>
public record EntitySnapshot(
    Guid Id,
    string Type,
    Position Position,
    int Health,
    int MaxHealth,
    IReadOnlyDictionary<string, object> Properties
);

/// <summary>
/// Immutable game state snapshot.
/// </summary>
public record GameState(
    GameStateType State,
    int TurnNumber,
    Guid? PlayerEntityId,
    int CurrentLevel,
    DateTimeOffset StartTime
);

/// <summary>
/// World position (x, y coordinates).
/// </summary>
public record Position(int X, int Y);

/// <summary>
/// Combat result details.
/// </summary>
public record CombatResult(
    int DamageDealt,
    bool IsHit,
    bool IsKill,
    string? SpecialEffect
);

/// <summary>
/// Game start options.
/// </summary>
public record GameStartOptions(
    string Difficulty,
    int? Seed,
    string? PlayerName
);

/// <summary>
/// Game state types.
/// </summary>
public enum GameStateType
{
    NotStarted,
    Running,
    Paused,
    GameOver,
    Victory
}
```

---

## UI Domain Contracts (Tier 1)

### Assembly: LablabBean.Contracts.UI

**Target Framework**: net8.0  
**Dependencies**: `LablabBean.Plugins.Contracts`  
**Namespace Root**: `LablabBean.Contracts.UI`

### UI Service Interface

**Namespace**: `LablabBean.Contracts.UI.Services`

```csharp
/// <summary>
/// UI service contract for rendering and input handling.
/// Platform-independent interface for display updates and user interaction.
/// </summary>
public interface IService
{
    /// <summary>
    /// Initialize the UI system.
    /// </summary>
    /// <param name="options">UI initialization options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when UI is ready</returns>
    Task InitializeAsync(UIInitOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Render the game viewport with current entities and terrain.
    /// </summary>
    /// <param name="viewport">Viewport bounds to render</param>
    /// <param name="entities">Entities to display</param>
    /// <returns>Task that completes when rendering is done</returns>
    Task RenderViewportAsync(ViewportBounds viewport, IReadOnlyCollection<EntitySnapshot> entities);

    /// <summary>
    /// Update the UI display (refresh screen, process pending updates).
    /// </summary>
    /// <returns>Task that completes when display is updated</returns>
    Task UpdateDisplayAsync();

    /// <summary>
    /// Handle user input command.
    /// Publishes InputReceivedEvent for each input.
    /// </summary>
    /// <param name="command">Input command to process</param>
    /// <returns>Task that completes when input is handled</returns>
    Task HandleInputAsync(InputCommand command);

    /// <summary>
    /// Get current viewport bounds.
    /// </summary>
    /// <returns>Current viewport bounds</returns>
    ViewportBounds GetViewport();

    /// <summary>
    /// Set viewport center position (camera follow).
    /// Publishes ViewportChangedEvent on change.
    /// </summary>
    /// <param name="centerPosition">New center position</param>
    void SetViewportCenter(Position centerPosition);
}
```

### UI Events

**Namespace**: `LablabBean.Contracts.UI.Events`

```csharp
/// <summary>
/// Published when user input is received.
/// </summary>
public record InputReceivedEvent(
    InputCommand Command,
    DateTimeOffset Timestamp
)
{
    public InputReceivedEvent(InputCommand command)
        : this(command, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Published when viewport bounds change (resize, camera move).
/// </summary>
public record ViewportChangedEvent(
    ViewportBounds OldViewport,
    ViewportBounds NewViewport,
    DateTimeOffset Timestamp
)
{
    public ViewportChangedEvent(ViewportBounds oldViewport, ViewportBounds newViewport)
        : this(oldViewport, newViewport, DateTimeOffset.UtcNow)
    {
    }
}
```

### UI Models

**Namespace**: `LablabBean.Contracts.UI.Models`

```csharp
/// <summary>
/// User input command.
/// </summary>
public record InputCommand(
    InputType Type,
    string Key,
    IReadOnlyDictionary<string, object>? Metadata = null
);

/// <summary>
/// Viewport bounds (visible area).
/// </summary>
public record ViewportBounds(
    Position TopLeft,
    int Width,
    int Height
)
{
    public Position Center => new(TopLeft.X + Width / 2, TopLeft.Y + Height / 2);
    public Position BottomRight => new(TopLeft.X + Width - 1, TopLeft.Y + Height - 1);
}

/// <summary>
/// UI initialization options.
/// </summary>
public record UIInitOptions(
    int ViewportWidth,
    int ViewportHeight,
    bool EnableMouse,
    string Theme
);

/// <summary>
/// Input types.
/// </summary>
public enum InputType
{
    Movement,
    Action,
    Menu,
    System
}
```

---

## Event Bus Implementation (Tier 2)

### EventBus Class

**Assembly**: `LablabBean.Plugins.Core`  
**Namespace**: `LablabBean.Plugins.Core`

```csharp
/// <summary>
/// Thread-safe event bus implementation with sequential subscriber execution.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly ILogger<EventBus> _logger;
    private readonly ConcurrentDictionary<Type, List<Func<object, Task>>> _subscribers;

    public EventBus(ILogger<EventBus> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subscribers = new ConcurrentDictionary<Type, List<Func<object, Task>>>();
    }

    public async Task PublishAsync<T>(T eventData) where T : class
    {
        if (eventData == null)
            throw new ArgumentNullException(nameof(eventData));

        var eventType = typeof(T);
        
        if (!_subscribers.TryGetValue(eventType, out var handlers))
        {
            // No subscribers - this is valid, just return
            return;
        }

        // Execute subscribers sequentially
        foreach (var handler in handlers)
        {
            try
            {
                await handler(eventData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Event subscriber failed for {EventType}. Event: {@Event}", 
                    eventType.Name, 
                    eventData);
            }
        }
    }

    public void Subscribe<T>(Func<T, Task> handler) where T : class
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var eventType = typeof(T);
        
        // Wrap typed handler in object handler
        Func<object, Task> wrappedHandler = obj => handler((T)obj);

        _subscribers.AddOrUpdate(
            eventType,
            _ => new List<Func<object, Task>> { wrappedHandler },
            (_, existing) =>
            {
                lock (existing)
                {
                    existing.Add(wrappedHandler);
                }
                return existing;
            }
        );
    }
}
```

---

## Validation Rules

### Event Validation
- ✅ All events MUST be `record` types
- ✅ All events MUST have `DateTimeOffset Timestamp` property
- ✅ All events MUST provide convenience constructor without timestamp
- ✅ Event naming MUST follow `{Subject}{Action}Event` pattern
- ✅ All event properties MUST be read-only (record enforces this)

### Service Interface Validation
- ✅ Service interfaces MUST be named `IService` within domain namespace
- ✅ Service interfaces MUST use async methods for I/O operations
- ✅ Service interfaces MUST be technology-agnostic (no UI framework dependencies)
- ✅ Service interfaces MUST document when events are published

### Contract Assembly Validation
- ✅ Contract assemblies MUST only reference `LablabBean.Plugins.Contracts`
- ✅ Contract assemblies MUST target net8.0
- ✅ Contract assemblies MUST follow namespace pattern: `LablabBean.Contracts.{Domain}`
- ✅ Supporting types (events, models) MUST be in appropriate sub-namespaces

---

## Dependency Graph

```
LablabBean.Contracts.Game
    └── LablabBean.Plugins.Contracts

LablabBean.Contracts.UI
    └── LablabBean.Plugins.Contracts

LablabBean.Plugins.Core
    ├── LablabBean.Plugins.Contracts
    └── Microsoft.Extensions.Logging

Plugin Implementations
    ├── LablabBean.Contracts.Game (or UI)
    ├── LablabBean.Plugins.Contracts
    └── Platform-specific dependencies (Terminal.Gui, etc.)
```

**Key Principle**: Tier 1 contracts have minimal dependencies (only Tier 2 infrastructure). This enables platform independence and clean separation of concerns.

---

## Next Steps

Phase 1 design complete. Ready to generate:
1. ✅ `data-model.md` - This document
2. ⏭️ `contracts/` - Interface definition files
3. ⏭️ `quickstart.md` - Developer guide

**Phase 1 Status**: Data model complete, proceeding to contract generation.
