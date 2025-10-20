using LablabBean.Game.Core.Services;
using LablabBean.Game.Core.Worlds;
using LablabBean.Game.SadConsole.Renderers;
using Microsoft.Extensions.Logging;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;

namespace LablabBean.Game.SadConsole.Screens;

/// <summary>
/// Main game screen for SadConsole
/// Combines world rendering and HUD
/// </summary>
public class GameScreen : ScreenObject
{
    private readonly ILogger<GameScreen> _logger;
    private readonly GameStateManager _gameStateManager;
    private readonly WorldRenderer _worldRenderer;
    private readonly HudRenderer _hudRenderer;
    private bool _isInitialized;

    public GameScreen(
        ILogger<GameScreen> logger,
        GameStateManager gameStateManager,
        int width,
        int height)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));

        // Create renderers
        int hudWidth = 30;
        _worldRenderer = new WorldRenderer(width - hudWidth, height);
        _hudRenderer = new HudRenderer(hudWidth, height);

        // Position world renderer
        _worldRenderer.Surface.Position = new Point(0, 0);

        // Position HUD renderer on the right
        _hudRenderer.Console.Position = new Point(width - hudWidth, 0);

        // Add to children
        Children.Add(_worldRenderer.Surface);
        Children.Add(_hudRenderer.Console);

        UseMouse = true;
        UseKeyboard = true;
    }

    /// <summary>
    /// Initializes a new game
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
            return;

        _logger.LogInformation("Initializing game screen");

        // Initialize the game
        _gameStateManager.InitializeNewGame(80, 40);

        _hudRenderer.AddMessage("Welcome to the Dungeon!");
        _hudRenderer.AddMessage("Use arrow keys or WASD to move.");
        _hudRenderer.AddMessage("Press 'E' to switch to edit mode.");
        _hudRenderer.AddMessage("Press 'ESC' to quit.");

        _isInitialized = true;

        // Initial render
        Render();
    }

    /// <summary>
    /// Renders the game
    /// </summary>
    private void Render()
    {
        if (!_isInitialized || _gameStateManager.CurrentMap == null)
            return;

        // Render world
        _worldRenderer.Render(_gameStateManager.WorldManager.CurrentWorld, _gameStateManager.CurrentMap);

        // Render HUD
        _hudRenderer.Update(_gameStateManager.WorldManager.CurrentWorld);
    }

    /// <summary>
    /// Updates the game state
    /// </summary>
    public override void Update(TimeSpan delta)
    {
        base.Update(delta);

        if (_isInitialized)
        {
            _gameStateManager.Update();
        }
    }

    /// <summary>
    /// Handles keyboard input
    /// </summary>
    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        if (!_isInitialized)
            return false;

        bool actionTaken = false;

        // Movement
        if (keyboard.IsKeyPressed(Keys.Up) || keyboard.IsKeyPressed(Keys.W))
        {
            actionTaken = _gameStateManager.HandlePlayerMove(0, -1);
        }
        else if (keyboard.IsKeyPressed(Keys.Down) || keyboard.IsKeyPressed(Keys.S))
        {
            actionTaken = _gameStateManager.HandlePlayerMove(0, 1);
        }
        else if (keyboard.IsKeyPressed(Keys.Left) || keyboard.IsKeyPressed(Keys.A))
        {
            actionTaken = _gameStateManager.HandlePlayerMove(-1, 0);
        }
        else if (keyboard.IsKeyPressed(Keys.Right) || keyboard.IsKeyPressed(Keys.D))
        {
            actionTaken = _gameStateManager.HandlePlayerMove(1, 0);
        }
        // Diagonal movement
        else if (keyboard.IsKeyPressed(Keys.Home))
        {
            actionTaken = _gameStateManager.HandlePlayerMove(-1, -1);
        }
        else if (keyboard.IsKeyPressed(Keys.PageUp))
        {
            actionTaken = _gameStateManager.HandlePlayerMove(1, -1);
        }
        else if (keyboard.IsKeyPressed(Keys.End))
        {
            actionTaken = _gameStateManager.HandlePlayerMove(-1, 1);
        }
        else if (keyboard.IsKeyPressed(Keys.PageDown))
        {
            actionTaken = _gameStateManager.HandlePlayerMove(1, 1);
        }
        // Mode switching
        else if (keyboard.IsKeyPressed(Keys.E))
        {
            ToggleMode();
            return true;
        }
        // Quit
        else if (keyboard.IsKeyPressed(Keys.Escape))
        {
            Game.Instance.MonoGameInstance.Exit();
            return true;
        }

        if (actionTaken)
        {
            Render();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Toggles between play and edit modes
    /// </summary>
    private void ToggleMode()
    {
        var newMode = _gameStateManager.CurrentMode == GameMode.Play
            ? GameMode.Edit
            : GameMode.Play;

        _gameStateManager.SwitchMode(newMode);
        _hudRenderer.AddMessage($"Switched to {newMode} mode");

        Render();
    }
}
