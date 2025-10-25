# LLM-Powered NPC Dialogue System

The NPC System now includes optional LLM-powered dialogue generation for dynamic, contextual conversations.

## Features

### 1. Dynamic Dialogue Tree Generation

Generate complete dialogue trees based on NPC context:

- NPC name, role, and personality
- Player level and location
- Special context (quest availability, lore, etc.)

### 2. Real-Time Response Generation

Generate NPC responses dynamically during conversation:

```csharp
var response = await dialogueGenerator.GenerateResponseAsync(
    npcName: "Eldrin",
    npcRole: "Quest Giver",
    playerMessage: "Tell me about the dungeon",
    conversationContext: "Previous conversation..."
);
```

### 3. Dynamic Choice Generation

Generate player dialogue options on-the-fly:

```csharp
var choices = await dialogueGenerator.GenerateChoicesAsync(
    npcName: "Eldrin",
    npcRole: "Quest Giver",
    npcText: "The dungeon holds ancient secrets...",
    choiceCount: 3
);
// Returns 3 different player response options with distinct tones
```

## Usage

### Basic Setup

```csharp
// Enable LLM dialogue generation
var chatClient = new ChatClient("your-llm-endpoint");
services.AddSingleton<IChatClient>(chatClient);

// NPCPlugin auto-detects and enables LLM features
```

### Create NPC with LLM Dialogue

```csharp
// Create quest-giver with dynamic dialogue
var (npcEntity, dialogueTree) = await npcFactory.CreateQuestGiverAsync(
    name: "Eldrin the Wise",
    playerLevel: 5,
    personality: "Wise and experienced mentor who speaks in riddles"
);

// Dialogue tree is automatically generated based on context
```

### Specialized NPC Creation

#### Quest Giver

```csharp
var (entity, dialogue) = await npcFactory.CreateQuestGiverAsync(
    "Eldrin",
    playerLevel: 5,
    personality: "Wise and experienced, eager to help adventurers"
);
```

#### Merchant

```csharp
var (entity, dialogue) = await npcFactory.CreateMerchantAsync(
    "Mara",
    playerLevel: 3,
    personality: "Shrewd but fair, always looking for profit"
);
```

#### Lore Keeper

```csharp
var (entity, dialogue) = await npcFactory.CreateLoreKeeperAsync(
    "Ancient Scholar",
    playerLevel: 10,
    personality: "Ancient and knowledgeable, speaks in riddles"
);
```

### Custom Dialogue Generation

```csharp
var context = new DialogueGenerationContext(
    NPCName: "Guardian",
    NPCRole: "Dungeon Guardian",
    PlayerLevel: 8,
    Personality: "Stern but honorable, tests worthy adventurers",
    Location: "Sacred Temple",
    SpecialContext: "Guards the entrance to the inner sanctum"
);

var dialogueTree = await dialogueGenerator.GenerateDialogueTreeAsync(context);
```

### Template NPCs (No LLM)

```csharp
// Works without LLM - uses predefined templates
var npcEntity = npcFactory.CreateNPC(
    name: "Guard",
    role: "Guard",
    dialogueTree: null // Uses fallback dialogue
);
```

## Dialogue Tree Structure

Generated dialogue trees follow this structure:

```json
{
  "id": "eldrin-dialogue-001",
  "name": "Eldrin the Wise",
  "startNodeId": "greeting",
  "nodes": {
    "greeting": {
      "id": "greeting",
      "text": "Well met, traveler! I am Eldrin.",
      "choices": [
        {
          "text": "Who are you?",
          "nextNodeId": "about"
        },
        {
          "text": "Do you have any quests?",
          "nextNodeId": "quests"
        },
        {
          "text": "Farewell",
          "nextNodeId": "end"
        }
      ]
    },
    "about": {
      "id": "about",
      "text": "I've been guiding adventurers for decades...",
      "choices": [...]
    },
    "end": {
      "id": "end",
      "text": "Safe travels, friend.",
      "choices": []
    }
  }
}
```

## Dialogue Features

### Branching Conversations

- Multiple choice paths
- Context-aware responses
- Personality-driven dialogue

### Dynamic Adaptation

- Responds to player level
- Adjusts based on location
- Incorporates special context

