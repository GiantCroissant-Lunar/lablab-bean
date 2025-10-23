using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Game.Core.Systems;
using SadRogue.Primitives;

namespace LablabBean.Game.Core.Maps;

/// <summary>
/// Result of a level transition attempt
/// </summary>
public class LevelTransitionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int NewLevel { get; set; }
    public bool NewRecordDepth { get; set; }
    public bool VictoryTriggered { get; set; }

    public LevelTransitionResult()
    {
        Message = string.Empty;
    }
}

/// <summary>
/// Manages dungeon level generation, persistence, and transitions
/// </summary>
public class LevelManager
{
    private readonly World _world;
    private readonly MapGenerator _mapGenerator;
    private readonly DifficultyScalingSystem _difficultyScaling;
    private readonly Dictionary<int, DungeonLevel> _levelCache;
    private readonly Random _random;

    public event Action<int>? OnLevelCompleted;
    public event Action<int>? OnNewDepthReached;
    public event Action? OnDungeonCompleted;

    public int CurrentLevel { get; private set; }
    public int PersonalBestDepth { get; private set; }
    public bool EndlessModeEnabled { get; set; }
    public int VictoryLevel { get; set; }

    private const int MAX_CACHED_LEVELS = 10;

    public LevelManager(World world, MapGenerator mapGenerator, DifficultyScalingSystem difficultyScaling, int? seed = null)
    {
        _world = world;
        _mapGenerator = mapGenerator;
        _difficultyScaling = difficultyScaling;
        _levelCache = new Dictionary<int, DungeonLevel>();
        _random = seed.HasValue ? new Random(seed.Value) : new Random();

        CurrentLevel = 1;
        PersonalBestDepth = 1;
        EndlessModeEnabled = false;
        VictoryLevel = 20;
    }

    /// <summary>
    /// Initialize the first level (called during game initialization)
    /// </summary>
    public void InitializeFirstLevel(DungeonLevel level)
    {
        CurrentLevel = 1;
        _levelCache[1] = level;

        // Place staircases on first level
        PlaceStaircases(level);
    }

    /// <summary>
    /// Descend to the next dungeon level
    /// </summary>
    public LevelTransitionResult DescendLevel(Entity player)
    {
        var result = new LevelTransitionResult();

        // Check if on victory level
        if (!EndlessModeEnabled && CurrentLevel >= VictoryLevel)
        {
            result.Success = true;
            result.Message = "You have reached the Victory Chamber!";
            result.NewLevel = CurrentLevel;
            result.VictoryTriggered = true;

            OnDungeonCompleted?.Invoke();

            return result;
        }

        // Save current level state
        SaveLevelState(CurrentLevel);

        // Move to next level
        int nextLevel = CurrentLevel + 1;

        // Generate or restore level
        if (_levelCache.ContainsKey(nextLevel))
        {
            RestoreLevelState(nextLevel);
        }
        else
        {
            GenerateNewLevel(nextLevel);
        }

        CurrentLevel = nextLevel;

        // Update personal best
        bool newRecord = UpdatePersonalBest(CurrentLevel);

        if (newRecord)
        {
            OnNewDepthReached?.Invoke(CurrentLevel);
        }

        OnLevelCompleted?.Invoke(CurrentLevel - 1);

        // Place player at upward staircase
        PlacePlayerAtStaircase(player, Components.StaircaseDirection.Up);

        result.Success = true;
        result.Message = $"Descending to Level {CurrentLevel}...";
        result.NewLevel = CurrentLevel;
        result.NewRecordDepth = newRecord;
        result.VictoryTriggered = false;

        return result;
    }

    /// <summary>
    /// Ascend to the previous dungeon level
    /// </summary>
    public LevelTransitionResult AscendLevel(Entity player)
    {
        var result = new LevelTransitionResult();

        if (CurrentLevel <= 1)
        {
            result.Success = false;
            result.Message = "You cannot ascend further.";
            result.NewLevel = CurrentLevel;
            return result;
        }

        // Save current level state
        SaveLevelState(CurrentLevel);

        // Move to previous level
        int prevLevel = CurrentLevel - 1;

        // Restore previous level
        if (_levelCache.ContainsKey(prevLevel))
        {
            RestoreLevelState(prevLevel);
        }
        else
        {
            // This shouldn't happen (we should have cached it on the way down)
            GenerateNewLevel(prevLevel);
        }

        CurrentLevel = prevLevel;

        // Place player at downward staircase
        PlacePlayerAtStaircase(player, Components.StaircaseDirection.Down);

        result.Success = true;
        result.Message = $"Ascending to Level {CurrentLevel}...";
        result.NewLevel = CurrentLevel;
        result.NewRecordDepth = false;

        return result;
    }

