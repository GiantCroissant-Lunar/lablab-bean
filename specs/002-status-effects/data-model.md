# Data Model: Status Effects System

**Feature**: Status Effects System
**Date**: 2025-10-21
**Phase**: 1 - Design & Contracts

## Overview

This document defines the complete data model for the status effects system using ECS components. All components follow the Arch ECS framework patterns established in `LablabBean.Game.Core`.

## Component Definitions

### Core Status Effect Components

#### StatusEffect Struct

```csharp
namespace LablabBean.Game.Core.Components;

/// <summary>
/// Represents a single active status effect on an entity.
/// Value type (struct) for performance.
/// </summary>
public struct StatusEffect
{
    /// <summary>Type of effect (Poison, Strength, Haste, etc.)</summary>
    public EffectType Type { get; set; }

    /// <summary>Effect magnitude (damage per turn, stat bonus, etc.)</summary>
    public int Magnitude { get; set; }

    /// <summary>Remaining duration in turns (1-99)</summary>
    public int Duration { get; set; }

    /// <summary>Category for grouping and processing</summary>
    public EffectCategory Category { get; set; }

    /// <summary>Source of effect (for tracking/debugging)</summary>
    public EffectSource Source { get; set; }

    /// <summary>Display color for HUD</summary>
    public EffectColor Color { get; set; }

    /// <summary>Display name for HUD</summary>
    public string DisplayName => Type.ToString();

    /// <summary>Whether this effect has expired</summary>
    public bool IsExpired => Duration <= 0;
}

public enum EffectType
{
    // Damage Over Time
    Poison,
    Bleed,
    Burning,

    // Healing Over Time
    Regeneration,
    Blessed,

    // Stat Buffs
    Strength,      // +Attack
    Haste,         // +Speed
    IronSkin,      // +Defense

    // Stat Debuffs
    Weakness,      // -Attack
    Slow,          // -Speed
    Fragile        // -Defense
}

public enum EffectCategory
{
    DamageOverTime,
    HealingOverTime,
    StatBuff,
    StatDebuff
}

public enum EffectSource
{
    Consumable,
    EnemyAttack,
    EnemySpell,
    Environmental,
    Other
}

public enum EffectColor
{
    Red,           // Debuffs, DoT
    Green,         // Buffs, HoT
    Blue,          // Defense buffs
    Cyan,          // Speed buffs
    Yellow,        // Healing
    Purple,        // Neutral
    Orange,        // Fire effects
    DarkRed        // Severe debuffs
}
```

#### StatusEffects Component

```csharp
namespace LablabBean.Game.Core.Components;

/// <summary>
/// Component attached to entities (player, enemies) to track active status effects.
/// </summary>
public struct StatusEffects
{
    /// <summary>List of currently active effects</summary>
    public List<StatusEffect> ActiveEffects { get; set; }

    /// <summary>Maximum number of concurrent effects (10 for MVP)</summary>
    public int MaxEffects { get; set; }

    /// <summary>Current number of active effects</summary>
    public int Count => ActiveEffects?.Count ?? 0;

    /// <summary>Whether max effects reached</summary>
    public bool IsFull => Count >= MaxEffects;

    /// <summary>Initialize with empty effects list</summary>
    public static StatusEffects CreateEmpty()
    {
        return new StatusEffects
        {
            ActiveEffects = new List<StatusEffect>(),
            MaxEffects = 10
        };
    }
}
```

### Effect Definition Data

#### EffectDefinition Struct

```csharp
namespace LablabBean.Game.Core.Components;

/// <summary>
/// Template for creating status effects.
/// Defines default values for each effect type.
/// </summary>
public struct EffectDefinition
{
    public EffectType Type { get; set; }
    public int DefaultMagnitude { get; set; }
    public int DefaultDuration { get; set; }
    public EffectCategory Category { get; set; }
    public EffectColor Color { get; set; }
    public string Description { get; set; }
}
```

## Effect Type Specifications

### Damage Over Time Effects

#### Poison

```csharp
public static readonly EffectDefinition Poison = new()
{
    Type = EffectType.Poison,
    DefaultMagnitude = 3,      // 3 HP damage per turn
    DefaultDuration = 5,        // 5 turns
    Category = EffectCategory.DamageOverTime,
    Color = EffectColor.Red,
    Description = "Deals 3 damage per turn for 5 turns"
};
```

#### Bleed

```csharp
public static readonly EffectDefinition Bleed = new()
{
    Type = EffectType.Bleed,
    DefaultMagnitude = 2,      // 2 HP damage per turn
    DefaultDuration = 8,        // 8 turns
    Category = EffectCategory.DamageOverTime,
    Color = EffectColor.DarkRed,
    Description = "Deals 2 damage per turn for 8 turns"
};
```

#### Burning

