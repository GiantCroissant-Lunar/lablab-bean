# Quick Start: Core Gameplay Systems

**Feature**: Core Gameplay Systems (Quest, NPC/Dialogue, Progression, Spells, Merchant, Boss, Hazards)
**Audience**: Developers implementing or extending these systems
**Date**: 2025-10-23

## Overview

This guide provides quick-start instructions for working with the seven new gameplay systems. All systems follow the plugin architecture with ECS components, systems, and services.

---

## Plugin Architecture Pattern

All seven gameplay systems follow this structure:

```
LablabBean.Plugins.{SystemName}/
├── Components/          # ECS component structs
├── Systems/             # ECS system classes (process components)
├── Services/            # High-level service APIs (exposed via IPluginContext)
├── Data/                # JSON data files (quests, dialogue, spells, etc.)
├── Plugin.cs            # IPlugin implementation (entry point)
└── plugin.json          # Plugin manifest
```

**Key Concepts**:

- **Components**: Data-only structs attached to entities
- **Systems**: Logic that processes entities with specific component combinations
- **Services**: High-level APIs registered in DI container, accessible via `IPluginContext.GetService<T>()`

---

## 1. Quest System

### Adding a New Quest

**Step 1**: Define quest data in JSON

```json
// Data/Quests/retrieve-artifact.json
{
  "id": "quest-001-artifact",
  "name": "The Lost Artifact",
  "description": "Retrieve the magical artifact from level 5 and return it to Eldrin.",
  "questGiverId": "npc-eldrin",
  "objectives": [
    {
      "type": "ReachLocation",
      "target": "level-5",
      "required": 1
    },
    {
      "type": "CollectItems",
      "target": "item-artifact",
      "required": 1
    }
  ],
  "rewards": {
    "experiencePoints": 500,
    "gold": 100,
    "items": ["item-magic-ring"]
  },
  "prerequisites": {
    "minimumLevel": 3
  }
}
```

**Step 2**: Use QuestService to interact

```csharp
var questService = pluginContext.GetService<IQuestService>();

// Start quest (validates prerequisites)
bool started = questService.StartQuest(playerId, Guid.Parse("quest-001-artifact"));

// Check progress
var activeQuests = questService.GetActiveQuests(playerId);
foreach (var quest in activeQuests)
{
    Console.WriteLine($"{quest.Name}: {quest.Objectives.Count(o => o.IsCompleted)}/{quest.Objectives.Count}");
}

// Complete quest
if (questService.AreObjectivesComplete(playerId, questId))
{
    questService.CompleteQuest(playerId, questId);
}
```

**Event Integration**:
Quest objectives auto-update via events:

```csharp
// In your combat system
eventBus.Publish(new EnemyKilledEvent(enemyType: "goblin"));
// QuestSystem subscribes and updates all "KillEnemies" objectives targeting "goblin"
```

---

## 2. NPC & Dialogue System

### Creating an NPC with Dialogue

**Step 1**: Define dialogue tree in JSON

```json
// Data/Dialogue/eldrin-questgiver.json
{
  "id": "dialogue-eldrin",
  "rootNodeId": "node-greeting",
  "nodes": {
    "node-greeting": {
      "id": "node-greeting",
      "npcText": "Greetings, traveler. I require your assistance.",
      "choices": [
        {
          "playerText": "What do you need?",
          "nextNodeId": "node-quest-offer",
          "condition": "!questActive('quest-001-artifact')",
          "action": null
        },
        {
          "playerText": "I have the artifact.",
          "nextNodeId": "node-quest-complete",
          "condition": "hasItem('item-artifact')",
          "action": { "type": "CompleteQuest", "parameters": { "questId": "quest-001-artifact" } }
        },
        {
          "playerText": "Farewell.",
          "nextNodeId": null,
          "condition": null,
          "action": null
        }
      ],
      "isTerminal": false
    },
    "node-quest-offer": {
      "id": "node-quest-offer",
      "npcText": "A magical artifact lies in the depths of level 5. Retrieve it for me.",
      "choices": [
        {
          "playerText": "I'll do it.",
          "nextNodeId": "node-quest-accepted",
          "condition": null,
          "action": { "type": "AcceptQuest", "parameters": { "questId": "quest-001-artifact" } }
        },
        {
          "playerText": "Not interested.",
          "nextNodeId": null,
          "condition": null,
          "action": null
        }
      ],
      "isTerminal": false
    }
  }
}
```

**Step 2**: Spawn NPC with dialogue reference

```csharp
var world = World.Create();
var npc = world.Create(
    new NPC
    {
        Id = Guid.NewGuid(),
        Name = "Eldrin the Wise",
        Type = NPCType.QuestGiver,
        DialogueTreeId = Guid.Parse("dialogue-eldrin"),
        State = new Dictionary<string, object>()
    },
    new Position { X = 10, Y = 15 },
    new Renderable { Glyph = '@', Color = Color.Blue }
);
```

