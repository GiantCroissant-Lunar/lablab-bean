using LablabBean.Contracts.Diagnostic.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Diagnostic.Console.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Diagnostic.Console;

public class DiagnosticConsolePlugin : IPlugin
{
    private DiagnosticService? _diagnosticService;

    public string Id => "lablab-bean.diagnostic-console";
    public string Name => "Diagnostic Console";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _diagnosticService = new DiagnosticService(context.Logger);

        context.Registry.Register<IService>(
            _diagnosticService,
            new ServiceMetadata
            {
                Priority = 100,
                Name = "DiagnosticService",
                Version = "1.0.0"
            }
        );

        context.Logger.LogInformation("Diagnostic Console service registered");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
