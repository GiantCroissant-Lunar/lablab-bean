---
title: Gameplay Systems Quick Reference
date: 2025-10-25
status: Complete
type: quick-reference
tags: [gameplay, api, quick-reference, developers]
---

# Gameplay Systems Quick Reference Guide

**Purpose**: Fast reference for developers using the 7 gameplay systems
**Audience**: Game developers integrating Lablab Bean framework
**Last Updated**: 2025-10-25

---

## 🎮 1. Quest System

### Create a Quest

```csharp
using LablabBean.Plugins.Quest.Services;

// Get service via DI
var questService = serviceProvider.GetRequiredService<IQuestService>();

// Create a quest
var quest = questService.CreateQuest(
    questId: "dragon_slayer",
    title: "Slay the Dragon",
    description: "Defeat the ancient red dragon terrorizing the village",
    level: 10
);

// Add objectives
questService.AddObjective(quest.Id, "kill_dragon", "Defeat the Ancient Red Dragon", 1);
questService.AddObjective(quest.Id, "collect_treasure", "Collect Dragon Hoard", 1);

// Set rewards
questService.SetReward(quest.Id, gold: 1000, experience: 5000);
```

### Track Progress

```csharp
// Accept quest for player
questService.AcceptQuest(playerEntity, quest.Id);

// Update objective progress
questService.UpdateObjectiveProgress(playerEntity, quest.Id, "kill_dragon", 1);

// Complete quest
if (questService.IsQuestComplete(playerEntity, quest.Id))
{
    questService.CompleteQuest(playerEntity, quest.Id);
}
```

---

## 💬 2. NPC/Dialogue System

### Create an NPC

```csharp
using LablabBean.Plugins.NPC.Services;

var npcService = serviceProvider.GetRequiredService<INPCService>();

var npc = npcService.CreateNPC(
    name: "Eldrin the Wise",
    role: "Quest Giver",
    dialogueTreeId: "eldrin_main_dialogue"
);
```

### Create Dialogue Tree

```csharp
var dialogueTree = new DialogueTree
{
    Id = "eldrin_main_dialogue",
    Name = "Eldrin's Greeting",
    StartNodeId = "start",
    Nodes = new Dictionary<string, DialogueNode>
    {
        ["start"] = new DialogueNode
        {
            Id = "start",
            Text = "Greetings, traveler. I have a quest for you.",
            Choices = new List<DialogueChoice>
            {
                new() { Text = "Tell me about the quest", NextNodeId = "quest_info" },
                new() { Text = "Who are you?", NextNodeId = "about" },
                new() { Text = "Goodbye", NextNodeId = "end" }
            }
        }
        // Add more nodes...
    }
};

npcService.LoadDialogueTree(dialogueTree);
```

### Start Conversation

```csharp
npcService.StartConversation(playerEntity, npcEntity);
var currentNode = npcService.GetCurrentDialogueNode(playerEntity, npcEntity);
// Display currentNode.Text and currentNode.Choices to player
```

---

## 📊 3. Character Progression

### Award Experience

```csharp
using LablabBean.Plugins.Progression.Services;

var progressionService = serviceProvider.GetRequiredService<IProgressionService>();

// Award XP (automatically handles leveling)
progressionService.AwardExperience(playerEntity, 1500);
```

### Handle Level Up

```csharp
// Listen for level up events
messageBus.Subscribe<LevelUpEvent>(evt =>
{
    Console.WriteLine($"Congratulations! You reached level {evt.NewLevel}!");
    Console.WriteLine($"You gained {evt.StatPointsAwarded} stat points!");
});
```

### Skill Tree

```csharp
// Unlock a skill
progressionService.UnlockSkill(playerEntity, "fireball");

// Check if skill is available
if (progressionService.HasSkill(playerEntity, "fireball"))
{
    // Player can cast fireball
}
```

---

## ✨ 4. Spell/Ability System

### Register Spells

```csharp
using LablabBean.Plugins.Spells.Services;

var spellService = serviceProvider.GetRequiredService<ISpellService>();

var fireball = new SpellDefinition
{
    Id = "fireball",
    Name = "Fireball",
    ManaCost = 50,
    Cooldown = 3.0f,
    Range = 10f,
    Damage = 75,
    School = SpellSchool.Fire,
    TargetType = SpellTargetType.Enemy,
    AreaOfEffect = 3f
};

spellService.RegisterSpell(fireball);
```

### Cast a Spell

