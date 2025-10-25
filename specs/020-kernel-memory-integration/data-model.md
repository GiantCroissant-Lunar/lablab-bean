# Data Model: Kernel Memory Integration

**Date**: 2025-10-25
**Feature**: Kernel Memory Integration for NPC Intelligence
**Phase**: 1 - Data Model Design

## Overview

This document defines the data entities and relationships for the Kernel Memory integration feature. The model supports semantic memory retrieval, knowledge base RAG, tactical learning, and relationship tracking for NPCs.

---

## Core Entities

### 1. MemoryEntry

**Purpose**: Represents a single NPC memory that will be stored with semantic embeddings for similarity-based retrieval.

**Attributes**:

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| `EntityId` | string | Yes | Unique identifier of the NPC who owns this memory | Not null/empty |
| `EventType` | string | Yes | Category of memory (e.g., "decision", "interaction", "event") | Not null/empty |
| `Description` | string | Yes | Natural language description of the memory | Max 2000 chars |
| `Timestamp` | DateTime | Yes | When the memory was created | Valid datetime |
| `Importance` | float | Yes | Importance score for retention prioritization | 0.0-1.0 |
| `Metadata` | Dictionary<string, object> | No | Additional context (emotion, location, etc.) | Any JSON-serializable |

**Relationships**:

- Belongs to one NPC entity (via `EntityId`)
- May reference other entities in `Metadata` (e.g., interaction target)

**Business Rules**:

- Importance score determines promotion from short-term to long-term memory in legacy system
- Description is used for embedding generation (semantic search)
- EventType used for filtering (e.g., retrieve only "interaction" memories)

**Example**:

```csharp
var memory = new MemoryEntry
{
    EntityId = "employee_001",
    EventType = "interaction",
    Description = "Had a difficult conversation with an angry customer about a refund. Customer was frustrated but I remained calm and resolved the issue.",
    Timestamp = DateTime.UtcNow,
    Importance = 0.75f,
    Metadata = new Dictionary<string, object>
    {
        { "emotion", "stressed" },
        { "target_entity", "customer_042" },
        { "outcome", "positive" }
    }
};
```

---

### 2. MemoryRetrievalOptions

**Purpose**: Configuration for memory retrieval queries, specifying filters and limits.

**Attributes**:

| Field | Type | Required | Description | Default |
|-------|------|----------|-------------|---------|
| `EntityId` | string | Yes | Filter to specific NPC's memories | - |
| `MemoryTypes` | List<string> | No | Filter by event types (e.g., ["interaction", "decision"]) | All types |
| `MinImportance` | float? | No | Minimum importance threshold | null (no filter) |
| `TimestampFrom` | DateTime? | No | Retrieve memories after this time | null (all time) |
| `TimestampTo` | DateTime? | No | Retrieve memories before this time | null (all time) |
| `Limit` | int | No | Maximum number of memories to retrieve | 5 |
| `MinRelevance` | float | No | Minimum semantic similarity score (0.0-1.0) | 0.7 |

**Business Rules**:

- `MinRelevance` determines quality threshold for semantic matches
- `Limit` prevents unbounded result sets
- Filters are combined with AND logic (all conditions must match)

**Example**:

```csharp
var options = new MemoryRetrievalOptions
{
    EntityId = "employee_001",
    MemoryTypes = new List<string> { "interaction", "customer_service" },
    MinImportance = 0.6f,
    Limit = 5,
    MinRelevance = 0.75f
};
```

---

### 3. MemoryResult

**Purpose**: Represents a retrieved memory with relevance scoring and source attribution.

