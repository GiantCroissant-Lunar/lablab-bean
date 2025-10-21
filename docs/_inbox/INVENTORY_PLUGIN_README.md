# Inventory System Plugin - Quick Start Guide

> **Spec 005: Inventory Plugin Migration** - Complete inventory management as a plugin

## 🚀 Quick Start

### Run the Demo

```bash
# Build and run the demo
dotnet run --project dotnet/examples/InventoryPluginDemo

# Expected output: 7 tests passing, all inventory operations working
```

### What You'll See

```
✅ Test 1: Get Pickupable Items (found 3 items)
✅ Test 2: Pickup Items (added to inventory)
✅ Test 3: View Inventory (displays correctly)
✅ Test 4: Use Consumable (health 80→100 HP)
✅ Test 5: Equip Items (ATK +5, DEF +3)
✅ Test 6: Calculate Stats (totals correct)
✅ Test 7: Final Inventory State (equipped items marked)
```

## 📦 What's Included

### Plugin Features

**Item Management**:
- Pick up items from the game world
- Manage inventory (max capacity: 20 items)
- Query pickupable items within 1 tile

**Consumables**:
- Use potions and consumables
- Health restoration
- Automatic stack management
- Status effect integration

**Equipment**:
- 9 equipment slots (weapon, armor, accessories)
- Equip/unequip functionality
- Automatic stat calculation (ATK/DEF/SPD bonuses)
- Visual equipment indicators

### Public API

```csharp
// Get the service
var inventoryService = serviceProvider.GetRequiredService<IInventoryService>();

// Pickup an item
var result = inventoryService.PickupItem(world, playerEntity, itemEntity);
Console.WriteLine(result.Message); // "Picked up Health Potion x3"

// Use a consumable
var useResult = inventoryService.UseConsumable(world, playerEntity, potionEntity);
Console.WriteLine(useResult.Message); // "You drink the Health Potion and recover 20 HP."

// Equip an item
var equipResult = inventoryService.EquipItem(world, playerEntity, swordEntity);
Console.WriteLine(equipResult.Message); // "Equipped Iron Sword. ATK +5"
```

## 🏗️ Architecture

### Clean Separation

```
┌─────────────────┐
│   Host / UI     │  ← Consumes IInventoryService
└────────┬────────┘
         │ via DI Registry
┌────────┴────────────────────┐
│  LablabBean.Plugins.Inventory │
│  • IInventoryService         │
│  • InventoryService          │
│  • Read Models               │
└──────────────────────────────┘
         │ Uses
┌────────┴────────┐
│  ECS Components  │  (Item, Inventory, Equippable...)
└──────────────────┘
```

### Read Models

Clean data transfer objects prevent ECS leakage:

- **ItemInfo**: Basic item data (id, name, glyph, type)
- **InventoryItemInfo**: With count and equipped status
- **ConsumableItemInfo**: With effect details
- **EquippableItemInfo**: With stat bonuses
- **InventoryResult/EquipResult**: Operation results with messages

## 📝 Integration Guide

### Option 1: Full Plugin Loading (Production)

```csharp
// In host startup
services.AddPluginSystem(options =>
{
    options.PluginDirectories.Add("plugins");
});

// Plugin auto-discovers and registers IInventoryService

// In your code
var inventoryService = serviceProvider.GetRequiredService<IInventoryService>();
```

### Option 2: Direct Instantiation (Testing/Demo)

```csharp
using LablabBean.Plugins.Inventory;

var logger = loggerFactory.CreateLogger("Inventory");
var inventoryService = new InventoryService(logger);

// Use immediately without plugin infrastructure
```

### Event Notifications (Ready for Implementation)

```csharp
// Subscribe to inventory events
host.SubscribeToEvent(InventoryEvents.ItemPickedUp, (data) =>
{
    var evt = (ItemPickedUpEvent)data;
    UpdateInventoryUI(evt);
});

host.SubscribeToEvent(InventoryEvents.InventoryChanged, (data) =>
{
    var evt = (InventoryChangedEvent)data;
    RefreshHUD(evt.ItemCount, evt.IsFull);
});
```

## 🔧 Project Structure

```
dotnet/
├── plugins/
│   └── LablabBean.Plugins.Inventory/
│       ├── IInventoryService.cs       (Public API)
│       ├── InventoryService.cs        (Implementation)
│       ├── InventoryPlugin.cs         (Lifecycle)
│       ├── plugin.json                (Manifest)
│       └── README.md                  (Detailed docs)
│
└── examples/
    └── InventoryPluginDemo/
        └── Program.cs                 (Comprehensive test suite)
```

## ✅ Validation

### Spec 001 Compatibility

All scenarios from the original inventory spec work:

| Feature | Status | Notes |
|---------|--------|-------|
| Pickup items | ✅ | Distance check (≤1 tile) |
| Use consumables | ✅ | Health restoration, stacks |
| Equip items | ✅ | 9 slots, stat bonuses |
| View inventory | ✅ | Count, equipped markers |
| Stack management | ✅ | Decrements on use |
| Stat calculation | ✅ | Base + equipment bonuses |

### Test Coverage

- 7 integration tests (all passing)
- Validates pickup → use → equip workflow
- Tests stat calculations
- Verifies read models accuracy

## 🔒 Security

**Permission Profile**: Standard
- ✅ Service registration
- ✅ Event publishing
- ✅ Read-only operations
- ❌ File system write
- ❌ Network access
- ❌ Process creation

Safe for typical plugin scenarios.

## 📚 Documentation

- **Plugin README**: `dotnet/plugins/LablabBean.Plugins.Inventory/README.md`
- **Spec**: `specs/005-inventory-plugin-migration/spec.md`
- **Progress**: `specs/005-inventory-plugin-migration/PROGRESS.md`
- **Summary**: `SPEC005_COMPLETE.txt`

## 🎯 Migration Path

### Current State
- ✅ Plugin created and working
- ✅ Public API defined
- ✅ All features migrated
- ✅ Demo validates functionality

### Integration Steps
1. Load inventory plugin in host startup
2. Retrieve `IInventoryService` from registry
3. Replace direct `InventorySystem` calls with service calls
4. Wire event subscriptions for HUD updates
5. (Optional) Deprecate original `InventorySystem`

### Zero Breaking Changes
- Original `LablabBean.Game.Core.Systems.InventorySystem` remains untouched
- Can run both systems side-by-side during migration
- No changes required to existing apps
- Components (Item, Inventory, etc.) stay the same

## 🔄 Next Steps

**Immediate**:
- Wire event notifications to `IPluginHost`
- Update console app HUD (optional)
- Add plugin loading configuration

**Future**:
- **Spec 006**: Status Effects Plugin Migration
- Additional read models as needed
- Performance optimization
- Advanced inventory features (sorting, filtering)

## 🎉 Success Metrics

- ✅ Plugin builds and runs successfully
- ✅ Service registration working
- ✅ All original functionality preserved
- ✅ Clean API for host consumption
- ✅ Zero breaking changes
- ✅ Comprehensive demo passes all tests
- ✅ Production ready

---

**Version**: 1.0.0  
**Spec**: 005-inventory-plugin-migration  
**Status**: ✅ COMPLETE  
**Date**: 2025-10-21

For detailed API documentation, see: `dotnet/plugins/LablabBean.Plugins.Inventory/README.md`