    /// <summary>
    /// Generate a new dungeon level
    /// </summary>
    private void GenerateNewLevel(int levelNumber)
    {
        // Generate map
        var map = _mapGenerator.GenerateRoomsAndCorridors(80, 50);

        // Create level
        var level = new DungeonLevel(levelNumber, map);

        // Place staircases
        PlaceStaircases(level);

        // Cache the level
        _levelCache[levelNumber] = level;

        // Manage cache size
        ManageLevelCache();
    }

    /// <summary>
    /// Place upward and downward staircases
    /// </summary>
    private void PlaceStaircases(DungeonLevel level)
    {
        // Find all walkable positions
        var walkablePositions = new List<Point>();
        for (int x = 0; x < level.Map.Width; x++)
        {
            for (int y = 0; y < level.Map.Height; y++)
            {
                var pos = new Point(x, y);
                if (level.Map.IsWalkable(pos))
                {
                    walkablePositions.Add(pos);
                }
            }
        }

        if (walkablePositions.Count < 2)
            return;

        // Place upward staircase (except on level 1)
        if (level.LevelNumber > 1)
        {
            var upPos = walkablePositions[_random.Next(walkablePositions.Count)];
            level.UpStaircasePosition = upPos;
            walkablePositions.Remove(upPos);

            var upStairs = _world.Create(
                new Staircase(Components.StaircaseDirection.Up, level.LevelNumber - 1),
                new Position(upPos),
                new Renderable('<', Color.White)
            );
        }

        // Place downward staircase (except on victory level in non-endless mode)
        if (EndlessModeEnabled || level.LevelNumber < VictoryLevel)
        {
            var downPos = walkablePositions[_random.Next(walkablePositions.Count)];
            level.DownStaircasePosition = downPos;

            var downStairs = _world.Create(
                new Staircase(Components.StaircaseDirection.Down, level.LevelNumber + 1),
                new Position(downPos),
                new Renderable('>', Color.White)
            );
        }
    }

    /// <summary>
    /// Save current level state before leaving
    /// </summary>
    private void SaveLevelState(int levelNumber)
    {
        if (!_levelCache.ContainsKey(levelNumber))
            return;

        var level = _levelCache[levelNumber];
        level.Entities.Clear();
        level.LastVisited = DateTime.UtcNow;

        // Snapshot all non-player entities
        var query = new QueryDescription().WithAll<Position>().WithNone<Player>();
        _world.Query(in query, (Entity entity) =>
        {
            var snapshot = SnapshotEntity(entity);
            level.Entities.Add(snapshot);
        });
    }

    /// <summary>
    /// Restore a previously visited level
    /// </summary>
    private void RestoreLevelState(int levelNumber)
    {
        if (!_levelCache.ContainsKey(levelNumber))
            return;

        var level = _levelCache[levelNumber];

        // Clear all non-player entities
        var query = new QueryDescription().WithAll<Position>().WithNone<Player>();
        var entitiesToDestroy = new List<Entity>();
        _world.Query(in query, (Entity entity) =>
        {
            entitiesToDestroy.Add(entity);
        });

        foreach (var entity in entitiesToDestroy)
        {
            _world.Destroy(entity);
        }

        // Restore entities from snapshots
        foreach (var snapshot in level.Entities.Where(s => s.IsActive))
        {
            RestoreEntity(snapshot);
        }
    }

    /// <summary>
    /// Create a snapshot of an entity
    /// </summary>
    private EntitySnapshot SnapshotEntity(Entity entity)
    {
        var snapshot = new EntitySnapshot();

        // Determine archetype
        if (_world.Has<Enemy>(entity))
            snapshot.Archetype = "Enemy";
        else if (_world.Has<Item>(entity))
            snapshot.Archetype = "Item";
        else if (_world.Has<Staircase>(entity))
            snapshot.Archetype = "Staircase";
        else
            snapshot.Archetype = "Other";

        // Snapshot components
        if (_world.Has<Position>(entity))
            snapshot.Components["Position"] = _world.Get<Position>(entity);
        if (_world.Has<Health>(entity))
            snapshot.Components["Health"] = _world.Get<Health>(entity);
        if (_world.Has<Combat>(entity))
            snapshot.Components["Combat"] = _world.Get<Combat>(entity);
        if (_world.Has<Enemy>(entity))
            snapshot.Components["Enemy"] = _world.Get<Enemy>(entity);
        if (_world.Has<Item>(entity))
            snapshot.Components["Item"] = _world.Get<Item>(entity);
        if (_world.Has<Staircase>(entity))
            snapshot.Components["Staircase"] = _world.Get<Staircase>(entity);
        if (_world.Has<Renderable>(entity))
            snapshot.Components["Renderable"] = _world.Get<Renderable>(entity);
        if (_world.Has<Actor>(entity))
            snapshot.Components["Actor"] = _world.Get<Actor>(entity);

        // Mark as inactive if dead
        if (_world.Has<Health>(entity))
        {
            var health = _world.Get<Health>(entity);
            snapshot.IsActive = health.IsAlive;
        }

        return snapshot;
    }

