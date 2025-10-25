using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Plugins.Spells.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Spells.Systems;

/// <summary>
/// System for managing mana regeneration.
/// </summary>
public class ManaSystem
{
    private readonly World _world;
    private readonly ILogger _logger;
    private readonly QueryDescription _manaQuery;

    public ManaSystem(World world, ILogger logger)
    {
        _world = world;
        _logger = logger;
        _manaQuery = new QueryDescription().WithAll<Mana>();
    }

    public void RegenerateMana(bool inCombat)
    {
        _world.Query(in _manaQuery, (ref Mana mana) =>
        {
            if (mana.Current >= mana.Maximum) return;

            var regenAmount = inCombat ? mana.CombatRegenRate : mana.RegenRate;
            mana.Restore(regenAmount);
        });
    }

    public void RestoreMana(Entity entity, int amount)
    {
        if (!entity.Has<Mana>()) return;

        ref var mana = ref entity.Get<Mana>();
        mana.Restore(amount);

        _logger.LogDebug("Restored {Amount} mana to entity {Entity}", amount, entity);
    }

    public bool ConsumeMana(Entity entity, int amount)
    {
        if (!entity.Has<Mana>()) return false;

        ref var mana = ref entity.Get<Mana>();
        var consumed = mana.Consume(amount);

        if (consumed)
        {
            _logger.LogDebug("Consumed {Amount} mana from entity {Entity}", amount, entity);
        }

        return consumed;
    }

    public (int current, int maximum) GetMana(Entity entity)
    {
        if (!entity.Has<Mana>())
            return (0, 0);

        var mana = entity.Get<Mana>();
        return (mana.Current, mana.Maximum);
    }
}
