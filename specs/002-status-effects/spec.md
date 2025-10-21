# Feature Specification: Status Effects System

**Feature Branch**: `002-status-effects`
**Created**: 2025-10-21
**Status**: Draft
**Input**: "Status Effects System: Players and enemies can be affected by temporary status effects like poison, speed boosts, strength buffs, and debuffs. These effects last for a certain number of turns and modify combat stats, health regeneration, or other gameplay mechanics. Consumable items can apply buffs, enemies can inflict debuffs through attacks, and the HUD displays active effects with remaining duration."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Poison Damage Over Time (Priority: P1)

A player fights a Goblin and gets hit. The attack inflicts poison, causing the player to lose 3 HP at the start of each turn for 5 turns. The HUD shows "Poisoned (5)" indicating the effect and remaining duration. The player can cure the poison by drinking an Antidote Potion, immediately removing the effect and stopping further damage.

**Why this priority**: Core debuff mechanic that adds consequence to combat. Demonstrates the fundamental status effect system (application, duration tracking, tick damage, display, and removal). Without this working, no other status effects can function.

**Independent Test**: Can be fully tested by having an enemy inflict poison, observing turn-by-turn damage, checking HUD display updates, and using an antidote to remove it. Delivers immediate tactical value by making combat more dangerous and consumables more valuable.

**Acceptance Scenarios**:

1. **Given** a player at 100/100 HP fights a Goblin, **When** the Goblin's attack hits, **Then** the player is poisoned for 5 turns and loses 3 HP immediately
2. **Given** a player is poisoned with 3 turns remaining, **When** the player's turn begins, **Then** 3 HP is lost, the HUD shows "Poisoned (2)", and the duration decrements
3. **Given** a player is poisoned with 1 turn remaining, **When** the turn ends, **Then** the poison effect is automatically removed after final damage
4. **Given** a player is poisoned, **When** the player drinks an Antidote Potion, **Then** the poison is immediately removed and no further damage occurs

---

### User Story 2 - Buff Effects from Consumables (Priority: P1)

During combat, a player's health drops dangerously low. They drink a Strength Potion, gaining +5 attack for 10 turns. The HUD displays "Strength (10)" in green. The player's attacks now deal significantly more damage. After 10 turns, the buff expires automatically and attack returns to normal.

**Why this priority**: Core buff mechanic that makes consumable items more strategic. Essential MVP functionality alongside debuffs. Creates meaningful tactical choices (use buff now vs. save for boss).

**Independent Test**: Can be tested by using a buff potion, verifying stat increase in combat, watching duration countdown, and confirming automatic expiration. Delivers value by making consumables beyond healing potions useful.

**Acceptance Scenarios**:

1. **Given** a player with 10 base attack, **When** they drink a Strength Potion, **Then** attack increases to 15 and HUD shows "Strength (10)"
2. **Given** a player has the Strength buff active, **When** they attack an enemy, **Then** damage calculation uses the buffed attack value (15 instead of 10)
3. **Given** a Strength buff has 1 turn remaining, **When** that turn ends, **Then** the buff expires, attack returns to 10, and HUD no longer shows the effect
4. **Given** a player already has a Strength buff active, **When** they drink another Strength Potion, **Then** the duration resets to 10 turns (does not stack attack bonus)

---

### User Story 3 - Speed Modification Effects (Priority: P2)

A player equips a Ring of Speed (+10 speed) and drinks a Haste Potion (+20 speed for 8 turns). The combined speed boost allows the player to act twice before slow enemies act once. The HUD shows "Haste (8)" in green. When the haste expires, speed returns to the equipment-only bonus, and turn order adjusts accordingly.

**Why this priority**: Adds tactical depth by modifying turn order. P2 because it builds on P1 buff mechanics but requires understanding of the actor/energy system. Provides strategic advantage against tough enemies.

**Independent Test**: Can be tested by applying speed buffs, observing turn order changes, and verifying energy accumulation rate increases. Delivers value by creating tactical positioning advantages.

**Acceptance Scenarios**:

1. **Given** a player with 100 base speed, **When** they drink a Haste Potion, **Then** speed increases to 120 and they gain turns more frequently
2. **Given** a player has Haste active and enemies have 80 speed, **When** combat occurs, **Then** the player acts approximately 1.5x more frequently than enemies
3. **Given** a player is slowed by an enemy spell (-30 speed for 6 turns), **When** the effect is applied, **Then** speed drops to 70 and enemies act more frequently
4. **Given** a player has both Haste (+20) and Slow (-30) active simultaneously, **When** effects are calculated, **Then** the net result is -10 speed (effects stack)

---

### User Story 4 - Multiple Effect Management (Priority: P2)

