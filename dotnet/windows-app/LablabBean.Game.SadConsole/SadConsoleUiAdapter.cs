using Arch.Core;
using LablabBean.Contracts.Game.Models;
using LablabBean.Contracts.Game.UI;
using LablabBean.Contracts.UI.Models;
using LablabBean.Contracts.UI.Services;
using LablabBean.Game.Core.Maps;
using LablabBean.Game.SadConsole.Screens;
using LablabBean.Rendering.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LablabBean.Game.SadConsole;

/// <summary>
/// SadConsole adapter implementing IService and IDungeonCrawlerUI.
/// Wraps GameScreen and provides interface compliance for the plugin system.
/// </summary>
public class SadConsoleUiAdapter : IService, IDungeonCrawlerUI
{
    private readonly ISceneRenderer _sceneRenderer;
    private readonly ILogger<SadConsoleUiAdapter> _logger;
    private readonly IServiceProvider _serviceProvider;
    private GameScreen? _gameScreen;
    private World? _currentWorld;
    private DungeonMap? _currentMap;
    private bool _initialized;

    public SadConsoleUiAdapter(
        ISceneRenderer sceneRenderer,
        ILogger<SadConsoleUiAdapter> logger,
        IServiceProvider serviceProvider)
    {
        _sceneRenderer = sceneRenderer ?? throw new ArgumentNullException(nameof(sceneRenderer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    #region IService Implementation

    public Task InitializeAsync(UIInitOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing SadConsole UI Adapter");
        Initialize();
        return Task.CompletedTask;
    }

    public Task RenderViewportAsync(ViewportBounds viewport, IReadOnlyCollection<Contracts.Game.Models.EntitySnapshot> entities)
    {
        _logger.LogDebug("Render viewport: {EntityCount} entities", entities.Count);

        // TODO: Update GameScreen with new viewport data
        // The GameScreen will handle actual rendering through its Update/Draw cycle

        return Task.CompletedTask;
    }

    public Task UpdateDisplayAsync()
    {
        _logger.LogDebug("Display update requested");
        // SadConsole handles this automatically through its game loop
        return Task.CompletedTask;
    }

    public Task HandleInputAsync(InputCommand command)
    {
        _logger.LogDebug("Input command: {Command}", command);
        // TODO: Route input to GameScreen
        return Task.CompletedTask;
    }

    public ViewportBounds GetViewport()
    {
        // TODO: Get actual viewport from GameScreen
        return new ViewportBounds(new Position(0, 0), 80, 50);
    }

    public void SetViewportCenter(Position centerPosition)
    {
        _logger.LogDebug("Set viewport center: ({X}, {Y})", centerPosition.X, centerPosition.Y);
        // TODO: Update GameScreen camera
    }

    public void Initialize()
    {
        if (_initialized)
        {
            _logger.LogWarning("SadConsoleUiAdapter already initialized");
            return;
        }

        _logger.LogInformation("Creating SadConsole GameScreen");

        // Create GameScreen with proper dependencies from DI
        // Default dimensions - should match host configuration
        const int width = 120;
        const int height = 40;

        _gameScreen = ActivatorUtilities.CreateInstance<GameScreen>(_serviceProvider, width, height);
        _gameScreen.Initialize();

        _initialized = true;
        _logger.LogInformation("SadConsole UI adapter initialized with GameScreen ({Width}x{Height})", width, height);
    }

    public GameScreen? GetGameScreen()
    {
        return _gameScreen;
    }

    #endregion

    #region IDungeonCrawlerUI Implementation

    public void ToggleHud()
    {
        _logger.LogInformation("HUD toggle requested");
        // TODO: Toggle HUD visibility in GameScreen
    }

    public void ShowDialogue(string speaker, string text, string[]? choices = null)
    {
        _logger.LogInformation("Dialogue: {Speaker} - {Text}", speaker, text);
        // TODO: Show dialogue overlay in GameScreen
    }

    public void HideDialogue()
    {
        _logger.LogDebug("Hide dialogue requested");
        // TODO: Hide dialogue in GameScreen
    }

    public void ShowQuests()
    {
        _logger.LogInformation("Show quests requested");
        // TODO: Show quest panel in GameScreen
    }

    public void HideQuests()
    {
        _logger.LogDebug("Hide quests requested");
        // TODO: Hide quest panel in GameScreen
    }

    public void ShowInventory()
    {
        _logger.LogInformation("Show inventory requested");
        // TODO: Show inventory panel in GameScreen
    }

    public void HideInventory()
    {
        _logger.LogDebug("Hide inventory requested");
        // TODO: Hide inventory panel in GameScreen
    }

    public void UpdateCameraFollow(int entityX, int entityY)
    {
        _logger.LogDebug("Camera follow: ({X}, {Y})", entityX, entityY);
        // TODO: Update camera in GameScreen
    }

    public void SetCameraFollow(int entityId)
    {
        _logger.LogDebug("Set camera follow: entity {EntityId}", entityId);
        // TODO: Set camera to follow specific entity
    }

    public void UpdatePlayerStats(int health, int maxHealth, int mana, int maxMana, int level, int experience)
    {
        _logger.LogDebug("Update player stats: HP {Health}/{MaxHealth}, MP {Mana}/{MaxMana}, Lvl {Level}, XP {Experience}",
            health, maxHealth, mana, maxMana, level, experience);
        // TODO: Update HUD with player stats
    }

    #endregion

    #region Game State Management

    public void SetWorldContext(World world, DungeonMap map)
    {
        _logger.LogInformation("Setting world context");
        _currentWorld = world;
        _currentMap = map;

        // TODO: Update GameScreen with new world/map
    }

    #endregion
}
