using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.ConfigManager;

/// <summary>
/// Example plugin that provides configuration management.
/// </summary>
public class ConfigManagerPlugin : IPlugin
{
    public string Id => "config-manager";
    public string Name => "Config Manager";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        context.Logger.LogInformation("Config Manager plugin initializing...");

        var eventBus = context.Registry.Get<IEventBus>();
        var configService = new InMemoryConfigService(eventBus, context.Logger);

        context.Registry.Register<LablabBean.Contracts.Config.Services.IService>(
            configService,
            new ServiceMetadata { Priority = 200, Name = "ConfigManager", Version = "1.0.0" }
        );

        context.Logger.LogInformation("Config Manager plugin initialized");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct = default) => Task.CompletedTask;
}
