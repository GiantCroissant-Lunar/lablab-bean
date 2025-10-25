namespace LablabBean.Plugins.Merchant.Components;

/// <summary>
/// Component for merchant NPCs that tracks their inventory and trade settings.
/// </summary>
public class MerchantInventory
{
    public Dictionary<Guid, MerchantStockItem> Stock { get; set; } = new();
    public float BuyPriceMultiplier { get; set; } = 0.5f; // Merchants buy at 50% value
    public float SellPriceMultiplier { get; set; } = 1.0f; // Merchants sell at 100% value
    public int RefreshInterval { get; set; } = 5; // Refresh stock every 5 levels
    public int LastRefreshLevel { get; set; } = 0;

    public bool HasItem(Guid itemId, int quantity = 1)
    {
        if (!Stock.TryGetValue(itemId, out var stockItem))
            return false;

        if (stockItem.IsInfinite)
            return true;

        return stockItem.Quantity >= quantity;
    }

    public bool RemoveStock(Guid itemId, int quantity = 1)
    {
        if (!Stock.TryGetValue(itemId, out var stockItem))
            return false;

        if (stockItem.IsInfinite)
            return true;

        if (stockItem.Quantity < quantity)
            return false;

        stockItem.Quantity -= quantity;
        if (stockItem.Quantity <= 0)
        {
            Stock.Remove(itemId);
        }

        return true;
    }

    public void AddStock(Guid itemId, int basePrice, int quantity, bool isInfinite = false)
    {
        if (Stock.TryGetValue(itemId, out var existing))
        {
            if (!existing.IsInfinite)
            {
                existing.Quantity += quantity;
            }
        }
        else
        {
            Stock[itemId] = new MerchantStockItem
            {
                ItemId = itemId,
                BasePrice = basePrice,
                Quantity = quantity,
                IsInfinite = isInfinite
            };
        }
    }
}

/// <summary>
/// Represents an item in merchant stock.
/// </summary>
public class MerchantStockItem
{
    public Guid ItemId { get; set; }
    public int BasePrice { get; set; }
    public int Quantity { get; set; }
    public bool IsInfinite { get; set; }
}