**Attributes**:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Memory` | MemoryEntry | Yes | The original memory entry |
| `RelevanceScore` | float | Yes | Semantic similarity to query (0.0-1.0) |
| `DocumentId` | string | Yes | Kernel Memory document ID for tracing |
| `SourcePartition` | string | No | For knowledge base queries: which document section matched |

**Relationships**:

- Derived from `MemoryEntry`
- May reference `KnowledgeBaseDocument` (for RAG queries)

**Business Rules**:

- `RelevanceScore` indicates how well the memory matches the query context
- Higher scores (closer to 1.0) indicate stronger semantic match
- Results typically sorted by `RelevanceScore` descending

**Example**:

```csharp
var result = new MemoryResult
{
    Memory = memoryEntry,
    RelevanceScore = 0.87f,
    DocumentId = "memory_employee_001_638351234567890123",
    SourcePartition = null // Only used for knowledge base queries
};
```

---

### 4. KnowledgeBaseDocument

**Purpose**: Represents indexed reference material (personality files, policies, lore) that NPCs query for behavior grounding.

**Attributes**:

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| `DocumentId` | string | Yes | Unique identifier for the document | Not null/empty |
| `Title` | string | Yes | Human-readable document name | Not null/empty |
| `Content` | string | Yes | Full document text content | Max 50,000 chars |
| `RoleTags` | List<string> | Yes | Which NPC roles can access (e.g., ["boss", "employee"]) | At least one tag |
| `CategoryTags` | List<string> | No | Additional categorization (e.g., ["policy", "lore"]) | Optional |
| `LastUpdated` | DateTime | Yes | When document was last modified | Valid datetime |

**Relationships**:

- Accessible by NPCs matching `RoleTags`
- Many NPCs can query the same document (many-to-many)

**Business Rules**:

- Documents indexed with embeddings for semantic search
- NPCs query documents via natural language questions
- Responses include citations (which document answered the question)
- Updates to documents automatically re-indexed without code changes

**Example**:

```csharp
var document = new KnowledgeBaseDocument
{
    DocumentId = "employee_handbook_v1",
    Title = "Employee Customer Service Guidelines",
    Content = File.ReadAllText("knowledge/employee_handbook.txt"),
    RoleTags = new List<string> { "employee" },
    CategoryTags = new List<string> { "policy", "customer_service" },
    LastUpdated = DateTime.UtcNow
};
```

---

### 5. TacticalObservation

**Purpose**: Specialized memory entry for recording player combat behavior and enemy counter-strategy effectiveness.

**Attributes**:

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| `EnemyEntityId` | string | Yes | Which enemy observed this behavior | Not null/empty |
| `PlayerBehavior` | PlayerBehavior enum | Yes | Type of player behavior observed | Valid enum value |
| `Situation` | string | Yes | Combat context description | Max 500 chars |
| `EffectivenessRating` | float | Yes | How well the observed behavior worked for player | 0.0-1.0 |
| `CounterTactic` | string | No | Tactic used to counter (for learning) | Max 200 chars |
| `Timestamp` | DateTime | Yes | When observation occurred | Valid datetime |
| `Metadata` | Dictionary<string, object> | No | Additional context (location, health, etc.) | JSON-serializable |

**Relationships**:

- Belongs to one tactical enemy entity
- References player behavior patterns (via enum)
- Aggregated to build `PlayerBehaviorProfile`

**Business Rules**:

- Multiple observations of same behavior aggregated to identify patterns
- Effectiveness rating guides which counter-tactics to employ
- Persistent across sessions for long-term tactical learning

**PlayerBehavior Enum**:

```csharp
public enum PlayerBehavior
{
    AggressiveRush,      // Player charges directly
    DefensivePositioning, // Player maintains distance
    HitAndRun,           // Player engages briefly then retreats
    AbilityCombo,        // Player uses specific ability sequence
    Flanking,            // Player attempts to circle around
    Kiting               // Player attacks while moving away
}
```

**Example**:

```csharp
var observation = new TacticalObservation
{
    EnemyEntityId = "tactical_enemy_005",
    PlayerBehavior = PlayerBehavior.AggressiveRush,
    Situation = "Player charged directly through narrow corridor toward enemy group",
    EffectivenessRating = 0.85f, // Worked well for player
    CounterTactic = "flanking", // What we tried
    Timestamp = DateTime.UtcNow,
    Metadata = new Dictionary<string, object>
    {
        { "player_health", 0.75f },
        { "enemy_count", 3 },
        { "terrain", "corridor" }
    }
};
```

---

### 6. RelationshipEvent

**Purpose**: Records interactions between entities (NPC-NPC or NPC-Player) for relationship history tracking.

**Attributes**:

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| `EventId` | string | Yes | Unique identifier for this event | GUID |
| `Participant1Id` | string | Yes | First entity involved | Not null/empty |
| `Participant2Id` | string | Yes | Second entity involved | Not null/empty |
| `InteractionType` | string | Yes | Type of interaction (e.g., "conflict", "collaboration") | Not null/empty |
| `Description` | string | Yes | Natural language description | Max 1000 chars |
| `Outcome` | OutcomeType enum | Yes | Result of interaction | Valid enum value |
| `EmotionalImpact` | string | No | Emotional effect (e.g., "frustrated", "pleased") | Max 100 chars |
| `Timestamp` | DateTime | Yes | When interaction occurred | Valid datetime |

**Relationships**:

- Links two entities (directional or bidirectional based on query)
- Can be queried from either participant's perspective

**Business Rules**:

- Used to build relationship context for dialogue generation
- Semantically searchable (e.g., "conflict events with X")
- Emotional impact affects future interaction tone

**OutcomeType Enum**:

```csharp
public enum OutcomeType
{
    Positive,   // Successful, beneficial outcome
    Neutral,    // No significant impact
    Negative    // Conflict, failure, problematic
}
```

**Example**:

```csharp
var relationshipEvent = new RelationshipEvent
{
    EventId = Guid.NewGuid().ToString(),
    Participant1Id = "employee_001",
    Participant2Id = "boss_003",
    InteractionType = "performance_review",
    Description = "Boss praised employee for excellent customer service during busy period",
    Outcome = OutcomeType.Positive,
    EmotionalImpact = "pleased",
    Timestamp = DateTime.UtcNow
};
```

---

## Kernel Memory Internal Entities

These entities are managed by Kernel Memory internally but are important to understand:

### 7. Document (Kernel Memory)

**Purpose**: Internal KM representation of indexed content.

**Attributes** (abstracted):

- `DocumentId`: Unique identifier
- `Tags`: Key-value pairs for filtering
- `Partitions`: Text chunks with embeddings
- `Embeddings`: Vector representations (managed by KM)

**Note**: We interact with this via `IKernelMemory` interface, not directly.

---

### 8. Citation (Kernel Memory)

**Purpose**: Source attribution for knowledge base RAG responses.

**Attributes** (abstracted):

- `SourceDocument`: Which document answered the question
- `Partition`: Which section of the document
- `RelevanceScore`: How well it matched

**Note**: Returned in knowledge base query results for transparency.

---

## Entity Relationships Diagram

```
┌─────────────────┐
│   NPC Entity    │
│   (EntityId)    │
└────────┬────────┘
         │
         │ owns (1:N)
         │
         ▼
