using System.Text.Json;
using System.Reflection;
using LablabBean.Reporting.Analytics;
using LablabBean.Reporting.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Analytics;

public class SessionMetricsCollector : IDisposable
{
    private readonly SessionStatisticsProvider _provider;
    private readonly ILogger<SessionMetricsCollector> _logger;
    private readonly AdvancedAnalyticsCollector _advancedAnalytics;
    private readonly AchievementSystem? _achievementSystem;
    private readonly LeaderboardSystem? _leaderboardSystem;
    private readonly string _sessionId;
    private readonly DateTime _sessionStart;
    private readonly string _version;

    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public int LevelsCompleted { get; set; }
    public int ItemsCollected { get; set; }
    public int DungeonsCompleted { get; set; }
    public int MaxDepth { get; set; }

    public double KDRatio => TotalDeaths > 0 ? (double)TotalKills / TotalDeaths : TotalKills;

    public AdvancedAnalyticsCollector AdvancedAnalytics => _advancedAnalytics;
    public AchievementSystem? AchievementSystem => _achievementSystem;

    public SessionMetricsCollector(
        SessionStatisticsProvider provider,
        ILogger<SessionMetricsCollector> logger,
        AdvancedAnalyticsCollector advancedAnalytics,
        AchievementSystem? achievementSystem = null,
        LeaderboardSystem? leaderboardSystem = null)
    {
        _provider = provider;
        _logger = logger;
        _advancedAnalytics = advancedAnalytics;
        _achievementSystem = achievementSystem;
        _leaderboardSystem = leaderboardSystem;
        _sessionId = Guid.NewGuid().ToString();
        _sessionStart = DateTime.UtcNow;
        _version = GetVersion();

        _logger.LogInformation("Session started: {SessionId} (v{Version})", _sessionId, _version);
    }

    private static string GetVersion()
    {
        var assembly = Assembly.GetEntryAssembly();
        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                   ?? assembly?.GetName().Version?.ToString()
                   ?? "0.1.0-dev";
        return version;
    }

    /// <summary>
    /// Check achievements based on current metrics
    /// </summary>
    public List<LablabBean.Reporting.Contracts.Models.AchievementDefinition> CheckAchievements()
    {
        if (_achievementSystem == null)
            return new List<LablabBean.Reporting.Contracts.Models.AchievementDefinition>();

        var combatStats = _advancedAnalytics.GetCombatStatistics(TotalKills, TotalDeaths);
        var timeAnalytics = _advancedAnalytics.GetTimeAnalytics();

        var metrics = new Dictionary<string, double>
        {
            ["TotalKills"] = TotalKills,
            ["TotalDeaths"] = TotalDeaths,
            ["KDRatio"] = KDRatio,
            ["ItemsCollected"] = ItemsCollected,
            ["LevelsCompleted"] = LevelsCompleted,
            ["MaxDepth"] = MaxDepth,
            ["DungeonsCompleted"] = DungeonsCompleted,
            ["DamageDealt"] = combatStats.DamageDealt,
            ["DamageTaken"] = combatStats.DamageTaken,
            ["HealingReceived"] = combatStats.HealingReceived,
            ["CriticalHits"] = combatStats.CriticalHits,
            ["PerfectDodges"] = combatStats.PerfectDodges,
            ["TotalPlaytimeMinutes"] = timeAnalytics.TotalPlaytime.TotalMinutes,
            ["AvgTimePerLevelSeconds"] = timeAnalytics.AverageTimePerLevel.TotalSeconds
        };

        return _achievementSystem.CheckAchievements(metrics);
    }

    public async Task ExportSessionReportAsync(string outputPath, ReportFormat format)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            var timeAnalytics = _advancedAnalytics.GetTimeAnalytics();
            var combatStats = _advancedAnalytics.GetCombatStatistics(TotalKills, TotalDeaths);
            var itemBreakdown = _advancedAnalytics.GetItemTypeBreakdown();
            var enemyDistribution = _advancedAnalytics.GetEnemyTypeDistribution();

