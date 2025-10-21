using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using Microsoft.Extensions.Logging;
using SadRogue.Primitives;

namespace LablabBean.Game.Core.Systems;

/// <summary>
/// System that manages inventory operations: pickup, use, equip, drop
/// </summary>
public class InventorySystem
{
    private readonly ILogger<InventorySystem> _logger;

    public InventorySystem(ILogger<InventorySystem> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all items that the player can pick up (within 1 tile)
    /// </summary>
    public List<Entity> GetPickupableItems(World world, Entity playerEntity)
    {
        var pickupableItems = new List<Entity>();

        if (!world.Has<Position>(playerEntity))
            return pickupableItems;

        var playerPos = world.Get<Position>(playerEntity);

        // Query all items on the ground (have Position component)
        var query = new QueryDescription().WithAll<Item, Position, Renderable>();

        world.Query(in query, (Entity entity, ref Item item, ref Position pos, ref Renderable renderable) =>
        {
            // Check if item is adjacent or on same tile (within 1 tile)
            var distance = Math.Abs(pos.X - playerPos.X) + Math.Abs(pos.Y - playerPos.Y);
            if (distance <= 1)
            {
                pickupableItems.Add(entity);
            }
        });

        return pickupableItems;
    }

    /// <summary>
    /// Checks if the player can pick up an item
    /// </summary>
    public bool CanPickup(World world, Entity playerEntity, Entity itemEntity)
    {
        // Check if player has inventory
        if (!world.Has<Inventory>(playerEntity))
        {
            _logger.LogWarning("Player entity {PlayerId} has no Inventory component", playerEntity.Id);
            return false;
        }

        // Check if inventory is full
        var inventory = world.Get<Inventory>(playerEntity);
        if (inventory.IsFull)
        {
            _logger.LogDebug("Player inventory is full ({Count}/{Max})", inventory.CurrentCount, inventory.MaxCapacity);
            return false;
        }

        // Check if item exists and has Item component
        if (!world.IsAlive(itemEntity) || !world.Has<Item>(itemEntity))
        {
            _logger.LogWarning("Item entity {ItemId} is not valid or has no Item component", itemEntity.Id);
            return false;
        }

        // Check if item has Position (is on ground)
        if (!world.Has<Position>(itemEntity))
        {
            _logger.LogDebug("Item entity {ItemId} has no Position component (not on ground)", itemEntity.Id);
            return false;
        }

        // Check distance
        if (!world.Has<Position>(playerEntity))
            return false;

        var playerPos = world.Get<Position>(playerEntity);
        var itemPos = world.Get<Position>(itemEntity);
        var distance = Math.Abs(itemPos.X - playerPos.X) + Math.Abs(itemPos.Y - playerPos.Y);

        if (distance > 1)
        {
            _logger.LogDebug("Item too far away. Distance: {Distance}", distance);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Picks up an item and adds it to the player's inventory
    /// Returns a message describing the result
    /// </summary>
    public string PickupItem(World world, Entity playerEntity, Entity itemEntity)
    {
        if (!CanPickup(world, playerEntity, itemEntity))
        {
            var inventory = world.Get<Inventory>(playerEntity);
            if (inventory.IsFull)
                return "Inventory is full!";
            return "Cannot pick up that item.";
        }

        var item = world.Get<Item>(itemEntity);
        var playerInventory = world.Get<Inventory>(playerEntity);

        // Check if item is stackable and we already have some
        if (world.Has<Stackable>(itemEntity))
        {
            var stackable = world.Get<Stackable>(itemEntity);

            // Look for existing stack of same item
            // For now, we'll skip stacking and just add as separate item
            // TODO: Implement proper stacking in future iteration
        }

        // Remove position and rendering components (item is no longer on ground)
        world.Remove<Position>(itemEntity);
        world.Remove<Renderable>(itemEntity);
        world.Remove<Visible>(itemEntity);

        // Add to inventory
        playerInventory.AddItem(itemEntity.Id);
        world.Set(playerEntity, playerInventory);

        var count = world.Has<Stackable>(itemEntity) ? world.Get<Stackable>(itemEntity).Count : 1;
        var countStr = count > 1 ? $" x{count}" : "";

        _logger.LogInformation("Picked up {ItemName}{Count}", item.Name, countStr);
        return $"Picked up {item.Name}{countStr}";
    }

    /// <summary>
    /// Gets all items in the player's inventory
    /// Returns list of (Entity, Item, optional Stackable count, equipped status)
    /// </summary>
    public List<(Entity ItemEntity, Item Item, int Count, bool IsEquipped)> GetInventoryItems(World world, Entity playerEntity)
    {
        var inventoryItems = new List<(Entity, Item, int, bool)>();

        if (!world.Has<Inventory>(playerEntity))
            return inventoryItems;

        var inventory = world.Get<Inventory>(playerEntity);

        foreach (var itemId in inventory.Items)
        {
            // Note: In Arch, we store entity IDs as ints, but need Entity struct for queries
            // We'll query all items and match by ID since we can't reconstruct Entity from just ID
            var query = new QueryDescription().WithAll<Item>();
            var found = false;
            
            world.Query(in query, (Entity entity, ref Item item) =>
            {
                if (entity.Id == itemId)
                {
                    var count = world.Has<Stackable>(entity) ? world.Get<Stackable>(entity).Count : 1;
                    var equipped = IsEquipped(world, playerEntity, entity);
                    inventoryItems.Add((entity, item, count, equipped));
                    found = true;
                }
            });
            
            if (!found)
            {
                _logger.LogWarning("Item entity {ItemId} in inventory not found in world", itemId);
            }
        }

        return inventoryItems;
    }

    /// <summary>
    /// Checks if an item is currently equipped
    /// </summary>
    public bool IsEquipped(World world, Entity playerEntity, Entity itemEntity)
    {
        if (!world.Has<EquipmentSlots>(playerEntity))
            return false;

        var equipment = world.Get<EquipmentSlots>(playerEntity);

        // Check if this item entity ID is in any equipment slot
        foreach (var slot in equipment.Slots.Values)
        {
            if (slot == itemEntity.Id)
                return true;
        }

        return false;
    }
}
