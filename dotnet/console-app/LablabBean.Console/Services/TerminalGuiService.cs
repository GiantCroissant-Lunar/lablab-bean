using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terminal.Gui;

namespace LablabBean.Console.Services;

public class TerminalGuiService : ITerminalGuiService
{
    private readonly ILogger<TerminalGuiService> _logger;
    private readonly IServiceProvider _serviceProvider;

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
        Application.Init();
    }

    public void Run()
    {
        _logger.LogInformation("Starting Terminal.Gui application");

        try
        {
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
        Application.Shutdown();
    }
}
