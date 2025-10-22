using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.SceneLoader;

/// <summary>
/// Example plugin that provides a scene loader service.
/// </summary>
public class SceneLoaderPlugin : IPlugin
{
    public string Id => "scene-loader";
    public string Name => "Scene Loader";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        context.Logger.LogInformation("Scene Loader plugin initializing...");

        // Create and register the scene service
        var eventBus = context.Registry.Get<IEventBus>();
        var sceneService = new SceneLoaderService(eventBus, context.Logger);
        
        context.Registry.Register<LablabBean.Contracts.Scene.Services.IService>(
            sceneService,
            new ServiceMetadata 
            { 
                Priority = 200, 
                Name = "SceneLoader", 
                Version = "1.0.0" 
            }
        );

        context.Logger.LogInformation("Scene Loader plugin initialized");
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
