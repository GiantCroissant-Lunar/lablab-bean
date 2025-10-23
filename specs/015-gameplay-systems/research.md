# Research: Core Gameplay Systems

**Feature**: Core Gameplay Systems (Quest, NPC/Dialogue, Progression, Spells, Merchant, Boss, Hazards)
**Date**: 2025-10-23
**Status**: Complete

## Overview

This document captures architectural and design decisions for implementing seven gameplay systems in the Lablab-Bean dungeon crawler. All systems leverage the existing Arch ECS framework and plugin architecture.

---

## 1. Quest System Architecture

### Decision: Event-Driven Quest Progress Tracking

**Chosen Approach**: Quest objectives subscribe to game events (OnEnemyKilled, OnItemCollected, OnLocationReached, OnNPCTalk) and update progress automatically.

**Rationale**:

- Decouples quest logic from combat/movement/interaction systems
- Enables multiple quests to track the same event simultaneously
- Aligns with existing event-driven architecture (StatusEffects already use this pattern)
- Simplifies quest definition (declarative objectives rather than imperative checks)

**Alternatives Considered**:

- **Polling-based tracking**: Quest system queries game state each turn → Rejected due to performance overhead and tight coupling
- **Direct system calls**: Combat/Movement systems directly notify QuestSystem → Rejected as it creates bidirectional dependencies

**Implementation**:

```csharp
// Component structure
public struct Quest
{
    public Guid Id;
    public string Name;
    public QuestState State; // NotStarted, Active, Completed, Failed
    public List<QuestObjective> Objectives;
    public QuestRewards Rewards;
}

public struct QuestObjective
{
    public QuestObjectiveType Type; // KillEnemies, CollectItems, ReachLocation, TalkToNPC
    public string Target; // Enemy type, item ID, location name, NPC ID
    public int Required;
    public int Current;
}

// Event subscriptions in QuestSystem
OnEnemyKilled += (enemy) => UpdateKillObjectives(enemy.Type);
OnItemCollected += (item) => UpdateCollectionObjectives(item.Id);
```

**Best Practices**:

- Quest definitions stored as JSON files, loaded via ResourceLoader plugin
- Quest prerequisites checked via simple predicate system (level >= X, hasItem(Y), questCompleted(Z))
- Quest chains implemented as prerequisites referencing previous quest IDs

---

## 2. NPC & Dialogue System Architecture

### Decision: Graph-Based Dialogue Trees with Conditional Branching

**Chosen Approach**: Dialogue represented as directed graph with nodes (NPC speech + player choices) and edges (transitions). Conditions evaluated at runtime to filter available choices.

**Rationale**:

- Handles complex branching narratives (quest acceptance, reputation checks, item requirements)
- Supports dialogue state persistence (remembering previous conversations)
- Industry-standard approach used in RPGs (Dragon Age, Mass Effect, Disco Elysium)
- Enables future tooling (visual dialogue editor)

**Alternatives Considered**:

- **Linear scripts**: Simple sequence of text → Rejected, no player choice support
- **State machine**: Explicit states and transitions → Rejected as overly rigid for non-linear dialogue
- **Behavior trees**: Hierarchical structure → Rejected, overcomplicated for dialogue needs

**Implementation**:

```csharp
public struct NPC
{
    public Guid Id;
    public NPCType Type; // QuestGiver, Merchant, Lore, Hostile
    public DialogueTreeId DialogueTree;
    public Dictionary<string, object> State; // Persistent NPC memory
}

public struct DialogueTree
{
    public Guid Id;
    public DialogueNode RootNode;
    public Dictionary<Guid, DialogueNode> Nodes;
}

public struct DialogueNode
{
    public Guid Id;
    public string NPCText;
    public List<DialogueChoice> Choices;
}

public struct DialogueChoice
{
    public string PlayerText;
    public Guid NextNodeId;
    public Condition Condition; // e.g., "PlayerLevel >= 5", "HasItem(artifact)", "QuestActive(quest1)"
    public Action Action; // e.g., AcceptQuest, StartTrade, RevealLore
}
```

**Best Practices**:

- Dialogue trees stored as JSON, referenced by NPC ID
- Condition evaluation uses expression parser (simple DSL: `level >= 5 AND hasItem('key')`)
- Dialogue state persisted per NPC (track conversation progress, choices made)
- Use existing Event system for dialogue actions (PublishEvent<QuestAcceptedEvent>)

---

## 3. Character Progression System

