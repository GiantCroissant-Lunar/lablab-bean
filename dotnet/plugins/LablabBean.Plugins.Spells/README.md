# Spells & Abilities Plugin ✨

**Version**: 1.0.0 | **Plugin Type**: Gameplay System

---

## Overview

Mana-based magic system with 15 spells across 3 schools: Fire, Ice, and Lightning.

## Features

- **3 Magic Schools**: Fire (DoT), Ice (Control), Lightning (Burst)
- **15 Unique Spells**: Balanced progression from starter to master
- **Mana System**: Resource management with regeneration
- **Cooldowns**: Strategic timing for powerful spells
- **Status Effect Integration**: Burning, Frozen, Shocked synergies

## Quick Start

```csharp
services.AddSpellSystem();

// Learn spell
spellService.LearnSpell(playerEntity, "fireball");

// Cast spell
bool success = spellService.CastSpell(playerEntity, targetEntity, "fireball");
```

## Components

### Spellbook

```csharp
List<string> KnownSpells;    // Learned spell IDs
Dictionary<string, float> Cooldowns;  // Active cooldowns
int MaxSpellSlots;           // Capacity limit
```

### ManaPool

```csharp
int CurrentMana;             // Available mana
int MaxMana;                 // Maximum capacity
float RegenerationRate;      // Mana per second
```

### Spell

```csharp
string Id, Name, Description;
SpellSchool School;          // Fire, Ice, Lightning
int ManaCost;                // Mana required
float Cooldown;              // Seconds between casts
int BaseDamage;              // Base damage value
string[] StatusEffects;      // Applied effects
```

## Spell Database

### Fire School (Damage Over Time)

| Spell | Level | Mana | Damage | Effect |
|-------|-------|------|--------|--------|
| Spark | 1 | 10 | 15 | - |
| Fireball | 3 | 25 | 35 | Burning (3 dmg/turn) |
| Flame Burst | 6 | 40 | 60 | Burning + AoE |
| Inferno | 9 | 60 | 90 | Burning + Ignite |
| Meteor Strike | 12 | 100 | 150 | Massive AoE + Burning |

### Ice School (Control & Defense)

| Spell | Level | Mana | Damage | Effect |
|-------|-------|------|--------|--------|
| Frost Bolt | 1 | 10 | 12 | Slow (50%) |
| Ice Shard | 3 | 25 | 30 | Frozen (1 turn) |
| Blizzard | 6 | 40 | 50 | AoE Frozen |
| Glacial Prison | 9 | 60 | 70 | Frozen (2 turns) |
| Absolute Zero | 12 | 100 | 120 | AoE Shatter |

### Lightning School (Burst Damage)

| Spell | Level | Mana | Damage | Effect |
|-------|-------|------|--------|--------|
| Shock | 1 | 10 | 20 | - |
| Lightning Bolt | 3 | 25 | 45 | Paralyzed |
| Chain Lightning | 6 | 40 | 40 | Hits 3 targets |
| Thunderstorm | 9 | 60 | 80 | AoE Shocked |
| Judgment | 12 | 100 | 180 | Single target nuke |

## Services

### ISpellService

```csharp
bool LearnSpell(Entity caster, string spellId);
bool CastSpell(Entity caster, Entity target, string spellId);
bool CanCastSpell(Entity caster, string spellId);
void UpdateCooldowns(float deltaTime);
void RegenerateMana(Entity caster, float deltaTime);
List<Spell> GetKnownSpells(Entity caster);
Spell? GetSpell(string spellId);
```

## Mana System

### Configuration

- **Starting Mana**: 100
- **Mana per Level**: +20
- **Regeneration**: 5 mana/second (base)
- **Combat Penalty**: -50% regen during combat

### Mana Costs by Tier

- Starter (Lvl 1): 10 mana
- Basic (Lvl 3): 25 mana
- Advanced (Lvl 6): 40 mana
- Expert (Lvl 9): 60 mana
- Master (Lvl 12): 100 mana

## Cooldowns

Strategic resource management:

- Low power: 2 seconds
- Medium power: 5 seconds
- High power: 10 seconds
- Ultimate: 20 seconds

## Integration

### With Status Effects

```csharp
// Apply burning from Fireball
var spell = spellService.GetSpell("fireball");
foreach (var effectId in spell.StatusEffects)
{
    statusEffectService.ApplyEffect(target, effectId, caster);
}
```

### With Progression

```csharp
// Unlock spells on level up
if (player.Get<CharacterProgression>().Level >= 3)
{
    spellService.LearnSpell(player, "fireball");
}
```

### With Combat

```csharp
// Player turn: cast spell
if (Input.Key == ConsoleKey.M)
{
    var spell = ShowSpellSelectionUI();
    var target = SelectTarget();
    spellService.CastSpell(player, target, spell.Id);
}
```

## Events

- `SpellLearnedEvent(string spellId)`
- `SpellCastEvent(Entity caster, Entity target, string spellId, bool success)`
- `ManaDepleted Event(Entity caster)`
- `CooldownExpiredEvent(string spellId)`

## AI Spell Selection

```csharp
// AI chooses optimal spell
var bestSpell = spellAI.SelectBestSpell(
    caster: enemyEntity,
    target: playerEntity,
    strategy: SpellStrategy.MaximizeDamage
);

if (spellService.CanCastSpell(enemyEntity, bestSpell.Id))
{
    spellService.CastSpell(enemyEntity, playerEntity, bestSpell.Id);
}
```

## Performance

- Spell casting: <1ms
- Cooldown updates: <0.1ms per entity
- Mana regeneration: <0.5ms per frame (100 entities)
- Database: 15 spells, <10ms load time

## Configuration

```json
{
  "startingMana": 100,
  "manaPerLevel": 20,
  "baseRegenRate": 5.0,
  "combatRegenPenalty": 0.5,
  "spellDatabasePath": "Data/Spells/spell_database.json"
}
```

## Spell Progression Path

### Recommended Learning Order

1. **Level 1**: Learn one starter spell (Spark, Frost Bolt, or Shock)
2. **Level 3**: Upgrade to basic spell (Fireball, Ice Shard, Lightning Bolt)
3. **Level 6**: Learn advanced AoE (Flame Burst, Blizzard, Chain Lightning)
4. **Level 9**: Expert single-target (Inferno, Glacial Prison, Thunderstorm)
5. **Level 12**: Master ultimate (Meteor Strike, Absolute Zero, Judgment)

### Hybrid Builds

- **Fire/Lightning**: Burst damage with DoT followup
- **Ice/Fire**: Control + Damage
- **Lightning/Ice**: Crowd control with burst finish

---

**See**: [INTEGRATION_EXAMPLES.md](INTEGRATION_EXAMPLES.md)
