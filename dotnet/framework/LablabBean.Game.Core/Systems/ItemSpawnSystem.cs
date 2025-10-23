using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using Microsoft.Extensions.Logging;
using SadRogue.Primitives;

namespace LablabBean.Game.Core.Systems;

/// <summary>
/// System that spawns items in the dungeon and handles loot drops
/// </summary>
public class ItemSpawnSystem
{
    private readonly ILogger<ItemSpawnSystem> _logger;

    public ItemSpawnSystem(ILogger<ItemSpawnSystem> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Spawns an item at the specified position
    /// </summary>
    /// <typeparam name="T">ItemDefinition type (e.g., typeof(ItemDefinitions.HealingPotion))</typeparam>
    public Entity SpawnItem(World world, Point position, Type itemDefinitionType)
    {
        var itemProp = itemDefinitionType.GetProperty("Item");
        var renderableProp = itemDefinitionType.GetProperty("Renderable");

        if (itemProp == null || renderableProp == null)
            throw new ArgumentException($"Type {itemDefinitionType.Name} is not a valid ItemDefinition");

        var item = (Item)itemProp.GetValue(null)!;
        var renderable = (Renderable)renderableProp.GetValue(null)!;

        // Create base entity with common components
        var entity = world.Create(
            item,
            new Position(position),
            renderable,
            new Visible(true)
        );

        // Add optional components based on item type
        var consumableProp = itemDefinitionType.GetProperty("Consumable");
        if (consumableProp != null)
        {
            var consumable = (Consumable)consumableProp.GetValue(null)!;
            world.Add(entity, consumable);
        }

        var equippableProp = itemDefinitionType.GetProperty("Equippable");
        if (equippableProp != null)
        {
            var equippable = (Equippable)equippableProp.GetValue(null)!;
            world.Add(entity, equippable);
        }

        var stackableProp = itemDefinitionType.GetProperty("Stackable");
        if (stackableProp != null)
        {
            var stackable = (Stackable)stackableProp.GetValue(null)!;
            world.Add(entity, stackable);
        }

        _logger.LogDebug("Spawned {ItemName} at position ({X}, {Y})", item.Name, position.X, position.Y);
        return entity;
    }

    /// <summary>
    /// Spawns a healing potion at the specified position (convenience method)
    /// </summary>
    public Entity SpawnHealingPotion(World world, Point position)
    {
        return SpawnItem(world, position, typeof(ItemDefinitions.HealingPotion));
    }

    /// <summary>
    /// Spawns an iron sword at the specified position (convenience method)
    /// </summary>
    public Entity SpawnIronSword(World world, Point position)
    {
        return SpawnItem(world, position, typeof(ItemDefinitions.IronSword));
    }

    /// <summary>
    /// Spawns items in dungeon rooms (20-50% chance per room)
    /// Uses weighted spawn tables for variety
    /// </summary>
    public void SpawnItemsInRooms(World world, List<Rectangle> rooms, Random random)
    {
        int itemsSpawned = 0;

        // Weighted spawn table for room items
        var spawnTable = new List<(Type ItemType, int Weight)>
        {
            // Consumables (60% total)
            (typeof(ItemDefinitions.HealingPotion), 40),
            (typeof(ItemDefinitions.GreaterHealingPotion), 20),

            // Weapons (20% total)
            (typeof(ItemDefinitions.Dagger), 8),
            (typeof(ItemDefinitions.IronSword), 8),
            (typeof(ItemDefinitions.SteelSword), 4),

            // Armor (15% total)
            (typeof(ItemDefinitions.LeatherArmor), 6),
            (typeof(ItemDefinitions.LeatherHelmet), 4),
            (typeof(ItemDefinitions.WoodenShield), 3),
            (typeof(ItemDefinitions.ChainMail), 2),

            // Accessories (5% total)
            (typeof(ItemDefinitions.RingOfStrength), 2),
            (typeof(ItemDefinitions.RingOfProtection), 2),
            (typeof(ItemDefinitions.RingOfSpeed), 1)
        };

        int totalWeight = spawnTable.Sum(entry => entry.Weight);

        // Spawn items in 20-50% of rooms
        foreach (var room in rooms)
        {
            // Skip first room (player spawn)
            if (room == rooms[0])
                continue;

            // 20-50% chance to spawn an item
            if (random.Next(100) < 35)
            {
                // Pick random item from weighted table
                int roll = random.Next(totalWeight);
                int cumulativeWeight = 0;
                Type? selectedItemType = null;

                foreach (var (itemType, weight) in spawnTable)
                {
                    cumulativeWeight += weight;
                    if (roll < cumulativeWeight)
                    {
                        selectedItemType = itemType;
                        break;
                    }
                }

                if (selectedItemType != null)
                {
                    // Spawn at random position in room
                    int x = random.Next(room.X + 1, room.X + room.Width - 1);
                    int y = random.Next(room.Y + 1, room.Y + room.Height - 1);
                    var position = new Point(x, y);

                    SpawnItem(world, position, selectedItemType);
                    itemsSpawned++;
                }
            }
        }

        _logger.LogInformation("Spawned {ItemCount} items across {RoomCount} rooms", itemsSpawned, rooms.Count);
    }

    /// <summary>
    /// Spawns loot when an enemy dies
    /// 30% chance for healing potion, 10% chance for equipment
    /// </summary>
    public void SpawnEnemyLoot(World world, Point position, Random random)
    {
        int roll = random.Next(100);

        if (roll < 30)
        {
            // 30% chance: Healing potion
            SpawnItem(world, position, typeof(ItemDefinitions.HealingPotion));
            _logger.LogDebug("Enemy dropped Healing Potion at ({X}, {Y})", position.X, position.Y);
        }
        else if (roll < 40)
        {
            // 10% chance: Random equipment
            var equipmentTable = new[]
            {
                typeof(ItemDefinitions.Dagger),
                typeof(ItemDefinitions.IronSword),
                typeof(ItemDefinitions.LeatherArmor),
                typeof(ItemDefinitions.LeatherHelmet),
                typeof(ItemDefinitions.WoodenShield)
            };

            var selectedEquipment = equipmentTable[random.Next(equipmentTable.Length)];
            SpawnItem(world, position, selectedEquipment);

            var itemName = selectedEquipment.Name.Replace("ItemDefinitions.", "").Replace("+", " ");
            _logger.LogDebug("Enemy dropped {ItemName} at ({X}, {Y})", itemName, position.X, position.Y);
        }
        // 60% chance: No loot
    }
}

/// <summary>
/// Predefined item templates for spawning
/// </summary>
public static class ItemDefinitions
{
    // Consumables - Healing Potions
    public static class HealingPotion
    {
        public static Item Item => new Item("Healing Potion", '!', ItemType.Consumable, "Restores 30 HP when consumed", weight: 1);
        public static Consumable Consumable => new Consumable(ConsumableEffect.RestoreHealth, effectValue: 30, usableOutOfCombat: true);
        public static Stackable Stackable => new Stackable(count: 1, maxStack: 99);
        public static Renderable Renderable => new Renderable('!', Color.Red, Color.Black, zOrder: 10);
    }

