using Arch.Core;
using LablabBean.Contracts.Game.Models;
using LablabBean.Contracts.Game.UI;
using LablabBean.Contracts.Game.UI.Services;
using LablabBean.Contracts.UI.Models;
using LablabBean.Contracts.UI.Services;
using LablabBean.Game.Core.Maps;
using LablabBean.Game.TerminalUI.Services;
using LablabBean.Game.TerminalUI.Views;
using Microsoft.Extensions.Logging;
using Terminal.Gui;
using GameEntitySnapshot = LablabBean.Contracts.Game.Models.EntitySnapshot;

namespace LablabBean.Game.TerminalUI;

/// <summary>
/// Terminal.Gui adapter implementing IUiService and IDungeonCrawlerUI.
/// Orchestrates HUD, world view, activity log, and dialogue/quest/inventory panels.
/// </summary>
public class TerminalUiAdapter : IService, IDungeonCrawlerUI
{
    private readonly ILogger<TerminalUiAdapter> _logger;
    private readonly HudService _hudService;
    private readonly WorldViewService _worldViewService;
    private readonly ActivityLogView _activityLogView;

    private Window? _mainWindow;
    private Window? _dialogueWindow;
    private Window? _questWindow;
    private ViewportBounds _viewport;
    private Position _cameraCenter;
    private bool _initialized;
    private bool _hudVisible = true;

    private World? _world;
    private DungeonMap? _map;

    public TerminalUiAdapter(
        ILogger<TerminalUiAdapter> logger,
        HudService hudService,
        WorldViewService worldViewService)
    {
        _logger = logger;
        _hudService = hudService;
        _worldViewService = worldViewService;
        _activityLogView = new ActivityLogView("Activity");

        _viewport = new ViewportBounds(0, 0, 80, 24);
        _cameraCenter = new Position(0, 0);
    }

    public async Task InitializeAsync(UIInitOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing Terminal UI adapter");

        Application.Init();

        _mainWindow = new Window
        {
            Title = options.Title ?? "LablabBean - Dungeon Crawler",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.Single
        };

        _mainWindow.Add(_worldViewService.WorldView);
        _mainWindow.Add(_hudService.HudView);
        _mainWindow.Add(_activityLogView);

        _mainWindow.KeyDown += HandleKeyDown;

        if (Application.Top != null)
        {
            Application.Top.Add(_mainWindow);
        }

        _initialized = true;
        _logger.LogInformation("Terminal UI adapter initialized");

        await Task.CompletedTask;
    }

    public Task RenderViewportAsync(ViewportBounds viewport, IReadOnlyCollection<GameEntitySnapshot> entities)
    {
        _viewport = viewport;

        if (_world != null && _map != null)
        {
            _worldViewService.Render(_world, _map);
        }

        return Task.CompletedTask;
    }

    public Task UpdateDisplayAsync()
    {
        if (_world != null)
        {
            _hudService.Update(_world);
        }

        Application.Refresh();
        return Task.CompletedTask;
    }

    public Task HandleInputAsync(InputCommand command)
    {
        return Task.CompletedTask;
    }

    public ViewportBounds GetViewport()
    {
        return _viewport;
    }

    public void SetViewportCenter(Position centerPosition)
    {
        _cameraCenter = centerPosition;
        _logger.LogDebug("Camera centered at {X},{Y}", centerPosition.X, centerPosition.Y);
    }

    public void SetWorld(World world)
    {
        _world = world;
    }

    public void SetMap(DungeonMap map)
    {
        _map = map;
    }

    public void BindActivityLog(IActivityLog activityLog)
    {
        if (activityLog is LablabBean.Contracts.UI.Services.IActivityLogService legacyService)
        {
            _activityLogView.Bind(legacyService);
        }
    }

    #region IDungeonCrawlerUI Implementation

    public void ToggleHud()
    {
        _hudVisible = !_hudVisible;
        _hudService.HudView.Visible = _hudVisible;
        _logger.LogInformation("HUD visibility toggled: {Visible}", _hudVisible);
    }

    public void ShowDialogue(string speaker, string text, string[]? choices = null)
    {
        if (_dialogueWindow == null)
        {
            _dialogueWindow = new Window
            {
                Title = $"Dialogue - {speaker}",
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Percent(70),
                Height = Dim.Percent(50),
                BorderStyle = LineStyle.Double
            };

            var textLabel = new Label
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(2),
                Height = Dim.Fill(2),
                Text = text
            };
            _dialogueWindow.Add(textLabel);

            if (Application.Top != null)
            {
                Application.Top.Add(_dialogueWindow);
            }
        }
        else
        {
            _dialogueWindow.Title = $"Dialogue - {speaker}";
            if (_dialogueWindow.Subviews.Count > 0 && _dialogueWindow.Subviews[0] is Label label)
            {
                label.Text = text;
            }
            _dialogueWindow.Visible = true;
        }

        _logger.LogInformation("Showing dialogue from {Speaker}", speaker);
    }

    public void HideDialogue()
    {
        if (_dialogueWindow != null)
        {
            _dialogueWindow.Visible = false;
        }
        _logger.LogInformation("Dialogue hidden");
    }

    public void ShowQuests()
    {
        if (_questWindow == null)
        {
            _questWindow = new Window
            {
                Title = "Quest Log",
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Percent(80),
                Height = Dim.Percent(80),
                BorderStyle = LineStyle.Double
            };

            var questLabel = new Label
            {
                X = 1,
                Y = 1,
                Text = "No active quests."
            };
            _questWindow.Add(questLabel);

            if (Application.Top != null)
            {
                Application.Top.Add(_questWindow);
            }
        }
        else
        {
            _questWindow.Visible = true;
        }

        _logger.LogInformation("Quest log shown");
    }

    public void HideQuests()
    {
        if (_questWindow != null)
        {
            _questWindow.Visible = false;
        }
        _logger.LogInformation("Quest log hidden");
    }

    public void ShowInventory()
    {
        _logger.LogInformation("Inventory panel requested (not yet implemented)");
    }

    public void HideInventory()
    {
        _logger.LogInformation("Hide inventory requested");
    }

    public void UpdatePlayerStats(int health, int maxHealth, int mana, int maxMana, int level, int experience)
    {
        _logger.LogDebug("Player stats updated: HP={HP}/{MaxHP}, Level={Level}", health, maxHealth, level);
    }

    public void SetCameraFollow(int entityId)
    {
        _logger.LogInformation("Camera follow set to entity {EntityId}", entityId);
    }

    #endregion

    private void HandleKeyDown(Key.KeyEventEventArgs e)
    {
        if (e.KeyEvent.Key == Key.Q)
        {
            Application.RequestStop();
            e.Handled = true;
        }
        else if (e.KeyEvent.Key == Key.H)
        {
            ToggleHud();
            e.Handled = true;
        }
    }
}
