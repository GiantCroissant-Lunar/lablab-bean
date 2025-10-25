# Research: Intelligent Avatar System Technology Decisions

**Feature**: 019-intelligent-avatar-system
**Date**: 2025-10-24
**Status**: Complete

## Overview

This document captures technology decisions and best practices research for implementing the three-layer intelligent avatar architecture (ECS + Akka.NET + Semantic Kernel).

---

## 1. Akka.NET Best Practices for .NET 8

### Decision: Use Akka.Hosting with Microsoft.Extensions.Hosting

**Rationale**:

- Seamless integration with existing `Microsoft.Extensions.DependencyInjection` container
- Automatic actor system lifecycle management (startup/shutdown)
- Configuration through `IConfiguration` (appsettings.json)
- Logging integration with `ILogger<T>`

**Implementation**:

```csharp
// Program.cs
builder.Services.AddAkka("DungeonCrawler", (builder, serviceProvider) =>
{
    builder
        .WithActors((system, registry) =>
        {
            var eventBus = serviceProvider.GetRequiredService<IEventBus>();
            var skBridge = system.ActorOf(Props.Create(() => new SemanticKernelBridgeActor(...)));

            registry.Register<SemanticKernelBridgeActor>(skBridge);
        })
        .WithActorAskTimeout(TimeSpan.FromSeconds(10))
        .AddStartup((system, registry) =>
        {
            // Start supervisor and bridge actors
        });
});
```

**Alternatives Considered**:

- Manual `ActorSystem.Create()`: Rejected - no hosting integration, manual lifecycle
- Akka.Bootstrap: Rejected - deprecated in favor of Akka.Hosting

---

### Decision: One-For-One Supervisor with Restart Strategy

**Rationale**:

- LLM timeouts/errors should only affect individual avatars, not siblings
- Restart preserves actor path for consistent event bus routing
- Max 3 retries within 1 minute prevents infinite restart loops
- Stop directive for non-recoverable errors (ArgumentException)

**Implementation**:

```csharp
protected override SupervisorStrategy SupervisorStrategy()
{
    return new OneForOneStrategy(
        maxNrOfRetries: 3,
        withinTimeRange: TimeSpan.FromMinutes(1),
        localOnlyDecider: ex => ex switch
        {
            HttpRequestException => Directive.Restart,  // LLM API failure
            TimeoutException => Directive.Restart,       // LLM timeout
            ArgumentException => Directive.Stop,         // Invalid state
            _ => Directive.Escalate                      // Unknown error
        });
}
```

**Alternatives Considered**:

- All-For-One: Rejected - one avatar failure shouldn't restart all NPCs
- Resume: Rejected - corrupted state requires restart

---

### Decision: Akka.Persistence.Sql with SQLite

**Rationale**:

- Lightweight, file-based (no server required)
- Good for single-player desktop game
- Works cross-platform (Windows/Linux/macOS)
- Easy backup (copy `.db` file)

**Configuration**:

```json
{
  "akka": {
    "persistence": {
      "journal": {
        "plugin": "akka.persistence.journal.sql",
        "sql": {
          "connection-string": "Data Source=avatars.db",
          "provider-name": "Microsoft.Data.Sqlite"
        }
      },
      "snapshot-store": {
        "plugin": "akka.persistence.snapshot-store.sql",
        "sql": {
          "connection-string": "Data Source=avatars.db",
          "provider-name": "Microsoft.Data.Sqlite"
        }
      }
    }
  }
}
```

**Alternatives Considered**:

- In-memory: Rejected for production - loses state on restart
- PostgreSQL: Rejected - overkill for single-player game
- File-based journal: Rejected - SQLite more robust for queries

---

### Decision: System.Text.Json for Message Serialization

**Rationale**:

- Native .NET 8 support, no extra dependencies
- Compatible with Semantic Kernel JSON responses
- Good performance for game messages
- Akka.NET 1.5.31+ supports it natively

**Configuration**:

```csharp
builder.WithJsonSerializer(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false,  // Compact for performance
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
});
```

**Alternatives Considered**:

- Hyperion: Rejected - not needed for simple message types
- Newtonsoft.Json: Rejected - prefer modern System.Text.Json

---

## 2. Semantic Kernel Integration Patterns

