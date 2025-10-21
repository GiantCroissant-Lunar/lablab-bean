using FluentAssertions;
using LablabBean.Contracts.Game.Models;
using LablabBean.Contracts.Game.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Core;
using LablabBean.Plugins.MockGame;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LablabBean.Plugins.Core.Tests;

/// <summary>
/// Integration tests for User Story 2: Game Service Contract
/// Tests service registration, retrieval, and priority-based selection.
/// </summary>
public class GameServiceIntegrationTests
{
    // T049: Register service and retrieve via IRegistry.Get<IService>()
    [Fact]
    public async Task GameService_RegisterAndRetrieve_ViaRegistry()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var registry = new ServiceRegistry();
        
        registry.Register<IEventBus>(eventBus, new ServiceMetadata { Priority = 1000, Name = "EventBus", Version = "1.0.0" });

        var mockGamePlugin = new MockGamePlugin();
        var mockContext = new MockPluginContext(registry);

        // Act - Initialize plugin (registers service)
        await mockGamePlugin.InitializeAsync(mockContext);

        // Retrieve service via IRegistry
        var gameService = registry.Get<IService>();

        // Assert
        gameService.Should().NotBeNull("service should be registered and retrievable");
        gameService.Should().BeOfType<MockGameService>("should retrieve the mock game service");
    }

    // T050: Multiple implementations with priority selection
    [Fact]
    public async Task GameService_MultipleImplementations_SelectsByPriority()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var registry = new ServiceRegistry();
        registry.Register<IEventBus>(eventBus, new ServiceMetadata { Priority = 1000, Name = "EventBus", Version = "1.0.0" });

        // Register first implementation with priority 100
        var service1 = new MockGameService(eventBus, NullLogger.Instance);
        registry.Register<IService>(service1, new ServiceMetadata 
        { 
            Priority = 100, 
            Name = "LowPriorityService", 
            Version = "1.0.0" 
        });

        // Register second implementation with priority 200 (higher)
        var service2 = new MockGameService(eventBus, NullLogger.Instance);
        registry.Register<IService>(service2, new ServiceMetadata 
        { 
            Priority = 200, 
            Name = "HighPriorityService", 
            Version = "1.0.0" 
        });

        // Act - Get service with highest priority
        var selectedService = registry.Get<IService>(SelectionMode.HighestPriority);

        // Assert
        selectedService.Should().BeSameAs(service2, "should select service with highest priority (200)");
        selectedService.Should().NotBeSameAs(service1, "should not select lower priority service");
    }

    [Fact]
    public async Task GameService_CanStartGame_AndPublishEvents()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var registry = new ServiceRegistry();
        registry.Register<IEventBus>(eventBus, new ServiceMetadata { Priority = 1000, Name = "EventBus", Version = "1.0.0" });

        var mockGamePlugin = new MockGamePlugin();
        var mockContext = new MockPluginContext(registry);
        await mockGamePlugin.InitializeAsync(mockContext);

        var gameService = registry.Get<IService>();
        var eventReceived = false;

        eventBus.Subscribe<LablabBean.Contracts.Game.Events.GameStateChangedEvent>(evt =>
        {
            eventReceived = true;
            return Task.CompletedTask;
        });

        // Act
        await gameService.StartGameAsync(new GameStartOptions("Normal", 12345, "TestPlayer"));

        // Assert
        gameService.GetGameState().State.Should().Be(GameStateType.Running);
        eventReceived.Should().BeTrue("GameStateChangedEvent should have been published");
    }

    [Fact]
    public async Task GameService_CanSpawnAndMoveEntities()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var registry = new ServiceRegistry();
        registry.Register<IEventBus>(eventBus, new ServiceMetadata { Priority = 1000, Name = "EventBus", Version = "1.0.0" });

        var gameService = new MockGameService(eventBus, NullLogger.Instance);
        registry.Register<IService>(gameService, new ServiceMetadata { Priority = 200, Name = "MockGame", Version = "1.0.0" });

        // Act - Spawn entity
        var entityId = await gameService.SpawnEntityAsync("player", new Position(5, 5));

        // Assert - Entity exists
        var entities = gameService.GetEntities();
        entities.Should().ContainSingle();
        entities.First().Id.Should().Be(entityId);
        entities.First().Position.Should().Be(new Position(5, 5));

        // Act - Move entity
        var moved = await gameService.MoveEntityAsync(entityId, new Position(6, 5));

        // Assert - Entity moved
        moved.Should().BeTrue();
        var movedEntity = gameService.GetEntities().First();
        movedEntity.Position.Should().Be(new Position(6, 5));
    }

    [Fact]
    public async Task GameService_CombatSystem_Works()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var gameService = new MockGameService(eventBus, NullLogger.Instance);

        var attackerId = await gameService.SpawnEntityAsync("player", new Position(5, 5));
        var targetId = await gameService.SpawnEntityAsync("goblin", new Position(6, 5));

        // Act
        var result = await gameService.AttackAsync(attackerId, targetId);

        // Assert
        result.Should().NotBeNull();
        if (result.IsHit)
        {
            result.DamageDealt.Should().BeGreaterThan(0);
        }
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
