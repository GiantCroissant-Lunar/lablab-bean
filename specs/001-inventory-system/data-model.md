# Data Model: Inventory System

**Feature**: Inventory System with Item Pickup and Usage
**Date**: 2025-10-21
**Phase**: 1 - Design & Contracts

## Overview

This document defines the complete data model for the inventory system using ECS components. All components follow the Arch ECS framework patterns established in `LablabBean.Game.Core`.

## Component Definitions

### Core Item Components

#### Item Component
```csharp
namespace LablabBean.Game.Core.Components;

/// <summary>
/// Base component for all items in the game.
/// Attached to entities that represent items (on ground or in inventory).
/// </summary>
public struct Item
{
    /// <summary>Item display name (e.g., "Healing Potion", "Iron Sword")</summary>
    public string Name { get; set; }
    
    /// <summary>Visual representation character (e.g., '!', '/', '[')</summary>
    public char Glyph { get; set; }
    
    /// <summary>Item description for tooltips/inspection</summary>
    public string Description { get; set; }
    
    /// <summary>Item type for categorization</summary>
    public ItemType Type { get; set; }
    
    /// <summary>Item weight (for future encumbrance system)</summary>
    public int Weight { get; set; }
}

public enum ItemType
{
    Consumable,
    Weapon,
    Armor,
    Accessory,
    Miscellaneous
}
```

#### Consumable Component
```csharp
namespace LablabBean.Game.Core.Components;

/// <summary>
/// Component for items that can be consumed (used once and destroyed).
/// </summary>
public struct Consumable
{
    /// <summary>Type of consumable effect</summary>
    public ConsumableEffect Effect { get; set; }
    
    /// <summary>Magnitude of effect (e.g., 30 HP for healing potion)</summary>
    public int EffectValue { get; set; }
    
    /// <summary>Whether this consumable can be used outside of combat</summary>
    public bool UsableOutOfCombat { get; set; }
}

public enum ConsumableEffect
{
    RestoreHealth,
    RestoreMana,      // Future
    IncreaseSpeed,    // Future
    CurePoison        // Future
}
```

#### Equippable Component
```csharp
namespace LablabBean.Game.Core.Components;

/// <summary>
/// Component for items that can be equipped to modify stats.
/// </summary>
public struct Equippable
{
    /// <summary>Which equipment slot this item occupies</summary>
    public EquipmentSlot Slot { get; set; }
    
    /// <summary>Attack bonus (for weapons)</summary>
    public int AttackBonus { get; set; }
    
    /// <summary>Defense bonus (for armor)</summary>
    public int DefenseBonus { get; set; }
    
    /// <summary>Speed modifier (negative = slower, positive = faster)</summary>
    public int SpeedModifier { get; set; }
    
    /// <summary>Whether this item requires two hands (future: two-handed weapons)</summary>
    public bool TwoHanded { get; set; }
}

public enum EquipmentSlot
{
    MainHand,
    OffHand,
    Head,
    Chest,
    Legs,
    Feet,
    Hands,
    Accessory1,
    Accessory2
}
```

#### Stackable Component
```csharp
namespace LablabBean.Game.Core.Components;

/// <summary>
/// Component for items that can stack (multiple instances in one inventory slot).
/// Typically used for consumables.
/// </summary>
public struct Stackable
{
    /// <summary>Current number of items in this stack</summary>
    public int Count { get; set; }
    
    /// <summary>Maximum stack size (e.g., 99 for potions)</summary>
    public int MaxStack { get; set; }
}
```

### Player Inventory Components

#### Inventory Component
```csharp
namespace LablabBean.Game.Core.Components;

/// <summary>
/// Component attached to player entity to store inventory.
/// Contains references to item entities.
/// </summary>
public struct Inventory
{
    /// <summary>List of item entity references</summary>
    public List<EntityReference> Items { get; set; }
    
    /// <summary>Maximum number of items (20 for MVP)</summary>
    public int MaxCapacity { get; set; }
    
    /// <summary>Current number of items (including stacks)</summary>
    public int CurrentCount => Items?.Count ?? 0;
    
    /// <summary>Whether inventory is full</summary>
    public bool IsFull => CurrentCount >= MaxCapacity;
}
```

