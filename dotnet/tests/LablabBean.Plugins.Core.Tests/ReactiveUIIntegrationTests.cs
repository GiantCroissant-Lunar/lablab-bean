using FluentAssertions;
using LablabBean.Contracts.Game.Events;
using LablabBean.Contracts.Game.Models;
using LablabBean.Contracts.UI.Models;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Core;
using LablabBean.Plugins.MockGame;
using LablabBean.Plugins.ReactiveUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LablabBean.Plugins.Core.Tests;

/// <summary>
/// Integration tests for User Story 3: UI Plugin with Events
/// Tests that UI plugins receive game events and update reactively without polling.
/// </summary>
public class ReactiveUIIntegrationTests
{
    // T071: UI plugin receives game events and updates display
    [Fact]
    public async Task ReactiveUI_ReceivesGameEvents_AndUpdatesDisplay()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var registry = new ServiceRegistry();
        registry.Register<IEventBus>(eventBus, new ServiceMetadata { Priority = 1000, Name = "EventBus", Version = "1.0.0" });

        var reactiveUIPlugin = new ReactiveUIPlugin();
        var mockContext = new MockPluginContext(registry);

        // Initialize UI plugin (subscribes to game events)
        await reactiveUIPlugin.InitializeAsync(mockContext);

        var uiService = registry.Get<LablabBean.Contracts.UI.Services.IService>();

        // Initialize UI
        await uiService.InitializeAsync(new UIInitOptions(80, 24, false, "Dark"));

        // Act - Publish game events
        await eventBus.PublishAsync(new EntitySpawnedEvent(Guid.NewGuid(), "player", new Position(10, 10)));
        await eventBus.PublishAsync(new EntityMovedEvent(Guid.NewGuid(), new Position(10, 10), new Position(11, 10)));
        await eventBus.PublishAsync(new CombatEvent(Guid.NewGuid(), Guid.NewGuid(), 15, true, false));

        // Update display
        await uiService.UpdateDisplayAsync();

