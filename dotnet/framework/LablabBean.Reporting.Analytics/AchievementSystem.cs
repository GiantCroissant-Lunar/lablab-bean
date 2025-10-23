using LablabBean.Reporting.Contracts.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LablabBean.Reporting.Analytics;

/// <summary>
/// Achievement system - tracks and unlocks achievements based on game metrics
/// </summary>
public class AchievementSystem
{
    private readonly ILogger<AchievementSystem> _logger;
    private readonly List<AchievementDefinition> _achievements;
    private readonly List<AchievementUnlock> _unlocks;
    private readonly string _sessionId;

    public event Action<AchievementDefinition>? OnAchievementUnlocked;

    public IReadOnlyList<AchievementDefinition> AllAchievements => _achievements.AsReadOnly();
    public IReadOnlyList<AchievementUnlock> Unlocks => _unlocks.AsReadOnly();

    public AchievementSystem(ILogger<AchievementSystem> logger, string sessionId)
    {
        _logger = logger;
        _sessionId = sessionId;
        _achievements = new List<AchievementDefinition>();
        _unlocks = new List<AchievementUnlock>();

        InitializeAchievements();
    }

    private void InitializeAchievements()
    {
        // Combat Achievements
        _achievements.Add(new AchievementDefinition
        {
            Id = "first_blood",
            Name = "First Blood",
            Description = "Kill your first enemy",
            Category = AchievementCategory.Combat,
            Rarity = AchievementRarity.Common,
            Points = 10,
            Icon = "⚔️",
            Condition = new AchievementCondition { MetricName = "TotalKills", TargetValue = 1, ComparisonOperator = ">=" }
        });

        _achievements.Add(new AchievementDefinition
        {
            Id = "slayer",
            Name = "Slayer",
            Description = "Kill 50 enemies",
            Category = AchievementCategory.Combat,
            Rarity = AchievementRarity.Uncommon,
            Points = 25,
            Icon = "⚔️",
            Condition = new AchievementCondition { MetricName = "TotalKills", TargetValue = 50, ComparisonOperator = ">=" }
        });

        _achievements.Add(new AchievementDefinition
        {
            Id = "exterminator",
            Name = "Exterminator",
            Description = "Kill 200 enemies",
            Category = AchievementCategory.Combat,
            Rarity = AchievementRarity.Rare,
            Points = 50,
            Icon = "⚔️",
            Condition = new AchievementCondition { MetricName = "TotalKills", TargetValue = 200, ComparisonOperator = ">=" }
        });

        _achievements.Add(new AchievementDefinition
        {
            Id = "dominator",
            Name = "Dominator",
            Description = "Achieve a K/D ratio of 10.0 or higher",
            Category = AchievementCategory.Combat,
            Rarity = AchievementRarity.Epic,
            Points = 100,
            Icon = "👑",
            Condition = new AchievementCondition { MetricName = "KDRatio", TargetValue = 10.0, ComparisonOperator = ">=" }
        });

        // Survival Achievements
        _achievements.Add(new AchievementDefinition
        {
            Id = "survivor",
            Name = "Survivor",
            Description = "Complete 5 levels without dying",
            Category = AchievementCategory.Survival,
            Rarity = AchievementRarity.Uncommon,
            Points = 30,
            Icon = "🛡️",
            Condition = new AchievementCondition { MetricName = "LevelsCompleted", TargetValue = 5, ComparisonOperator = ">=" }
        });

        _achievements.Add(new AchievementDefinition
        {
            Id = "immortal",
            Name = "Immortal",
            Description = "Complete a session with 0 deaths",
            Category = AchievementCategory.Survival,
            Rarity = AchievementRarity.Epic,
            Points = 75,
            Icon = "👼",
            Condition = new AchievementCondition { MetricName = "TotalDeaths", TargetValue = 0, ComparisonOperator = "==" }
        });

        _achievements.Add(new AchievementDefinition
        {
            Id = "tank",
            Name = "Tank",
            Description = "Take 500 damage in one session",
            Category = AchievementCategory.Survival,
            Rarity = AchievementRarity.Uncommon,
            Points = 25,
            Icon = "🛡️",
            Condition = new AchievementCondition { MetricName = "DamageTaken", TargetValue = 500, ComparisonOperator = ">=" }
        });

        // Collection Achievements
        _achievements.Add(new AchievementDefinition
        {
            Id = "treasure_hunter",
            Name = "Treasure Hunter",
            Description = "Collect 10 items",
            Category = AchievementCategory.Collection,
            Rarity = AchievementRarity.Common,
            Points = 10,
            Icon = "💎",
            Condition = new AchievementCondition { MetricName = "ItemsCollected", TargetValue = 10, ComparisonOperator = ">=" }
        });

        _achievements.Add(new AchievementDefinition
        {
            Id = "loot_goblin",
            Name = "Loot Goblin",
            Description = "Collect 50 items",
            Category = AchievementCategory.Collection,
            Rarity = AchievementRarity.Uncommon,
            Points = 30,
            Icon = "💰",
            Condition = new AchievementCondition { MetricName = "ItemsCollected", TargetValue = 50, ComparisonOperator = ">=" }
        });

        _achievements.Add(new AchievementDefinition
        {
            Id = "hoarder",
            Name = "Hoarder",
            Description = "Collect 100 items",
            Category = AchievementCategory.Collection,
            Rarity = AchievementRarity.Rare,
            Points = 50,
            Icon = "🏦",
            Condition = new AchievementCondition { MetricName = "ItemsCollected", TargetValue = 100, ComparisonOperator = ">=" }
        });

        // Exploration Achievements
        _achievements.Add(new AchievementDefinition
        {
            Id = "explorer",
            Name = "Explorer",
            Description = "Reach depth 10",
            Category = AchievementCategory.Exploration,
            Rarity = AchievementRarity.Common,
            Points = 15,
            Icon = "🗺️",
            Condition = new AchievementCondition { MetricName = "MaxDepth", TargetValue = 10, ComparisonOperator = ">=" }
        });

        _achievements.Add(new AchievementDefinition
        {
            Id = "deep_diver",
            Name = "Deep Diver",
            Description = "Reach depth 25",
            Category = AchievementCategory.Exploration,
            Rarity = AchievementRarity.Uncommon,
            Points = 35,
            Icon = "🏔️",
            Condition = new AchievementCondition { MetricName = "MaxDepth", TargetValue = 25, ComparisonOperator = ">=" }
        });

        _achievements.Add(new AchievementDefinition
        {
            Id = "dungeon_master",
            Name = "Dungeon Master",
            Description = "Complete 3 dungeons",
            Category = AchievementCategory.Exploration,
            Rarity = AchievementRarity.Rare,
            Points = 50,
            Icon = "🏰",
            Condition = new AchievementCondition { MetricName = "DungeonsCompleted", TargetValue = 3, ComparisonOperator = ">=" }
        });

        // Mastery Achievements
        _achievements.Add(new AchievementDefinition
        {
            Id = "glass_cannon",
            Name = "Glass Cannon",
            Description = "Deal 1000 damage with K/D ratio above 5.0",
            Category = AchievementCategory.Mastery,
            Rarity = AchievementRarity.Epic,
            Points = 100,
            Icon = "💥",
            Condition = new AchievementCondition { MetricName = "DamageDealt", TargetValue = 1000, ComparisonOperator = ">=" }
        });

        _achievements.Add(new AchievementDefinition
        {
            Id = "critical_master",
            Name = "Critical Master",
            Description = "Land 50 critical hits",
            Category = AchievementCategory.Mastery,
            Rarity = AchievementRarity.Rare,
            Points = 60,
            Icon = "💢",
            Condition = new AchievementCondition { MetricName = "CriticalHits", TargetValue = 50, ComparisonOperator = ">=" }
        });

        _achievements.Add(new AchievementDefinition
        {
            Id = "untouchable",
            Name = "Untouchable",
            Description = "Achieve 25 perfect dodges",
            Category = AchievementCategory.Mastery,
            Rarity = AchievementRarity.Rare,
            Points = 55,
            Icon = "🌪️",
            Condition = new AchievementCondition { MetricName = "PerfectDodges", TargetValue = 25, ComparisonOperator = ">=" }
        });

        // Speed Achievements
        _achievements.Add(new AchievementDefinition
        {
            Id = "speed_runner",
            Name = "Speed Runner",
            Description = "Complete a level in under 2 minutes",
            Category = AchievementCategory.Speed,
            Rarity = AchievementRarity.Uncommon,
            Points = 40,
            Icon = "⚡",
            Condition = new AchievementCondition { MetricName = "AvgTimePerLevelSeconds", TargetValue = 120, ComparisonOperator = "<=" }
        });

        _achievements.Add(new AchievementDefinition
        {
            Id = "marathon_runner",
            Name = "Marathon Runner",
            Description = "Play for 30 minutes in one session",
            Category = AchievementCategory.Speed,
            Rarity = AchievementRarity.Common,
            Points = 20,
            Icon = "🏃",
            Condition = new AchievementCondition { MetricName = "TotalPlaytimeMinutes", TargetValue = 30, ComparisonOperator = ">=" }
        });

        _logger.LogInformation("Initialized {Count} achievements", _achievements.Count);
    }

