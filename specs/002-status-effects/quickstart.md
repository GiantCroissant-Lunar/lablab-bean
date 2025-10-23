# Quickstart Guide: Status Effects System

**Feature**: Status Effects System
**Date**: 2025-10-21
**Audience**: Developers implementing or extending the status effects system

## Overview

This guide provides practical examples for working with the status effects system. All code examples assume you have access to the `World` instance and relevant entity references.

## Table of Contents

1. [Applying Status Effects](#applying-status-effects)
2. [Removing Status Effects](#removing-status-effects)
3. [Processing Effects (Turn-Based)](#processing-effects-turn-based)
4. [Calculating Stats with Effects](#calculating-stats-with-effects)
5. [Integrating with Consumables](#integrating-with-consumables)
6. [Integrating with Enemy Attacks](#integrating-with-enemy-attacks)
7. [Displaying Effects in HUD](#displaying-effects-in-hud)
8. [Testing Examples](#testing-examples)

---

## Applying Status Effects

### Apply Poison to Player

```csharp
using LablabBean.Game.Core.Systems;
using LablabBean.Game.Core.Components;

var statusEffectSystem = new StatusEffectSystem(world);

// Apply poison using predefined definition
var result = statusEffectSystem.ApplyEffect(
    playerEntity,
    EffectDefinitions.Poison,
    EffectSource.EnemyAttack
);

if (result.Success)
{
    Console.WriteLine(result.Message); // "You are poisoned!"
}
```

### Apply Custom Effect

```csharp
// Apply custom strength buff (different magnitude/duration)
var result = statusEffectSystem.ApplyEffect(
    playerEntity,
    effectType: EffectType.Strength,
    magnitude: 8,        // +8 attack instead of default +5
    duration: 15,        // 15 turns instead of default 10
    source: EffectSource.Consumable
);

Console.WriteLine(result.Message); // "Strength increased!"
```

### Apply Multiple Effects

```csharp
// Apply poison, then regeneration
statusEffectSystem.ApplyEffect(playerEntity, EffectDefinitions.Poison, EffectSource.EnemyAttack);
statusEffectSystem.ApplyEffect(playerEntity, EffectDefinitions.Regeneration, EffectSource.Consumable);

// Player now has both effects active
// Net effect: -3 HP + 2 HP = -1 HP per turn
```

### Check if Effect Can Be Applied

```csharp
if (statusEffectSystem.CanApplyEffect(playerEntity, EffectType.Haste))
{
    // Apply haste
    statusEffectSystem.ApplyEffect(playerEntity, EffectDefinitions.Haste, EffectSource.Consumable);
}
else
{
    Console.WriteLine("Cannot apply effect (max effects reached)");
}
```

---

## Removing Status Effects

### Remove Specific Effect (Antidote)

```csharp
// Player uses antidote to cure poison
var result = statusEffectSystem.RemoveEffect(playerEntity, EffectType.Poison);

if (result.Success)
{
    Console.WriteLine(result.Message); // "Poison cured!"
}
else
{
    Console.WriteLine(result.Message); // "Not poisoned"
}
```

### Remove All Negative Effects (Universal Cure)

```csharp
// Player uses universal cure potion
var result = statusEffectSystem.RemoveAllNegativeEffects(playerEntity);

Console.WriteLine(result.Message);
// "All negative effects removed!" or "No negative effects active"
```

### Clear All Effects (On Death)

```csharp
// Called by CombatSystem when entity dies
statusEffectSystem.ClearAllEffects(deadEntity);

// All effects immediately removed, no feedback messages
```

---

## Processing Effects (Turn-Based)

### Process Effects at Turn Start

```csharp
// Called by ActorSystem at the start of entity's turn
var messages = statusEffectSystem.ProcessEffects(playerEntity);

// Display all feedback messages
foreach (var message in messages)
{
    hudService.AddMessage(message);
}

// Example messages:
// "You take 3 damage from poison."
// "You heal 2 HP from regeneration."
// "Strength buff has worn off."
```

### Process All Entities with Effects

```csharp
// Batch processing for all entities
statusEffectSystem.ProcessAllEffects();

// Processes effects for player and all enemies in one call
```

### Manual Turn Processing Example

```csharp
// In ActorSystem.ProcessTurn()
public void ProcessTurn(Entity entity)
{
    // 1. Process status effects FIRST
    var effectMessages = _statusEffectSystem.ProcessEffects(entity);
    foreach (var msg in effectMessages)
    {
        _hudService.AddMessage(msg);
    }

    // 2. Check if entity died from DoT
    var health = world.Get<Health>(entity);
    if (health.Current <= 0)
    {
        HandleDeath(entity);
        return; // Don't process turn if dead
    }

    // 3. Entity takes turn actions
    ProcessEntityActions(entity);
}
```

---

## Calculating Stats with Effects

### Get Stat Modifiers from Effects

```csharp
// Get only the modifiers from status effects
var (attackMod, defenseMod, speedMod) = statusEffectSystem.CalculateStatModifiers(playerEntity);

Console.WriteLine($"Attack modifier: {attackMod}");
Console.WriteLine($"Defense modifier: {defenseMod}");
Console.WriteLine($"Speed modifier: {speedMod}");

// Example output:
// Attack modifier: 2 (Strength +5, Weakness -3)
// Defense modifier: 5 (IronSkin +5)
// Speed modifier: -10 (Haste +20, Slow -30)
```

### Get Total Stats (Base + Equipment + Effects)

```csharp
// Get final stats including all modifiers
var (attack, defense, speed) = statusEffectSystem.CalculateTotalStats(playerEntity);

Console.WriteLine($"Total Attack: {attack}");
Console.WriteLine($"Total Defense: {defense}");
Console.WriteLine($"Total Speed: {speed}");

// Used by CombatSystem for damage calculation
```

### Use in Combat Calculation

```csharp
// In CombatSystem.CalculateDamage()
public int CalculateDamage(Entity attacker, Entity defender)
{
    // Get stats with all modifiers (base + equipment + effects)
    var (attackerAttack, _, _) = _statusEffectSystem.CalculateTotalStats(attacker);
    var (_, defenderDefense, _) = _statusEffectSystem.CalculateTotalStats(defender);

    int baseDamage = attackerAttack - defenderDefense;
    int finalDamage = Math.Max(1, baseDamage); // Minimum 1 damage

    return finalDamage;
}
```

---

## Integrating with Consumables

### Buff Potion Usage

```csharp
// In InventorySystem.UseConsumable()
public InventoryResult UseConsumable(Entity playerEntity, Entity itemEntity)
{
    var consumable = world.Get<Consumable>(itemEntity);

    // Check if consumable applies a status effect
    if (consumable.AppliesEffect.HasValue)
    {
        var result = _statusEffectSystem.ApplyEffect(
            playerEntity,
            effectType: consumable.AppliesEffect.Value,
            magnitude: consumable.EffectMagnitude ?? 0,
            duration: consumable.EffectDuration ?? 0,
            source: EffectSource.Consumable
        );

        if (!result.Success)
        {
            return InventoryResult.Failed(result.Message);
        }

        // Consume the item
        RemoveOrDecrementItem(playerEntity, itemEntity);

        return InventoryResult.Succeeded(result.Message);
    }

    // ... existing consumable logic (healing, etc.)
}
```

### Antidote Usage

```csharp
// In InventorySystem.UseConsumable()
if (consumable.RemovesEffect.HasValue)
{
    // Remove specific effect type
    var result = _statusEffectSystem.RemoveEffect(
        playerEntity,
        consumable.RemovesEffect.Value
    );

    RemoveOrDecrementItem(playerEntity, itemEntity);
    return InventoryResult.Succeeded(result.Message);
}

if (consumable.RemovesAllNegativeEffects)
{
    // Universal cure
    var result = _statusEffectSystem.RemoveAllNegativeEffects(playerEntity);

    RemoveOrDecrementItem(playerEntity, itemEntity);
    return InventoryResult.Succeeded(result.Message);
}
```

### New Consumable Definitions

```csharp
// Strength Potion
public static readonly ItemDefinition StrengthPotion = new()
{
    Name = "Strength Potion",
    Glyph = '!',
    Description = "Increases attack by 5 for 10 turns",
    Type = ItemType.Consumable,
    ConsumableEffect = null,
    AppliesEffect = EffectType.Strength,
    EffectMagnitude = 5,
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
```

---

## Integrating with Enemy Attacks

### Enemy with Poison Attack

```csharp
// Create Toxic Spider enemy
var toxicSpider = world.Create(
    new Enemy
    {
        EnemyType = "Toxic Spider",
        Behavior = AIBehavior.Chase,
        InflictsEffect = EffectType.Poison,
        EffectProbability = 40, // 40% chance
        EffectMagnitude = 3,
        EffectDuration = 5
    },
    new Combat { Attack = 5, Defense = 2 },
    new Health { Current = 20, Maximum = 20 },
    new Position { X = 10, Y = 10 },
    new StatusEffects { ActiveEffects = new List<StatusEffect>(), MaxEffects = 10 }
);
```

### Apply Effect on Hit

```csharp
// In CombatSystem.ProcessAttack()
public void ProcessAttack(Entity attacker, Entity defender)
{
    // Calculate and apply damage
    int damage = CalculateDamage(attacker, defender);
    ApplyDamage(defender, damage);

    // Check if attacker inflicts status effect
    if (world.Has<Enemy>(attacker))
    {
        var enemy = world.Get<Enemy>(attacker);

        if (enemy.InflictsEffect.HasValue)
        {
            // Roll for effect application
            int roll = _random.Next(100);
            if (roll < enemy.EffectProbability)
            {
                var result = _statusEffectSystem.ApplyEffect(
                    defender,
                    effectType: enemy.InflictsEffect.Value,
                    magnitude: enemy.EffectMagnitude ?? 0,
                    duration: enemy.EffectDuration ?? 0,
                    source: EffectSource.EnemyAttack
                );

                if (result.Success)
                {
                    _hudService.AddMessage(result.Message);
                }
            }
        }
    }
}
```

### Enemy Buff Spell

```csharp
// Shaman casts buff on ally
public void CastBuffSpell(Entity caster, Entity target)
{
    var enemy = world.Get<Enemy>(caster);

    if (enemy.InflictsEffect.HasValue)
    {
        var result = _statusEffectSystem.ApplyEffect(
            target,
            effectType: enemy.InflictsEffect.Value,
            magnitude: enemy.EffectMagnitude ?? 0,
            duration: enemy.EffectDuration ?? 0,
            source: EffectSource.EnemySpell
        );

        _hudService.AddMessage($"{enemy.EnemyType} casts a spell on ally!");
        _hudService.AddMessage(result.Message);
    }
}
```

---

## Displaying Effects in HUD

### Add Status Effects Panel

```csharp
// In HudService.cs
using Terminal.Gui;

public class HudService
{
    private FrameView _statusEffectsFrame;
    private ListView _statusEffectsList;

    public void CreateStatusEffectsPanel(View parent)
    {
        _statusEffectsFrame = new FrameView("Status Effects")
        {
            X = 0,
            Y = Pos.Bottom(_healthBar) + 1,
            Width = 30,
            Height = 8
        };

        _statusEffectsList = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _statusEffectsFrame.Add(_statusEffectsList);
        parent.Add(_statusEffectsFrame);
    }

    public void UpdateStatusEffects(Entity playerEntity, StatusEffectSystem statusEffectSystem)
    {
        var effects = statusEffectSystem.GetActiveEffects(playerEntity);
        var displayItems = new List<string>();

        // Sort by duration (longest first)
        var sortedEffects = effects.OrderByDescending(e => e.Duration).Take(5);

        foreach (var effect in sortedEffects)
        {
            var formatted = statusEffectSystem.FormatEffectForDisplay(effect);
            displayItems.Add(formatted);
        }

        if (displayItems.Count == 0)
        {
            displayItems.Add("(No active effects)");
        }

        _statusEffectsList.SetSource(displayItems);
    }
}
```

### Color-Coded Display

```csharp
// Format effect with color
public string FormatEffectForDisplay(StatusEffect effect)
{
    string colorCode = effect.Color switch
    {
        EffectColor.Red => "\x1b[31m",      // Red for debuffs
        EffectColor.Green => "\x1b[32m",    // Green for buffs
        EffectColor.Blue => "\x1b[34m",     // Blue for defense
        EffectColor.Cyan => "\x1b[36m",     // Cyan for speed
        EffectColor.Yellow => "\x1b[33m",   // Yellow for healing
        _ => "\x1b[0m"                       // Default
    };

    string reset = "\x1b[0m";

    return $"{colorCode}{effect.DisplayName} ({effect.Duration}){reset}";
}
```

### Update HUD Every Turn

```csharp
// In DungeonCrawlerService.cs, after processing turn
private void OnPlayerTurnEnd()
{
    // Process effects
    var messages = _statusEffectSystem.ProcessEffects(_playerEntity);
    foreach (var msg in messages)
    {
        _hudService.AddMessage(msg);
    }

    // Update HUD display
    _hudService.UpdateStatusEffects(_playerEntity, _statusEffectSystem);
    _hudService.UpdateStats(_playerEntity, _statusEffectSystem);
}
```

---

## Testing Examples

### Unit Test: Apply and Remove Effect

```csharp
[Fact]
public void ApplyEffect_AddsToActiveEffects()
{
    // Arrange
    var world = World.Create();
    var statusEffectSystem = new StatusEffectSystem(world);

    var player = world.Create(
        new StatusEffects { ActiveEffects = new List<StatusEffect>(), MaxEffects = 10 }
    );

    // Act
    var result = statusEffectSystem.ApplyEffect(
        player,
        EffectDefinitions.Poison,
        EffectSource.EnemyAttack
    );

    // Assert
    Assert.True(result.Success);
    Assert.Contains("poisoned", result.Message.ToLower());

    var effects = world.Get<StatusEffects>(player);
    Assert.Single(effects.ActiveEffects);
    Assert.Equal(EffectType.Poison, effects.ActiveEffects[0].Type);
}

[Fact]
public void RemoveEffect_RemovesFromActiveEffects()
{
    // Arrange
    var world = World.Create();
    var statusEffectSystem = new StatusEffectSystem(world);

    var player = world.Create(
        new StatusEffects { ActiveEffects = new List<StatusEffect>(), MaxEffects = 10 }
    );

    statusEffectSystem.ApplyEffect(player, EffectDefinitions.Poison, EffectSource.EnemyAttack);

    // Act
    var result = statusEffectSystem.RemoveEffect(player, EffectType.Poison);

    // Assert
    Assert.True(result.Success);
    var effects = world.Get<StatusEffects>(player);
    Assert.Empty(effects.ActiveEffects);
}
```

### Integration Test: Effect Stacking

```csharp
[Fact]
public void ApplyEffect_SameType_RefreshesDuration()
{
    // Arrange
    var world = World.Create();
    var statusEffectSystem = new StatusEffectSystem(world);

    var player = world.Create(
        new StatusEffects { ActiveEffects = new List<StatusEffect>(), MaxEffects = 10 }
    );

    // Apply Strength (+5 ATK, 10 turns)
    statusEffectSystem.ApplyEffect(player, EffectDefinitions.Strength, EffectSource.Consumable);

    // Wait 5 turns (simulate)
    var effects = world.Get<StatusEffects>(player);
    effects.ActiveEffects[0].Duration = 5;

    // Act: Apply Strength again
    statusEffectSystem.ApplyEffect(player, EffectDefinitions.Strength, EffectSource.Consumable);

    // Assert: Duration refreshed to 10, magnitude still 5
    effects = world.Get<StatusEffects>(player);
    Assert.Single(effects.ActiveEffects);
    Assert.Equal(10, effects.ActiveEffects[0].Duration);
    Assert.Equal(5, effects.ActiveEffects[0].Magnitude);
}
```

### Integration Test: DoT Processing

```csharp
[Fact]
public void ProcessEffects_AppliesDamageOverTime()
{
    // Arrange
    var world = World.Create();
    var statusEffectSystem = new StatusEffectSystem(world);

    var player = world.Create(
        new Health { Current = 50, Maximum = 100 },
        new StatusEffects { ActiveEffects = new List<StatusEffect>(), MaxEffects = 10 }
    );

    statusEffectSystem.ApplyEffect(player, EffectDefinitions.Poison, EffectSource.EnemyAttack);

    // Act: Process effects (simulates turn start)
    var messages = statusEffectSystem.ProcessEffects(player);

    // Assert: Damage applied, duration decremented
    var health = world.Get<Health>(player);
    Assert.Equal(47, health.Current); // 50 - 3 poison damage

    var effects = world.Get<StatusEffects>(player);
    Assert.Equal(4, effects.ActiveEffects[0].Duration); // 5 - 1

    Assert.Contains("poison", messages[0].ToLower());
}
```

### Manual Test: Full Combat with Effects

```csharp
// 1. Player fights Toxic Spider
var spider = CreateToxicSpider();
CombatSystem.ProcessAttack(spider, playerEntity);

// 2. Player gets poisoned (40% chance)
// Verify: HUD shows "Poisoned (5)" in red

// 3. Player drinks Strength Potion
InventorySystem.UseConsumable(playerEntity, strengthPotionEntity);

// Verify: HUD shows "Strength (10)" in green

// 4. Player attacks with buffed stats
var (attack, _, _) = StatusEffectSystem.CalculateTotalStats(playerEntity);
// Verify: Attack = base + equipment + 5 (Strength buff)

// 5. Turn ends, effects process
StatusEffectSystem.ProcessEffects(playerEntity);

// Verify:
// - Player takes 3 poison damage
// - HUD shows "Poisoned (4)" (duration decremented)
// - HUD shows "Strength (9)"

// 6. Player uses Antidote
InventorySystem.UseConsumable(playerEntity, antidoteEntity);

// Verify:
// - "Poison cured!" message
// - HUD no longer shows Poisoned
// - Strength buff still active
```

---

## Common Patterns

### Pattern: Check Before Apply

```csharp
public void UseBuffPotion(Entity player, EffectType effectType)
{
    if (statusEffectSystem.HasEffect(player, effectType))
    {
        Console.WriteLine($"You already have {effectType} active.");
        Console.WriteLine("Using this potion will refresh the duration.");
    }

    var result = statusEffectSystem.ApplyEffect(
        player,
        EffectDefinitions.GetDefinition(effectType),
        EffectSource.Consumable
    );

    Console.WriteLine(result.Message);
}
```

### Pattern: Multiple Effect Display

```csharp
public void ShowActiveEffects(Entity entity)
{
    var effects = statusEffectSystem.GetActiveEffects(entity);

    if (effects.Count == 0)
    {
        Console.WriteLine("No active effects.");
        return;
    }

    Console.WriteLine($"Active Effects ({effects.Count}):");

    var buffs = effects.Where(e => e.Category == EffectCategory.StatBuff);
    var debuffs = effects.Where(e => e.Category == EffectCategory.StatDebuff);
    var dot = effects.Where(e => e.Category == EffectCategory.DamageOverTime);
    var hot = effects.Where(e => e.Category == EffectCategory.HealingOverTime);

    if (buffs.Any())
    {
        Console.WriteLine("  Buffs:");
        foreach (var buff in buffs)
            Console.WriteLine($"    {buff.DisplayName} ({buff.Duration} turns)");
    }

    if (debuffs.Any())
    {
        Console.WriteLine("  Debuffs:");
        foreach (var debuff in debuffs)
            Console.WriteLine($"    {debuff.DisplayName} ({debuff.Duration} turns)");
    }

    if (dot.Any())
    {
        Console.WriteLine("  Damage Over Time:");
        foreach (var effect in dot)
            Console.WriteLine($"    {effect.DisplayName} (-{effect.Magnitude} HP/turn, {effect.Duration} turns)");
    }

    if (hot.Any())
    {
        Console.WriteLine("  Healing Over Time:");
        foreach (var effect in hot)
            Console.WriteLine($"    {effect.DisplayName} (+{effect.Magnitude} HP/turn, {effect.Duration} turns)");
    }
}
```

### Pattern: Net Effect Calculation

```csharp
public int CalculateNetHealthChange(Entity entity)
{
    var dotEffects = statusEffectSystem.GetDamageOverTimeEffects(entity);
    var hotEffects = statusEffectSystem.GetHealingOverTimeEffects(entity);

    int totalDamage = dotEffects.Sum(e => e.Magnitude);
    int totalHealing = hotEffects.Sum(e => e.Magnitude);

    int netChange = totalHealing - totalDamage;

    Console.WriteLine($"Net HP change per turn: {netChange:+#;-#;0}");
    return netChange;
}
```

---

## Next Steps

- Review [data-model.md](./data-model.md) for complete component schemas
- Review [contracts/](./contracts/) for StatusEffectSystem interface
- Implement StatusEffectSystem in `LablabBean.Game.Core/Systems/`
- Extend ActorSystem to call ProcessEffects() at turn start
- Extend CombatSystem to use CalculateTotalStats()
- Extend InventorySystem for buff potions and antidotes
- Add status effects panel to HUD
- Write unit tests for each system method
- Run integration tests for full workflows

---

**Quickstart Complete**: Ready to begin implementation!
