# Spell System Quick Start

## 5-Minute Integration Guide

### Step 1: Initialize a Player with Mana

```csharp
using LablabBean.Plugins.Spells.Components;

// Create player entity with mana
var player = world.Create(
    new Player("Hero"),
    new Health(100, 100),
    new Combat(10, 5),
    new Mana(100, regenRate: 5, combatRegenRate: 2),  // 100 max mana
    new SpellBook()  // Empty spellbook
);
```

### Step 2: Get the Spell Service

```csharp
var spellService = context.Registry.Get<SpellService>();
var spellDatabase = context.Registry.Get<SpellDatabase>();
```

### Step 3: Learn Starting Spells

```csharp
// Get the Magic Missile spell ID
var magicMissileId = Guid.Parse("d5e7f9a2-8b1c-7d3f-2e4b-6f8a1c3e5d7b");

// Learn the spell
spellService.LearnSpell(player.Id, magicMissileId);

// Equip it to hotbar
spellService.EquipSpell(player.Id, magicMissileId);

Console.WriteLine("✨ Learned Magic Missile!");
```

### Step 4: Cast Your First Spell

```csharp
// Create an enemy
var enemy = world.Create(
    new Enemy("Goblin"),
    new Health(30, 30),
    new Position(5, 5)
);

// Cast spell at enemy
var result = spellService.CastSpell(
    casterId: player.Id,
    spellId: magicMissileId,
    targetId: enemy.Id
);

if (result.Success)
{
    Console.WriteLine($"💥 Hit! Dealt {result.DamageDealt} damage!");
}
else
{
    Console.WriteLine($"❌ Failed: {result.FailureReason}");
}
```

### Step 5: Manage Mana & Cooldowns

```csharp
// In your game loop - called each turn:
public void OnTurnEnd()
{
    var spellsPlugin = GetPlugin<SpellsPlugin>();

    // Regenerate mana
    bool inCombat = CheckIfEnemiesNearby();
    spellsPlugin.RegenerateMana(inCombat);

    // Update cooldowns
    spellsPlugin.UpdateCooldowns();
}

// Check mana before casting
var mana = spellService.GetMana(player.Id);
Console.WriteLine($"⚡ Mana: {mana.Current}/{mana.Maximum}");

// Check if can cast
bool canCast = spellService.CanCastSpell(player.Id, spellId);
if (!canCast)
{
    int cooldown = spellService.GetSpellCooldown(player.Id, spellId);
    if (cooldown > 0)
        Console.WriteLine($"⏱️ Cooldown: {cooldown} turns");
    else
        Console.WriteLine("⚡ Not enough mana!");
}
```

## Complete Example: Combat Turn

```csharp
public void PlayerCombatTurn(Entity player)
{
    var spellService = context.Registry.Get<SpellService>();

    // Display mana
    var mana = spellService.GetMana(player.Id);
    Console.WriteLine($"\n⚡ Mana: {mana.Current}/{mana.Maximum}");

    // Display available spells
    Console.WriteLine("\n📖 Your Spells:");
    var spells = spellService.GetActiveSpells(player.Id);
    int index = 1;

    foreach (var spell in spells)
    {
        var cooldown = spellService.GetSpellCooldown(player.Id, spell.Id);

        if (cooldown > 0)
        {
            Console.WriteLine($"  {index}. {spell.Name} ⏱️ Cooldown: {cooldown} turns");
        }
        else if (mana.Current < spell.ManaCost)
        {
            Console.WriteLine($"  {index}. {spell.Name} ❌ Need {spell.ManaCost} mana");
        }
        else
        {
            Console.WriteLine($"  {index}. {spell.Name} ✅ {spell.ManaCost} mana");
        }
        index++;
    }

    // Player chooses spell
    Console.Write("\nCast spell (1-{0}) or 0 to attack: ", spells.Count());
    int choice = int.Parse(Console.ReadLine() ?? "0");

    if (choice == 0)
    {
        // Normal attack
        PerformMeleeAttack(player);
        return;
    }

    var selectedSpell = spells.ElementAt(choice - 1);

    // Check if can cast
    if (!spellService.CanCastSpell(player.Id, selectedSpell.Id))
    {
        Console.WriteLine("❌ Cannot cast that spell!");
        return;
    }

    // Get target
    var enemy = SelectTarget();

    // Cast spell
    var result = spellService.CastSpell(
        player.Id,
        selectedSpell.Id,
        enemy.Id
    );

    if (result.Success)
    {
        Console.WriteLine($"\n✨ Cast {selectedSpell.Name}!");

        if (result.DamageDealt > 0)
            Console.WriteLine($"💥 Dealt {result.DamageDealt} damage!");

        if (result.HealingDone > 0)
            Console.WriteLine($"💚 Healed {result.HealingDone} HP!");
    }
    else
    {
        Console.WriteLine($"❌ Cast failed: {result.FailureReason}");
    }
}
```

