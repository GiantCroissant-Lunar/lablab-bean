// Contract: LevelManager
// Purpose: Defines the interface for dungeon level management and transitions
// Location: LablabBean.Game.Core/Maps/LevelManager.cs

namespace LablabBean.Game.Core.Maps;

using Arch.Core;
using LablabBean.Game.Core.Components;

/// <summary>
/// Manages dungeon level persistence, transitions, and state.
/// Handles level caching, entity snapshotting, and player transitions.
/// </summary>
public class LevelManager
{
    private readonly World _world;
    private readonly MapGenerator _mapGenerator;
    private readonly DifficultyScalingSystem _difficultyScaling;
    private readonly Dictionary<int, DungeonLevel> _levelCache;

    public int CurrentLevel { get; private set; }
    public int PersonalBestDepth { get; private set; }
    public bool EndlessModeEnabled { get; set; }

    public const int MaxScalingLevel = 30;
    public const int VictoryLevel = 20;

    public LevelManager(World world, MapGenerator mapGenerator, DifficultyScalingSystem difficultyScaling)
    {
        _world = world;
        _mapGenerator = mapGenerator;
        _difficultyScaling = difficultyScaling;
        _levelCache = new Dictionary<int, DungeonLevel>();
        CurrentLevel = 1;
        PersonalBestDepth = 1;
    }

    // ═══════════════════════════════════════════════════════════════
    // LEVEL TRANSITIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Transitions player to a different dungeon level.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="targetLevel">Target level number</param>
    /// <param name="direction">Direction of transition (up/down)</param>
    /// <returns>Result indicating success or failure</returns>
    /// <remarks>
    /// Workflow:
    /// 1. Validate transition (can't ascend from level 1, etc.)
    /// 2. Save current level state (snapshot entities)
    /// 3. Load or generate target level
    /// 4. Place player at appropriate staircase
    /// 5. Update current level and personal best
    /// 6. Return feedback message
    ///
    /// Preconditions:
    /// - Player must be on a staircase tile
    /// - Target level must be valid (1-30 for normal, unlimited for endless)
    /// - Direction must match staircase type
    ///
    /// Effects:
    /// - Current level saved to cache
    /// - Target level loaded from cache or generated
    /// - Player position updated
    /// - CurrentLevel updated
    /// - PersonalBestDepth updated if new record
    /// </remarks>
    public LevelTransitionResult TransitionToLevel(
        Entity playerEntity,
        int targetLevel,
        StaircaseDirection direction);

    /// <summary>
    /// Descends player to the next level down.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <returns>Result indicating success or failure</returns>
    public LevelTransitionResult DescendLevel(Entity playerEntity);

    /// <summary>
    /// Ascends player to the previous level up.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <returns>Result indicating success or failure</returns>
    public LevelTransitionResult AscendLevel(Entity playerEntity);

    /// <summary>
    /// Checks if player can transition in the given direction.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <param name="direction">Direction to check</param>
    /// <returns>True if transition is possible</returns>
    public bool CanTransition(Entity playerEntity, StaircaseDirection direction);

    // ═══════════════════════════════════════════════════════════════
    // LEVEL GENERATION & LOADING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Generates a new dungeon level.
    /// </summary>
    /// <param name="levelNumber">Level number to generate</param>
    /// <returns>Generated dungeon level</returns>
    /// <remarks>
    /// - Generates map with rooms and corridors
    /// - Places up/down staircases
    /// - Spawns enemies with difficulty scaling
    /// - Spawns items with loot scaling
    /// - Caches generated level
    /// </remarks>
    public DungeonLevel GenerateLevel(int levelNumber);

    /// <summary>
    /// Loads a level from cache or generates if not cached.
    /// </summary>
    /// <param name="levelNumber">Level number to load</param>
    /// <returns>Dungeon level (cached or newly generated)</returns>
    public DungeonLevel GetOrGenerateLevel(int levelNumber);

    /// <summary>
    /// Checks if a level exists in the cache.
    /// </summary>
    /// <param name="levelNumber">Level number to check</param>
    /// <returns>True if level is cached</returns>
    public bool IsLevelCached(int levelNumber);

    /// <summary>
    /// Generates the victory chamber (level 20 final boss).
    /// </summary>
    /// <returns>Victory chamber level</returns>
    public DungeonLevel GenerateVictoryChamber();

    // ═══════════════════════════════════════════════════════════════
    // STATE PERSISTENCE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Saves the current level state to cache.
    /// </summary>
    /// <param name="levelNumber">Level number to save</param>
    /// <remarks>
    /// - Snapshots all entities (enemies, items, staircases)
    /// - Stores map data
    /// - Excludes player entity
    /// - Caches in _levelCache dictionary
    /// </remarks>
    public void SaveLevelState(int levelNumber);

    /// <summary>
    /// Restores a level from cache.
    /// </summary>
    /// <param name="levelNumber">Level number to restore</param>
    /// <remarks>
    /// - Loads map data
    /// - Recreates entities from snapshots
    /// - Restores entity positions and components
    /// - Skips inactive entities (dead enemies, picked-up items)
    /// </remarks>
    public void RestoreLevelState(int levelNumber);

    /// <summary>
    /// Creates a snapshot of all entities on the current level.
    /// </summary>
    /// <returns>List of entity snapshots</returns>
    public List<EntitySnapshot> SnapshotEntities();

    /// <summary>
    /// Recreates entities from snapshots.
    /// </summary>
    /// <param name="snapshots">List of entity snapshots</param>
    public void RestoreEntities(List<EntitySnapshot> snapshots);

    /// <summary>
    /// Snapshots a single entity.
    /// </summary>
    /// <param name="entity">Entity to snapshot</param>
    /// <returns>Entity snapshot</returns>
    public EntitySnapshot SnapshotEntity(Entity entity);

