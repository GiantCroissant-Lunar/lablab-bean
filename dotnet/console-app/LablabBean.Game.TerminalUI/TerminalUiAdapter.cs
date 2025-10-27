using Arch.Core;
using LablabBean.Contracts.Game.Models;
using LablabBean.Contracts.Game.UI;
using LablabBean.Contracts.UI.Models;
using LablabBean.Contracts.UI.Services;
using LablabBean.Rendering.Contracts;
using Microsoft.Extensions.Logging;
using Terminal.Gui;

namespace LablabBean.Game.TerminalUI;

/// <summary>
/// Terminal.Gui adapter implementing IUiService and IDungeonCrawlerUI.
/// Phase 2: Minimal implementation to get the architecture in place.
/// Full Terminal.Gui API compatibility will be addressed in Phase 3.
/// </summary>
public class TerminalUiAdapter : IService, IDungeonCrawlerUI
{
    private readonly ISceneRenderer _sceneRenderer;
    private readonly ILogger _logger;
    private Window? _mainWindow;

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

    public Task RenderViewportAsync(ViewportBounds viewport, IReadOnlyCollection<EntitySnapshot> entities)
    {
        _logger.LogDebug("Render viewport: {EntityCount} entities", entities.Count);
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
            Title = "LablabBean - Dungeon Crawler (Phase 2 - Minimal UI)",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var label = new Label
        {
            Text = "Terminal UI Adapter - Phase 2 Implementation\nPress Q to quit",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        _mainWindow.Add(label);
        _logger.LogInformation("Terminal UI adapter initialized (minimal placeholder)");
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
    }

    public void SetCameraFollow(int entityId)
    {
        _logger.LogDebug("Camera follow entity: {EntityId}", entityId);
    }

    #endregion
}
