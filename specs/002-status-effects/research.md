# Research: Status Effects System Implementation

**Feature**: Status Effects System
**Date**: 2025-10-21
**Phase**: 0 - Research & Technical Decisions

## Overview

This document consolidates research findings and technical decisions for implementing the status effects system in the dungeon crawler game. All decisions align with the existing ECS architecture and integrate with the inventory system (spec-001).

## Research Areas

### 1. Status Effect Data Model

**Research Question**: How should status effects be represented in the Arch ECS framework?

**Decision**: Use composition-based component design with effect instances

- **StatusEffect** struct: Individual effect instance (type, magnitude, duration, source)
- **StatusEffects** component: Container attached to entities holding list of active effects
- **EffectType** enum: All possible effect types (Poison, Strength, Haste, etc.)
- **EffectCategory** enum: Grouping (Buff, Debuff, DamageOverTime, HealingOverTime)

**Rationale**:

- Follows existing ECS patterns (similar to Inventory component holding item references)
- Each effect is a value type (struct) for performance
- List-based storage enables multiple simultaneous effects
- Enum-based typing allows efficient switch statements and queries

**Alternatives Considered**:

- Separate component per effect type → Rejected: Too many components, hard to iterate
- Effect entities with references → Rejected: Overhead for temporary data, complicates cleanup
- Bitflags for active effects → Rejected: Can't store magnitude/duration per effect

### 2. Effect Duration Tracking

**Research Question**: How should turn-based duration be tracked and decremented?

**Decision**: Integrate with existing ActorSystem turn processing

- Duration stored as integer (turns remaining) in StatusEffect struct
- ActorSystem calls StatusEffectSystem.ProcessEffects() at turn start
- StatusEffectSystem decrements all durations, removes expired effects
- Effects tick (apply damage/healing) BEFORE duration decrement

**Rationale**:

- Leverages existing turn-based energy system
- Consistent with how other turn-based mechanics work
- Clear execution order: Tick effects → Decrement duration → Remove if zero
- No additional timing infrastructure needed

**Alternatives Considered**:

- Separate effect timer system → Rejected: Duplicates turn tracking logic
- Duration decrement at turn end → Rejected: Confusing for players (effect shows 0 turns but still active)
- Real-time duration → Rejected: Out of scope, turn-based is simpler

### 3. Stat Modification Strategy

**Research Question**: How should effects modify combat stats (attack, defense, speed)?

**Decision**: Calculate total modifiers on-demand from active effects

- StatusEffectSystem provides `CalculateStatModifiers()` method
- Returns tuple of (attackMod, defenseMod, speedMod) by summing all active effects
- CombatSystem calls this before damage calculation
- ActorSystem calls this before energy accumulation

**Rationale**:

- No need to cache/invalidate stat values
- Always accurate (reflects current active effects)
- Simple additive stacking (two +5 attack effects = +10 total)
- Performance acceptable (<1ms for 10 effects)

**Alternatives Considered**:

- Cache modifiers in StatusEffects component → Rejected: Cache invalidation complexity
- Modify base stats directly → Rejected: Hard to reverse, error-prone
- Event-based stat recalculation → Rejected: Over-engineered for turn-based game

### 4. Effect Application Mechanism

**Research Question**: How should effects be applied to entities (from items, attacks, etc.)?

**Decision**: StatusEffectSystem provides `ApplyEffect()` method

- Takes target entity, effect type, magnitude, duration, source
- Checks for existing effect of same type (refreshes duration if found)
- Validates duration (1-99 turns), magnitude (non-zero)
- Adds effect to StatusEffects.ActiveEffects list
- Returns success/failure with feedback message

**Rationale**:

- Centralized application logic (single source of truth)
- Consistent behavior regardless of source (item, attack, spell)
- Stacking rules enforced in one place
- Easy to extend with additional validation

**Alternatives Considered**:

- Effects apply themselves → Rejected: Violates ECS (logic in data)
- Separate application methods per source → Rejected: Code duplication
- Event-based application → Rejected: Unnecessary complexity