A player is simultaneously affected by three status effects: Poisoned (-3 HP per turn, 4 turns left), Strength (+5 attack, 7 turns left), and Regeneration (+2 HP per turn, 10 turns left). The HUD displays all three effects with their remaining durations. Each turn, poison applies -3 HP, then regeneration applies +2 HP, for a net -1 HP per turn. After 4 turns, poison expires but strength and regeneration remain active.

**Why this priority**: Real gameplay will involve multiple simultaneous effects. P2 because it requires the basic effect system to work first, but is essential for rich tactical gameplay. Demonstrates effect stacking and interaction.

**Independent Test**: Can be tested by applying multiple effects, verifying each ticks independently, checking HUD displays all effects, and confirming proper expiration timing. Delivers value by enabling complex tactical situations.

**Acceptance Scenarios**:

1. **Given** a player has 3 different status effects active, **When** viewing the HUD, **Then** all 3 effects are displayed with their names and remaining durations
2. **Given** a player has Poison (-3 HP) and Regeneration (+2 HP) active, **When** a turn passes, **Then** poison damage applies first, then regeneration healing, for net -1 HP
3. **Given** a player has 5 status effects active and the oldest one expires, **When** the turn ends, **Then** only the expired effect is removed and the other 4 remain
4. **Given** a player has Strength (+5 ATK) and Weakness (-3 ATK) simultaneously, **When** calculating attack, **Then** both modifiers apply for net +2 attack

---

### User Story 5 - Enemy Status Effects (Priority: P3)

A player encounters a Toxic Spider enemy. When the spider attacks, there's a 40% chance to inflict poison. The player must decide whether to risk continued combat or retreat. Some enemies (Shamans) can cast buff spells on themselves or allies, temporarily increasing their combat effectiveness.

**Why this priority**: Makes enemies more interesting and dangerous. P3 because it requires P1/P2 infrastructure but adds variety rather than core functionality. Creates tactical decisions and enemy differentiation.

**Independent Test**: Can be tested by fighting specific enemy types, observing effect application probability, and verifying enemy-inflicted effects work the same as player effects. Delivers value by increasing combat variety and difficulty.

**Acceptance Scenarios**:

1. **Given** a Toxic Spider attacks the player, **When** the attack hits, **Then** there's a 40% chance the player becomes poisoned for 5 turns
2. **Given** a Shaman enemy casts "Berserk" on an Orc ally, **When** the spell succeeds, **Then** the Orc gains +8 attack for 6 turns
3. **Given** a player fights a buffed enemy, **When** dealing damage, **Then** the enemy's buffed stats are used in combat calculations
4. **Given** an enemy has a debuff (Weakness) applied by a player spell, **When** the enemy attacks, **Then** the reduced attack value is used

---

### Edge Cases

- What happens when a player tries to apply the same buff twice (e.g., two Strength Potions)?
  - Duration refreshes to maximum, buff value does not stack (prevents abuse)

- What happens if a status effect would reduce a stat below zero (e.g., Weakness -10 on 5 base attack)?
  - Stat is capped at minimum value (1 for attack/defense/speed, 0 for other values)

- What happens when a player or enemy dies while having status effects active?
  - All status effects are immediately cleared (no persistent effects after death)

- What happens if the HUD needs to display more than 5 active status effects?
  - HUD shows the 5 effects with longest remaining duration, others are still active but not displayed

- What happens when two effects modify the same stat in opposite directions?
  - Both modifiers apply (additive stacking), net effect can be positive, negative, or neutral

- What happens if an effect duration is set to 0 or negative turns?
  - Effect is invalid and is not applied (minimum duration is 1 turn)

- What happens when a Regeneration effect would heal beyond maximum health?
  - Health is capped at maximum, excess healing is wasted

- What happens when a status effect is meant to be permanent (like equipment bonuses)?
  - Equipment bonuses are NOT status effects (separate system), status effects always have finite duration

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support at least 8 distinct status effect types: Poison, Regeneration, Strength, Weakness, Haste, Slow, Defense Boost, Defense Break
- **FR-002**: System MUST track duration in turns for each active status effect (minimum 1 turn, maximum 99 turns)
- **FR-003**: System MUST decrement all status effect durations by 1 at the end of each entity's turn
- **FR-004**: System MUST automatically remove status effects when duration reaches 0
- **FR-005**: System MUST apply damage-over-time effects (Poison, Bleed) at the start of the affected entity's turn
- **FR-006**: System MUST apply heal-over-time effects (Regeneration) at the start of the affected entity's turn
- **FR-007**: System MUST modify combat stats (attack, defense, speed) based on active buff/debuff effects
- **FR-008**: System MUST display active player status effects in the HUD with effect name and remaining duration
- **FR-009**: System MUST support multiple simultaneous status effects on a single entity (up to 10 concurrent effects)
- **FR-010**: System MUST handle stacking rules: same effect refreshes duration but does not stack magnitude
- **FR-011**: System MUST allow consumable items to apply positive status effects when used
- **FR-012**: System MUST allow enemy attacks to inflict negative status effects with configurable probability
- **FR-013**: System MUST provide feedback messages when status effects are applied ("You are poisoned!")
- **FR-014**: System MUST provide feedback messages when status effects expire ("Strength buff has worn off.")
- **FR-015**: System MUST calculate net stat modifiers when multiple effects affect the same stat (additive stacking)
- **FR-016**: System MUST prevent stats from going below minimum values due to debuffs (attack/defense/speed minimum 1)
- **FR-017**: System MUST clear all status effects when an entity dies
- **FR-018**: System MUST support antidote items that remove specific effect types (e.g., Cure Poison removes Poison)