    public static class GreaterHealingPotion
    {
        public static Item Item => new Item("Greater Healing Potion", '!', ItemType.Consumable, "Restores 50 HP when consumed", weight: 1);
        public static Consumable Consumable => new Consumable(ConsumableEffect.RestoreHealth, effectValue: 50, usableOutOfCombat: true);
        public static Stackable Stackable => new Stackable(count: 1, maxStack: 99);
        public static Renderable Renderable => new Renderable('!', Color.Pink, Color.Black, zOrder: 10);
    }

    // Weapons - Swords
    public static class IronSword
    {
        public static Item Item => new Item("Iron Sword", '/', ItemType.Weapon, "A sturdy iron sword", weight: 5);
        public static Equippable Equippable => new Equippable(EquipmentSlot.MainHand, attackBonus: 5, defenseBonus: 0);
        public static Renderable Renderable => new Renderable('/', Color.Silver, Color.Black, zOrder: 10);
    }

    public static class SteelSword
    {
        public static Item Item => new Item("Steel Sword", '/', ItemType.Weapon, "A well-crafted steel sword", weight: 5);
        public static Equippable Equippable => new Equippable(EquipmentSlot.MainHand, attackBonus: 10, defenseBonus: 0);
        public static Renderable Renderable => new Renderable('/', Color.LightBlue, Color.Black, zOrder: 10);
    }

    public static class Dagger
    {
        public static Item Item => new Item("Dagger", '/', ItemType.Weapon, "A quick, light blade", weight: 2);
        public static Equippable Equippable => new Equippable(EquipmentSlot.MainHand, attackBonus: 3, defenseBonus: 0, speedModifier: 5);
        public static Renderable Renderable => new Renderable('/', Color.Gray, Color.Black, zOrder: 10);
    }