### Decision: Single Kernel Instance with Multiple Agents

**Rationale**:

- Share kernel configuration and plugins across agents
- Reduce memory overhead (one kernel = one HttpClient pool)
- Easier to manage API rate limits globally
- Agents maintain separate chat histories

**Implementation**:

```csharp
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: "gpt-4o-mini",
        apiKey: configuration["SemanticKernel:ApiKey"])
    .Build();

// Bridge actor creates agents on demand
private NpcIntelligenceAgent GetOrCreateAgent(string avatarId)
{
    if (!_agents.ContainsKey(avatarId))
    {
        _agents[avatarId] = new NpcIntelligenceAgent(_kernel, _eventBus, avatarId);
    }
    return _agents[avatarId];
}
```

**Alternatives Considered**:

- One kernel per agent: Rejected - wasteful for 10+ NPCs
- Agent orchestration (SK Agents framework): Considered for future, starting simple

---

### Decision: Structured JSON Prompts with Response Format

**Rationale**:

- Reliable parsing of AI decisions (avoid regex/string parsing)
- OpenAI JSON mode ensures valid JSON output
- Easy to validate and deserialize
- Clear contract between prompt and code

**Prompt Template**:

```csharp
var prompt = $$$"""
You are {{{personality}}}, an NPC in a dungeon crawler.

[context here]

Respond in JSON format:
{
  "action": "Chase|Flee|Wander|Patrol|Idle",
  "reasoning": "Brief explanation",
  "new_emotional_state": "Fearful|Confident|Cautious|Aggressive|Neutral",
  "internal_thought": "What you're thinking"
}
""";

var settings = new OpenAIPromptExecutionSettings
{
    ResponseFormat = "json_object",  // Enforce JSON mode
    MaxTokens = 300,
    Temperature = 0.7  // Balance creativity with consistency
};
```

**Alternatives Considered**:

- Natural language parsing: Rejected - unreliable, needs regex
- XML format: Rejected - JSON simpler and SK native
- Function calling: Considered for future advanced features

---

### Decision: Timeout with Polly Circuit Breaker

**Rationale**:

- Prevent cascade failures when LLM is slow/down
- Circuit opens after 3 consecutive failures
- 30-second break before retry
- Fallback to simple AI immediately

**Implementation**:

```csharp
var policy = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (ex, duration) =>
        {
            _logger.LogWarning("Circuit breaker opened for {Duration}s", duration.TotalSeconds);
            // Fall back to simple AI
        });

await policy.ExecuteAsync(async () =>
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    return await _kernel.InvokePromptAsync<string>(prompt, cancellationToken: cts.Token);
});
```

**Alternatives Considered**:

- Simple timeout: Rejected - no protection against sustained failures
- Retry without circuit breaker: Rejected - wastes API quota during outages

---

### Decision: Response Caching for Common Scenarios

**Rationale**:

- Reduce API costs for repetitive situations
- Improve response time for cached decisions
- Cache key: hash of personality + emotional state + context
- LRU cache with 100 entry limit

**Implementation**:

```csharp
private readonly LruCache<string, AIDecisionMessage> _decisionCache = new(capacity: 100);

public async Task<AIDecisionMessage> DecideActionAsync(AvatarContext context)
{
    var cacheKey = ComputeCacheKey(context);

    if (_decisionCache.TryGet(cacheKey, out var cached))
    {
        _logger.LogDebug("Cache hit for avatar {Id}", context.AvatarId);
        return cached;
    }

    var decision = await InvokeLLM(context);
    _decisionCache.Add(cacheKey, decision);
    return decision;
}
```

**Alternatives Considered**:

- No caching: Rejected - expensive for repetitive gameplay
- Redis cache: Rejected - overkill for single-player game
- Time-based expiry: Considered, but LRU sufficient for game sessions

---

## 3. ECS-Actor Bridge Patterns

### Decision: Component-Based Actor References

**Rationale**:

- Arch ECS components are value types (structs) - fast queries
- `AkkaActorRef` stores `IActorRef` as component
- Query entities by component presence
- Automatic cleanup when entity destroyed (component removed)

**Component Design**:

```csharp
public struct AkkaActorRef
{
    public IActorRef ActorRef { get; set; }

    public AkkaActorRef(IActorRef actorRef)
    {
        ActorRef = actorRef ?? throw new ArgumentNullException(nameof(actorRef));
    }
}

// Query usage
var query = new QueryDescription().WithAll<Position, IntelligentAI, AkkaActorRef>();
world.Query(in query, (Entity entity, ref AkkaActorRef actorRef) =>
{
    actorRef.ActorRef.Tell(new PlayerNearbyMessage(...));
});
```

**Alternatives Considered**:

- Entity ID → Actor mapping: Rejected - requires Dictionary lookup
- Tag component: Rejected - doesn't store actor reference

---

### Decision: Explicit Entity-Actor Lifecycle Coordination

**Rationale**:

- ECS entity destruction must stop corresponding actor
- Actor crash should mark entity as "AI failed" but not destroy entity
- Bidirectional lifecycle events for coordination

**Implementation**:

```csharp
// ECS → Actor: Entity destroyed
public void DestroyIntelligentEntity(World world, Entity entity)
{
    if (entity.Has<AkkaActorRef>())
    {
        ref var actorRef = ref entity.Get<AkkaActorRef>();
        actorRef.ActorRef.Tell(PoisonPill.Instance);  // Graceful actor shutdown
    }
    world.Destroy(entity);
}

// Actor → ECS: Actor crashed (supervisor stopped it)
protected override void PostStop()
{
    // Publish event so ECS system can handle cleanup
    Context.ActorSelection("/user/event-bus-bridge")
        .Tell(new PublishGameEvent(new ActorStoppedEvent(_entity.Id)));

    base.PostStop();
}
```

**Alternatives Considered**:

- Automatic cleanup: Rejected - race conditions possible
- Keep entity alive after actor crash: Chosen - allows fallback AI

---

### Decision: Optimized Queries with Intelligent AI Tag

**Rationale**:

- Most entities don't have actors (simple enemies)
- Query only entities with `IntelligentAI` component
- Reduces iteration overhead in `AISystem`

**Query Pattern**:

```csharp
// Only process entities with intelligent AI
var intelligentQuery = new QueryDescription()
    .WithAll<Actor, IntelligentAI, AkkaActorRef>();

world.Query(in intelligentQuery, (Entity entity, ref Actor actor, ref IntelligentAI ai) =>
{
    if (actor.CanAct && ai.Capabilities.HasFlag(AICapability.Planning))
    {
        // Process intelligent entity
    }
});
```

**Alternatives Considered**:

- Query all entities with `AI` component: Rejected - includes simple AI
- Separate intelligent entity world: Rejected - complicates rendering queries

---

### Decision: Actor State Separate from ECS World Serialization

**Rationale**:

- Akka.Persistence handles actor state (memories, relationships)
- ECS serialization handles component data (Position, Health, etc.)
- Coordinated save: save ECS → trigger actor snapshots → wait for completion
- On load: restore ECS → recreate actors → restore from snapshots

**Save Flow**:

```csharp
public async Task SaveGameAsync(string savePath)
{
    // 1. Save ECS world state
    await SaveECSWorldAsync(savePath);

    // 2. Trigger actor snapshots
    var snapshotTasks = new List<Task>();
    // ... tell all avatar actors to snapshot
    await Task.WhenAll(snapshotTasks);

    // 3. Confirm save complete
    _logger.LogInformation("Game saved with {Count} intelligent avatars", avatarCount);
}
```

**Alternatives Considered**:

- Single unified save file: Rejected - couples ECS and actor systems
- Actor state in ECS components: Rejected - bloats component size

---

## 4. Event Bus ↔ Actor Communication

### Decision: Bidirectional Bridge Actor Pattern

**Rationale**:

- Event bus and actor system are independent
- Bridge actor subscribes to event bus and forwards to actors
- Actors send to bridge which publishes to event bus
- Decouples event bus from knowing about actors

**Bridge Implementation**:

```csharp
public class EventBusAkkaAdapter : ReceiveActor
{
    private readonly IEventBus _eventBus;

    public EventBusAkkaAdapter(IEventBus eventBus)
    {
        _eventBus = eventBus;

        // Event Bus → Actors
        _eventBus.Subscribe<PlayerMovedEvent>(async evt =>
        {
            Context.ActorSelection("/user/avatars/*")
                .Tell(new PlayerNearbyMessage(evt.NewPosition, evt.VisibleEntities));
        });

        // Actors → Event Bus
        Receive<PublishGameEvent>(msg =>
        {
            _eventBus.PublishAsync(msg.Event);
        });
    }
}
```

