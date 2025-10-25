# Phase 5: Combat Memory & Dynamic Enemy AI

**Status**: 🚀 Starting
**Priority**: High
**Started**: 2025-10-25
**Depends On**: Phase 4 (Memory-Enhanced NPC System)

---

## 📋 Overview

**Goal**: Extend the memory system to track combat encounters, enabling enemies and NPCs to remember fights, adapt tactics, and create dynamic combat experiences.

**Key Features**:

- Combat encounter memory storage
- Enemy tactic adaptation based on past fights
- Revenge/grudge system for defeated enemies
- Combat analytics and statistics
- Integration with relationship system

---

## 🎯 User Stories

### US1: Enemy Remembers Defeat

**As a** player
**I want** enemies to remember when I defeated them
**So that** they can seek revenge or avoid me in future encounters

**Acceptance Criteria**:

- ✅ Enemy tracks previous combat outcomes
- ✅ Enemy remembers player tactics (spells used, attack patterns)
- ✅ Enemy becomes more aggressive after defeats
- ✅ Enemy avoids player after multiple defeats

### US2: Adaptive Combat Tactics

**As a** player
**I want** enemies to adapt their tactics based on previous fights
**So that** combat remains challenging and unpredictable

**Acceptance Criteria**:

- ✅ Enemy learns player's preferred attack types
- ✅ Enemy changes tactics to counter player's strategy
- ✅ Enemy remembers successful/failed tactics
- ✅ Tactic adaptation happens dynamically during combat

### US3: Combat Statistics Dashboard

**As a** developer/admin
**I want** to view combat analytics for NPCs/enemies
**So that** I can balance combat and debug AI behavior

**Acceptance Criteria**:

- ✅ Track total fights, wins, losses per NPC
- ✅ Track damage dealt/received over time
- ✅ Identify most used player tactics
- ✅ Export combat analytics to file

---

## 🏗️ Architecture

### New Components

#### 1. CombatMemory Component

```csharp
public struct CombatMemory
{
    public Dictionary<Guid, CombatHistory> Encounters { get; set; }
    public DateTime LastCombatTime { get; set; }
    public int TotalCombats { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public CombatEmotionalState EmotionalState { get; set; }
}

public class CombatHistory
{
    public Guid OpponentId { get; set; }
    public List<CombatEncounter> Encounters { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public DateTime LastFight { get; set; }
    public CombatRelationship Relationship { get; set; } // Rival, Nemesis, Afraid
}

public class CombatEncounter
{
    public DateTime Timestamp { get; set; }
    public CombatOutcome Outcome { get; set; } // Win, Loss, Flee, Draw
    public int DamageDealt { get; set; }
    public int DamageReceived { get; set; }
    public List<string> TacticsUsed { get; set; }
    public List<string> OpponentTactics { get; set; }
    public TimeSpan Duration { get; set; }
    public int TurnsToWin { get; set; }
}

public enum CombatEmotionalState
{
    Neutral,
    Confident,
    Cautious,
    Afraid,
    Vengeful,
    Desperate
}

public enum CombatRelationship
{
    Neutral,
    Rival,      // Evenly matched
    Nemesis,    // Bitter enemy (multiple defeats)
    Afraid,     // Dominated by opponent
    Hunter      // Seeking revenge
}
```

#### 2. TacticalMemory Component

```csharp
public struct TacticalMemory
{
    public Dictionary<string, TacticAnalysis> LearnedTactics { get; set; }
    public Dictionary<string, TacticCounters> CounterStrategies { get; set; }
    public List<string> SuccessfulTactics { get; set; }
    public List<string> FailedTactics { get; set; }
}

public class TacticAnalysis
{
    public string TacticName { get; set; }
    public int TimesUsed { get; set; }
    public int SuccessCount { get; set; }
    public double SuccessRate { get; set; }
    public double AverageDamage { get; set; }
    public DateTime LastUsed { get; set; }
}

public class TacticCounters
{
    public string OpponentTactic { get; set; }
    public List<string> EffectiveCounters { get; set; }
    public Dictionary<string, double> CounterSuccessRates { get; set; }
}
```

### New Systems

#### 1. CombatMemorySystem

**Responsibilities**:

- Track combat start/end events
- Record damage dealt/received
- Store combat outcomes
- Update combat statistics
- Determine combat relationships

**Key Methods**:

- `OnCombatStart(Entity attacker, Entity defender)`
- `OnCombatEnd(Entity winner, Entity loser, CombatEncounter details)`
- `RecordDamage(Entity attacker, Entity target, int damage, string tactic)`
- `UpdateCombatRelationship(Entity npc, Entity opponent)`

#### 2. AdaptiveTacticsSystem

**Responsibilities**:

- Analyze combat memory
- Identify player patterns
- Select appropriate counter-tactics
- Update tactic success rates
- Adjust AI behavior dynamically

**Key Methods**:

- `AnalyzeOpponentTactics(Guid playerId, CombatMemory memory)`
- `SelectCounterTactic(string playerTactic, TacticalMemory tactics)`
- `UpdateTacticSuccessRate(string tactic, bool success)`
- `GetAdaptedBehavior(Entity enemy, Entity player)`

### New Services

#### ICombatMemoryService

```csharp
public interface ICombatMemoryService
{
    Task<CombatHistory> GetCombatHistoryAsync(Guid npcId, Guid opponentId);
    Task RecordCombatAsync(Guid npcId, CombatEncounter encounter);
    Task<CombatAnalytics> GetCombatAnalyticsAsync(Guid npcId);
    Task<List<CombatEncounter>> GetRecentCombatsAsync(Guid npcId, int count = 10);
    Task<CombatRelationship> GetCombatRelationshipAsync(Guid npcId, Guid opponentId);
    Task<Dictionary<string, TacticAnalysis>> GetTacticAnalysisAsync(Guid npcId);
    Task<string> GetOptimalCounterTacticAsync(Guid npcId, string playerTactic);
}

public class CombatAnalytics
{
    public int TotalCombats { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public double WinRate { get; set; }
    public int TotalDamageDealt { get; set; }
    public int TotalDamageReceived { get; set; }
    public double AverageCombatDuration { get; set; }
    public Dictionary<string, int> TacticUsageCount { get; set; }
    public Dictionary<Guid, CombatRelationship> Relationships { get; set; }
}
```

---

## 🔄 Integration Points

### 1. With Phase 4 Memory System

```csharp
// Combat affects relationships
public void OnCombatEnd(Entity winner, Entity loser)
{
    // Record in combat memory
    combatMemoryService.RecordCombatAsync(loser.Id, encounter);

    // Update relationship (combat creates negative interactions)
    var interaction = new Interaction
    {
        Type = InteractionType.Combat,
        Timestamp = DateTime.UtcNow,
        SentimentScore = -30, // Combat is negative
        Context = $"Lost fight to {winner.Name}"
    };

    memoryService.RecordInteractionAsync(loser.Id, winner.Id, interaction);
}
```

### 2. With Combat System

```csharp
// Hook into existing combat events
public class CombatEventHandler
{
    public void OnDamageDealt(DamageEvent evt)
    {
        combatMemorySystem.RecordDamage(evt.Attacker, evt.Target, evt.Damage, evt.TacticUsed);
    }

    public void OnCombatStart(CombatStartEvent evt)
    {
        // Check combat memory
        var history = combatMemoryService.GetCombatHistoryAsync(evt.Enemy.Id, evt.Player.Id);

        // Adjust enemy behavior based on history
        if (history.Losses >= 3)
        {
            evt.Enemy.SetEmotionalState(CombatEmotionalState.Afraid);
            evt.Enemy.SetBehavior(BehaviorType.Cautious);
        }
    }
}
```

### 3. With AI System

```csharp
// Adaptive AI decision-making
public string SelectTactic(Entity enemy, Entity player)
{
    var memory = enemy.Get<CombatMemory>();
    var tactical = enemy.Get<TacticalMemory>();

    // Analyze player's previous tactics
    var playerPattern = adaptiveTacticsSystem.AnalyzeOpponentTactics(player.Id, memory);

    // Select counter-tactic
    var counterTactic = adaptiveTacticsSystem.SelectCounterTactic(playerPattern, tactical);

    return counterTactic;
}
```

---

## 📊 Example Scenarios

### Scenario 1: First Encounter

```
Player attacks Goblin #42 for the first time
→ Goblin uses default tactics (aggressive melee)
→ Player defeats Goblin with fire spells
→ Combat memory recorded:
  - Outcome: Loss
  - Player tactic: Fire Spells
  - Damage received: 80hp
```

### Scenario 2: Revenge Encounter

```
Player encounters Goblin #42 again (3 days later)
→ Goblin remembers previous defeat
→ Emotional state: Vengeful
→ Tactic adapted: Keeps distance, throws rocks
→ Dialogue: "You! I remember you! You won't burn me again!"
```

