// Contract: InventorySystem
// Purpose: Defines the interface for inventory management operations
// Location: LablabBean.Game.Core/Systems/InventorySystem.cs

namespace LablabBean.Game.Core.Systems;

using Arch.Core;
using LablabBean.Game.Core.Components;

/// <summary>
/// System responsible for managing player inventory operations.
/// Handles pickup, drop, use, equip, and unequip actions.
/// </summary>
public class InventorySystem
{
    private readonly World _world;

    public InventorySystem(World world)
    {
        _world = world;
    }

    // ═══════════════════════════════════════════════════════════════
    // PICKUP OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to pick up an item from the ground.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="itemEntity">The item entity to pick up</param>
    /// <returns>Result indicating success or failure reason</returns>
    /// <remarks>
    /// Preconditions:
    /// - Item must have Position component (be on ground)
    /// - Item must be adjacent to player (within 1 tile)
    /// - Player inventory must not be full
    /// - Item must be stackable OR inventory has space
    ///
    /// Effects:
    /// - Removes Position, Renderable, Visible from item
    /// - Adds item reference to player's Inventory.Items
    /// - If stackable and matching item exists, merges stacks
    /// - Returns feedback message
    /// </remarks>
    public InventoryResult PickupItem(Entity playerEntity, Entity itemEntity);

    /// <summary>
    /// Gets all items adjacent to the player that can be picked up.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <returns>List of item entities within pickup range</returns>
    public List<Entity> GetPickupableItems(Entity playerEntity);

    /// <summary>
    /// Checks if player can pick up the specified item.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="itemEntity">The item to check</param>
    /// <returns>True if pickup is possible, false otherwise</returns>
    public bool CanPickup(Entity playerEntity, Entity itemEntity);

    // ═══════════════════════════════════════════════════════════════
    // DROP OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Drops an item from inventory onto the ground at player's position.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="itemEntity">The item entity to drop</param>
    /// <returns>Result indicating success or failure reason</returns>
    /// <remarks>
    /// Preconditions:
    /// - Item must be in player's inventory
    /// - Item must not be equipped
    ///
    /// Effects:
    /// - Removes item reference from Inventory.Items
    /// - Adds Position (player's location), Renderable, Visible to item
    /// - Returns feedback message
    /// </remarks>
    public InventoryResult DropItem(Entity playerEntity, Entity itemEntity);

    /// <summary>
    /// Checks if player can drop the specified item.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="itemEntity">The item to check</param>
    /// <returns>True if drop is possible, false otherwise</returns>
    public bool CanDrop(Entity playerEntity, Entity itemEntity);

    // ═══════════════════════════════════════════════════════════════
    // USE OPERATIONS (CONSUMABLES)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Uses a consumable item from inventory.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="itemEntity">The consumable item to use</param>
    /// <returns>Result indicating success or failure reason</returns>
    /// <remarks>
    /// Preconditions:
    /// - Item must be in player's inventory
    /// - Item must have Consumable component
    /// - Usage context must be valid (e.g., not at full health for healing potion)
    ///
    /// Effects:
    /// - Applies consumable effect (heal, buff, etc.)
    /// - Decrements Stackable.Count if stackable
    /// - Destroys item entity if count reaches 0
    /// - Removes from Inventory.Items if destroyed
    /// - Returns feedback message with effect description
    /// </remarks>
    public InventoryResult UseConsumable(Entity playerEntity, Entity itemEntity);

    /// <summary>
    /// Checks if player can use the specified consumable.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="itemEntity">The consumable to check</param>
    /// <returns>True if usage is possible, false otherwise</returns>
    public bool CanUseConsumable(Entity playerEntity, Entity itemEntity);

    // ═══════════════════════════════════════════════════════════════
    // EQUIP OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Equips an item from inventory to an equipment slot.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="itemEntity">The item to equip</param>
    /// <returns>Result indicating success or failure reason</returns>
    /// <remarks>
    /// Preconditions:
    /// - Item must be in player's inventory
    /// - Item must have Equippable component
    /// - Target slot must be compatible with item
    ///
    /// Effects:
    /// - If slot occupied, unequips old item (stays in inventory)
    /// - Sets EquipmentSlots[slot] = itemEntity
    /// - Recalculates player stats (attack, defense, speed)
    /// - Returns feedback message with stat changes
    /// </remarks>
    public InventoryResult EquipItem(Entity playerEntity, Entity itemEntity);