#### EquipmentSlots Component
```csharp
namespace LablabBean.Game.Core.Components;

/// <summary>
/// Component attached to player entity to track equipped items.
/// Maps equipment slots to item entity references.
/// </summary>
public struct EquipmentSlots
{
    /// <summary>Dictionary mapping slot type to equipped item entity (null if empty)</summary>
    public Dictionary<EquipmentSlot, EntityReference?> Slots { get; set; }
    
    /// <summary>Initialize with empty slots</summary>
    public static EquipmentSlots CreateEmpty()
    {
        return new EquipmentSlots
        {
            Slots = new Dictionary<EquipmentSlot, EntityReference?>
            {
                { EquipmentSlot.MainHand, null },
                { EquipmentSlot.OffHand, null },
                { EquipmentSlot.Head, null },
                { EquipmentSlot.Chest, null },
                { EquipmentSlot.Legs, null },
                { EquipmentSlot.Feet, null },
                { EquipmentSlot.Hands, null },
                { EquipmentSlot.Accessory1, null },
                { EquipmentSlot.Accessory2, null }
            }
        };
    }
}
```

## Entity Archetypes

### Item on Ground
**Components**: `Item`, `Position`, `Renderable`, `Visible`
- Optional: `Consumable`, `Equippable`, `Stackable`

**Example**: Healing potion on dungeon floor
```csharp
var itemEntity = world.Create(
    new Item { Name = "Healing Potion", Glyph = '!', Type = ItemType.Consumable },
    new Position { X = 10, Y = 15 },
    new Renderable { Glyph = '!', ForegroundColor = Color.Red },
    new Visible { IsVisible = true },
    new Consumable { Effect = ConsumableEffect.RestoreHealth, EffectValue = 30 },
    new Stackable { Count = 1, MaxStack = 99 }
);
```

### Item in Inventory
**Components**: `Item`, `Consumable` OR `Equippable`
- Optional: `Stackable`
- **No** `Position` component (not on map)

**Example**: Sword in player inventory
```csharp
var swordEntity = world.Create(
    new Item { Name = "Iron Sword", Glyph = '/', Type = ItemType.Weapon },
    new Equippable { 
        Slot = EquipmentSlot.MainHand, 
        AttackBonus = 5, 
        DefenseBonus = 0 
    }
);
// Add to player's inventory
playerInventory.Items.Add(swordEntity.Reference());
```

### Equipped Item
**Components**: Same as "Item in Inventory"
- Referenced in player's `EquipmentSlots` component
- Also present in player's `Inventory` component

**Example**: Equipped armor
```csharp
var armorEntity = world.Create(
    new Item { Name = "Leather Armor", Glyph = '[', Type = ItemType.Armor },
    new Equippable { 
        Slot = EquipmentSlot.Chest, 
        AttackBonus = 0, 
        DefenseBonus = 3 
    }
);
// Add to inventory and equip
playerInventory.Items.Add(armorEntity.Reference());
playerEquipment.Slots[EquipmentSlot.Chest] = armorEntity.Reference();
```

## State Transitions

### Item Lifecycle

```
┌─────────────┐
│   Spawned   │ (Item + Position + Renderable)
└──────┬──────┘
       │ Player presses 'G' (pickup)
       ▼
┌─────────────┐
│ In Inventory│ (Item, no Position, in Inventory.Items list)
└──────┬──────┘
       │ Player presses 'U' (use/equip)
       ▼
┌─────────────┐  ┌──────────────┐
│  Consumed   │  │   Equipped   │ (Item, in EquipmentSlots.Slots)
│  (Deleted)  │  └──────┬───────┘
└─────────────┘         │ Unequip or replace
                        ▼
                  ┌─────────────┐
                  │ In Inventory│
                  └──────┬──────┘
                         │ Drop
                         ▼
                  ┌─────────────┐
                  │   On Ground │
                  └─────────────┘
```

### Inventory State Machine

```
Player Action          Precondition                    Effect
─────────────────────────────────────────────────────────────────────
Pickup ('G')          - Item adjacent                 - Remove Position from item
                      - Inventory not full            - Add to Inventory.Items
                      - Item has Position             - Show "Picked up X" message

Use Consumable ('U')  - Item in inventory             - Apply effect (heal, etc.)
                      - Item has Consumable           - Decrement Stackable.Count
                      - Valid usage context           - Remove if Count = 0
                                                      - Consume player turn

Equip ('U')           - Item in inventory             - Unequip old item in slot
                      - Item has Equippable           - Set EquipmentSlots[slot] = item
                      - Slot available/compatible     - Recalculate stats
                                                      - Show "Equipped X" message

Unequip               - Item equipped                 - Set EquipmentSlots[slot] = null
                      - Inventory has space           - Recalculate stats
                                                      - Show "Unequipped X" message

Drop                  - Item in inventory             - Add Position at player location
                      - Item not equipped             - Remove from Inventory.Items
                                                      - Add Renderable + Visible
```

## Validation Rules

### Inventory Constraints
- ✅ Max 20 items in inventory (enforced by `Inventory.MaxCapacity`)
- ✅ Stackable items with same name share one slot (up to `Stackable.MaxStack`)
- ✅ Cannot pickup if inventory full
- ✅ Cannot drop equipped items (must unequip first)

