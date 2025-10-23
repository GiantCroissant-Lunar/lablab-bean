using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.ResourceLoader;

/// <summary>
/// Example plugin that provides async resource loading.
/// </summary>
public class ResourceLoaderPlugin : IPlugin
{
    public string Id => "resource-loader";
    public string Name => "Resource Loader";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        context.Logger.LogInformation("Resource Loader plugin initializing...");

        var eventBus = context.Registry.Get<IEventBus>();
        var resourceService = new InMemoryResourceService(eventBus, context.Logger);

        context.Registry.Register<LablabBean.Contracts.Resource.Services.IService>(
            resourceService,
            new ServiceMetadata { Priority = 200, Name = "ResourceLoader", Version = "1.0.0" }
        );

        context.Logger.LogInformation("Resource Loader plugin initialized");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct = default) => Task.CompletedTask;
}
