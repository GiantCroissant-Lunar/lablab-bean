using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Inventory;

/// <summary>
/// Implementation of inventory service - encapsulates all inventory logic
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly ILogger _logger;

    public InventoryService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public List<ItemInfo> GetPickupableItems(World world, Entity playerEntity)
    {
        var pickupableItems = new List<ItemInfo>();

        if (!world.Has<Position>(playerEntity))
            return pickupableItems;

        var playerPos = world.Get<Position>(playerEntity);
        var query = new QueryDescription().WithAll<Item, Position, Renderable>();

        world.Query(in query, (Entity entity, ref Item item, ref Position pos, ref Renderable renderable) =>
        {
            var distance = Math.Abs(pos.X - playerPos.X) + Math.Abs(pos.Y - playerPos.Y);
            if (distance <= 1)
            {
                pickupableItems.Add(new ItemInfo(entity.Id, item.Name, item.Glyph, item.Description, item.Type));
            }
        });

        return pickupableItems;
    }

    public InventoryResult PickupItem(World world, Entity playerEntity, Entity itemEntity)
    {
        if (!CanPickup(world, playerEntity, itemEntity, out var reason))
        {
            return new InventoryResult(false, reason);
        }

        var item = world.Get<Item>(itemEntity);
        var playerInventory = world.Get<Game.Core.Components.Inventory>(playerEntity);

        // Remove position and rendering components (item no longer on ground)
        world.Remove<Position>(itemEntity);
        world.Remove<Renderable>(itemEntity);
        world.Remove<Visible>(itemEntity);

        // Add to inventory
        playerInventory.AddItem(itemEntity.Id);
        world.Set(playerEntity, playerInventory);

        var count = world.Has<Stackable>(itemEntity) ? world.Get<Stackable>(itemEntity).Count : 1;
        var countStr = count > 1 ? $" x{count}" : "";
        var message = $"Picked up {item.Name}{countStr}";

        _logger.LogInformation("Picked up {ItemName}{Count}", item.Name, countStr);
        return new InventoryResult(true, message);
    }

    public List<InventoryItemInfo> GetInventoryItems(World world, Entity playerEntity)
    {
        var inventoryItems = new List<InventoryItemInfo>();

        if (!world.Has<Game.Core.Components.Inventory>(playerEntity))
            return inventoryItems;

        var inventory = world.Get<Game.Core.Components.Inventory>(playerEntity);
        var query = new QueryDescription().WithAll<Item>();

        foreach (var itemId in inventory.Items)
        {
            world.Query(in query, (Entity entity, ref Item item) =>
            {
                if (entity.Id == itemId)
                {
                    var count = world.Has<Stackable>(entity) ? world.Get<Stackable>(entity).Count : 1;
                    var equipped = IsEquipped(world, playerEntity, entity);
                    inventoryItems.Add(new InventoryItemInfo(entity.Id, item.Name, item.Glyph, item.Type, count, equipped));
                }
            });
        }

        return inventoryItems;
    }

    public List<ConsumableItemInfo> GetConsumables(World world, Entity playerEntity)
    {
        var consumables = new List<ConsumableItemInfo>();

        if (!world.Has<Game.Core.Components.Inventory>(playerEntity))
            return consumables;

        var inventory = world.Get<Game.Core.Components.Inventory>(playerEntity);
        var query = new QueryDescription().WithAll<Item, Consumable>();

        foreach (var itemId in inventory.Items)
        {
            world.Query(in query, (Entity entity, ref Item item, ref Consumable consumable) =>
            {
                if (entity.Id == itemId)
                {
                    var count = world.Has<Stackable>(entity) ? world.Get<Stackable>(entity).Count : 1;
                    consumables.Add(new ConsumableItemInfo(entity.Id, item.Name, item.Glyph, consumable.Effect, consumable.EffectValue, count));
                }
            });
        }

        return consumables;
    }

    public InventoryResult UseConsumable(World world, Entity playerEntity, Entity itemEntity, object? statusEffectSystem = null)
    {
        if (!CanUseConsumable(world, playerEntity, itemEntity, out var reason))
        {
            return new InventoryResult(false, reason);
        }

        var item = world.Get<Item>(itemEntity);
        var consumable = world.Get<Consumable>(itemEntity);
        string message;

        // Handle status effect system integration if provided
        if (statusEffectSystem != null)
        {
            message = ApplyConsumableWithStatusEffects(world, playerEntity, itemEntity, consumable, statusEffectSystem);
        }
        else
        {
            message = ApplyTraditionalConsumable(world, playerEntity, consumable, item.Name);
        }

        // Handle stackable items
        if (world.Has<Stackable>(itemEntity))
        {
            var stackable = world.Get<Stackable>(itemEntity);
            stackable.Count--;

            if (stackable.Count <= 0)
            {
                RemoveItemFromInventory(world, playerEntity, itemEntity);
                world.Destroy(itemEntity);
            }
            else
            {
                world.Set(itemEntity, stackable);
            }
        }
        else
        {
            RemoveItemFromInventory(world, playerEntity, itemEntity);
            world.Destroy(itemEntity);
        }

        _logger.LogInformation("Player used {ItemName}: {Message}", item.Name, message);
        return new InventoryResult(true, message);
    }

    public List<EquippableItemInfo> GetEquippables(World world, Entity playerEntity)
    {
        var equippables = new List<EquippableItemInfo>();

        if (!world.Has<Game.Core.Components.Inventory>(playerEntity))
            return equippables;

        var inventory = world.Get<Game.Core.Components.Inventory>(playerEntity);
        var query = new QueryDescription().WithAll<Item, Equippable>();

        foreach (var itemId in inventory.Items)
        {
            world.Query(in query, (Entity entity, ref Item item, ref Equippable equippable) =>
            {
                if (entity.Id == itemId)
                {
                    equippables.Add(new EquippableItemInfo(
                        entity.Id, item.Name, item.Glyph, equippable.Slot,
                        equippable.AttackBonus, equippable.DefenseBonus, equippable.SpeedModifier));
                }
            });
        }

        return equippables;
    }

    public EquipResult EquipItem(World world, Entity playerEntity, Entity itemEntity)
    {
        if (!CanEquip(world, playerEntity, itemEntity, out var reason))
        {
            return new EquipResult(false, reason, 0, 0, 0);
        }

        var item = world.Get<Item>(itemEntity);
        var equippable = world.Get<Equippable>(itemEntity);
        var equipment = world.Get<EquipmentSlots>(playerEntity);

        // Unequip old item in the same slot if exists
        if (equipment.Slots[equippable.Slot].HasValue)
        {
            UnequipItemInternal(world, playerEntity, equippable.Slot);
        }

        // Equip new item
        equipment.Slots[equippable.Slot] = itemEntity.Id;
        world.Set(playerEntity, equipment);

        // Update player stats
        UpdatePlayerStats(world, playerEntity);

        var statChanges = BuildStatChangeMessage(equippable);
        var message = statChanges.Length > 0
            ? $"Equipped {item.Name}. {statChanges}"
            : $"Equipped {item.Name}.";

        _logger.LogInformation("Equipped {ItemName} in {Slot}", item.Name, equippable.Slot);
        return new EquipResult(true, message, equippable.AttackBonus, equippable.DefenseBonus, equippable.SpeedModifier);
    }

    public InventoryResult UnequipItem(World world, Entity playerEntity, EquipmentSlot slot)
    {
        if (!world.Has<EquipmentSlots>(playerEntity))
        {
            return new InventoryResult(false, "No equipment slots.");
        }

        var equipment = world.Get<EquipmentSlots>(playerEntity);

        if (!equipment.Slots[slot].HasValue)
        {
            return new InventoryResult(false, $"No item equipped in {slot} slot.");
        }

        var itemName = UnequipItemInternal(world, playerEntity, slot);
        UpdatePlayerStats(world, playerEntity);

        var message = $"Unequipped {itemName}.";
        _logger.LogInformation("Unequipped {ItemName} from {Slot}", itemName, slot);
        return new InventoryResult(true, message);
    }

    public (int Attack, int Defense, int Speed) CalculateTotalStats(World world, Entity playerEntity)
    {
        int baseAttack = 10;
        int baseDefense = 5;
        int baseSpeed = 100;

        int totalAttackBonus = 0;
        int totalDefenseBonus = 0;
        int totalSpeedModifier = 0;

        if (world.Has<EquipmentSlots>(playerEntity))
        {
            var equipment = world.Get<EquipmentSlots>(playerEntity);
            var query = new QueryDescription().WithAll<Equippable>();

            foreach (var slotValue in equipment.Slots.Values)
            {
                if (!slotValue.HasValue)
                    continue;

                var itemId = slotValue.Value;
                world.Query(in query, (Entity entity, ref Equippable equippable) =>
                {
                    if (entity.Id == itemId)
                    {
                        totalAttackBonus += equippable.AttackBonus;
                        totalDefenseBonus += equippable.DefenseBonus;
                        totalSpeedModifier += equippable.SpeedModifier;
                    }
                });
            }
        }

        return (baseAttack + totalAttackBonus, baseDefense + totalDefenseBonus, baseSpeed + totalSpeedModifier);
    }

    // Private helper methods

    private bool CanPickup(World world, Entity playerEntity, Entity itemEntity, out string reason)
    {
        if (!world.Has<Game.Core.Components.Inventory>(playerEntity))
        {
            reason = "No inventory.";
            return false;
        }

        var inventory = world.Get<Game.Core.Components.Inventory>(playerEntity);
        if (inventory.IsFull)
        {
            reason = "Inventory is full!";
            return false;
        }

        if (!world.IsAlive(itemEntity) || !world.Has<Item>(itemEntity))
        {
            reason = "Invalid item.";
            return false;
        }

        if (!world.Has<Position>(itemEntity) || !world.Has<Position>(playerEntity))
        {
            reason = "Item not accessible.";
            return false;
        }

        var playerPos = world.Get<Position>(playerEntity);
        var itemPos = world.Get<Position>(itemEntity);
        var distance = Math.Abs(itemPos.X - playerPos.X) + Math.Abs(itemPos.Y - playerPos.Y);

        if (distance > 1)
        {
            reason = "Too far away.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private bool CanUseConsumable(World world, Entity playerEntity, Entity itemEntity, out string reason)
    {
        if (!world.Has<Game.Core.Components.Inventory>(playerEntity))
        {
            reason = "No inventory.";
            return false;
        }

        var inventory = world.Get<Game.Core.Components.Inventory>(playerEntity);
        if (!inventory.Items.Contains(itemEntity.Id))
        {
            reason = "Item not in inventory.";
            return false;
        }

        if (!world.Has<Consumable>(itemEntity))
        {
            reason = "Item is not consumable.";
            return false;
        }

        var consumable = world.Get<Consumable>(itemEntity);
        if (consumable.Effect == ConsumableEffect.RestoreHealth)
        {
            if (!world.Has<Health>(playerEntity))
            {
                reason = "Cannot use.";
                return false;
            }

            var health = world.Get<Health>(playerEntity);
            if (health.Current >= health.Maximum)
            {
                reason = "Already at full health!";
                return false;
            }
        }

        reason = string.Empty;
        return true;
    }

    private bool CanEquip(World world, Entity playerEntity, Entity itemEntity, out string reason)
    {
        if (!world.Has<Game.Core.Components.Inventory>(playerEntity) || !world.Has<EquipmentSlots>(playerEntity))
        {
            reason = "Cannot equip items.";
            return false;
        }

        var inventory = world.Get<Game.Core.Components.Inventory>(playerEntity);
        if (!inventory.Items.Contains(itemEntity.Id))
        {
            reason = "Item not in inventory.";
            return false;
        }

        if (!world.Has<Equippable>(itemEntity))
        {
            reason = "Item is not equippable.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private bool IsEquipped(World world, Entity playerEntity, Entity itemEntity)
    {
        if (!world.Has<EquipmentSlots>(playerEntity))
            return false;

        var equipment = world.Get<EquipmentSlots>(playerEntity);
        return equipment.Slots.Values.Any(slot => slot == itemEntity.Id);
    }

    private void RemoveItemFromInventory(World world, Entity playerEntity, Entity itemEntity)
    {
        if (!world.Has<Game.Core.Components.Inventory>(playerEntity))
            return;

        var inventory = world.Get<Game.Core.Components.Inventory>(playerEntity);
        inventory.Items.Remove(itemEntity.Id);
        world.Set(playerEntity, inventory);
    }

    private string ApplyConsumableWithStatusEffects(World world, Entity playerEntity, Entity itemEntity, Consumable consumable, object statusEffectSystem)
    {
        // Integration with status effect system (dynamic invocation to avoid hard dependency)
        var systemType = statusEffectSystem.GetType();

        if (consumable.AppliesEffect.HasValue)
        {
            var method = systemType.GetMethod("ApplyEffect");
            if (method != null)
            {
                var result = method.Invoke(statusEffectSystem, new object?[]
                {
                    world,
                    playerEntity,
                    consumable.AppliesEffect.Value,
                    consumable.EffectMagnitude ?? 10,
                    consumable.EffectDuration ?? 5,
                    EffectSource.Consumable
                });

                if (result != null)
                {
                    var messageProperty = result.GetType().GetProperty("Message");
                    return messageProperty?.GetValue(result)?.ToString() ?? "Effect applied.";
                }
            }
        }
        else if (consumable.RemovesEffect.HasValue)
        {
            var method = systemType.GetMethod("RemoveEffect");
            if (method != null)
            {
                var result = method.Invoke(statusEffectSystem, new object[] { world, playerEntity, consumable.RemovesEffect.Value });
                if (result != null)
                {
                    var messageProperty = result.GetType().GetProperty("Message");
                    return messageProperty?.GetValue(result)?.ToString() ?? "Effect removed.";
                }
            }
        }
        else if (consumable.RemovesAllNegativeEffects)
        {
            var method = systemType.GetMethod("RemoveAllNegativeEffects");
            if (method != null)
            {
                var result = method.Invoke(statusEffectSystem, new object[] { world, playerEntity });
                if (result != null)
                {
                    var messageProperty = result.GetType().GetProperty("Message");
                    return messageProperty?.GetValue(result)?.ToString() ?? "Negative effects removed.";
                }
            }
        }

        return "Consumed.";
    }

    private string ApplyTraditionalConsumable(World world, Entity playerEntity, Consumable consumable, string itemName)
    {
        return consumable.Effect switch
        {
            ConsumableEffect.RestoreHealth => ApplyHealingEffect(world, playerEntity, consumable.EffectValue, itemName),
            ConsumableEffect.RestoreMana => $"You consume the {itemName}. (Mana system not yet implemented)",
            ConsumableEffect.IncreaseSpeed => $"You consume the {itemName}. (Speed buff not yet implemented)",
            ConsumableEffect.CurePoison => $"You consume the {itemName}. (Poison system not yet implemented)",
            _ => $"You consume the {itemName}."
        };
    }

    private string ApplyHealingEffect(World world, Entity playerEntity, int healAmount, string itemName)
    {
        if (!world.Has<Health>(playerEntity))
            return $"You consume the {itemName}, but nothing happens.";

        var health = world.Get<Health>(playerEntity);
        int oldHealth = health.Current;

        health.Current = Math.Min(health.Current + healAmount, health.Maximum);
        world.Set(playerEntity, health);

        int actualHealing = health.Current - oldHealth;
        return $"You drink the {itemName} and recover {actualHealing} HP.";
    }

    private void UpdatePlayerStats(World world, Entity playerEntity)
    {
        var (newAttack, newDefense, newSpeed) = CalculateTotalStats(world, playerEntity);

        if (world.Has<Combat>(playerEntity))
        {
            var combat = world.Get<Combat>(playerEntity);
            combat.Attack = newAttack;
            combat.Defense = newDefense;
            world.Set(playerEntity, combat);
        }

        if (world.Has<Actor>(playerEntity))
        {
            var actor = world.Get<Actor>(playerEntity);
            actor.Speed = newSpeed;
            world.Set(playerEntity, actor);
        }
    }

    private string UnequipItemInternal(World world, Entity playerEntity, EquipmentSlot slot)
    {
        var equipment = world.Get<EquipmentSlots>(playerEntity);
        var itemId = equipment.Slots[slot]!.Value;
        string itemName = "Unknown";

        var query = new QueryDescription().WithAll<Item>();
        world.Query(in query, (Entity entity, ref Item item) =>
        {
            if (entity.Id == itemId)
            {
                itemName = item.Name;
            }
        });

        equipment.Slots[slot] = null;
        world.Set(playerEntity, equipment);

        return itemName;
    }

    private string BuildStatChangeMessage(Equippable equippable)
    {
        var statChanges = new List<string>();
        if (equippable.AttackBonus != 0)
            statChanges.Add($"ATK {(equippable.AttackBonus > 0 ? "+" : "")}{equippable.AttackBonus}");
        if (equippable.DefenseBonus != 0)
            statChanges.Add($"DEF {(equippable.DefenseBonus > 0 ? "+" : "")}{equippable.DefenseBonus}");
        if (equippable.SpeedModifier != 0)
            statChanges.Add($"SPD {(equippable.SpeedModifier > 0 ? "+" : "")}{equippable.SpeedModifier}");

        return string.Join(", ", statChanges);
    }
}
