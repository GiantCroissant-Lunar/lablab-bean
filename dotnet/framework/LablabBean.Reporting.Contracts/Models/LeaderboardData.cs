using System;
using System.Collections.Generic;

namespace LablabBean.Reporting.Contracts.Models;

/// <summary>
/// Leaderboard categories for rankings
/// </summary>
public enum LeaderboardCategory
{
    TotalScore,
    HighestKills,
    BestKDRatio,
    MostLevelsCompleted,
    FastestCompletion,
    MostItemsCollected,
    DeepestDungeon,
    AchievementPoints
}

/// <summary>
/// Single leaderboard entry
/// </summary>
public class LeaderboardEntry
{
    public string PlayerName { get; set; } = "Player";
    public string SessionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public long Score { get; set; }
    public int Rank { get; set; }

    // Additional stats for context
    public Dictionary<string, object> Stats { get; set; } = new();
}

/// <summary>
/// Complete leaderboard data for a category
/// </summary>
public class LeaderboardData
{
    public LeaderboardCategory Category { get; set; }
    public List<LeaderboardEntry> Entries { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public int MaxEntries { get; set; } = 100;
}

/// <summary>
/// All leaderboards collection
/// </summary>
public class LeaderboardCollection
{
    public Dictionary<LeaderboardCategory, LeaderboardData> Leaderboards { get; set; } = new();
    public DateTime LastSaved { get; set; }
    public string Version { get; set; } = "1.0.0";
}

/// <summary>
/// Player persistent data
/// </summary>
public class PlayerProfile
{
    public string PlayerName { get; set; } = "Player";
    public DateTime CreatedAt { get; set; }
    public DateTime LastPlayedAt { get; set; }

    // Lifetime stats
    public int TotalSessions { get; set; }
    public TimeSpan TotalPlaytime { get; set; }
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public int TotalItemsCollected { get; set; }
    public int TotalLevelsCompleted { get; set; }
    public int TotalDungeonsCompleted { get; set; }
    public int DeepestDepthReached { get; set; }

    // Achievements
    public List<AchievementUnlock> UnlockedAchievements { get; set; } = new();
    public int TotalAchievementPoints { get; set; }

    // Personal bests
    public Dictionary<LeaderboardCategory, long> PersonalBests { get; set; } = new();

    // Session history (keep last 50)
    public List<SessionSummary> RecentSessions { get; set; } = new();
}

/// <summary>
/// Condensed session summary for history
/// </summary>
public class SessionSummary
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int LevelsCompleted { get; set; }
    public int ItemsCollected { get; set; }
    public int AchievementsUnlocked { get; set; }
    public long TotalScore { get; set; }
}
