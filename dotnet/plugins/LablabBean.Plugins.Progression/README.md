# Character Progression Plugin ⬆️

**Version**: 1.0.0 | **Plugin Type**: Gameplay System

---

## Overview

Complete character progression system with experience, leveling, stat growth, and ability unlocks.

## Features

- **Experience System**: Gain XP from combat, quests, exploration
- **Dynamic Leveling**: Balanced exponential curve (level 1-20)
- **Stat Growth**: Health, Attack, Defense scaling per level
- **Ability Unlocks**: New spell/ability slots at key levels
- **Prestige**: Optional post-max-level progression

## Quick Start

```csharp
services.AddProgressionSystem();

// Award experience
progressionService.GainExperience(playerEntity, 100);

// Check level up
if (player.Get<CharacterProgression>().ExperiencePoints >= nextLevelXP)
{
    progressionService.LevelUp(playerEntity);
}
```

## Components

### CharacterProgression

```csharp
int Level;                  // Current level (1-20)
int ExperiencePoints;       // Current XP
int ExperienceToNextLevel;  // XP needed for next level
int StatPoints;             // Unspent stat points
int AbilitySlots;           // Available ability slots
```

### ExperienceSource

```csharp
enum ExperienceSource
{
    Combat,       // Defeating enemies
    Quest,        // Completing quests
    Exploration,  // Discovering areas
    Crafting      // Creating items
}
```

## Leveling Curve

| Level | XP Required | Total XP | HP | Attack | Defense |
|-------|------------|----------|-----|--------|---------|
| 1     | 0          | 0        | 100 | 10     | 5       |
| 5     | 800        | 2,000    | 180 | 18     | 9       |
| 10    | 2,000      | 10,000   | 280 | 28     | 14      |
| 15    | 4,000      | 30,000   | 380 | 38     | 19      |
| 20    | 8,000      | 70,000   | 500 | 50     | 25      |

Formula: `XP = 100 * level^1.5`

## Services

### IProgressionService

```csharp
void GainExperience(Entity player, int amount, ExperienceSource source);
void LevelUp(Entity player);
void AllocateStatPoint(Entity player, StatType stat);
int CalculateExperienceReward(int enemyLevel, int playerLevel);
bool CanLevelUp(Entity player);
```

## Stat Growth

Per level:

- **Health**: +20 HP
- **Attack**: +2 Attack Power
- **Defense**: +1 Defense
- **Stat Points**: +3 (manual allocation)
- **Ability Slot**: +1 every 3 levels

## Ability Unlocks

- Level 1: 1 ability slot (starter)
- Level 3: 2 slots
- Level 6: 3 slots
- Level 9: 4 slots
- Level 12: 5 slots
- Level 15: 6 slots
- Level 18: 7 slots (master tier)

## Events

- `ExperienceGainedEvent(int amount, ExperienceSource source)`
- `LevelUpEvent(int newLevel, Stats bonuses)`
- `StatPointAllocatedEvent(StatType stat)`
- `AbilitySlotUnlockedEvent(int slotNumber)`

## Integration

### With Combat System

```csharp
// Award XP for defeating enemy
var xpReward = progressionService.CalculateExperienceReward(
    enemyLevel: enemy.Get<CharacterProgression>().Level,
    playerLevel: player.Get<CharacterProgression>().Level
);
progressionService.GainExperience(player, xpReward, ExperienceSource.Combat);
```

### With Quest System

```csharp
// Grant quest XP reward
progressionService.GainExperience(
    player,
    quest.Get<Quest>().Rewards.ExperiencePoints,
    ExperienceSource.Quest
);
```

### With Spell System

```csharp
// Check if player can learn spell
var progression = player.Get<CharacterProgression>();
if (progression.AbilitySlots > currentSpellCount)
{
    spellService.LearnSpell(player, spellId);
}
```

## Performance

- XP calculations: <0.1ms
- Level up processing: <2ms (includes stat recalculation)
- Supports 1000+ entities with progression

## Configuration

```json
{
  "maxLevel": 20,
  "baseXPMultiplier": 100,
  "xpCurveExponent": 1.5,
  "statPointsPerLevel": 3,
  "hpPerLevel": 20,
  "attackPerLevel": 2,
  "defensePerLevel": 1
}
```

---

**See**: [INTEGRATION_EXAMPLES.md](INTEGRATION_EXAMPLES.md)
