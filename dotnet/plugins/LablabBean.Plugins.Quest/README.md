# Quest System Plugin 🎯

**Version**: 1.0.0
**Plugin Type**: Gameplay System
**Dependencies**: LablabBean.Game.Core, Arch, GoRogue

---

## Overview

The Quest System provides a comprehensive quest and objective tracking system for Lablab-Bean, enabling players to receive quests from NPCs, track objectives, and earn rewards upon completion.

### Key Features

- **Quest Management**: Create, track, and complete quests with multiple objectives
- **Objective Types**: Kill, Collect, Reach, and Talk objectives
- **Rewards System**: Experience points, gold, and item rewards
- **Prerequisites**: Level requirements, required quests, and item requirements
- **Quest Chains**: Support for sequential quests with dependencies
- **AI Generation**: Optional AI-powered quest generation via Semantic Kernel
- **Database**: Pre-built database of 15 unique quests

---

## Quick Start

### 1. Register the Plugin

```csharp
services.AddQuestSystem();
```

### 2. Create a Quest

```csharp
var questService = pluginContext.GetService<IQuestService>();
var quest = questFactory.CreateQuest(
    "Goblin Extermination",
    "Clear out the goblin infestation",
    objectives: new List<QuestObjective>
    {
        new(Guid.NewGuid().ToString(), questId, ObjectiveType.Kill, "Defeat goblins", "Goblin", 5)
    },
    rewards: new QuestRewards(experiencePoints: 100, gold: 50),
    questGiverId: npcEntityId
);
```

### 3. Track Progress

```csharp
// Player kills a goblin
questService.UpdateObjectiveProgress(questEntity, "Kill", "Goblin", 1);

// Check if quest is complete
if (questService.IsQuestComplete(questEntity))
{
    questService.CompleteQuest(questEntity, playerEntity);
}
```

---

## Components

### Quest

Main quest component containing all quest data.

**Fields**:

- `Id` (string): Unique quest identifier
- `Title` (string): Quest display name
- `Description` (string): Quest narrative text
- `State` (QuestState): Current state (NotStarted, Active, Completed, Failed)
- `QuestGiverId` (int): Entity ID of quest-giving NPC
- `Objectives` (List<QuestObjective>): List of quest objectives

### QuestObjective

Individual objective within a quest.

**Fields**:

- `Id` (string): Unique objective identifier
- `QuestId` (string): Parent quest ID
- `Type` (ObjectiveType): Type of objective (Kill, Collect, Reach, Talk)
- `Description` (string): Objective description
- `Target` (string): Target identifier (enemy type, item ID, location, NPC)
- `Required` (int): Required count
- `Current` (int): Current progress

### Quest Log

Player's active quest list.

**Fields**:

- `ActiveQuests` (List<string>): List of active quest IDs
- `CompletedQuests` (List<string>): List of completed quest IDs
- `FailedQuests` (List<string>): List of failed quest IDs

---

## Systems

### QuestSystem

Core ECS system for quest processing.

**Responsibilities**:

- Update quest states
- Track objective progress
- Handle quest completion
- Grant rewards

### QuestUIUpdateSystem

Updates UI with quest state changes.

**Responsibilities**:

- Refresh quest log UI
- Update objective progress displays
- Show completion notifications

---

## Services

### IQuestService

High-level quest management API.

**Key Methods**:

```csharp
Entity CreateQuest(string title, string description, ...);
void StartQuest(Entity questEntity, Entity playerEntity);
void UpdateObjectiveProgress(Entity questEntity, string objectiveType, string target, int amount);
bool IsQuestComplete(Entity questEntity);
void CompleteQuest(Entity questEntity, Entity playerEntity);
void FailQuest(Entity questEntity, Entity playerEntity);
Entity? GetQuestById(string questId);
List<Entity> GetActiveQuests(Entity playerEntity);
List<Entity> GetCompletedQuests(Entity playerEntity);
```

---

## Quest Database

15 pre-built quests included:

### Tutorial Quests (Level 1-2)