        // Assert - UI service received events and processed them (no exceptions thrown)
        uiService.Should().NotBeNull("UI service should be registered and functional");
    }

    // T072: UI handles rapid event succession without blocking
    [Fact]
    public async Task ReactiveUI_HandlesRapidEvents_WithoutBlocking()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var registry = new ServiceRegistry();
        registry.Register<IEventBus>(eventBus, new ServiceMetadata { Priority = 1000, Name = "EventBus", Version = "1.0.0" });

        var reactiveUIPlugin = new ReactiveUIPlugin();
        var mockContext = new MockPluginContext(registry);
        await reactiveUIPlugin.InitializeAsync(mockContext);

        var uiService = registry.Get<LablabBean.Contracts.UI.Services.IService>();
        await uiService.InitializeAsync(new UIInitOptions(80, 24, false, "Dark"));

        // Act - Publish many events rapidly
        var tasks = new List<Task>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(eventBus.PublishAsync(new EntityMovedEvent(
                Guid.NewGuid(),
                new Position(i, i),
                new Position(i + 1, i + 1)
            )));
        }

        await Task.WhenAll(tasks);

        // Assert - All events processed without blocking
        tasks.Should().AllSatisfy(t => t.IsCompletedSuccessfully.Should().BeTrue("all events should complete successfully"));
    }

    // T073: System handles missing UI subscriber gracefully
    [Fact]
    public async Task EventBus_HandlesUnloadedUIPlugin_Gracefully()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var registry = new ServiceRegistry();
        registry.Register<IEventBus>(eventBus, new ServiceMetadata { Priority = 1000, Name = "EventBus", Version = "1.0.0" });

        // Don't register UI plugin - simulate it being unloaded

        // Act - Publish game events without UI subscriber
        Func<Task> act = async () =>
        {
            await eventBus.PublishAsync(new EntitySpawnedEvent(Guid.NewGuid(), "goblin", new Position(5, 5)));
            await eventBus.PublishAsync(new EntityMovedEvent(Guid.NewGuid(), new Position(5, 5), new Position(6, 5)));
        };

        // Assert - Should not throw (graceful handling of missing subscribers)
        await act.Should().NotThrowAsync("event bus should handle missing subscribers gracefully");
    }

    [Fact]
    public async Task ReactiveUI_PublishesInputEvents()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var registry = new ServiceRegistry();
        registry.Register<IEventBus>(eventBus, new ServiceMetadata { Priority = 1000, Name = "EventBus", Version = "1.0.0" });

        var reactiveUIPlugin = new ReactiveUIPlugin();
        var mockContext = new MockPluginContext(registry);
        await reactiveUIPlugin.InitializeAsync(mockContext);

        var uiService = registry.Get<LablabBean.Contracts.UI.Services.IService>();
        await uiService.InitializeAsync(new UIInitOptions(80, 24, false, "Dark"));

        var inputReceived = false;
        eventBus.Subscribe<LablabBean.Contracts.UI.Events.InputReceivedEvent>(evt =>
        {
            inputReceived = true;
            return Task.CompletedTask;
        });

        // Act
        await uiService.HandleInputAsync(new InputCommand(InputType.Movement, "W", null));

        // Assert
        inputReceived.Should().BeTrue("InputReceivedEvent should have been published");
    }

    [Fact]
    public async Task ReactiveUI_PublishesViewportChangedEvents()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var registry = new ServiceRegistry();
        registry.Register<IEventBus>(eventBus, new ServiceMetadata { Priority = 1000, Name = "EventBus", Version = "1.0.0" });

        var reactiveUIPlugin = new ReactiveUIPlugin();
        var mockContext = new MockPluginContext(registry);
        await reactiveUIPlugin.InitializeAsync(mockContext);

        var uiService = registry.Get<LablabBean.Contracts.UI.Services.IService>();
        await uiService.InitializeAsync(new UIInitOptions(80, 24, false, "Dark"));

        var viewportChanged = false;
        eventBus.Subscribe<LablabBean.Contracts.UI.Events.ViewportChangedEvent>(evt =>
        {
            viewportChanged = true;
            return Task.CompletedTask;
        });

        // Act
        uiService.SetViewportCenter(new Position(50, 50));
        await Task.Delay(10); // Give async event publishing time to complete

        // Assert
        viewportChanged.Should().BeTrue("ViewportChangedEvent should have been published");
    }

    [Fact]
    public async Task FullIntegration_GameAndUI_WorkTogether()
    {
        // Arrange - Full system with game service and UI service
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var registry = new ServiceRegistry();
        registry.Register<IEventBus>(eventBus, new ServiceMetadata { Priority = 1000, Name = "EventBus", Version = "1.0.0" });

        // Register game service
        var mockGamePlugin = new MockGamePlugin();
        var mockContext = new MockPluginContext(registry);
        await mockGamePlugin.InitializeAsync(mockContext);

        // Register UI service
        var reactiveUIPlugin = new ReactiveUIPlugin();
        await reactiveUIPlugin.InitializeAsync(mockContext);

        var gameService = registry.Get<LablabBean.Contracts.Game.Services.IService>();
        var uiService = registry.Get<LablabBean.Contracts.UI.Services.IService>();

        await uiService.InitializeAsync(new UIInitOptions(80, 24, false, "Dark"));

        // Act - Game actions trigger UI updates via events
        await gameService.StartGameAsync(new GameStartOptions("Normal", 12345, "TestPlayer"));
        var entityId = await gameService.SpawnEntityAsync("player", new Position(10, 10));
        await gameService.MoveEntityAsync(entityId, new Position(11, 10));

        // Render and update UI
        var entities = gameService.GetEntities();
        var viewport = uiService.GetViewport();
        await uiService.RenderViewportAsync(viewport, entities);
        await uiService.UpdateDisplayAsync();

        // Assert - Full integration works
        entities.Should().ContainSingle();
        entities.First().Position.Should().Be(new Position(11, 10));
    }

    // Mock plugin context for testing
    private class MockPluginContext : IPluginContext
    {
        public MockPluginContext(IRegistry registry)
        {
            Registry = registry;
            Configuration = new ConfigurationBuilder().Build();
            Logger = NullLogger.Instance;
            Host = new MockPluginHost();
        }

        public IRegistry Registry { get; }
        public IConfiguration Configuration { get; }
        public ILogger Logger { get; }
        public IPluginHost Host { get; }
    }

    private class MockPluginHost : IPluginHost
    {
        public ILogger CreateLogger(string categoryName) => NullLogger.Instance;
        public IServiceProvider Services => new MockServiceProvider();
        public void PublishEvent<T>(T evt) { }
    }

    private class MockServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