### Equipment Constraints
- ✅ Only one item per slot (except accessories: 2 slots)
- ✅ Item's `Equippable.Slot` must match target slot
- ✅ Equipping new item auto-unequips old item in same slot
- ✅ Two-handed weapons occupy both MainHand and OffHand (future)

### Usage Constraints
- ✅ Consumables can only be used once (then destroyed)
- ✅ Cannot use healing potion at full health
- ✅ Using item consumes player turn (energy cost)
- ✅ Equipment can be equipped/unequipped any time (no turn cost for MVP)

## Query Patterns

### Common ECS Queries

```csharp
// Find all items on ground near player
var itemsNearPlayer = world.Query(
    in new QueryDescription()
        .WithAll<Item, Position>()
        .WithNone<Inventory>()
);

// Find all consumables in player inventory
var consumablesInInventory = world.Query(
    in new QueryDescription()
        .WithAll<Item, Consumable>()
);

// Get player's equipped weapon
var playerEquipment = world.Get<EquipmentSlots>(playerEntity);
var weaponRef = playerEquipment.Slots[EquipmentSlot.MainHand];
if (weaponRef.HasValue)
{
    var weapon = world.Get<Equippable>(weaponRef.Value);
    int attackBonus = weapon.AttackBonus;
}

// Find stackable items of same type
var potions = world.Query(
    in new QueryDescription()
        .WithAll<Item, Stackable>()
).Where(e => world.Get<Item>(e).Name == "Healing Potion");
```

## Stat Calculation

### Combat Stats with Equipment

```csharp
public static (int attack, int defense, int speed) CalculateStats(
    World world, 
    Entity playerEntity)
{
    var baseCombat = world.Get<Combat>(playerEntity);
    var equipment = world.Get<EquipmentSlots>(playerEntity);
    
    int totalAttack = baseCombat.Attack;
    int totalDefense = baseCombat.Defense;
    int totalSpeed = 100; // Base speed
    
    foreach (var (slot, itemRef) in equipment.Slots)
    {
        if (itemRef.HasValue && world.Has<Equippable>(itemRef.Value))
        {
            var equippable = world.Get<Equippable>(itemRef.Value);
            totalAttack += equippable.AttackBonus;
            totalDefense += equippable.DefenseBonus;
            totalSpeed += equippable.SpeedModifier;
        }
    }
    
    return (totalAttack, totalDefense, totalSpeed);
}
```

## Serialization Schema (Future)

### JSON Format for Save/Load

```json
{
  "inventory": {
    "maxCapacity": 20,
    "items": [
      {
        "id": "item_001",
        "name": "Healing Potion",
        "type": "Consumable",
        "glyph": "!",
        "consumable": {
          "effect": "RestoreHealth",
          "value": 30
        },
        "stackable": {
          "count": 3,
          "maxStack": 99
        }
      },
      {
        "id": "item_002",
        "name": "Iron Sword",
        "type": "Weapon",
        "glyph": "/",
        "equippable": {
          "slot": "MainHand",
          "attackBonus": 5,
          "defenseBonus": 0
        }
      }
    ]
  },
  "equipment": {
    "MainHand": "item_002",
    "Chest": "item_003",
    "Head": null,
    "Legs": null
  }
}
```

## Integration Points

### With Existing Systems

#### CombatSystem
- **Reads**: `EquipmentSlots` to calculate attack/defense
- **Modification**: Update `CalculateDamage()` to include equipment bonuses

#### MovementSystem
- **No changes required**: Items on ground don't block movement

#### ActorSystem
- **Modification**: Item usage consumes energy (player turn)

#### GameStateManager
- **Modification**: Initialize player with `Inventory` and `EquipmentSlots` components
- **Modification**: Handle item-related input (G, U keys)

### With New Systems

#### InventorySystem
- **Creates**: New system to handle all inventory operations
- **Queries**: Items in inventory, items on ground, equipped items

#### ItemSpawnSystem
- **Creates**: New system to spawn items during map generation and enemy death
- **Queries**: Room locations, enemy death events

## Performance Characteristics

### Memory Usage
- **Per Item Entity**: ~100 bytes (components + entity overhead)
- **100 Items**: ~10 KB
- **Player Inventory**: ~2 KB (20 item references + metadata)

### Query Performance
- **Items near player**: O(n) where n = items with Position (typically <50)
- **Items in inventory**: O(1) access via `Inventory.Items` list
- **Equipped item lookup**: O(1) dictionary access
- **Stack merging**: O(n) where n = inventory size (max 20)

**Conclusion**: All operations well within performance budget (<1ms).

---

**Data Model Complete**: All components, entities, and relationships defined.
**Next**: Generate system contracts and API definitions.
