# Feature Specification: Inventory System with Item Pickup and Usage

**Feature Branch**: `001-inventory-system`
**Created**: 2025-10-21
**Status**: Draft
**Input**: "Inventory System with Item Pickup and Usage: Players need the ability to collect items found in the dungeon, manage their inventory, and use items like healing potions or equipment."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Item Pickup (Priority: P1)

A player explores the dungeon and finds a healing potion on the floor. They walk adjacent to the item and press the 'G' key to pick it up. The item disappears from the map and appears in their inventory display in the HUD.

**Why this priority**: Core mechanic that enables all other inventory features. Without pickup, no other inventory functionality is possible. This delivers immediate value by allowing resource collection.

**Independent Test**: Can be fully tested by spawning an item, walking next to it, pressing 'G', and verifying the item moves from the map to the inventory. Delivers the value of being able to collect resources found in the dungeon.

**Acceptance Scenarios**:

1. **Given** a healing potion is on the dungeon floor and the player is standing adjacent to it, **When** the player presses the 'G' key, **Then** the potion is removed from the map and added to the player's inventory
2. **Given** the player is standing on a tile with multiple items, **When** the player presses 'G', **Then** the player can choose which item to pick up
3. **Given** the player's inventory is full (20 items), **When** the player attempts to pick up another item, **Then** the system displays "Inventory full" and the item remains on the ground
4. **Given** an item is 2 tiles away from the player, **When** the player presses 'G', **Then** nothing happens (pickup only works for adjacent items)

---

### User Story 2 - Inventory Display (Priority: P1)

As a player explores the dungeon, they can see their current inventory at all times in the HUD panel. The display shows item names, quantities (for stackable items), and which items are currently equipped.

**Why this priority**: Essential for players to know what resources they have available. Without visibility into inventory, players cannot make informed decisions about item usage. This is P1 because it's required alongside pickup to provide a complete MVP experience.

**Independent Test**: Can be tested by adding items to inventory programmatically and verifying they appear in the HUD with correct names, quantities, and equipment status. Delivers value of inventory visibility.

**Acceptance Scenarios**:

1. **Given** the player has 3 healing potions and 1 sword in their inventory, **When** viewing the HUD, **Then** the inventory panel shows "Healing Potion (3)" and "Iron Sword (equipped)"
2. **Given** the player has no items, **When** viewing the HUD, **Then** the inventory panel shows "Inventory: Empty"
3. **Given** the player picks up a new item, **When** the item is added to inventory, **Then** the HUD updates immediately to show the new item
4. **Given** the player has 20/20 inventory slots filled, **When** viewing the HUD, **Then** the inventory shows "Inventory: 20/20 (FULL)" in a warning color

---

### User Story 3 - Consume Healing Potions (Priority: P2)

During combat, a player's health drops to 30/100. They open their inventory, select a healing potion, and press 'U' to use it. Their health increases by 30 points (to 60/100), the potion is consumed and removed from inventory, and a message appears: "You drink the healing potion and recover 30 HP."

**Why this priority**: Adds strategic resource management and improves survivability. P2 because it requires both pickup (P1) and display (P1) to be functional first, but is the most impactful use case for items.

**Independent Test**: Can be tested by reducing player health, using a potion from inventory, and verifying health increase and potion removal. Delivers the value of healing during gameplay.

**Acceptance Scenarios**:

1. **Given** the player has 30/100 HP and a healing potion in inventory, **When** the player selects the potion and presses 'U', **Then** the player's HP increases by 30 (to 60/100) and the potion is removed from inventory
2. **Given** the player has 90/100 HP and uses a 30 HP healing potion, **When** the potion is consumed, **Then** the player's HP becomes 100/100 (capped at maximum) and 20 HP of healing is wasted
3. **Given** the player has 100/100 HP and a healing potion, **When** the player attempts to use the potion, **Then** the system displays "Already at full health" and the potion is not consumed
4. **Given** the player is in combat and uses a healing potion, **When** the potion is consumed, **Then** using the potion consumes the player's turn and enemies can act afterward

---

### User Story 4 - Equip Weapons and Armor (Priority: P3)

A player finds a better sword in the dungeon. They pick it up and press 'U' to use/equip it. The new sword replaces their current weapon, and their attack stat increases. The old weapon is unequipped but remains in inventory (or drops to the ground if inventory is full).

**Why this priority**: Adds character progression and strategic equipment choices. P3 because it builds on item usage mechanics (P2) and provides deeper gameplay but isn't essential for MVP.

**Independent Test**: Can be tested by equipping a weapon, verifying stat changes, and confirming the old weapon's status. Delivers the value of equipment upgrades and stat progression.

**Acceptance Scenarios**:

1. **Given** the player has an Iron Sword (5 ATK) equipped and picks up a Steel Sword (10 ATK), **When** the player equips the Steel Sword, **Then** the player's attack stat increases from 5 to 10 and the Iron Sword is unequipped
2. **Given** the player has a full inventory and equips a new weapon, **When** the old weapon is unequipped, **Then** the old weapon drops to the ground at the player's feet
3. **Given** the player has light armor (+3 DEF) equipped, **When** the player equips heavy armor (+8 DEF), **Then** the player's defense stat increases by 5 (from +3 to +8)
4. **Given** the player selects an already-equipped weapon, **When** the player presses 'U', **Then** the weapon is unequipped and the stat bonus is removed

