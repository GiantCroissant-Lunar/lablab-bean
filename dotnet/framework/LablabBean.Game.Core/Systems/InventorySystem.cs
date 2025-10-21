using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using Microsoft.Extensions.Logging;

namespace LablabBean.Game.Core.Systems;

/// <summary>
/// System that manages inventory operations: pickup, use, equip, drop
/// </summary>
public class InventorySystem
{
    private readonly ILogger<InventorySystem> _logger;

    public InventorySystem(ILogger<InventorySystem> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