**Alternatives Considered**:

- Direct event bus in actors: Rejected - tight coupling
- Mediator pattern: Rejected - bridge simpler for our use case

---

### Decision: Actor Selection for Broadcast, Direct Tell for Targeted

**Rationale**:

- `Context.ActorSelection("/user/avatars/*")` broadcasts to all avatars
- `_specificActor.Tell(msg)` targets specific avatar
- Selection lazy, doesn't fail if no matches
- Direct tell faster for known actors

**Usage Patterns**:

```csharp
// Broadcast: Player moved, notify all nearby NPCs
Context.ActorSelection("/user/avatars/*")
    .Tell(new PlayerNearbyMessage(playerPos, visibleEntities));

// Targeted: Player talks to specific NPC
if (entity.Has<AkkaActorRef>())
{
    ref var actorRef = ref entity.Get<AkkaActorRef>();
    actorRef.ActorRef.Tell(new DialogueRequestMessage(playerInput));
}
```

**Alternatives Considered**:

- Publish-subscribe within Akka: Rejected - reinvents event bus
- Always use selection: Rejected - slower for targeted messages

---

### Decision: Event Ordering via Sequential Event Bus

**Rationale**:

- Existing event bus is sequential (not parallel)
- Guarantees event order (PlayerMoved → EnemyKilledEvent)
- Actors receive events in order published
- No additional ordering logic needed

**Impact**:

- Bridge must handle events sequentially
- Actor tells are async, order preserved per sender

**Alternatives Considered**:

- Parallel event bus: Rejected - breaks ordering guarantees
- Event timestamps: Rejected - not needed with sequential bus

---

### Decision: Minimal Performance Impact via Selective Bridging

**Rationale**:

- Don't forward all events to actor system
- Only bridge events intelligent avatars care about
- Reduces actor mailbox pressure
- Maintains 1.1M+ events/sec bus throughput

**Selective Bridging**:

```csharp
// Only bridge relevant events
_eventBus.Subscribe<PlayerMovedEvent>(ForwardToActors);
_eventBus.Subscribe<EnemyKilledEvent>(ForwardToActors);
_eventBus.Subscribe<NPCInteractionEvent>(ForwardToActors);

// Don't bridge: ItemCollectedEvent, SpellCastEvent, etc.
```

**Alternatives Considered**:

- Bridge all events: Rejected - unnecessary actor messages
- Filter in actors: Rejected - wastes mailbox space

---

## 5. Fallback AI Strategies

### Decision: Timeout Detection with Automatic Fallback

**Rationale**:

- 5-second timeout for LLM responses
- On timeout: immediately use last known behavior or default to Chase
- Log timeout for monitoring
- Player sees brief pause but gameplay continues

**Implementation**:

```csharp
public async Task<AIDecisionMessage> DecideActionAsync(AvatarContext context)
{
    try
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        return await InvokeLLMAsync(context, cts.Token);
    }
    catch (OperationCanceledException)
    {
        _logger.LogWarning("LLM timeout for avatar {Id}, using fallback", context.AvatarId);
        return CreateFallbackDecision(context);
    }
}

private AIDecisionMessage CreateFallbackDecision(AvatarContext context)
{
    // Use personality-appropriate fallback
    var fallback = context.Personality.Contains("aggressive", StringComparison.OrdinalIgnoreCase)
        ? AIBehavior.Chase
        : AIBehavior.Wander;

    return new AIDecisionMessage
    {
        SuggestedBehavior = fallback,
        Reasoning = "[Fallback: LLM timeout]",
        NewEmotionalState = context.EmotionalState,  // Preserve state
        InternalThought = ""
    };
}
```

**Alternatives Considered**:

- Wait indefinitely: Rejected - freezes gameplay
- Aggressive 2s timeout: Rejected - cuts off valid responses

---

### Decision: Polly Circuit Breaker for Sustained Failures

**Rationale**:

- Detect when LLM service is down (3 failures)
- Open circuit for 30 seconds
- All requests use fallback AI during break
- Automatic half-open retry after break

