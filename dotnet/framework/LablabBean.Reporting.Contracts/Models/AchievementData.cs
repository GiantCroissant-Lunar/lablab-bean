namespace LablabBean.Reporting.Contracts.Models;

/// <summary>
/// Achievement category classification
/// </summary>
public enum AchievementCategory
{
    Combat,
    Exploration,
    Collection,
    Survival,
    Speed,
    Mastery
}

/// <summary>
/// Achievement rarity/difficulty
/// </summary>
public enum AchievementRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// Achievement definition
/// </summary>
public class AchievementDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AchievementCategory Category { get; set; }
    public AchievementRarity Rarity { get; set; }
    public int Points { get; set; }
    public string Icon { get; set; } = "🏆";

    // Unlock conditions
    public AchievementCondition Condition { get; set; } = new();
}

/// <summary>
/// Achievement unlock condition
/// </summary>
public class AchievementCondition
{
    public string MetricName { get; set; } = string.Empty;
    public double TargetValue { get; set; }
    public string ComparisonOperator { get; set; } = ">="; // >=, <=, ==, >, <
}

/// <summary>
/// Achievement unlock data
/// </summary>
public class AchievementUnlock
{
    public string AchievementId { get; set; } = string.Empty;
    public System.DateTime UnlockTime { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
}

/// <summary>
/// Achievement progress tracking
/// </summary>
public class AchievementProgress
{
    public string AchievementId { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double TargetValue { get; set; }
    public double ProgressPercentage => TargetValue > 0 ? (CurrentValue / TargetValue * 100) : 0;
    public bool IsUnlocked { get; set; }
}
