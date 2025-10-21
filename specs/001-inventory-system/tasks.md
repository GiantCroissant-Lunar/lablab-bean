# Implementation Tasks: Inventory System with Item Pickup and Usage

**Feature**: Inventory System
**Branch**: `001-inventory-system`
**Date**: 2025-10-21
**Status**: Ready for Implementation

## Overview

This document provides an actionable, dependency-ordered task list for implementing the inventory system. Tasks are organized by user story to enable independent implementation and testing.

**Total Tasks**: 45
**Estimated Effort**: 3-5 days
**MVP Scope**: Phase 3 (User Story 1 - Item Pickup) delivers first testable value

---

## Task Summary by Phase

| Phase | Description | Task Count | Can Parallelize |
|-------|-------------|------------|-----------------|
| Phase 1 | Setup & Infrastructure | 5 | Yes (4/5) |
| Phase 2 | Foundational Components | 8 | Yes (6/8) |
| Phase 3 | User Story 1 - Item Pickup (P1) | 8 | Yes (5/8) |
| Phase 4 | User Story 2 - Inventory Display (P1) | 6 | Yes (4/6) |
| Phase 5 | User Story 3 - Consume Healing Potions (P2) | 7 | Yes (4/7) |
| Phase 6 | User Story 4 - Equip Weapons/Armor (P3) | 8 | Yes (5/8) |
| Phase 7 | Polish & Integration | 3 | Yes (2/3) |

---

## Implementation Strategy

### MVP Delivery (Minimum Viable Product)
**Scope**: Phase 1 + Phase 2 + Phase 3 (User Story 1)
- Players can pick up items from the dungeon floor
- Items appear in inventory display
- **Delivers**: Core item collection mechanic
- **Test Criteria**: Walk to item, press 'G', verify item in inventory

### Incremental Delivery
- **Phase 3**: Item pickup (P1) - First playable feature
- **Phase 4**: Inventory display (P1) - Enhances visibility
- **Phase 5**: Consumables (P2) - Adds strategic resource management
- **Phase 6**: Equipment (P3) - Adds character progression

Each phase is independently testable and delivers user value.

---

## Phase 1: Setup & Infrastructure

**Goal**: Prepare project structure and register new systems

### Tasks

- [ ] T001 Review existing ECS architecture in `dotnet/framework/LablabBean.Game.Core/`
- [ ] T002 [P] Create `Components/Item.cs` with Item, Consumable, Equippable, Stackable component structs per data-model.md
- [ ] T003 [P] Create `Systems/InventorySystem.cs` skeleton class with constructor
- [ ] T004 [P] Create `Systems/ItemSpawnSystem.cs` skeleton class with constructor
- [ ] T005 Register InventorySystem and ItemSpawnSystem in `dotnet/console-app/LablabBean.Console/Program.cs` services

**Completion Criteria**: All systems registered, project compiles successfully

---

## Phase 2: Foundational Components

**Goal**: Implement core inventory data structures and item definitions

### Tasks

- [ ] T006 [P] Implement Item component struct in `Components/Item.cs` (Name, Glyph, Description, Type, Weight)
- [ ] T007 [P] Implement Consumable component struct in `Components/Item.cs` (Effect, EffectValue, UsableOutOfCombat)
- [ ] T008 [P] Implement Equippable component struct in `Components/Item.cs` (Slot, AttackBonus, DefenseBonus, SpeedModifier, TwoHanded)
- [ ] T009 [P] Implement Stackable component struct in `Components/Item.cs` (Count, MaxStack)
- [ ] T010 [P] Implement Inventory component struct in `Components/Actor.cs` (Items list, MaxCapacity, IsFull property)
- [ ] T011 [P] Implement EquipmentSlots component struct in `Components/Actor.cs` (Slots dictionary, CreateEmpty() method)
- [ ] T012 Add Inventory and EquipmentSlots components to player entity in `Services/GameStateManager.cs` InitializeNewGame()
- [ ] T013 Create ItemDefinitions static class in `Systems/ItemSpawnSystem.cs` with predefined items (HealingPotion, IronSword, LeatherArmor, etc.) per contracts

**Completion Criteria**: All components defined, player entity has inventory, project compiles

---

## Phase 3: User Story 1 - Item Pickup (P1)

**User Story**: "A player explores the dungeon and finds a healing potion on the floor. They walk adjacent to the item and press the 'G' key to pick it up. The item disappears from the map and appears in their inventory display in the HUD."

