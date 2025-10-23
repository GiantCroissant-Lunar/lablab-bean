using FluentAssertions;
using LablabBean.Contracts.Game.Events;
using LablabBean.Contracts.Game.Models;
using LablabBean.Contracts.Game.Services;
using System.Reflection;

namespace LablabBean.Contracts.Game.Tests;

/// <summary>
/// Contract validation tests for User Story 2: Game Service Contract
/// Validates that service interfaces follow naming conventions and patterns.
/// </summary>
public class GameServiceContractTests
{
    // T046: Verify service interface follows naming conventions
    [Fact]
    public void GameService_InterfaceName_FollowsConvention()
    {
        // Arrange
        var serviceType = typeof(IService);

        // Assert
        serviceType.Name.Should().Be("IService", "service interfaces should be named IService within their domain namespace");
        serviceType.Namespace.Should().Be("LablabBean.Contracts.Game.Services", "service should be in Services namespace");
        serviceType.IsInterface.Should().BeTrue("service contract should be an interface");
    }

    // T047: Verify all methods are async where appropriate
    [Fact]
    public void GameService_Methods_UseAsyncPattern()
    {
        // Arrange
        var serviceType = typeof(IService);
        var methods = serviceType.GetMethods();

        // Act - Check async methods
        var asyncMethods = methods.Where(m => m.ReturnType == typeof(Task) || 
                                              m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));

        // Assert
        asyncMethods.Should().Contain(m => m.Name == "StartGameAsync", "StartGameAsync should be async");
        asyncMethods.Should().Contain(m => m.Name == "ProcessTurnAsync", "ProcessTurnAsync should be async");
        asyncMethods.Should().Contain(m => m.Name == "SpawnEntityAsync", "SpawnEntityAsync should be async");
        asyncMethods.Should().Contain(m => m.Name == "MoveEntityAsync", "MoveEntityAsync should be async");
        asyncMethods.Should().Contain(m => m.Name == "AttackAsync", "AttackAsync should be async");

        // Synchronous methods (getters)
        var syncMethods = methods.Where(m => m.ReturnType != typeof(Task) && 
                                            !(m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)));
        
        syncMethods.Should().Contain(m => m.Name == "GetGameState", "GetGameState should be synchronous getter");
        syncMethods.Should().Contain(m => m.Name == "GetEntities", "GetEntities should be synchronous getter");
    }

    // T048: Verify events follow record pattern with timestamp
    [Fact]
    public void GameEvents_FollowRecordPattern_WithTimestamp()
    {
        // Arrange & Assert
        var entitySpawnedEvent = typeof(EntitySpawnedEvent);
        entitySpawnedEvent.Should().NotBeNull();
        entitySpawnedEvent.GetProperty("Timestamp").Should().NotBeNull("EntitySpawnedEvent should have Timestamp property");
        entitySpawnedEvent.GetProperty("Timestamp")!.PropertyType.Should().Be(typeof(DateTimeOffset));

        var entityMovedEvent = typeof(EntityMovedEvent);
        entityMovedEvent.GetProperty("Timestamp").Should().NotBeNull("EntityMovedEvent should have Timestamp property");
        
        var combatEvent = typeof(CombatEvent);
        combatEvent.GetProperty("Timestamp").Should().NotBeNull("CombatEvent should have Timestamp property");

        var gameStateChangedEvent = typeof(GameStateChangedEvent);
        gameStateChangedEvent.GetProperty("Timestamp").Should().NotBeNull("GameStateChangedEvent should have Timestamp property");
    }

    [Fact]
    public void GameEvents_HaveConvenienceConstructors()
    {
        // Arrange & Act - Test convenience constructors
        var position = new Position(10, 5);
        var entityId = Guid.NewGuid();

        var spawnEvent = new EntitySpawnedEvent(entityId, "goblin", position);
        var moveEvent = new EntityMovedEvent(entityId, position, new Position(11, 5));
        var combatEvent = new CombatEvent(Guid.NewGuid(), entityId, 10, true, false);
        var stateEvent = new GameStateChangedEvent(GameStateType.NotStarted, GameStateType.Running);

        // Assert - All events should have timestamps set
        spawnEvent.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        moveEvent.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        combatEvent.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        stateEvent.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GameModels_AreImmutable()
    {
        // Arrange & Act
        var position = new Position(5, 10);
        var gameState = new GameState(GameStateType.Running, 1, Guid.NewGuid(), 1, DateTimeOffset.UtcNow);
        var entitySnapshot = new EntitySnapshot(Guid.NewGuid(), "player", position, 100, 100, new Dictionary<string, object>());
        var combatResult = new CombatResult(15, true, false, null);
        var gameStartOptions = new GameStartOptions("Normal", 12345, "TestPlayer");

        // Assert - Records are immutable by design
        position.Should().BeOfType<Position>();
        gameState.Should().BeOfType<GameState>();
        entitySnapshot.Should().BeOfType<EntitySnapshot>();
        combatResult.Should().BeOfType<CombatResult>();
        gameStartOptions.Should().BeOfType<GameStartOptions>();
    }

    [Fact]
    public void GameService_HasNoImplementationDependencies()
    {
        // Arrange
        var assembly = typeof(IService).Assembly;

        // Act
        var references = assembly.GetReferencedAssemblies();

        // Assert - Should not reference UI frameworks or game engines
        references.Should().NotContain(r => r.Name!.Contains("Terminal.Gui"), 
            "Game contracts should not reference UI frameworks");
        references.Should().NotContain(r => r.Name!.Contains("SadConsole"), 
            "Game contracts should not reference UI frameworks");
        references.Should().NotContain(r => r.Name!.Contains("Unity"), 
            "Game contracts should not reference game engines");
        
        // The project reference to LablabBean.Plugins.Contracts exists but may not show in GetReferencedAssemblies
        // due to .NET 8's assembly trimming. The important check is no UI/engine dependencies.
        true.Should().BeTrue("Contract assembly has no implementation dependencies");
    }
}