### Key Entities

- **Status Effect**: Represents a temporary modifier affecting an entity. Has attributes: type (Poison/Strength/etc.), magnitude (damage/stat change per turn), duration in turns remaining, effect category (buff/debuff/damage/healing), application source (item/enemy attack), visual color (red for debuffs, green for buffs, purple for neutral)

- **Effect Type**: Enumeration of all possible status effect types. Categories:
  - Damage-over-time: Poison, Bleed, Burning
  - Healing-over-time: Regeneration, Blessed
  - Stat buffs: Strength (+attack), Haste (+speed), Iron Skin (+defense)
  - Stat debuffs: Weakness (-attack), Slow (-speed), Fragile (-defense)

- **Status Effect Component**: Container attached to entities (player or enemies) holding a list of active status effects. Has attributes: active effects list, maximum concurrent effects (10), total stat modifiers (cached for performance)

- **Antidote/Cure Item**: Consumable item that removes specific status effects. Has attributes: target effect type(s) to remove, removal scope (single effect or all negative effects)

- **Effect Application**: Event or action that creates a new status effect. Has attributes: effect type, magnitude, duration, probability of application (100% for potions, 10-50% for enemy attacks), target entity

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Players can apply and observe at least 6 different status effect types within a single combat encounter
- **SC-002**: Status effect durations count down correctly 100% of the time (no bugs with turn tracking)
- **SC-003**: HUD displays active effects with remaining duration, updating every turn in real-time
- **SC-004**: 100% of status effects automatically expire when duration reaches 0 without manual intervention
- **SC-005**: Combat stat calculations correctly apply all active buffs and debuffs (verified through damage testing)
- **SC-006**: Players can successfully use antidote items to remove poison with 100% success rate
- **SC-007**: Multiple status effects (up to 10) can be active simultaneously without system errors or performance degradation
- **SC-008**: Enemy-inflicted status effects work identically to player-applied effects (verified through combat testing)
- **SC-009**: Status effects create tactical decision points in 80%+ of combat encounters (use buff now vs. later, prioritize curing poison)
- **SC-010**: System handles edge cases gracefully (same buff applied twice, stat going negative, effect on dead entity) with no crashes

## Assumptions

1. Status effects use the existing turn-based energy system from the Actor component
2. HUD has space for displaying at least 5 concurrent status effects
3. Effect magnitude values are fixed per effect type (Poison always deals 3 damage, Strength always grants +5 attack)
4. Status effects apply to both player and enemies using the same system
5. Effect visual indicators use text-based display with color coding (green buffs, red debuffs)
6. Consumable items from spec-001 are extended to include buff potions and antidotes
7. Enemy attacks can have attached effect application data (probability and effect type)
8. Maximum 10 concurrent effects per entity is sufficient for all gameplay scenarios
9. Effect stacking is always additive (two +5 attack buffs = +10 attack if both are active)
10. No status effect immunity or resistance mechanics in MVP version

## Dependencies

1. Actor System (for turn-based duration tracking)
2. Combat System (for stat modification and damage calculation)
3. Inventory System (spec-001 - for consumable items that apply effects)
4. HUD Service (for displaying active effects)
5. ItemSpawnSystem (for spawning new potion types)
6. Health Component (for damage-over-time and healing-over-time effects)

## Out of Scope

The following are explicitly NOT included in this feature:

- Effect immunity or resistance mechanics (e.g., "Orcs are immune to poison")
- Status effect cleansing spells or abilities
- Aura effects that affect multiple entities in an area
- Conditional effects (e.g., "Double damage when below 50% HP")
- Effect synergies (e.g., "Burning deals extra damage when target is Wet")
- Visual particle effects or animations
- Sound effects for status application/expiration
- Detailed effect descriptions or tooltips
- Stackable magnitude (multiple Strength buffs do NOT combine to +10, +15, etc.)
- Enemy-only or player-only status effects (all effects work for both)
- Effect transfer mechanics (passing poison to another entity)
- Timed effects based on real-time duration (all effects are turn-based)
