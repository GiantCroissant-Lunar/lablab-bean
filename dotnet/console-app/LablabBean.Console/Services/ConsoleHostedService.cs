using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LablabBean.Console.Services;

public class ConsoleHostedService : IHostedService
{
    private readonly ILogger<ConsoleHostedService> _logger;
    private readonly ITerminalGuiService _terminalGuiService;
    private readonly IHostApplicationLifetime _lifetime;

    public ConsoleHostedService(
        ILogger<ConsoleHostedService> logger,
        ITerminalGuiService terminalGuiService,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _terminalGuiService = terminalGuiService;
        _lifetime = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Console application starting");

        _lifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(() =>
            {
                try
                {
                    _terminalGuiService.Initialize();
                    _terminalGuiService.Run();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running Terminal.Gui application");
                }
                finally
                {
                    _terminalGuiService.Shutdown();
                    _lifetime.StopApplication();
                }
            }, cancellationToken);
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Console application stopping");
        return Task.CompletedTask;
    }
}