**Step 3**: Interact with NPC

```csharp
var npcService = pluginContext.GetService<INPCService>();

// Start dialogue
var initialNode = npcService.StartDialogue(playerId, npcId);
Console.WriteLine(initialNode.NPCText);
foreach (var choice in initialNode.Choices.Where(c => c.IsAvailable))
{
    Console.WriteLine($"{choice.Index}: {choice.PlayerText}");
}

// Player selects choice
var nextNode = npcService.SelectChoice(playerId, choiceIndex: 0);
if (nextNode == null)
{
    // Dialogue ended
    npcService.EndDialogue(playerId);
}
```

---

## 3. Character Progression System

### Awarding Experience

```csharp
var progressionService = pluginContext.GetService<IProgressionService>();

// Award XP from combat
bool leveledUp = progressionService.AwardExperience(playerId, amount: 50);
if (leveledUp)
{
    var newLevel = progressionService.GetLevel(playerId);
    var stats = progressionService.GetLevelUpStats(newLevel);
    Console.WriteLine($"Level up! New level: {newLevel}");
    Console.WriteLine($"Health +{stats.HealthBonus}, Attack +{stats.AttackBonus}");
}

// Check level for quest prerequisites
if (progressionService.MeetsLevelRequirement(playerId, requiredLevel: 5))
{
    // Unlock high-level quest
}
```

### Subscribing to Level-Up Events

```csharp
eventBus.Subscribe<PlayerLevelUpEvent>((evt) =>
{
    var player = world.Get<Player>(evt.PlayerId);
    var health = world.Get<Health>(evt.PlayerId);
    var combat = world.Get<Combat>(evt.PlayerId);

    // Apply stat increases
    health.Maximum += evt.StatIncreases.HealthBonus;
    health.Current = health.Maximum; // Full heal on level-up
    combat.Attack += evt.StatIncreases.AttackBonus;
    combat.Defense += evt.StatIncreases.DefenseBonus;

    // Unlock spells
    foreach (var spellId in evt.UnlockedAbilities)
    {
        spellService.LearnSpell(evt.PlayerId, spellId);
    }
});
```

---

## 4. Spell & Ability System

### Defining a Spell

```json
// Data/Spells/fireball.json
{
  "id": "spell-fireball",
  "name": "Fireball",
  "description": "Hurls a ball of fire that explodes on impact, damaging all nearby enemies.",
  "type": "AOE",
  "manaCost": 20,
  "cooldown": 3,
  "targeting": "AOE",
  "range": 8,
  "effect": {
    "type": "AreaDamage",
    "value": 30,
    "radius": 2
  }
}
```

### Casting Spells

```csharp
var spellService = pluginContext.GetService<ISpellService>();

// Player learns spell (from level-up, quest, or spell tome)
spellService.LearnSpell(playerId, Guid.Parse("spell-fireball"));
spellService.EquipSpell(playerId, Guid.Parse("spell-fireball"));

// Cast spell at target location
var result = spellService.CastSpell(
    casterId: playerId,
    spellId: Guid.Parse("spell-fireball"),
    targetX: 15,
    targetY: 20
);

if (!result.Success)
{
    Console.WriteLine($"Cast failed: {result.FailureReason}");
}

// Check mana
var mana = spellService.GetMana(playerId);
Console.WriteLine($"Mana: {mana.Current}/{mana.Maximum}");
```

### Mana Regeneration

```csharp
// In your turn processing system
public void ProcessTurn(World world, bool inCombat)
{
    spellService.RegenerateMana(inCombat);
    // inCombat = false → RegenRate
    // inCombat = true → CombatRegenRate (50% of RegenRate)
}
```

---

## 5. Merchant & Trading System

### Creating a Merchant

```json
// Data/Merchants/blacksmith.json
{
  "id": "merchant-blacksmith",
  "stock": [
    {
      "itemId": "item-iron-sword",
      "quantity": 5,
      "basePrice": 100,
      "sellPriceMultiplier": 1.5,
      "buyPriceMultiplier": 0.5
    },
    {
      "itemId": "item-health-potion",
      "quantity": -1,  // Infinite stock
      "basePrice": 25,
      "sellPriceMultiplier": 1.5,
      "buyPriceMultiplier": 0.5
    }
  ],
  "refreshInterval": 5
}
```

