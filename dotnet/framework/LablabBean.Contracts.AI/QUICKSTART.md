# Quick Start: Using Memory Service

## Setup (Program.cs / Startup)

```csharp
using LablabBean.Contracts.AI.Extensions;

// Add memory service to DI container
builder.Services.AddKernelMemoryService(builder.Configuration);
```

## Basic Usage

### 1. Store a Memory

```csharp
using LablabBean.Contracts.AI.Memory;

public class NpcDialogueHandler
{
    private readonly IMemoryService _memoryService;

    public NpcDialogueHandler(IMemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    public async Task HandlePlayerResponse(string playerId, string npcId, string response)
    {
        var memory = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Content = $"Player responded: {response}",
            EntityId = playerId,
            MemoryType = "conversation",
            Importance = 0.7, // 0.0 - 1.0
            Tags = new Dictionary<string, string>
            {
                { "npc_id", npcId },
                { "dialogue_type", "response" }
            }
        };

        await _memoryService.StoreMemoryAsync(memory);
    }
}
```

### 2. Retrieve Relevant Memories

```csharp
public async Task<string> GenerateNpcResponse(string playerId, string npcId, string context)
{
    // Find relevant past interactions
    var memories = await _memoryService.RetrieveRelevantMemoriesAsync(
        query: "previous conversations with this player",
        options: new MemoryRetrievalOptions
        {
            EntityId = playerId,
            MemoryType = "conversation",
            MinRelevanceScore = 0.6,
            MinImportance = 0.5,
            Limit = 5,
            Tags = new Dictionary<string, string> { { "npc_id", npcId } }
        }
    );

    // Use memories to inform NPC response
    foreach (var memory in memories)
    {
        Console.WriteLine($"Relevance: {memory.RelevanceScore:P}");
        Console.WriteLine($"Memory: {memory.Memory.Content}");
    }

    // Generate contextual response...
}
```

### 3. Update Memory Importance

```csharp
// Player completed a major quest - increase importance of related memories
await _memoryService.UpdateMemoryImportanceAsync(
    memoryId: "mem-quest-123",
    importance: 0.95
);
```

### 4. Clean Up Old Memories

```csharp
// Remove memories older than 30 days
var deletedCount = await _memoryService.CleanOldMemoriesAsync(
    entityId: playerId,
    olderThan: TimeSpan.FromDays(30)
);

Console.WriteLine($"Cleaned {deletedCount} old memories");
```

## Common Patterns

### Pattern 1: Dual-Write for Game Events

```csharp
public async Task OnCombatAction(CombatEvent evt)
{
    // 1. Execute game logic
    await _combatSystem.ProcessAction(evt);

    // 2. Store memory (fire-and-forget for performance)
    _ = Task.Run(async () =>
    {
        await _memoryService.StoreMemoryAsync(new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Content = $"Player used {evt.AbilityName} against {evt.TargetId}",
            EntityId = evt.PlayerId,
            MemoryType = "combat",
            Importance = evt.Outcome == "success" ? 0.8 : 0.6,
            Tags = new Dictionary<string, string>
            {
                { "ability", evt.AbilityName },
                { "outcome", evt.Outcome }
            }
        });
    });
}
```

### Pattern 2: Context-Aware NPC Decisions

```csharp
public async Task<MerchantPricing> CalculatePrices(string playerId)
{
    // Check player's relationship history
    var memories = await _memoryService.RetrieveRelevantMemoriesAsync(
        query: "player helped merchant or was generous",
        options: new MemoryRetrievalOptions
        {
            EntityId = playerId,
            MemoryType = "relationship",
            MinRelevanceScore = 0.7,
            Limit = 10
        }
    );

    // Calculate discount based on positive interactions
    var positiveMemories = memories.Count(m => m.Memory.Importance > 0.7);
    var discount = Math.Min(0.25f, positiveMemories * 0.05f);

    return new MerchantPricing { Discount = discount };
}
```

### Pattern 3: Learning Player Tactics

```csharp
public async Task<BossStrategy> AdaptBossStrategy(string playerId, BossEncounter encounter)
{
    // Retrieve player's combat patterns
    var combatMemories = await _memoryService.RetrieveRelevantMemoriesAsync(
        query: "player combat tactics and patterns",
        options: new MemoryRetrievalOptions
        {
            EntityId = playerId,
            MemoryType = "combat",
            MinImportance = 0.6,
            FromTimestamp = DateTimeOffset.UtcNow.AddHours(-2), // Recent session
            Limit = 20
        }
    );

    // Analyze patterns
    var isAggressive = combatMemories.Count(m => m.Memory.Tags.ContainsKey("aggressive")) > 10;
    var usesStealth = combatMemories.Any(m => m.Memory.Tags.ContainsKey("stealth"));

    // Adapt boss behavior
    if (isAggressive)
    {
        return BossStrategy.Defensive;
    }
    else if (usesStealth)
    {
        return BossStrategy.AreaDenial;
    }

    return BossStrategy.Balanced;
}
```

## Memory Types Best Practices

| Memory Type | Use Case | Importance Range |
|-------------|----------|------------------|
| `conversation` | NPC dialogues, player choices | 0.5 - 0.9 |
| `combat` | Battle actions, tactics | 0.6 - 0.9 |
| `relationship` | Trust, reputation changes | 0.7 - 1.0 |
| `observation` | Ambient world interactions | 0.3 - 0.6 |
| `tactical` | Strategic decisions | 0.7 - 0.9 |
| `quest` | Quest milestones | 0.8 - 1.0 |

## Performance Tips

1. **Fire-and-forget for non-critical memories**:

   ```csharp
   _ = Task.Run(async () => await StoreMemoryAsync(memory));
   ```

2. **Use appropriate limits**:

   ```csharp
   Limit = 5  // For real-time decisions
   Limit = 20 // For analysis
   ```

3. **Filter by importance** to reduce noise:

   ```csharp
   MinImportance = 0.7  // Only significant memories
   ```

4. **Use time filters** for recent context:

   ```csharp
   FromTimestamp = DateTimeOffset.UtcNow.AddHours(-1)
   ```

## Testing Your Integration

```csharp
[Fact]
public async Task NPC_RemembersPlayerChoice()
{
    // Arrange
    var memory = new MemoryEntry
    {
        Id = "test-mem",
        Content = "Player saved the merchant from bandits",
        EntityId = "player-test",
        MemoryType = "heroic_action",
        Importance = 0.9
    };

    // Act
    await _memoryService.StoreMemoryAsync(memory);

    var retrieved = await _memoryService.RetrieveRelevantMemoriesAsync(
        "merchant rescue",
        new MemoryRetrievalOptions
        {
            EntityId = "player-test",
            Limit = 1
        }
    );

    // Assert
    Assert.Single(retrieved);
    Assert.Contains("merchant", retrieved.First().Memory.Content);
}
```

---

**For full API documentation, see `IMemoryService.cs`**
**For implementation details, see `PHASE3_SUMMARY.md`**
