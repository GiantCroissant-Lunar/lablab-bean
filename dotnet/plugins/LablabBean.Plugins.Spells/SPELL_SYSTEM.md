# Spell System Documentation

## Overview

The Spell System provides a complete magic combat framework with mana management, spell casting, cooldowns, and diverse spell effects. Players can learn spells, equip them to a hotbar, and cast them using mana resources.

## Features

### 📖 Spell Learning & Management

- **SpellBook Component**: Tracks known and equipped spells
- **Spell Unlocks**: Spells unlock at specific character levels
- **Hotbar System**: Equip up to 8 spells for quick access
- **Persistent Learning**: Spells remain learned across sessions

### ⚡ Mana System

- **Resource Management**: Current/Maximum mana pool
- **Regeneration**: Auto-regeneration with combat/non-combat rates
- **Consumption**: Spells consume mana on cast
- **Restoration**: Mana potions and rest restore mana

### 🎯 Spell Casting

- **Targeting Types**:
  - **Single**: Target one entity
  - **AOE**: Area of Effect around a point
  - **Self**: Cast on yourself
  - **Directional**: Line-based targeting (planned)

- **Validation**:
  - Mana cost checking
  - Cooldown validation
  - Range verification
  - Spell knowledge check

### 🔥 Spell Effects

- **Damage**: Deal damage to targets
- **Heal**: Restore health
- **Shield**: Temporary defense boost
- **Buff**: Increase attack/stats
- **Debuff**: Decrease enemy stats
- **Status Effects**: Apply poison, stun, etc.

### ⏱️ Cooldown System

- **Turn-Based**: Cooldowns decrease per turn
- **Spell-Specific**: Each spell has individual cooldown
- **Balance**: Prevents spam-casting powerful spells

## Components

### Mana

```csharp
public struct Mana
{
    public int Current { get; set; }
    public int Maximum { get; set; }
    public int RegenRate { get; set; }
    public int CombatRegenRate { get; set; }
}
```

**Usage**:

```csharp
entity.Add(new Mana(100, regenRate: 5, combatRegenRate: 2));
```

### SpellBook

```csharp
public class SpellBook
{
    public HashSet<Guid> KnownSpells { get; set; }
    public List<Guid> EquippedSpells { get; set; }
    public int MaxEquippedSlots { get; set; } = 8;
}
```

**Usage**:

```csharp
var spellBook = new SpellBook();
spellBook.LearnSpell(spellId);
spellBook.EquipSpell(spellId);
```

### SpellCooldown

```csharp
public class SpellCooldown
{
    public Dictionary<Guid, int> ActiveCooldowns { get; set; }
}
```

## Systems

### ManaSystem

Manages mana regeneration and consumption.

**Methods**:

- `RegenerateMana(bool inCombat)`: Regenerates mana for all entities
- `RestoreMana(Entity entity, int amount)`: Manually restore mana
- `ConsumeMana(Entity entity, int amount)`: Deduct mana

### SpellCooldownSystem

Tracks and updates spell cooldowns.

**Methods**:

- `UpdateCooldowns()`: Decrements all active cooldowns
- `IsOnCooldown(Entity entity, Guid spellId)`: Check cooldown status
- `StartCooldown(Entity entity, Guid spellId, int turns)`: Begin cooldown

### SpellCastingSystem

Validates and executes spell casts.

**Methods**:

- `CastSpell(Entity caster, Guid spellId, Entity? target, Point? targetPosition)`: Cast a spell
- `CanCastSpell(Entity caster, Guid spellId)`: Check if spell can be cast

### SpellEffectSystem

Applies spell effects to targets.

**Methods**:

- `ApplyEffects(Entity caster, Entity target, List<SpellEffect> effects)`: Execute spell effects

## Service API

### SpellService

The main API for interacting with the spell system.

#### Spell Casting