    /// <summary>
    /// Restore an entity from a snapshot
    /// </summary>
    private Entity RestoreEntity(EntitySnapshot snapshot)
    {
        var entity = _world.Create();

        // Restore components
        foreach (var kvp in snapshot.Components)
        {
            switch (kvp.Key)
            {
                case "Position":
                    _world.Add(entity, (Position)kvp.Value);
                    break;
                case "Health":
                    _world.Add(entity, (Health)kvp.Value);
                    break;
                case "Combat":
                    _world.Add(entity, (Combat)kvp.Value);
                    break;
                case "Enemy":
                    _world.Add(entity, (Enemy)kvp.Value);
                    break;
                case "Item":
                    _world.Add(entity, (Item)kvp.Value);
                    break;
                case "Staircase":
                    _world.Add(entity, (Staircase)kvp.Value);
                    break;
                case "Renderable":
                    _world.Add(entity, (Renderable)kvp.Value);
                    break;
                case "Actor":
                    _world.Add(entity, (Actor)kvp.Value);
                    break;
            }
        }

        return entity;
    }

    /// <summary>
    /// Place player at a specific staircase
    /// </summary>
    private void PlacePlayerAtStaircase(Entity player, Components.StaircaseDirection direction)
    {
        if (!_levelCache.ContainsKey(CurrentLevel))
            return;

        var level = _levelCache[CurrentLevel];
        Point targetPos = direction == Components.StaircaseDirection.Up
            ? level.UpStaircasePosition
            : level.DownStaircasePosition;

        if (targetPos != Point.None)
        {
            _world.Set(player, new Position(targetPos));
        }
    }

    /// <summary>
    /// Update personal best depth
    /// </summary>
    public bool UpdatePersonalBest(int newLevel)
    {
        if (newLevel > PersonalBestDepth)
        {
            PersonalBestDepth = newLevel;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get the staircase entity at the player's position
    /// </summary>
    public Entity? GetPlayerStaircase(Entity player)
    {
        if (!_world.Has<Position>(player))
            return null;

        var playerPos = _world.Get<Position>(player);

        Entity? foundStaircase = null;
        var query = new QueryDescription().WithAll<Staircase, Position>();
        _world.Query(in query, (Entity entity) =>
        {
            var staircasePos = _world.Get<Position>(entity);
            if (staircasePos.Point == playerPos.Point)
            {
                foundStaircase = entity;
            }
        });

        return foundStaircase;
    }

    /// <summary>
    /// Check if player can transition (not in combat, on staircase)
    /// </summary>
    public bool CanTransition(Entity player, Components.StaircaseDirection direction)
    {
        var staircase = GetPlayerStaircase(player);
        if (staircase == null)
            return false;

        var staircaseComp = _world.Get<Staircase>(staircase.Value);
        return staircaseComp.Direction == direction;
    }

    /// <summary>
    /// Manage level cache size
    /// </summary>
    private void ManageLevelCache()
    {
        if (_levelCache.Count <= MAX_CACHED_LEVELS)
            return;

        // Remove furthest levels from current
        var levelsToRemove = _levelCache.Keys
            .OrderByDescending(level => Math.Abs(level - CurrentLevel))
            .Skip(MAX_CACHED_LEVELS)
            .ToList();

        foreach (var level in levelsToRemove)
        {
            _levelCache.Remove(level);
        }
    }

    /// <summary>
    /// Get current dungeon map
    /// </summary>
    public DungeonMap? GetCurrentMap()
    {
        return _levelCache.ContainsKey(CurrentLevel) ? _levelCache[CurrentLevel].Map : null;
    }

    /// <summary>
    /// Check if a level is cached
    /// </summary>
    public bool IsLevelCached(int levelNumber)
    {
        return _levelCache.ContainsKey(levelNumber);
    }
}
