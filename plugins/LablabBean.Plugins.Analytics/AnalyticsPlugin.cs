using LablabBean.Contracts.Game.Events;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Analytics;

/// <summary>
/// Analytics plugin that tracks game events without direct dependency on game plugin.
/// Demonstrates event-driven plugin architecture.
/// </summary>
public class AnalyticsPlugin : IPlugin
{
    private ILogger? _logger;
    private IEventBus? _eventBus;
    private int _entitySpawnCount;
    private int _entityMoveCount;
    private int _combatEventCount;

    public string Id => "lablab-bean.analytics";
    public string Name => "Analytics Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _eventBus = context.Registry.Get<IEventBus>();

        // Subscribe to game events
        _eventBus.Subscribe<EntitySpawnedEvent>(OnEntitySpawned);
        _eventBus.Subscribe<EntityMovedEvent>(OnEntityMoved);
        _eventBus.Subscribe<CombatEvent>(OnCombat);

        _logger.LogInformation("Analytics plugin initialized - subscribed to game events");
        return Task.CompletedTask;
    }

    private Task OnEntitySpawned(EntitySpawnedEvent evt)
    {
        _entitySpawnCount++;
        _logger?.LogInformation(
            "Entity spawned: {Type} at ({X}, {Y}). Total spawns: {Count}",
            evt.EntityType, evt.Position.X, evt.Position.Y, _entitySpawnCount);
        return Task.CompletedTask;
    }

    private Task OnEntityMoved(EntityMovedEvent evt)
    {
        _entityMoveCount++;
        _logger?.LogDebug(
            "Entity moved: {Id} from ({X1},{Y1}) to ({X2},{Y2}). Total moves: {Count}",
            evt.EntityId, evt.OldPosition.X, evt.OldPosition.Y,
            evt.NewPosition.X, evt.NewPosition.Y, _entityMoveCount);
        return Task.CompletedTask;
    }

    private Task OnCombat(CombatEvent evt)
    {
        _combatEventCount++;
        _logger?.LogInformation(
            "Combat: {Attacker} → {Target}, Damage: {Damage}, Hit: {Hit}, Kill: {Kill}. Total combats: {Count}",
            evt.AttackerId, evt.TargetId, evt.DamageDealt, evt.IsHit, evt.IsKill, _combatEventCount);
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Analytics plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation(
            "Analytics summary - Spawns: {Spawns}, Moves: {Moves}, Combats: {Combats}",
            _entitySpawnCount, _entityMoveCount, _combatEventCount);
        return Task.CompletedTask;
    }
}
