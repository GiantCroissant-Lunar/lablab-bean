# NPC & Dialogue System Plugin 💬

**Version**: 1.0.0 | **Plugin Type**: Gameplay System

---

## Overview

Comprehensive NPC interaction and dialogue system with branching conversations, reputation tracking, and dynamic responses.

## Features

- **Branching Dialogue**: Tree-based conversation system
- **Dialogue Actions**: OfferQuest, OpenTrade, GiveItem, TriggerEvent
- **Reputation System**: Track player-NPC relationships
- **Dynamic Responses**: Dialogue influenced by reputation, quests, items
- **NPC Database**: 10 pre-configured NPCs with unique personalities

## Quick Start

```csharp
// Register plugin
services.AddNPCSystem();

// Create NPC with dialogue
var npc = npcFactory.CreateNPC(
    "Eldrin",
    NPCRole.QuestGiver,
    new DialogueTree(rootNodeId: "greeting")
);

// Start conversation
npcService.StartDialogue(playerEntity, npcEntity);
```

## Components

- **NPC**: Entity data, role, affinity
- **DialogueState**: Active conversation tracking
- **DialogueTree**: Conversation structure
- **DialogueNode**: Individual conversation entry
- **Reputation**: Player-NPC relationship scores

## Services

### INPCService

```csharp
Entity CreateNPC(string name, NPCRole role, DialogueTree dialogue);
void StartDialogue(Entity player, Entity npc);
void SelectDialogueOption(Entity player, int optionIndex);
void UpdateReputation(Entity player, Entity npc, int change);
int GetReputation(Entity player, Entity npc);
```

## NPC Database

10 NPCs included:

1. **Eldrin the Wise** - Quest Giver (Town Elder)
2. **Greta the Smith** - Merchant (Weapons & Armor)
3. **Finn the Scout** - Quest Giver (Explorer)
4. **Mora the Mystic** - Merchant (Potions & Scrolls)
5. **Grimm the Guard** - Quest Giver (City Watch)
6. **Luna the Innkeeper** - Merchant (Food & Rest)
7. **Thom the Thief** - Quest Giver (Rogues Guild)
8. **Aria the Healer** - Merchant (Healing Services)
9. **Borin the Drunk** - Lore (Town Gossip)
10. **The Stranger** - Mysterious (Hidden Quests)

## Dialogue Tree Example

```json
{
  "rootNodeId": "greeting",
  "nodes": {
    "greeting": {
      "text": "Greetings, adventurer! What brings you here?",
      "options": [
        { "text": "I'm looking for work", "nextNodeId": "quest-offer" },
        { "text": "Just passing through", "nextNodeId": "farewell" }
      ]
    },
    "quest-offer": {
      "text": "Ah, you're in luck! I have a task that needs doing...",
      "actions": [{ "type": "OfferQuest", "questId": "goblin-menace" }]
    }
  }
}
```

## Integration

### With Quest System

```csharp
// Offer quest via dialogue
action.Type = DialogueActionType.OfferQuest;
action.QuestId = "goblin-menace";
```

### With Merchant System

```csharp
// Open trade interface
action.Type = DialogueActionType.OpenTrade;
```

## Events

- `DialogueStartedEvent`
- `DialogueOptionSelectedEvent`
- `DialogueEndedEvent`
- `ReputationChangedEvent`

## Performance

- Dialogue processing: <0.5ms per interaction
- Reputation lookups: O(1) hash table
- Tree traversal: O(1) dictionary lookup

---

**See**: [INTEGRATION_EXAMPLES.md](INTEGRATION_EXAMPLES.md)
