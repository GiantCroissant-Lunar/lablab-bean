using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Spell.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Spell.Systems;

/// <summary>
/// System for regenerating mana over time.
/// </summary>
public class ManaRegenerationSystem
{
    private readonly World _world;
    private readonly ILogger _logger;
    private readonly QueryDescription _manaQuery;

    public ManaRegenerationSystem(World world, ILogger logger)
    {
        _world = world;
        _logger = logger;
        _manaQuery = new QueryDescription().WithAll<Mana>();
    }

    public void Update(float deltaTime)
    {
        _world.Query(in _manaQuery, (Entity entity, ref Mana mana) =>
        {
            if (mana.Current < mana.Max)
            {
                mana.Restore(mana.Regen);
            }
        });
    }
}