```csharp
// Spawn merchant NPC
var merchant = world.Create(
    new NPC
    {
        Id = Guid.NewGuid(),
        Name = "Grim the Blacksmith",
        Type = NPCType.Merchant,
        DialogueTreeId = Guid.Parse("dialogue-merchant-generic"),
        MerchantInventoryId = Guid.Parse("merchant-blacksmith")
    },
    new Position { X = 5, Y = 10 },
    new Renderable { Glyph = '$', Color = Color.Yellow }
);
```

### Trading

```csharp
var merchantService = pluginContext.GetService<IMerchantService>();

// Start trade
merchantService.StartTrade(playerId, merchantId);

// Get merchant inventory
var inventory = merchantService.GetMerchantInventory(merchantId);
foreach (var item in inventory.Where(i => i.InStock))
{
    Console.WriteLine($"{item.ItemName}: {item.SellPrice} gold (stock: {item.Quantity})");
}

// Buy item
bool success = merchantService.BuyItem(playerId, merchantId, itemId, quantity: 1);
if (!success)
{
    Console.WriteLine("Purchase failed (insufficient gold or inventory full)");
}

// Sell item
merchantService.SellItem(playerId, merchantId, itemId, quantity: 1);

// End trade
merchantService.EndTrade(playerId);
```

---

## 6. Boss Encounter System

### Defining a Boss

```json
// Data/Bosses/dragon-lord.json
{
  "id": "boss-dragon-lord",
  "name": "Azrathor the Dragon Lord",
  "phases": [
    {
      "healthThreshold": 1.0,
      "behavior": "Chase",
      "abilities": [
        {
          "id": "ability-fire-breath",
          "name": "Fire Breath",
          "type": "AOEAttack",
          "cooldown": 5,
          "effect": {
            "type": "AreaDamage",
            "value": 40,
            "radius": 3
          }
        }
      ],
      "abilityChance": 0.3
    },
    {
      "healthThreshold": 0.5,
      "behavior": "Ranged",
      "abilities": [
        {
          "id": "ability-summon-drakes",
          "name": "Summon Drakes",
          "type": "SummonMinions",
          "cooldown": 10,
          "effect": {
            "type": "Summon",
            "summonEnemyType": "drake",
            "value": 3
          }
        }
      ],
      "abilityChance": 0.5
    }
  ],
  "guaranteedLoot": ["item-dragon-scale", "item-legendary-sword"]
}
```

### Spawning and Managing Bosses

```csharp
var bossService = pluginContext.GetService<IBossService>();

// Check if level should have boss
if (bossService.IsBossLevel(dungeonLevel: 10))
{
    var bossId = bossService.GetBossForLevel(10);
    if (bossId.HasValue && !bossService.IsBossDefeated(bossId.Value))
    {
        // Spawn boss in center of boss room
        var bossEntityId = bossService.SpawnBoss(bossId.Value, dungeonLevel: 10, x: 40, y: 25);
    }
}

// After boss takes damage
bossService.CheckPhaseTransition(bossEntityId);

// Boss AI turn
if (bossService.TryUseAbility(bossEntityId))
{
    Console.WriteLine("Boss used special ability!");
}

// On boss death
if (boss.IsDead)
{
    bossService.MarkBossDefeated(bossId);
    // Drop guaranteed loot
}
```

---

## 7. Environmental Hazards & Traps

### Placing Traps

```csharp
var hazardService = pluginContext.GetService<IHazardService>();

// Place spike trap in corridor
var trapId = hazardService.PlaceTrap(
    TrapType.Spike,
    x: 12,
    y: 15,
    detectionDifficulty: 15,  // DC 15 Perception
    disarmDifficulty: 12      // DC 12 skill check
);
```

### Detection and Disarming

```csharp
// Player moves - check for traps
var detectedTraps = hazardService.CheckForTraps(playerId, radius: 1);
foreach (var trapId in detectedTraps)
{
    var trapInfo = hazardService.GetTrapInfo(trapId);
    Console.WriteLine($"Detected {trapInfo.Type} trap at ({trapInfo.X}, {trapInfo.Y})!");
}

// Player attempts to disarm
bool disarmed = hazardService.DisarmTrap(playerId, trapId);
if (disarmed)
{
    Console.WriteLine("Trap disarmed!");
}
else
{
    Console.WriteLine("Disarm failed! Trap triggered!");
    // hazardService.TriggerTrap() called automatically on failure
}
```

### Environmental Hazards

```csharp
// Place lava tile
var hazardId = hazardService.PlaceHazard(HazardType.Lava, x: 20, y: 30);

// Check if tile blocks movement
if (hazardService.IsHazardBlocking(x: 20, y: 30))
{
    Console.WriteLine("Cannot move here - lava blocks passage");
}

// Apply damage each turn
if (player.Position.X == 20 && player.Position.Y == 30)
{
    hazardService.ApplyHazardDamage(playerId, hazardId);
}
```

---

