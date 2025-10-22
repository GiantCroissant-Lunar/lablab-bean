using LablabBean.Contracts.Game.Events;
using LablabBean.Contracts.Game.Models;
using LablabBean.Contracts.Game.Services;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.MockGame;

/// <summary>
/// Mock implementation of game service for testing and demonstration.
/// </summary>
public class MockGameService : IService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly Dictionary<Guid, EntitySnapshot> _entities = new();
    private GameState _gameState;

    public MockGameService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameState = new GameState(
            GameStateType.NotStarted,
            TurnNumber: 0,
            PlayerEntityId: null,
            CurrentLevel: 1,
            StartTime: DateTimeOffset.UtcNow
        );
    }

    public async Task StartGameAsync(GameStartOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting game with difficulty: {Difficulty}, Seed: {Seed}, Player: {Player}",
            options.Difficulty, options.Seed, options.PlayerName ?? "Unknown");

        var oldState = _gameState.State;
        _gameState = _gameState with 
        { 
            State = GameStateType.Running,
            StartTime = DateTimeOffset.UtcNow
        };

        await _eventBus.PublishAsync(new GameStateChangedEvent(oldState, GameStateType.Running, "Game started"));
        _logger.LogInformation("Game started successfully");
    }

    public async Task<Guid> SpawnEntityAsync(string entityType, Position position)
    {
        // Check if position is occupied
        if (_entities.Values.Any(e => e.Position.X == position.X && e.Position.Y == position.Y))
        {
            throw new InvalidOperationException($"Position ({position.X}, {position.Y}) is already occupied");
        }

        var entityId = Guid.NewGuid();
        var entity = new EntitySnapshot(
            Id: entityId,
            Type: entityType,
            Position: position,
            Health: 100,
            MaxHealth: 100,
            Properties: new Dictionary<string, object>()
        );

        _entities[entityId] = entity;

        await _eventBus.PublishAsync(new EntitySpawnedEvent(entityId, entityType, position));

        _logger.LogInformation("Spawned {Type} at ({X}, {Y}) with ID {Id}",
            entityType, position.X, position.Y, entityId);

        return entityId;
    }

    public async Task<bool> MoveEntityAsync(Guid entityId, Position newPosition)
    {
        if (!_entities.TryGetValue(entityId, out var entity))
        {
            _logger.LogWarning("Cannot move entity {Id}: entity not found", entityId);
            return false;
        }

        // Check if target position is occupied
        if (_entities.Values.Any(e => e.Id != entityId && e.Position.X == newPosition.X && e.Position.Y == newPosition.Y))
        {
            _logger.LogDebug("Cannot move entity {Id}: position ({X}, {Y}) is occupied",
                entityId, newPosition.X, newPosition.Y);
            return false;
        }

        var oldPosition = entity.Position;
        _entities[entityId] = entity with { Position = newPosition };

        await _eventBus.PublishAsync(new EntityMovedEvent(entityId, oldPosition, newPosition));

        _logger.LogDebug("Moved entity {Id} from ({X1}, {Y1}) to ({X2}, {Y2})",
            entityId, oldPosition.X, oldPosition.Y, newPosition.X, newPosition.Y);

        return true;
    }

    public async Task<CombatResult> AttackAsync(Guid attackerId, Guid targetId)
    {
        if (!_entities.TryGetValue(attackerId, out var attacker))
            throw new InvalidOperationException($"Attacker {attackerId} not found");

        if (!_entities.TryGetValue(targetId, out var target))
            throw new InvalidOperationException($"Target {targetId} not found");

        if (target.Health <= 0)
            throw new InvalidOperationException($"Target {targetId} is already dead");

        // Simple combat logic
        var damage = Random.Shared.Next(5, 15);
        var isHit = Random.Shared.Next(100) < 80; // 80% hit chance
        var actualDamage = isHit ? damage : 0;

        var newHealth = Math.Max(0, target.Health - actualDamage);
        var isKill = newHealth == 0;

        _entities[targetId] = target with { Health = newHealth };

        await _eventBus.PublishAsync(new CombatEvent(attackerId, targetId, actualDamage, isHit, isKill));

        _logger.LogInformation("Combat: {Attacker} → {Target}, Damage: {Damage}, Hit: {Hit}, Kill: {Kill}",
            attackerId, targetId, actualDamage, isHit, isKill);

        return new CombatResult(actualDamage, isHit, isKill, SpecialEffect: null);
    }

    public Task ProcessTurnAsync(CancellationToken cancellationToken = default)
    {
        _gameState = _gameState with { TurnNumber = _gameState.TurnNumber + 1 };
        _logger.LogDebug("Processed turn {Turn}", _gameState.TurnNumber);
        return Task.CompletedTask;
    }

    public GameState GetGameState() => _gameState;

    public IReadOnlyCollection<EntitySnapshot> GetEntities() => _entities.Values.ToList();
}
