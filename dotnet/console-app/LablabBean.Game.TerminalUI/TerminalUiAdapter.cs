using Arch.Core;
using LablabBean.Contracts.Game.Models;
using LablabBean.Contracts.Game.UI;
using LablabBean.Contracts.UI.Models;
using LablabBean.Contracts.UI.Services;
using LablabBean.Game.Core.Maps;
using LablabBean.Game.Core.Systems;
using LablabBean.Game.TerminalUI.Services;
using LablabBean.Game.TerminalUI.Views;
using LablabBean.Rendering.Contracts;
using Microsoft.Extensions.Logging;
using Terminal.Gui;

namespace LablabBean.Game.TerminalUI;

/// <summary>
/// Terminal.Gui adapter implementing IUiService and IDungeonCrawlerUI.
/// Phase 3: Full Terminal.Gui v2 API implementation with HUD, WorldView, and ActivityLog.
/// </summary>
public class TerminalUiAdapter : IService, IDungeonCrawlerUI
{
    private readonly ISceneRenderer _sceneRenderer;
    private readonly ILogger _logger;
    private Window? _mainWindow;
    private HudService? _hudService;
    private WorldViewService? _worldViewService;
    private ActivityLogView? _activityLogView;
    private World? _currentWorld;
    private DungeonMap? _currentMap;

    public TerminalUiAdapter(ISceneRenderer sceneRenderer, ILogger logger)
    {
        _sceneRenderer = sceneRenderer;
        _logger = logger;
    }

    #region IService Implementation

    public Task InitializeAsync(UIInitOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing Terminal UI Adapter");
        Initialize();
        return Task.CompletedTask;
    }

    public Task RenderViewportAsync(ViewportBounds viewport, IReadOnlyCollection<Contracts.Game.Models.EntitySnapshot> entities)
    {
        _logger.LogDebug("Render viewport: {EntityCount} entities", entities.Count);

        // If we have a world and map, render them
        if (_currentWorld != null && _currentMap != null && _worldViewService != null && _hudService != null)
        {
            _worldViewService.Render(_currentWorld, _currentMap);
            _hudService.Update(_currentWorld);
        }

        return Task.CompletedTask;
    }

    public Task UpdateDisplayAsync()
    {
        _logger.LogDebug("Display update requested");
        return Task.CompletedTask;
    }

    public Task HandleInputAsync(InputCommand command)
    {
        _logger.LogDebug("Input command: {Command}", command);
        return Task.CompletedTask;
    }

    public ViewportBounds GetViewport()
    {
        return new ViewportBounds(new Position(0, 0), 80, 24);
    }

    public void SetViewportCenter(Position centerPosition)
    {
        _logger.LogDebug("Set viewport center: ({X}, {Y})", centerPosition.X, centerPosition.Y);
    }

    public void Initialize()
    {
        _mainWindow = new Window
        {
            Title = "LablabBean - Dungeon Crawler",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        // Initialize services (placeholder - need proper DI)
        // For now, create with temporary logger adapters
        var inventorySystem = new InventorySystem(new LoggerAdapter<InventorySystem>(_logger));
        _hudService = new HudService(new LoggerAdapter<HudService>(_logger), inventorySystem);
        _worldViewService = new WorldViewService(new LoggerAdapter<WorldViewService>(_logger));
        _activityLogView = new ActivityLogView("Activity Log");

        // Add views to main window
        _mainWindow.Add(_worldViewService.WorldView);
        _mainWindow.Add(_hudService.HudView);
        _mainWindow.Add(_activityLogView);

        _logger.LogInformation("Terminal UI adapter initialized with full HUD, WorldView, and ActivityLog");
    }

    public Window GetMainWindow()
    {
        return _mainWindow ?? throw new InvalidOperationException("UI not initialized");
    }

    #endregion

    #region IDungeonCrawlerUI Implementation

    public void ToggleHud()
    {
        _logger.LogInformation("HUD toggle requested");
    }

    public void ShowDialogue(string speaker, string text, string[]? choices = null)
    {
        _logger.LogInformation("Dialogue: {Speaker} - {Text}", speaker, text);
    }

    public void HideDialogue()
    {
        _logger.LogDebug("Hide dialogue requested");
    }

    public void ShowQuests()
    {
        _logger.LogInformation("Show quests requested");
    }

    public void HideQuests()
    {
        _logger.LogDebug("Hide quests requested");
    }

    public void ShowInventory()
    {
        _logger.LogInformation("Show inventory requested");
    }

    public void HideInventory()
    {
        _logger.LogDebug("Hide inventory requested");
    }

    public void UpdatePlayerStats(int health, int maxHealth, int mana, int maxMana, int level, int experience)
    {
        _logger.LogDebug("Player stats updated: HP {Health}/{MaxHealth}, Mana {Mana}/{MaxMana}, Level {Level}, XP {Experience}",
            health, maxHealth, mana, maxMana, level, experience);

        // Update HUD if available
        if (_hudService != null && _currentWorld != null)
        {
            _hudService.Update(_currentWorld);
        }
    }

    public void SetCameraFollow(int entityId)
    {
        _logger.LogDebug("Camera follow entity: {EntityId}", entityId);
    }

    // Helper method to set current world and map for rendering
    public void SetWorldContext(World world, DungeonMap map)
    {
        _currentWorld = world;
        _currentMap = map;
    }

    #endregion
}

/// <summary>
/// Helper class to adapt ILogger to ILogger<T>
/// </summary>
internal class LoggerAdapter<T> : ILogger<T>
{
    private readonly ILogger _logger;

    public LoggerAdapter(ILogger logger)
    {
        _logger = logger;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => _logger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel)
        => _logger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => _logger.Log(logLevel, eventId, state, exception, formatter);
}
