using Arch.Core;
using Microsoft.Extensions.Logging;

namespace LablabBean.Game.Core.Worlds;

/// <summary>
/// Manages multiple ECS worlds for different game modes (Play and Edit)
/// Each mode has its own world snapshot that can be switched between
/// </summary>
public class GameWorldManager : IDisposable
{
    private readonly ILogger<GameWorldManager> _logger;
    private readonly Dictionary<GameMode, World> _worlds = new();
    private GameMode _currentMode = GameMode.Play;
    private bool _disposed;

    public GameWorldManager(ILogger<GameWorldManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeWorlds();
    }

    /// <summary>
    /// Gets the current active game mode
    /// </summary>
    public GameMode CurrentMode => _currentMode;

    /// <summary>
    /// Gets the current active world for the current mode
    /// </summary>
    public World CurrentWorld => _worlds[_currentMode];

    /// <summary>
    /// Gets a specific world by game mode
    /// </summary>
    public World GetWorld(GameMode mode) => _worlds[mode];

    /// <summary>
    /// Switches to a different game mode, activating its world
    /// </summary>
    public void SwitchMode(GameMode newMode)
    {
        if (_currentMode == newMode)
        {
            _logger.LogDebug("Already in {Mode} mode", newMode);
            return;
        }

        var previousMode = _currentMode;
        _currentMode = newMode;

        _logger.LogInformation("Switched from {PreviousMode} to {NewMode} mode", previousMode, newMode);
        OnModeChanged?.Invoke(previousMode, newMode);
    }

    /// <summary>
    /// Event raised when the game mode changes
    /// </summary>
    public event Action<GameMode, GameMode>? OnModeChanged;

    /// <summary>
    /// Creates a snapshot of the current world state
    /// Useful for saving or copying world state
    /// </summary>
    public WorldSnapshot CreateSnapshot(GameMode mode)
    {
        var world = _worlds[mode];
        var entities = new List<Entity>();

        // Query all entities in the world
        var query = new QueryDescription();
        world.Query(in query, (in Entity entity) => entities.Add(entity));

        _logger.LogDebug("Created snapshot of {Mode} world with {EntityCount} entities", mode, entities.Count);
        return new WorldSnapshot(mode, entities);
    }

    /// <summary>
    /// Clears all entities from a specific world
    /// </summary>
    public void ClearWorld(GameMode mode)
    {
        var world = _worlds[mode];
        world.Clear();
        _logger.LogInformation("Cleared {Mode} world", mode);
    }

    /// <summary>
    /// Resets a world by disposing and recreating it
    /// </summary>
    public void ResetWorld(GameMode mode)
    {
        var world = _worlds[mode];
        world.Dispose();
        _worlds[mode] = World.Create();
        _logger.LogInformation("Reset {Mode} world", mode);
    }

    private void InitializeWorlds()
    {
        // Create a world for each game mode
        foreach (GameMode mode in Enum.GetValues<GameMode>())
        {
            _worlds[mode] = World.Create();
            _logger.LogDebug("Initialized {Mode} world", mode);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var (mode, world) in _worlds)
        {
            world.Dispose();
            _logger.LogDebug("Disposed {Mode} world", mode);
        }

        _worlds.Clear();
        _disposed = true;
    }
}

/// <summary>
/// Represents a snapshot of a world state at a point in time
/// </summary>
public record WorldSnapshot(GameMode Mode, List<Entity> Entities);
