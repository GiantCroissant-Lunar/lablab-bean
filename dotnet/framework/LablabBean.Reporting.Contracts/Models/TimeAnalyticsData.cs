namespace LablabBean.Reporting.Contracts.Models;

/// <summary>
/// Time-based analytics
/// </summary>
public class TimeAnalyticsData
{
    public System.TimeSpan TotalPlaytime { get; set; }
    public System.TimeSpan AverageTimePerLevel { get; set; }
    public System.TimeSpan AverageTimePerDungeon { get; set; }
    public System.DateTime SessionStartTime { get; set; }
    public System.DateTime? SessionEndTime { get; set; }
}
