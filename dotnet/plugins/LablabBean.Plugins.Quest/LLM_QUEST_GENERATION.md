# LLM-Powered Quest Generation

The Quest System now includes optional LLM-powered quest generation for dynamic, contextual quests.

## Features

### 1. Dynamic Quest Generation

Generate quests based on:

- Player level and class
- Current dungeon floor
- Nearby enemies
- Available NPCs
- Previously completed quests

### 2. Quest Options

Generate multiple quest options for player choice:

```csharp
var context = new QuestGenerationContext(
    PlayerLevel: 5,
    DungeonLevel: 3,
    PlayerClass: "Warrior",
    NearbyEnemyTypes: new() { "Goblin", "Skeleton", "Bat" },
    AvailableNPCs: new() { "Eldrin the Wise", "Mara the Merchant" }
);

var quests = await questFactory.CreateQuestOptionsAsync(context, npcId, count: 3);
// Returns 3 different AI-generated quests
```

### 3. Quest Chains

Generate multi-part story quests:

```csharp
var chain = await questFactory.CreateQuestChainAsync(context, npcId, chainLength: 3);
// Returns 3 quests where each requires completing the previous one
```

## Usage

### Basic Setup

```csharp
// In your game initialization:
var chatClient = new ChatClient("your-llm-endpoint");
var questFactory = new QuestFactory(world, new QuestGeneratorAgent(chatClient));

// Generate a dynamic quest
var context = new QuestGenerationContext(
    PlayerLevel: playerLevel,
    DungeonLevel: currentFloor
);

var questEntity = await questFactory.CreateDynamicQuestAsync(context, questGiverNPCId);
```

### Template Quests (No LLM Required)

```csharp
// Create template quests without LLM
var questFactory = new QuestFactory(world); // No QuestGeneratorAgent

// Collection quest
var collectQuest = questFactory.CreateCollectionQuest("Health Potion", 3, npcId, playerLevel);

// Exploration quest
var exploreQuest = questFactory.CreateExplorationQuest(targetFloor: 5, npcId, playerLevel);

// Manual quest
var objectives = new List<QuestObjective>
{
    new(QuestObjectiveType.KillEnemies, "Dragon", 1)
};
var rewards = new QuestRewards(experiencePoints: 1000, gold: 500);
var bossQuest = questFactory.CreateQuest(
    "Slay the Dragon",
    "A fearsome dragon threatens the kingdom!",
    objectives,
    rewards,
    npcId
);
```

### Fallback Behavior

The system gracefully handles LLM unavailability:

- If no `IChatClient` is available, template quests are used
- If LLM request fails, fallback to predefined quest templates
- Always provides functional quests even without AI

## Quest Generation Context

### Required Fields

- `PlayerLevel`: Player's current level (affects rewards/difficulty)
- `DungeonLevel`: Current dungeon floor

### Optional Fields

- `PlayerClass`: "Warrior", "Mage", "Rogue", etc.
- `NearbyEnemyTypes`: Enemies on current floor
- `AvailableNPCs`: NPCs player can interact with
- `CompletedQuests`: Previous quest names (for continuity)
- `AdditionalContext`: Custom instructions for quest generation

## Quest Types

The system supports 6 objective types:

### 1. KillEnemies

```json
{
  "type": "KillEnemies",
  "target": "Goblin",
  "required": 10
}
```

### 2. CollectItems

```json
{
  "type": "CollectItems",
  "target": "Ancient Key",
  "required": 1
}
```

### 3. ReachLocation

```json
{
  "type": "ReachLocation",
  "target": "Floor5",
  "required": 1
}
```

### 4. TalkToNPC

```json
{
  "type": "TalkToNPC",
  "target": "Eldrin",
  "required": 1
}
```

### 5. DeliverItem

```json
{
  "type": "DeliverItem",
  "target": "Mara",
  "required": 1
}
```

### 6. Survive

```json
{
  "type": "Survive",
  "target": "Arena",
  "required": 10
}
```

_(Survive 10 turns)_

## Example Generated Quest

```json
{
  "name": "The Goblin King's Hoard",
  "description": "Eldrin has discovered that the goblins on floor 3 are hoarding stolen artifacts. Defeat the Goblin King and recover the Ancient Amulet.",
  "objectives": [
    {
      "type": "KillEnemies",
      "target": "Goblin King",
      "required": 1,
      "description": "Defeat the Goblin King"
    },
    {
      "type": "CollectItems",
      "target": "Ancient Amulet",
      "required": 1,
      "description": "Retrieve the Ancient Amulet"
    }
  ],
  "rewards": {
    "experiencePoints": 500,
    "gold": 250
  },
  "minimumLevel": 5
}
```

## Integration with Progression System

Quests automatically grant XP through the Progression plugin:

```csharp
// When quest is completed:
// 1. QuestRewardSystem.CompleteQuest() is called
// 2. XP is awarded via ProgressionService.AwardExperience()
// 3. Player may level up (cascading levels supported)
// 4. Gold is added to player inventory
```

## Performance Considerations

- LLM requests are async and non-blocking
- Fallback quests are instant
- Quest options generate 3 quests sequentially (~100ms delay between)
- Quest chains generate sequentially with context continuity
- All quest data is stored in ECS components (no external dependencies)

## Configuration

Enable LLM quest generation:

```csharp
// Register IChatClient before initializing QuestPlugin
context.Registry.Register<IChatClient>(yourChatClient);

// QuestPlugin will automatically detect and use it
```

Disable LLM quest generation:

```csharp
// Simply don't register IChatClient
// QuestPlugin works in template-only mode
```

## Testing

Check LLM availability:

```csharp
if (questFactory.HasLLMGeneration)
{
    // Can generate AI quests
    var quest = await questFactory.CreateDynamicQuestAsync(context, npcId);
}
else
{
    // Use templates only
    var quest = questFactory.CreateCollectionQuest("Potion", 5, npcId, playerLevel);
}
```

---

**Version**: 1.1.0
**Added**: 2025-10-24
**Dependencies**: Quest Plugin, Microsoft.Extensions.AI (optional)
