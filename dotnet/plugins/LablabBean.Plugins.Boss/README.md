# Boss Encounters Plugin 👹

**Version**: 1.0.0 | **Plugin Type**: Gameplay System

---

## Overview

Epic boss encounter system with unique mechanics, phases, and legendary rewards.

## Features

- **5 Unique Bosses**: Goblin King to Ancient Dragon
- **Phase System**: Bosses change tactics at HP thresholds
- **Special Abilities**: Enrage, summons, AoE attacks
- **Legendary Loot**: Boss-specific unique items
- **Difficulty Scaling**: Adapts to player level

## Quick Start

```csharp
services.AddBossSystem();

// Create boss encounter
var boss = bossFactory.CreateBoss("dragon", playerLevel: 15);

// Handle boss mechanics
bossSystem.Update(deltaTime);
```

## Components

### Boss

```csharp
string Id, Name;
BossType Type;              // Mini, Standard, Epic, Raid
int CurrentPhase;           // Phase progression
List<BossAbility> Abilities;
Dictionary<int, BossPhase> Phases;  // HP% -> Phase
bool IsEnraged;
```

### BossAbility

```csharp
string Id, Name, Description;
AbilityType Type;           // Summon, AoE, Buff, Heal
int Cooldown;               // Seconds
int Damage;
string[] StatusEffects;
```

### BossPhase

```csharp
int PhaseNumber;
int HealthThreshold;        // HP% to trigger
string[] EnabledAbilities;
float DamageModifier;
string PhaseTransitionText;
```

## Boss Database

### 1. Goblin King (Level 5, Mini-Boss)

- **HP**: 500
- **Phases**: 2 (100%, 50%)
- **Abilities**:
  - Goblin Rally: Summons 3 goblin warriors
  - War Cry: +20% attack to all goblins
  - Cleave: 30 damage AoE
- **Loot**: Crown of the Goblin King (Rare helm)

### 2. Corrupted Treant (Level 8, Standard)

- **HP**: 800
- **Phases**: 3 (100%, 66%, 33%)
- **Abilities**:
  - Root Tangle: Immobilize player for 2 turns
  - Poison Spores: 10 damage/turn AoE
  - Nature's Wrath: 50 damage + knockback
  - Regeneration: Heal 50 HP (Phase 3)
- **Loot**: Bark Shield (Epic, +15 defense)

### 3. Flame Warden (Level 12, Standard)

- **HP**: 1200
- **Phases**: 3 (100%, 60%, 30%)
- **Abilities**:
  - Fireball Barrage: 40 damage x3
  - Flame Wall: Creates hazards
  - Immolation Aura: 15 damage/turn to nearby
  - Phoenix Form: Full heal once at 10% HP
- **Loot**: Warden's Flame Staff (Epic, +30 fire damage)

### 4. Shadow Assassin (Level 15, Standard)

- **HP**: 1000 (low HP, high dodge)
- **Phases**: 2 (100%, 40%)
- **Abilities**:
  - Shadow Step: Teleport + 60 damage backstab
  - Smoke Bomb: Invisibility for 2 turns
  - Poison Blade: 30 damage + 5 damage/turn
  - Clone: Creates 2 copies (Phase 2)
- **Loot**: Assassin's Cloak (Legendary, +50% crit)

### 5. Ancient Dragon (Level 20, Epic)

- **HP**: 3000
- **Phases**: 4 (100%, 75%, 50%, 25%)
- **Abilities**:
  - Dragon's Breath: 100 damage cone
  - Wing Buffet: Knockback + 40 damage AoE
  - Tail Swipe: 70 damage cleave
  - Aerial Assault: Flies up, immune for 1 turn
  - Summon Drakes: 2 mini-dragons (Phase 3)
  - Enrage: +50% damage, +30% speed (Phase 4)
- **Loot**: Dragonscale Armor (Legendary set)

## Systems

### BossSystem

- Phase transition detection
- Ability cooldown management
- Enrage mechanics
- Boss AI behavior

### BossAbilitySystem

- Execute special abilities
- Handle summons
- Apply status effects
- Calculate complex damage

## Services

### IBossService

```csharp
Entity CreateBoss(string bossId, int playerLevel);
void TriggerPhaseTransition(Entity boss, int newPhase);
bool CanUseAbility(Entity boss, string abilityId);
void UseAbility(Entity boss, Entity target, string abilityId);
void CheckEnrage(Entity boss);
List<Entity> GetActiveBosses(Entity player);
BossLootTable GetBossLoot(string bossId);
```

## Phase Transitions

### Automatic Triggers

```csharp
// Boss loses HP
if (currentHP <= boss.Phases[2].HealthThreshold)
{
    bossService.TriggerPhaseTransition(boss, 2);
    // New abilities unlocked, damage increased
}
```

### Phase Benefits

- Unlock new abilities
- Damage/defense modifiers
- Heal or buff effects
- Visual/audio cues

## Enrage Mechanic

**Triggers**:

- Boss below 20% HP for >60 seconds
- All minions defeated
- Player uses specific items

**Effects**:

- +50% damage
- +30% attack speed
- -20% cooldowns
- Cannot be CCed

## Integration

### With Combat

```csharp
// Boss turn in combat
var ability = bossAI.SelectAbility(boss, player);
bossService.UseAbility(boss, player, ability.Id);
```

### With Dungeon Progression

```csharp
// Spawn boss on floor
if (currentFloor % 5 == 0)  // Every 5 floors
{
    var boss = bossFactory.CreateBoss("goblin_king", playerLevel);
    worldService.SpawnEntity(boss, bossRoom);
}
```

### With Loot System

```csharp
// Grant boss loot on defeat
if (boss.Get<Health>().CurrentHP <= 0)
{
    var loot = bossService.GetBossLoot(boss.Get<Boss>().Id);
    lootService.DropLoot(boss, loot);
}
```

## Events

- `BossSpawnedEvent(Entity boss)`
- `BossPhaseTransitionEvent(Entity boss, int oldPhase, int newPhase)`
- `BossAbilityUsedEvent(Entity boss, string abilityId)`
- `BossEnragedEvent(Entity boss)`
- `BossDefeatedEvent(Entity boss, List<Entity> rewards)`

## Boss Difficulty Tiers

| Tier | HP Mult | Damage Mult | Phase Count | Abilities |
|------|---------|-------------|-------------|-----------|
| Mini | 1.0x | 1.0x | 1-2 | 2-3 |
| Standard | 1.5x | 1.2x | 2-3 | 3-5 |
| Epic | 3.0x | 1.5x | 3-4 | 5-7 |
| Raid | 5.0x | 2.0x | 4-5 | 8-10 |

## Performance

- Boss AI: <5ms per boss per turn
- Ability execution: <2ms
- Phase transitions: <3ms
- Supports 10 concurrent bosses

## Configuration

```json
{
  "enablePhaseTransitions": true,
  "enrageThresholdHP": 0.2,
  "enrageTimerSeconds": 60,
  "bossScalingMultiplier": 1.0,
  "guaranteedLegendaryDrop": true,
  "bossDatabasePath": "Data/Bosses/boss_database.json"
}
```

## Strategy Tips

### Goblin King

- Focus on killing summons quickly
- Save burst damage for Phase 2
- Interrupt War Cry if possible

### Ancient Dragon

- Bring fire resistance potions
- Use cover during Aerial Assault
- Kill drakes before they heal dragon
- Save defensive cooldowns for Enrage phase

---

**See**: [INTEGRATION_EXAMPLES.md](INTEGRATION_EXAMPLES.md)