### Scenario 3: Multiple Defeats (Fear Response)

```
Player defeats Goblin #42 four times
→ Combat relationship: Afraid
→ Goblin flees on sight
→ Dialogue: "No! Not you again! I yield!"
→ Won't engage unless cornered
```

### Scenario 4: Evenly Matched (Rivalry)

```
Player trades wins/losses with Orc Warrior
→ Combat relationship: Rival
→ Orc studies player tactics intensely
→ Adapts strategy every fight
→ Dialogue: "Another round? I've learned from our last fight!"
```

---

## 🎨 Production Examples

### Example 1: RevengeSeekingEnemy

**File**: `Examples/RevengeSeekingEnemy.cs`

**Features**:

- Tracks defeat history
- Increases aggression after losses
- Uses adapted tactics
- Special dialogue for revenge encounters

### Example 2: AdaptiveRivalSystem

**File**: `Examples/AdaptiveRivalSystem.cs`

**Features**:

- Identifies rival relationships
- Escalates difficulty over time
- Learns player's weaknesses
- Creates dynamic boss-like encounters

### Example 3: CombatAnalyticsDashboard

**File**: `Examples/CombatAnalyticsDashboard.cs`

**Features**:

- Visualizes combat statistics
- Shows tactic success rates
- Identifies dominant strategies
- Exports detailed reports

---

## 📝 Implementation Tasks

### Phase 5.1: Combat Memory Tracking (2-3 hours)

- [ ] **T101**: Create `CombatMemory` component
- [ ] **T102**: Create `TacticalMemory` component
- [ ] **T103**: Implement `CombatMemorySystem`
- [ ] **T104**: Implement `ICombatMemoryService`
- [ ] **T105**: Add combat event handlers

### Phase 5.2: Adaptive Tactics (3-4 hours)

- [ ] **T106**: Implement `AdaptiveTacticsSystem`
- [ ] **T107**: Create tactic analysis algorithms
- [ ] **T108**: Implement counter-tactic selection
- [ ] **T109**: Add tactic success rate tracking
- [ ] **T110**: Integrate with AI system

### Phase 5.3: Production Examples (2-3 hours)

- [ ] **T111**: Create `RevengeSeekingEnemy.cs`
- [ ] **T112**: Create `AdaptiveRivalSystem.cs`
- [ ] **T113**: Create `CombatAnalyticsDashboard.cs`

### Phase 5.4: Testing & Documentation (2 hours)

- [ ] **T114**: Write unit tests for CombatMemoryService
- [ ] **T115**: Write unit tests for AdaptiveTacticsSystem
- [ ] **T116**: Create USAGE_EXAMPLES.md
- [ ] **T117**: Update ARCHITECTURE.md
- [ ] **T118**: Create PHASE5_SUMMARY.md

**Total Estimated Time**: 9-12 hours

---

## ✅ Success Criteria

### Functional Requirements

- ✅ Enemies remember combat outcomes
- ✅ Tactics adapt based on player patterns
- ✅ Combat relationships evolve (Rival, Nemesis, Afraid)
- ✅ Analytics track all combat statistics
- ✅ Integration with Phase 4 memory system

### Code Quality

- ✅ Zero compilation errors
- ✅ XML documentation complete
- ✅ Unit tests with 80%+ coverage
- ✅ Production-ready examples

### Documentation

- ✅ Complete usage guide
- ✅ Architecture documentation
- ✅ API reference
- ✅ Integration examples

---

## 🚀 Getting Started

### Step 1: Review Phase 4

```bash
# Review existing memory system
cat dotnet/plugins/LablabBean.Plugins.NPC/PHASE4_REFINEMENT_SUMMARY.md
```

### Step 2: Start Implementation

```bash
# Create combat memory components
# Start with T101-T105
```

### Step 3: Test & Iterate

```bash
# Build and test
dotnet build
dotnet test
```

---

## 📚 References

- **Phase 4 Summary**: `PHASE4_REFINEMENT_SUMMARY.md`
- **Memory System**: `Services/NPCMemoryService.cs`
- **Relationship System**: `Systems/MemoryEnhancedDialogueSystem.cs`
- **Examples**: `Examples/` directory

---

**Ready to begin Phase 5? Let's make combat dynamic and memorable!** 🎮⚔️

**Status**: Ready for implementation
**Next Action**: Start with T101 (CombatMemory component)
