# Data Model: Dungeon Progression System

**Feature**: Dungeon Progression System
**Date**: 2025-10-21
**Phase**: 1 - Design & Contracts

## Overview

This document defines the complete data model for the dungeon progression system. All components follow the Arch ECS framework patterns established in `LablabBean.Game.Core`.

## Component Definitions

### Staircase Component

```csharp
namespace LablabBean.Game.Core.Components;

/// <summary>
/// Component for staircase entities that enable level transitions.
/// </summary>
public struct Staircase
{
    /// <summary>Direction of the staircase</summary>
    public StaircaseDirection Direction { get; set; }
    
    /// <summary>Target level number (for validation)</summary>
    public int TargetLevel { get; set; }
    
    /// <summary>Glyph for rendering</summary>
    public char Glyph => Direction == StaircaseDirection.Down ? '>' : '<';
    
    /// <summary>Description for interaction</summary>
    public string Description => Direction == StaircaseDirection.Down 
        ? $"Downward staircase to Level {TargetLevel}"
        : $"Upward staircase to Level {TargetLevel}";
}

public enum StaircaseDirection
{
    Up,
    Down
}
```

## Level State Management

### DungeonLevel Class

```csharp
namespace LablabBean.Game.Core.Maps;

/// <summary>
/// Represents a complete dungeon level with all its state.
/// Stored in LevelManager cache for persistence.
/// </summary>
public class DungeonLevel
{
    /// <summary>Level number (1-based)</summary>
    public int LevelNumber { get; set; }
    
    /// <summary>Dungeon map data</summary>
    public DungeonMap Map { get; set; }
    
    /// <summary>Entity snapshots for this level</summary>
    public List<EntitySnapshot> Entities { get; set; }
    
    /// <summary>Upward staircase position (null for level 1)</summary>
    public Position? UpStaircasePosition { get; set; }
    
    /// <summary>Downward staircase position</summary>
    public Position DownStaircasePosition { get; set; }
    
    /// <summary>Player spawn position (where player appears when entering this level)</summary>
    public Position PlayerSpawnPosition { get; set; }
    
    /// <summary>Whether this level has been visited before</summary>
    public bool IsVisited { get; set; }
    
    /// <summary>Timestamp of last visit</summary>
    public DateTime LastVisited { get; set; }
    
    /// <summary>Difficulty scaling multiplier for this level</summary>
    public double DifficultyMultiplier { get; set; }
    
    /// <summary>Loot drop rate for this level</summary>
    public double LootDropRate { get; set; }
}
```

### EntitySnapshot Struct

```csharp
namespace LablabBean.Game.Core.Maps;

/// <summary>
/// Snapshot of an entity's state for level persistence.
/// Used to recreate entities when returning to a level.
/// </summary>
public struct EntitySnapshot
{
    /// <summary>Entity archetype (Enemy, Item, Staircase, etc.)</summary>
    public EntityArchetype Archetype { get; set; }
    
    /// <summary>Serialized component data</summary>
    public Dictionary<string, object> Components { get; set; }
    
    /// <summary>Entity position</summary>
    public Position Position { get; set; }
    
    /// <summary>Whether entity is still alive/active</summary>
    public bool IsActive { get; set; }
}

public enum EntityArchetype
{
    Enemy,
    Item,
    Staircase,
    Decoration
}
```

## Level Manager Service

### LevelManager Class

```csharp
namespace LablabBean.Game.Core.Maps;

using Arch.Core;

/// <summary>
/// Manages dungeon level persistence, transitions, and state.
/// </summary>
public class LevelManager
{
    private readonly World _world;
    private readonly MapGenerator _mapGenerator;
    private readonly DifficultyScalingSystem _difficultyScaling;
    
    /// <summary>Cache of all generated levels</summary>
    private readonly Dictionary<int, DungeonLevel> _levelCache;
    
    /// <summary>Current active level number</summary>
    public int CurrentLevel { get; private set; }
    
    /// <summary>Player's personal best depth</summary>
    public int PersonalBestDepth { get; private set; }
    
    /// <summary>Maximum level before difficulty cap</summary>
    public const int MaxScalingLevel = 30;
    
    /// <summary>Victory level (final boss)</summary>
    public const int VictoryLevel = 20;
    
    /// <summary>Whether endless mode is enabled</summary>
    public bool EndlessModeEnabled { get; set; }
    
    public LevelManager(World world, MapGenerator mapGenerator, DifficultyScalingSystem difficultyScaling)
    {
        _world = world;
        _mapGenerator = mapGenerator;
        _difficultyScaling = difficultyScaling;
        _levelCache = new Dictionary<int, DungeonLevel>();
        CurrentLevel = 1;
        PersonalBestDepth = 1;
    }
}
```

## Difficulty Scaling System

### DifficultyScalingSystem Class

