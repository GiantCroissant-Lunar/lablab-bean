# Phase 6 - User Story 4: Tactical Learning - Index

**Status**: 🚀 IN PROGRESS
**Started**: 2025-10-26 08:36:10
**Tasks**: T056-T068 (13 tasks)

---

## 📊 Progress Tracker

| Phase | Tasks | Status |
|-------|-------|--------|
| Tests | T056-T058 (3) | ⏸️ WAITING |
| DTOs | T059 (1) | ⏸️ WAITING |
| Memory Service | T060-T061 (2) | ⏸️ WAITING |
| TacticsAgent | T062-T066 (5) | ⏸️ WAITING |
| Documentation | T067-T068 (2) | ⏸️ WAITING |

**Total**: 0/13 complete (0%)

---

## 🎯 User Story 4 Goal

Enable tactical enemies to **learn from player combat patterns** and **adapt their strategies** based on observed behaviors across encounters and sessions.

---

## 🏗️ Architecture Discovery

### Existing Components ✅

1. **TacticsAgent.cs** (LablabBean.AI.Agents)
   - ✅ `TrackPlayerBehavior(playerId, behaviorType, intensity)`
   - ✅ `CreateTacticalPlanAsync(context, playerId, ct)`
   - ✅ `CreateGroupTacticsPlanAsync(contexts, playerId, ct)`
   - ✅ Uses in-memory `PlayerBehaviorProfile`

2. **PlayerBehaviorType enum** (LablabBean.AI.Core.Events)

   ```csharp
   public enum PlayerBehaviorType {
       Unknown, RangedAttacks, MeleeAggressive, HitAndRun,
       Defensive, HealingFocused, AreaOfEffect, StatusEffects, Kiting
   }
   ```

3. **PlayerBehaviorObservedEvent** (LablabBean.AI.Core.Events)
   - Event-based behavior tracking system

4. **MemoryService** (LablabBean.AI.Agents.Services)
   - ✅ Semantic storage & retrieval
   - ✅ Qdrant persistence
   - ⏸️ Missing: Tactical-specific methods

### Integration Strategy

**Enhance existing TacticsAgent** with persistent memory:

- Add `IMemoryService` dependency
- Store tactical observations in Qdrant (cross-session learning)
- Retrieve similar past encounters before planning
- Replace in-memory profiles with semantic memory queries

---

## 📝 Task Details

### T056-T058: Tests (3 tasks)

**T056**: Unit test for `StoreTacticalObservationAsync`

- **File**: `TacticalMemoryTests.cs`
- **Verify**: Observations stored with correct tags
- **Tags**: `behaviorType`, `tactic`, `outcome`

**T057**: Unit test for `RetrieveSimilarTacticsAsync`

- **File**: `TacticalMemoryTests.cs`
- **Verify**: Semantic retrieval of similar encounters
- **Filter**: By behavior type, limit results

**T058**: Integration test for tactical learning loop

- **File**: `TacticalLearningTests.cs`
- **Scenario**:
  1. Player uses aggressive rush 5x
  2. Enemy stores observations
  3. Enemy retrieves patterns
  4. Enemy adapts to anti-rush tactics

### T059: TacticalObservation DTO (1 task)

```csharp
public record TacticalObservation
{
    public required string PlayerId { get; init; }
    public required PlayerBehaviorType BehaviorType { get; init; }
    public required string EncounterContext { get; init; }
    public required Dictionary<string, float> TacticEffectiveness { get; init; }
    public OutcomeType Outcome { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
```

### T060-T061: Memory Service Extensions (2 tasks)

**T060**: `StoreTacticalObservationAsync`

```csharp
public async Task StoreTacticalObservationAsync(
    string entityId,
    TacticalObservation observation,
    CancellationToken ct = default)
```

- Tags: `behavior:{BehaviorType}`, `outcome:{Outcome}`
- Content: JSON-serialized observation

**T061**: `RetrieveSimilarTacticsAsync`

```csharp
public async Task<List<MemoryResult>> RetrieveSimilarTacticsAsync(
    string entityId,
    PlayerBehaviorType behaviorFilter,
    int limit = 5,
    CancellationToken ct = default)
```

- Query: "Player behavior: {behaviorFilter}"
- Filter by memory type = "tactical"

### T062-T066: TacticsAgent Integration (5 tasks)

**T062**: Store observations after encounters

- Location: End of `CreateTacticalPlanAsync`
- Build `TacticalObservation` from plan results
- Call `MemoryService.StoreTacticalObservationAsync`

**T063**: Retrieve similar encounters before planning

- Location: Start of `CreateTacticalPlanAsync`
- Call `MemoryService.RetrieveSimilarTacticsAsync`
- Parse past observations from results

**T064**: Counter-tactic selection

- Use effectiveness ratings from past observations
- Boost tactics that succeeded against behavior type
- Penalize tactics that failed

**T065**: Pattern aggregation logic

- Analyze last N observations
- Identify dominant behavior (80%+ frequency)
- Surface pattern to tactic selection

**T066**: Logging

- Log tactical observations stored
- Log similar encounters retrieved
- Log counter-strategy selection reasoning

---

## 🔄 Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│ COMBAT ENCOUNTER                                            │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ TacticsAgent.CreateTacticalPlanAsync()                      │
│                                                              │
│ 1. RetrieveSimilarTacticsAsync(behaviorType) ───────────────┼──► IMemoryService
│    ↓                                                         │    (Qdrant)
│ 2. Parse past observations                                  │
│    ↓                                                         │
│ 3. Analyze patterns (aggregate effectiveness)               │
│    ↓                                                         │
│ 4. Select counter-tactics                                   │
│    ↓                                                         │
│ 5. Execute tactical plan                                    │
│    ↓                                                         │
│ 6. StoreTacticalObservationAsync(results) ──────────────────┼──► IMemoryService
│                                                              │    (Qdrant)
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ NEXT ENCOUNTER (Same player, similar behavior)              │
│ → Enemy adapts based on past effectiveness                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 📦 Files to Create/Modify

### New Files

```
dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/TacticalMemoryTests.cs
dotnet/framework/tests/LablabBean.AI.Agents.Tests/Integration/TacticalLearningTests.cs
```

### Modified Files

```
dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs
dotnet/framework/LablabBean.Contracts.AI/Memory/IMemoryService.cs
dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
dotnet/framework/LablabBean.AI.Agents/TacticsAgent.cs
```

---

## ✅ Success Criteria

**SC-007**: Tactical enemies adapt strategies based on player behavior patterns, measurable by at least 50% of enemies employing pattern-specific counter-tactics after observing 5+ similar player behaviors.

**Verification**:

1. ✅ Observations persist across app restarts
2. ✅ Similar patterns retrieved semantically
3. ✅ Counter-tactics selected based on effectiveness
4. ✅ Tests demonstrate adaptation loop

---

## 🚀 Next Actions

1. Create test file structure
2. Write failing tests (T056-T058)
3. Add TacticalObservation DTO (T059)
4. Implement memory methods (T060-T061)
5. Integrate with TacticsAgent (T062-T066)

---

**Let's make enemies learn! 🧠🎯**
