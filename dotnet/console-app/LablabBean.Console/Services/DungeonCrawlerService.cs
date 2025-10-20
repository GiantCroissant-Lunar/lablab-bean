using LablabBean.Game.Core.Services;
using LablabBean.Game.Core.Systems;
using LablabBean.Game.Core.Worlds;
using LablabBean.Game.TerminalUI.Services;
using Microsoft.Extensions.Logging;
using Terminal.Gui;

namespace LablabBean.Console.Services;

/// <summary>
/// Service that manages the dungeon crawler game in the console app
/// </summary>
public class DungeonCrawlerService : IDisposable
{
    private readonly ILogger<DungeonCrawlerService> _logger;
    private readonly GameStateManager _gameStateManager;
    private readonly HudService _hudService;
    private readonly WorldViewService _worldViewService;
    private Window? _gameWindow;
    private bool _disposed;
    private bool _isRunning;

    public DungeonCrawlerService(
        ILogger<DungeonCrawlerService> logger,
        GameStateManager gameStateManager,
        HudService hudService,
        WorldViewService worldViewService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));
        _hudService = hudService ?? throw new ArgumentNullException(nameof(hudService));
        _worldViewService = worldViewService ?? throw new ArgumentNullException(nameof(worldViewService));
    }

    /// <summary>
    /// Initializes the game window
    /// </summary>
    public Window CreateGameWindow()
    {
        _gameWindow = new Window()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Title = "Dungeon Crawler",
            CanFocus = true,
            BorderStyle = LineStyle.Single
        };

        // Add world view and HUD
        _gameWindow.Add(_worldViewService.WorldView);
        _gameWindow.Add(_hudService.HudView);

        // Set up key bindings using Terminal.Gui v2 KeyDown event
        _gameWindow.KeyDown += OnWindowKeyDown;
        
        // Set up a handler to render after layout is complete
        _gameWindow.LayoutComplete += (s, e) =>
        {
            // Render after first layout
            Update();
        };

        _logger.LogInformation("Game window created");

        return _gameWindow;
    }

    /// <summary>
    /// Handles window key down events
    /// </summary>
    private void OnWindowKeyDown(object? sender, Terminal.Gui.KeyEventEventArgs e)
    {
        // Terminal.Gui v2 uses KeyEventEventArgs with a KeyCode property
        // Try to extract the key - need to determine the correct property name at runtime
        var keyCodeProperty = e.GetType().GetProperty("KeyCode");
        if (keyCodeProperty != null)
        {
            var keyCode = (Terminal.Gui.Key?)keyCodeProperty.GetValue(e);
            if (keyCode.HasValue && OnKeyDown(keyCode.Value))
            {
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Starts a new game
    /// </summary>
    public void StartNewGame()
    {
        _logger.LogInformation("Starting new game");

        _gameStateManager.InitializeNewGame(80, 40);
        _isRunning = true;

        _hudService.AddMessage("Welcome to the Dungeon!");
        _hudService.AddMessage("Use arrow keys or WASD to move.");
        _hudService.AddMessage("Press 'E' to switch to edit mode.");
        _hudService.AddMessage("Press 'Q' to quit.");

        // Don't call Update here - it will be called after layout
        // Update();
    }

    /// <summary>
    /// Updates the game state and renders
    /// </summary>
    public void Update()
    {
        if (!_isRunning)
            return;

        // Update game logic
        _gameStateManager.Update();

        // Render
        if (_gameStateManager.CurrentMap != null)
        {
            _worldViewService.Render(_gameStateManager.WorldManager.CurrentWorld, _gameStateManager.CurrentMap);
            _hudService.Update(_gameStateManager.WorldManager.CurrentWorld);
        }
    }

    /// <summary>
    /// Handles keyboard input
    /// </summary>
    private bool OnKeyDown(Key key)
    {
        if (!_isRunning || _gameStateManager.CurrentMap == null)
            return false;

        bool actionTaken = false;

        // Movement keys
        switch (key)
        {
            // Arrow keys
            case Key.CursorUp:
                actionTaken = _gameStateManager.HandlePlayerMove(0, -1);
                break;
            case Key.CursorDown:
                actionTaken = _gameStateManager.HandlePlayerMove(0, 1);
                break;
            case Key.CursorLeft:
                actionTaken = _gameStateManager.HandlePlayerMove(-1, 0);
                break;
            case Key.CursorRight:
                actionTaken = _gameStateManager.HandlePlayerMove(1, 0);
                break;

            // WASD keys
            case Key.W:
            case Key.w:
                actionTaken = _gameStateManager.HandlePlayerMove(0, -1);
                break;
            case Key.S:
            case Key.s:
                actionTaken = _gameStateManager.HandlePlayerMove(0, 1);
                break;
            case Key.A:
            case Key.a:
                actionTaken = _gameStateManager.HandlePlayerMove(-1, 0);
                break;
            case Key.D:
            case Key.d:
                actionTaken = _gameStateManager.HandlePlayerMove(1, 0);
                break;

            // Diagonal movement (numpad)
            case Key.Home: // 7
                actionTaken = _gameStateManager.HandlePlayerMove(-1, -1);
                break;
            case Key.PageUp: // 9
                actionTaken = _gameStateManager.HandlePlayerMove(1, -1);
                break;
            case Key.End: // 1
                actionTaken = _gameStateManager.HandlePlayerMove(-1, 1);
                break;
            case Key.PageDown: // 3
                actionTaken = _gameStateManager.HandlePlayerMove(1, 1);
                break;

            // Mode switching
            case Key.E:
            case Key.e:
                ToggleMode();
                return true;

            // Quit
            case Key.Q:
            case Key.q:
                if (MessageBox.Query("Quit", "Are you sure you want to quit?", "Yes", "No") == 0)
                {
                    Application.RequestStop();
                }
                return true;
        }

        if (actionTaken)
        {
            // Update the game after player action
            Update();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Toggles between play and edit mode
    /// </summary>
    private void ToggleMode()
    {
        var newMode = _gameStateManager.CurrentMode == GameMode.Play
            ? GameMode.Edit
            : GameMode.Play;

        _gameStateManager.SwitchMode(newMode);

        _hudService.AddMessage($"Switched to {newMode} mode");
        _logger.LogInformation("Toggled to {Mode} mode", newMode);

        Update();
    }

    /// <summary>
    /// Stops the game
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _logger.LogInformation("Game stopped");
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        _gameStateManager?.Dispose();

        _disposed = true;
    }
}
