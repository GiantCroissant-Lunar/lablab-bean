using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Spells.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Spells.Systems;

/// <summary>
/// System for managing spell cooldowns.
/// </summary>
public class SpellCooldownSystem
{
    private readonly World _world;
    private readonly ILogger _logger;
    private readonly QueryDescription _cooldownQuery;

    public SpellCooldownSystem(World world, ILogger logger)
    {
        _world = world;
        _logger = logger;
        _cooldownQuery = new QueryDescription().WithAll<SpellCooldown>();
    }

    public void UpdateCooldowns()
    {
        _world.Query(in _cooldownQuery, (ref SpellCooldown cooldown) =>
        {
            cooldown.DecrementAll();
        });
    }

    public bool IsOnCooldown(Entity entity, Guid spellId)
    {
        if (!entity.Has<SpellCooldown>()) return false;

        var cooldown = entity.Get<SpellCooldown>();
        return cooldown.IsOnCooldown(spellId);
    }

    public int GetRemainingCooldown(Entity entity, Guid spellId)
    {
        if (!entity.Has<SpellCooldown>()) return 0;

        var cooldown = entity.Get<SpellCooldown>();
        return cooldown.GetRemainingCooldown(spellId);
    }

    public void StartCooldown(Entity entity, Guid spellId, int turns)
    {
        if (!entity.Has<SpellCooldown>())
        {
            entity.Add(new SpellCooldown());
        }

        ref var cooldown = ref entity.Get<SpellCooldown>();
        cooldown.StartCooldown(spellId, turns);

        _logger.LogDebug("Started {Turns} turn cooldown for spell {SpellId} on entity {Entity}",
            turns, spellId, entity);
    }
}
