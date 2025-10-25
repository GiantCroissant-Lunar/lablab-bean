# Phase 4 Memory System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         GAME LAYER                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐             │
│  │   Player     │  │    NPC       │  │   Dialogue   │             │
│  │   Entity     │  │   Entity     │  │     UI       │             │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘             │
└─────────┼──────────────────┼──────────────────┼───────────────────┘
          │                  │                  │
          └──────────────────┴──────────────────┘
                             │
          ┌──────────────────▼──────────────────┐
          │   MemoryEnhancedNPCService          │
          │   ┌──────────────────────────────┐  │
          │   │ • StartDialogueAsync()       │  │
          │   │ • SelectChoiceAsync()        │  │
          │   │ • GetRelationshipInsights()  │  │
          │   │ • GetRecentDialogueHistory() │  │
          │   └──────────────────────────────┘  │
          └──────────┬──────────────────────────┘
                     │
       ┌─────────────┴─────────────┐
       │                           │
       ▼                           ▼
┌────────────────────┐   ┌─────────────────────┐
│ DialogueSystem     │   │ IMemoryService      │
│ (Base)             │   │ (Kernel Memory)     │
│                    │   │                     │
│ • StartDialogue()  │   │ • StoreMemory()     │
│ • SelectChoice()   │   │ • RetrieveMemory()  │
│ • GetChoices()     │   │ • SearchMemories()  │
└────────────────────┘   └─────────────────────┘
                                  │
                                  ▼
                         ┌─────────────────┐
                         │  Vector Store   │
                         │  (In-Memory)    │
                         └─────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                       EXAMPLE LAYER                                 │
│                                                                     │
│  ┌──────────────────────────┐  ┌──────────────────────────────┐   │
│  │ RelationshipBasedMerchant│  │  ContextAwareGreetingSystem  │   │
│  ├──────────────────────────┤  ├──────────────────────────────┤   │
│  │ • GetPrice()             │  │ • GenerateGreeting()         │   │
│  │ • CanAccessExclusive()   │  │ • GenerateFarewell()         │   │
│  │ • GetGreeting()          │  │ • ExtractTopic()             │   │
│  └────────────┬─────────────┘  └────────────┬─────────────────┘   │
│               │                               │                     │
│               └───────────┬───────────────────┘                     │
│                           │                                         │
│                           ▼                                         │
│               ┌──────────────────────────────┐                     │
│               │ MemoryVisualizationDashboard │                     │
│               ├──────────────────────────────┤                     │
│               │ • GenerateDashboard()        │                     │
│               │ • GenerateReport()           │                     │
│               │ • PrintToConsole()           │                     │
│               └──────────────────────────────┘                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Data Flow

### 1. Dialogue Start

```
Player Talks to NPC
      ↓
MemoryEnhancedNPCService.StartDialogueAsync()
      ↓
DialogueSystem.StartDialogue()  →  [Fire-and-Forget]  →  Store Memory
      ↓                                                         ↓
Return Dialogue Entity                                 Vector Store
```

### 2. Relationship Query

```
Game needs relationship info
      ↓
GetRelationshipInsightsAsync(playerId, npcId)
      ↓
Search memories with filters:
  - entity_id = playerId
  - npc_id = npcId
  - memory_type = dialogue_*
      ↓
Calculate relationship level:
  - Count interactions
  - Sum importance scores
  - Apply level thresholds
      ↓
Return NpcRelationshipInsights
```

### 3. Dynamic Pricing

```
Merchant shows items
      ↓
RelationshipBasedMerchant.GetPriceAsync()
      ↓
GetRelationshipInsightsAsync()
      ↓
Apply discount based on level:
  - Stranger: 0%
  - Acquaintance: 5%
  - Friend: 10%
  - GoodFriend: 15%
  - CloseFriend: 20%
  - TrustedFriend: 25%
      ↓
Return discounted price
```

## Memory Structure

### Memory Entry

```json
{
  "id": "guid",
  "content": "Player chose 'buy_health_potion' with Gareth",
  "entityId": "player_123",
  "memoryType": "dialogue_choice",
  "importance": 0.65,
  "timestamp": "2025-01-15T10:30:00Z",
  "tags": {
    "npc_id": "merchant_gareth",
    "npc_name": "Gareth",
    "choice_id": "buy_health_potion",
    "event_type": "purchase"
  }
}
```

### Relationship Insights

```json
{
  "npcId": "merchant_gareth",
  "interactionCount": 7,
  "totalImportance": 4.2,
  "lastInteraction": "2025-01-15T08:30:00Z",
  "relationshipLevel": "Friend"
}
```

## Component Responsibilities

### Core System

| Component | Responsibility | Performance |
|-----------|---------------|-------------|
| MemoryEnhancedNPCService | Orchestrates NPC interactions | <5ms overhead |
| DialogueSystem | Manages dialogue state | Existing performance |
| IMemoryService | Stores/retrieves memories | Fire-and-forget |

### Examples

| Component | Responsibility | Use Case |
|-----------|---------------|----------|
| RelationshipBasedMerchant | Dynamic pricing logic | Shop systems |
| ContextAwareGreetingSystem | Personalized dialogue | All NPCs |
| MemoryVisualizationDashboard | Debug/analytics | Development/Admin |

## Integration Points

### Minimal Integration (Zero Code Changes)

```csharp
// Just add to DI
services.AddMemoryEnhancedNPC(configuration);

// Use MemoryEnhancedNPCService instead of NPCService
// Everything else works the same!
```

### Enhanced Integration (With Examples)

```csharp
// Add examples to DI
services.AddMemoryEnhancedNPC(configuration);
services.AddSingleton<RelationshipBasedMerchant>();
services.AddSingleton<ContextAwareGreetingSystem>();
services.AddSingleton<MemoryVisualizationDashboard>();

// Use in game code
var greeting = await _greetingSystem.GenerateGreetingAsync(...);
var price = await _merchant.GetPriceAsync(...);
```

## Scaling Characteristics

### Memory Storage

- **Per Interaction**: ~500 bytes
- **Per Player**: ~5-10 KB (10-20 interactions)
- **1000 Players**: ~5-10 MB
- **Growth Rate**: Linear with interactions

### Performance

- **Memory Write**: <1ms (fire-and-forget)
- **Memory Read**: <50ms (vector search)
- **Relationship Calc**: <10ms (cached)
- **Greeting Gen**: <30ms (includes memory read)

### Limits

- **Max Players**: Unlimited (memory is indexed)
- **Max NPCs**: Unlimited
- **Max Memories/Player**: 1000+ recommended
- **Memory Retention**: Configurable (default: forever)

## Future Enhancements (Phase 7)

```
Current: In-Memory Only
      ↓
Phase 7: Add Persistence
      ↓
┌──────────────────────────┐
│  Memory System           │
│                          │
│  In-Memory Cache         │
│        ↕                 │
│  Disk Storage (.json)    │
└──────────────────────────┘

Features:
• Cross-session persistence
• Memory summarization
• Automatic cleanup
• Compression
```

## Error Handling

```
Operation                    Error Strategy
─────────────────────────────────────────────────────
Memory Storage              Log + Continue
Memory Retrieval            Return empty list
Relationship Calculation    Return "Stranger"
Price Calculation           Return base price
Greeting Generation         Return default greeting
```

All operations are **fail-safe** - errors never break gameplay!

---

**Architecture Diagram** | Phase 4 Memory System
**Version**: 1.0 | **Last Updated**: 2025-01-15