            var sessionData = new
            {
                SessionId = _sessionId,
                Version = _version,
                StartTime = _sessionStart,
                EndTime = DateTime.UtcNow,
                DurationMinutes = (DateTime.UtcNow - _sessionStart).TotalMinutes,
                BasicStats = new
                {
                    TotalKills,
                    TotalDeaths,
                    LevelsCompleted,
                    ItemsCollected,
                    DungeonsCompleted,
                    MaxDepth,
                    KDRatio = TotalDeaths > 0 ? (double)TotalKills / TotalDeaths : TotalKills
                },
                TimeAnalytics = new
                {
                    TotalPlaytime = timeAnalytics.TotalPlaytime.ToString(@"hh\:mm\:ss"),
                    AverageTimePerLevel = timeAnalytics.AverageTimePerLevel.ToString(@"mm\:ss"),
                    AverageTimePerDungeon = timeAnalytics.AverageTimePerDungeon.ToString(@"hh\:mm\:ss")
                },
                CombatStatistics = new
                {
                    combatStats.DamageDealt,
                    combatStats.DamageTaken,
                    combatStats.HealingReceived,
                    combatStats.CriticalHits,
                    combatStats.PerfectDodges,
                    AverageDamagePerHit = $"{combatStats.AverageDamagePerHit:F1}",
                    SurvivalRate = $"{combatStats.SurvivalRate:F1}%"
                },
                ItemBreakdown = itemBreakdown.Select(i => new
                {
                    Type = i.Type.ToString(),
                    i.Count,
                    Percentage = $"{i.Percentage:F1}%"
                }),
                EnemyDistribution = enemyDistribution.Select(e => new
                {
                    Type = e.Type.ToString(),
                    e.Kills,
                    Percentage = $"{e.Percentage:F1}%"
                }),
                Achievements = _achievementSystem != null ? new
                {
                    TotalCount = _achievementSystem.AllAchievements.Count,
                    UnlockedCount = _achievementSystem.Unlocks.Count,
                    CompletionPercentage = $"{_achievementSystem.GetCompletionPercentage():F1}%",
                    TotalPoints = _achievementSystem.GetTotalPoints(),
                    Unlocked = _achievementSystem.Unlocks.Select(u => new
                    {
                        AchievementId = u.AchievementId,
                        Name = _achievementSystem.AllAchievements.FirstOrDefault(a => a.Id == u.AchievementId)?.Name,
                        UnlockTime = u.UnlockTime,
                        Points = _achievementSystem.AllAchievements.FirstOrDefault(a => a.Id == u.AchievementId)?.Points
                    })
                } : null
            };

            var json = JsonSerializer.Serialize(sessionData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(outputPath, json);

            _logger.LogInformation("Session report exported to {Path}", outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export session report");
            throw;
        }
    }

    public void Dispose()
    {
        var duration = DateTime.UtcNow - _sessionStart;
        _logger.LogInformation(
            "Session ended: {SessionId} | Duration: {Duration:mm\\:ss} | Kills: {Kills} | Deaths: {Deaths} | K/D: {KD:F2} | Items: {Items} | Levels: {Levels} | Max Depth: {Depth}",
            _sessionId,
            duration,
            TotalKills,
            TotalDeaths,
            TotalDeaths > 0 ? (double)TotalKills / TotalDeaths : TotalKills,
            ItemsCollected,
            LevelsCompleted,
            MaxDepth);

        // Submit session to leaderboard
        SubmitToLeaderboard();
    }

    /// <summary>
    /// Submit session to leaderboard and update player profile
    /// </summary>
    private void SubmitToLeaderboard()
    {
        if (_leaderboardSystem == null)
            return;

        try
        {
            var timeAnalytics = _advancedAnalytics.GetTimeAnalytics();
            var combatStats = _advancedAnalytics.GetCombatStatistics(TotalKills, TotalDeaths);

            // Create session statistics data
            var sessionData = new LablabBean.Reporting.Contracts.Models.SessionStatisticsData
            {
                SessionId = _sessionId,
                SessionStartTime = _sessionStart,
                SessionEndTime = DateTime.UtcNow,
                TotalPlaytime = timeAnalytics.TotalPlaytime,
                TotalKills = TotalKills,
                TotalDeaths = TotalDeaths,
                KillDeathRatio = (decimal)KDRatio,
                TotalDamageDealt = combatStats.DamageDealt,
                TotalDamageTaken = combatStats.DamageTaken,
                ItemsCollected = ItemsCollected,
                LevelsCompleted = LevelsCompleted,
                AchievementsUnlocked = _achievementSystem?.Unlocks.Count ?? 0
            };

            // Get newly unlocked achievements in this session
            var newAchievements = _achievementSystem != null
                ? _achievementSystem.AllAchievements.Where(a =>
                    _achievementSystem.Unlocks.Any(u => u.AchievementId == a.Id)).ToList()
                : new List<LablabBean.Reporting.Contracts.Models.AchievementDefinition>();

            // Submit to leaderboard
            var entries = _leaderboardSystem.SubmitSession(sessionData);
            if (entries.Any())
            {
                _logger.LogInformation("Submitted to {Count} leaderboards", entries.Count);
                foreach (var entry in entries.Take(3)) // Log top 3
                {
                    _logger.LogInformation("  - Rank #{Rank} in {Category}", entry.Rank, entry.Stats.GetValueOrDefault("Category", "Unknown"));
                }
            }

            // Update player profile
            _leaderboardSystem.UpdatePlayerProfile(sessionData, newAchievements);

            _logger.LogInformation("Player profile updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit session to leaderboard");
        }
    }
}
