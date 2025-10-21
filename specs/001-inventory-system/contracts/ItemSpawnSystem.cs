// Contract: ItemSpawnSystem
// Purpose: Defines the interface for item spawning and generation
// Location: LablabBean.Game.Core/Systems/ItemSpawnSystem.cs

namespace LablabBean.Game.Core.Systems;

using Arch.Core;
using LablabBean.Game.Core.Components;
using LablabBean.Game.Core.Maps;

/// <summary>
/// System responsible for spawning items in the dungeon.
/// Handles room-based spawning during generation and loot drops from enemies.
/// </summary>
public class ItemSpawnSystem
{
    private readonly World _world;
    private readonly Random _random;

    public ItemSpawnSystem(World world, Random? random = null)
    {
        _world = world;
        _random = random ?? new Random();
    }

    // ═══════════════════════════════════════════════════════════════
    // MAP GENERATION SPAWNING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Spawns items in dungeon rooms during map generation.
    /// </summary>
    /// <param name="map">The dungeon map</param>
    /// <param name="rooms">List of room rectangles</param>
    /// <remarks>
    /// Spawning Rules (from spec FR-001):
    /// - Healing potions: 20-50% of rooms
    /// - Weapons/Armor: 10-20% of rooms
    /// - Items spawn at random walkable positions within rooms
    /// - Multiple items can spawn in same room
    /// </remarks>
    public void SpawnItemsInRooms(DungeonMap map, List<Rectangle> rooms);

    /// <summary>
    /// Spawns a single item at a specific location.
    /// </summary>
    /// <param name="itemType">Type of item to spawn</param>
    /// <param name="position">Position to spawn at</param>
    /// <returns>The spawned item entity</returns>
    public Entity SpawnItem(ItemDefinition itemType, Position position);

    /// <summary>
    /// Gets a random item from a spawn table based on weights.
    /// </summary>
    /// <param name="table">The spawn table to use</param>
    /// <returns>Item definition to spawn</returns>
    public ItemDefinition GetRandomItem(SpawnTable table);

    // ═══════════════════════════════════════════════════════════════
    // ENEMY LOOT DROPS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Spawns loot when an enemy dies.
    /// </summary>
    /// <param name="enemyEntity">The enemy that died</param>
    /// <param name="position">Position where enemy died</param>
    /// <remarks>
    /// Drop Rates (from spec FR-002):
    /// - 30% chance to drop healing potion
    /// - 10% chance to drop equipment (weapon or armor)
    /// - Items spawn at enemy's death location
    /// </remarks>
    public void SpawnEnemyLoot(Entity enemyEntity, Position position);

    /// <summary>
    /// Determines what loot an enemy should drop based on enemy type.
    /// </summary>
    /// <param name="enemyType">Type of enemy (e.g., "Goblin", "Orc")</param>
    /// <returns>List of items to drop (may be empty)</returns>
    public List<ItemDefinition> DetermineLoot(string enemyType);

    // ═══════════════════════════════════════════════════════════════
    // ITEM DEFINITIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the spawn table for room-based item generation.
    /// </summary>
    /// <returns>Spawn table with weighted item definitions</returns>
    public SpawnTable GetRoomSpawnTable();

    /// <summary>
    /// Gets the spawn table for enemy loot drops.
    /// </summary>
    /// <param name="enemyType">Type of enemy</param>
    /// <returns>Spawn table specific to enemy type</returns>
    public SpawnTable GetEnemyLootTable(string enemyType);
}

/// <summary>
/// Definition of an item type that can be spawned.
/// </summary>
public struct ItemDefinition
{
    public string Name { get; set; }
    public char Glyph { get; set; }
    public string Description { get; set; }
    public ItemType Type { get; set; }
    
    // Consumable properties (if applicable)
    public ConsumableEffect? ConsumableEffect { get; set; }
    public int? ConsumableValue { get; set; }
    
    // Equipment properties (if applicable)
    public EquipmentSlot? EquipmentSlot { get; set; }
    public int? AttackBonus { get; set; }
    public int? DefenseBonus { get; set; }
    public int? SpeedModifier { get; set; }
    
    // Stackable properties
    public bool IsStackable { get; set; }
    public int MaxStackSize { get; set; }
}

/// <summary>
/// Weighted spawn table for random item selection.
/// </summary>
public class SpawnTable
{
    private readonly List<(ItemDefinition item, int weight)> _entries = new();
    private int _totalWeight;

    /// <summary>
    /// Adds an item to the spawn table with a weight.
    /// Higher weight = more likely to spawn.
    /// </summary>
    public void Add(ItemDefinition item, int weight)
    {
        _entries.Add((item, weight));
        _totalWeight += weight;
    }

    /// <summary>
    /// Selects a random item from the table based on weights.
    /// </summary>
    public ItemDefinition SelectRandom(Random random)
    {
        int roll = random.Next(_totalWeight);
        int cumulative = 0;
        
        foreach (var (item, weight) in _entries)
        {
            cumulative += weight;
            if (roll < cumulative)
                return item;
        }
        
        return _entries[^1].item; // Fallback
    }

    /// <summary>
    /// Gets all items in the table.
    /// </summary>
    public IReadOnlyList<(ItemDefinition item, int weight)> Entries => _entries;
}