```csharp
// Cast spell on target
if (spellService.CanCast(casterEntity, "fireball"))
{
    spellService.CastSpell(casterEntity, "fireball", targetEntity);
}

// Get cooldown info
var remainingCooldown = spellService.GetRemainingCooldown(casterEntity, "fireball");
```

### Manage Mana

```csharp
// Check mana
var currentMana = spellService.GetCurrentMana(casterEntity);
var maxMana = spellService.GetMaxMana(casterEntity);

// Restore mana
spellService.RestoreMana(casterEntity, 100);
```

---

## 🏪 5. Merchant Trading

### Create a Shop

```csharp
using LablabBean.Plugins.Merchant.Services;

var merchantService = serviceProvider.GetRequiredService<IMerchantService>();

var merchant = merchantService.CreateMerchant(
    name: "Grogar the Trader",
    shopType: ShopType.General
);

// Add items to shop
merchantService.AddItemToShop(merchant.Id, "health_potion", basePrice: 50, stock: 10);
merchantService.AddItemToShop(merchant.Id, "iron_sword", basePrice: 250, stock: 3);
```

### Buy/Sell Items

```csharp
// Buy item from merchant
if (merchantService.CanBuyItem(playerEntity, merchant.Id, "health_potion", quantity: 5))
{
    merchantService.BuyItem(playerEntity, merchant.Id, "health_potion", 5);
}

// Sell item to merchant
merchantService.SellItem(playerEntity, merchant.Id, "old_sword", quantity: 1);
```

### Dynamic Pricing

```csharp
// Get current price (affected by reputation, supply/demand)
var currentPrice = merchantService.GetItemPrice(merchant.Id, "health_potion");

// Improve reputation (lower prices)
merchantService.ImproveReputation(playerEntity, merchant.Id, 10);
```

---

## 🐉 6. Boss Encounter System

### Create a Boss

```csharp
using LablabBean.Plugins.Boss.Systems;

var bossEntity = world.Create(
    new BossComponent
    {
        Name = "Ancient Red Dragon",
        MaxHealth = 10000,
        CurrentHealth = 10000,
        Level = 15,
        IsEnraged = false,
        CurrentPhase = 1
    },
    new BossPhaseComponent
    {
        Phases = new List<BossPhase>
        {
            new BossPhase { PhaseNumber = 1, HealthThreshold = 1.0f, DamageMultiplier = 1.0f },
            new BossPhase { PhaseNumber = 2, HealthThreshold = 0.5f, DamageMultiplier = 1.5f },
            new BossPhase { PhaseNumber = 3, HealthThreshold = 0.25f, DamageMultiplier = 2.0f }
        }
    }
);
```

### Boss Abilities

```csharp
// Boss automatically uses abilities based on phase
// Configure abilities in BossAbilityComponent
var abilityComponent = new BossAbilityComponent
{
    Abilities = new List<BossAbility>
    {
        new BossAbility
        {
            Id = "fire_breath",
            Name = "Fire Breath",
            Cooldown = 10f,
            Damage = 200,
            RequiredPhase = 1
        },
        new BossAbility
        {
            Id = "meteor_storm",
            Name = "Meteor Storm",
            Cooldown = 20f,
            Damage = 500,
            RequiredPhase = 3
        }
    }
};
```

### Handle Boss Defeat

```csharp
// Listen for boss defeat events
messageBus.Subscribe<BossDefeatedEvent>(evt =>
{
    Console.WriteLine($"{evt.BossName} has been defeated!");
    // Award loot, trigger achievements, etc.
});
```

---

## ⚠️ 7. Environmental Hazards & Traps

### Create a Trap

```csharp
using LablabBean.Plugins.Hazards.Services;

var hazardService = serviceProvider.GetRequiredService<IHazardService>();

// Create a spike trap
var trap = hazardService.CreateHazard(
    position: new Position(10, 15),
    type: HazardType.SpikeTrap,
    damage: 50,
    isHidden: true,
    detectionDifficulty: 15
);
```

### Detect and Disarm

```csharp
// Try to detect trap
if (hazardService.TryDetectHazard(playerEntity, trapEntity, perceptionBonus: 5))
{
    Console.WriteLine("You detected a spike trap!");

    // Try to disarm
    if (hazardService.TryDisarmHazard(playerEntity, trapEntity, disarmBonus: 3))
    {
        Console.WriteLine("Trap disarmed successfully!");
    }
}
```

### Environmental Hazards