**Circuit Breaker Configuration**:

```csharp
private readonly IAsyncPolicy<AIDecisionMessage> _circuitBreaker = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .FallbackAsync(
        fallbackAction: (context, ct) => Task.FromResult(CreateFallbackDecision(context)),
        onFallbackAsync: (outcome, context) =>
        {
            _logger.LogWarning("Circuit breaker fallback triggered");
            return Task.CompletedTask;
        })
    .WrapAsync(Policy
        .Handle<HttpRequestException>()
        .Or<TimeoutException>()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30)));
```

**Alternatives Considered**:

- No circuit breaker: Rejected - wastes API calls during outages
- Exponential backoff: Rejected - circuit breaker more appropriate

---

### Decision: Decision Caching for Common Scenarios

**Rationale**:

- Cache frequently occurring scenarios (e.g., "player far away → wander")
- Instant response when cached
- Reduces API costs
- LRU eviction (100 entries)

**Cache Strategy**:

```csharp
private string ComputeCacheKey(AvatarContext context)
{
    // Hash: personality + emotional state + player distance bucket
    var distanceBucket = context.PlayerDistance switch
    {
        < 5 => "close",
        < 15 => "medium",
        _ => "far"
    };

    return $"{context.Personality}_{context.EmotionalState}_{distanceBucket}_{context.Health.Percentage:F1}";
}
```

**Alternatives Considered**:

- No caching: Rejected - repetitive API calls expensive
- Exact context caching: Rejected - low hit rate
- Time-based expiry: Considered, LRU sufficient

---

### Decision: Graceful Degradation UI Indication

**Rationale**:

- When using fallback AI, show subtle indicator to player
- Doesn't break immersion (no error messages mid-game)
- Activity log shows "[Boss used instinct]" instead of strategic thought
- Developer console shows full error

**UI Pattern**:

```csharp
if (decision.Reasoning.StartsWith("[Fallback:"))
{
    // User sees: "*The dragon acts on pure instinct*"
    activityLog.Add($"*{npcName} acts on pure instinct*");
}
else
{
    // User sees: "*The dragon's eyes gleam with predatory hunger*"
    activityLog.Add(decision.InternalThought);
}
```

**Alternatives Considered**:

- Show error messages: Rejected - breaks immersion
- Silent fallback: Rejected - player should notice intelligent boss became simple
- Full degradation to simple AI: Chosen approach maintains some personality

---

## 6. Save/Load with Actor State

### Decision: Akka.Persistence Snapshot Strategy

**Rationale**:

- Snapshot after every 10 significant events (combat, dialogue)
- Keep last 2 snapshots (current + previous)
- Snapshot on game save command
- Fast recovery on load (no event replay needed)

**Snapshot Configuration**:

```csharp
public class AvatarActor : ReceivePersistentActor
{
    public override string PersistenceId => $"avatar-{_entity.Id}";

    private int _eventsSinceSnapshot = 0;

    public AvatarActor(Entity entity, ...)
    {
        // Recover state from snapshot
        Recover<SnapshotOffer>(offer =>
        {
            if (offer.Snapshot is AvatarState state)
            {
                _state = state;
                _logger.LogInformation("Recovered avatar {Id} from snapshot", PersistenceId);
            }
        });

        Command<SaveSnapshotSuccess>(success =>
        {
            // Delete old snapshots (keep last 2)
            DeleteSnapshots(new SnapshotSelectionCriteria(success.Metadata.SequenceNr - 2));
        });
    }

    private void OnSignificantEvent()
    {
        _eventsSinceSnapshot++;
        if (_eventsSinceSnapshot >= 10)
        {
            SaveSnapshot(_state);
            _eventsSinceSnapshot = 0;
        }
    }
}
```

**Alternatives Considered**:

- Event sourcing (replay all events): Rejected - slow for long play sessions
- Snapshot on every event: Rejected - I/O overhead
- No snapshots: Rejected - can't restore NPC state

---

### Decision: Coordinated Save Flow with ECS

**Rationale**:

- Game save triggers both ECS and actor persistence
- Wait for all actors to complete snapshots
- Atomic save (both succeed or both fail)
- Save metadata tracks version for migration

**Save Coordination**:

```csharp
public async Task SaveGameAsync(string savePath)
{
    _logger.LogInformation("Starting game save to {Path}", savePath);

    // 1. Get all avatar actors
    var avatarActors = GetAllAvatarActors();

    // 2. Tell each actor to snapshot
    var snapshotTasks = avatarActors.Select(actor =>
        actor.Ask<SaveSnapshotSuccess>(new SaveSnapshotCommand(), TimeSpan.FromSeconds(10))
    ).ToList();

    // 3. Wait for all snapshots
    await Task.WhenAll(snapshotTasks);

    // 4. Save ECS world
    await SaveECSWorldAsync(savePath);

    // 5. Save metadata
    await SaveMetadataAsync(savePath, new SaveMetadata
    {
        Version = "1.0.0",
        Timestamp = DateTime.UtcNow,
        AvatarCount = avatarActors.Count
    });

    _logger.LogInformation("Game saved successfully");
}
```

**Alternatives Considered**:

- ECS save only: Rejected - loses NPC memories
- Fire-and-forget snapshots: Rejected - no completion guarantee

---

### Decision: Actor Recreation on Load with State Restore

**Rationale**:

- On load, recreate actors for entities with `AkkaActorRef` component
- Actors automatically recover from Akka.Persistence snapshots
- Akka.NET PersistenceId matches entity ID for consistency
- Chat histories restored from snapshots

**Load Flow**:

```csharp
public async Task LoadGameAsync(string savePath)
{
    _logger.LogInformation("Loading game from {Path}", savePath);

    // 1. Load ECS world
    await LoadECSWorldAsync(savePath);

    // 2. Query entities with AkkaActorRef components
    var intelligentEntities = QueryIntelligentEntities(_world);

    // 3. Recreate actors (they'll auto-recover from persistence)
    foreach (var entity in intelligentEntities)
    {
        var actorRef = CreateAvatarActor(entity);  // Actor recovers from snapshot

        // Update component with new actor reference
        ref var actorRefComponent = ref entity.Get<AkkaActorRef>();
        actorRefComponent.ActorRef = actorRef;
    }

    _logger.LogInformation("Loaded {Count} intelligent avatars", intelligentEntities.Count);
}
```

**Alternatives Considered**:

- Keep actors alive between loads: Rejected - clean shutdown better
- Manual state restoration: Rejected - Akka.Persistence handles it

---

### Decision: Schema Migration via Versioned Saves

**Rationale**:

