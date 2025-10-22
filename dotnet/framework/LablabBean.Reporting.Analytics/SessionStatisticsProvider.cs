using LablabBean.Reporting.Contracts.Attributes;
using LablabBean.Reporting.Contracts.Contracts;
using LablabBean.Reporting.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Analytics;

/// <summary>
/// Provides session statistics data from analytics event logs.
/// Parses JSONL (JSON Lines) format from AnalyticsPlugin.
/// </summary>
[ReportProvider("Session", "Analytics", priority: 0)]
public class SessionStatisticsProvider : IReportProvider
{
    private readonly ILogger<SessionStatisticsProvider> _logger;

    public SessionStatisticsProvider(ILogger<SessionStatisticsProvider> logger)
    {
        _logger = logger;
    }

    public async Task<object> GetReportDataAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Collecting session statistics from {DataPath}", request.DataPath ?? "default logs");

        var data = new SessionStatisticsData();

        try
        {
            if (!string.IsNullOrWhiteSpace(request.DataPath) && File.Exists(request.DataPath))
            {
                // Parse real JSONL session data
                var parser = new SessionJsonParser(_logger);
                var sessionData = await parser.ParseSessionLogAsync(request.DataPath, cancellationToken);
                
                data.SessionId = sessionData.SessionId;
                data.SessionStartTime = sessionData.SessionStartTime;
                data.SessionEndTime = sessionData.SessionEndTime;
                data.TotalPlaytime = sessionData.TotalPlaytime;
                data.TotalKills = sessionData.TotalKills;
                data.TotalDeaths = sessionData.TotalDeaths;
                data.KillDeathRatio = sessionData.KillDeathRatio;
                data.TotalDamageDealt = sessionData.TotalDamageDealt;
                data.TotalDamageTaken = sessionData.TotalDamageTaken;
                data.AverageDamagePerKill = sessionData.AverageDamagePerKill;
                data.ItemsCollected = sessionData.ItemsCollected;
                data.LevelsCompleted = sessionData.LevelsCompleted;
                data.AchievementsUnlocked = sessionData.AchievementsUnlocked;
                data.AverageFrameRate = sessionData.AverageFrameRate;
                data.TotalLoadTime = sessionData.TotalLoadTime;
                data.KeyEvents = sessionData.KeyEvents;
                
                _logger.LogInformation("Parsed session data: K/D={KD:F2}, Playtime={Playtime}", 
                    data.KillDeathRatio, data.TotalPlaytime);
            }
            else
            {
                // Generate sample data for demonstration
                _logger.LogWarning("No session data found at {DataPath}, generating sample data", request.DataPath);
                GenerateSampleData(data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting session statistics");
            GenerateSampleData(data);
        }

        data.ReportGeneratedAt = DateTime.UtcNow;
        return data;
    }

    public ReportMetadata GetMetadata()
    {
        return new ReportMetadata
        {
            Name = "Session",
            Description = "Game session statistics including playtime, combat, and progression",
            Category = "Analytics",
            SupportedFormats = new[] { ReportFormat.HTML, ReportFormat.CSV },
            DataSourcePattern = "*.jsonl"
        };
    }

    private void GenerateSampleData(SessionStatisticsData data)
    {
        data.SessionId = $"session-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        data.SessionStartTime = DateTime.UtcNow.AddHours(-2);
        data.SessionEndTime = DateTime.UtcNow;
        data.TotalPlaytime = TimeSpan.FromHours(2);
        
        // Combat stats
        data.TotalKills = 47;
        data.TotalDeaths = 12;
        data.KillDeathRatio = data.TotalDeaths > 0 ? (decimal)data.TotalKills / data.TotalDeaths : data.TotalKills;
        data.TotalDamageDealt = 12450;
        data.TotalDamageTaken = 3820;
        data.AverageDamagePerKill = data.TotalKills > 0 ? (decimal)data.TotalDamageDealt / data.TotalKills : 0;
        
        // Progression
        data.ItemsCollected = 28;
        data.LevelsCompleted = 5;
        data.AchievementsUnlocked = 3;
        
        // Performance
        data.AverageFrameRate = 58;
        data.TotalLoadTime = TimeSpan.FromSeconds(12.5);
        
        // Key events
        data.KeyEvents = new List<SessionEvent>
        {
            new() { Timestamp = data.SessionStartTime, EventType = "SessionStart", Description = "Game session started" },
            new() { Timestamp = data.SessionStartTime.AddMinutes(15), EventType = "LevelComplete", Description = "Completed Level 1" },
            new() { Timestamp = data.SessionStartTime.AddMinutes(45), EventType = "AchievementUnlocked", Description = "First Blood" },
            new() { Timestamp = data.SessionStartTime.AddMinutes(90), EventType = "LevelComplete", Description = "Completed Level 2" },
            new() { Timestamp = data.SessionEndTime, EventType = "SessionEnd", Description = "Game session ended" }
        };
    }
}
