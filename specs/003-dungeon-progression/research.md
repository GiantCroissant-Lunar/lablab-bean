# Research: Dungeon Progression System Implementation

**Feature**: Dungeon Progression System
**Date**: 2025-10-21
**Phase**: 0 - Research & Technical Decisions

## Overview

This document consolidates research findings and technical decisions for implementing the multi-level dungeon progression system. All decisions align with the existing ECS architecture and dungeon generation patterns.

## Research Areas

### 1. Level State Persistence Strategy

**Research Question**: How should dungeon level state be stored and restored when transitioning between levels?

**Decision**: In-memory level cache with serializable state snapshots

- **DungeonLevel** class: Contains map data, entity snapshots, metadata
- **LevelManager** service: Dictionary<int, DungeonLevel> cache
- **Entity snapshots**: Serialize component data when leaving level
- **Restoration**: Recreate entities from snapshots when returning

**Rationale**:

- Maintains exact dungeon state (enemy positions, items, etc.)
- Efficient for 20-30 levels (reasonable memory footprint)
- Enables save/load in future (serialize cache to JSON)
- Follows existing game state management patterns

**Alternatives Considered**:

- Regenerate levels on return → Rejected: Loses state, confusing for players
- Full ECS world serialization → Rejected: Too heavyweight, complex
- Database storage → Rejected: Overkill for single-player game

### 2. Staircase Placement Algorithm

**Research Question**: How should up/down staircases be placed in generated dungeons?

**Decision**: Place staircases in distant rooms during map generation

- Downward staircase ('>'): Placed in room farthest from start
- Upward staircase ('<'): Placed near player spawn (except level 1)
- Minimum distance: 30+ tiles apart
- Fallback: Largest room if distance constraint fails

**Rationale**:

- Encourages full dungeon exploration
- Prevents trivial level skipping
- Clear visual distinction (> vs <)
- Integrates with existing room-based generation

**Alternatives Considered**:

- Random room placement → Rejected: Can spawn too close together
- Fixed positions → Rejected: Predictable, reduces exploration
- Multiple staircases → Rejected: Adds complexity, reduces challenge

### 3. Difficulty Scaling Formula

**Research Question**: How should enemy stats scale with dungeon depth?

**Decision**: Exponential scaling with level cap

- **Formula**: `scaledStat = baseStat × (1.12 ^ level)`
- **Example**: Level 1 Goblin (20 HP, 5 ATK) → Level 5 (32 HP, 9 ATK)
- **Cap**: Level 30 (prevents integer overflow, maintains challenge)
- **Applies to**: Health, Attack, Defense

**Rationale**:

- 12% per level feels meaningful but not overwhelming
- Exponential curve creates increasing challenge
- Level 30 cap prevents stat explosion
- Matches roguelike conventions (DCSS, NetHack)

**Alternatives Considered**:

- Linear scaling (+10% per level) → Rejected: Too slow, boring
- Higher exponent (1.2^level) → Rejected: Too steep, frustrating
- Per-enemy-type scaling → Rejected: Balancing nightmare

### 4. Loot Scaling Strategy

**Research Question**: How should loot quality/quantity improve with depth?

**Decision**: Increase drop rates, not item power

- **Drop rate formula**: `dropRate = min(0.10 + (level × 0.05), 0.60)`
- **Example**: Level 1 (10% equipment drop) → Level 5 (30%) → Level 10 (60% cap)
- **Item power**: Unchanged (Iron Sword is same at all levels)
- **Better items**: Spawn more frequently at deeper levels

**Rationale**:

- Simpler than scaling item stats
- Rewards depth exploration with more loot
- Maintains item identity (Iron Sword = +5 ATK always)
- 60% cap prevents guaranteed drops

**Alternatives Considered**:

- Scale item stats → Rejected: Complicates inventory system
- Unlock new item types per level → Rejected: Requires many item definitions
- Guaranteed drops → Rejected: Removes RNG excitement

### 5. Level Transition Mechanics

**Research Question**: How should level transitions be triggered and executed?

**Decision**: Key-based interaction with staircase tiles

- Player moves onto staircase tile (Position matches)
- Press '>' key for downward, '<' key for upward
- Confirm prompt: "Descend to Level 2? (Y/N)"
- Transition: Save current level → Generate/load target level → Place player

