namespace LablabBean.Plugins.Merchant.Data;

/// <summary>
/// Defines a merchant item with pricing and stock information.
/// </summary>
public class MerchantItem
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int BasePrice { get; set; }
    public int InitialStock { get; set; }
    public bool IsInfinite { get; set; }
    public int MinLevel { get; set; }
    public ItemRarity Rarity { get; set; }
}

/// <summary>
/// Defines a complete merchant with their inventory.
/// </summary>
public class MerchantDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MerchantType Type { get; set; }
    public float BuyPriceMultiplier { get; set; } = 0.5f;
    public float SellPriceMultiplier { get; set; } = 1.0f;
    public List<MerchantItem> Inventory { get; set; } = new();
}

public enum MerchantType
{
    GeneralStore,
    Blacksmith,
    Alchemist,
    MagicShop,
    Tavern
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// Result of a trade transaction.
/// </summary>
public record TradeResult(
    bool Success,
    string? FailureReason = null,
    int GoldSpent = 0,
    int GoldEarned = 0,
    List<Guid>? ItemsTraded = null
);