---

### Edge Cases

- What happens when the player tries to pick up an item but their inventory is full?
  - System displays "Inventory full" message and item remains on ground
- What happens if an item drops on a tile that already has an item?
  - Items stack on the same tile; player can choose which to pick up
- How does the system handle using a consumable item when the player has multiple of that item?
  - One instance is consumed, quantity decreases by 1
- What happens when a player equips an item of the wrong type (e.g., trying to "equip" a potion)?
  - System displays "Cannot equip this item type"
- What happens if a player dies while carrying items?
  - Items drop to the ground at death location (for potential corpse-run mechanic)
- How does the system handle cursed or magical items?
  - Cursed items and magical effects are OUT OF SCOPE for this MVP (see Out of Scope section). All items in the initial version behave predictably with no special magical properties or curses

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST spawn items randomly in dungeon rooms (healing potions in 20-50% of rooms, weapons/armor in 10-20% of rooms)
- **FR-002**: System MUST spawn items when enemies are defeated (30% chance to drop a healing potion, 10% chance to drop equipment)
- **FR-003**: Players MUST be able to pick up items that are adjacent to their current position
- **FR-004**: System MUST enforce a maximum inventory capacity of 20 items
- **FR-005**: System MUST display current inventory contents in the HUD panel
- **FR-006**: System MUST show equipped items with a visual indicator (e.g., "(equipped)" tag)
- **FR-007**: System MUST allow players to consume healing potions to restore health
- **FR-008**: System MUST allow players to equip weapons and armor to modify combat stats
- **FR-009**: Healing potions MUST restore 30 HP when consumed
- **FR-010**: System MUST prevent healing potion usage when player is at full health
- **FR-011**: System MUST remove consumed items from inventory
- **FR-012**: System MUST unequip previous equipment when new equipment is equipped in the same slot
- **FR-013**: System MUST consume the player's turn when using an item during gameplay
- **FR-014**: System MUST display feedback messages for all item interactions (pickup, use, equip, etc.)
- **FR-015**: System MUST persist item data between game turns (items on ground remain until picked up)
- **FR-016**: System MUST cap health restoration at the player's maximum health

### Key Entities

- **Item**: Represents a collectable object in the dungeon. Has attributes: name, type (consumable/equipment), stack count, equipment slot (if applicable), stat modifiers (if equipment), healing value (if consumable)
- **Inventory**: Container for items owned by the player. Has attributes: items list, maximum capacity (20), current count
- **Equipment Slot**: Specific locations where equipment can be worn. Types: main hand (weapon), off hand (shield), head, chest, legs, feet, hands, accessory slots
- **Healing Potion**: Consumable item that restores health. Has attributes: healing amount (30 HP), stack size (up to 99)
- **Weapon**: Equippable item that increases attack stat. Has attributes: attack bonus, weapon type, durability (future consideration)
- **Armor**: Equippable item that increases defense stat. Has attributes: defense bonus, armor slot (head/chest/legs/etc.), movement speed penalty (future consideration)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Players can successfully pick up at least 3 different types of items within the first 5 minutes of gameplay
- **SC-002**: Inventory display updates in real-time (within 100ms) when items are added or removed
- **SC-003**: 100% of healing potion usages correctly restore health and consume the item
- **SC-004**: Players can equip weapons and armor with immediate stat updates (within one game turn)
- **SC-005**: System prevents all invalid item operations (pickup when full, use non-consumables, equip wrong slots) with clear error messages
- **SC-006**: Players can manage inventory through the entire gameplay session without crashes or data loss
- **SC-007**: Item spawning creates findable loot in at least 30% of dungeon rooms
- **SC-008**: 95% of players can understand and use the inventory system without external documentation (based on intuitive keybinds and UI labels)

## Assumptions

1. Inventory UI will be text-based and integrated into the existing Terminal.Gui HUD panel
2. Keybinds follow common roguelike conventions: 'G' for get/pickup, 'U' for use, 'I' for inventory (future)
3. Item stats (attack bonuses, defense bonuses, healing amounts) follow existing combat system calculations
4. Stackable items (potions) stack up to 99 per slot
5. Equipment slots follow standard RPG conventions (weapon, shield, head, chest, legs, etc.)
6. Item persistence follows existing ECS architecture (items are entities with Item components)
7. No item trading or dropping (beyond auto-drop when inventory full) in this initial version
8. Cursed items, magical effects, and item identification are out of scope for MVP

## Dependencies

1. Existing ECS framework (Arch) - already implemented
2. Existing component definitions (Item, Inventory, Consumable, Equippable) - already implemented
3. Existing HUD rendering service - needs extension for inventory display
4. Existing combat system - for equipment stat application
5. Existing input handling system - for new keybinds

## Out of Scope

The following are explicitly NOT included in this feature:

- Item crafting or combining
- Item shops or trading with NPCs
- Item durability or degradation
- Magical item identification mini-game
- Cursed items that cannot be unequipped
- Item sets or bonuses for wearing multiple pieces
- Inventory sorting or filtering
- Item comparison tooltips
- Auto-pickup or loot-all functionality
- Item rarity tiers or quality levels (in this version - basic items only)
