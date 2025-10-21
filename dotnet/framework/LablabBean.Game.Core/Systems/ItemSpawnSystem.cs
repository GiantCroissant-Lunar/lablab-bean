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
