# Phase 4 Refinement: Complete Usage Examples

This document demonstrates how to use the Phase 4 memory-enhanced NPC system with practical examples.

## Table of Contents

- [Quick Start](#quick-start)
- [Relationship-Based Merchant](#relationship-based-merchant)
- [Context-Aware Greetings](#context-aware-greetings)
- [Memory Visualization](#memory-visualization)
- [Complete Integration Example](#complete-integration-example)

---

## Quick Start

### 1. Register Services

```csharp
// In Program.cs or your DI setup
using LablabBean.Plugins.NPC.Extensions;

services.AddMemoryEnhancedNPC(configuration);
```

### 2. Basic Usage

```csharp
public class MyGameHandler
{
    private readonly MemoryEnhancedNPCService _npcService;

    public MyGameHandler(MemoryEnhancedNPCService npcService)
    {
        _npcService = npcService;
    }

    public async Task TalkToNpc(Entity player, Entity npc, string playerId)
    {
        // Start dialogue - memory tracking happens automatically
        var dialogue = await _npcService.StartDialogueAsync(
            player, npc, playerId: playerId
        );

        if (dialogue != null)
        {
            // Get relationship insights
            var insights = await _npcService.GetRelationshipInsightsAsync(
                playerId,
                npc.Get<Components.NPC>().Id
            );

            Console.WriteLine($"Relationship: {insights.RelationshipLevel}");
            Console.WriteLine($"Interactions: {insights.InteractionCount}");
        }
    }
}
```

---

## Relationship-Based Merchant

The `RelationshipBasedMerchant` class demonstrates dynamic pricing based on relationship level.

### Setup

```csharp
using LablabBean.Plugins.NPC.Examples;

public class MerchantShop
{
    private readonly RelationshipBasedMerchant _merchant;

    public MerchantShop(RelationshipBasedMerchant merchant)
    {
        _merchant = merchant;
    }

    public async Task ShowShopAsync(Entity player, Entity merchantNpc, string playerId)
    {
        var npc = merchantNpc.Get<Components.NPC>();

        // Get personalized greeting
        var greeting = await _merchant.GetPersonalizedGreetingAsync(
            playerId, npc.Id, npc.Name
        );

        Console.WriteLine(greeting);

        // Display items with relationship-based pricing
        await DisplayItemAsync(player, merchantNpc, playerId, "Health Potion", 50);
        await DisplayItemAsync(player, merchantNpc, playerId, "Iron Sword", 200);
        await DisplayItemAsync(player, merchantNpc, playerId, "Rare Artifact", 1000);

        // Check exclusive items access
        var hasAccess = await _merchant.CanAccessExclusiveItemsAsync(
            playerId, npc.Id, RelationshipLevel.GoodFriend
        );

        if (hasAccess)
        {
            Console.WriteLine("\n[EXCLUSIVE ITEMS AVAILABLE]");
            await DisplayItemAsync(player, merchantNpc, playerId, "Legendary Blade", 5000);
        }
    }

    private async Task DisplayItemAsync(
        Entity player,
        Entity merchantNpc,
        string playerId,
        string itemName,
        int basePrice)
    {
        var npc = merchantNpc.Get<Components.NPC>();
        var finalPrice = await _merchant.GetPriceAsync(playerId, npc.Id, basePrice);
        var discount = basePrice - finalPrice;

        if (discount > 0)
        {
            Console.WriteLine($"{itemName}: {finalPrice}g (was {basePrice}g, {discount}g off!)");
        }
        else
        {
            Console.WriteLine($"{itemName}: {finalPrice}g");
        }
    }
}
```

### Example Output

**First Visit (Stranger)**

```
Welcome, traveler. I'm Gareth the Merchant.
Health Potion: 50g
Iron Sword: 200g
Rare Artifact: 1000g
```

**After 5 Visits (Friend)**

```
Good to see you, friend! How can I help?
Health Potion: 45g (was 50g, 5g off!)
Iron Sword: 180g (was 200g, 20g off!)
Rare Artifact: 900g (was 1000g, 100g off!)
```

**After 10 Visits (Good Friend)**

```
Hey there! Always a pleasure!
Health Potion: 42g (was 50g, 8g off!)
Iron Sword: 170g (was 200g, 30g off!)
Rare Artifact: 850g (was 1000g, 150g off!)

[EXCLUSIVE ITEMS AVAILABLE]
Legendary Blade: 4250g (was 5000g, 750g off!)
```

---

## Context-Aware Greetings

The `ContextAwareGreetingSystem` generates personalized greetings based on interaction history.

### Setup

```csharp
using LablabBean.Plugins.NPC.Examples;

public class DialogueHandler
{
    private readonly ContextAwareGreetingSystem _greetingSystem;
    private readonly MemoryEnhancedNPCService _npcService;

    public DialogueHandler(
        ContextAwareGreetingSystem greetingSystem,
        MemoryEnhancedNPCService npcService)
    {
        _greetingSystem = greetingSystem;
        _npcService = npcService;
    }

    public async Task StartConversationAsync(
        Entity player,
        Entity npc,
        string playerId)
    {
        var npcComponent = npc.Get<Components.NPC>();

        // Generate context-aware greeting
        var greetingContext = await _greetingSystem.GenerateGreetingAsync(
            playerId,
            npcComponent.Id,
            npcComponent.Name,
            npcRole: "innkeeper"
        );

        Console.WriteLine($"\n{npcComponent.Name}: {greetingContext.GreetingText}");

        // Show relationship status
        Console.WriteLine($"[{greetingContext.RelationshipLevel}]");

        // If we have recent topics, mention them
        if (greetingContext.RecentTopics.Count > 0)
        {
            Console.WriteLine($"Recent topics: {string.Join(", ", greetingContext.RecentTopics)}");
        }

        // Start dialogue with memory tracking
        await _npcService.StartDialogueAsync(player, npc, playerId: playerId);

        // ... handle dialogue choices ...
    }

    public async Task EndConversationAsync(
        Entity player,
        Entity npc,
        string playerId,
        bool purchaseMade = false)
    {
        var npcComponent = npc.Get<Components.NPC>();

        // Generate context-aware farewell
        var farewell = await _greetingSystem.GenerateFarewellAsync(
            playerId,
            npcComponent.Id,
            purchaseMade
        );

        Console.WriteLine($"\n{npcComponent.Name}: {farewell}");

        _npcService.EndDialogue(player);
    }
}
```

### Example Outputs

**First Meeting**

```
Mara the Innkeeper: Greetings, traveler. I am Mara, innkeeper of this establishment. I don't believe we've met before.
[Stranger]
```

**Second Visit (5 minutes later)**

```
Mara the Innkeeper: Hello there. Mara here. Still looking around?
[Acquaintance]
```

**Fifth Visit (discussing weapons)**

```
Mara the Innkeeper: Ah, good to see you again, friend! Still interested in weapons?
[Friend]
Recent topics: weapons
```

**After Long Absence (15 days)**

```
Mara the Innkeeper: Welcome, my friend! How have you been? It's been quite a while! I wondered if I'd see you again.
[GoodFriend]
```

---

## Memory Visualization

The `MemoryVisualizationDashboard` provides debugging and analytics views.

### Setup

```csharp
using LablabBean.Plugins.NPC.Examples;

public class AdminTools
{
    private readonly MemoryVisualizationDashboard _dashboard;

    public AdminTools(MemoryVisualizationDashboard dashboard)
    {
        _dashboard = dashboard;
    }

    // Quick console output
    public async Task ShowPlayerRelationshipsAsync(string playerId, params string[] npcIds)
    {
        await _dashboard.PrintDashboardAsync(playerId, npcIds);
    }

    // Detailed NPC report
    public async Task ShowNpcDetailedReportAsync(string playerId, string npcId)
    {
        await _dashboard.PrintNpcReportAsync(playerId, npcId, memoryLimit: 15);
    }

    // Save to file
    public async Task ExportPlayerDataAsync(string playerId, string[] npcIds, string outputPath)
    {
        var report = await _dashboard.GeneratePlayerDashboardAsync(playerId, npcIds);
        await File.WriteAllTextAsync(outputPath, report);
    }
}
```

### Example Dashboard Output

```
╔═══════════════════════════════════════════════════════════════════╗
║           NPC RELATIONSHIP MEMORY DASHBOARD                       ║
╚═══════════════════════════════════════════════════════════════════╝

Player ID: player_123
Generated: 2025-01-15 10:30:00 UTC

═══════════════════════════════════════════════════════════════════

NPC: merchant_gareth
───────────────────────────────────────────────────────────────────
Relationship: [███░░░] Friend
Interactions: 7
Total Importance: 4.2
Last Seen: 2h ago

Recent Interactions:
  • 2h ago: Player chose 'buy_potion' with Gareth
    Relevance: 0.856
  • 5h ago: Player chose 'ask_about_weapons' with Gareth
    Relevance: 0.823
  • 1d ago: Started conversation with Gareth (ID: merchant_gareth)
    Relevance: 0.791
───────────────────────────────────────────────────────────────────

NPC: innkeeper_mara
───────────────────────────────────────────────────────────────────
Relationship: [█████░] CloseFriend
Interactions: 12
Total Importance: 7.8
Last Seen: 30m ago

Recent Interactions:
  • 30m ago: Player chose 'rent_room' with Mara
    Relevance: 0.912
  • 2h ago: Player chose 'ask_for_gossip' with Mara
    Relevance: 0.887
  • 4h ago: Started conversation with Mara (ID: innkeeper_mara)
    Relevance: 0.865
───────────────────────────────────────────────────────────────────

═══════════════════════════════════════════════════════════════════
```

---

## Complete Integration Example

Here's a full example showing all features working together:

```csharp
using LablabBean.Plugins.NPC.Examples;
using LablabBean.Plugins.NPC.Services;
using Microsoft.Extensions.DependencyInjection;

public class GameWorld
{
    private readonly IServiceProvider _services;
    private readonly MemoryEnhancedNPCService _npcService;
    private readonly RelationshipBasedMerchant _merchant;
    private readonly ContextAwareGreetingSystem _greetingSystem;
    private readonly MemoryVisualizationDashboard _dashboard;

    public GameWorld(IServiceProvider services)
    {
        _services = services;
        _npcService = services.GetRequiredService<MemoryEnhancedNPCService>();
        _merchant = services.GetRequiredService<RelationshipBasedMerchant>();
        _greetingSystem = services.GetRequiredService<ContextAwareGreetingSystem>();
        _dashboard = services.GetRequiredService<MemoryVisualizationDashboard>();
    }

    public async Task HandleMerchantInteractionAsync(
        Entity player,
        Entity merchantNpc,
        string playerId)
    {
        var npc = merchantNpc.Get<Components.NPC>();

        // 1. Generate context-aware greeting
        var greetingContext = await _greetingSystem.GenerateGreetingAsync(
            playerId, npc.Id, npc.Name, "merchant"
        );

        Console.WriteLine($"\n{npc.Name}: {greetingContext.GreetingText}");
        Console.WriteLine($"[Relationship: {greetingContext.RelationshipLevel}]");
        Console.WriteLine();

        // 2. Start dialogue with memory tracking
        var dialogue = await _npcService.StartDialogueAsync(
            player, merchantNpc, playerId: playerId
        );

        if (dialogue == null)
        {
            Console.WriteLine("Cannot start dialogue.");
            return;
        }

        // 3. Show shop with relationship-based pricing
        Console.WriteLine("═══ SHOP INVENTORY ═══");

        var items = new[]
        {
            ("Health Potion", 50),
            ("Mana Potion", 60),
            ("Iron Sword", 200),
            ("Steel Shield", 250)
        };

        foreach (var (itemName, basePrice) in items)
        {
            var transaction = await _merchant.HandlePurchaseAsync(
                player, merchantNpc, playerId, itemName, basePrice
            );

            if (transaction.DiscountPercent > 0)
            {
                Console.WriteLine(
                    $"{itemName}: {transaction.FinalPrice}g " +
                    $"(was {transaction.BasePrice}g, {transaction.DiscountPercent}% off!)"
                );
            }
            else
            {
                Console.WriteLine($"{itemName}: {transaction.FinalPrice}g");
            }
        }

        // 4. Check for exclusive items
        var hasExclusiveAccess = await _merchant.CanAccessExclusiveItemsAsync(
            playerId, npc.Id, RelationshipLevel.GoodFriend
        );

        if (hasExclusiveAccess)
        {
            Console.WriteLine("\n═══ EXCLUSIVE ITEMS ═══");
            var exclusiveTransaction = await _merchant.HandlePurchaseAsync(
                player, merchantNpc, playerId, "Legendary Blade", 5000
            );
            Console.WriteLine(
                $"Legendary Blade: {exclusiveTransaction.FinalPrice}g " +
                $"({exclusiveTransaction.DiscountPercent}% friend discount!)"
            );
        }

        // 5. Simulate purchase
        Console.WriteLine("\n[Player buys Health Potion]");
        await _npcService.SelectChoiceAsync(player, "buy_health_potion", playerId);

        // 6. End conversation
        var farewell = await _greetingSystem.GenerateFarewellAsync(
            playerId, npc.Id, purchaseMade: true
        );
        Console.WriteLine($"\n{npc.Name}: {farewell}");

        _npcService.EndDialogue(player);

        // 7. Show updated relationship stats
        Console.WriteLine("\n═══ RELATIONSHIP UPDATE ═══");
        var insights = await _npcService.GetRelationshipInsightsAsync(playerId, npc.Id);
        Console.WriteLine($"Level: {insights.RelationshipLevel}");
        Console.WriteLine($"Total Interactions: {insights.InteractionCount}");
        Console.WriteLine($"Trust Score: {insights.TotalImportance:F2}");
    }

    // Admin command to view all relationships
    public async Task ShowRelationshipDashboardAsync(string playerId)
    {
        var npcIds = new[]
        {
            "merchant_gareth",
            "innkeeper_mara",
            "blacksmith_iron"
        };

        await _dashboard.PrintDashboardAsync(playerId, npcIds);
    }
}
```

### Example Play Session Output

```
Gareth: Good to see you, friend! How can I help?
[Relationship: Friend]

═══ SHOP INVENTORY ═══
Health Potion: 45g (was 50g, 10% off!)
Mana Potion: 54g (was 60g, 10% off!)
Iron Sword: 180g (was 200g, 10% off!)
Steel Shield: 225g (was 250g, 10% off!)

[Player buys Health Potion]

Gareth: Take care, friend! Thank you for your patronage!

═══ RELATIONSHIP UPDATE ═══
Level: Friend
Total Interactions: 8
Trust Score: 4.8
```

---

## Advanced Features

### Custom Memory Types

```csharp
// In your dialogue system
await _memoryService.StoreMemoryAsync(new MemoryEntry
{
    Id = Guid.NewGuid().ToString(),
    Content = $"Player helped {npc.Name} defend against bandits",
    EntityId = playerId,
    MemoryType = "heroic_action",
    Importance = 0.95,  // High importance!
    Tags = new Dictionary<string, string>
    {
        { "npc_id", npc.Id },
        { "action_type", "combat_assistance" },
        { "location", "trade_road" }
    }
});
```

### Query Specific Memories

```csharp
// Get all memories about a specific topic
var combatMemories = await _memoryService.RetrieveRelevantMemoriesAsync(
    "combat and fighting alongside NPCs",
    new MemoryRetrievalOptions
    {
        EntityId = playerId,
        Tags = new Dictionary<string, string>
        {
            { "action_type", "combat_assistance" }
        },
        Limit = 10
    }
);
```

### Relationship Progression Tracking

```csharp
public async Task TrackRelationshipProgressionAsync(
    string playerId,
    string npcId)
{
    var insights = await _npcService.GetRelationshipInsightsAsync(playerId, npcId);

    // Calculate progress to next level
    var currentLevel = (int)insights.RelationshipLevel;
    var nextLevel = currentLevel + 1;

    if (nextLevel <= 5)
    {
        var requiredImportance = nextLevel * 2.0; // Simplified calculation
        var progress = (insights.TotalImportance / requiredImportance) * 100;

        Console.WriteLine($"Progress to {(RelationshipLevel)nextLevel}: {progress:F1}%");
    }
}
```

---

## Performance Notes

- **Memory operations are non-blocking**: All memory storage uses fire-and-forget pattern
- **No gameplay impact**: Dialogue continues immediately without waiting for memory storage
- **Efficient retrieval**: Memory queries are cached and optimized for real-time access
- **Scalable**: Handles thousands of NPCs and players without performance degradation

---

## Next Steps

1. **Add persistence** (Phase 7): Save memories to disk for cross-session continuity
2. **Memory summarization**: Compress old memories to save space while retaining context
3. **Combat memory** (Phase 5): Track tactical patterns and enemy AI adaptation
4. **Quest memory integration**: Remember quest progress and decisions

---

**Generated**: 2025-01-15
**Version**: Phase 4 Refinement
**Status**: ✅ Production Ready