/// <summary>
/// Predefined item definitions for the MVP.
/// </summary>
public static class ItemDefinitions
{
    // ═══════════════════════════════════════════════════════════════
    // CONSUMABLES
    // ═══════════════════════════════════════════════════════════════

    public static readonly ItemDefinition HealingPotion = new()
    {
        Name = "Healing Potion",
        Glyph = '!',
        Description = "A red potion that restores 30 HP.",
        Type = ItemType.Consumable,
        ConsumableEffect = Components.ConsumableEffect.RestoreHealth,
        ConsumableValue = 30,
        IsStackable = true,
        MaxStackSize = 99
    };

    // ═══════════════════════════════════════════════════════════════
    // WEAPONS
    // ═══════════════════════════════════════════════════════════════

    public static readonly ItemDefinition IronSword = new()
    {
        Name = "Iron Sword",
        Glyph = '/',
        Description = "A basic iron sword. +5 Attack.",
        Type = ItemType.Weapon,
        EquipmentSlot = Components.EquipmentSlot.MainHand,
        AttackBonus = 5,
        DefenseBonus = 0,
        SpeedModifier = 0,
        IsStackable = false
    };

    public static readonly ItemDefinition SteelSword = new()
    {
        Name = "Steel Sword",
        Glyph = '/',
        Description = "A well-crafted steel sword. +10 Attack.",
        Type = ItemType.Weapon,
        EquipmentSlot = Components.EquipmentSlot.MainHand,
        AttackBonus = 10,
        DefenseBonus = 0,
        SpeedModifier = 0,
        IsStackable = false
    };

    public static readonly ItemDefinition WoodenClub = new()
    {
        Name = "Wooden Club",
        Glyph = '\\',
        Description = "A crude wooden club. +3 Attack.",
        Type = ItemType.Weapon,
        EquipmentSlot = Components.EquipmentSlot.MainHand,
        AttackBonus = 3,
        DefenseBonus = 0,
        SpeedModifier = -5,
        IsStackable = false
    };

    // ═══════════════════════════════════════════════════════════════
    // ARMOR
    // ═══════════════════════════════════════════════════════════════

    public static readonly ItemDefinition LeatherArmor = new()
    {
        Name = "Leather Armor",
        Glyph = '[',
        Description = "Light leather armor. +3 Defense.",
        Type = ItemType.Armor,
        EquipmentSlot = Components.EquipmentSlot.Chest,
        AttackBonus = 0,
        DefenseBonus = 3,
        SpeedModifier = 0,
        IsStackable = false
    };

    public static readonly ItemDefinition ChainMail = new()
    {
        Name = "Chain Mail",
        Glyph = '[',
        Description = "Heavy chain mail armor. +8 Defense, -10 Speed.",
        Type = ItemType.Armor,
        EquipmentSlot = Components.EquipmentSlot.Chest,
        AttackBonus = 0,
        DefenseBonus = 8,
        SpeedModifier = -10,
        IsStackable = false
    };

    public static readonly ItemDefinition IronHelmet = new()
    {
        Name = "Iron Helmet",
        Glyph = ']',
        Description = "A sturdy iron helmet. +2 Defense.",
        Type = ItemType.Armor,
        EquipmentSlot = Components.EquipmentSlot.Head,
        AttackBonus = 0,
        DefenseBonus = 2,
        SpeedModifier = 0,
        IsStackable = false
    };

    // ═══════════════════════════════════════════════════════════════
    // SPAWN TABLES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Default spawn table for room-based item generation.
    /// Weights determine relative spawn rates.
    /// </summary>
    public static SpawnTable CreateRoomSpawnTable()
    {
        var table = new SpawnTable();
        
        // Consumables (higher weight = more common)
        table.Add(HealingPotion, 50);  // 50% of spawns
        
        // Weapons (medium weight)
        table.Add(WoodenClub, 15);     // 15%
        table.Add(IronSword, 10);      // 10%
        table.Add(SteelSword, 5);      // 5% (rare)
        
        // Armor (medium weight)
        table.Add(LeatherArmor, 10);   // 10%
        table.Add(IronHelmet, 8);      // 8%
        table.Add(ChainMail, 2);       // 2% (rare)
        
        return table;
    }

    /// <summary>
    /// Spawn table for basic enemy loot (goblins, rats, etc.)
    /// </summary>
    public static SpawnTable CreateBasicEnemyLootTable()
    {
        var table = new SpawnTable();
        table.Add(HealingPotion, 70);  // 70% potions
        table.Add(WoodenClub, 20);     // 20% weak weapons
        table.Add(LeatherArmor, 10);   // 10% weak armor
        return table;
    }

    /// <summary>
    /// Spawn table for elite enemy loot (orcs, trolls, etc.)
    /// </summary>
    public static SpawnTable CreateEliteEnemyLootTable()
    {
        var table = new SpawnTable();
        table.Add(HealingPotion, 40);  // 40% potions
        table.Add(IronSword, 30);      // 30% good weapons
        table.Add(ChainMail, 20);      // 20% good armor
        table.Add(SteelSword, 10);     // 10% rare weapons
        return table;
    }
}

/// <summary>
/// Simple rectangle struct for room boundaries.
/// </summary>
public struct Rectangle
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public (int x, int y) Center => (X + Width / 2, Y + Height / 2);
    
    public bool Contains(int x, int y)
        => x >= X && x < X + Width && y >= Y && y < Y + Height;
}