┌─────────────────────────┐
│    MemoryEntry          │◄───┐
│                         │    │
│  - EntityId             │    │ stored as
│  - EventType            │    │
│  - Description          │    │
│  - Timestamp            │    │
│  - Importance           │    │
│  - Metadata             │    │
└─────────────────────────┘    │
         │                     │
         │ retrieved as        │
         │                     │
         ▼                     │
┌─────────────────────────┐   │
│    MemoryResult         │   │
│                         │   │
│  - Memory (ref)         │───┘
│  - RelevanceScore       │
│  - DocumentId           │
│  - SourcePartition      │
└─────────────────────────┘


┌─────────────────────────┐
│  KnowledgeBaseDocument  │
│                         │
│  - DocumentId           │
│  - Title                │
│  - Content              │
│  - RoleTags             │◄──── NPC queries (N:M)
│  - CategoryTags         │
│  - LastUpdated          │
└─────────────────────────┘
         │
         │ provides
         │
         ▼
┌─────────────────────────┐
│    Citation (KM)        │
│                         │
│  - SourceDocument       │
│  - Partition            │
│  - RelevanceScore       │
└─────────────────────────┘


┌─────────────────────────┐
│  TacticalObservation    │
│                         │
│  - EnemyEntityId        │
│  - PlayerBehavior       │
│  - Situation            │
│  - EffectivenessRating  │
│  - CounterTactic        │
│  - Timestamp            │
│  - Metadata             │
└─────────────────────────┘
         │
         │ aggregated into
         │
         ▼