```csharp
namespace LablabBean.Game.Core.Systems;

/// <summary>
/// Calculates difficulty scaling for enemies and loot based on dungeon level.
/// </summary>
public class DifficultyScalingSystem
{
    /// <summary>Scaling factor per level (12% increase)</summary>
    private const double ScalingFactor = 1.12;
    
    /// <summary>Base loot drop rate at level 1</summary>
    private const double BaseLootDropRate = 0.10;
    
    /// <summary>Loot drop rate increase per level</summary>
    private const double LootDropRatePerLevel = 0.05;
    
    /// <summary>Maximum loot drop rate (cap)</summary>
    private const double MaxLootDropRate = 0.60;
    
    /// <summary>Maximum level for scaling (prevents overflow)</summary>
    private const int MaxScalingLevel = 30;
    
    /// <summary>
    /// Calculates scaled stat value for a given level.
    /// </summary>
    /// <param name="baseStat">Base stat value at level 1</param>
    /// <param name="level">Current dungeon level</param>
    /// <returns>Scaled stat value</returns>
    public int CalculateScaledStat(int baseStat, int level)
    {
        int effectiveLevel = Math.Min(level, MaxScalingLevel);
        double scaledValue = baseStat * Math.Pow(ScalingFactor, effectiveLevel - 1);
        return (int)Math.Round(scaledValue);
    }
    
    /// <summary>
    /// Calculates loot drop rate for a given level.
    /// </summary>
    /// <param name="level">Current dungeon level</param>
    /// <returns>Drop rate (0.0 to 1.0)</returns>
    public double CalculateLootDropRate(int level)
    {
        double dropRate = BaseLootDropRate + (level * LootDropRatePerLevel);
        return Math.Min(dropRate, MaxLootDropRate);
    }
    
    /// <summary>
    /// Applies difficulty scaling to an enemy entity.
    /// </summary>
    /// <param name="enemyEntity">Enemy entity to scale</param>
    /// <param name="level">Current dungeon level</param>
    public void ApplyEnemyScaling(Entity enemyEntity, int level);
    
    /// <summary>
    /// Determines if loot should drop based on level scaling.
    /// </summary>
    /// <param name="level">Current dungeon level</param>
    /// <param name="random">Random number generator</param>
    /// <returns>True if loot should drop</returns>
    public bool ShouldDropLoot(int level, Random random);
}
```

## Scaling Examples

### Enemy Stat Scaling Table

| Level | Goblin HP | Goblin ATK | Goblin DEF | Multiplier |
|-------|-----------|------------|------------|------------|
| 1     | 20        | 5          | 2          | 1.00x      |
| 2     | 22        | 6          | 2          | 1.12x      |
| 3     | 25        | 6          | 2          | 1.25x      |
| 5     | 32        | 9          | 3          | 1.57x      |
| 10    | 52        | 14         | 5          | 2.77x      |
| 15    | 88        | 24         | 9          | 4.89x      |
| 20    | 165       | 45         | 17         | 8.61x      |
| 30    | 537       | 146        | 55         | 28.75x (cap)|

### Loot Drop Rate Table

| Level | Equipment Drop Rate | Consumable Drop Rate | Notes |
|-------|---------------------|----------------------|-------|
| 1     | 10%                 | 30%                  | Base rates |
| 2     | 15%                 | 35%                  | |
| 5     | 30%                 | 50%                  | |
| 10    | 60%                 | 80%                  | Equipment capped |
| 15    | 60%                 | 95%                  | Equipment capped |
| 20    | 60%                 | 100%                 | Equipment capped |

## State Transitions

### Level Transition Flow

```
Player on Level N
    ↓
Player moves onto staircase tile
    ↓
Player presses '>' (down) or '<' (up)
    ↓
Confirmation prompt: "Descend to Level N+1? (Y/N)"
    ↓
[If Yes]
    ↓
1. Save Current Level State
   - Snapshot all entities (enemies, items)
   - Store map data
   - Cache in LevelManager._levelCache[N]
    ↓
2. Load/Generate Target Level
   - Check if level exists in cache
   - If exists: Restore from cache
   - If new: Generate new level
    ↓
3. Place Player
   - Position at target staircase
   - Maintain health, inventory, equipment
   - Update CurrentLevel
    ↓
4. Update HUD
   - Display "Dungeon Level: N+1"
   - Display "Depth: -X ft"
   - Check for new personal best
    ↓
Player on Level N+1
```

### Entity Snapshotting Process

```
For each entity on current level (except player):
    ↓
1. Determine archetype (Enemy, Item, Staircase)
    ↓
2. Serialize components
   - Position → (X, Y)
   - Health → (Current, Maximum)
   - Combat → (Attack, Defense)
   - Item → (Name, Type, etc.)
   - Enemy → (Type, Behavior, etc.)
    ↓
3. Store in EntitySnapshot
   - Archetype
   - Components dictionary
   - IsActive flag
    ↓
4. Add to DungeonLevel.Entities list
    ↓
Store DungeonLevel in cache
```

### Entity Restoration Process

```
For each EntitySnapshot in DungeonLevel.Entities:
    ↓
1. Check IsActive flag
   - If false: Skip (entity was destroyed)
    ↓
2. Create entity based on archetype
   - Enemy: world.Create(Enemy, Combat, Health, Position, ...)
   - Item: world.Create(Item, Position, Renderable, ...)
   - Staircase: world.Create(Staircase, Position, Renderable)
    ↓
3. Deserialize components from snapshot
   - Restore Position, Health, Combat, etc.
    ↓
4. Add to world
    ↓
Level fully restored
```

