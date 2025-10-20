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

        // Use SimpleWindow for compatibility
        var simpleWindow = new SimpleWindow();
        Application.Run(simpleWindow);
    }

    public void Shutdown()
    {
        _logger.LogInformation("Shutting down Terminal.Gui");
        Application.Shutdown();
    }
}
