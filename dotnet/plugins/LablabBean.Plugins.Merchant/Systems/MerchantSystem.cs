using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Plugins.Merchant.Components;
using LablabBean.Plugins.Merchant.Data;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Merchant.Systems;

/// <summary>
/// System for managing merchant stock and inventory.
/// </summary>
public class MerchantSystem
{
    private readonly World _world;
    private readonly ILogger _logger;
    private readonly QueryDescription _merchantQuery;

    public MerchantSystem(World world, ILogger logger)
    {
        _world = world;
        _logger = logger;
        _merchantQuery = new QueryDescription().WithAll<NPC.Components.NPC, MerchantInventory>();
    }

    public void RefreshMerchantStock(Entity merchant, int currentLevel)
    {
        if (!merchant.Has<MerchantInventory>())
            return;

        ref var inventory = ref merchant.Get<MerchantInventory>();

        // Check if refresh is needed
        if (currentLevel - inventory.LastRefreshLevel < inventory.RefreshInterval)
            return;

        inventory.LastRefreshLevel = currentLevel;

        // Refresh quantities for non-infinite items
        var itemsToRefresh = new List<Guid>(inventory.Stock.Keys);
        foreach (var itemId in itemsToRefresh)
        {
            var stockItem = inventory.Stock[itemId];
            if (!stockItem.IsInfinite)
            {
                // Restore some stock (50-100% of base)
                var restorePercent = 0.5f + (Random.Shared.NextSingle() * 0.5f);
                var baseQuantity = 5; // Default base quantity
                stockItem.Quantity = Math.Max(stockItem.Quantity, (int)(baseQuantity * restorePercent));
            }
        }

        _logger.LogInformation("Refreshed stock for merchant on level {Level}", currentLevel);
    }

    public void RefreshAllMerchants(int currentLevel)
    {
        _world.Query(in _merchantQuery, (Entity entity) =>
        {
            RefreshMerchantStock(entity, currentLevel);
        });

        _logger.LogDebug("Refreshed all merchant stocks for level {Level}", currentLevel);
    }

    public int CalculateSellPrice(Entity merchant, int basePrice)
    {
        if (!merchant.Has<MerchantInventory>())
            return basePrice;

        var inventory = merchant.Get<MerchantInventory>();
        return (int)(basePrice * inventory.SellPriceMultiplier);
    }

    public int CalculateBuyPrice(Entity merchant, int basePrice)
    {
        if (!merchant.Has<MerchantInventory>())
            return (int)(basePrice * 0.5f);

        var inventory = merchant.Get<MerchantInventory>();
        return (int)(basePrice * inventory.BuyPriceMultiplier);
    }
}