┌─────────────────────────┐
│  PlayerBehaviorProfile  │
│  (Existing)             │
│                         │
│  - BehaviorScores       │
│  - DominantBehavior     │
└─────────────────────────┘


┌─────────────────────────┐
│   NPC Entity 1          │
└───────────┬─────────────┘
            │
            │ participates in
            │
            ▼
┌─────────────────────────┐
│  RelationshipEvent      │
│                         │
│  - Participant1Id       │
│  - Participant2Id       │
│  - InteractionType      │
│  - Description          │
│  - Outcome              │
│  - EmotionalImpact      │
│  - Timestamp            │
└───────────┬─────────────┘
            │
            │ participates in
            │
            ▼
┌─────────────────────────┐
│   NPC Entity 2          │
└─────────────────────────┘
```

---

## Data Flow Examples

### Example 1: Semantic Memory Retrieval

```
1. Employee NPC needs to make a decision about customer interaction

2. MemoryRetrievalOptions created:
   - EntityId: "employee_001"
   - MemoryTypes: ["interaction", "customer_service"]
   - MinRelevance: 0.7
   - Limit: 5

3. Query sent to Kernel Memory:
   "How should I handle this angry customer?"

4. KM returns MemoryResult[] with:
   - 3 relevant past customer interactions
   - RelevanceScores: 0.89, 0.82, 0.75
   - Each includes original MemoryEntry

5. Agent uses descriptions from top 3 results in decision prompt
```

### Example 2: Knowledge Base RAG

```
1. Boss NPC needs guidance on handling underperforming employee

2. Query to knowledge base:
   "What should I do if an employee is underperforming?"

3. Filter by RoleTags: ["boss"]

4. KM searches indexed "Management Policies" document

5. Returns answer with Citation:
   - Answer: "Schedule a one-on-one meeting to discuss..."
   - SourceDocument: "boss_management_policies_v2"
   - Partition: "Section 4.2: Performance Management"

6. Boss NPC decision grounded in company policy
```

### Example 3: Tactical Learning

```
1. Tactical enemy observes player using aggressive rush

2. TacticalObservation created:
   - PlayerBehavior: AggressiveRush
   - Situation: "Player charged through corridor"
   - EffectivenessRating: 0.85 (worked well for player)

3. Stored in Kernel Memory with tags:
   - type: "tactical"
   - behavior: "aggressive_rush"
   - effectiveness: "0.85"

4. Next encounter, enemy queries:
   "How should I counter aggressive rushing players?"

5. KM retrieves similar past observations

6. Enemy employs flanking tactic based on learned patterns
```

---

## Validation Rules Summary

| Entity | Key Validations |
|--------|-----------------|
| `MemoryEntry` | Importance: 0.0-1.0; Description max 2000 chars; EntityId required |
| `MemoryRetrievalOptions` | MinRelevance: 0.0-1.0; Limit > 0; EntityId required |
| `MemoryResult` | RelevanceScore: 0.0-1.0; Memory not null |
| `KnowledgeBaseDocument` | Content max 50,000 chars; At least one RoleTag; DocumentId unique |
| `TacticalObservation` | EffectivenessRating: 0.0-1.0; PlayerBehavior valid enum; Situation max 500 chars |
| `RelationshipEvent` | Participant IDs required; Outcome valid enum; Description max 1000 chars |

---

## Next Steps

Data model design complete. Proceed to:

1. **Generate Contracts**: Create service interfaces (`IMemoryService`, `IKnowledgeBaseService`) in `contracts/` directory
2. **Create Quickstart**: Developer setup guide for local development
3. **Update Agent Context**: Add technology decisions to agent context file