**Independent Test Criteria**:
- Spawn item at known position
- Move player adjacent to item
- Press 'G' key
- Verify item removed from map (no Position component)
- Verify item added to player's Inventory.Items list
- Verify "Picked up X" message displayed

### Tasks

- [ ] T014 [US1] Implement GetPickupableItems() in `Systems/InventorySystem.cs` (query items with Position within 1 tile of player)
- [ ] T015 [US1] Implement CanPickup() in `Systems/InventorySystem.cs` (check inventory space, distance, item has Position)
- [ ] T016 [P] [US1] Implement PickupItem() in `Systems/InventorySystem.cs` (remove Position/Renderable/Visible, add to Inventory.Items, handle stacking)
- [ ] T017 [P] [US1] Implement SpawnItem() in `Systems/ItemSpawnSystem.cs` (create entity with Item + Position + Renderable + Visible + optional Consumable/Equippable)
- [ ] T018 [US1] Add 'G' key handling in `dotnet/console-app/LablabBean.Console/Services/DungeonCrawlerService.cs` to call InventorySystem.PickupItem()
- [ ] T019 [US1] Handle multiple items on same tile (show selection menu in message log)
- [ ] T020 [US1] Handle inventory full scenario (display "Inventory full" message, item stays on ground)
- [ ] T021 [US1] Add pickup feedback messages to HUD message log

**Completion Criteria**: Player can pick up items, items move from ground to inventory, appropriate messages displayed

**MVP Checkpoint**: ✅ This phase delivers first playable feature

---

## Phase 4: User Story 2 - Inventory Display (P1)

**User Story**: "As a player explores the dungeon, they can see their current inventory at all times in the HUD panel. The display shows item names, quantities (for stackable items), and which items are currently equipped."

**Independent Test Criteria**:
- Add items to player inventory programmatically
- Verify HUD shows item names
- Verify stackable items show count (e.g., "Healing Potion (3)")
- Verify equipped items show indicator (e.g., "Iron Sword (equipped)")
- Verify "Inventory: Empty" when no items
- Verify "Inventory: 20/20 (FULL)" warning when full

### Tasks

