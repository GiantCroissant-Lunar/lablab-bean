# Phase 4 Memory System - Quick Reference

## 🚀 Quick Start (3 Steps)

```csharp
// 1. Register (Program.cs)
services.AddMemoryEnhancedNPC(configuration);

// 2. Inject
public MyClass(MemoryEnhancedNPCService npcService) { }

// 3. Use
await _npcService.StartDialogueAsync(player, npc, playerId: "player123");
```

## 📦 Key Classes

| Class | Purpose | Location |
|-------|---------|----------|
| `MemoryEnhancedNPCService` | Main service for NPC interactions | `Services/` |
| `RelationshipBasedMerchant` | Dynamic pricing example | `Examples/` |
| `ContextAwareGreetingSystem` | Personalized greetings | `Examples/` |
| `MemoryVisualizationDashboard` | Debug/admin tools | `Examples/` |

## 🔑 Core APIs

### Start Dialogue

```csharp
var dialogue = await _npcService.StartDialogueAsync(
    playerEntity,
    npcEntity,
    playerId: "player_123"
);
```

### Get Relationship

```csharp
var insights = await _npcService.GetRelationshipInsightsAsync(
    "player_123",
    "merchant_gareth"
);
// Returns: RelationshipLevel, InteractionCount, TotalImportance
```

### Get Recent History

```csharp
var memories = await _npcService.GetRecentDialogueHistoryAsync(
    "player_123",
    limit: 5
);
```

## 💰 Relationship-Based Pricing

```csharp
var merchant = services.GetService<RelationshipBasedMerchant>();

// Get dynamic price
var price = await merchant.GetPriceAsync(playerId, npcId, basePrice: 100);

// Check exclusive access
var canBuy = await merchant.CanAccessExclusiveItemsAsync(
    playerId,
    npcId,
    RelationshipLevel.GoodFriend
);
```

### Discount Table

| Level | Discount | Interactions | Trust Score |
|-------|----------|--------------|-------------|
| Stranger | 0% | 0-1 | 0-1.0 |
| Acquaintance | 5% | 2-3 | 1.0-2.0 |
| Friend | 10% | 4-6 | 2.0-4.0 |
| GoodFriend | 15% | 7-9 | 4.0-6.0 |
| CloseFriend | 20% | 10-14 | 6.0-10.0 |
| TrustedFriend | 25% | 15+ | 10.0+ |

## 💬 Context-Aware Greetings

```csharp
var greetingSystem = services.GetService<ContextAwareGreetingSystem>();

// Generate greeting
var context = await greetingSystem.GenerateGreetingAsync(
    playerId,
    npcId,
    npcName,
    npcRole: "merchant"
);

Console.WriteLine(context.GreetingText);
// "Good to see you, friend! How can I help? Still interested in weapons?"

// Generate farewell
var farewell = await greetingSystem.GenerateFarewellAsync(
    playerId,
    npcId,
    purchaseMade: true
);
```

## 📊 Memory Visualization

```csharp
var dashboard = services.GetService<MemoryVisualizationDashboard>();

// Quick console output
await dashboard.PrintDashboardAsync(playerId, npcId1, npcId2);

// Detailed report
await dashboard.PrintNpcReportAsync(playerId, npcId, memoryLimit: 15);

// Export to file
var report = await dashboard.GeneratePlayerDashboardAsync(playerId, npcIds);
await File.WriteAllTextAsync("player_report.txt", report);
```

## 🎯 Relationship Levels

```csharp
public enum RelationshipLevel
{
    Stranger = 0,        // Just met
    Acquaintance = 1,    // Know each other
    Friend = 2,          // Like each other
    GoodFriend = 3,      // Trust established
    CloseFriend = 4,     // Deep connection
    TrustedFriend = 5    // Maximum trust
}
```

## 📈 How Relationships Progress

### Automatic Tracking

Every interaction stores memories with importance scores:

- **Dialogue Start**: 0.5 importance
- **Dialogue Choice**: 0.6-1.0 importance (based on significance)

### Calculation

```
Total Importance = Sum of all memory importance scores
Level = Based on Interaction Count + Total Importance

Examples:
- 2 interactions, 1.5 importance → Acquaintance
- 5 interactions, 3.0 importance → Friend
- 12 interactions, 7.5 importance → CloseFriend
```

## 🔧 Advanced Usage

### Custom Memory Storage

