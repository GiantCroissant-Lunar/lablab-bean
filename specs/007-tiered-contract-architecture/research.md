# Research: Tiered Contract Architecture

**Date**: 2025-10-21
**Feature**: 007-tiered-contract-architecture
**Phase**: 0 - Research & Clarification

## Overview

This document consolidates research findings for adopting cross-milo tier 1 and tier 2 contract design patterns in lablab-bean. All technical unknowns from the Technical Context have been resolved through analysis of the cross-milo reference architecture and existing lablab-bean codebase.

## Research Tasks

### 1. Event Bus Implementation Patterns

**Question**: What are the best practices for implementing `IEventBus` in .NET 8 with async/await patterns?

**Decision**: Use `ConcurrentDictionary<Type, List<Func<object, Task>>>` for subscriber storage with sequential async execution.

**Rationale**:

- **Thread Safety**: `ConcurrentDictionary` provides lock-free reads for high-performance event publishing
- **Sequential Execution**: Subscribers execute in order via `foreach` + `await`, ensuring predictable behavior
- **Error Isolation**: `try-catch` around each subscriber prevents cascading failures
- **Type Safety**: Generic `PublishAsync<T>()` and `Subscribe<T>()` methods provide compile-time type checking

**Alternatives Considered**:

- **Reactive Extensions (Rx.NET)**: Rejected - adds external dependency, overkill for simple pub-sub
- **Parallel Execution**: Rejected - unpredictable ordering, harder to debug, spec requires sequential
- **MediatR**: Rejected - designed for request/response, not pub-sub; adds dependency

**Reference**: Cross-milo `IEventBus` interface at `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts/IEventBus.cs`

---

### 2. Event Definition Best Practices

**Question**: Should events be `record` types or `class` types? What properties are required?

**Decision**: Use immutable `record` types with required `DateTimeOffset Timestamp` property.

**Rationale**:

- **Immutability**: Records are immutable by default, preventing accidental mutation after publishing
- **Value Equality**: Records provide structural equality, useful for testing and debugging
- **Concise Syntax**: Records reduce boilerplate with primary constructors
- **Timestamp**: Required for event ordering, debugging, and analytics

**Pattern**:

```csharp
public record EntitySpawnedEvent(
    Guid EntityId,
    string EntityType,
    Position Position,
    DateTimeOffset Timestamp
)
{
    // Convenience constructor without timestamp
    public EntitySpawnedEvent(Guid entityId, string entityType, Position position)
        : this(entityId, entityType, position, DateTimeOffset.UtcNow)
    {
    }
}
```

**Alternatives Considered**:

- **Class with init-only properties**: Rejected - more verbose, no structural equality
- **Struct**: Rejected - boxing overhead when stored in event bus, no inheritance

**Reference**: Cross-milo event patterns in `docs/_inbox/cross-milo-contract-adoption-analysis.md`

---

### 3. Domain Contract Assembly Organization

**Question**: How should contract assemblies be organized? What naming conventions should be used?

**Decision**: Follow cross-milo pattern: `LablabBean.Contracts.{Domain}` with generic `IService` naming.

**Rationale**:

- **Namespace Clarity**: Full namespace `LablabBean.Contracts.Game.Services.IService` is unambiguous
- **Scalability**: Easy to add multiple services per domain (e.g., `IService`, `IAdvancedService`)
- **Consistency**: Matches cross-milo's 17 contract assemblies pattern
- **Separation**: Contracts have zero implementation dependencies (only reference `LablabBean.Plugins.Contracts`)

**Structure**:

```
LablabBean.Contracts.Game/
├── Services/
│   └── IService.cs              # namespace: LablabBean.Contracts.Game.Services
├── Events/
│   ├── EntitySpawnedEvent.cs    # namespace: LablabBean.Contracts.Game.Events
│   └── ...
└── Models/
    └── EntitySnapshot.cs         # namespace: LablabBean.Contracts.Game.Models
```

**Alternatives Considered**:

- **Specific naming (IGameService)**: Rejected - redundant with namespace, less scalable
- **Flat structure (all in root)**: Rejected - harder to navigate, less organized
- **Single Contracts assembly**: Rejected - violates separation of concerns, creates coupling

