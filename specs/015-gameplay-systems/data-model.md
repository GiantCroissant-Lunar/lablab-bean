# Data Model: Core Gameplay Systems

**Feature**: Core Gameplay Systems
**Date**: 2025-10-23
**Status**: Complete

## Overview

This document defines all ECS components and data structures for the seven gameplay systems. All components follow the existing Arch ECS pattern: structs for components, classes for services.

---

## 1. Quest System Components

### Quest

Represents a quest mission with objectives and rewards.

**Fields**:

- `Id` (Guid): Unique quest identifier
- `Name` (string): Display name
- `Description` (string): Quest text shown to player
- `QuestGiverNPCId` (Guid): Reference to NPC who gives quest
- `State` (QuestState): Current state
- `Objectives` (List<QuestObjective>): List of objectives
- `Rewards` (QuestRewards): Rewards granted on completion
- `Prerequisites` (QuestPrerequisites): Requirements to start quest

**Relationships**:

- References NPC entity (quest-giver)
- Referenced by QuestLog component (active quests list)

**Validation**:

- At least one objective required
- Name and Description non-empty
- Prerequisites validated before quest can start

---

### QuestObjective

Individual objective within a quest.

**Fields**:

- `Type` (QuestObjectiveType): Type of objective
- `Target` (string): Specific target (enemy type, item ID, location name, NPC ID)
- `Required` (int): Number required (e.g., kill 5 goblins)
- `Current` (int): Current progress
- `IsCompleted` (bool): Derived property (Current >= Required)

**Enum: QuestObjectiveType**:

- `KillEnemies`: Defeat specific enemy types
- `CollectItems`: Gather specific items
- `ReachLocation`: Travel to specific coordinates/level
- `TalkToNPC`: Interact with specific NPC
- `DeliverItem`: Give item to NPC
- `Survive`: Stay alive for N turns

---

### QuestLog

Tracks player's quests.

**Fields**:

- `ActiveQuests` (List<Guid>): Quest IDs currently in progress
- `CompletedQuests` (List<Guid>): Quest IDs finished
- `FailedQuests` (List<Guid>): Quest IDs failed
- `MaxActiveQuests` (int): Limit on simultaneous active quests (default: 20)

**Validation**:

- ActiveQuests count ≤ MaxActiveQuests
- Quest IDs unique across all lists

---

### QuestRewards

Rewards granted on quest completion.

**Fields**:

- `ExperiencePoints` (int): XP reward
- `Gold` (int): Gold reward
- `Items` (List<Guid>): Item IDs to grant
- `Reputation` (Dictionary<string, int>): Faction reputation changes (future expansion)

---

### QuestPrerequisites

Requirements to start a quest.

**Fields**:

- `MinimumLevel` (int): Required player level (0 = no requirement)
- `RequiredQuests` (List<Guid>): Previous quests that must be completed
- `RequiredItems` (List<Guid>): Items player must possess
- `ForbiddenQuests` (List<Guid>): Quests that cannot be active/completed (mutually exclusive)

---

### Enums

```csharp
public enum QuestState
{
    NotStarted,
    Active,
    Completed,
    Failed
}

public enum QuestObjectiveType
{
    KillEnemies,
    CollectItems,
    ReachLocation,
    TalkToNPC,
    DeliverItem,
    Survive
}
```

---

## 2. NPC & Dialogue System Components

### NPC

Non-player character entity.

**Fields**:

- `Id` (Guid): Unique NPC identifier
- `Name` (string): Display name
- `Type` (NPCType): NPC role
- `DialogueTreeId` (Guid): Reference to dialogue tree data
- `State` (Dictionary<string, object>): Persistent NPC memory (quest flags, conversation state)
- `MerchantInventoryId` (Guid?): Optional reference to merchant inventory (if Type == Merchant)
- `IsHostile` (bool): If true, attacks on sight (overrides Type)

**Relationships**:

- Referenced by Quest (QuestGiverNPCId)
- References DialogueTree (DialogueTreeId)
- References MerchantInventory (if merchant)

**Validation**:

- Name non-empty
- Type == Merchant requires MerchantInventoryId != null

---

### DialogueState

Tracks active dialogue session.

**Fields**:

- `ActiveNPCId` (Guid): NPC being talked to
- `CurrentNodeId` (Guid): Current dialogue node
- `History` (List<Guid>): Node IDs visited this conversation
- `IsActive` (bool): Dialogue session in progress

**State Transitions**:

- `StartDialogue(NPCId)` → IsActive = true, CurrentNodeId = RootNode
- `SelectChoice(ChoiceIndex)` → CurrentNodeId = NextNode
- `EndDialogue()` → IsActive = false, clear history

---

### DialogueTree (Data, not component)