### 5. Effect Stacking Rules

**Research Question**: How should multiple applications of the same effect be handled?

**Decision**: Same effect type refreshes duration, does not stack magnitude

- If Strength (+5 ATK, 10 turns) is active and player drinks another Strength Potion
- Duration resets to 10 turns, attack bonus remains +5 (not +10)
- Different effect types stack additively (Strength +5 + Haste +20 speed = both active)

**Rationale**:

- Prevents abuse (spam potions for massive stat boost)
- Intuitive for players (potion extends duration, not power)
- Simplifies balance (effect magnitude is fixed)
- Matches common roguelike conventions

**Alternatives Considered**:

- Full stacking → Rejected: Balance nightmare, encourages hoarding
- No stacking (ignore new application) → Rejected: Wastes consumables
- Diminishing returns → Rejected: Too complex for MVP

### 6. Damage/Healing Over Time Processing

**Research Question**: When and how should DoT/HoT effects apply their damage/healing?

**Decision**: Process at START of affected entity's turn

- Order: DoT effects → HoT effects → Duration decrement → Turn actions
- Poison deals damage before player can act (can die from poison)
- Regeneration heals before turn (can save from death)
- Net effect visible immediately (Poison -3 + Regen +2 = -1 HP per turn)

**Rationale**:

- Clear, predictable timing
- Player sees effect before making decisions
- Matches roguelike conventions (NetHack, DCSS)
- DoT before HoT prevents instant-death edge cases

**Alternatives Considered**:

- Process at turn end → Rejected: Player acts before seeing effect, confusing
- Process between turns → Rejected: No clear "between" in turn-based system
- HoT before DoT → Rejected: Can create instant-death scenarios

### 7. HUD Display Strategy

**Research Question**: How should active effects be displayed in the Terminal.Gui HUD?

**Decision**: Add status effects panel to HUD

- Show up to 5 effects (longest duration first if >5 active)
- Format: "Effect Name (Duration)" with color coding
- Green for buffs, red for debuffs, purple for neutral
- Update every turn after effect processing

**Rationale**:

- Consistent with existing HUD design (health bar, stats, inventory)
- Color coding provides instant visual feedback
- Duration display helps tactical planning
- 5-effect limit prevents HUD clutter

**Alternatives Considered**:

- Show all effects → Rejected: Can overflow HUD with 10 effects
- Icon-based display → Rejected: Limited ASCII character set
- Separate effects window → Rejected: Clutters screen

### 8. Effect Removal (Antidotes)

**Research Question**: How should antidote items remove specific effects?

**Decision**: Extend consumable items with effect removal capability

- Add `RemovesEffectType` property to Consumable component
- InventorySystem.UseConsumable() checks for removal property
- Calls StatusEffectSystem.RemoveEffect(entity, effectType)
- Provides feedback ("Poison cured!")

**Rationale**:

- Reuses existing consumable item infrastructure
- Simple property-based configuration
- Can target specific effect types or categories
- Integrates cleanly with inventory system

**Alternatives Considered**:

- Separate antidote item type → Rejected: Duplicates consumable logic
- Automatic effect removal → Rejected: Removes tactical choice
- Cure-all potions only → Rejected: Less interesting than targeted cures

## Technology Stack Validation

### Existing Dependencies (No Changes Required)

- ✅ **Arch ECS (1.3.3)**: Supports all required component patterns
- ✅ **Terminal.Gui (2.0.0-pre.2)**: Color-coded text display sufficient
- ✅ **Existing Systems**: Actor, Combat, Inventory systems provide integration points

### New Dependencies

- ❌ None required - all features achievable with existing stack

## Performance Considerations

### Expected Load

- 10-30 entities with active effects simultaneously
- 1-10 effects per entity (max 10)
- Effect processing per turn: ~30 entities × ~5 effects = 150 operations
- Stat calculation: ~10 combat calculations per turn

### Optimization Strategy