```csharp
public static readonly EffectDefinition Burning = new()
{
    Type = EffectType.Burning,
    DefaultMagnitude = 4,      // 4 HP damage per turn
    DefaultDuration = 3,        // 3 turns
    Category = EffectCategory.DamageOverTime,
    Color = EffectColor.Orange,
    Description = "Deals 4 damage per turn for 3 turns"
};
```

### Healing Over Time Effects

#### Regeneration

```csharp
public static readonly EffectDefinition Regeneration = new()
{
    Type = EffectType.Regeneration,
    DefaultMagnitude = 2,      // 2 HP healing per turn
    DefaultDuration = 10,       // 10 turns
    Category = EffectCategory.HealingOverTime,
    Color = EffectColor.Green,
    Description = "Heals 2 HP per turn for 10 turns"
};
```

#### Blessed

```csharp
public static readonly EffectDefinition Blessed = new()
{
    Type = EffectType.Blessed,
    DefaultMagnitude = 1,      // 1 HP healing per turn
    DefaultDuration = 20,       // 20 turns
    Category = EffectCategory.HealingOverTime,
    Color = EffectColor.Yellow,
    Description = "Heals 1 HP per turn for 20 turns"
};
```

### Stat Buff Effects

#### Strength

```csharp
public static readonly EffectDefinition Strength = new()
{
    Type = EffectType.Strength,
    DefaultMagnitude = 5,      // +5 attack
    DefaultDuration = 10,       // 10 turns
    Category = EffectCategory.StatBuff,
    Color = EffectColor.Green,
    Description = "Increases attack by 5 for 10 turns"
};
```

#### Haste

```csharp
public static readonly EffectDefinition Haste = new()
{
    Type = EffectType.Haste,
    DefaultMagnitude = 20,     // +20 speed
    DefaultDuration = 8,        // 8 turns
    Category = EffectCategory.StatBuff,
    Color = EffectColor.Cyan,
    Description = "Increases speed by 20 for 8 turns"
};
```

#### Iron Skin

```csharp
public static readonly EffectDefinition IronSkin = new()
{
    Type = EffectType.IronSkin,
    DefaultMagnitude = 5,      // +5 defense
    DefaultDuration = 12,       // 12 turns
    Category = EffectCategory.StatBuff,
    Color = EffectColor.Blue,
    Description = "Increases defense by 5 for 12 turns"
};
```

### Stat Debuff Effects

#### Weakness

```csharp
public static readonly EffectDefinition Weakness = new()
{
    Type = EffectType.Weakness,
    DefaultMagnitude = 3,      // -3 attack
    DefaultDuration = 6,        // 6 turns
    Category = EffectCategory.StatDebuff,
    Color = EffectColor.Red,
    Description = "Decreases attack by 3 for 6 turns"
};
```

#### Slow

```csharp
public static readonly EffectDefinition Slow = new()
{
    Type = EffectType.Slow,
    DefaultMagnitude = 30,     // -30 speed
    DefaultDuration = 6,        // 6 turns
    Category = EffectCategory.StatDebuff,
    Color = EffectColor.Red,
    Description = "Decreases speed by 30 for 6 turns"
};
```

#### Fragile

```csharp
public static readonly EffectDefinition Fragile = new()
{
    Type = EffectType.Fragile,
    DefaultMagnitude = 3,      // -3 defense
    DefaultDuration = 6,        // 6 turns
    Category = EffectCategory.StatDebuff,
    Color = EffectColor.DarkRed,
    Description = "Decreases defense by 3 for 6 turns"
};
```

## State Transitions

### Effect Lifecycle

```
┌─────────────┐
│   Applied   │ (Effect added to StatusEffects.ActiveEffects)
└──────┬──────┘
       │ Each turn
       ▼
┌─────────────┐
│   Active    │ (Tick damage/healing, decrement duration)
└──────┬──────┘
       │ Duration reaches 0 OR removed by antidote
       ▼
┌─────────────┐
│   Expired   │ (Removed from ActiveEffects list)
└─────────────┘
```

### Turn Processing Order

```
Entity Turn Starts
    ↓
1. Process DoT Effects (Poison, Bleed, Burning)
    - Apply damage to Health component
    - Cap health at 0 (can die from DoT)
    ↓
2. Process HoT Effects (Regeneration, Blessed)
    - Apply healing to Health component
    - Cap health at maximum
    ↓
3. Decrement All Effect Durations
    - Duration -= 1 for each effect
    ↓
4. Remove Expired Effects
    - Remove effects where Duration <= 0
    - Show "Effect expired" message
    ↓
5. Entity Takes Turn Actions
    - Combat, movement, etc.
    - Stat modifiers from effects applied during actions
```

## Validation Rules

### Effect Application Constraints

