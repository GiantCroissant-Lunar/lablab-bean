# Research: Inventory System Implementation

**Feature**: Inventory System with Item Pickup and Usage
**Date**: 2025-10-21
**Phase**: 0 - Research & Technical Decisions

## Overview

This document consolidates research findings and technical decisions for implementing the inventory system in the dungeon crawler game. All decisions align with the existing ECS architecture and established patterns.

## Research Areas

### 1. ECS Component Design for Items

**Research Question**: How should items be represented in the Arch ECS framework?

**Decision**: Use composition-based component design

- **Item** component: Base data (name, glyph, description)
- **Consumable** component: Healing value, usage effect
- **Equippable** component: Slot type, stat modifiers
- **Stackable** component: Current stack count, max stack size

**Rationale**:

- Follows existing pattern used for Actor/Player/Enemy components
- Enables flexible item types without inheritance
- Arch ECS queries can efficiently filter by component combinations
- Easy to add new item behaviors (e.g., Throwable, Readable) later

**Alternatives Considered**:

- Single Item component with all properties → Rejected: Violates ECS principles, wastes memory
- Item type enum with switch statements → Rejected: Not extensible, harder to maintain

### 2. Inventory Storage Pattern

**Research Question**: How should the player's inventory be stored and managed?

**Decision**: Use Inventory component with List<EntityReference>

- Inventory component attached to player entity
- Stores entity references to item entities
- Items remain as entities even when in inventory
- Maximum capacity enforced at system level

**Rationale**:

- Items maintain their full component data while in inventory
- Enables queries like "find all consumables in inventory"
- Consistent with ECS philosophy (entities are data containers)
- Supports future features (item inspection, sorting, filtering)

**Alternatives Considered**:

- Separate inventory data structure → Rejected: Breaks ECS patterns, duplicates data
- Destroy items and recreate on drop → Rejected: Loses item state, inefficient

### 3. Item Spawning Strategy

**Research Question**: How should items be spawned in dungeons?

**Decision**: Implement ItemSpawnSystem with weighted random selection

- Spawn during map generation (room-based)
- Spawn on enemy death (loot drops)
- Use spawn tables with item types and probabilities
- Items spawn as entities with Position component

**Rationale**:

- Integrates with existing MapGenerator workflow
- Weighted tables allow easy balancing (adjust drop rates)
- Position component enables spatial queries (items near player)
- Follows pattern used for enemy spawning

**Alternatives Considered**:

- Fixed item placements → Rejected: Not roguelike, no replayability
- Procedural item generation → Rejected: Out of scope for MVP

### 4. Equipment Slot System

**Research Question**: How should equipment slots be managed?

**Decision**: Use EquipmentSlots component with Dictionary<SlotType, EntityReference?>

- Enum for slot types (MainHand, OffHand, Head, Chest, Legs, Feet)
- Each slot stores reference to equipped item entity (or null)
- Equippable component defines which slot(s) an item can use

**Rationale**:

- Type-safe slot management
- Easy to query "what's in main hand?"
- Supports future multi-slot items (two-handed weapons)
- Clear ownership model (item in slot = equipped)

**Alternatives Considered**:

- Separate component per slot → Rejected: Too many components, harder to iterate
- Single "equipped item" reference → Rejected: Doesn't support multiple equipment pieces

### 5. Item Usage System

**Research Question**: How should item usage (consume, equip) be handled?

**Decision**: Implement InventorySystem with command pattern

- Commands: PickupItem, UseItem, DropItem, EquipItem, UnequipItem
- System validates preconditions (inventory space, item type, etc.)
- System applies effects (heal player, modify stats, etc.)
- System updates game state (remove consumed items, swap equipment)

**Rationale**:

- Clear separation of concerns (validation → execution)
- Easy to test each command independently
- Supports undo/redo in future (edit mode)
- Consistent with turn-based game loop

**Alternatives Considered**:

- Item components with behavior methods → Rejected: Violates ECS (logic in data)
- Event-based system → Rejected: Over-engineered for turn-based game

### 6. HUD Integration

**Research Question**: How should inventory be displayed in Terminal.Gui HUD?

**Decision**: Extend HudService with inventory panel

- Add FrameView for inventory display
- Show item list with quantities and equipment status
- Update on inventory changes (reactive pattern)
- Use existing color scheme and layout conventions

**Rationale**:

- Consistent with existing HUD design (health bar, stats, messages)
- Terminal.Gui FrameView provides clean UI container
- Reactive updates ensure UI stays in sync
- Minimal changes to existing HudService structure

**Alternatives Considered**:

- Separate inventory window → Rejected: Clutters screen, not roguelike convention
- Full-screen inventory overlay → Rejected: Overkill for 20-item limit

### 7. Input Handling

**Research Question**: How should inventory keybinds be handled?

**Decision**: Extend DungeonCrawlerService input handling

