using LablabBean.Contracts.Game.Services;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.MockGame;

/// <summary>
/// Plugin that provides a mock game service implementation.
/// Demonstrates service contract pattern with priority-based registration.
/// </summary>
public class MockGamePlugin : IPlugin
{
    private MockGameService? _gameService;

    public string Id => "lablab-bean.mock-game";
    public string Name => "Mock Game Service";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        var eventBus = context.Registry.Get<IEventBus>();
        _gameService = new MockGameService(eventBus, context.Logger);

        // Register with priority 200 (higher than default 100)
        context.Registry.Register<IService>(
            _gameService,
            new ServiceMetadata
            {
                Priority = 200,
                Name = "MockGameService",
                Version = "1.0.0"
            }
        );

        context.Logger.LogInformation("Mock game service registered with priority 200");
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
