using LablabBean.Contracts.UI.Services;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.ReactiveUI;

/// <summary>
/// Plugin that provides a reactive UI service implementation.
/// Demonstrates event-driven UI updates without polling.
/// </summary>
public class ReactiveUIPlugin : IPlugin
{
    private ReactiveUIService? _uiService;

    public string Id => "lablab-bean.reactive-ui";
    public string Name => "Reactive UI Service";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        var eventBus = context.Registry.Get<IEventBus>();
        _uiService = new ReactiveUIService(eventBus, context.Logger);

        // Register with priority 100 (standard plugin priority)
        context.Registry.Register<IService>(
            _uiService,
            new ServiceMetadata
            {
                Priority = 100,
                Name = "ReactiveUIService",
                Version = "1.0.0"
            }
        );

        context.Logger.LogInformation("Reactive UI service registered with priority 100");
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
