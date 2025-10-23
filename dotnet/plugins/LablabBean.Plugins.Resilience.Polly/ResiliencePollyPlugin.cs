using LablabBean.Contracts.Resilience.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Resilience.Polly.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Resilience.Polly;

public class ResiliencePollyPlugin : IPlugin
{
    private ResilienceService? _resilienceService;

    public string Id => "lablab-bean.resilience-polly";
    public string Name => "Resilience Polly";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _resilienceService = new ResilienceService(context.Logger);

        context.Registry.Register<IService>(
            _resilienceService,
            new ServiceMetadata
            {
                Priority = 100,
                Name = "ResilienceService",
                Version = "1.0.0"
            }
        );

        context.Logger.LogInformation("Resilience Polly service registered");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _resilienceService?.Dispose();
        return Task.CompletedTask;
    }
}
