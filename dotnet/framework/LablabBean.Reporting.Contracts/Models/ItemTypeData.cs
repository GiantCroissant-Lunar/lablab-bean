namespace LablabBean.Reporting.Contracts.Models;

/// <summary>
/// Item type classification
/// </summary>
public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Treasure,
    Key,
    Other
}

/// <summary>
/// Tracks item collection by type
/// </summary>
public class ItemTypeData
{
    public ItemType Type { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}