    /// <summary>
    /// Unequips an item from an equipment slot.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="slot">The equipment slot to unequip</param>
    /// <returns>Result indicating success or failure reason</returns>
    /// <remarks>
    /// Preconditions:
    /// - Slot must have an equipped item
    ///
    /// Effects:
    /// - Sets EquipmentSlots[slot] = null
    /// - Item remains in inventory
    /// - Recalculates player stats
    /// - Returns feedback message with stat changes
    /// </remarks>
    public InventoryResult UnequipItem(Entity playerEntity, EquipmentSlot slot);

    /// <summary>
    /// Checks if player can equip the specified item.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="itemEntity">The item to check</param>
    /// <returns>True if equip is possible, false otherwise</returns>
    public bool CanEquip(Entity playerEntity, Entity itemEntity);

    // ═══════════════════════════════════════════════════════════════
    // QUERY OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all items currently in player's inventory.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <returns>List of item entities in inventory</returns>
    public List<Entity> GetInventoryItems(Entity playerEntity);

    /// <summary>
    /// Gets the item equipped in a specific slot.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="slot">The equipment slot to query</param>
    /// <returns>Equipped item entity, or null if slot is empty</returns>
    public Entity? GetEquippedItem(Entity playerEntity, EquipmentSlot slot);

    /// <summary>
    /// Checks if an item is currently equipped.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="itemEntity">The item to check</param>
    /// <returns>True if item is equipped, false otherwise</returns>
    public bool IsEquipped(Entity playerEntity, Entity itemEntity);

    /// <summary>
    /// Gets all consumable items in inventory.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <returns>List of consumable item entities</returns>
    public List<Entity> GetConsumables(Entity playerEntity);

    /// <summary>
    /// Gets all equippable items in inventory.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <returns>List of equippable item entities</returns>
    public List<Entity> GetEquippables(Entity playerEntity);

    // ═══════════════════════════════════════════════════════════════
    // STAT CALCULATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates total combat stats including equipment bonuses.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <returns>Tuple of (attack, defense, speed) with equipment bonuses applied</returns>
    /// <remarks>
    /// Reads base stats from Combat component and adds bonuses from all equipped items.
    /// Used by CombatSystem to calculate damage and by HUD to display stats.
    /// </remarks>
    public (int attack, int defense, int speed) CalculateTotalStats(Entity playerEntity);
}

/// <summary>
/// Result of an inventory operation.
/// </summary>
public struct InventoryResult
{
    /// <summary>Whether the operation succeeded</summary>
    public bool Success { get; set; }

    /// <summary>Feedback message to display to player</summary>
    public string Message { get; set; }

    /// <summary>Optional stat changes (for equipment)</summary>
    public StatChanges? StatChanges { get; set; }

    public static InventoryResult Succeeded(string message, StatChanges? statChanges = null)
        => new() { Success = true, Message = message, StatChanges = statChanges };

    public static InventoryResult Failed(string message)
        => new() { Success = false, Message = message };
}

/// <summary>
/// Stat changes from equipment operations.
/// </summary>
public struct StatChanges
{
    public int AttackChange { get; set; }
    public int DefenseChange { get; set; }
    public int SpeedChange { get; set; }

    public override string ToString()
    {
        var parts = new List<string>();
        if (AttackChange != 0) parts.Add($"ATK {(AttackChange > 0 ? "+" : "")}{AttackChange}");
        if (DefenseChange != 0) parts.Add($"DEF {(DefenseChange > 0 ? "+" : "")}{DefenseChange}");
        if (SpeedChange != 0) parts.Add($"SPD {(SpeedChange > 0 ? "+" : "")}{SpeedChange}");
        return string.Join(", ", parts);
    }
}