## Common Patterns

### Service Access

```csharp
// In your plugin or system
public class YourSystem
{
    private readonly IQuestService _questService;
    private readonly INPCService _npcService;

    public YourSystem(IPluginContext pluginContext)
    {
        _questService = pluginContext.GetService<IQuestService>();
        _npcService = pluginContext.GetService<INPCService>();
    }
}
```

### Event Subscriptions

```csharp
// In your plugin's Initialize method
public void Initialize(IPluginContext context)
{
    var eventBus = context.GetService<IEventBus>();

    eventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
    eventBus.Subscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
    eventBus.Subscribe<SpellCastEvent>(OnSpellCast);
}
```

### Persistence

```csharp
// Save game state
var storageService = pluginContext.GetService<IPersistentStorageService>();

var saveData = new
{
    player = new
    {
        experience = progressionService.GetExperience(playerId),
        questLog = questService.GetActiveQuests(playerId),
        gold = merchantService.GetGold(playerId),
        spells = spellService.GetKnownSpells(playerId)
    }
};

storageService.Save("game-save.json", saveData);

// Load game state
var loaded = storageService.Load<dynamic>("game-save.json");
// Restore components from loaded data
```

---

## Testing

### Unit Test Example

```csharp
[Fact]
public void QuestService_StartQuest_ValidatesPrerequisites()
{
    // Arrange
    var world = World.Create();
    var player = world.Create(
        new Experience { Level = 2 },
        new QuestLog()
    );

    var questService = new QuestService(world);
    var questId = Guid.Parse("quest-001-artifact"); // Requires level 3

    // Act
    bool started = questService.StartQuest(player, questId);

    // Assert
    Assert.False(started); // Should fail - player is level 2, quest requires 3
}
```

### Integration Test Example

```csharp
[Fact]
public void FullQuestCycle_StartToCompletion()
{
    // Arrange
    var world = CreateTestWorld();
    var player = CreateTestPlayer(level: 5);
    var questService = GetService<IQuestService>();
    var progressionService = GetService<IProgressionService>();

    var questId = Guid.Parse("quest-001-artifact");

    // Act & Assert
    // Start quest
    Assert.True(questService.StartQuest(player, questId));
    Assert.Contains(questId, questService.GetActiveQuests(player).Select(q => q.Id));

    // Complete objectives
    questService.OnLocationReached(player, level: 5, x: 0, y: 0);
    questService.OnItemCollected(player, Guid.Parse("item-artifact"));

    // Complete quest
    Assert.True(questService.CompleteQuest(player, questId));
    Assert.Contains(questId, questService.GetCompletedQuests(player).Select(q => q.Id));

    // Verify rewards
    var exp = progressionService.GetExperience(player);
    Assert.True(exp.TotalXPGained >= 500); // Quest rewards 500 XP
}
```

---

## Performance Considerations

### ECS Query Optimization

```csharp
// Good: Specific query with required components only
var query = new QueryDescription()
    .WithAll<Quest, QuestLog>();
world.Query(in query, (Entity entity, ref Quest quest, ref QuestLog log) =>
{
    // Process
});

// Bad: Over-broad query
world.GetAllEntities(); // Iterates everything
```

### Event Batching

```csharp
// Good: Batch multiple events
eventBus.BeginBatch();
for (int i = 0; i < 100; i++)
{
    eventBus.Publish(new ItemCollectedEvent(itemId));
}
eventBus.EndBatch(); // Process all at once

// Bad: Publish individually in loop
for (int i = 0; i < 100; i++)
{
    eventBus.Publish(new ItemCollectedEvent(itemId)); // Triggers handlers 100 times
}
```

---

## Troubleshooting

### Common Issues

**"Service not found"**

- Ensure plugin is registered in plugin manifest
- Check service registration in Plugin.Initialize()
- Verify IPluginContext is passed correctly

**"Quest not starting"**

- Check prerequisites (level, previous quests, items)
- Verify quest JSON is valid and loaded
- Ensure QuestLog component exists on player

**"Spell cast fails with 'Insufficient mana'"**

- Check Mana component Current value
- Verify spell ManaCost in JSON
- Ensure mana regeneration is working (call RegenerateMana each turn)

**"Trap not detecting"**

- Verify player has Perception stat
- Check DetectionDifficulty vs player's Perception
- Ensure CheckForTraps is called on player movement

---

## Next Steps

- **Implementation**: See `tasks.md` for task breakdown (generated via `/speckit.tasks`)
- **Architecture**: See `plan.md` for full technical design
- **Data Models**: See `data-model.md` for complete component definitions
- **Contracts**: See `/contracts/` for service interfaces

---

**Quick Start Status**: ✅ Complete
**Ready for Implementation**: Yes
