namespace LablabBean.Reporting.Contracts.Models;

/// <summary>
/// Enemy type classification
/// </summary>
public enum EnemyType
{
    Goblin,
    Orc,
    Skeleton,
    Zombie,
    Dragon,
    Boss,
    Other
}

/// <summary>
/// Tracks kills by enemy type
/// </summary>
public class EnemyTypeData
{
    public EnemyType Type { get; set; }
    public int Kills { get; set; }
    public double Percentage { get; set; }
}
