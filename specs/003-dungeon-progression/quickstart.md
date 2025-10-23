# Quickstart Guide: Dungeon Progression System

**Feature**: Dungeon Progression System
**Date**: 2025-10-21
**Audience**: Developers implementing or extending the dungeon progression system

## Overview

This guide provides practical examples for working with the dungeon progression system. All code examples assume you have access to the `World` instance and relevant entity references.

## Table of Contents

1. [Level Transitions](#level-transitions)
2. [Difficulty Scaling](#difficulty-scaling)
3. [Level State Persistence](#level-state-persistence)
4. [Staircase Placement](#staircase-placement)
5. [HUD Integration](#hud-integration)
6. [Victory Condition](#victory-condition)
7. [Testing Examples](#testing-examples)

---

## Level Transitions

### Descend to Next Level

```csharp
using LablabBean.Game.Core.Maps;
using LablabBean.Game.Core.Systems;

var levelManager = new LevelManager(world, mapGenerator, difficultyScaling);

// Player presses '>' key while on downward staircase
var result = levelManager.DescendLevel(playerEntity);

if (result.Success)
{
    Console.WriteLine(result.Message); // "Descending to Level 2..."

    if (result.NewRecordDepth)
    {
        Console.WriteLine("New Record Depth: Level 2!");
    }

    if (result.VictoryTriggered)
    {
        Console.WriteLine("You have reached the Victory Chamber!");
    }
}
```

### Ascend to Previous Level

```csharp
// Player presses '<' key while on upward staircase
var result = levelManager.AscendLevel(playerEntity);

if (result.Success)
{
    Console.WriteLine(result.Message); // "Ascending to Level 1..."
}
else
{
    Console.WriteLine(result.Message); // "Cannot ascend further"
}
```

### Check if Transition is Possible

```csharp
// Check if player can descend
if (levelManager.CanTransition(playerEntity, StaircaseDirection.Down))
{
    // Show prompt: "Descend to Level 3? (Y/N)"
}

// Check if player can ascend
if (levelManager.CanTransition(playerEntity, StaircaseDirection.Up))
{
    // Show prompt: "Ascend to Level 2? (Y/N)"
}
```

### Handle Staircase Interaction

```csharp
// In DungeonCrawlerService.cs
private void OnStaircaseInteraction(char key)
{
    // Get staircase at player position
    var staircase = _levelManager.GetPlayerStaircase(_playerEntity);

    if (staircase == null)
    {
        _hudService.AddMessage("You are not on a staircase.");
        return;
    }

    var staircaseComp = _world.Get<Staircase>(staircase.Value);

    // Validate key matches staircase direction
    if (key == '>' && staircaseComp.Direction != StaircaseDirection.Down)
    {
        _hudService.AddMessage("This is an upward staircase. Press '<' to ascend.");
        return;
    }

    if (key == '<' && staircaseComp.Direction != StaircaseDirection.Up)
    {
        _hudService.AddMessage("This is a downward staircase. Press '>' to descend.");
        return;
    }

    // Confirm transition
    if (ShowConfirmation($"{staircaseComp.Description}. Continue? (Y/N)"))
    {
        var result = staircaseComp.Direction == StaircaseDirection.Down
            ? _levelManager.DescendLevel(_playerEntity)
            : _levelManager.AscendLevel(_playerEntity);

        _hudService.AddMessage(result.Message);

        if (result.NewRecordDepth)
        {
            _hudService.AddMessage($"New Record Depth: Level {result.NewLevel}!");
        }
    }
}
```

---

## Difficulty Scaling

### Apply Scaling to Enemy

```csharp
var difficultyScaling = new DifficultyScalingSystem(world);

// Spawn base enemy (level 1 stats)
var goblin = world.Create(
    new Enemy { EnemyType = "Goblin", Behavior = AIBehavior.Chase },
    new Health { Current = 20, Maximum = 20 },
    new Combat { Attack = 5, Defense = 2 },
    new Position { X = 10, Y = 10 }
);

// Apply scaling for current level
int currentLevel = 5;
difficultyScaling.ApplyEnemyScaling(goblin, currentLevel);

// Goblin now has scaled stats:
// Health: 32/32 (was 20/20)
// Attack: 9 (was 5)
// Defense: 3 (was 2)
```

### Calculate Scaled Stats Manually

```csharp
// Calculate what stats would be at level 10
int baseHealth = 20;
int baseAttack = 5;
int baseDefense = 2;

int scaledHealth = difficultyScaling.CalculateScaledHealth(baseHealth, 10);
int scaledAttack = difficultyScaling.CalculateScaledAttack(baseAttack, 10);
int scaledDefense = difficultyScaling.CalculateScaledDefense(baseDefense, 10);

Console.WriteLine($"Level 10 Goblin: {scaledHealth} HP, {scaledAttack} ATK, {scaledDefense} DEF");
// Output: "Level 10 Goblin: 52 HP, 14 ATK, 5 DEF"
```

### Loot Drop Rate Scaling

```csharp
// Check if enemy should drop loot
int currentLevel = 5;

if (difficultyScaling.ShouldDropLoot(currentLevel))
{
    // 30% chance at level 5
    if (difficultyScaling.ShouldDropEquipment(currentLevel))
    {
        // Drop equipment
        SpawnEquipment(enemyPosition);
    }
    else
    {
        // Drop consumable
        SpawnConsumable(enemyPosition);
    }
}
```

### Get Scaling Statistics

```csharp
// Get detailed scaling info for a level
var stats = difficultyScaling.GetScalingStats(10);

Console.WriteLine($"Level {stats.Level}:");
Console.WriteLine($"  Multiplier: {stats.Multiplier:F2}x");
Console.WriteLine($"  Loot Drop Rate: {stats.LootDropRate:P0}");
Console.WriteLine($"  Depth: {stats.DepthInFeet} ft");
Console.WriteLine($"  Example Enemy: {stats.ExampleHealth} HP, {stats.ExampleAttack} ATK, {stats.ExampleDefense} DEF");

// Output:
// Level 10:
//   Multiplier: 2.77x
//   Loot Drop Rate: 60%
//   Depth: -300 ft
//   Example Enemy: 52 HP, 14 ATK, 5 DEF
```

---

## Level State Persistence

### Save Current Level

```csharp
// Before transitioning to another level
int currentLevel = 3;
levelManager.SaveLevelState(currentLevel);

// All entities (enemies, items) are snapshotted
// Map data is cached
// Player entity is excluded
```

### Restore Previous Level

```csharp
// When returning to a previously visited level
int targetLevel = 2;

if (levelManager.IsLevelCached(targetLevel))
{
    levelManager.RestoreLevelState(targetLevel);
    // Entities recreated from snapshots
    // Dead enemies stay dead
    // Picked-up items stay gone
}
else
{
    // Level not visited yet, generate new
    var newLevel = levelManager.GenerateLevel(targetLevel);
}
```

### Entity Snapshotting

```csharp
// Snapshot a single entity
var enemySnapshot = levelManager.SnapshotEntity(enemyEntity);

// Snapshot contains:
// - Archetype (Enemy, Item, etc.)
// - Component data (Position, Health, Combat, etc.)
// - IsActive flag (false if dead/destroyed)

// Restore entity from snapshot
var restoredEnemy = levelManager.RestoreEntity(enemySnapshot);
```

---

## Staircase Placement

### Place Staircases in Generated Level

```csharp
// In MapGenerator.cs
public DungeonMap GenerateLevel(int levelNumber)
{
    var map = GenerateRooms();
    var rooms = GetRooms();

    var level = new DungeonLevel
    {
        LevelNumber = levelNumber,
        Map = map,
        Entities = new List<EntitySnapshot>()
    };

    // Place staircases
    _levelManager.PlaceStaircases(level, rooms);

    // Spawn enemies with scaling
    SpawnEnemies(map, rooms, levelNumber);

    // Spawn items with scaling
    SpawnItems(map, rooms, levelNumber);

    return map;
}
```

### Create Staircase Entities

```csharp
// Downward staircase
var downStairs = world.Create(
    new Staircase
    {
        Direction = StaircaseDirection.Down,
        TargetLevel = currentLevel + 1
    },
    new Position { X = 45, Y = 30 },
    new Renderable { Glyph = '>', ForegroundColor = Color.White }
);

// Upward staircase (except level 1)
if (currentLevel > 1)
{
    var upStairs = world.Create(
        new Staircase
        {
            Direction = StaircaseDirection.Up,
            TargetLevel = currentLevel - 1
        },
        new Position { X = 5, Y = 5 },
        new Renderable { Glyph = '<', ForegroundColor = Color.White }
    );
}
```

---

## HUD Integration

### Display Current Level and Depth

```csharp
// In HudService.cs
public void UpdateLevelDisplay(int currentLevel, int personalBest)
{
    var depthString = _difficultyScaling.GetDepthDisplayString(currentLevel);

    _levelLabel.Text = $"Dungeon Level: {currentLevel}";
    _depthLabel.Text = depthString; // "Depth: -150 ft"
    _bestLabel.Text = $"Personal Best: Level {personalBest}";
}
```

### Show Transition Messages

```csharp
// After level transition
private void OnLevelTransition(LevelTransitionResult result)
{
    _hudService.AddMessage(result.Message);

    if (result.NewRecordDepth)
    {
        _hudService.AddMessage($"New Record Depth: Level {result.NewLevel}!", Color.Yellow);
    }

    _hudService.UpdateLevelDisplay(result.NewLevel, _levelManager.PersonalBestDepth);
}
```

---

## Victory Condition

### Check for Victory

```csharp
// When descending from level 20
if (levelManager.ShouldTransitionToVictoryChamber(currentLevel, StaircaseDirection.Down))
{
    var victoryChamber = levelManager.GenerateVictoryChamber();

    // Load victory chamber instead of level 21
    // Spawn final boss
    // Show victory message when boss defeated
}
```

### Generate Victory Chamber

```csharp
public DungeonLevel GenerateVictoryChamber()
{
    var level = new DungeonLevel
    {
        LevelNumber = 21, // Special victory level
        Map = GenerateSingleLargeRoom(50, 30),
        Entities = new List<EntitySnapshot>()
    };

    // Spawn final boss in center
    var bossPosition = new Position { X = 25, Y = 15 };
    var boss = CreateFinalBoss(bossPosition);

    // Spawn treasure hoard
    SpawnVictoryTreasure(level);

    return level;
}

private Entity CreateFinalBoss(Position position)
{
    var boss = world.Create(
        new Enemy { EnemyType = "Ancient Dragon", Behavior = AIBehavior.Aggressive },
        new Health { Current = 500, Maximum = 500 },
        new Combat { Attack = 60, Defense = 25 },
        position,
        new Renderable { Glyph = 'D', ForegroundColor = Color.Red }
    );

    return boss;
}
```

### Endless Mode

```csharp
// Enable endless mode (no victory condition)
levelManager.EndlessModeEnabled = true;

// Level 20 descend leads to level 21, 22, 23...
// Difficulty caps at level 30, then plateaus
```

---

## Testing Examples

### Unit Test: Difficulty Scaling

```csharp
[Fact]
public void CalculateScaledStat_Level5_Returns57PercentIncrease()
{
    // Arrange
    var difficultyScaling = new DifficultyScalingSystem(world);
    int baseStat = 20;
    int level = 5;

    // Act
    int scaledStat = difficultyScaling.CalculateScaledStat(baseStat, level);

    // Assert
    Assert.Equal(32, scaledStat); // 20 × 1.57 ≈ 32
}

[Fact]
public void CalculateLootDropRate_Level10_ReturnsCapped60Percent()
{
    // Arrange
    var difficultyScaling = new DifficultyScalingSystem(world);

    // Act
    double dropRate = difficultyScaling.CalculateLootDropRate(10);

    // Assert
    Assert.Equal(0.60, dropRate); // Capped at 60%
}
```

### Integration Test: Level Transition

```csharp
[Fact]
public void DescendLevel_SavesCurrentState_RestoresOnAscend()
{
    // Arrange
    var levelManager = new LevelManager(world, mapGenerator, difficultyScaling);
    var player = CreatePlayer();

    // Kill an enemy on level 1
    var enemy = CreateEnemy(new Position { X = 10, Y = 10 });
    KillEnemy(enemy);

    // Act: Descend to level 2
    levelManager.DescendLevel(player);

    // Act: Ascend back to level 1
    levelManager.AscendLevel(player);

    // Assert: Enemy still dead
    var enemiesOnLevel1 = GetEnemiesOnCurrentLevel();
    Assert.DoesNotContain(enemy, enemiesOnLevel1);
}
```

### Manual Test: Full Progression

```csharp
// 1. Start on level 1
Assert.Equal(1, levelManager.CurrentLevel);

// 2. Find downward staircase
var downStairs = FindStaircase(StaircaseDirection.Down);
Assert.NotNull(downStairs);

// 3. Move player to staircase
MovePlayerTo(downStairs.Position);

// 4. Descend to level 2
var result = levelManager.DescendLevel(playerEntity);
Assert.True(result.Success);
Assert.Equal(2, result.NewLevel);

// 5. Verify enemies are tougher
var level2Enemy = SpawnEnemy();
Assert.True(level2Enemy.Health > 20); // Scaled up

// 6. Ascend back to level 1
result = levelManager.AscendLevel(playerEntity);
Assert.Equal(1, result.NewLevel);

// 7. Verify level 1 state preserved
// (enemies dead, items picked up, etc.)
```

---

## Common Patterns

### Pattern: Level Transition with Confirmation

```csharp
public void HandleStaircaseInput(char key)
{
    var staircase = _levelManager.GetPlayerStaircase(_playerEntity);
    if (staircase == null) return;

    var staircaseComp = _world.Get<Staircase>(staircase.Value);
    int targetLevel = staircaseComp.TargetLevel;

    // Show confirmation
    string prompt = staircaseComp.Direction == StaircaseDirection.Down
        ? $"Descend to Level {targetLevel}? (Y/N)"
        : $"Ascend to Level {targetLevel}? (Y/N)";

    if (ShowConfirmation(prompt))
    {
        var result = staircaseComp.Direction == StaircaseDirection.Down
            ? _levelManager.DescendLevel(_playerEntity)
            : _levelManager.AscendLevel(_playerEntity);

        HandleTransitionResult(result);
    }
}
```

### Pattern: Cache Management

```csharp
// Evict distant levels to save memory
public void ManageLevelCache()
{
    int currentLevel = _levelManager.CurrentLevel;

    // Keep only levels within ±5 of current
    _levelManager.EvictDistantLevels(keepRange: 5);

    // Example: At level 10, keep levels 5-15, evict others
    int cachedCount = _levelManager.GetCachedLevelCount();
    Console.WriteLine($"Cached levels: {cachedCount}");
}
```

### Pattern: Personal Best Tracking

```csharp
public void TrackPersonalBest(int newLevel)
{
    if (_levelManager.UpdatePersonalBest(newLevel))
    {
        // New record!
        _hudService.AddMessage($"New Record Depth: Level {newLevel}!", Color.Yellow);

        // Save to player profile (future)
        SavePlayerProfile();
    }
}
```

---

## Next Steps

- Review [data-model.md](./data-model.md) for complete component schemas
- Review [contracts/](./contracts/) for LevelManager and DifficultyScalingSystem interfaces
- Implement LevelManager in `LablabBean.Game.Core/Maps/`
- Implement DifficultyScalingSystem in `LablabBean.Game.Core/Systems/`
- Extend MapGenerator to place staircases
- Extend EnemySpawnSystem and ItemSpawnSystem for scaling
- Add level display to HUD
- Add staircase interaction to DungeonCrawlerService
- Write unit tests for scaling formulas
- Run integration tests for level transitions

---

**Quickstart Complete**: Ready to begin implementation!