```csharp
// Create lava pool (ongoing damage)
var lavaPool = hazardService.CreateHazard(
    position: new Position(20, 30),
    type: HazardType.LavaPool,
    damage: 25,
    damageInterval: 1.0f, // Damage every second
    isHidden: false
);

// Create poison gas (applies effect)
var poisonGas = hazardService.CreateHazard(
    position: new Position(5, 8),
    type: HazardType.PoisonGas,
    damage: 10,
    effectDuration: 10f // 10 seconds of poison
);
```

---

## 🎯 Event System (Cross-System Communication)

### Subscribe to Events

```csharp
using MessagePipe;

// Quest events
messageBus.Subscribe<QuestCompletedEvent>(evt =>
{
    // Award achievements, update UI, etc.
});

// Level up events
messageBus.Subscribe<LevelUpEvent>(evt =>
{
    // Show level up UI, play sound, etc.
});

// Spell cast events
messageBus.Subscribe<SpellCastEvent>(evt =>
{
    // Show visual effects, play sounds, etc.
});

// Boss phase change events
messageBus.Subscribe<BossPhaseChangeEvent>(evt =>
{
    // Show warning, change music, etc.
});
```

### Publish Custom Events

```csharp
// Publish custom event
var publisher = serviceProvider.GetRequiredService<IPublisher<CustomEvent>>();
publisher.Publish(new CustomEvent { /* data */ });
```

---

## 🔧 ECS Query Patterns

### Query Entities

```csharp
using Arch.Core;

// Query all entities with specific components
var query = new QueryDescription().WithAll<HealthComponent, PositionComponent>();

world.Query(in query, (Entity entity, ref HealthComponent health, ref PositionComponent pos) =>
{
    // Process each entity
    if (health.Current <= 0)
    {
        // Handle death
    }
});
```

### Get Component

```csharp
// Get component from entity
if (world.TryGet<HealthComponent>(entity, out var health))
{
    Console.WriteLine($"Health: {health.Current}/{health.Max}");
}
```

### Modify Component

```csharp
// Modify component
world.Set(entity, new HealthComponent
{
    Current = 100,
    Max = 100
});
```

---

## 📦 Dependency Injection Setup

### Register Services (in Program.cs)

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register framework services
services.AddLogging();
services.AddSingleton<World>(World.Create());
services.AddMessagePipe();

// Register gameplay services (usually done by plugins)
services.AddSingleton<IQuestService, QuestService>();
services.AddSingleton<INPCService, NPCService>();
services.AddSingleton<IProgressionService, ProgressionService>();
services.AddSingleton<ISpellService, SpellService>();
services.AddSingleton<IMerchantService, MerchantService>();
services.AddSingleton<IHazardService, HazardService>();

var serviceProvider = services.BuildServiceProvider();
```

---

## 🎨 Common Patterns

### Service + System Pattern

```csharp
// Service: High-level API for game developers
public class QuestService : IQuestService
{
    public Quest CreateQuest(...) { /* ... */ }
    public void AcceptQuest(...) { /* ... */ }
}

// System: ECS logic that runs every frame/update
public partial class QuestSystem : BaseSystem<World, float>
{
    public override void Update(in World world, in float deltaTime)
    {
        // Process quest logic
    }
}
```

### Component Pattern

```csharp
// Components are data-only structs
public struct QuestTrackerComponent
{
    public List<string> ActiveQuests;
    public List<string> CompletedQuests;
    public Dictionary<string, int> ObjectiveProgress;
}
```

### Event Pattern

```csharp
// Events are immutable records
public record QuestCompletedEvent(
    Entity Player,
    string QuestId,
    int GoldRewarded,
    int ExperienceRewarded
);
```

---

## 🚀 Getting Started Checklist

- [ ] Review this quick reference
- [ ] Check individual plugin README files for details
- [ ] Set up DI container with required services
- [ ] Create World and register systems
- [ ] Subscribe to relevant events
- [ ] Test each system individually
- [ ] Test cross-system integration
- [ ] Profile performance for your game

---

## 📖 Additional Resources

- **Root README**: Overview and architecture
- **dotnet/README**: Framework details
- **Plugin READMEs**: Detailed documentation per system
- **Phase 10 Summary**: Complete project status
- **Example Projects**: Coming soon!

---

## 💡 Tips & Best Practices

1. **Use Services for Gameplay Code**: Don't manipulate ECS directly unless necessary
2. **Subscribe to Events**: Loosely coupled systems are easier to maintain
3. **Leverage DI**: Let the container manage dependencies
4. **Profile Early**: Test performance with realistic entity counts
5. **Read the Source**: Services and systems are well-documented with XML comments

---

**Last Updated**: 2025-10-25
**Version**: 1.0
**Framework Version**: Lablab Bean v0.1.0 (Phase 10 Complete)