**Rationale**:

- Explicit player choice (prevents accidental transitions)
- Familiar roguelike controls (>, < keys)
- Confirmation prevents misclicks
- Clear feedback during transition

**Alternatives Considered**:

- Automatic transition on step → Rejected: Accidental level changes
- Separate "use stairs" command → Rejected: Extra key binding
- No confirmation → Rejected: Frustrating for mistakes

### 6. Level State Snapshot Strategy

**Research Question**: What data needs to be saved when leaving a level?

**Decision**: Snapshot entities and map state

- **Map data**: Tile types, walkability, explored status
- **Entities**: All non-player entities (enemies, items, staircases)
- **Component data**: Position, Health, Combat, Item, etc.
- **Exclusions**: Player entity (persists across levels), temporary effects

**Rationale**:

- Complete state restoration (enemies stay dead, items stay picked up)
- Player is only entity that transitions
- Efficient serialization (components are structs)
- Supports future save/load feature

**Alternatives Considered**:

- Save only map → Rejected: Enemies respawn, confusing
- Save everything including player → Rejected: Player should transition
- Regenerate enemies → Rejected: Breaks immersion

### 7. Victory Condition Implementation

**Research Question**: How should the level 20 victory condition work?

**Decision**: Special victory chamber with final boss

- Level 20 downward staircase leads to victory chamber
- Victory chamber: Single large room with boss enemy
- Boss defeat triggers victory screen
- Endless mode: Config flag disables victory chamber

**Rationale**:

- Clear end goal for completionists
- Boss fight provides climactic challenge
- Endless mode for replayability
- Simple toggle for game mode

**Alternatives Considered**:

- No victory condition → Rejected: Lacks closure
- Artifact collection → Rejected: Adds complexity
- Time-based victory → Rejected: Conflicts with turn-based gameplay

### 8. Depth Tracking and Display

**Research Question**: How should current depth and personal best be tracked?

**Decision**: Persistent depth tracking with HUD display

- **Current depth**: Stored in GameState, displayed in HUD
- **Personal best**: Saved to player profile (future: JSON file)
- **Display format**: "Dungeon Level: 5" and "Depth: -150 ft" (30 ft/level)
- **Record notification**: "New Record Depth: Level 6!" on achievement

**Rationale**:

- Clear player feedback
- Motivates depth exploration
- Depth in feet adds flavor
- Personal best creates replayability goal

**Alternatives Considered**:

- Level only (no feet) → Rejected: Less immersive
- No personal best → Rejected: Misses motivation opportunity
- Leaderboards → Rejected: Single-player game

## Technology Stack Validation

### Existing Dependencies (No Changes Required)

- ✅ **Arch ECS (1.3.3)**: Supports entity snapshotting and recreation
- ✅ **GoRogue (3.0.0-beta09)**: Map generation already established
- ✅ **Existing Systems**: MapGenerator, EnemySpawnSystem, ItemSpawnSystem provide integration points

### New Dependencies

- ❌ None required - all features achievable with existing stack

## Performance Considerations

### Expected Load

- 20-30 dungeon levels in memory simultaneously
- 50-100 rooms per level
- 10-30 enemies per level
- 20-100 items per level
- Level cache: ~500 KB per level × 30 = ~15 MB total

### Optimization Strategy

- Lazy loading: Generate levels on first visit only
- Cache eviction: Remove levels beyond ±5 from current (keep 11 levels max)
- Snapshot compression: Store only changed entities
- Transition speed: <100ms for level switch

**Conclusion**: Performance is acceptable. 15 MB memory footprint is negligible for modern systems.

## Integration Points

### Existing Systems to Extend

1. **MapGenerator**: Add staircase placement logic
2. **EnemySpawnSystem**: Apply difficulty scaling to spawned enemies
3. **ItemSpawnSystem**: Apply loot drop rate scaling
4. **GameStateManager**: Integrate LevelManager for level transitions
5. **HudService**: Display current level and depth
6. **DungeonCrawlerService**: Handle staircase interaction (>, < keys)

### New Systems to Create

1. **LevelManager**: Level persistence, transitions, state snapshots
2. **DifficultyScalingSystem**: Calculate scaled stats and drop rates

### Data Dependencies

