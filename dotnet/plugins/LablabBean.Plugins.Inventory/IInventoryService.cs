using Arch.Core;
using LablabBean.Game.Core.Components;

namespace LablabBean.Plugins.Inventory;

/// <summary>
/// Public service interface for inventory operations.
/// Exposed to host and other plugins via DI registry.
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Gets all items that can be picked up near the player
    /// </summary>
    List<ItemInfo> GetPickupableItems(World world, Entity playerEntity);

    /// <summary>
    /// Attempts to pick up an item
    /// </summary>
    InventoryResult PickupItem(World world, Entity playerEntity, Entity itemEntity);

    /// <summary>
    /// Gets all items in player's inventory
    /// </summary>
    List<InventoryItemInfo> GetInventoryItems(World world, Entity playerEntity);

    /// <summary>
    /// Gets all consumable items in inventory
    /// </summary>
    List<ConsumableItemInfo> GetConsumables(World world, Entity playerEntity);

    /// <summary>
    /// Uses a consumable item
    /// </summary>
    InventoryResult UseConsumable(World world, Entity playerEntity, Entity itemEntity, object? statusEffectSystem = null);

    /// <summary>
    /// Gets all equippable items in inventory
    /// </summary>
    List<EquippableItemInfo> GetEquippables(World world, Entity playerEntity);

    /// <summary>
    /// Equips an item
    /// </summary>
    EquipResult EquipItem(World world, Entity playerEntity, Entity itemEntity);

    /// <summary>
    /// Unequips an item from a slot
    /// </summary>
    InventoryResult UnequipItem(World world, Entity playerEntity, EquipmentSlot slot);

    /// <summary>
    /// Calculates total stats from base stats + equipment
    /// </summary>
    (int Attack, int Defense, int Speed) CalculateTotalStats(World world, Entity playerEntity);
}

/// <summary>
/// Read model for item information
/// </summary>
public record ItemInfo(int EntityId, string Name, char Glyph, string Description, ItemType Type);

/// <summary>
/// Read model for inventory item information
/// </summary>
public record InventoryItemInfo(int EntityId, string Name, char Glyph, ItemType Type, int Count, bool IsEquipped);

/// <summary>
/// Read model for consumable item information
/// </summary>
public record ConsumableItemInfo(int EntityId, string Name, char Glyph, ConsumableEffect Effect, int EffectValue, int Count);

/// <summary>
/// Read model for equippable item information
/// </summary>
public record EquippableItemInfo(int EntityId, string Name, char Glyph, EquipmentSlot Slot, int AttackBonus, int DefenseBonus, int SpeedModifier);

/// <summary>
/// Result of an inventory operation
/// </summary>
public record InventoryResult(bool Success, string Message);

/// <summary>
/// Result of an equip operation with stat changes
/// </summary>
public record EquipResult(bool Success, string Message, int AttackChange, int DefenseChange, int SpeedChange);

/// <summary>
/// Events published by inventory plugin to host
/// </summary>
public static class InventoryEvents
{
    public const string ItemPickedUp = "Inventory.ItemPickedUp";
    public const string ItemUsed = "Inventory.ItemUsed";
    public const string ItemEquipped = "Inventory.ItemEquipped";
    public const string ItemUnequipped = "Inventory.ItemUnequipped";
    public const string InventoryChanged = "Inventory.Changed";
}

/// <summary>
/// Event data for inventory changes
/// </summary>
public record InventoryChangedEvent(int PlayerEntityId, int ItemCount, bool IsFull);

/// <summary>
/// Event data for item pickup
/// </summary>
public record ItemPickedUpEvent(int PlayerEntityId, string ItemName, int Count);

/// <summary>
/// Event data for item usage
/// </summary>
public record ItemUsedEvent(int PlayerEntityId, string ItemName, string Effect);

/// <summary>
/// Event data for item equip/unequip
/// </summary>
public record ItemEquippedEvent(int PlayerEntityId, string ItemName, EquipmentSlot Slot, int AttackChange, int DefenseChange, int SpeedChange);