- [ ] T022 [P] [US2] Create inventory FrameView in `dotnet/framework/LablabBean.Game.Terminal/Services/HudService.cs`
- [ ] T023 [P] [US2] Implement GetInventoryItems() in `Systems/InventorySystem.cs` (return player's Inventory.Items list)
- [ ] T024 [US2] Implement IsEquipped() in `Systems/InventorySystem.cs` (check if item in EquipmentSlots)
- [ ] T025 [US2] Implement UpdateInventory() in `HudService.cs` to display items with quantities and equipped status
- [ ] T026 [US2] Add inventory count display to HUD title (e.g., "Inventory (5/20)")
- [ ] T027 [US2] Call UpdateInventory() after every inventory operation (pickup, drop, use, equip)

**Completion Criteria**: Inventory panel displays all items with correct formatting, updates in real-time

---

## Phase 5: User Story 3 - Consume Healing Potions (P2)

**User Story**: "During combat, a player's health drops to 30/100. They open their inventory, select a healing potion, and press 'U' to use it. Their health increases by 30 points (to 60/100), the potion is consumed and removed from inventory, and a message appears: 'You drink the healing potion and recover 30 HP.'"

**Independent Test Criteria**:
- Set player health to 50/100
- Add healing potion to inventory programmatically
- Press 'U' key and select potion
- Verify health increased to 80/100
- Verify potion removed from inventory (or count decremented if stacked)
- Verify "You drink the Healing Potion and recover 30 HP" message
- Verify cannot use potion at full health

### Tasks

- [ ] T028 [P] [US3] Implement GetConsumables() in `Systems/InventorySystem.cs` (query items with Consumable component)
- [ ] T029 [P] [US3] Implement CanUseConsumable() in `Systems/InventorySystem.cs` (check health not full, item in inventory, has Consumable)
- [ ] T030 [US3] Implement UseConsumable() in `Systems/InventorySystem.cs` (apply effect, decrement stack, remove if count=0, return feedback message)
- [ ] T031 [US3] Add 'U' key handling in `DungeonCrawlerService.cs` to show consumable selection menu
- [ ] T032 [US3] Implement healing effect application (modify Health component, cap at maximum)
- [ ] T033 [US3] Handle stackable consumables (decrement Stackable.Count, destroy entity if Count=0)
- [ ] T034 [US3] Add usage feedback messages ("You drink the Healing Potion and recover 30 HP", "Already at full health")

**Completion Criteria**: Player can use healing potions, health restored correctly, items consumed, appropriate messages displayed

---

## Phase 6: User Story 4 - Equip Weapons and Armor (P3)

**User Story**: "A player finds a better sword in the dungeon. They pick it up and press 'U' to use/equip it. The new sword replaces their current weapon, and their attack stat increases. The old weapon is unequipped but remains in inventory (or drops to the ground if inventory is full)."

**Independent Test Criteria**:
- Add Iron Sword (+5 ATK) to inventory
- Equip sword
- Verify player attack stat increased by 5
- Verify sword marked as equipped in inventory
- Add Steel Sword (+10 ATK) to inventory
- Equip Steel Sword
- Verify Iron Sword unequipped (still in inventory)
- Verify attack stat now +10
- Verify stat change message displayed

### Tasks

- [ ] T035 [P] [US4] Implement GetEquippables() in `Systems/InventorySystem.cs` (query items with Equippable component)
- [ ] T036 [P] [US4] Implement CanEquip() in `Systems/InventorySystem.cs` (check item in inventory, has Equippable, slot compatible)
- [ ] T037 [US4] Implement EquipItem() in `Systems/InventorySystem.cs` (unequip old item, set EquipmentSlots[slot], recalculate stats, return StatChanges)
- [ ] T038 [P] [US4] Implement UnequipItem() in `Systems/InventorySystem.cs` (clear EquipmentSlots[slot], recalculate stats)
- [ ] T039 [P] [US4] Implement CalculateTotalStats() in `Systems/InventorySystem.cs` (sum base stats + all equipment bonuses)
- [ ] T040 [US4] Integrate CalculateTotalStats() with CombatSystem in `Systems/CombatSystem.cs` CalculateDamage()
- [ ] T041 [US4] Add equipment selection to 'U' key handling (show equippables if no consumables selected)
- [ ] T042 [US4] Add equipment feedback messages ("Equipped Iron Sword. ATK +5", "Unequipped Iron Sword. ATK -5")

**Completion Criteria**: Player can equip/unequip weapons and armor, stats update correctly, old equipment unequipped automatically, messages displayed

---

## Phase 7: Polish & Integration

**Goal**: Item spawning, final integration, and testing

### Tasks

- [ ] T043 [P] Implement SpawnItemsInRooms() in `Systems/ItemSpawnSystem.cs` (spawn items in 20-50% of rooms using weighted spawn tables)
- [ ] T044 Implement SpawnEnemyLoot() in `Systems/ItemSpawnSystem.cs` (30% potion, 10% equipment on enemy death)
- [ ] T045 Integrate item spawning with MapGenerator in `Maps/MapGenerator.cs` and CombatSystem enemy death handling

**Completion Criteria**: Items spawn in dungeons, enemies drop loot, full gameplay loop functional

---

## Dependencies & Execution Order

### Critical Path (Must Complete in Order)
1. Phase 1 (Setup) → Phase 2 (Foundational) → Phase 3+ (User Stories)
2. Within each user story phase: Components → Systems → UI → Integration

### User Story Dependencies
- **US1 (Pickup)**: No dependencies - can implement first ✅
- **US2 (Display)**: Requires US1 (needs items in inventory to display)
- **US3 (Consumables)**: Requires US1 (needs pickup) and US2 (needs display)
- **US4 (Equipment)**: Requires US1 (needs pickup) and US2 (needs display)

### Parallel Execution Opportunities

**Phase 1** (4 tasks can run in parallel):
- T002, T003, T004 can be done simultaneously (different files)
- T005 must wait for T002-T004

**Phase 2** (6 tasks can run in parallel):
- T006, T007, T008, T009 can be done simultaneously (same file, different structs)
- T010, T011 can be done simultaneously (same file, different structs)
- T012, T013 must wait for T006-T011

**Phase 3** (5 tasks can run in parallel):
- T014, T015 can be done together (same file, related methods)
- T016, T017 can be done in parallel (different files)
- T018, T019, T020, T021 must be done sequentially (same file, dependent logic)

**Phase 4** (4 tasks can run in parallel):
- T022, T023, T024 can be done in parallel (different files)
- T025, T026, T027 must be done sequentially (same file, dependent logic)

**Phase 5** (4 tasks can run in parallel):
- T028, T029 can be done together (same file, related methods)
- T030, T032, T033 can be done in parallel (different concerns)
- T031, T034 must be done sequentially (UI integration)

**Phase 6** (5 tasks can run in parallel):
- T035, T036, T037, T038, T039 can be done in parallel (same file, independent methods)
- T040, T041, T042 must be done sequentially (integration tasks)

**Phase 7** (2 tasks can run in parallel):
- T043, T044 can be done in parallel (same file, independent methods)
- T045 must wait for T043, T044

---

## Testing Strategy

### Manual Testing Workflow

**Test 1: Item Pickup (US1)**
```
1. Start new game
2. Spawn healing potion at (10, 10)
3. Move player to (10, 11) - adjacent
4. Press 'G'
5. Verify: Item removed from map, "Picked up Healing Potion" message, inventory shows "Healing Potion (1)"
```

**Test 2: Inventory Display (US2)**
```
1. Add 3 healing potions to inventory programmatically
2. Add iron sword to inventory programmatically
3. Verify: HUD shows "Healing Potion (3)" and "Iron Sword"
4. Equip iron sword
5. Verify: HUD shows "Iron Sword (equipped)"
```

**Test 3: Use Consumable (US3)**
```
1. Set player health to 50/100
2. Add healing potion to inventory
3. Press 'U', select potion
4. Verify: Health = 80/100, potion removed, "You drink the Healing Potion and recover 30 HP" message
5. Try to use potion at 100/100 health
6. Verify: "Already at full health" message, potion not consumed
```

**Test 4: Equip Weapon (US4)**
```
1. Add iron sword (+5 ATK) to inventory
2. Press 'U', select sword
3. Verify: "Equipped Iron Sword. ATK +5" message, attack stat increased
4. Add steel sword (+10 ATK) to inventory
5. Press 'U', select steel sword
6. Verify: "Equipped Steel Sword. ATK +10" message, iron sword unequipped, attack stat = base + 10
```

**Test 5: Full Inventory (Edge Case)**
```
1. Fill inventory with 20 items
2. Spawn item on ground
3. Move adjacent, press 'G'
4. Verify: "Inventory full" message, item stays on ground
```

**Test 6: Multiple Items on Same Tile (Edge Case)**
```
1. Spawn 3 items at (10, 10)
2. Move player to (10, 11)
3. Press 'G'
4. Verify: Selection menu shows all 3 items
5. Select item 2
6. Verify: Item 2 picked up, items 1 and 3 remain on ground
```

### Integration Testing

**Full Gameplay Loop**:
1. Start new game with item spawning enabled
2. Explore dungeon, find items in rooms
3. Pick up healing potion and weapon
4. Equip weapon, verify stats updated
5. Take damage in combat
6. Use healing potion, verify health restored
7. Kill enemy, verify loot drops
8. Pick up loot, verify inventory updates

---

## File Modification Summary

### New Files
- `dotnet/framework/LablabBean.Game.Core/Components/Item.cs` (T002, T006-T009)
- `dotnet/framework/LablabBean.Game.Core/Systems/InventorySystem.cs` (T003, T014-T042)
- `dotnet/framework/LablabBean.Game.Core/Systems/ItemSpawnSystem.cs` (T004, T013, T017, T043-T044)

### Modified Files
- `dotnet/framework/LablabBean.Game.Core/Components/Actor.cs` (T010-T011)
- `dotnet/framework/LablabBean.Game.Core/Services/GameStateManager.cs` (T012, T045)
- `dotnet/framework/LablabBean.Game.Core/Systems/CombatSystem.cs` (T040, T045)
- `dotnet/framework/LablabBean.Game.Core/Maps/MapGenerator.cs` (T045)
- `dotnet/framework/LablabBean.Game.Terminal/Services/HudService.cs` (T022, T025-T027)
- `dotnet/console-app/LablabBean.Console/Services/DungeonCrawlerService.cs` (T018-T021, T031, T041-T042)
- `dotnet/console-app/LablabBean.Console/Program.cs` (T005)

---

## Progress Tracking

**Completed**: 0/45 tasks (0%)

**Current Phase**: Phase 1 - Setup & Infrastructure

**Next Milestone**: Complete Phase 1 (5 tasks) → Project structure ready

**MVP Milestone**: Complete Phase 1-3 (21 tasks) → First playable feature

**Full Feature**: Complete all phases (45 tasks) → Complete inventory system

---

## Notes

- All file paths are relative to repository root
- Tasks marked [P] can be parallelized with other [P] tasks in the same phase
- Tasks marked [US#] belong to specific user stories
- Each phase is independently testable
- Refer to `data-model.md` for component schemas
- Refer to `contracts/` for system interfaces
- Refer to `quickstart.md` for code examples

---

**Ready for Implementation**: All tasks defined, dependencies mapped, testing strategy documented.

**Next Step**: Begin Phase 1 (Setup & Infrastructure) or run `/speckit.implement` to start automated implementation.