```csharp
// Cast single-target spell
var result = spellService.CastSpell(
    casterId: playerEntity.Id,
    spellId: fireballId,
    targetId: enemyEntity.Id
);

if (result.Success)
{
    Console.WriteLine($"Dealt {result.DamageDealt} damage!");
}
else
{
    Console.WriteLine($"Cast failed: {result.FailureReason}");
}

// Cast AOE spell
var result = spellService.CastSpell(
    casterId: playerEntity.Id,
    spellId: lightningStrikeId,
    targetX: 10,
    targetY: 5
);

// Cast self-buff
var result = spellService.CastSpell(
    casterId: playerEntity.Id,
    spellId: battleFuryId
);

// Check if can cast
bool canCast = spellService.CanCastSpell(playerEntity.Id, spellId);
```

#### Mana Management

```csharp
// Get mana info
var mana = spellService.GetMana(playerEntity.Id);
Console.WriteLine($"Mana: {mana.Current}/{mana.Maximum}");

// Restore mana (e.g., from potion)
spellService.RestoreMana(playerEntity.Id, 50);

// Manual mana consumption
bool consumed = spellService.ConsumeMana(playerEntity.Id, 10);

// Regenerate mana (call per turn)
spellService.RegenerateMana(inCombat: true);
```

#### Spell Learning

```csharp
// Learn a spell
bool learned = spellService.LearnSpell(playerEntity.Id, fireballId);

// Check if known
bool knows = spellService.KnowsSpell(playerEntity.Id, fireballId);

// Equip to hotbar
bool equipped = spellService.EquipSpell(playerEntity.Id, fireballId);

// Unequip
spellService.UnequipSpell(playerEntity.Id, fireballId);

// Get all known spells
var knownSpells = spellService.GetKnownSpells(playerEntity.Id);
foreach (var spell in knownSpells)
{
    Console.WriteLine($"{spell.Name}: {spell.ManaCost} mana, {spell.Cooldown} turn cooldown");
}

// Get active (equipped) spells
var activeSpells = spellService.GetActiveSpells(playerEntity.Id);
```

#### Spell Queries

```csharp
// Get spell info
var spellInfo = spellService.GetSpellInfo(spellId);
Console.WriteLine($"{spellInfo.Name}");
Console.WriteLine($"  {spellInfo.Description}");
Console.WriteLine($"  Cost: {spellInfo.ManaCost} mana");
Console.WriteLine($"  Cooldown: {spellInfo.Cooldown} turns");
Console.WriteLine($"  Range: {spellInfo.Range}");
Console.WriteLine($"  Type: {spellInfo.Type}");

// Get remaining cooldown
int cooldown = spellService.GetSpellCooldown(playerEntity.Id, spellId);
if (cooldown > 0)
{
    Console.WriteLine($"Cooldown: {cooldown} turns remaining");
}
```

## Spell Definitions

Spells are defined in JSON files in `Data/Spells/`:

### Spell JSON Structure

```json
{
  "id": "f7a9d8c5-3b12-4e8f-9c7a-1d5e6f8a9b2c",
  "name": "Fireball",
  "description": "Hurls a ball of fire that explodes on impact",
  "type": "Offensive",
  "targeting": "Single",
  "manaCost": 15,
  "cooldown": 2,
  "range": 5,
  "areaRadius": 0,
  "minLevel": 3,
  "effects": [
    {
      "type": "Damage",
      "value": 25,
      "duration": 0
    }
  ]
}
```

### Field Descriptions

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | Unique identifier |
| `name` | string | Display name |
| `description` | string | Flavor text |
| `type` | SpellType | Offensive/Defensive/Healing/Utility/AOE |
| `targeting` | TargetingType | Single/AOE/Self/Directional |
| `manaCost` | int | Mana consumed per cast |
| `cooldown` | int | Turns before can cast again |
| `range` | int | Maximum distance (0 = infinite) |
| `areaRadius` | int | Radius for AOE spells |
| `minLevel` | int | Required player level |
| `effects` | SpellEffect[] | Array of effects to apply |

### Effect Types

| Type | Description | Value | Duration |
|------|-------------|-------|----------|
| `Damage` | Deal damage | Damage amount | N/A |
| `Heal` | Restore health | Healing amount | N/A |
| `Shield` | Increase defense | Defense bonus | Turns active |
| `Buff` | Increase attack | Attack bonus | Turns active |
| `Debuff` | Decrease attack | Attack reduction | Turns active |
| `StatusEffect` | Apply status | N/A | Turns active |

## Included Spells

### 1. Magic Missile (Level 1)