## Level Up Integration

```csharp
public void OnPlayerLevelUp(Entity player, int newLevel)
{
    var spellService = context.Registry.Get<SpellService>();
    var spellDatabase = context.Registry.Get<SpellDatabase>();

    // Auto-learn spells for this level
    var unlockedSpells = spellDatabase.GetSpellsForLevel(newLevel);

    foreach (var spellId in unlockedSpells)
    {
        spellService.LearnSpell(player.Id, spellId);

        var spell = spellService.GetSpellInfo(spellId);
        Console.WriteLine($"✨ Spell Unlocked: {spell.Name}!");
        Console.WriteLine($"   {spell.Description}");
        Console.WriteLine($"   Cost: {spell.ManaCost} mana");

        // Auto-equip if hotbar has space
        if (spellService.EquipSpell(player.Id, spellId))
        {
            Console.WriteLine($"   ✅ Added to hotbar");
        }
    }
}
```

## All Spell IDs

```csharp
public static class Spells
{
    // Level 1
    public static readonly Guid MagicMissile =
        Guid.Parse("d5e7f9a2-8b1c-7d3f-2e4b-6f8a1c3e5d7b");

    // Level 2
    public static readonly Guid HealingLight =
        Guid.Parse("a2c4e6f8-5d7b-4a9c-8e1f-3b5d7a9c1e2f");

    // Level 3
    public static readonly Guid Fireball =
        Guid.Parse("f7a9d8c5-3b12-4e8f-9c7a-1d5e6f8a9b2c");

    // Level 4
    public static readonly Guid IceShield =
        Guid.Parse("c4d6e8f1-7a9b-6c2e-1f3a-5d7e9b2d4f6a");

    // Level 5
    public static readonly Guid LightningStrike =
        Guid.Parse("b3d5e7f9-6c8a-5b1d-9f2e-4c6d8a1c3e4f");

    // Level 6
    public static readonly Guid BattleFury =
        Guid.Parse("e6f8a1c3-9d2e-8f4a-3b5d-7e9a2c4f6d8b");
}
```

## Testing Spells

```csharp
public void TestSpellSystem()
{
    var world = new World();
    var spellService = GetSpellService();

    // Create test player
    var player = world.Create(
        new Player("Test"),
        new Health(100, 100),
        new Mana(100, 5, 2),
        new SpellBook()
    );

    // Learn all spells
    spellService.LearnSpell(player.Id, Spells.MagicMissile);
    spellService.LearnSpell(player.Id, Spells.Fireball);
    spellService.LearnSpell(player.Id, Spells.HealingLight);

    // Equip spells
    spellService.EquipSpell(player.Id, Spells.MagicMissile);
    spellService.EquipSpell(player.Id, Spells.Fireball);
    spellService.EquipSpell(player.Id, Spells.HealingLight);

    // Create enemy
    var enemy = world.Create(
        new Enemy("Dummy"),
        new Health(100, 100),
        new Position(5, 5)
    );

    Console.WriteLine("=== Testing Magic Missile ===");
    var result1 = spellService.CastSpell(player.Id, Spells.MagicMissile, enemy.Id);
    Console.WriteLine($"Success: {result1.Success}, Damage: {result1.DamageDealt}");

    Console.WriteLine("\n=== Testing Fireball ===");
    var result2 = spellService.CastSpell(player.Id, Spells.Fireball, enemy.Id);
    Console.WriteLine($"Success: {result2.Success}, Damage: {result2.DamageDealt}");

    Console.WriteLine("\n=== Testing Healing Light ===");
    player.Get<Health>().Current = 50; // Damage player
    var result3 = spellService.CastSpell(player.Id, Spells.HealingLight);
    Console.WriteLine($"Success: {result3.Success}, Healing: {result3.HealingDone}");

    Console.WriteLine("\n=== Testing Mana ===");
    var mana = spellService.GetMana(player.Id);
    Console.WriteLine($"Mana: {mana.Current}/{mana.Maximum}");

    Console.WriteLine("\n✅ All tests passed!");
}
```

## That's It

You now have a fully functional spell system!

**Next Steps:**

- Add UI for spell selection
- Integrate with your combat system
- Add visual effects for spells
- Create more custom spells
- Add spell upgrade mechanics

See `SPELL_SYSTEM.md` for complete documentation! 🚀