- 'G' key: Pickup adjacent item
- 'U' key: Use selected item (opens item selection if multiple)
- 'I' key: Toggle inventory focus (future enhancement)
- Input processed during player turn only

**Rationale**:

- Follows roguelike conventions (NetHack, DCSS, etc.)
- Integrates with existing turn-based input system
- Clear user feedback via message log
- Non-conflicting with existing keybinds

**Alternatives Considered**:

- Mouse-based inventory → Rejected: Not accessible in pure terminal
- Number keys for quick-use → Rejected: Adds complexity, not MVP

### 8. Item-Ground Interaction

**Research Question**: How should items on the ground be managed?

**Decision**: Items on ground are entities with Position + Item components

- Rendered on map like other entities
- Spatial queries find items at position
- Multiple items can occupy same tile (stacking)
- Pickup shows selection if multiple items present

**Rationale**:

- Consistent with entity-based design
- Leverages existing rendering pipeline
- Spatial queries already optimized in Arch
- Supports "corpse with loot" scenarios

**Alternatives Considered**:

- Separate ground item collection → Rejected: Duplicates spatial data
- Tile-based item storage → Rejected: Doesn't fit ECS model

## Technology Stack Validation

### Existing Dependencies (No Changes Required)

- ✅ **Arch ECS (1.3.3)**: Supports all required component patterns
- ✅ **GoRogue (3.0.0-beta09)**: Not needed for inventory (spatial queries via Arch)
- ✅ **Terminal.Gui (2.0.0-pre.2)**: FrameView and ListView sufficient for UI
- ✅ **SadConsole (10.0.3)**: Rendering patterns already established

### New Dependencies

- ❌ None required - all features achievable with existing stack

## Performance Considerations

### Expected Load

- 20-100 item entities per playthrough
- 20 items max in player inventory
- 1-10 items visible on screen at once
- Inventory queries: <1ms (Arch ECS optimized)

### Optimization Strategy

- Use Arch queries with component filters (O(n) over matching entities only)
- Cache inventory contents in Inventory component (avoid repeated queries)
- Update HUD only on inventory changes (event-driven, not per-frame)
- Spatial queries limited to player's FOV radius

**Conclusion**: No performance concerns expected. ECS architecture handles this scale trivially.

## Integration Points

### Existing Systems to Extend

1. **GameStateManager**: Add inventory system registration and initialization
2. **HudService**: Add inventory display panel
3. **DungeonCrawlerService**: Add inventory input handling
4. **MapGenerator**: Add item spawning during generation
5. **CombatSystem**: Integrate with equipment stat modifiers

### New Systems to Create

1. **InventorySystem**: Core inventory logic (pickup, use, drop, equip)
2. **ItemSpawnSystem**: Item generation and placement

### Data Dependencies

- Player entity must have Inventory and EquipmentSlots components
- Item entities must have Position (if on ground) or be referenced in Inventory
- Combat stats (Attack, Defense) must read from EquipmentSlots

## Risk Assessment

### Low Risk

- ✅ Component design: Follows established patterns
- ✅ System integration: Additive only, no breaking changes
- ✅ UI rendering: Extends existing HUD service
- ✅ Performance: Well within ECS capabilities

### Medium Risk

- ⚠️ **Equipment stat application**: Must integrate with CombatSystem
  - Mitigation: Add stat calculation helper that reads EquipmentSlots
- ⚠️ **Item selection UI**: Multiple items on same tile needs UI
  - Mitigation: Simple numbered list in message log (roguelike standard)

### High Risk

- ❌ None identified

## Testing Strategy

### Unit Tests

- Component creation and initialization
- Inventory capacity enforcement
- Item stacking logic
- Equipment slot validation
- Stat modifier calculations

### Integration Tests

- Pickup item from ground → appears in inventory
- Use consumable → health increases, item removed
- Equip weapon → stats updated, old weapon unequipped
- Drop item → appears on ground at player position

### Manual Tests

- Full gameplay loop with inventory usage
- Edge cases (full inventory, multiple items on tile, etc.)
- UI responsiveness and clarity
- Cross-platform testing (Terminal.Gui on Windows/Linux/macOS)

## Open Questions

### Resolved

- ✅ How to represent items? → ECS entities with components
- ✅ How to store inventory? → Component with entity references
- ✅ How to handle equipment? → Slot-based system with dictionary
- ✅ How to display in HUD? → Extend existing HudService

### Deferred (Out of Scope)

- Item durability and degradation
- Item identification and curses
- Item crafting and combining
- Save/load serialization (future enhancement)

## Next Steps

Proceed to **Phase 1: Design & Contracts** to create:

1. `data-model.md` - Detailed component and entity schemas
2. `contracts/` - System interfaces and command definitions
3. `quickstart.md` - Developer guide for using inventory system

---

**Research Complete**: All technical decisions documented and validated against existing architecture.