    // Armor - Chest
    public static class LeatherArmor
    {
        public static Item Item => new Item("Leather Armor", '[', ItemType.Armor, "Light leather protection", weight: 8);
        public static Equippable Equippable => new Equippable(EquipmentSlot.Chest, attackBonus: 0, defenseBonus: 3);
        public static Renderable Renderable => new Renderable('[', Color.Brown, Color.Black, zOrder: 10);
    }

    public static class ChainMail
    {
        public static Item Item => new Item("Chain Mail", '[', ItemType.Armor, "Interlocking metal rings", weight: 15);
        public static Equippable Equippable => new Equippable(EquipmentSlot.Chest, attackBonus: 0, defenseBonus: 6, speedModifier: -5);
        public static Renderable Renderable => new Renderable('[', Color.Silver, Color.Black, zOrder: 10);
    }

    public static class PlateArmor
    {
        public static Item Item => new Item("Plate Armor", '[', ItemType.Armor, "Heavy steel plate armor", weight: 25);
        public static Equippable Equippable => new Equippable(EquipmentSlot.Chest, attackBonus: 0, defenseBonus: 10, speedModifier: -10);
        public static Renderable Renderable => new Renderable('[', Color.LightGray, Color.Black, zOrder: 10);
    }

    // Armor - Head
    public static class LeatherHelmet
    {
        public static Item Item => new Item("Leather Helmet", '^', ItemType.Armor, "Basic head protection", weight: 2);
        public static Equippable Equippable => new Equippable(EquipmentSlot.Head, attackBonus: 0, defenseBonus: 1);
        public static Renderable Renderable => new Renderable('^', Color.Brown, Color.Black, zOrder: 10);
    }

    public static class IronHelmet
    {
        public static Item Item => new Item("Iron Helmet", '^', ItemType.Armor, "Sturdy iron helm", weight: 5);
        public static Equippable Equippable => new Equippable(EquipmentSlot.Head, attackBonus: 0, defenseBonus: 2);
        public static Renderable Renderable => new Renderable('^', Color.Silver, Color.Black, zOrder: 10);
    }

    // Shields
    public static class WoodenShield
    {
        public static Item Item => new Item("Wooden Shield", ')', ItemType.Armor, "A simple wooden shield", weight: 6);
        public static Equippable Equippable => new Equippable(EquipmentSlot.OffHand, attackBonus: 0, defenseBonus: 2);
        public static Renderable Renderable => new Renderable(')', Color.Brown, Color.Black, zOrder: 10);
    }

    public static class IronShield
    {
        public static Item Item => new Item("Iron Shield", ')', ItemType.Armor, "A sturdy iron shield", weight: 10);
        public static Equippable Equippable => new Equippable(EquipmentSlot.OffHand, attackBonus: 0, defenseBonus: 4);
        public static Renderable Renderable => new Renderable(')', Color.Silver, Color.Black, zOrder: 10);
    }

    // Accessories - Rings
    public static class RingOfStrength
    {
        public static Item Item => new Item("Ring of Strength", '=', ItemType.Accessory, "Increases attack power", weight: 0);
        public static Equippable Equippable => new Equippable(EquipmentSlot.Accessory1, attackBonus: 3, defenseBonus: 0);
        public static Renderable Renderable => new Renderable('=', Color.Gold, Color.Black, zOrder: 10);
    }

    public static class RingOfProtection
    {
        public static Item Item => new Item("Ring of Protection", '=', ItemType.Accessory, "Increases defense", weight: 0);
        public static Equippable Equippable => new Equippable(EquipmentSlot.Accessory1, attackBonus: 0, defenseBonus: 3);
        public static Renderable Renderable => new Renderable('=', Color.Silver, Color.Black, zOrder: 10);
    }

    public static class RingOfSpeed
    {
        public static Item Item => new Item("Ring of Speed", '=', ItemType.Accessory, "Increases movement speed", weight: 0);
        public static Equippable Equippable => new Equippable(EquipmentSlot.Accessory1, attackBonus: 0, defenseBonus: 0, speedModifier: 10);
        public static Renderable Renderable => new Renderable('=', Color.Cyan, Color.Black, zOrder: 10);
    }
}
