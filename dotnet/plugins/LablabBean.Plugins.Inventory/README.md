# Inventory System Plugin

Complete inventory management plugin for the Lablab Bean dungeon crawler.

## Overview

This plugin encapsulates all inventory-related functionality behind a clean service boundary, following the tiered plugin architecture (Spec 004). It was migrated from the core game system (Spec 001) to demonstrate the plugin architecture.

## Features

### Item Management

- **Pickup Items**: Pick up items from the game world
- **Inventory Storage**: Manage items in player inventory (max capacity: 20)
- **Item Information**: Query pickupable items, inventory contents

### Consumables

- **Use Items**: Consume potions and other usable items
- **Health Restoration**: Heal with health potions
- **Stack Management**: Automatic handling of stackable items
- **Status Effects**: Integration with status effect system

### Equipment

- **Equip Items**: Equip weapons, armor, and accessories
- **Equipment Slots**: 9 equipment slots (weapon, off-hand, armor pieces, accessories)
- **Stat Bonuses**: Automatic stat calculation from equipped items
- **Unequip**: Remove equipment from slots

## Public API

### IInventoryService

The main service interface exposed to the host and other plugins:

```csharp
public interface IInventoryService
{
    // Pickup
    List<ItemInfo> GetPickupableItems(World world, Entity playerEntity);
    InventoryResult PickupItem(World world, Entity playerEntity, Entity itemEntity);

    // Inventory
    List<InventoryItemInfo> GetInventoryItems(World world, Entity playerEntity);

    // Consumables
    List<ConsumableItemInfo> GetConsumables(World world, Entity playerEntity);
    InventoryResult UseConsumable(World world, Entity playerEntity, Entity itemEntity, object? statusEffectSystem = null);

    // Equipment
    List<EquippableItemInfo> GetEquippables(World world, Entity playerEntity);
    EquipResult EquipItem(World world, Entity playerEntity, Entity itemEntity);
    InventoryResult UnequipItem(World world, Entity playerEntity, EquipmentSlot slot);

    // Stats
    (int Attack, int Defense, int Speed) CalculateTotalStats(World world, Entity playerEntity);
}
```

### Read Models

Clean data transfer objects for host consumption:

- **ItemInfo**: Basic item information
- **InventoryItemInfo**: Inventory item with count and equipped status
- **ConsumableItemInfo**: Consumable with effect details
- **EquippableItemInfo**: Equipment with stat bonuses
- **InventoryResult**: Operation result with success flag and message
- **EquipResult**: Equip result with stat changes

## Events

The plugin publishes events for UI updates:

- **Inventory.ItemPickedUp**: When an item is picked up
- **Inventory.ItemUsed**: When a consumable is used
- **Inventory.ItemEquipped**: When an item is equipped
- **Inventory.ItemUnequipped**: When an item is unequipped
- **Inventory.Changed**: When inventory state changes

## Usage

### Host Integration

```csharp
// Get the service from DI (after plugin loads)
var inventoryService = serviceProvider.GetRequiredService<IInventoryService>();

// Pickup an item
var result = inventoryService.PickupItem(world, playerEntity, itemEntity);
if (result.Success)
{
    Console.WriteLine(result.Message); // "Picked up Health Potion"
}

// Get all consumables
var consumables = inventoryService.GetConsumables(world, playerEntity);
foreach (var consumable in consumables)
{
    Console.WriteLine($"{consumable.Name} x{consumable.Count}");
}

// Use a consumable
var useResult = inventoryService.UseConsumable(world, playerEntity, potionEntity, statusEffectSystem);
Console.WriteLine(useResult.Message); // "You drink the Health Potion and recover 20 HP."

// Equip an item
var equipResult = inventoryService.EquipItem(world, playerEntity, swordEntity);
Console.WriteLine(equipResult.Message); // "Equipped Iron Sword. ATK +5"
```

### Event Subscription

```csharp
// Subscribe to inventory events (via IPluginHost)
host.SubscribeToEvent(InventoryEvents.ItemPickedUp, (data) =>
{
    var evt = (ItemPickedUpEvent)data;
    UpdateInventoryUI(evt);
});
```

## Dependencies

- **Arch.Extended**: ECS framework
- **LablabBean.Game.Core**: Core game components
- **LablabBean.Plugins.Contracts**: Plugin system contracts

## Components Used

The plugin operates on these ECS components:

- **Item**: Base item data (name, glyph, type)
- **Inventory**: Player inventory storage
- **Consumable**: Consumable item effects
- **Equippable**: Equipment stats and bonuses
- **Stackable**: Stack count for stackable items
- **EquipmentSlots**: Equipment slot assignments
- **Health**: Health component for healing
- **Combat**: Combat stats (attack, defense)
- **Actor**: Actor stats (speed)

## Security

This plugin uses the **Standard** permission profile:

- ✅ File system read
- ✅ Service registration
- ✅ Event publishing
- ❌ File system write
- ❌ Network access
- ❌ Process creation

## Testing

See `specs/005-inventory-plugin-migration/tasks.md` for validation checklist against Spec 001 acceptance criteria.

## Migration Notes

This plugin was migrated from `LablabBean.Game.Core.Systems.InventorySystem` following Spec 005:

1. ✅ Extracted inventory logic into plugin boundary
2. ✅ Created public IInventoryService interface
3. ✅ Implemented read models for host consumption
4. ✅ Added event notifications for UI integration
5. ✅ Maintained backward compatibility with existing components
6. ✅ Zero breaking changes to data model

## Version History

- **1.0.0**: Initial plugin migration from core systems

## License

[Project License]