- ✅ Duration must be 1-99 turns (enforced at application)
- ✅ Magnitude must be non-zero (enforced at application)
- ✅ Max 10 concurrent effects per entity (enforced at application)
- ✅ Same effect type refreshes duration, doesn't stack magnitude

### Stat Modification Constraints

- ✅ Attack cannot go below 1 (minimum enforced)
- ✅ Defense cannot go below 1 (minimum enforced)
- ✅ Speed cannot go below 1 (minimum enforced)
- ✅ Multiple effects on same stat stack additively

### Effect Removal Constraints

- ✅ All effects cleared on entity death
- ✅ Antidotes remove specific effect types only
- ✅ Effect removal is immediate (no delay)

## Query Patterns

### Common ECS Queries

```csharp
// Get all entities with active status effects
var entitiesWithEffects = world.Query(
    in new QueryDescription()
        .WithAll<StatusEffects>()
).Where(e => world.Get<StatusEffects>(e).Count > 0);

// Get player's active effects
var playerEffects = world.Get<StatusEffects>(playerEntity);
var activeEffects = playerEffects.ActiveEffects;

// Find entities affected by poison
var poisonedEntities = world.Query(
    in new QueryDescription()
        .WithAll<StatusEffects>()
).Where(e =>
{
    var effects = world.Get<StatusEffects>(e);
    return effects.ActiveEffects.Any(eff => eff.Type == EffectType.Poison);
});

// Get all DoT effects on an entity
var dotEffects = statusEffects.ActiveEffects
    .Where(e => e.Category == EffectCategory.DamageOverTime);

// Get all buffs on an entity
var buffs = statusEffects.ActiveEffects
    .Where(e => e.Category == EffectCategory.StatBuff);
```

## Stat Calculation with Effects

### Combat Stats with Status Effects

```csharp
public static (int attack, int defense, int speed) CalculateStatsWithEffects(
    World world,
    Entity entity)
{
    // Get base stats
    var combat = world.Get<Combat>(entity);
    int baseAttack = combat.Attack;
    int baseDefense = combat.Defense;
    int baseSpeed = 100; // Default speed

    // Get equipment bonuses (from inventory system)
    if (world.Has<EquipmentSlots>(entity))
    {
        var equipment = world.Get<EquipmentSlots>(entity);
        foreach (var (slot, itemRef) in equipment.Slots)
        {
            if (itemRef.HasValue && world.Has<Equippable>(itemRef.Value))
            {
                var equippable = world.Get<Equippable>(itemRef.Value);
                baseAttack += equippable.AttackBonus;
                baseDefense += equippable.DefenseBonus;
                baseSpeed += equippable.SpeedModifier;
            }
        }
    }

    // Apply status effect modifiers
    if (world.Has<StatusEffects>(entity))
    {
        var statusEffects = world.Get<StatusEffects>(entity);

        foreach (var effect in statusEffects.ActiveEffects)
        {
            switch (effect.Type)
            {
                case EffectType.Strength:
                    baseAttack += effect.Magnitude;
                    break;
                case EffectType.Weakness:
                    baseAttack -= effect.Magnitude;
                    break;
                case EffectType.IronSkin:
                    baseDefense += effect.Magnitude;
                    break;
                case EffectType.Fragile:
                    baseDefense -= effect.Magnitude;
                    break;
                case EffectType.Haste:
                    baseSpeed += effect.Magnitude;
                    break;
                case EffectType.Slow:
                    baseSpeed -= effect.Magnitude;
                    break;
            }
        }
    }

    // Enforce minimums
    int finalAttack = Math.Max(1, baseAttack);
    int finalDefense = Math.Max(1, baseDefense);
    int finalSpeed = Math.Max(1, baseSpeed);

    return (finalAttack, finalDefense, finalSpeed);
}
```

## Integration with Consumable Items

### Extended Consumable Component

```csharp
namespace LablabBean.Game.Core.Components;

/// <summary>
/// Extended consumable component with status effect application.
/// Builds on spec-001 Consumable component.
/// </summary>
public struct Consumable
{
    // Existing from spec-001
    public ConsumableEffect Effect { get; set; }
    public int EffectValue { get; set; }
    public bool UsableOutOfCombat { get; set; }

    // NEW: Status effect application
    public EffectType? AppliesEffect { get; set; }
    public int? EffectMagnitude { get; set; }
    public int? EffectDuration { get; set; }

    // NEW: Status effect removal (antidotes)
    public EffectType? RemovesEffect { get; set; }
    public bool RemovesAllNegativeEffects { get; set; }
}
```

### New Consumable Item Definitions