    /// <summary>
    /// Check achievements against current metrics
    /// </summary>
    public List<AchievementDefinition> CheckAchievements(Dictionary<string, double> metrics)
    {
        var newlyUnlocked = new List<AchievementDefinition>();

        foreach (var achievement in _achievements)
        {
            // Skip if already unlocked
            if (_unlocks.Any(u => u.AchievementId == achievement.Id))
                continue;

            if (IsAchievementUnlocked(achievement, metrics))
            {
                var unlock = new AchievementUnlock
                {
                    AchievementId = achievement.Id,
                    UnlockTime = DateTime.UtcNow,
                    SessionId = _sessionId,
                    CurrentValue = metrics.GetValueOrDefault(achievement.Condition.MetricName, 0)
                };

                _unlocks.Add(unlock);
                newlyUnlocked.Add(achievement);

                _logger.LogInformation(
                    "Achievement unlocked: {Name} ({Points} points)",
                    achievement.Name,
                    achievement.Points);

                OnAchievementUnlocked?.Invoke(achievement);
            }
        }

        return newlyUnlocked;
    }

    private bool IsAchievementUnlocked(AchievementDefinition achievement, Dictionary<string, double> metrics)
    {
        if (!metrics.TryGetValue(achievement.Condition.MetricName, out var currentValue))
            return false;

        return achievement.Condition.ComparisonOperator switch
        {
            ">=" => currentValue >= achievement.Condition.TargetValue,
            "<=" => currentValue <= achievement.Condition.TargetValue,
            "==" => Math.Abs(currentValue - achievement.Condition.TargetValue) < 0.001,
            ">" => currentValue > achievement.Condition.TargetValue,
            "<" => currentValue < achievement.Condition.TargetValue,
            _ => false
        };
    }

