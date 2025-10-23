using LablabBean.Contracts.Input.ActionMap;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.InputActionMap;

/// <summary>
/// Plugin that provides Unity-inspired Action Map input management.
/// </summary>
public class InputActionMapPlugin : IPlugin
{
    public string Id => "input-action-map";
    public string Name => "Input Action Map";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        context.Logger.LogInformation("Input Action Map plugin initializing...");

        var eventBus = context.Registry.Get<IEventBus>();

        // Register action map service
        var actionMapService = new ActionMapService(eventBus, context.Logger);
        context.Registry.Register<IService>(
            actionMapService,
            new ServiceMetadata { Priority = 300, Name = "ActionMapService", Version = "1.0.0" }
        );

        context.Logger.LogInformation("Input Action Map plugin initialized");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct = default) => Task.CompletedTask;
}
