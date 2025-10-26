# Phase 6 - User Story 4: Tactical Learning - Kickoff

**Created**: 2025-10-26 08:36:10
**Status**: 🚀 STARTING
**Story**: Adaptive Tactical Enemy Behavior
**Priority**: P4
**Tasks**: T056-T068 (13 tasks)
**Estimated Time**: 2-3 hours

---

## 🎯 Goal

Enable tactical enemies to analyze and learn from player combat behavior patterns across multiple encounters and sessions. Enemies adapt their strategies based on observed player tendencies (aggressive rushing, defensive positioning, ability usage patterns), creating evolving combat challenges.

## ✅ Success Criteria

**SC-007**: Tactical enemies adapt strategies based on player behavior patterns, measurable by at least 50% of enemies employing pattern-specific counter-tactics after observing 5+ similar player behaviors.

## 🧪 Acceptance Scenarios

1. **Aggressive Rush Pattern**
   - **Given**: Player uses aggressive rushing tactics in 10 combat encounters
   - **When**: Tactical enemies plan strategy for 11th encounter
   - **Then**: Enemies employ anti-rush tactics (flanking, defensive positioning)

2. **Ability Combo Pattern**
   - **Given**: Player frequently uses specific ability combo
   - **When**: Tactical enemies observe pattern 5+ times
   - **Then**: Enemies adjust positioning and timing to counter the combo

3. **Cross-Session Learning**
   - **Given**: Tactical observations from session 1 show hit-and-run tactics
   - **When**: Player encounters enemies in session 2
   - **Then**: Enemies use tactics effective against hit-and-run strategies

## 📋 Task Breakdown

### 1️⃣ Tests First (T056-T058) - 3 tasks

- [ ] **T056**: Unit test for `StoreTacticalObservationAsync`
  - File: `dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/TacticalMemoryTests.cs`
  - Purpose: Verify tactical observations are stored with behavior tags

- [ ] **T057**: Unit test for `RetrieveSimilarTacticsAsync`
  - File: `dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/TacticalMemoryTests.cs`
  - Purpose: Verify similar tactical patterns are retrieved correctly

- [ ] **T058**: Integration test for tactical learning loop
  - File: `dotnet/framework/tests/LablabBean.AI.Agents.Tests/Integration/TacticalLearningTests.cs`
  - Purpose: End-to-end test of observation → storage → retrieval → adaptation

### 2️⃣ DTOs (T059) - 1 task

- [ ] **T059**: Create/verify `TacticalObservation` DTO
  - File: `dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs`
  - Fields: PlayerBehavior, EncounterContext, TacticEffectiveness, Timestamp
  - Purpose: Standard format for tactical observations

### 3️⃣ Memory Service Extensions (T060-T061) - 2 tasks

- [ ] **T060**: Implement `StoreTacticalObservationAsync`
  - File: `dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs`
  - Logic: Store with behavior-specific tagging for filtering

- [ ] **T061**: Implement `RetrieveSimilarTacticsAsync`
  - File: `dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs`
  - Logic: Semantic search filtered by behavior type

### 4️⃣ TacticsAgent Integration (T062-T066) - 5 tasks

- [ ] **T062**: Store observations after encounters
  - File: `dotnet/framework/LablabBean.AI.Agents/TacticsAgent.cs`
  - Logic: Call `StoreTacticalObservationAsync` post-combat

- [ ] **T063**: Retrieve similar encounters before planning
  - File: `dotnet/framework/LablabBean.AI.Agents/TacticsAgent.cs`
  - Logic: Call `RetrieveSimilarTacticsAsync` pre-combat

- [ ] **T064**: Counter-tactic selection
  - File: `dotnet/framework/LablabBean.AI.Agents/TacticsAgent.cs`
  - Logic: Select tactics based on effectiveness ratings

- [ ] **T065**: Pattern aggregation logic
  - File: `dotnet/framework/LablabBean.AI.Agents/TacticsAgent.cs`
  - Logic: Identify dominant player behavior patterns

- [ ] **T066**: Logging
  - File: `dotnet/framework/LablabBean.AI.Agents/TacticsAgent.cs`
  - Purpose: Debug and monitoring

### 5️⃣ Documentation (T067-T068) - 2 tasks

- [ ] **T067**: Update tactical learning documentation
- [ ] **T068**: Add tactical learning examples

---

## 🏗️ Technical Approach

### 1. TacticalObservation DTO Structure

```csharp
public class TacticalObservation
{
    public string PlayerId { get; set; }
    public PlayerBehavior BehaviorType { get; set; }  // Aggressive, Defensive, Balanced
    public string EncounterContext { get; set; }       // Description of encounter
    public Dictionary<string, float> TacticEffectiveness { get; set; }  // Tactic -> Success rate
    public DateTimeOffset Timestamp { get; set; }
}

public enum PlayerBehavior
{
    AggressiveRush,
    DefensivePosture,
    HitAndRun,
    RangedKiting,
    AbilitySpam,
    ComboBased
}
```

### 2. Memory Service Methods

```csharp
Task StoreTacticalObservationAsync(
    string entityId,
    TacticalObservation observation,
    CancellationToken ct = default);

Task<List<MemoryResult>> RetrieveSimilarTacticsAsync(
    string entityId,
    PlayerBehavior behaviorFilter,
    int limit = 5,
    CancellationToken ct = default);
```

### 3. TacticsAgent Integration Flow

```
Combat Start → RetrieveSimilarTacticsAsync
              ↓
         Analyze patterns
              ↓
         Select counter-tactics
              ↓
         Execute plan
              ↓
Combat End → StoreTacticalObservationAsync
```

---

## 🔍 Discovery Notes

### Existing TacticsAgent Investigation

Need to check:

1. Does `TacticsAgent.cs` already exist?
2. What's the current tactical planning API?
3. Integration points with combat system?

Let's discover and adapt to existing architecture!

---

## 📊 Dependencies

### Required Infrastructure (Already Complete ✅)

- ✅ US1: Semantic memory retrieval
- ✅ US2: Persistent storage with Qdrant
- ✅ Core `MemoryService` implementation

### New Components Needed

- [ ] TacticalObservation DTO
- [ ] Tactical memory methods
- [ ] TacticsAgent (or equivalent) integration

---

## 🎯 Session Goals

### Hour 1: Tests & DTOs (T056-T059)

- Create test files
- Define TacticalObservation DTO
- Write failing tests

### Hour 2: Memory Service (T060-T061)

- Implement tactical storage
- Implement tactical retrieval
- Pass memory service tests

### Hour 3: Agent Integration (T062-T066)

- Discover/adapt TacticsAgent
- Store observations post-combat
- Retrieve & use patterns pre-combat
- Pass integration tests

---

## 📁 Key Files

### New Files to Create

```
dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/TacticalMemoryTests.cs
dotnet/framework/tests/LablabBean.AI.Agents.Tests/Integration/TacticalLearningTests.cs
```

### Files to Modify

```
dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs
dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
dotnet/framework/LablabBean.AI.Agents/TacticsAgent.cs (or equivalent)
```

---

## 🚀 Let's Start

**Current Status**: 55/80 tasks (69%)
**After US4**: 68/80 tasks (85%)
**Progress Gain**: +13 tasks (+16%)

Ready to implement tactical enemy learning! 🧠🎯

---

**Next Steps**:

1. Discover existing tactical/combat agent structure
2. Create test infrastructure
3. Implement tactical memory extensions
4. Integrate with combat system

Let's make enemies smart! 💪
