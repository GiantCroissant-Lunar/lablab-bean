using System.Text.Json;
using LablabBean.Reporting.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Analytics;

/// <summary>
/// Parses session analytics data from JSONL (JSON Lines) format.
/// Each line is a JSON event: {"timestamp": "...", "event": "...", "data": {...}}
/// </summary>
public class SessionJsonParser
{
    private readonly ILogger _logger;

    public SessionJsonParser(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<SessionStatisticsData> ParseSessionLogAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var data = new SessionStatisticsData();
        var events = new List<SessionEvent>();

        int totalKills = 0;
        int totalDeaths = 0;
        long totalDamageDealt = 0;
        long totalDamageTaken = 0;
        int itemsCollected = 0;
        int levelsCompleted = 0;
        int achievementsUnlocked = 0;
        var frameSamples = new List<int>();
        var loadTimes = new List<double>();

        DateTime? sessionStart = null;
        DateTime? sessionEnd = null;

        try
        {
            using var reader = new StreamReader(filePath);
            int lineNumber = 0;

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var eventData = JsonSerializer.Deserialize<AnalyticsEvent>(line);
                    if (eventData == null)
                        continue;

                    // Track session timing
                    if (eventData.EventType == "SessionStart")
                    {
                        sessionStart = eventData.Timestamp;
                        data.SessionId = eventData.Data.GetValueOrDefault("sessionId", "unknown");
                    }
                    else if (eventData.EventType == "SessionEnd")
                    {
                        sessionEnd = eventData.Timestamp;
                    }

                    // Combat events
                    else if (eventData.EventType == "Kill" || eventData.EventType == "EnemyKilled")
                    {
                        totalKills++;
                        events.Add(new SessionEvent
                        {
                            Timestamp = eventData.Timestamp,
                            EventType = "Kill",
                            Description = eventData.Data.GetValueOrDefault("enemy", "Enemy") + " killed",
                            Metadata = new Dictionary<string, object>(eventData.Data.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value)))
                        });
                    }
                    else if (eventData.EventType == "Death" || eventData.EventType == "PlayerDeath")
                    {
                        totalDeaths++;
                        events.Add(new SessionEvent
                        {
                            Timestamp = eventData.Timestamp,
                            EventType = "Death",
                            Description = "Player died",
                            Metadata = new Dictionary<string, object>(eventData.Data.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value)))
                        });
                    }
                    else if (eventData.EventType == "DamageDealt")
                    {
                        if (eventData.Data.TryGetValue("amount", out var dmgStr) && int.TryParse(dmgStr, out var dmg))
                            totalDamageDealt += dmg;
                    }
                    else if (eventData.EventType == "DamageTaken")
                    {
                        if (eventData.Data.TryGetValue("amount", out var dmgStr) && int.TryParse(dmgStr, out var dmg))
                            totalDamageTaken += dmg;
                    }

                    // Progression events
                    else if (eventData.EventType == "ItemCollected")
                    {
                        itemsCollected++;
                    }
                    else if (eventData.EventType == "LevelComplete")
                    {
                        levelsCompleted++;
                        events.Add(new SessionEvent
                        {
                            Timestamp = eventData.Timestamp,
                            EventType = "LevelComplete",
                            Description = $"Completed {eventData.Data.GetValueOrDefault("level", "Level")}",
                            Metadata = new Dictionary<string, object>(eventData.Data.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value)))
                        });
                    }
                    else if (eventData.EventType == "AchievementUnlocked")
                    {
                        achievementsUnlocked++;
                        events.Add(new SessionEvent
                        {
                            Timestamp = eventData.Timestamp,
                            EventType = "AchievementUnlocked",
                            Description = eventData.Data.GetValueOrDefault("achievement", "Achievement unlocked"),
                            Metadata = new Dictionary<string, object>(eventData.Data.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value)))
                        });
                    }

                    // Performance metrics
                    else if (eventData.EventType == "FrameRate")
                    {
                        if (eventData.Data.TryGetValue("fps", out var fpsStr) && int.TryParse(fpsStr, out var fps))
                            frameSamples.Add(fps);
                    }
                    else if (eventData.EventType == "LoadTime")
                    {
                        if (eventData.Data.TryGetValue("duration", out var durationStr) && double.TryParse(durationStr, out var duration))
                            loadTimes.Add(duration);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse JSON at line {LineNumber}: {Line}", lineNumber, line);
                }
            }

            // Calculate final statistics
            data.SessionStartTime = sessionStart ?? DateTime.UtcNow.AddHours(-1);
            data.SessionEndTime = sessionEnd ?? DateTime.UtcNow;
            data.TotalPlaytime = data.SessionEndTime - data.SessionStartTime;

            data.TotalKills = totalKills;
            data.TotalDeaths = totalDeaths;
            data.KillDeathRatio = totalDeaths > 0 ? (decimal)totalKills / totalDeaths : totalKills;

            data.TotalDamageDealt = (int)totalDamageDealt;
            data.TotalDamageTaken = (int)totalDamageTaken;
            data.AverageDamagePerKill = totalKills > 0 ? (decimal)totalDamageDealt / totalKills : 0;

            data.ItemsCollected = itemsCollected;
            data.LevelsCompleted = levelsCompleted;
            data.AchievementsUnlocked = achievementsUnlocked;

            data.AverageFrameRate = frameSamples.Count > 0 ? (int)frameSamples.Average() : 60;
            data.TotalLoadTime = TimeSpan.FromSeconds(loadTimes.Sum());

            data.KeyEvents = events.OrderBy(e => e.Timestamp).ToList();

            _logger.LogInformation("Parsed {EventCount} events from {FilePath}", events.Count, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing session log {FilePath}", filePath);
            throw;
        }

        return data;
    }

    private class AnalyticsEvent
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public Dictionary<string, string> Data { get; set; } = new();
    }
}

internal static class DictionaryExtensions
{
    public static string GetValueOrDefault(this Dictionary<string, string> dict, string key, string defaultValue)
    {
        return dict.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