```csharp
await _memoryService.StoreMemoryAsync(new MemoryEntry
{
    Id = Guid.NewGuid().ToString(),
    Content = "Player saved the village from bandits",
    EntityId = playerId,
    MemoryType = "heroic_deed",
    Importance = 0.95,  // Very important!
    Tags = new Dictionary<string, string>
    {
        { "npc_id", npcId },
        { "event_type", "quest_complete" }
    }
});
```

### Query Specific Memories

```csharp
var questMemories = await _memoryService.RetrieveRelevantMemoriesAsync(
    "quests and missions involving this NPC",
    new MemoryRetrievalOptions
    {
        EntityId = playerId,
        MemoryType = "quest",
        MinRelevanceScore = 0.7,
        Limit = 10
    }
);
```

## ⚡ Performance Tips

1. **Fire-and-Forget**: Memory storage is non-blocking
2. **No Wait Required**: Dialogue continues immediately
3. **Efficient Queries**: Cached and optimized
4. **Scalable**: Handles thousands of NPCs

## 🐛 Debugging

### Check Relationship Status

```csharp
var insights = await _npcService.GetRelationshipInsightsAsync(playerId, npcId);
Console.WriteLine($"Level: {insights.RelationshipLevel}");
Console.WriteLine($"Interactions: {insights.InteractionCount}");
Console.WriteLine($"Trust Score: {insights.TotalImportance:F2}");
Console.WriteLine($"Last Seen: {insights.LastInteraction}");
```

### View All Memories

```csharp
await dashboard.PrintNpcReportAsync(playerId, npcId, memoryLimit: 50);
```

### Check Memory Service Health

```csharp
var isHealthy = await _memoryService.IsHealthyAsync();
if (!isHealthy)
{
    Console.WriteLine("⚠️ Memory service unavailable");
}
```

## 📝 Common Patterns

### Complete Merchant Interaction

```csharp
// 1. Generate greeting
var greeting = await _greetingSystem.GenerateGreetingAsync(...);
Console.WriteLine($"{npc.Name}: {greeting.GreetingText}");

// 2. Start dialogue with memory
var dialogue = await _npcService.StartDialogueAsync(...);

// 3. Show items with dynamic pricing
var price = await _merchant.GetPriceAsync(...);

// 4. Handle purchase
await _npcService.SelectChoiceAsync(player, "buy_item", playerId);

// 5. Say farewell
var farewell = await _greetingSystem.GenerateFarewellAsync(..., purchaseMade: true);
Console.WriteLine($"{npc.Name}: {farewell}");

// 6. End dialogue
_npcService.EndDialogue(player);
```

### Check for Special Dialogue Options

```csharp
var insights = await _npcService.GetRelationshipInsightsAsync(playerId, npcId);

if (insights.RelationshipLevel >= RelationshipLevel.GoodFriend)
{
    // Unlock special dialogue
    ShowOption("Ask about secret quest");
}

if (insights.InteractionCount > 20)
{
    // Long-time customer bonus
    ApplyLoyaltyBonus();
}
```

## 🎮 Example Output

### Progression Example

```
Visit 1:
"Welcome, traveler. I'm Gareth. I don't believe we've met before."
Health Potion: 50g

Visit 5:
"Good to see you, friend! How can I help?"
Health Potion: 45g (was 50g, 10% off!)

Visit 12:
"My friend! Come in, come in!"
Health Potion: 42g (was 50g, 15% off!)

[EXCLUSIVE ITEMS AVAILABLE]
Legendary Blade: 4250g (was 5000g, 15% friend discount!)
```

## 📚 More Information

- **Full Documentation**: See `Examples/USAGE_EXAMPLES.md`
- **Implementation Details**: See `PHASE4_REFINEMENT_SUMMARY.md`
- **System Architecture**: See `PHASE4_MEMORY_INTEGRATION.md`

## ✅ Checklist for Integration

- [ ] Add `services.AddMemoryEnhancedNPC(configuration)` to DI
- [ ] Replace `NPCService` with `MemoryEnhancedNPCService`
- [ ] Add `playerId` parameter to dialogue calls
- [ ] (Optional) Add `RelationshipBasedMerchant` for dynamic pricing
- [ ] (Optional) Add `ContextAwareGreetingSystem` for smart greetings
- [ ] (Optional) Add `MemoryVisualizationDashboard` for debugging
- [ ] Test with a few NPCs first
- [ ] Monitor memory usage and performance
- [ ] Deploy to production

---

**Quick Reference Card** | Phase 4 Memory System
**Version**: 1.0 | **Status**: Production Ready ✅