Represents dialogue structure (loaded from JSON).

**Fields**:

- `Id` (Guid): Unique tree identifier
- `RootNodeId` (Guid): Starting node
- `Nodes` (Dictionary<Guid, DialogueNode>): All nodes

---

### DialogueNode (Data)

Single dialogue interaction.

**Fields**:

- `Id` (Guid): Unique node identifier
- `NPCText` (string): What NPC says
- `Choices` (List<DialogueChoice>): Player response options
- `IsTerminal` (bool): If true, ends dialogue when reached

**Validation**:

- NPCText non-empty
- Non-terminal nodes must have at least one choice

---

### DialogueChoice (Data)

Player dialogue option.

**Fields**:

- `PlayerText` (string): What player says
- `NextNodeId` (Guid): Node to transition to
- `Condition` (string): Expression to evaluate (e.g., "level >= 5")
- `Action` (DialogueAction): Event to trigger on selection

---

### DialogueAction (Data)

Action triggered by dialogue choice.

**Fields**:

- `Type` (DialogueActionType): Action type
- `Parameters` (Dictionary<string, object>): Action-specific data

**Enum: DialogueActionType**:

- `AcceptQuest`: Start quest (Parameters: QuestId)
- `CompleteQuest`: Turn in quest (Parameters: QuestId)
- `OpenTrade`: Start merchant interface
- `RevealLore`: Display lore text
- `SetNPCState`: Modify NPC.State (Parameters: Key, Value)
- `None`: No action

---

### Enums

```csharp
public enum NPCType
{
    QuestGiver,
    Merchant,
    Lore,
    Friendly,
    Neutral,
    Hostile
}
```

---

## 3. Character Progression Components

### Experience

Tracks player experience and level.

**Fields**:

- `CurrentXP` (int): Total experience points earned
- `Level` (int): Current level (starts at 1)
- `XPToNextLevel` (int): XP required to reach next level
- `TotalXPGained` (int): Lifetime XP for statistics

**Derived Values**:

- `XPToNextLevel = CalculateXPRequired(Level + 1) - CurrentXP`

**Validation**:

- Level >= 1
- CurrentXP >= 0

---

### Level (Optional, can be derived from Experience)

Stores level-specific data if needed.

**Fields**:

- `Current` (int): Current level
- `SkillPoints` (int): Unspent skill points (future: skill trees)
- `AbilitySlots` (int): Number of active abilities player can equip

---

### LevelUpEvent (Event, not component)

Published when player levels up.

**Fields**:

- `NewLevel` (int)
- `StatIncreases` (LevelUpStats)
- `UnlockedAbilities` (List<Guid>)

---

### LevelUpStats (Data)

Stat increases granted on level-up.

**Fields**:

- `HealthBonus` (int): Increase to max health
- `AttackBonus` (int): Increase to attack
- `DefenseBonus` (int): Increase to defense
- `ManaBonus` (int): Increase to max mana
- `SpeedBonus` (int): Increase to speed

---

## 4. Spell & Ability System Components

### Mana

Spell-casting resource.

**Fields**:

- `Current` (int): Current mana points
- `Maximum` (int): Max mana capacity
- `RegenRate` (int): Mana regenerated per turn (out of combat)
- `CombatRegenRate` (int): Mana regenerated per turn (in combat, typically 50% of RegenRate)

**Validation**:

- 0 ≤ Current ≤ Maximum
- RegenRate >= 0
- CombatRegenRate >= 0

---

### SpellBook

Collection of spells player knows.

**Fields**:

- `KnownSpells` (List<Guid>): Spell IDs player has learned
- `ActiveSpells` (List<Guid>): Spells equipped to hotbar
- `MaxActiveSpells` (int): Hotbar capacity (default: 10)

**Validation**:

- ActiveSpells.Count ≤ MaxActiveSpells
- All ActiveSpells must be in KnownSpells

---

### SpellCooldown

Tracks spell cooldowns.

**Fields**:

- `Cooldowns` (Dictionary<Guid, int>): SpellId → turns remaining
- `GlobalCooldown` (int): Turns before any spell can be cast (0 = ready)

**State Transitions**:

- `CastSpell(SpellId)` → Cooldowns[SpellId] = Spell.Cooldown, GlobalCooldown = 1
- `TickCooldowns()` → Decrement all cooldown values by 1

---

### Spell (Data, not component)

Spell definition (loaded from JSON).

**Fields**:

- `Id` (Guid): Unique spell identifier
- `Name` (string): Display name
- `Description` (string): Spell description
- `Type` (SpellType): Spell category
- `ManaCost` (int): Mana required to cast
- `Cooldown` (int): Turns before spell can be cast again
- `Targeting` (TargetingType): How spell targets
- `Range` (int): Max range in tiles
- `Effect` (SpellEffect): What spell does