    /// <summary>
    /// Restores a single entity from snapshot.
    /// </summary>
    /// <param name="snapshot">Entity snapshot</param>
    /// <returns>Recreated entity</returns>
    public Entity RestoreEntity(EntitySnapshot snapshot);

    // ═══════════════════════════════════════════════════════════════
    // STAIRCASE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Places staircases in a generated level.
    /// </summary>
    /// <param name="level">Dungeon level</param>
    /// <param name="rooms">List of rooms in the level</param>
    /// <remarks>
    /// - Downward staircase: Placed in farthest room from start
    /// - Upward staircase: Placed near start (except level 1)
    /// - Minimum distance: 30+ tiles apart
    /// </remarks>
    public void PlaceStaircases(DungeonLevel level, List<Rectangle> rooms);

    /// <summary>
    /// Gets the staircase entity at the player's position.
    /// </summary>
    /// <param name="playerPosition">Player's current position</param>
    /// <returns>Staircase entity if found, null otherwise</returns>
    public Entity? GetStaircaseAtPosition(Position playerPosition);

    /// <summary>
    /// Checks if player is standing on a staircase.
    /// </summary>
    /// <param name="playerEntity">The player entity</param>
    /// <returns>Staircase entity if standing on one, null otherwise</returns>
    public Entity? GetPlayerStaircase(Entity playerEntity);

    // ═══════════════════════════════════════════════════════════════
    // DEPTH TRACKING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Updates the player's personal best depth.
    /// </summary>
    /// <param name="level">Current level number</param>
    /// <returns>True if new record was set</returns>
    public bool UpdatePersonalBest(int level);

    /// <summary>
    /// Calculates depth in feet for a given level.
    /// </summary>
    /// <param name="level">Level number</param>
    /// <returns>Depth in feet (negative value)</returns>
    public int CalculateDepthInFeet(int level);

    /// <summary>
    /// Gets formatted depth string for HUD display.
    /// </summary>
    /// <param name="level">Level number</param>
    /// <returns>Formatted string (e.g., "Depth: -150 ft")</returns>
    public string GetDepthDisplayString(int level);

    // ═══════════════════════════════════════════════════════════════
    // CACHE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Evicts levels from cache that are far from current level.
    /// </summary>
    /// <param name="keepRange">Number of levels to keep above/below current (default: 5)</param>
    /// <remarks>
    /// Keeps only levels within ±keepRange of current level to save memory.
    /// Example: Current level 10, keepRange 5 → Keep levels 5-15, evict others
    /// </remarks>
    public void EvictDistantLevels(int keepRange = 5);

    /// <summary>
    /// Clears all cached levels.
    /// </summary>
    public void ClearCache();

    /// <summary>
    /// Gets the number of cached levels.
    /// </summary>
    /// <returns>Number of levels in cache</returns>
    public int GetCachedLevelCount();

    // ═══════════════════════════════════════════════════════════════
    // VICTORY CONDITION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if the player has reached the victory condition.
    /// </summary>
    /// <param name="level">Current level number</param>
    /// <returns>True if victory condition is met</returns>
    public bool IsVictoryLevel(int level);

    /// <summary>
    /// Checks if the player should transition to victory chamber.
    /// </summary>
    /// <param name="currentLevel">Current level number</param>
    /// <param name="direction">Direction of transition</param>
    /// <returns>True if should transition to victory chamber</returns>
    public bool ShouldTransitionToVictoryChamber(int currentLevel, StaircaseDirection direction);
}

/// <summary>
/// Result of a level transition operation.
/// </summary>
public struct LevelTransitionResult
{
    /// <summary>Whether the transition succeeded</summary>
    public bool Success { get; set; }

    /// <summary>Feedback message to display</summary>
    public string Message { get; set; }

    /// <summary>New level number after transition</summary>
    public int NewLevel { get; set; }

    /// <summary>Whether a new personal best was achieved</summary>
    public bool NewRecordDepth { get; set; }

    /// <summary>Whether victory condition was triggered</summary>
    public bool VictoryTriggered { get; set; }

    public static LevelTransitionResult Succeeded(string message, int newLevel, bool newRecord = false, bool victory = false)
        => new() { Success = true, Message = message, NewLevel = newLevel, NewRecordDepth = newRecord, VictoryTriggered = victory };

    public static LevelTransitionResult Failed(string message)
        => new() { Success = false, Message = message };
}

/// <summary>
/// Dungeon level data container.
/// </summary>
public class DungeonLevel
{
    public int LevelNumber { get; set; }
    public DungeonMap Map { get; set; }
    public List<EntitySnapshot> Entities { get; set; }
    public Position? UpStaircasePosition { get; set; }
    public Position DownStaircasePosition { get; set; }
    public Position PlayerSpawnPosition { get; set; }
    public bool IsVisited { get; set; }
    public DateTime LastVisited { get; set; }
    public double DifficultyMultiplier { get; set; }
    public double LootDropRate { get; set; }
}

/// <summary>
/// Entity snapshot for level persistence.
/// </summary>
public struct EntitySnapshot
{
    public EntityArchetype Archetype { get; set; }
    public Dictionary<string, object> Components { get; set; }
    public Position Position { get; set; }
    public bool IsActive { get; set; }
}

public enum EntityArchetype
{
    Enemy,
    Item,
    Staircase,
    Decoration
}

/// <summary>
/// Simple rectangle for room boundaries.
/// </summary>
public struct Rectangle
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public (int x, int y) Center => (X + Width / 2, Y + Height / 2);

    public bool Contains(int x, int y)
        => x >= X && x < X + Width && y >= Y && y < Y + Height;
}
