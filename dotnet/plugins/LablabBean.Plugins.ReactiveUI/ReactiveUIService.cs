using LablabBean.Contracts.Game.Events;
using LablabBean.Contracts.Game.Models;
using LablabBean.Contracts.UI.Events;
using LablabBean.Contracts.UI.Models;
using LablabBean.Contracts.UI.Services;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.ReactiveUI;

/// <summary>
/// Reactive UI service that automatically updates when game state changes via event subscriptions.
/// Demonstrates event-driven UI pattern without polling.
/// </summary>
public class ReactiveUIService : IService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private ViewportBounds _viewport;
    private bool _needsRedraw;
    private int _entityMoveCount;
    private int _entitySpawnCount;
    private int _combatCount;

    public ReactiveUIService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _viewport = new ViewportBounds(new Position(0, 0), Width: 80, Height: 24);
        _needsRedraw = false;
    }

    public Task InitializeAsync(UIInitOptions options, CancellationToken cancellationToken = default)
    {
        _viewport = new ViewportBounds(
            new Position(0, 0),
            options.ViewportWidth,
            options.ViewportHeight
        );

        // Subscribe to game events for reactive updates
        _eventBus.Subscribe<EntityMovedEvent>(OnEntityMoved);
        _eventBus.Subscribe<EntitySpawnedEvent>(OnEntitySpawned);
        _eventBus.Subscribe<CombatEvent>(OnCombat);
        _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

        _logger.LogInformation("Reactive UI initialized with viewport {W}x{H}. Subscribed to game events.",
            options.ViewportWidth, options.ViewportHeight);

        return Task.CompletedTask;
    }

    private Task OnEntityMoved(EntityMovedEvent evt)
    {
        _entityMoveCount++;
        _logger.LogDebug("Entity moved: {Id} from ({X1},{Y1}) to ({X2},{Y2}). Total moves: {Count}",
            evt.EntityId, evt.OldPosition.X, evt.OldPosition.Y,
            evt.NewPosition.X, evt.NewPosition.Y, _entityMoveCount);
        _needsRedraw = true;
        return Task.CompletedTask;
    }

    private Task OnEntitySpawned(EntitySpawnedEvent evt)
    {
        _entitySpawnCount++;
        _logger.LogInformation("Entity spawned: {Type} at ({X},{Y}). Total spawns: {Count}. UI marked for redraw.",
            evt.EntityType, evt.Position.X, evt.Position.Y, _entitySpawnCount);
        _needsRedraw = true;
        return Task.CompletedTask;
    }

    private Task OnCombat(CombatEvent evt)
    {
        _combatCount++;
        _logger.LogInformation("Combat animation: {Attacker} → {Target}, Damage: {Damage}. Total combats: {Count}",
            evt.AttackerId, evt.TargetId, evt.DamageDealt, _combatCount);
        _needsRedraw = true;
        return Task.CompletedTask;
    }

    private Task OnGameStateChanged(GameStateChangedEvent evt)
    {
        _logger.LogInformation("Game state changed: {Old} → {New}. Reason: {Reason}",
            evt.OldState, evt.NewState, evt.Reason ?? "N/A");
        _needsRedraw = true;
        return Task.CompletedTask;
    }

    public Task RenderViewportAsync(ViewportBounds viewport, IReadOnlyCollection<EntitySnapshot> entities)
    {
        // Render logic here (platform-specific implementation would go here)
        _logger.LogDebug("Rendering {Count} entities in viewport ({X},{Y}) {W}x{H}",
            entities.Count, viewport.TopLeft.X, viewport.TopLeft.Y, viewport.Width, viewport.Height);
        return Task.CompletedTask;
    }

    public Task UpdateDisplayAsync()
    {
        if (_needsRedraw)
        {
            _logger.LogDebug("Updating display (redraw needed)");
            _needsRedraw = false;
        }
        return Task.CompletedTask;
    }

    public async Task HandleInputAsync(InputCommand command)
    {
        _logger.LogDebug("Handling input: {Type} - {Key}", command.Type, command.Key);
        await _eventBus.PublishAsync(new InputReceivedEvent(command));
    }

    public ViewportBounds GetViewport() => _viewport;

    public async void SetViewportCenter(Position centerPosition)
    {
        var oldViewport = _viewport;
        var newTopLeft = new Position(
            centerPosition.X - _viewport.Width / 2,
            centerPosition.Y - _viewport.Height / 2
        );
        _viewport = _viewport with { TopLeft = newTopLeft };

        _logger.LogDebug("Viewport centered on ({X},{Y})", centerPosition.X, centerPosition.Y);
        await _eventBus.PublishAsync(new ViewportChangedEvent(oldViewport, _viewport));
        _needsRedraw = true;
    }
}