1. **First Steps** - Learn basic movement and combat
2. **Goblin Menace** - Clear out 5 goblins
3. **Treasure Hunter** - Find 3 health potions

### Early Game (Level 3-5)

4. **Deeper Descent** - Reach dungeon floor 5
5. **Bandit Trouble** - Defeat 10 bandits
6. **Lost Artifact** - Recover the Ancient Amulet

### Mid Game (Level 6-10)

7. **Orc Invasion** - Defeat 15 orcs
8. **Supply Run** - Collect 10 iron ingots
9. **The Dark Depths** - Reach dungeon floor 15

### Late Game (Level 11-15)

10. **Dragon's Hoard** - Defeat the dragon boss
11. **Master Collector** - Gather 5 legendary items
12. **Final Descent** - Reach the deepest floor

### Chain Quests

13-15. **The Ancient Prophecy** (3-part chain)

---

## AI Quest Generation

Optional AI-powered quest generation using Semantic Kernel.

### Setup

```csharp
services.AddQuestGenerator(config =>
{
    config.UseOpenAI(apiKey, modelId);
});
```

### Generate Quests

```csharp
var generator = pluginContext.GetService<IQuestGenerator>();
var context = new QuestGenerationContext
{
    PlayerLevel = 5,
    CompletedQuestCount = 10,
    CurrentFloor = 8,
    RecentEncounters = new List<string> { "Goblin", "Bandit" }
};

var generatedQuest = await generator.GenerateQuestAsync(context);
```

---

## Events

The Quest System emits the following events:

- `QuestAcceptedEvent` - Player accepts a quest
- `QuestCompletedEvent` - Quest completed successfully
- `QuestFailedEvent` - Quest failed
- `QuestObjectiveUpdatedEvent` - Objective progress updated
- `QuestChainAdvancedEvent` - Next quest in chain unlocked

---

## Configuration

### QuestSystemConfig

```csharp
{
  "maxActiveQuests": 10,
  "enableAIGeneration": false,
  "questDatabasePath": "Data/Quests/quest_database.json",
  "allowQuestAbandonment": true,
  "objectiveProgressNotifications": true
}
```

---

## Integration Examples

### With NPC System

```csharp
// NPC dialogue action to offer quest
var action = new DialogueAction
{
    Type = DialogueActionType.OfferQuest,
    QuestId = "goblin-menace"
};
```

### With Progression System

```csharp
// Check level requirement
if (player.Get<CharacterProgression>().Level >= quest.Get<Quest>().MinLevel)
{
    questService.StartQuest(quest, player);
}
```

### With Inventory System

```csharp
// Grant quest reward items
foreach (var itemId in rewards.ItemIds)
{
    inventoryService.AddItem(playerEntity, itemId);
}
```

---

## Testing

```bash
dotnet test dotnet/tests/LablabBean.Plugins.Quest.Tests/
```

### Test Coverage

- ✅ Quest creation and initialization
- ✅ Objective progress tracking
- ✅ Quest completion and rewards
- ✅ Quest prerequisites
- ✅ Quest chains
- ✅ AI generation (mocked)

---

## Performance

- **Quest Updates**: <1ms per quest per frame
- **Objective Tracking**: O(1) lookup by type/target
- **Active Quest Limit**: 10 concurrent quests (configurable)
- **Database Loading**: <50ms for 100 quests

---

## Known Limitations

- Maximum 10 active quests per player (configurable)
- Objectives limited to 4 types (Kill, Collect, Reach, Talk)
- AI generation requires external API (optional feature)
- No support for timed quests (future enhancement)

---

## Changelog

### v1.0.0 (2025-01-25)

- ✅ Initial release
- ✅ Core quest system
- ✅ 15 pre-built quests
- ✅ AI generation support
- ✅ Quest chains

---

## License

MIT License - Part of Lablab-Bean project

---

**For more examples and integration patterns, see**: [INTEGRATION_EXAMPLES.md](INTEGRATION_EXAMPLES.md)
