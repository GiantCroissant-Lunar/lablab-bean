using LablabBean.Contracts.Analytics.Services;
using LablabBean.Contracts.Game.Events;
using LablabBean.Plugins.Analytics.Services;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Analytics;

/// <summary>
/// Analytics plugin that provides analytics service and tracks game events.
/// Demonstrates both service provision and event-driven architecture.
/// </summary>
public class AnalyticsPlugin : IPlugin
{
    private ILogger? _logger;
    private IEventBus? _eventBus;
    private AnalyticsService? _analyticsService;
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

        // Register analytics service
        _analyticsService = new AnalyticsService(context.Logger);
        context.Registry.Register<IService>(
            _analyticsService,
            new ServiceMetadata
            {
                Priority = 100,
                Name = "AnalyticsService",
                Version = "1.0.0"
            }
        );

        // Subscribe to game events
        _eventBus.Subscribe<EntitySpawnedEvent>(OnEntitySpawned);
        _eventBus.Subscribe<EntityMovedEvent>(OnEntityMoved);
        _eventBus.Subscribe<CombatEvent>(OnCombat);

        _logger.LogInformation("Analytics plugin initialized - service registered and subscribed to game events");
        return Task.CompletedTask;
    }

    private Task OnEntitySpawned(EntitySpawnedEvent evt)
    {
        _entitySpawnCount++;
        _logger?.LogInformation(
            "Entity spawned: {Type} at ({X}, {Y}). Total spawns: {Count}",
            evt.EntityType, evt.Position.X, evt.Position.Y, _entitySpawnCount);
        
        // Track in analytics service
        _analyticsService?.TrackEvent("entity_spawned", new
        {
            entity_type = evt.EntityType,
            position_x = evt.Position.X,
            position_y = evt.Position.Y
        });
        
        return Task.CompletedTask;
    }

    private Task OnEntityMoved(EntityMovedEvent evt)
    {
        _entityMoveCount++;
        _logger?.LogDebug(
            "Entity moved: {Id} from ({X1},{Y1}) to ({X2},{Y2}). Total moves: {Count}",
            evt.EntityId, evt.OldPosition.X, evt.OldPosition.Y,
            evt.NewPosition.X, evt.NewPosition.Y, _entityMoveCount);
        
        // Track in analytics service (sample every 10th move to reduce noise)
        if (_entityMoveCount % 10 == 0)
        {
            _analyticsService?.TrackEvent("entity_moved", new
            {
                entity_id = evt.EntityId,
                distance = Math.Abs(evt.NewPosition.X - evt.OldPosition.X) + Math.Abs(evt.NewPosition.Y - evt.OldPosition.Y)
            });
        }
        
        return Task.CompletedTask;
    }

    private Task OnCombat(CombatEvent evt)
    {
        _combatEventCount++;
        _logger?.LogInformation(
            "Combat: {Attacker} → {Target}, Damage: {Damage}, Hit: {Hit}, Kill: {Kill}. Total combats: {Count}",
            evt.AttackerId, evt.TargetId, evt.DamageDealt, evt.IsHit, evt.IsKill, _combatEventCount);
        
        // Track in analytics service
        _analyticsService?.TrackEvent("combat", new
        {
            attacker_id = evt.AttackerId,
            target_id = evt.TargetId,
            damage = evt.DamageDealt,
            is_hit = evt.IsHit,
            is_kill = evt.IsKill
        });
        
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
        
        // Flush analytics events
        _analyticsService?.FlushEvents();
        
        return Task.CompletedTask;
    }
}
