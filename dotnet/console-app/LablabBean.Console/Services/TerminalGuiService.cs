using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terminal.Gui;

namespace LablabBean.Console.Services;

public class TerminalGuiService : ITerminalGuiService
{
    private readonly ILogger<TerminalGuiService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private bool _tuiInitFailed;

    public TerminalGuiService(
        ILogger<TerminalGuiService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void Initialize()
    {
        _logger.LogInformation("Initializing Terminal.Gui");
        try
        {
            // Attempt to disable Terminal.Gui configuration assembly scanning to avoid
            // ReflectionTypeLoadException from unrelated loaded plugin assemblies.
            try
            {
                // If property exists in current Terminal.Gui version
                // this will prevent scanning AppDomain assemblies.
                Terminal.Gui.ConfigurationManager.Enabled = false;
            }
            catch
            {
                // Best effort; continue if not available
            }
            Application.Init();
        }
        catch (ReflectionTypeLoadException)
        {
            _tuiInitFailed = true;
            _logger.LogWarning("Terminal.Gui initialization failed due to type loading issues. Falling back to non-interactive mode.");
        }
        catch (TypeLoadException)
        {
            _tuiInitFailed = true;
            _logger.LogWarning("Terminal.Gui initialization failed due to missing types. Falling back to non-interactive mode.");
        }
        catch (Exception)
        {
            _tuiInitFailed = true;
            _logger.LogWarning("Terminal.Gui initialization failed. Falling back to non-interactive mode.");
        }
    }

    public void Run()
    {
        _logger.LogInformation("Starting Terminal.Gui application");

        try
        {
            if (_tuiInitFailed)
            {
                _logger.LogWarning("TUI not available. Use CLI commands instead (e.g., 'plugins list', 'report plugin', 'kb ...').");
                return;
            }
            // Get the dungeon crawler service
            var dungeonCrawlerService = _serviceProvider.GetRequiredService<DungeonCrawlerService>();

            // Create the game window
            var gameWindow = dungeonCrawlerService.CreateGameWindow();

            // Start a new game
            dungeonCrawlerService.StartNewGame();

            // Run the application
            Application.Run(gameWindow);

            // Proper cleanup after Application.Run exits
            gameWindow.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Terminal.Gui application run");
            throw;
        }
        finally
        {
            _logger.LogInformation("Terminal.Gui application finished");
        }
    }

    public void Shutdown()
    {
        _logger.LogInformation("Shutting down Terminal.Gui");
        if (!_tuiInitFailed)
        {
            Application.Shutdown();
        }
        else
        {
            _logger.LogInformation("TUI was not initialized; nothing to shut down.");
        }
    }
}