    /// <summary>
    /// Get progress for all achievements
    /// </summary>
    public List<AchievementProgress> GetProgress(Dictionary<string, double> metrics)
    {
        var progress = new List<AchievementProgress>();

        foreach (var achievement in _achievements)
        {
            var currentValue = metrics.GetValueOrDefault(achievement.Condition.MetricName, 0);
            var isUnlocked = _unlocks.Any(u => u.AchievementId == achievement.Id);

            progress.Add(new AchievementProgress
            {
                AchievementId = achievement.Id,
                CurrentValue = currentValue,
                TargetValue = achievement.Condition.TargetValue,
                IsUnlocked = isUnlocked
            });
        }

        return progress;
    }

    /// <summary>
    /// Get total achievement points earned
    /// </summary>
    public int GetTotalPoints()
    {
        return _unlocks
            .Select(u => _achievements.FirstOrDefault(a => a.Id == u.AchievementId))
            .Where(a => a != null)
            .Sum(a => a!.Points);
    }

    /// <summary>
    /// Get achievement completion percentage
    /// </summary>
    public double GetCompletionPercentage()
    {
        if (_achievements.Count == 0) return 0;
        return (double)_unlocks.Count / _achievements.Count * 100;
    }

    /// <summary>
    /// Export achievements to JSON
    /// </summary>
    public string ExportToJson()
    {
        var data = new
        {
            TotalAchievements = _achievements.Count,
            UnlockedCount = _unlocks.Count,
            CompletionPercentage = GetCompletionPercentage(),
            TotalPoints = GetTotalPoints(),
            Unlocks = _unlocks.Select(u => new
            {
                Achievement = _achievements.FirstOrDefault(a => a.Id == u.AchievementId),
                u.UnlockTime,
                u.CurrentValue
            })
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }
}
