using LablabBean.Console.Views;
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

        // Use InteractiveWindow instead of MainWindow
        var menuService = _serviceProvider.GetRequiredService<IMenuService>();
        var interactiveWindow = new InteractiveWindow(menuService);
        Application.Run(interactiveWindow);
    }

    public void Shutdown()
    {
        _logger.LogInformation("Shutting down Terminal.Gui");
        Application.Shutdown();
    }
}
