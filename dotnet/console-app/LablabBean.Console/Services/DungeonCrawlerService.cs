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
    private TextView? _debugLogView;
    private readonly List<string> _debugLogs = new();
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
        
        // Add debug log panel at the bottom
        var debugFrame = new FrameView("Debug Log (Press L to copy to clipboard)")
        {
            X = 0,
            Y = Pos.AnchorEnd(10),
            Width = Dim.Fill(30), // Leave space for HUD
            Height = 10,
            CanFocus = false
        };
        
        _debugLogView = new TextView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            CanFocus = false,
            WordWrap = true,
            ColorScheme = new ColorScheme()
            {
                Normal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
                Focus = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black)
            }
        };
        
        debugFrame.Add(_debugLogView);
        _gameWindow.Add(debugFrame);

        // Set up key bindings using Terminal.Gui v2 KeyDown event
        _gameWindow.KeyDown += OnWindowKeyDown;
        
        // Set up a handler to render after layout is complete
        _gameWindow.LayoutComplete += (s, e) =>
        {
            // Render after first layout
            Update();
            
            // Make sure game window has focus
            _gameWindow.SetFocus();
        };

        _logger.LogInformation("Game window created");

        return _gameWindow;
    }

    /// <summary>
    /// Handles window key down events
    /// </summary>
    private void OnWindowKeyDown(object? sender, Terminal.Gui.KeyEventEventArgs e)
    {
        AddDebugLog($"KeyDown event received");
        
        // Try different properties to find the key value
        Key? keyValue = null;
        
        // Try KeyEvent property (Terminal.Gui v2 pre-71)
        var keyEventProp = e.GetType().GetProperty("KeyEvent");
        if (keyEventProp != null)
        {
            AddDebugLog($"Found KeyEvent property");
            var keyEvent = keyEventProp.GetValue(e);
            if (keyEvent != null)
            {
                AddDebugLog($"KeyEvent type: {keyEvent.GetType().Name}");
                
                // Log all properties of KeyEvent to find the right one
                var props = keyEvent.GetType().GetProperties();
                AddDebugLog($"KeyEvent properties: {string.Join(", ", props.Select(p => p.Name))}");
                
                // KeyEvent might BE the Key directly if it's a Key type
                if (keyEvent is Key k)
                {
                    keyValue = k;
                    AddDebugLog($"KeyEvent IS a Key: {keyValue}");
                }
                else
                {
                    // Try to get KeyValue property (Terminal.Gui v2 pre-71 uses KeyValue, not Key!)
                    var keyValueProp = keyEvent.GetType().GetProperty("KeyValue");
                    if (keyValueProp != null)
                    {
                        var keyValueObj = keyValueProp.GetValue(keyEvent);
                        AddDebugLog($"KeyValue type: {keyValueObj?.GetType().Name ?? "null"}");
                        AddDebugLog($"KeyValue value: {keyValueObj}");
                        
                        // Try direct cast
                        if (keyValueObj is Key keyFromValue)
                        {
                            keyValue = keyFromValue;
                            AddDebugLog($"KeyValue is Key: {keyValue}");
                        }
                        // Try converting from int/uint
                        else if (keyValueObj != null)
                        {
                            try
                            {
                                // KeyValue might be an int representation
                                var intValue = Convert.ToInt32(keyValueObj);
                                keyValue = (Key)intValue;
                                AddDebugLog($"Converted KeyValue ({intValue}) to Key: {keyValue}");
                            }
                            catch (Exception ex)
                            {
                                AddDebugLog($"Failed to convert KeyValue: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        // Fallback to Key property
                        var keyProp = keyEvent.GetType().GetProperty("Key");
                        if (keyProp != null)
                        {
                            keyValue = keyProp.GetValue(keyEvent) as Key?;
                            AddDebugLog($"Extracted Key from property: {keyValue}");
                        }
                        else
                        {
                            AddDebugLog($"KeyEvent has no Key or KeyValue property");
                        }
                    }
                }
            }
        }
        
        // Try KeyCode property (older versions)
        if (!keyValue.HasValue)
        {
            var keyCodeProp = e.GetType().GetProperty("KeyCode");
            if (keyCodeProp != null)
            {
                keyValue = keyCodeProp.GetValue(e) as Key?;
                AddDebugLog($"Got key from KeyCode: {keyValue}");
            }
        }
        
        // Try Key property as last resort
        if (!keyValue.HasValue)
        {
            var keyProp = e.GetType().GetProperty("Key");
            if (keyProp != null)
            {
                keyValue = keyProp.GetValue(e) as Key?;
                AddDebugLog($"Got key from Key: {keyValue}");
            }
        }
        
        if (keyValue.HasValue && OnKeyDown(keyValue.Value))
        {
            AddDebugLog($"Key {keyValue} handled, action taken");
            e.Handled = true;
        }
        else if (!keyValue.HasValue)
        {
            AddDebugLog($"ERROR: Could not extract key!");
            _logger.LogWarning("Could not extract key from event: {EventType}", e.GetType().Name);
        }
        else
        {
            AddDebugLog($"Key {keyValue} received but no action");
        }
    }
    
    /// <summary>
    /// Adds a message to the debug log view
    /// </summary>
    private void AddDebugLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var logMessage = $"[{timestamp}] {message}";
        _debugLogs.Add(logMessage);
        
        // Keep only last 100 lines
        if (_debugLogs.Count > 100)
            _debugLogs.RemoveAt(0);
        
        // Update TextView with bright text
        if (_debugLogView != null)
        {
            _debugLogView.Text = string.Join("\n", _debugLogs);
            // Scroll to bottom
            _debugLogView.MoveEnd();
        }
        
        // Also add to HUD messages for visibility
        // _hudService.AddMessage(message); // Removed - using Debug Log instead
    }

    /// <summary>
    /// Starts a new game
    /// </summary>
    public void StartNewGame()
    {
        _logger.LogInformation("Starting new game");

        _gameStateManager.InitializeNewGame(80, 40);
        _isRunning = true;

        // _hudService.AddMessage("Welcome to the Dungeon!"); // Removed - using Debug Log instead
        // _hudService.AddMessage("Use arrow keys or WASD to move."); // Removed - using Debug Log instead
        // _hudService.AddMessage("Press 'E' to switch to edit mode."); // Removed - using Debug Log instead
        // _hudService.AddMessage("Press 'Q' to quit."); // Removed - using Debug Log instead

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
        
        // Ensure game window keeps focus
        _gameWindow?.SetFocus();
    }

    /// <summary>
    /// Processes NPC turns until it's the player's turn again
    /// </summary>
    private void ProcessNPCTurns()
    {
        if (!_isRunning || _gameStateManager.CurrentMap == null)
            return;

        var world = _gameStateManager.WorldManager.CurrentWorld;
        int maxIterations = 100; // Safety limit to prevent infinite loops
        int iterations = 0;

        // Keep processing until it's the player's turn or we hit the safety limit
        while (!_gameStateManager.IsPlayerTurn() && iterations < maxIterations)
        {
            AddDebugLog("Processing NPC turn...");
            _gameStateManager.Update();
            
            // Render after each NPC action
            if (_gameStateManager.CurrentMap != null)
            {
                _worldViewService.Render(_gameStateManager.WorldManager.CurrentWorld, _gameStateManager.CurrentMap);
                _hudService.Update(_gameStateManager.WorldManager.CurrentWorld);
            }
            
            iterations++;
            
            // Small delay to make NPC actions visible (optional, can be removed for instant processing)
            System.Threading.Thread.Sleep(50);
        }

        if (iterations >= maxIterations)
        {
            AddDebugLog("WARNING: Hit max iterations in ProcessNPCTurns");
            _logger.LogWarning("ProcessNPCTurns hit maximum iterations limit");
        }
        else if (iterations > 0)
        {
            AddDebugLog($"Processed {iterations} NPC turns");
        }
    }

    /// <summary>
    /// Handles keyboard input
    /// </summary>
    private bool OnKeyDown(Key key)
    {
        AddDebugLog($"OnKeyDown: {key}");
        
        if (!_isRunning || _gameStateManager.CurrentMap == null)
        {
            AddDebugLog($"Game not running or no map!");
            _logger.LogWarning("Cannot handle key - running: {Running}, map exists: {MapExists}", 
                _isRunning, _gameStateManager.CurrentMap != null);
            return false;
        }

        bool actionTaken = false;

        // Movement keys
        switch (key)
        {
            // Arrow keys
            case Key.CursorUp:
                AddDebugLog("Moving up");
                actionTaken = _gameStateManager.HandlePlayerMove(0, -1);
                break;
            case Key.CursorDown:
                AddDebugLog("Moving down");
                actionTaken = _gameStateManager.HandlePlayerMove(0, 1);
                break;
            case Key.CursorLeft:
                AddDebugLog("Moving left");
                actionTaken = _gameStateManager.HandlePlayerMove(-1, 0);
                break;
            case Key.CursorRight:
                AddDebugLog("Moving right");
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

            // Pickup item
            case Key.G:
            case Key.g:
                AddDebugLog("Attempting to pick up item(s)");
                var pickupMessages = _gameStateManager.HandlePlayerPickup();
                foreach (var message in pickupMessages)
                {
                    AddDebugLog(message);
                }
                // Action is taken if an item was actually picked up (energy consumed)
                if (pickupMessages.Any(m => m.StartsWith("Picked up")))
                {
                    actionTaken = true;
                }
                break;

            // Use/Consume item
            case Key.U:
            case Key.u:
                AddDebugLog("Opening item use menu");
                var useMessage = _gameStateManager.HandlePlayerUseItem();
                if (!string.IsNullOrEmpty(useMessage))
                {
                    AddDebugLog(useMessage);
                    // Check if the action was successful (not an error message)
                    if (!useMessage.StartsWith("Cannot") && !useMessage.StartsWith("Already") && !useMessage.StartsWith("No"))
                    {
                        actionTaken = true;
                    }
                }
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
            AddDebugLog("Action taken, updating game");
            // Update the game after player action
            Update();
            
            // Continue processing NPC turns until it's the player's turn again
            ProcessNPCTurns();
            
            return true;
        }

        AddDebugLog($"No action for key: {key}");
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

        // _hudService.AddMessage($"Switched to {newMode} mode"); // Removed - using Debug Log instead
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