---

### SpellEffect (Data)

Defines spell outcome.

**Fields**:

- `Type` (SpellEffectType): Effect category
- `Value` (int): Effect magnitude (damage, heal amount, duration)
- `StatusEffectId` (Guid?): StatusEffect to apply (if Type == ApplyStatus)
- `Radius` (int): AOE radius (if Targeting == AOE)

**Enum: SpellEffectType**:

- `Damage`: Deal damage to target
- `Heal`: Restore health to target
- `ApplyStatus`: Apply buff/debuff
- `Teleport`: Move target
- `Summon`: Create entity
- `AreaDamage`: Damage all in radius

---

### Enums

```csharp
public enum SpellType
{
    Offensive,
    Defensive,
    Healing,
    Utility,
    AOE
}

public enum TargetingType
{
    Single,      // Target one entity
    AOE,         // Target area
    Self,        // Caster only
    Directional  // Line from caster
}
```

---

## 5. Merchant & Trading System Components

### Gold

Currency component.

**Fields**:

- `Amount` (int): Current gold

**Validation**:

- Amount >= 0

---

### MerchantInventory

Merchant's stock.

**Fields**:

- `Id` (Guid): Unique inventory identifier
- `Stock` (List<MerchantItem>): Items for sale
- `BuybackItems` (List<Guid>): Items player recently sold (can buy back)
- `RefreshInterval` (int): Dungeon levels between stock refresh
- `LastRefreshLevel` (int): Dungeon level when last refreshed

**State Transitions**:

- `RefreshStock()` → Generate new Stock list, clear BuybackItems

---

### MerchantItem (Data)

Item in merchant inventory.

**Fields**:

- `ItemId` (Guid): Reference to Item
- `Quantity` (int): Stock count (-1 = infinite)
- `BasePrice` (int): Base item value
- `SellPriceMultiplier` (float): Markup when selling to player (default: 1.5)
- `BuyPriceMultiplier` (float): Markdown when buying from player (default: 0.5)

**Derived Values**:

- `SellPrice = BasePrice * SellPriceMultiplier`
- `BuyPrice = BasePrice * BuyPriceMultiplier`

---

### TradeState (Component)

Active trading session.

**Fields**:

- `MerchantId` (Guid): NPC being traded with
- `IsActive` (bool): Trade window open
- `PlayerOffering` (List<Guid>): Items player wants to sell
- `MerchantOffering` (List<Guid>): Items player wants to buy

---

## 6. Boss Encounter Components

### Boss

Marks entity as boss.

**Fields**:

- `Id` (Guid): Unique boss identifier
- `Name` (string): Boss display name
- `Phases` (List<BossPhase>): Phase definitions
- `CurrentPhase` (int): Index of active phase
- `GuaranteedLootTable` (List<Guid>): Item IDs that always drop
- `HasBeenDefeated` (bool): Persistent flag (prevents respawn)

**State Transitions**:

- `CheckPhaseTransition()` → If health < Phase.HealthThreshold, advance CurrentPhase

---

### BossPhase (Data)

Boss behavior at specific health threshold.

**Fields**:

- `HealthThreshold` (float): Health % to trigger (1.0 = 100%, 0.25 = 25%)
- `Behavior` (AIBehavior): AI behavior for this phase
- `Abilities` (List<BossAbility>): Special attacks
- `AbilityChance` (float): Probability per turn to use ability (0.0-1.0)

---

### BossAbility (Data)

Special boss attack.

**Fields**:

- `Id` (Guid): Unique ability identifier
- `Name` (string): Display name
- `Type` (BossAbilityType): Ability category
- `Cooldown` (int): Turns between uses
- `Effect` (AbilityEffect): What ability does

---

### AbilityEffect (Data)

Defines boss ability outcome.

**Fields**:

- `Type` (AbilityEffectType): Effect category
- `Value` (int): Effect magnitude
- `Radius` (int): AOE radius (if applicable)
- `StatusEffectId` (Guid?): StatusEffect to apply
- `SummonEnemyType` (string?): Enemy type to spawn (if Type == Summon)

**Enum: BossAbilityType**:

- `Teleport`: Move boss to random location
- `SummonMinions`: Spawn additional enemies
- `AOEAttack`: Damage all in radius
- `Heal`: Restore boss health
- `StatusCloud`: Create hazard zone

**Enum: AbilityEffectType**:

- `AreaDamage`
- `Summon`
- `Teleport`
- `HealSelf`
- `ApplyStatus`

---

## 7. Environmental Hazards & Traps Components

### Trap

Trap entity.

**Fields**:

- `Id` (Guid): Unique trap identifier
- `Type` (TrapType): Trap category
- `DetectionDifficulty` (int): Perception DC to detect
- `DisarmDifficulty` (int): Skill check DC to disarm
- `State` (TrapState): Current state
- `Effect` (TrapEffect): What happens when triggered

**State Transitions**:

- `Detect()` → State = Detected
- `Trigger()` → State = Triggered, apply Effect
- `Disarm()` → State = Disarmed (or Triggered on failure)

---

### TrapState (Enum)

```csharp
public enum TrapState
{
    Hidden,      // Not yet detected
    Detected,    // Player sees trap
    Triggered,   // Trap has activated
    Disarmed     // Trap neutralized
}
```

---

### TrapEffect (Data)

Defines trap outcome.

**Fields**:

- `Damage` (int): Physical damage dealt
- `StatusEffectId` (Guid?): StatusEffect to apply (e.g., Poison)
- `KnockbackDistance` (int): Tiles to push entity

---

### EnvironmentalHazard

Persistent map hazard.

**Fields**:

- `Type` (HazardType): Hazard category
- `DamagePerTurn` (int): Damage dealt each turn entity stands on tile
- `BlocksMovement` (bool): If true, impassable
- `StatusEffectId` (Guid?): StatusEffect applied while standing on tile

**Enum: HazardType**:

- `Lava`: High damage, blocks movement
- `Pit`: Instant death or heavy damage, blocks movement
- `CollapsingFloor`: Delays movement, eventual pit formation
- `PoisonGas`: Applies Poison status

---

### Enums

```csharp
public enum TrapType
{
    Spike,      // Physical damage
    PoisonGas,  // Poison status
    Fire,       // Fire damage + Burning status
    Arrow,      // Ranged physical damage
    Net,        // Slow status
    Alarm       // Alerts nearby enemies (future)
}
```

---

## Cross-System Component Relationships

### Component Dependency Graph

```
Player Entity:
├── Experience (Progression)
├── QuestLog (Quest)
├── Gold (Merchant)
├── Mana (Spell)
├── SpellBook (Spell)
├── SpellCooldown (Spell)
├── Inventory (existing, enhanced)
├── Health (existing)
├── Combat (existing)
└── Position (existing)

NPC Entity:
├── NPC (NPC/Dialogue)
├── MerchantInventory? (Merchant, optional)
├── DialogueState? (NPC/Dialogue, when talking)
├── Health (existing)
├── Position (existing)
└── Renderable (existing)

Boss Entity:
├── Boss (Boss)
├── Enemy (existing)
├── Health (existing)
├── Combat (existing)
├── AI (existing, enhanced)
└── Position (existing)

Trap Entity:
├── Trap (Hazards)
├── Position (existing)
└── Renderable (existing)

Hazard Tile:
├── EnvironmentalHazard (Hazards)
└── Position (existing)
```

---

## Validation Rules Summary

### Quest System

- Quest must have at least one objective
- Active quests ≤ MaxActiveQuests
- Prerequisites validated before quest start

### NPC & Dialogue

- Merchant NPCs must have MerchantInventory
- Non-terminal dialogue nodes must have choices
- Dialogue conditions must be valid expressions

### Progression

- Level >= 1
- XP >= 0

### Spells

- 0 ≤ Mana.Current ≤ Mana.Maximum
- ActiveSpells ⊂ KnownSpells

### Merchant

- Gold >= 0
- Stock quantity >= -1 (-1 = infinite)

### Boss

- Phases ordered by descending HealthThreshold

### Traps

- DetectionDifficulty, DisarmDifficulty > 0

---

## Persistence Requirements

Components that must be saved/loaded:

**Player**:

- Experience (level, XP)
- QuestLog (active/completed quests)
- Gold (currency)
- SpellBook (known/active spells)
- SpellCooldown (current cooldowns)

**World**:

- NPC.State (dialogue history, quest flags)
- Boss.HasBeenDefeated (prevent respawn)
- Trap.State (triggered/disarmed)
- MerchantInventory (stock, refresh tracking)

**JSON Structure**:

```json
{
  "player": {
    "experience": { "currentXP": 500, "level": 5 },
    "questLog": { "activeQuests": ["quest-1", "quest-2"] },
    "gold": { "amount": 1500 },
    "spellBook": { "knownSpells": ["fireball", "heal"], "activeSpells": ["fireball"] }
  },
  "world": {
    "npcs": [
      { "id": "npc-1", "state": { "questGiven": true, "dialogueFlag": "grateful" } }
    ],
    "bosses": [
      { "id": "boss-1", "hasBeenDefeated": true }
    ],
    "traps": [
      { "id": "trap-1", "state": "Disarmed" }
    ]
  }
}
```

---

**Data Model Status**: ✅ Complete
**Next Phase**: Contracts Generation
