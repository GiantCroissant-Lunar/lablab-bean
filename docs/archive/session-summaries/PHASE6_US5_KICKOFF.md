# 🚀 Phase 6 - User Story 5: Relationship Memory - KICKOFF

**Started**: 2025-10-26 00:59 UTC
**Goal**: NPCs maintain rich, searchable relationship histories with semantic retrieval
**Tasks**: 12 (T069-T080)
**Estimated Duration**: 2-3 hours

---

## 🎯 User Story

**AS A** player
**I WANT** NPCs to remember our relationship history and past interactions
**SO THAT** dialogue and behavior feel personalized and contextually aware

### Success Criteria

- ✅ Relationship memories stored with semantic embeddings
- ✅ NPCs retrieve contextually relevant past interactions
- ✅ Cross-entity relationship tracking (Alice ↔ Bob)
- ✅ Relationship sentiment analysis (positive/negative/neutral)
- ✅ 80% of interactions reflect relevant relationship history

---

## 📋 Task Breakdown

### Phase 1: Tests (T069-T071) - 3 tasks

Write tests FIRST, ensure they FAIL before implementation

- [ ] **T069** - Unit test for StoreRelationshipMemoryAsync
  - File: `dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/RelationshipMemoryTests.cs`
  - Tests: Store relationship interaction, verify tags, check sentiment

- [ ] **T070** - Unit test for RetrieveRelevantRelationshipHistoryAsync
  - File: `dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/RelationshipMemoryTests.cs`
  - Tests: Query by entities, semantic search, filter by sentiment

- [ ] **T071** - Integration test for relationship-aware dialogue
  - File: `dotnet/framework/tests/LablabBean.AI.Agents.Tests/Integration/RelationshipDialogueTests.cs`
  - Tests: Full workflow with multiple interactions, verify context awareness

### Phase 2: DTOs (T072-T073) - 2 tasks

- [ ] **T072** - Create RelationshipMemory DTO
  - File: `dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs`
  - Fields: `Entity1Id, Entity2Id, InteractionType, Sentiment, Description, Timestamp`

- [ ] **T073** - Create InteractionType enum
  - File: `dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs`
  - Values: `Conversation, Trade, Combat, Collaboration, Betrayal, Gift, Quest`

### Phase 3: Memory Service (T074-T075) - 2 tasks

- [ ] **T074** - Implement StoreRelationshipMemoryAsync
  - File: `dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs`
  - Add relationship-specific tags (both entity IDs, interaction type, sentiment)
  - Generate embeddings for semantic search

- [ ] **T075** - Implement RetrieveRelevantRelationshipHistoryAsync
  - File: `dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs`
  - Semantic search filtered by entity pair
  - Optional sentiment filtering
  - Return chronologically ordered results with relevance scores

### Phase 4: NPC Integration (T076-T079) - 4 tasks

- [ ] **T076** - Update EmployeeIntelligenceAgent with relationship memory storage
  - File: `dotnet/framework/LablabBean.AI.Agents/EmployeeIntelligenceAgent.cs`
  - Store post-interaction relationship memories
  - Include sentiment analysis

- [ ] **T077** - Update EmployeeIntelligenceAgent with relationship context retrieval
  - File: `dotnet/framework/LablabBean.AI.Agents/EmployeeIntelligenceAgent.cs`
  - Retrieve relevant relationship history before decision
  - Include in prompt context

- [ ] **T078** - Update BossIntelligenceAgent with relationship memory storage
  - File: `dotnet/framework/LablabBean.AI.Agents/BossIntelligenceAgent.cs`
  - Store post-interaction relationship memories

- [ ] **T079** - Update BossIntelligenceAgent with relationship context retrieval
  - File: `dotnet/framework/LablabBean.AI.Agents/BossIntelligenceAgent.cs`
  - Retrieve relevant relationship history before decision

### Phase 5: Polish (T080) - 1 task