**Reference**: Cross-milo contract structure at `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.*/`

---

### 4. Service Registration and Priority

**Question**: How should service implementations register themselves? What priority values should be used?

**Decision**: Use existing `IRegistry.Register<T>()` with priority metadata. Framework services: 1000+, game plugins: 100-500, default: 100.

**Rationale**:

- **Existing Infrastructure**: `IRegistry` and `ServiceRegistry` already implement priority-based selection
- **Backward Compatible**: No changes needed to existing registration code
- **Clear Hierarchy**: Priority ranges provide clear ordering (framework > game > default)
- **Flexibility**: Plugins can override default implementations by registering with higher priority

**Pattern**:

```csharp
// In plugin's InitializeAsync()
var gameService = new MyGameService(context);
context.Registry.Register<LablabBean.Contracts.Game.Services.IService>(
    gameService,
    new ServiceMetadata
    {
        Priority = 200,
        Name = "MyGameService",
        Version = "1.0.0"
    }
);
```

**Alternatives Considered**:

- **Attribute-based registration**: Rejected - requires reflection, less explicit
- **Convention-based registration**: Rejected - magic behavior, harder to debug
- **DI container integration**: Rejected - adds complexity, existing `IRegistry` is sufficient

**Reference**: Existing `ServiceRegistry.cs` at `dotnet/framework/LablabBean.Plugins.Core/ServiceRegistry.cs`

---

### 5. Event Bus Registration and Lifecycle

**Question**: When should the event bus be registered? How do plugins subscribe to events?

**Decision**: Register `IEventBus` as singleton in `ServiceCollectionExtensions`, expose via `IRegistry`, plugins subscribe during `InitializeAsync()`.

**Rationale**:

- **Singleton Lifetime**: Event bus must live for entire application lifetime
- **Early Registration**: Available before any plugins load
- **Consistent Access**: Plugins retrieve via `context.Registry.Get<IEventBus>()`
- **Subscription Timing**: Plugins subscribe during initialization, before game loop starts

**Pattern**:

```csharp
// In ServiceCollectionExtensions.cs
services.AddSingleton<EventBus>();
services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<EventBus>());

// In plugin's InitializeAsync()
var eventBus = context.Registry.Get<IEventBus>();
eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
{
    await HandleEntitySpawned(evt);
});
```

**Alternatives Considered**:

- **Scoped lifetime**: Rejected - event bus must be singleton for cross-plugin communication
- **Lazy registration**: Rejected - plugins need event bus during initialization
- **Separate subscription API**: Rejected - `IEventBus` interface is sufficient

**Reference**: Cross-milo `IEventBus` usage in `docs/_inbox/cross-milo-contract-adoption-analysis.md`

---

### 6. Error Handling and Logging

**Question**: How should the event bus handle subscriber exceptions? What logging is required?

**Decision**: Catch exceptions per subscriber, log error with event type and subscriber details, continue to next subscriber.

**Rationale**:

- **Isolation**: One failing subscriber should not break others (spec requirement FR-004)
- **Observability**: Log errors with context for debugging (spec requirement FR-005)
- **Resilience**: Event publishing completes even if all subscribers fail
- **Debugging**: Include event type, subscriber count, and exception details in logs

**Pattern**:

```csharp
public async Task PublishAsync<T>(T eventData) where T : class
{
    var subscribers = GetSubscribers<T>();
    foreach (var handler in subscribers)
    {
        try
        {
            await handler(eventData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Event subscriber failed for {EventType}. Event: {@Event}",
                typeof(T).Name,
                eventData);
        }
    }
}
```

**Alternatives Considered**:

- **Fail-fast**: Rejected - violates spec requirement for error isolation
- **Dead letter queue**: Rejected - overkill for in-memory event bus, no persistence
- **Retry logic**: Rejected - adds complexity, subscribers should handle retries if needed

**Reference**: Spec requirements FR-004 and FR-005

---

### 7. Performance Optimization Strategies