- Use struct for StatusEffect (value type, stack-allocated)
- List-based storage (O(n) iteration, acceptable for n≤10)
- Cache-free stat calculation (recalculate on-demand, <1ms)
- Batch effect processing (all entities in single system call)

**Conclusion**: No performance concerns. Turn-based gameplay has no real-time pressure.

## Integration Points

### Existing Systems to Extend

1. **ActorSystem**: Call StatusEffectSystem at turn start for duration tracking
2. **CombatSystem**: Query stat modifiers before damage calculation
3. **InventorySystem**: Apply effects when using buff potions, remove effects with antidotes
4. **HudService**: Display active effects panel
5. **GameStateManager**: Register StatusEffectSystem

### New Systems to Create

1. **StatusEffectSystem**: Core effect logic (apply, tick, remove, calculate modifiers)

### Data Dependencies

- Entities must have StatusEffects component to be affected
- Player and enemies both use same StatusEffects component
- Consumable items extended with effect application data
- Enemy attacks extended with effect application probability

## Risk Assessment

### Low Risk

- ✅ Component design: Follows established patterns
- ✅ System integration: Clear integration points identified
- ✅ Performance: Well within acceptable limits
- ✅ UI rendering: Extends existing HUD service

### Medium Risk

- ⚠️ **Effect timing complexity**: Multiple effects interacting
  - Mitigation: Clear execution order documented, comprehensive testing
- ⚠️ **Stat calculation integration**: Must not break existing combat
  - Mitigation: Additive modifiers only, test with/without effects

### High Risk

- ❌ None identified

## Testing Strategy

### Unit Tests

- Effect application and stacking rules
- Duration tracking and expiration
- Stat modifier calculation (multiple effects)
- Effect removal (antidotes)
- Edge cases (negative stats, zero duration, dead entity)

### Integration Tests

- Effect applied from consumable → stat changes → combat damage changes
- Effect applied from enemy attack → player takes DoT damage → uses antidote
- Multiple effects active → all tick correctly → expire independently
- Effect duration tracking across multiple turns

### Manual Tests

- Full combat with poison, buffs, and healing
- Edge cases (10 simultaneous effects, stat going negative, etc.)
- HUD display with various effect combinations
- Cross-platform testing (Terminal.Gui on Windows/Linux/macOS)

## Effect Type Definitions (MVP)

### Damage Over Time

- **Poison**: -3 HP per turn, 5 turns, red
- **Bleed**: -2 HP per turn, 8 turns, dark red
- **Burning**: -4 HP per turn, 3 turns, orange

### Healing Over Time

- **Regeneration**: +2 HP per turn, 10 turns, light green
- **Blessed**: +1 HP per turn, 20 turns, yellow

### Stat Buffs

- **Strength**: +5 attack, 10 turns, green
- **Haste**: +20 speed, 8 turns, cyan
- **Iron Skin**: +5 defense, 12 turns, blue

### Stat Debuffs

- **Weakness**: -3 attack, 6 turns, red
- **Slow**: -30 speed, 6 turns, brown
- **Fragile**: -3 defense, 6 turns, dark red

## Open Questions

### Resolved

- ✅ How to represent effects? → Struct instances in component list
- ✅ How to track duration? → Integrate with ActorSystem turn processing
- ✅ How to modify stats? → Calculate on-demand from active effects
- ✅ How to handle stacking? → Refresh duration, don't stack magnitude
- ✅ How to display in HUD? → Color-coded text panel with duration

### Deferred (Out of Scope)

- Effect immunity/resistance mechanics
- Visual particle effects or animations
- Effect synergies (burning + wet = extra damage)
- Aura effects (affect multiple entities)
- Conditional effects (trigger on low health)

## Next Steps

Proceed to **Phase 1: Design & Contracts** to create:

1. `data-model.md` - Detailed component and effect schemas
2. `contracts/` - StatusEffectSystem interface
3. `quickstart.md` - Developer guide for using status effects

---

**Research Complete**: All technical decisions documented and validated against existing architecture.
