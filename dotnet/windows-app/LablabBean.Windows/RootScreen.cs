using Microsoft.Extensions.Logging;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SadConsoleConsole = SadConsole.Console;

namespace LablabBean.Windows;

public class RootScreen : ScreenObject
{
    private readonly ILogger<RootScreen>? _logger;
    private SadConsoleConsole _mainConsole;
    private SadConsoleConsole _statusBar;
    private SadConsoleConsole _menuBar;

    public RootScreen()
    {
        _logger = (ILogger<RootScreen>?)Game.Instance.Services?.GetService(typeof(ILogger<RootScreen>));
        _logger?.LogInformation("Initializing RootScreen");

        // Create menu bar
        _menuBar = new SadConsoleConsole(GameSettings.GAME_WIDTH, 1)
        {
            Position = new Point(0, 0)
        };
        _menuBar.Surface.DefaultBackground = Color.DarkBlue;
        _menuBar.Surface.Clear();
        _menuBar.Cursor.Position = new Point(1, 0);
        _menuBar.Cursor.Print(" File  Edit  View  Help ");
        Children.Add(_menuBar);

        // Create main console
        _mainConsole = new SadConsoleConsole(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT - 2)
        {
            Position = new Point(0, 1)
        };
        _mainConsole.Surface.DefaultBackground = Color.Black;
        _mainConsole.Surface.DefaultForeground = Color.White;
        _mainConsole.Surface.Clear();
        
        DrawWelcomeScreen();
        Children.Add(_mainConsole);

        // Create status bar
        _statusBar = new SadConsoleConsole(GameSettings.GAME_WIDTH, 1)
        {
            Position = new Point(0, GameSettings.GAME_HEIGHT - 1)
        };
        _statusBar.Surface.DefaultBackground = Color.DarkGray;
        _statusBar.Surface.DefaultForeground = Color.White;
        _statusBar.Surface.Clear();
        _statusBar.Cursor.Position = new Point(1, 0);
        _statusBar.Cursor.Print("Lablab Bean v0.1.0 | Press ESC to exit | F1 for help");
        Children.Add(_statusBar);

        IsFocused = true;
    }

    private void DrawWelcomeScreen()
    {
        var centerX = GameSettings.GAME_WIDTH / 2;
        var centerY = (GameSettings.GAME_HEIGHT - 2) / 2;

        // Draw title
        _mainConsole.Cursor.Position = new Point(centerX - 10, centerY - 5);
        _mainConsole.Cursor.Print("╔════════════════════╗", Color.Cyan);
        _mainConsole.Cursor.Position = new Point(centerX - 10, centerY - 4);
        _mainConsole.Cursor.Print("║   LABLAB BEAN     ║", Color.Cyan);
        _mainConsole.Cursor.Position = new Point(centerX - 10, centerY - 3);
        _mainConsole.Cursor.Print("╚════════════════════╝", Color.Cyan);

        // Draw info
        _mainConsole.Cursor.Position = new Point(centerX - 20, centerY);
        _mainConsole.Cursor.Print("Welcome to Lablab Bean!", Color.White);

        _mainConsole.Cursor.Position = new Point(centerX - 25, centerY + 2);
        _mainConsole.Cursor.Print("Built with SadConsole and .NET 8", Color.Gray);

        _mainConsole.Cursor.Position = new Point(centerX - 20, centerY + 4);
        _mainConsole.Cursor.Print("Features:", Color.Yellow);

        _mainConsole.Cursor.Position = new Point(centerX - 20, centerY + 5);
        _mainConsole.Cursor.Print("• Reactive programming with ReactiveUI", Color.White);

        _mainConsole.Cursor.Position = new Point(centerX - 20, centerY + 6);
        _mainConsole.Cursor.Print("• Dependency injection", Color.White);

        _mainConsole.Cursor.Position = new Point(centerX - 20, centerY + 7);
        _mainConsole.Cursor.Print("• Logging with Serilog", Color.White);

        _mainConsole.Cursor.Position = new Point(centerX - 20, centerY + 9);
        _mainConsole.Cursor.Print("Press any key to continue...", Color.Green);
    }

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            _logger?.LogInformation("ESC pressed, exiting application");
            Game.Instance.MonoGameInstance.Exit();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.F1))
        {
            ShowHelp();
            return true;
        }

        return base.ProcessKeyboard(keyboard);
    }

    private void ShowHelp()
    {
        _mainConsole.Surface.Clear();
        
        _mainConsole.Cursor.Position = new Point(2, 2);
        _mainConsole.Cursor.Print("=== HELP ===", Color.Cyan);

        _mainConsole.Cursor.Position = new Point(2, 4);
        _mainConsole.Cursor.Print("Keyboard Shortcuts:", Color.Yellow);

        _mainConsole.Cursor.Position = new Point(2, 6);
        _mainConsole.Cursor.Print("ESC    - Exit application", Color.White);

        _mainConsole.Cursor.Position = new Point(2, 7);
        _mainConsole.Cursor.Print("F1     - Show this help", Color.White);

        _mainConsole.Cursor.Position = new Point(2, 10);
        _mainConsole.Cursor.Print("Press any key to return...", Color.Green);
    }
}