### Fallback System

1. **LLM Available**: Generate dynamic dialogue
2. **LLM Fails**: Use template dialogue
3. **No LLM**: Template dialogue only

## Integration with Quest System

```csharp
// Create quest-giver NPC
var (questGiver, dialogue) = await npcFactory.CreateQuestGiverAsync("Eldrin", playerLevel);

// Quest dialogue node can trigger quest acceptance
var questNode = new DialogueNode
{
    Id = "accept-quest",
    Text = "Will you help me retrieve the artifact?",
    OnEnterActions = new List<DialogueAction>
    {
        new()
        {
            Type = DialogueActionType.OfferQuest,
            Parameters = new() { ["questId"] = "ancient-artifact-quest" }
        }
    },
    Choices = new List<DialogueChoice>
    {
        new() { Text = "I accept", NextNodeId = "quest-accepted" },
        new() { Text = "Not now", NextNodeId = "quest-declined" }
    }
};
```

## NPC State Management

NPCs can remember player choices:

```csharp
// Set NPC state
npc.SetState("quest_offered", "true");
npc.SetState("player_reputation", "friendly");

// Check state in dialogue
if (npc.HasState("quest_offered"))
{
    // Show different dialogue for return visits
}

// Get state value
var reputation = npc.GetState("player_reputation");
```

## Dialogue Conditions

Choices can have conditions:

```csharp
new DialogueChoice
{
    Text = "I completed your quest!",
    NextNodeId = "quest-complete",
    Condition = "quest:ancient-artifact:completed"
}
```

## Example Generated Dialogue

**Quest Giver (Eldrin)**:

```
Node 1 (Greeting):
"Greetings, young adventurer! I am Eldrin, keeper of ancient lore."

Choices:
→ "What brings you to this place?" [Curious]
→ "Do you need help with anything?" [Helpful]
→ "I'm just passing through." [Neutral]

Node 2 (Quest Offer):
"The dungeon below holds a powerful artifact. Will you retrieve it for me?"

Choices:
→ "I accept your quest!" [Eager]
→ "What's the reward?" [Pragmatic]
→ "This sounds dangerous..." [Cautious]
```

## Performance

- LLM requests: ~1-3 seconds per dialogue tree
- Caching: Generated trees stored in memory
- Fallback: Instant template dialogue (<1ms)
- Real-time responses: ~500ms-1s per message

## Configuration

### Enable LLM

```csharp
services.AddSingleton<IChatClient>(chatClient);
// NPCPlugin detects and enables
```

### Disable LLM

```csharp
// Don't register IChatClient
// System works with templates only
```

### Check Availability

```csharp
if (npcFactory.HasLLMGeneration)
{
    // Use dynamic dialogue
    var (npc, dialogue) = await npcFactory.CreateQuestGiverAsync("Eldrin", level);
}
else
{
    // Use template
    var npc = npcFactory.CreateNPC("Eldrin", "QuestGiver");
}
```

## Dialogue Actions

Supported action types:

- `OfferQuest`: Present quest to player
- `CompleteQuest`: Turn in completed quest
- `GiveItem`: NPC gives item to player
- `TakeItem`: NPC takes item from player
- `SetState`: Update NPC state
- `OpenTrade`: Start merchant trading

## Best Practices

1. **Personality Consistency**: Use clear personality descriptions
2. **Context Richness**: Provide detailed special context for better dialogue
3. **Fallback Planning**: Always have template dialogue ready
4. **State Tracking**: Use NPC state for persistent conversations
5. **Choice Variety**: Generate 3-4 distinct choices per node

## Testing

```csharp
// Test LLM dialogue generation
var context = new DialogueGenerationContext(
    NPCName: "Test NPC",
    NPCRole: "Test",
    PlayerLevel: 1
);

var tree = await dialogueGenerator.GenerateDialogueTreeAsync(context);

Assert.NotNull(tree);
Assert.True(tree.Nodes.Count >= 3);
Assert.NotEmpty(tree.GetStartNode()?.Choices);
```

---

**Version**: 1.1.0
**Added**: 2025-10-24
**Dependencies**: NPC Plugin, Microsoft.Extensions.AI (optional)
