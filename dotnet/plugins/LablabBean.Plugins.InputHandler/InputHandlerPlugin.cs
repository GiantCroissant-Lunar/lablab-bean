using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.InputHandler;

/// <summary>
/// Example plugin that provides input routing and mapping services.
/// </summary>
public class InputHandlerPlugin : IPlugin
{
    public string Id => "input-handler";
    public string Name => "Input Handler";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        context.Logger.LogInformation("Input Handler plugin initializing...");

        var eventBus = context.Registry.Get<IEventBus>();

        // Register input router service
        var routerService = new InputRouterService<LablabBean.Contracts.Input.InputEvent>(
            eventBus,
            context.Logger
        );
        context.Registry.Register<LablabBean.Contracts.Input.Router.IService<LablabBean.Contracts.Input.InputEvent>>(
            routerService,
            new ServiceMetadata { Priority = 200, Name = "InputRouter", Version = "1.0.0" }
        );

        // Register input mapper service
        var mapperService = new InputMapperService(eventBus, context.Logger);
        context.Registry.Register<LablabBean.Contracts.Input.Mapper.IService>(
            mapperService,
            new ServiceMetadata { Priority = 200, Name = "InputMapper", Version = "1.0.0" }
        );

        context.Logger.LogInformation("Input Handler plugin initialized");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct = default) => Task.CompletedTask;
}
