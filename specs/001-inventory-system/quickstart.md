# Quickstart Guide: Inventory System

**Feature**: Inventory System with Item Pickup and Usage
**Date**: 2025-10-21
**Audience**: Developers implementing or extending the inventory system

## Overview

This guide provides practical examples for working with the inventory system. All code examples assume you have access to the `World` instance and relevant entity references.

## Table of Contents

1. [Creating Items](#creating-items)
2. [Spawning Items](#spawning-items)
3. [Picking Up Items](#picking-up-items)
4. [Using Consumables](#using-consumables)
5. [Equipping Items](#equipping-items)
6. [Querying Inventory](#querying-inventory)
7. [Displaying Inventory in HUD](#displaying-inventory-in-hud)
8. [Testing Examples](#testing-examples)

---

## Creating Items

### Create a Healing Potion

```csharp
using LablabBean.Game.Core.Components;

// Create healing potion entity
var potionEntity = world.Create(
    new Item
    {
        Name = "Healing Potion",
        Glyph = '!',
        Description = "Restores 30 HP",
        Type = ItemType.Consumable,
        Weight = 1
    },
    new Consumable
    {
        Effect = ConsumableEffect.RestoreHealth,
        EffectValue = 30,
        UsableOutOfCombat = true
    },
    new Stackable
    {
        Count = 1,
        MaxStack = 99
    }
);
```

### Create a Weapon

```csharp
// Create iron sword entity
var swordEntity = world.Create(
    new Item
    {
        Name = "Iron Sword",
        Glyph = '/',
        Description = "+5 Attack",
        Type = ItemType.Weapon,
        Weight = 5
    },
    new Equippable
    {
        Slot = EquipmentSlot.MainHand,
        AttackBonus = 5,
        DefenseBonus = 0,
        SpeedModifier = 0,
        TwoHanded = false
    }
);
```

### Create Armor

```csharp
// Create leather armor entity
var armorEntity = world.Create(
    new Item
    {
        Name = "Leather Armor",
        Glyph = '[',
        Description = "+3 Defense",
        Type = ItemType.Armor,
        Weight = 10
    },
    new Equippable
    {
        Slot = EquipmentSlot.Chest,
        AttackBonus = 0,
        DefenseBonus = 3,
        SpeedModifier = 0,
        TwoHanded = false
    }
);
```

---

## Spawning Items

### Spawn Item on Ground

```csharp
using LablabBean.Game.Core.Systems;

var itemSpawnSystem = new ItemSpawnSystem(world);

// Spawn healing potion at position (10, 15)
var position = new Position { X = 10, Y = 15 };
var itemDef = ItemDefinitions.HealingPotion;
var spawnedItem = itemSpawnSystem.SpawnItem(itemDef, position);

// Item now has Position, Renderable, and Visible components
```

### Spawn Items in Rooms (During Map Generation)

```csharp
// In MapGenerator.cs, after creating rooms
var itemSpawnSystem = new ItemSpawnSystem(world);
itemSpawnSystem.SpawnItemsInRooms(dungeonMap, rooms);

// This will spawn items in 20-50% of rooms based on spawn tables
```

### Spawn Enemy Loot

```csharp
// In CombatSystem.cs, when enemy dies
var enemyPosition = world.Get<Position>(enemyEntity);
var enemyName = world.Get<Name>(enemyEntity);

itemSpawnSystem.SpawnEnemyLoot(enemyEntity, enemyPosition);

// 30% chance for potion, 10% chance for equipment
```

---

## Picking Up Items

### Basic Pickup

```csharp
var inventorySystem = new InventorySystem(world);

// Get items adjacent to player
var pickupableItems = inventorySystem.GetPickupableItems(playerEntity);

if (pickupableItems.Count > 0)
{
    var itemToPickup = pickupableItems[0];
    var result = inventorySystem.PickupItem(playerEntity, itemToPickup);

    if (result.Success)
    {
        Console.WriteLine(result.Message); // "Picked up Healing Potion"
    }
    else
    {
        Console.WriteLine(result.Message); // "Inventory full"
    }
}
```

### Pickup with Multiple Items on Same Tile

```csharp
var pickupableItems = inventorySystem.GetPickupableItems(playerEntity);

if (pickupableItems.Count > 1)
{
    // Show selection menu to player
    Console.WriteLine("Multiple items here:");
    for (int i = 0; i < pickupableItems.Count; i++)
    {
        var item = world.Get<Item>(pickupableItems[i]);
        Console.WriteLine($"{i + 1}. {item.Name}");
    }

    // Player selects item by number
    int selection = GetPlayerSelection();
    var selectedItem = pickupableItems[selection - 1];

    var result = inventorySystem.PickupItem(playerEntity, selectedItem);
    Console.WriteLine(result.Message);
}
```

### Check if Pickup is Possible

```csharp
if (inventorySystem.CanPickup(playerEntity, itemEntity))
{
    // Proceed with pickup
}
else
{
    // Show error message (inventory full, item too far, etc.)
}
```

---

## Using Consumables

### Use Healing Potion

```csharp
// Player presses 'U' key
var consumables = inventorySystem.GetConsumables(playerEntity);

if (consumables.Count > 0)
{
    var potion = consumables[0]; // First consumable
    var result = inventorySystem.UseConsumable(playerEntity, potion);

    if (result.Success)
    {
        Console.WriteLine(result.Message);
        // "You drink the Healing Potion and recover 30 HP."
    }
    else
    {
        Console.WriteLine(result.Message);
        // "Already at full health"
    }
}
```

### Check if Consumable Can Be Used

```csharp
var health = world.Get<Health>(playerEntity);
var consumable = world.Get<Consumable>(potionEntity);

if (consumable.Effect == ConsumableEffect.RestoreHealth)
{
    if (health.Current >= health.Maximum)
    {
        Console.WriteLine("Already at full health");
        return;
    }
}

// Proceed with usage
var result = inventorySystem.UseConsumable(playerEntity, potionEntity);
```

---

## Equipping Items

### Equip a Weapon

```csharp
var equippables = inventorySystem.GetEquippables(playerEntity);

// Find iron sword in inventory
var sword = equippables.FirstOrDefault(e =>
    world.Get<Item>(e).Name == "Iron Sword");

if (sword != default)
{
    var result = inventorySystem.EquipItem(playerEntity, sword);

    if (result.Success)
    {
        Console.WriteLine(result.Message);
        // "Equipped Iron Sword. ATK +5"

        if (result.StatChanges.HasValue)
        {
            Console.WriteLine(result.StatChanges.Value.ToString());
            // "ATK +5"
        }
    }
}
```

### Unequip an Item

```csharp
var result = inventorySystem.UnequipItem(playerEntity, EquipmentSlot.MainHand);

if (result.Success)
{
    Console.WriteLine(result.Message);
    // "Unequipped Iron Sword. ATK -5"
}
```

### Check What's Equipped

```csharp
var equippedWeapon = inventorySystem.GetEquippedItem(
    playerEntity,
    EquipmentSlot.MainHand
);

if (equippedWeapon.HasValue)
{
    var item = world.Get<Item>(equippedWeapon.Value);
    Console.WriteLine($"Wielding: {item.Name}");
}
else
{
    Console.WriteLine("No weapon equipped");
}
```

---

## Querying Inventory

### Get All Inventory Items

```csharp
var inventoryItems = inventorySystem.GetInventoryItems(playerEntity);

Console.WriteLine($"Inventory ({inventoryItems.Count}/20):");
foreach (var itemEntity in inventoryItems)
{
    var item = world.Get<Item>(itemEntity);

    // Check if stackable
    if (world.Has<Stackable>(itemEntity))
    {
        var stackable = world.Get<Stackable>(itemEntity);
        Console.WriteLine($"- {item.Name} ({stackable.Count})");
    }
    else
    {
        Console.WriteLine($"- {item.Name}");
    }

    // Check if equipped
    if (inventorySystem.IsEquipped(playerEntity, itemEntity))
    {
        Console.WriteLine("  (equipped)");
    }
}
```

### Filter by Item Type

```csharp
var consumables = inventorySystem.GetConsumables(playerEntity);
var equippables = inventorySystem.GetEquippables(playerEntity);

Console.WriteLine($"Consumables: {consumables.Count}");
Console.WriteLine($"Equipment: {equippables.Count}");
```

### Calculate Total Stats

```csharp
var (attack, defense, speed) = inventorySystem.CalculateTotalStats(playerEntity);

Console.WriteLine($"Attack: {attack}");
Console.WriteLine($"Defense: {defense}");
Console.WriteLine($"Speed: {speed}");
```

---

## Displaying Inventory in HUD

### Extend HudService (Terminal.Gui)

```csharp
// In HudService.cs
using Terminal.Gui;

public class HudService
{
    private FrameView _inventoryFrame;
    private ListView _inventoryList;

    public void CreateInventoryPanel(View parent)
    {
        _inventoryFrame = new FrameView("Inventory")
        {
            X = 0,
            Y = Pos.Bottom(parent) - 15,
            Width = 30,
            Height = 15
        };

        _inventoryList = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _inventoryFrame.Add(_inventoryList);
        parent.Add(_inventoryFrame);
    }

    public void UpdateInventory(Entity playerEntity, InventorySystem inventorySystem)
    {
        var items = inventorySystem.GetInventoryItems(playerEntity);
        var displayItems = new List<string>();

        foreach (var itemEntity in items)
        {
            var item = _world.Get<Item>(itemEntity);
            var display = item.Name;

            // Add stack count
            if (_world.Has<Stackable>(itemEntity))
            {
                var stackable = _world.Get<Stackable>(itemEntity);
                display += $" ({stackable.Count})";
            }

            // Add equipped indicator
            if (inventorySystem.IsEquipped(playerEntity, itemEntity))
            {
                display += " [E]";
            }

            displayItems.Add(display);
        }

        _inventoryList.SetSource(displayItems);

        // Update title with count
        var inventory = _world.Get<Inventory>(playerEntity);
        _inventoryFrame.Title = $"Inventory ({inventory.CurrentCount}/{inventory.MaxCapacity})";
    }
}
```

### Update HUD on Inventory Changes

```csharp
// In DungeonCrawlerService.cs, after inventory operations
private void OnPickupItem()
{
    var result = _inventorySystem.PickupItem(_playerEntity, selectedItem);

    if (result.Success)
    {
        _hudService.UpdateInventory(_playerEntity, _inventorySystem);
        _hudService.AddMessage(result.Message);
    }
}
```

---

## Testing Examples

### Unit Test: Pickup Item

```csharp
[Fact]
public void PickupItem_AddsToInventory()
{
    // Arrange
    var world = World.Create();
    var inventorySystem = new InventorySystem(world);

    var player = world.Create(
        new Position { X = 5, Y = 5 },
        new Inventory { Items = new List<EntityReference>(), MaxCapacity = 20 },
        new EquipmentSlots { Slots = EquipmentSlots.CreateEmpty().Slots }
    );

    var item = world.Create(
        new Item { Name = "Test Potion", Glyph = '!', Type = ItemType.Consumable },
        new Position { X = 5, Y = 6 }, // Adjacent to player
        new Consumable { Effect = ConsumableEffect.RestoreHealth, EffectValue = 30 }
    );

    // Act
    var result = inventorySystem.PickupItem(player, item);

    // Assert
    Assert.True(result.Success);
    Assert.Contains("Picked up", result.Message);

    var inventory = world.Get<Inventory>(player);
    Assert.Single(inventory.Items);
    Assert.False(world.Has<Position>(item)); // Position removed
}
```

### Integration Test: Use Healing Potion

```csharp
[Fact]
public void UseHealingPotion_RestoresHealth()
{
    // Arrange
    var world = World.Create();
    var inventorySystem = new InventorySystem(world);

    var player = world.Create(
        new Health { Current = 50, Maximum = 100 },
        new Inventory { Items = new List<EntityReference>(), MaxCapacity = 20 }
    );

    var potion = world.Create(
        new Item { Name = "Healing Potion", Glyph = '!', Type = ItemType.Consumable },
        new Consumable { Effect = ConsumableEffect.RestoreHealth, EffectValue = 30 },
        new Stackable { Count = 1, MaxStack = 99 }
    );

    var inventory = world.Get<Inventory>(player);
    inventory.Items.Add(potion.Reference());

    // Act
    var result = inventorySystem.UseConsumable(player, potion);

    // Assert
    Assert.True(result.Success);
    var health = world.Get<Health>(player);
    Assert.Equal(80, health.Current); // 50 + 30
}
```

### Manual Test: Full Inventory Workflow

```csharp
// 1. Spawn items in dungeon
var itemSpawnSystem = new ItemSpawnSystem(world);
itemSpawnSystem.SpawnItemsInRooms(map, rooms);

// 2. Player walks to item and presses 'G'
var pickupableItems = inventorySystem.GetPickupableItems(playerEntity);
var result = inventorySystem.PickupItem(playerEntity, pickupableItems[0]);
Console.WriteLine(result.Message); // "Picked up Healing Potion"

// 3. Player takes damage in combat
var health = world.Get<Health>(playerEntity);
health.Current -= 40;

// 4. Player presses 'U' to use potion
var consumables = inventorySystem.GetConsumables(playerEntity);
result = inventorySystem.UseConsumable(playerEntity, consumables[0]);
Console.WriteLine(result.Message); // "You drink the Healing Potion and recover 30 HP."

// 5. Player finds weapon and equips it
var weapon = pickupableItems.First(i => world.Get<Item>(i).Type == ItemType.Weapon);
inventorySystem.PickupItem(playerEntity, weapon);
result = inventorySystem.EquipItem(playerEntity, weapon);
Console.WriteLine(result.Message); // "Equipped Iron Sword. ATK +5"

// 6. Verify stats updated
var (attack, defense, speed) = inventorySystem.CalculateTotalStats(playerEntity);
Assert.Equal(15, attack); // Base 10 + weapon 5
```

---

## Common Patterns

### Pattern: Item Selection Menu

```csharp
public Entity? ShowItemSelectionMenu(List<Entity> items, string prompt)
{
    if (items.Count == 0) return null;
    if (items.Count == 1) return items[0];

    Console.WriteLine(prompt);
    for (int i = 0; i < items.Count; i++)
    {
        var item = world.Get<Item>(items[i]);
        Console.WriteLine($"{i + 1}. {item.Name}");
    }

    int selection = GetPlayerInput(1, items.Count);
    return items[selection - 1];
}
```

### Pattern: Inventory Full Handling

```csharp
var result = inventorySystem.PickupItem(playerEntity, itemEntity);

if (!result.Success && result.Message.Contains("full"))
{
    // Offer to drop an item first
    Console.WriteLine("Inventory full. Drop an item? (Y/N)");
    if (GetYesNo())
    {
        var itemToDrop = ShowItemSelectionMenu(
            inventorySystem.GetInventoryItems(playerEntity),
            "Select item to drop:"
        );

        if (itemToDrop.HasValue)
        {
            inventorySystem.DropItem(playerEntity, itemToDrop.Value);
            // Retry pickup
            inventorySystem.PickupItem(playerEntity, itemEntity);
        }
    }
}
```

### Pattern: Equipment Comparison

```csharp
public void ShowEquipmentComparison(Entity playerEntity, Entity newItem)
{
    var equippable = world.Get<Equippable>(newItem);
    var currentItem = inventorySystem.GetEquippedItem(playerEntity, equippable.Slot);

    Console.WriteLine($"New: {world.Get<Item>(newItem).Name}");
    Console.WriteLine($"  ATK: {equippable.AttackBonus}, DEF: {equippable.DefenseBonus}");

    if (currentItem.HasValue)
    {
        var currentEquippable = world.Get<Equippable>(currentItem.Value);
        Console.WriteLine($"Current: {world.Get<Item>(currentItem.Value).Name}");
        Console.WriteLine($"  ATK: {currentEquippable.AttackBonus}, DEF: {currentEquippable.DefenseBonus}");

        int atkDiff = equippable.AttackBonus - currentEquippable.AttackBonus;
        int defDiff = equippable.DefenseBonus - currentEquippable.DefenseBonus;

        Console.WriteLine($"Change: ATK {(atkDiff >= 0 ? "+" : "")}{atkDiff}, DEF {(defDiff >= 0 ? "+" : "")}{defDiff}");
    }
}
```

---

## Next Steps

- Review [data-model.md](./data-model.md) for complete component schemas
- Review [contracts/](./contracts/) for system interfaces
- Implement systems in `LablabBean.Game.Core/Systems/`
- Extend HUD in `LablabBean.Game.Terminal/Services/HudService.cs`
- Add input handling in `LablabBean.Console/Services/DungeonCrawlerService.cs`
- Write unit tests for each system method
- Run integration tests for full workflows

---

**Quickstart Complete**: Ready to begin implementation!