- [ ] **T080** - Add comprehensive logging for relationship operations
  - Files: All modified services/agents
  - Log: Storage events, retrieval queries, sentiment changes, relationship milestones

---

## 🏗️ Technical Design

### RelationshipMemory DTO

```csharp
public record RelationshipMemory
{
    public string Entity1Id { get; init; } = string.Empty;
    public string Entity2Id { get; init; } = string.Empty;
    public InteractionType InteractionType { get; init; }
    public string Sentiment { get; init; } = "neutral"; // positive, negative, neutral
    public string Description { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

public enum InteractionType
{
    Conversation,
    Trade,
    Combat,
    Collaboration,
    Betrayal,
    Gift,
    Quest
}
```

### Memory Tagging Strategy

```
Tags:
- entity1:{entity1Id}
- entity2:{entity2Id}
- relationship:{entity1Id}_{entity2Id}
- interaction:{interactionType}
- sentiment:{sentiment}
- type:relationship

Description: "{entity1Name} and {entity2Name} {interaction}: {description}"
```

### Semantic Search Pattern

```csharp
// Retrieve relationship history between two entities
var history = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
    entity1Id: playerId,
    entity2Id: npcId,
    query: currentInteractionContext,
    maxResults: 5,
    sentiment: "positive" // Optional filter
);
```

---

## 🔄 Integration Points

### EmployeeIntelligenceAgent Changes

**Before Decision**:

```csharp
// Retrieve relationship history with player
var relationshipHistory = await _memoryService
    .RetrieveRelevantRelationshipHistoryAsync(
        employeeId,
        playerId,
        currentContext
    );

// Include in prompt
prompt += $"\nRelationship History:\n{FormatHistory(relationshipHistory)}";
```

**After Interaction**:

```csharp
// Store relationship memory
await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
{
    Entity1Id = employeeId,
    Entity2Id = playerId,
    InteractionType = InteractionType.Conversation,
    Sentiment = AnalyzeSentiment(outcome),
    Description = $"Discussed {topic}, outcome: {outcome}",
    Timestamp = DateTime.UtcNow
});
```

---

## 🎪 Example Scenario

### Initial Interaction

**Player**: "Hey Alice, can you help me with this report?"
**Alice**: "Sure, happy to help!" (No prior history)

**Memory Stored**:

```
Entity1: alice_123
Entity2: player_456
Type: Collaboration
Sentiment: positive
Description: "Player requested help with report, Alice agreed enthusiastically"
```

### Later Interaction

**Player**: "Alice, I need another favor..."
**Alice**: "Of course! You helped me last week, remember? I owe you one!"

**Context Retrieved**:

- Previous collaboration (positive sentiment)
- Past favor exchange
- Established trust relationship

---

## ✅ Validation Tests

1. **Basic Storage** - Store relationship memory, verify retrieval by entity pair
2. **Semantic Search** - Similar contexts retrieve relevant past interactions
3. **Sentiment Filtering** - Can filter history by positive/negative/neutral
4. **Chronological Order** - Results ordered by relevance, then recency
5. **Cross-Entity** - Works bidirectionally (Alice→Bob same as Bob→Alice)
6. **Multiple Relationships** - NPC maintains distinct histories with multiple entities

---

## 📊 Success Metrics

- **Storage**: All relationship interactions captured
- **Retrieval Accuracy**: 80% of retrieved memories are contextually relevant
- **Sentiment Accuracy**: 75% agreement with manual sentiment labeling
- **Performance**: Retrieval < 500ms for 5 results
- **Context Impact**: Measurable difference in NPC responses with/without history

---

## 🚀 Let's Build

**Starting with**: T069 (Unit tests for relationship memory storage)
**TDD Approach**: Tests → Implementation → Validation
**Progress Tracking**: Update after each task

---

**Previous**: User Story 4 (Tactical Learning) ✅
**Current**: User Story 5 (Relationship Memory) 🔄
**Phase 6 Progress**: 68/80 tasks (85%)