- Save file includes version number
- On load, check version and apply migrations
- Backwards compatibility for one previous version
- Forward compatibility not supported (older game can't load newer saves)

**Migration Pattern**:

```csharp
public async Task<AvatarState> MigrateState(AvatarState state, string fromVersion)
{
    return fromVersion switch
    {
        "1.0.0" => state,  // Current version, no migration
        "0.9.0" => MigrateFrom090(state),  // Add new fields
        _ => throw new NotSupportedException($"Cannot load save from version {fromVersion}")
    };
}

private AvatarState MigrateFrom090(AvatarState state)
{
    // v0.9.0 didn't have EmotionalState, default to Neutral
    return state with { EmotionalState = "Neutral" };
}
```

**Alternatives Considered**:

- No versioning: Rejected - breaks on schema changes
- Full backward compatibility: Rejected - maintenance burden

---

## 7. LLM API Integration

### Decision: OpenAI API with gpt-4o-mini for Development

**Rationale**:

- Cost-effective for development ($0.15/1M tokens input)
- Fast response times (avg 1-2s)
- JSON mode support (reliable structured output)
- Good reasoning quality for game AI
- Production can upgrade to gpt-4o for better personality

**Model Selection**:

```json
{
  "SemanticKernel": {
    "ModelId": "gpt-4o-mini",
    "ApiKey": "sk-...",
    "MaxTokens": 300,
    "Temperature": 0.7
  }
}
```

**Alternatives Considered**:

- gpt-3.5-turbo: Rejected - less reliable JSON mode
- gpt-4o: Considered for production (better personality depth)
- Claude 3 Haiku: Considered, OpenAI chosen for SK native support

---

### Decision: Local LLM with Ollama for Offline Development

**Rationale**:

- No API costs during development
- Works offline (airplane coding)
- Faster iteration (no network latency)
- Switch to OpenAI for production
- Use llama3.2:3b model (fast, decent quality)

**Ollama Integration**:

```csharp
var endpoint = configuration["SemanticKernel:LocalEndpoint"];
if (!string.IsNullOrEmpty(endpoint))
{
    builder.AddOpenAIChatCompletion(
        modelId: "llama3.2",
        apiKey: "ollama",  // Ollama doesn't need real key
        endpoint: new Uri(endpoint));  // http://localhost:11434
}
```

**Alternatives Considered**:

- LM Studio: Rejected - Ollama easier CLI setup
- Running OpenAI in dev: Rejected - costs add up during iteration

---

### Decision: Prompt Token Optimization

**Rationale**:

- Limit context to 5 most recent memories (not all 10)
- Compress visible entities (count + types, not full details)
- Use abbreviations in system prompts where clear
- Target <500 tokens per request

**Token Optimization**:

```csharp
private string FormatMemories(List<string> memories)
{
    // Take last 5 memories only
    return string.Join("\n- ", memories.TakeLast(5));
}

private string FormatVisibleEntities(List<Entity> entities)
{
    // Summarize: "3 enemies (Goblin x2, Orc), 1 player, 2 items"
    var groups = entities.GroupBy(e => e.Type);
    return string.Join(", ", groups.Select(g =>
        g.Count() > 1 ? $"{g.Key} x{g.Count()}" : g.Key));
}
```

**Alternatives Considered**:

- Full context every time: Rejected - expensive, slow
- Ultra-compressed context: Rejected - hurts decision quality

---

### Decision: JSON Mode with Structured Output

**Rationale**:

- OpenAI JSON mode guarantees valid JSON
- Reduces parsing errors
- Clear contract between prompt and code
- Easy to validate schema

**JSON Mode Configuration**:

```csharp
var settings = new OpenAIPromptExecutionSettings
{
    ResponseFormat = "json_object",  // Force JSON mode
    MaxTokens = 300,
    Temperature = 0.7,  // Creative but consistent
    TopP = 0.9,
    FrequencyPenalty = 0.0,
    PresencePenalty = 0.0
};

var result = await _kernel.InvokePromptAsync<string>(prompt, new(settings));
var decision = JsonSerializer.Deserialize<AIDecisionResponse>(result);
```

**Alternatives Considered**:

- Natural language parsing: Rejected - unreliable
- Function calling: Considered for future, JSON mode simpler for now

---

## Summary of Key Decisions

| Area | Decision | Rationale |
|------|----------|-----------|
| **Akka.NET Hosting** | Akka.Hosting with Microsoft.Extensions | Seamless DI and lifecycle integration |
| **Actor Supervision** | One-For-One with Restart (3 retries/min) | Isolate failures, preserve actor paths |
| **Persistence** | Akka.Persistence.Sql + SQLite | Lightweight, cross-platform, file-based |
| **Serialization** | System.Text.Json | Native .NET 8, SK compatible |
| **SK Architecture** | Single Kernel + Multiple Agents | Share config, reduce overhead |
| **Prompt Format** | Structured JSON with OpenAI JSON mode | Reliable parsing, clear contracts |
| **Timeout Strategy** | 5s timeout + Polly Circuit Breaker | Graceful degradation, protect against outages |
| **Caching** | LRU cache (100 entries) | Reduce costs, improve latency |
| **ECS Bridge** | Component-based actor references | Fast queries, automatic cleanup |
| **Event Bridge** | Bidirectional bridge actor | Decouple systems, selective forwarding |
| **Fallback AI** | Immediate fallback on timeout/failure | Maintain gameplay, log for monitoring |
| **Save/Load** | Coordinated snapshots (ECS + Akka) | Atomic saves, state preservation |
| **LLM Provider** | OpenAI gpt-4o-mini (prod), Ollama (dev) | Cost-effective, reliable, offline option |

---

## Next Steps

✅ **Phase 0 Complete** - All technology decisions documented

⏭️ **Phase 1**: Proceed to design phase

- Generate `data-model.md` with detailed component and model designs
- Generate `contracts/` with message and interface contracts
- Generate `quickstart.md` with developer onboarding guide
- Update `.agent/adapters/claude.md` with technology context

---

**Research Status**: Complete
**Last Updated**: 2025-10-24
**Reviewed By**: TBD