- **Cost**: 8 mana
- **Cooldown**: 0 turns
- **Effect**: 15 damage
- **Perfect starter spell - never misses!**

### 2. Healing Light (Level 2)

- **Cost**: 20 mana
- **Cooldown**: 3 turns
- **Effect**: Heal 40 HP
- **Self-cast healing spell**

### 3. Fireball (Level 3)

- **Cost**: 15 mana
- **Cooldown**: 2 turns
- **Effect**: 25 damage
- **Classic offensive spell**

### 4. Ice Shield (Level 4)

- **Cost**: 12 mana
- **Cooldown**: 5 turns
- **Effect**: +15 defense for 5 turns
- **Defensive barrier**

### 5. Lightning Strike (Level 5)

- **Cost**: 30 mana
- **Cooldown**: 4 turns
- **Effect**: 35 damage in 2-tile radius
- **Powerful AOE spell**

### 6. Battle Fury (Level 6)

- **Cost**: 18 mana
- **Cooldown**: 6 turns
- **Effect**: +10 attack for 4 turns
- **Combat buff**

## Spell Unlocks

Spells unlock automatically at specific levels. Configure in `Data/SpellUnlocks.json`:

```json
{
  "spellUnlocks": {
    "1": ["magic-missile-id"],
    "2": ["healing-light-id"],
    "3": ["fireball-id"],
    "4": ["ice-shield-id"],
    "5": ["lightning-strike-id"],
    "6": ["battle-fury-id"]
  }
}
```

## Integration Examples

### Quest Integration

Reward player with spell knowledge:

```csharp
public void OnQuestComplete(Guid questId, Entity player)
{
    var spellService = context.Registry.Get<SpellService>();

    if (questId == ancientTomeQuestId)
    {
        spellService.LearnSpell(player.Id, fireballId);
        Console.WriteLine("You learned Fireball!");
    }
}
```

### Progression Integration

Auto-learn spells on level up:

```csharp
public void OnPlayerLevelUp(Entity player, int newLevel)
{
    var spellService = context.Registry.Get<SpellService>();
    var spellDatabase = context.Registry.Get<SpellDatabase>();

    var unlockedSpells = spellDatabase.GetSpellsForLevel(newLevel);
    foreach (var spellId in unlockedSpells)
    {
        spellService.LearnSpell(player.Id, spellId);
        var spell = spellService.GetSpellInfo(spellId);
        Console.WriteLine($"Spell Unlocked: {spell.Name}!");
    }
}
```

### Combat Integration

Cast spells during battle:

```csharp
public void PlayerTurn(Entity player)
{
    var spellService = context.Registry.Get<SpellService>();

    // Show equipped spells
    var spells = spellService.GetActiveSpells(player.Id);
    for (int i = 0; i < spells.Count(); i++)
    {
        var spell = spells.ElementAt(i);
        var cooldown = spellService.GetSpellCooldown(player.Id, spell.Id);

        if (cooldown > 0)
        {
            Console.WriteLine($"{i+1}. {spell.Name} (Cooldown: {cooldown})");
        }
        else
        {
            Console.WriteLine($"{i+1}. {spell.Name} ({spell.ManaCost} mana)");
        }
    }

    // Cast selected spell
    var selectedSpell = GetPlayerChoice();
    var target = GetTarget();

    var result = spellService.CastSpell(
        player.Id,
        selectedSpell.Id,
        target.Id
    );

    if (result.Success)
    {
        Console.WriteLine($"Cast {selectedSpell.Name}! Dealt {result.DamageDealt} damage!");
    }
    else
    {
        Console.WriteLine($"Failed: {result.FailureReason}");
    }
}

public void EndTurn()
{
    var spellService = context.Registry.Get<SpellService>();

    // Regenerate mana
    bool inCombat = CheckIfInCombat();
    spellService.RegenerateMana(inCombat);

    // Update cooldowns
    var spellsPlugin = GetPlugin<SpellsPlugin>();
    spellsPlugin.UpdateCooldowns();
}
```

### Merchant Integration

Sell spell tomes:

```csharp
public void OnPurchaseSpellTome(Entity player, Guid spellTomeItemId)
{
    var spellService = context.Registry.Get<SpellService>();

    // Map item to spell
    var spellId = GetSpellIdForItem(spellTomeItemId);

    if (spellService.KnowsSpell(player.Id, spellId))
    {
        Console.WriteLine("You already know this spell!");
        return;
    }

    spellService.LearnSpell(player.Id, spellId);
    var spell = spellService.GetSpellInfo(spellId);
    Console.WriteLine($"Learned {spell.Name} from the tome!");
}
```

## System Updates

Call these methods each game turn:

```csharp
public void Update()
{
    var spellsPlugin = GetPlugin<SpellsPlugin>();

    // Update cooldowns (once per turn)
    spellsPlugin.UpdateCooldowns();

    // Regenerate mana (once per turn)
    bool inCombat = DetermineIfInCombat();
    spellsPlugin.RegenerateMana(inCombat);
}
```

## Creating Custom Spells

### 1. Create JSON Definition

Create `my_spell.json` in `Data/Spells/`:

```json
{
  "id": "12345678-1234-1234-1234-123456789abc",
  "name": "Frost Nova",
  "description": "Freezes all nearby enemies",
  "type": "AOE",
  "targeting": "Self",
  "manaCost": 25,
  "cooldown": 3,
  "range": 0,
  "areaRadius": 3,
  "minLevel": 4,
  "effects": [
    {
      "type": "Damage",
      "value": 20,
      "duration": 0
    },
    {
      "type": "Debuff",
      "value": 5,
      "duration": 2
    }
  ]
}
```

### 2. Add to Spell Unlocks

Edit `Data/SpellUnlocks.json`:

```json
{
  "spellUnlocks": {
    "4": ["...", "12345678-1234-1234-1234-123456789abc"]
  }
}
```

### 3. Reload and Test

Spells are loaded on plugin initialization. Restart to see new spells.

## Best Practices

### ⚡ Performance

- Keep spell databases in memory (loaded once)
- Use efficient entity queries
- Batch cooldown/regen updates

### 🎮 Game Balance

- Scale mana costs with spell power
- Use cooldowns for powerful spells
- Balance regen rates (combat vs rest)
- Limit hotbar slots to force choices

### 🐛 Debugging

- Check spell database loaded correctly
- Verify entity has Mana component
- Validate spell IDs match definitions
- Log cast failures for diagnostics

### 📊 Progression

- Unlock spells gradually (1-2 per level)
- Start with simple spells (Magic Missile)
- Introduce complexity over time
- Mix spell types for variety

## Troubleshooting

### "Spell not found"

- Check spell ID matches JSON definition
- Verify JSON loaded successfully
- Check logs for load errors

### "Caster has no spellbook"

- Add SpellBook component to entity
- Use `LearnSpell()` to initialize

### "Insufficient mana"

- Add Mana component to entity
- Ensure mana regeneration is running
- Check spell mana cost vs current mana

### "Target not found"

- Verify target entity exists
- Check entity ID is correct
- Use proper targeting type

## Architecture

```
SpellsPlugin
├── Components
│   ├── Mana.cs
│   ├── SpellBook.cs
│   └── SpellCooldown.cs
├── Data
│   ├── Spell.cs
│   ├── SpellEffect.cs
│   ├── SpellCastResult.cs
│   └── Spells/
│       ├── fireball.json
│       ├── healing_light.json
│       └── ...
├── Systems
│   ├── ManaSystem.cs
│   ├── SpellCooldownSystem.cs
│   ├── SpellCastingSystem.cs
│   └── SpellEffectSystem.cs
├── Services
│   ├── SpellService.cs
│   └── SpellDatabase.cs
└── SpellsPlugin.cs
```

## Version History

### v1.0.0 (Current)

- ✅ Core mana system
- ✅ Spell casting with validation
- ✅ Cooldown management
- ✅ 6 starter spells
- ✅ Single/AOE/Self targeting
- ✅ Level-based unlocks
- ✅ Spell learning & hotbar

### Planned Features

- 🔜 Directional targeting
- 🔜 Spell combos
- 🔜 Spell upgrades
- 🔜 Mana potions integration
- 🔜 Status effect integration
- 🔜 Spell visual effects
- 🔜 Spell animations

---

**Ready for Phase 7 or other systems!** 🚀
