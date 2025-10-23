namespace LablabBean.Reporting.Contracts.Models;

/// <summary>
/// Detailed combat statistics
/// </summary>
public class CombatStatisticsData
{
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public double KDRatio { get; set; }
    public int DamageDealt { get; set; }
    public int DamageTaken { get; set; }
    public int HealingReceived { get; set; }
    public int CriticalHits { get; set; }
    public int PerfectDodges { get; set; }
    public double AverageDamagePerHit { get; set; }
    public double SurvivalRate { get; set; }
}
