using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Game.Core.Systems;

/// <summary>
/// System that spawns items in the dungeon and handles loot drops
/// </summary>
public class ItemSpawnSystem
{
    private readonly ILogger<ItemSpawnSystem> _logger;

    public ItemSpawnSystem(ILogger<ItemSpawnSystem> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