```csharp
// Strength Potion
public static readonly ItemDefinition StrengthPotion = new()
{
    Name = "Strength Potion",
    Glyph = '!',
    Description = "Increases attack by 5 for 10 turns",
    Type = ItemType.Consumable,
    ConsumableEffect = null, // No immediate effect
    AppliesEffect = EffectType.Strength,
    EffectMagnitude = 5,
    EffectDuration = 10,
    IsStackable = true,
    MaxStackSize = 99
};

// Haste Potion
public static readonly ItemDefinition HastePotion = new()
{
    Name = "Haste Potion",
    Glyph = '!',
    Description = "Increases speed by 20 for 8 turns",
    Type = ItemType.Consumable,
    AppliesEffect = EffectType.Haste,
    EffectMagnitude = 20,
    EffectDuration = 8,
    IsStackable = true,
    MaxStackSize = 99
};

// Regeneration Potion
public static readonly ItemDefinition RegenerationPotion = new()
{
    Name = "Regeneration Potion",
    Glyph = '!',
    Description = "Heals 2 HP per turn for 10 turns",
    Type = ItemType.Consumable,
    AppliesEffect = EffectType.Regeneration,
    EffectMagnitude = 2,
    EffectDuration = 10,
    IsStackable = true,
    MaxStackSize = 99
};

// Antidote
public static readonly ItemDefinition Antidote = new()
{
    Name = "Antidote",
    Glyph = '!',
    Description = "Cures poison",
    Type = ItemType.Consumable,
    RemovesEffect = EffectType.Poison,
    IsStackable = true,
    MaxStackSize = 99
};

// Universal Cure
public static readonly ItemDefinition UniversalCure = new()
{
    Name = "Universal Cure",
    Glyph = '!',
    Description = "Removes all negative effects",
    Type = ItemType.Consumable,
    RemovesAllNegativeEffects = true,
    IsStackable = true,
    MaxStackSize = 99
};
```

## Integration with Enemy Attacks

### Extended Enemy Component

```csharp
namespace LablabBean.Game.Core.Components;

/// <summary>
/// Extended enemy component with status effect application on attack.
/// </summary>
public struct Enemy
{
    // Existing properties
    public string EnemyType { get; set; }
    public AIBehavior Behavior { get; set; }

    // NEW: Status effect application
    public EffectType? InflictsEffect { get; set; }
    public int EffectProbability { get; set; } // 0-100%
    public int? EffectMagnitude { get; set; }
    public int? EffectDuration { get; set; }
}
```

### Enemy Definitions with Effects

```csharp
// Toxic Spider (inflicts poison)
var toxicSpider = world.Create(
    new Enemy
    {
        EnemyType = "Toxic Spider",
        Behavior = AIBehavior.Chase,
        InflictsEffect = EffectType.Poison,
        EffectProbability = 40, // 40% chance on hit
        EffectMagnitude = 3,
        EffectDuration = 5
    },
    new Combat { Attack = 5, Defense = 2 },
    new Health { Current = 20, Maximum = 20 },
    // ... other components
);

// Shaman (casts buff on allies)
var shaman = world.Create(
    new Enemy
    {
        EnemyType = "Shaman",
        Behavior = AIBehavior.Support,
        InflictsEffect = EffectType.Strength, // Buffs allies
        EffectProbability = 100, // Always succeeds
        EffectMagnitude = 8,
        EffectDuration = 6
    },
    // ... other components
);
```

## Performance Characteristics

### Memory Usage

- **Per StatusEffect**: ~32 bytes (struct with enums and ints)
- **Per StatusEffects Component**: ~100 bytes + (32 × effect count)
- **30 Entities × 5 Effects**: ~5 KB total
- **Negligible**: Effect processing overhead

### Query Performance

- **Entities with effects**: O(n) where n = total entities (typically <100)
- **Effect processing per entity**: O(m) where m = effects per entity (max 10)
- **Total per turn**: O(n × m) = O(100 × 10) = 1000 operations
- **Execution time**: <1ms per turn

**Conclusion**: Performance is excellent. No optimization needed for MVP.

## Integration Points

### With Existing Systems

#### ActorSystem

- **Integration**: Call StatusEffectSystem.ProcessEffects() at turn start
- **Modification**: Add effect processing before energy accumulation

#### CombatSystem

- **Integration**: Call StatusEffectSystem.CalculateStatModifiers() before damage calculation
- **Modification**: Use modified stats instead of base stats

#### InventorySystem

- **Integration**: Call StatusEffectSystem.ApplyEffect() when using buff potions
- **Integration**: Call StatusEffectSystem.RemoveEffect() when using antidotes
- **Modification**: Check Consumable.AppliesEffect and Consumable.RemovesEffect

#### HudService

- **Integration**: Display StatusEffects.ActiveEffects in new panel
- **Modification**: Add status effects panel below health bar

### With New Systems

#### StatusEffectSystem

- **Creates**: New system to handle all status effect operations
- **Methods**: ApplyEffect, RemoveEffect, ProcessEffects, CalculateStatModifiers

---

**Data Model Complete**: All components, effects, and relationships defined.
**Next**: Generate system contracts and API definitions.