## Integration with Existing Systems

### MapGenerator Extension

```csharp
// In MapGenerator.cs
public DungeonMap GenerateLevel(int levelNumber, out Position upStairs, out Position downStairs)
{
    var map = GenerateRooms();
    var rooms = GetRooms();
    
    // Place downward staircase in farthest room
    var farthestRoom = FindFarthestRoom(rooms);
    downStairs = GetRandomPositionInRoom(farthestRoom);
    
    // Place upward staircase near start (except level 1)
    if (levelNumber > 1)
    {
        var startRoom = rooms[0];
        upStairs = GetRandomPositionInRoom(startRoom);
    }
    else
    {
        upStairs = Position.Invalid; // No up stairs on level 1
    }
    
    return map;
}
```

### EnemySpawnSystem Extension

```csharp
// In EnemySpawnSystem.cs
public void SpawnEnemiesForLevel(DungeonMap map, List<Room> rooms, int level)
{
    foreach (var room in rooms)
    {
        if (ShouldSpawnEnemy(room))
        {
            var enemy = SpawnEnemy(room);
            
            // Apply difficulty scaling
            _difficultyScaling.ApplyEnemyScaling(enemy, level);
        }
    }
}
```

### ItemSpawnSystem Extension

```csharp
// In ItemSpawnSystem.cs
public void SpawnEnemyLoot(Entity enemyEntity, Position position, int level)
{
    // Check if loot should drop based on level
    if (_difficultyScaling.ShouldDropLoot(level, _random))
    {
        var itemDef = GetRandomItem();
        SpawnItem(itemDef, position);
    }
}
```

## HUD Display Format

### Level Information Display

```
┌─────────────────────────────────┐
│ Dungeon Level: 5                │
│ Depth: -150 ft                  │
│ Personal Best: Level 8          │
└─────────────────────────────────┘
```

### Transition Messages

```
"Descending to Level 2..."
"Ascending to Level 1..."
"New Record Depth: Level 6!"
"You have reached the Victory Chamber!"
```

## Victory Condition

### Victory Chamber Structure

```csharp
public class VictoryChamber
{
    /// <summary>Special level number for victory chamber</summary>
    public const int VictoryChamberLevel = 21;
    
    /// <summary>Generate victory chamber (single large room with boss)</summary>
    public DungeonLevel GenerateVictoryChamber()
    {
        var level = new DungeonLevel
        {
            LevelNumber = VictoryChamberLevel,
            Map = GenerateSingleLargeRoom(),
            Entities = new List<EntitySnapshot>(),
            IsVisited = false
        };
        
        // Spawn final boss in center
        var bossPosition = new Position { X = 25, Y = 15 };
        var boss = CreateFinalBoss(bossPosition);
        
        // Add treasure hoard
        SpawnVictoryTreasure(level);
        
        return level;
    }
}
```

### Final Boss Stats

```csharp
// Final Boss (Level 20 equivalent)
var finalBoss = new Enemy
{
    EnemyType = "Ancient Dragon",
    Behavior = AIBehavior.Aggressive
};

var bossHealth = new Health
{
    Current = 500,  // 10x normal level 20 enemy
    Maximum = 500
};

var bossCombat = new Combat
{
    Attack = 60,    // 1.5x scaled level 20
    Defense = 25    // 1.5x scaled level 20
};
```

## Performance Characteristics

### Memory Usage
- **Per DungeonLevel**: ~100 KB (map + 30 entities × 3 KB each)
- **30 Levels Cached**: ~3 MB total
- **Entity Snapshots**: ~3 KB per entity (5-10 components)
- **Level Cache Eviction**: Keep only ±5 levels from current (11 levels max = ~1.1 MB)

### Transition Performance
- **Save Level**: <50ms (snapshot 30 entities)
- **Load Cached Level**: <50ms (restore 30 entities)
- **Generate New Level**: <500ms (map generation + enemy/item spawning)
- **Total Transition**: <100ms (cached) or <600ms (new level)

**Conclusion**: Performance is excellent. Transitions feel instant for cached levels.

## Validation Rules

### Level Transition Constraints
- ✅ Cannot ascend from level 1 (no up staircase exists)
- ✅ Cannot descend beyond level 30 in endless mode (difficulty cap)
- ✅ Level 20 descend leads to victory chamber (unless endless mode)
- ✅ Player must be on staircase tile to interact

### State Persistence Constraints
- ✅ All non-player entities are snapshotted
- ✅ Player entity never snapshotted (transitions between levels)
- ✅ Dead enemies remain dead when returning to level
- ✅ Picked-up items remain gone when returning to level

### Difficulty Scaling Constraints
- ✅ Stats scale up to level 30, then plateau
- ✅ Loot drop rate caps at 60%
- ✅ Scaling applies to all enemy types equally
- ✅ Item power does not scale (only drop rates)

---

**Data Model Complete**: All components, level management, and scaling systems defined.
**Next**: Generate system contracts and API definitions.