**Question**: How can we achieve <10ms event publishing and 1000 events/second throughput?

**Decision**: Use lock-free reads with `ConcurrentDictionary`, avoid allocations in hot path, consider object pooling for high-frequency events.

**Rationale**:

- **Lock-Free Reads**: `ConcurrentDictionary` allows concurrent reads without locks
- **Minimal Allocations**: Reuse subscriber lists, avoid LINQ in hot path
- **Object Pooling**: For high-frequency events (e.g., EntityMovedEvent), use `ArrayPool<T>`
- **Async Execution**: Sequential async execution is fast enough for 10 subscribers

**Optimization Checklist**:

- ✅ Use `ConcurrentDictionary` for subscriber storage
- ✅ Avoid LINQ in `PublishAsync` (use `foreach` instead)
- ✅ Cache `Type` lookups
- ⚠️ Consider `ArrayPool<T>` if profiling shows allocation pressure
- ⚠️ Add performance counters for event throughput monitoring

**Alternatives Considered**:

- **Parallel execution**: Rejected - spec requires sequential, adds complexity
- **Pre-compiled delegates**: Rejected - minimal benefit, adds complexity
- **Custom allocator**: Rejected - premature optimization, .NET 8 GC is efficient

**Reference**: Spec success criteria SC-003 and SC-004

---

### 8. Testing Strategy

**Question**: What testing approach ensures event bus reliability and contract correctness?

**Decision**: Three-layer testing: unit tests for event bus, contract tests for service interfaces, integration tests for cross-plugin communication.

**Rationale**:

- **Unit Tests**: Fast, isolated, test event bus behavior (subscribe, publish, error handling)
- **Contract Tests**: Validate service interfaces follow conventions (naming, async patterns)
- **Integration Tests**: End-to-end scenarios (plugin A publishes, plugin B receives)

**Test Coverage**:

1. **EventBusTests.cs**:
   - Subscribe and publish single event
   - Multiple subscribers receive same event
   - Subscriber exception doesn't affect others
   - No subscribers completes successfully
   - Concurrent publishing from multiple threads

2. **GameServiceContractTests.cs**:
   - Interface follows naming conventions
   - All methods are async where appropriate
   - Events follow record pattern with timestamp

3. **Integration Tests**:
   - Analytics plugin receives game events
   - UI plugin receives game state changes
   - Event publishing performance (<10ms)

**Alternatives Considered**:

- **Property-based testing**: Rejected - overkill for simple pub-sub, harder to maintain
- **Mutation testing**: Rejected - not required by spec, adds CI time
- **Load testing**: Deferred - add if performance issues arise

**Reference**: Spec success criteria SC-003, SC-004, SC-008

---

## Summary of Decisions

| Area | Decision | Rationale |
|------|----------|-----------|
| Event Bus Storage | `ConcurrentDictionary<Type, List<Func<object, Task>>>` | Thread-safe, lock-free reads, sequential execution |
| Event Types | Immutable `record` with `DateTimeOffset Timestamp` | Immutability, value equality, concise syntax |
| Contract Organization | `LablabBean.Contracts.{Domain}` with generic `IService` | Scalability, consistency with cross-milo |
| Service Registration | Existing `IRegistry` with priority metadata | Backward compatible, proven pattern |
| Event Bus Lifecycle | Singleton registered in DI, exposed via `IRegistry` | Available to all plugins, consistent access |
| Error Handling | Catch per subscriber, log error, continue | Isolation, observability, resilience |
| Performance | Lock-free reads, minimal allocations, object pooling | <10ms publishing, 1000 events/sec |
| Testing | Unit + Contract + Integration tests | Comprehensive coverage, fast feedback |

## Next Steps

All technical unknowns have been resolved. Ready to proceed to **Phase 1: Design & Contracts**.

Phase 1 will generate:

1. `data-model.md` - Entity and event definitions
2. `contracts/` - Interface definitions for `IEventBus`, `IService` (Game), `IService` (UI)
3. `quickstart.md` - Developer guide for event-driven plugin development

---

**Phase 0 Complete**: ✅ All clarifications resolved, ready for design phase.