### Decision: Level-Based Stat Scaling with Diminishing Returns

**Chosen Approach**: Players gain XP from combat/quests, level up at exponential thresholds, receive fixed stat bonuses per level with soft caps.

**Rationale**:

- Balances early power growth (satisfying) with late-game progression (prevents runaway scaling)
- Standard RPG formula: `XPRequired(level) = BaseXP * (level^1.8)` provides smooth curve
- Fixed stat increases per level simplify balance (predictable power curve)
- Soft caps (e.g., max level 50) prevent infinite progression abuse

**Alternatives Considered**:

- **Skill-based progression**: XP allocated to individual skills → Rejected as too complex for scope
- **Linear XP scaling**: Same XP per level → Rejected, makes early levels too grindy
- **Random stat increases**: Dice rolls on level-up → Rejected, introduces unpredictability harmful for balance

**Implementation**:

```csharp
public struct Experience
{
    public int CurrentXP;
    public int Level;
    public int XPToNextLevel;
}

public struct LevelUpStats
{
    public int HealthBonus = 10;
    public int AttackBonus = 2;
    public int DefenseBonus = 1;
    public int ManaBonus = 5;
    public int SpeedBonus = 1;
}

// XP calculation
public int CalculateXPRequired(int level)
{
    const int baseXP = 100;
    return (int)(baseXP * Math.Pow(level, 1.8));
}

// XP sources
- Enemy kill: BaseXP * EnemyLevel * DifficultyScaling
- Quest completion: Fixed reward defined in quest data
- Discovery: Location-specific bonuses (dungeon level * 10)
```

**Best Practices**:

- Max level cap at 50 (prevents unbounded progression)
- XP gain from repeated enemy kills reduced by 50% to discourage grinding
- Quest XP grants apply once (no repeated completions)
- Level-up notification via event system (OnPlayerLevelUp)

---

## 4. Spell & Ability System

### Decision: Mana Pool with Regeneration + Cooldown-Based Casting

**Chosen Approach**: Spells cost mana (finite resource), mana regenerates over time (slower in combat), powerful spells have cooldowns to prevent spam.

**Rationale**:

- Mana resource creates strategic decision-making (save mana for healing or use for damage?)
- Regeneration ensures players never get stuck without resources
- Cooldowns balance powerful spells without excessive mana costs
- Standard RPG pattern (D&D, WoW, Diablo)

**Alternatives Considered**:

- **Mana only (no cooldowns)**: All balancing via mana cost → Rejected, leads to mana-stacking meta
- **Cooldowns only (no mana)**: Like MOBA abilities → Rejected, doesn't fit RPG flavor
- **Spell slots** (Vancian magic): Limited casts per rest → Rejected as too restrictive for dungeon crawler

**Implementation**:

```csharp
public struct Mana
{
    public int Current;
    public int Maximum;
    public int RegenRate; // Per turn out of combat
    public int CombatRegenRate; // Per turn in combat (50% of RegenRate)
}

public struct Spell
{
    public Guid Id;
    public string Name;
    public SpellType Type; // Offensive, Defensive, Healing, Utility, AOE
    public int ManaCost;
    public int Cooldown; // Turns
    public TargetingType Targeting; // Single, AOE, Self
    public SpellEffect Effect; // Damage calculation, buff/debuff data, heal amount
}

public struct SpellBook
{
    public List<Guid> KnownSpells;
    public int MaxActiveSpells = 10; // Hotbar limit
}

public struct SpellCooldown
{
    public Dictionary<Guid, int> Cooldowns; // SpellId -> turns remaining
}
```

**Best Practices**:

- Spell learning via level-up, quest rewards, or spell tome items
- Spell effects reuse existing StatusEffect system (buff/debuff spells apply StatusEffect components)
- AOE spells use existing GoRogue radius calculations
- Mana regeneration paused for 3 turns after taking damage (prevents regen-tanking)

---

## 5. Merchant & Trading System

### Decision: Gold Currency with Dynamic Pricing + Merchant Stock

**Chosen Approach**: Players earn gold from loot/quests, merchants sell items at markup (1.5x base value), buy items at markdown (0.5x base value), stock refreshes on dungeon level change.

**Rationale**:

- Gold as universal currency simplifies trading (no bartering complexity)
- Buy/sell price differential creates economic sink (prevents infinite gold from buying/selling)
- Stock refresh adds variety (players can't buy everything from one merchant)
- Aligns with roguelike tradition (Nethack, DCSS)

**Alternatives Considered**:

- **Barter system**: Item-for-item trades → Rejected as too complex for scope
- **Multiple currencies**: Gold, gems, reputation → Rejected, adds UI complexity
- **Unlimited stock**: Merchants never run out → Rejected, reduces strategic decisions

**Implementation**:

```csharp
public struct Gold
{
    public int Amount;
}

public struct MerchantInventory
{
    public List<MerchantItem> Stock;
    public int RefreshInterval = 5; // Dungeon levels
    public int LastRefreshLevel;
}

public struct MerchantItem
{
    public Guid ItemId;
    public int Quantity;
    public int BasePrice;
    public float Markup = 1.5f; // Sell to player
    public float Markdown = 0.5f; // Buy from player
}

// Pricing
public int CalculateSellPrice(Item item, Merchant merchant)
{
    return (int)(item.BaseValue * merchant.Markup);
}

public int CalculateBuyPrice(Item item, Merchant merchant)
{
    return (int)(item.BaseValue * merchant.Markdown);
}
```

**Best Practices**:

- Gold persists across game sessions (saved with player data)
- Merchant stock defined in JSON files (allows easy content creation)
- Stock refresh uses weighted random selection (common items 70%, rare 25%, legendary 5%)
- Integration with existing Inventory plugin (enhance with Gold component)

---

## 6. Boss Encounter System

### Decision: Phase-Based AI with Unique Abilities

**Chosen Approach**: Bosses are Enemy entities with enhanced stats, multiple AI phases (triggered at health thresholds), and special abilities unavailable to regular enemies.

**Rationale**:

- Phases add mechanical depth (boss changes strategy at 75%, 50%, 25% health)
- Unique abilities create memorable encounters (teleport, summon minions, AOE attacks)
- Reuses existing AI and Combat systems (just extends, doesn't replace)
- Scalable complexity (simple bosses = 1 phase, complex bosses = 4 phases)

**Alternatives Considered**:

- **Script-based bosses**: Hardcoded behavior sequences → Rejected, inflexible and hard to test
- **Static bosses**: Just higher stats, no phases → Rejected, boring encounters
- **Puzzle bosses**: Require specific items/actions to defeat → Rejected, too adventure-game-like for dungeon crawler

**Implementation**:

```csharp
public struct Boss
{
    public Guid Id;
    public string Name;
    public List<BossPhase> Phases;
    public int CurrentPhase;
    public LootTable GuaranteedLoot;
}

public struct BossPhase
{
    public float HealthThreshold; // 0.75f, 0.5f, 0.25f
    public AIBehavior Behavior; // Chase, Ranged, Summon, Defensive
    public List<BossAbility> Abilities;
    public float AbilityChance = 0.3f; // 30% chance per turn
}

public struct BossAbility
{
    public string Name;
    public AbilityType Type; // Teleport, SummonMinions, AOEAttack, Heal, StatusCloud
    public int Cooldown;
    public AbilityEffect Effect;
}
```

**Best Practices**:

- Boss spawns on designated levels (every 5 levels: 5, 10, 15, 20)
- Boss room generation: Larger rooms (15x15 vs 8x8 normal), no other enemies
- Phase transitions trigger animation/notification (pause combat for 1 turn)
- Boss defeat sets persistent flag (no respawn in same session)

---

## 7. Environmental Hazards & Traps

### Decision: Tile-Based Hazards with Perception Detection

**Chosen Approach**: Traps are entities placed on tiles, detected by Perception checks, disarmed by skill checks, persistent state stored per level.

**Rationale**:

- Tile-based fits roguelike grid movement (no collision detection complexity)
- Perception-based detection rewards stat investment (meaningful character choice)
- Skill checks add risk/reward (attempt disarm or avoid?)
- Persistent state prevents respawn abuse (trap stays disarmed)

**Alternatives Considered**:

- **Always visible traps**: No detection → Rejected, reduces tactical depth
- **Random detection**: No player control → Rejected, feels unfair
- **One-time traps**: Disappear after trigger → Rejected for persistent world

**Implementation**:

```csharp
public struct Trap
{
    public Guid Id;
    public TrapType Type; // Spike, PoisonGas, Fire, Arrow
    public int DetectionDifficulty; // Compared to player Perception
    public int DisarmDifficulty; // Skill check DC
    public TrapState State; // Hidden, Detected, Triggered, Disarmed
    public TrapEffect Effect; // Damage, StatusEffect application
}

public struct EnvironmentalHazard
{
    public HazardType Type; // Lava, Pit, CollapsingFloor
    public int DamagePerTurn;
    public bool BlocksMovement; // Pits block, Lava doesn't
}

// Detection check
public bool DetectTrap(Entity player, Trap trap)
{
    var perception = player.Get<Stats>().Perception;
    var roll = Random.Next(1, 21); // d20
    return roll + perception >= trap.DetectionDifficulty;
}
```

**Best Practices**:

- Trap density: 1-2 traps per dungeon level (not every room)
- Trap placement: Doorways, corridors (natural choke points)
- Disarm failure triggers trap (risk/reward decision)
- Environmental hazards visible (no perception check for lava)

---

## Technology Stack Summary

### Core Dependencies

| Technology | Purpose | Version | Justification |
|------------|---------|---------|---------------|
| Arch ECS | Entity-Component-System | Latest | Existing framework, high performance |
| GoRogue | Roguelike algorithms (FOV, pathfinding) | Latest | Existing framework, field-tested |
| Newtonsoft.Json | Serialization (quests, dialogue, spells) | Latest | Existing, robust JSON support |
| xUnit | Unit testing | Latest | Existing test framework |

### Plugin Dependencies

All plugins reference:

- LablabBean.Plugins.Contracts (IPlugin, IPluginContext)
- LablabBean.Plugins.Core (PluginRegistry, PluginSandbox)
- Microsoft.Extensions.DependencyInjection (service registration)

### Performance Considerations

- **Quest System**: Event-driven, O(1) objective updates
- **Dialogue System**: Graph traversal, O(1) node lookup via dictionary
- **Progression**: O(1) XP calculations, triggered only on gain/level-up
- **Spells**: Mana regen batched per turn, cooldowns tracked in dictionary
- **Merchant**: Stock cached, O(1) price lookups
- **Boss**: Phase checks only on health change, O(1)
- **Traps**: Detection checks on movement only, O(1) per move

**Expected Impact**: <5ms additional per-turn processing across all systems combined.

---

## Integration Points

### Cross-System Dependencies

1. **Quest ↔ NPC**: Quests reference NPC IDs for quest-givers, NPCs trigger quest acceptance via dialogue actions
2. **Quest ↔ Progression**: Quest completion grants XP, progression system publishes OnLevelUp for level-gated quests
3. **Spell ↔ StatusEffects**: Buff/debuff spells apply StatusEffect components, reuse existing rendering/duration logic
4. **Spell ↔ Combat**: Offensive spells deal damage via CombatSystem, defensive spells modify Combat component stats
5. **Merchant ↔ Inventory**: Trading adds/removes items from player Inventory, requires Gold component
6. **Boss ↔ AI**: Boss phases modify AI.Behavior component, reuse existing AISystem logic
7. **Traps ↔ StatusEffects**: Poison gas traps apply Poison status, fire vents apply Burning

### Event System Usage

New events published:

- `OnQuestAccepted`, `OnQuestCompleted`, `OnQuestFailed`
- `OnNPCInteraction`, `OnDialogueChoice`
- `OnPlayerLevelUp`, `OnXPGained`
- `OnSpellCast`, `OnSpellLearned`
- `OnTrade`, `OnGoldChanged`
- `OnBossPhaseChange`, `OnBossDefeated`
- `OnTrapTriggered`, `OnTrapDisarmed`

---

## Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Plugin interdependencies create circular references | High | Medium | Define clear service boundaries, use event system for cross-plugin communication |
| Quest system performance degrades with 100+ active quests | Medium | Low | Benchmark quest update loop, optimize event filtering, lazy evaluation |
| Dialogue trees become unmanageable without tooling | Medium | High | Prioritize JSON schema validation, create simple text-based editor, defer visual editor to future |
| Balance issues with spell/progression scaling | High | High | Implement extensive playtesting, tunable config files (JSON), damage/XP multipliers |
| Save file compatibility breaks with new components | High | Medium | Version save files, migration scripts for data format changes |

---

## Next Steps (Phase 1)

1. **Data Model**: Define all component structures (Quest, NPC, Experience, Spell, etc.) in data-model.md
2. **Contracts**: Generate plugin service contracts (IQuestService, INPCService, etc.) in /contracts/
3. **Quickstart**: Create developer guide for adding new quests, NPCs, spells in quickstart.md
4. **Agent Context**: Update .agent/adapters/claude.md with new technology decisions

---

**Research Status**: ✅ Complete
**Next Phase**: Phase 1 - Design & Contracts
