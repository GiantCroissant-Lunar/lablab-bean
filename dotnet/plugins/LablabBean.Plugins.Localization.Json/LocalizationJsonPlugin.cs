using LablabBean.Contracts.Localization.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Localization.Json.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Localization.Json;

public class LocalizationJsonPlugin : IPlugin
{
    private LocalizationService? _localizationService;

    public string Id => "lablab-bean.localization-json";
    public string Name => "Localization JSON";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _localizationService = new LocalizationService(context.Logger);

        context.Registry.Register<IService>(
            _localizationService,
            new ServiceMetadata
            {
                Priority = 100,
                Name = "LocalizationService",
                Version = "1.0.0"
            }
        );

        context.Logger.LogInformation("Localization JSON service registered");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _localizationService?.Dispose();
        return Task.CompletedTask;
    }
}