- Staircases must have Position and Staircase components
- DungeonLevel stores map and entity snapshots
- GameState tracks current level number
- Player profile tracks personal best depth

## Risk Assessment

### Low Risk

- ✅ Level caching: Straightforward dictionary-based storage
- ✅ Staircase placement: Extends existing room-based generation
- ✅ Scaling formulas: Simple mathematical calculations
- ✅ HUD display: Extends existing HUD service

### Medium Risk

- ⚠️ **Entity snapshotting complexity**: Serializing all component types
  - Mitigation: Start with essential components, add others incrementally
- ⚠️ **State restoration bugs**: Entities not recreating correctly
  - Mitigation: Comprehensive testing, verify each component type

### High Risk

- ❌ None identified

## Testing Strategy

### Unit Tests

- Difficulty scaling formulas (verify 1.12^level calculations)
- Loot drop rate calculations
- Staircase placement (distance constraints)
- Level state snapshotting and restoration

### Integration Tests

- Descend to level 2 → Ascend to level 1 → Verify state unchanged
- Kill enemy on level 1 → Descend → Ascend → Verify enemy still dead
- Pick up item on level 2 → Descend → Ascend → Verify item gone
- Reach level 20 → Verify victory chamber triggers

### Manual Tests

- Full playthrough: Descend to level 10, ascend back to level 1
- Verify enemy difficulty increases noticeably
- Verify loot quality improves with depth
- Test victory condition at level 20
- Test endless mode beyond level 20

## Scaling Formulas Reference

### Enemy Stat Scaling

```csharp
public int CalculateScaledStat(int baseStat, int level)
{
    const double scalingFactor = 1.12;
    const int maxLevel = 30;

    int effectiveLevel = Math.Min(level, maxLevel);
    double scaledValue = baseStat * Math.Pow(scalingFactor, effectiveLevel - 1);

    return (int)Math.Round(scaledValue);
}

// Examples:
// Level 1 Goblin: 20 HP, 5 ATK, 2 DEF
// Level 5 Goblin: 32 HP, 9 ATK, 3 DEF (60% increase)
// Level 10 Goblin: 52 HP, 14 ATK, 5 DEF (160% increase)
// Level 20 Goblin: 165 HP, 45 ATK, 17 DEF (725% increase)
```

### Loot Drop Rate Scaling

```csharp
public double CalculateLootDropRate(int level)
{
    const double baseRate = 0.10;      // 10% at level 1
    const double ratePerLevel = 0.05;  // +5% per level
    const double maxRate = 0.60;       // 60% cap

    double dropRate = baseRate + (level * ratePerLevel);
    return Math.Min(dropRate, maxRate);
}

// Examples:
// Level 1: 10% equipment drop chance
// Level 5: 30% equipment drop chance
// Level 10: 60% equipment drop chance (capped)
// Level 20: 60% equipment drop chance (capped)
```

### Depth Display Calculation

```csharp
public int CalculateDepthInFeet(int level)
{
    const int feetPerLevel = 30;
    return level * feetPerLevel;
}

// Examples:
// Level 1: -30 ft
// Level 5: -150 ft
// Level 10: -300 ft
// Level 20: -600 ft
```

## Open Questions

### Resolved

- ✅ How to store level state? → In-memory cache with snapshots
- ✅ How to place staircases? → Distant rooms during generation
- ✅ How to scale difficulty? → Exponential formula (1.12^level)
- ✅ How to improve loot? → Increase drop rates, not item power
- ✅ How to trigger transitions? → Key-based interaction with confirmation
- ✅ How to handle victory? → Special chamber at level 20 with boss

### Deferred (Out of Scope)

- Save/load persistence (future enhancement)
- Procedural boss generation (use existing enemy with high stats)
- Level themes/biomes (all levels use same tileset for MVP)
- Special level events (shrines, shops, etc.)
- Level skip mechanics (teleportation, etc.)

## Next Steps

Proceed to **Phase 1: Design & Contracts** to create:

1. `data-model.md` - DungeonLevel, Staircase, LevelManager schemas
2. `contracts/` - LevelManager and DifficultyScalingSystem interfaces
3. `quickstart.md` - Developer guide for level transitions

---

**Research Complete**: All technical decisions documented and validated against existing architecture.
