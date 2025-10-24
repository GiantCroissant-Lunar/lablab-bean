using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Contracts.UI.Models;
using LablabBean.Game.Core.Components;
using Microsoft.Extensions.Logging;
using SadRogue.Primitives;

namespace LablabBean.Game.Core.Systems;

/// <summary>
/// Renderer-agnostic activity log writer. Stores entries in a singleton ActivityLog entity.
/// </summary>
public class ActivityLogSystem
{
    private readonly ILogger<ActivityLogSystem> _logger;

    public ActivityLogSystem(ILogger<ActivityLogSystem> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Entity EnsureLogEntity(World world, int capacity = 200)
    {
        var query = new QueryDescription().WithAll<ActivityLog>();
        Entity? found = null;

        world.Query(in query, (Entity e, ref ActivityLog log) =>
        {
            if (found == null)
            {
                found = e;
            }
        });

        if (found.HasValue)
        {
            return found.Value;
        }

        var entity = world.Create(new ActivityLog(capacity));
        _logger.LogDebug("Created ActivityLog singleton entity {EntityId}", entity.Id);
        return entity;
    }

    public void Append(World world, string message, ActivitySeverity severity,
        int? originEntityId = null, Point? position = null, string[]? tags = null, char? icon = null, Color? iconColor = null,
        ActivityCategory category = ActivityCategory.System)
    {
        var entity = EnsureLogEntity(world);
        var log = world.Get<ActivityLog>(entity);
        log.Add(new ActivityEntry(message, severity, null, category, originEntityId, position, tags, icon, iconColor));
        world.Set(entity, log);
    }

    // Convenience helpers
    public void Info(World w, string msg) => Append(w, msg, ActivitySeverity.Info, category: ActivityCategory.System);
    public void Success(World w, string msg) => Append(w, msg, ActivitySeverity.Success, category: ActivityCategory.System);
    public void Warning(World w, string msg) => Append(w, msg, ActivitySeverity.Warning, category: ActivityCategory.System);
    public void Error(World w, string msg) => Append(w, msg, ActivitySeverity.Error, category: ActivityCategory.System);
    public void Combat(World w, string msg) => Append(w, msg, ActivitySeverity.Combat, category: ActivityCategory.Combat);
    public void Loot(World w, string msg) => Append(w, msg, ActivitySeverity.Loot, category: ActivityCategory.Items);

    // Utilities for common messages
    public void LogDamage(World world, Entity attacker, Entity target, int damage)
    {
        var attackerName = GetEntityName(attacker);
        var targetName = GetEntityName(target);
        var isPlayer = target.Has<Player>();
        var msg = isPlayer
            ? $"{attackerName} hits you for {damage}"
            : $"You hit {targetName} for {damage}";
        Append(world, msg, ActivitySeverity.Combat, category: ActivityCategory.Combat);
    }

    public void LogMiss(World world, Entity attacker, Entity target)
    {
        var attackerName = GetEntityName(attacker);
        var targetName = GetEntityName(target);
        var isPlayer = target.Has<Player>();
        var msg = isPlayer ? $"You dodge {attackerName}"
                           : $"You miss {targetName}";
        Append(world, msg, ActivitySeverity.Combat, category: ActivityCategory.Combat);
    }

    public void LogDeath(World world, Entity entity)
    {
        var name = GetEntityName(entity);
        var isPlayer = entity.Has<Player>();
        var msg = isPlayer ? "You have died" : $"{name} is defeated";
        Append(world, msg, ActivitySeverity.Combat, category: ActivityCategory.Combat);
    }

    public void LogHeal(World world, Entity entity, int amount)
    {
        var isPlayer = entity.Has<Player>();
        var name = GetEntityName(entity);
        var msg = isPlayer ? $"You recover {amount} HP" : $"{name} recovers {amount} HP";
        Append(world, msg, ActivitySeverity.Success, category: ActivityCategory.Combat);
    }

    public void LogPickup(World world, Entity itemEntity)
    {
        if (!itemEntity.Has<Item>()) return;
        var item = itemEntity.Get<Item>();
        var count = itemEntity.Has<Stackable>() ? itemEntity.Get<Stackable>().Count : 1;
        var suffix = count > 1 ? $" ({count})" : string.Empty;
        Append(world, $"Picked up {item.Name}{suffix}", ActivitySeverity.Loot, category: ActivityCategory.Items);
    }

    public void LogDepth(World world, int depth)
    {
        Append(world, $"Descended to level {depth}", ActivitySeverity.Info, category: ActivityCategory.Level);
    }

    private string GetEntityName(Entity entity)
    {
        if (entity.Has<Name>())
        {
            return entity.Get<Name>().Value;
        }
        if (entity.Has<Player>()) return "You";
        return $"Entity#{entity.Id}";
    }
}
