using System;
using System.Collections.Generic;
using System.Linq;
using LablabBean.Reporting.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Analytics;

/// <summary>
/// Manages leaderboard rankings and score calculations
/// </summary>
public class LeaderboardSystem
{
    private readonly ILogger<LeaderboardSystem> _logger;
    private readonly PersistenceService _persistenceService;
    private LeaderboardCollection _leaderboards;
    private PlayerProfile _playerProfile;

    public LeaderboardSystem(
        ILogger<LeaderboardSystem> logger,
        PersistenceService persistenceService)
    {
        _logger = logger;
        _persistenceService = persistenceService;
        _leaderboards = _persistenceService.LoadLeaderboards();
        _playerProfile = _persistenceService.LoadPlayerProfile();
    }

    /// <summary>
    /// Submit a session to all relevant leaderboards
    /// </summary>
    public List<LeaderboardEntry> SubmitSession(SessionStatisticsData sessionData)
    {
        var newEntries = new List<LeaderboardEntry>();
        var playerName = _playerProfile.PlayerName;

        // Calculate scores for each category
        var scores = CalculateScores(sessionData);

        foreach (var (category, score) in scores)
        {
            if (score <= 0) continue; // Skip zero scores

            var entry = new LeaderboardEntry
            {
                PlayerName = playerName,
                SessionId = sessionData.SessionId,
                Timestamp = sessionData.SessionEndTime,
                Score = score,
                Stats = CreateStatsContext(sessionData, category)
            };

            var added = AddToLeaderboard(category, entry);
            if (added)
            {
                newEntries.Add(entry);
            }
        }

        if (newEntries.Any())
        {
            _persistenceService.SaveLeaderboards(_leaderboards);
            _logger.LogInformation("Submitted session to {Count} leaderboards", newEntries.Count);
        }

        return newEntries;
    }

    /// <summary>
    /// Calculate scores for all leaderboard categories
    /// </summary>
    private Dictionary<LeaderboardCategory, long> CalculateScores(SessionStatisticsData sessionData)
    {
        var scores = new Dictionary<LeaderboardCategory, long>();

        // Total Score = Weighted combination of all stats
        var totalScore = CalculateTotalScore(sessionData);
        scores[LeaderboardCategory.TotalScore] = totalScore;

        // Category-specific scores
        scores[LeaderboardCategory.HighestKills] = sessionData.TotalKills;
        scores[LeaderboardCategory.BestKDRatio] = (long)(sessionData.KillDeathRatio * 100); // Multiply by 100 for precision
        scores[LeaderboardCategory.MostLevelsCompleted] = sessionData.LevelsCompleted;
        scores[LeaderboardCategory.MostItemsCollected] = sessionData.ItemsCollected;
        scores[LeaderboardCategory.AchievementPoints] = sessionData.AchievementsUnlocked * 10; // Placeholder

        // Fastest completion (negative time = better, so invert)
        if (sessionData.LevelsCompleted > 0)
        {
            var avgTimePerLevel = sessionData.TotalPlaytime.TotalSeconds / sessionData.LevelsCompleted;
            scores[LeaderboardCategory.FastestCompletion] = (long)(10000 - avgTimePerLevel); // Lower time = higher score
        }

        return scores;
    }

    /// <summary>
    /// Calculate total weighted score
    /// </summary>
    private long CalculateTotalScore(SessionStatisticsData sessionData)
    {
        long score = 0;

        // Combat performance (40% weight)
        score += sessionData.TotalKills * 100;
        score += sessionData.TotalDamageDealt / 10;
        score -= sessionData.TotalDeaths * 50;

        // Progression (30% weight)
        score += sessionData.LevelsCompleted * 500;
        score += sessionData.ItemsCollected * 20;

        // Achievements (20% weight)
        score += sessionData.AchievementsUnlocked * 1000;

        // Efficiency bonus (10% weight)
        if (sessionData.TotalDeaths > 0)
        {
            var kdBonus = (long)(sessionData.KillDeathRatio * 200);
            score += kdBonus;
        }

        // Time bonus (faster is better for same progression)
        if (sessionData.LevelsCompleted > 0)
        {
            var timePerLevel = sessionData.TotalPlaytime.TotalMinutes / sessionData.LevelsCompleted;
            if (timePerLevel < 5) // Completed level in under 5 minutes
            {
                score += (long)((5 - timePerLevel) * 100);
            }
        }

        return Math.Max(0, score); // Ensure non-negative
    }

    /// <summary>
    /// Add entry to leaderboard if it qualifies
    /// </summary>
    private bool AddToLeaderboard(LeaderboardCategory category, LeaderboardEntry entry)
    {
        if (!_leaderboards.Leaderboards.TryGetValue(category, out var leaderboard))
        {
            leaderboard = new LeaderboardData { Category = category };
            _leaderboards.Leaderboards[category] = leaderboard;
        }

        // Check if score qualifies (better than lowest entry or board not full)
        if (leaderboard.Entries.Count >= leaderboard.MaxEntries)
        {
            var lowestScore = leaderboard.Entries.Min(e => e.Score);
            if (entry.Score <= lowestScore)
            {
                return false; // Score doesn't qualify
            }

            // Remove lowest entry
            var lowestEntry = leaderboard.Entries.OrderBy(e => e.Score).First();
            leaderboard.Entries.Remove(lowestEntry);
        }

        // Add new entry
        leaderboard.Entries.Add(entry);

        // Re-rank all entries
        RankLeaderboard(leaderboard);

        leaderboard.LastUpdated = DateTime.UtcNow;

        _logger.LogInformation("Added to {Category} leaderboard: Rank #{Rank}, Score {Score}",
            category, entry.Rank, entry.Score);

        return true;
    }

    /// <summary>
    /// Assign ranks to leaderboard entries
    /// </summary>
    private void RankLeaderboard(LeaderboardData leaderboard)
    {
        var sorted = leaderboard.Entries
            .OrderByDescending(e => e.Score)
            .ThenBy(e => e.Timestamp)
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].Rank = i + 1;
        }

        leaderboard.Entries = sorted;
    }

    /// <summary>
    /// Get top entries for a category
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboard(LeaderboardCategory category, int topN = 10)
    {
        if (!_leaderboards.Leaderboards.TryGetValue(category, out var leaderboard))
        {
            return new List<LeaderboardEntry>();
        }

        return leaderboard.Entries
            .OrderBy(e => e.Rank)
            .Take(topN)
            .ToList();
    }

    /// <summary>
    /// Get player's rank in a category
    /// </summary>
    public int? GetPlayerRank(LeaderboardCategory category, string playerName)
    {
        if (!_leaderboards.Leaderboards.TryGetValue(category, out var leaderboard))
        {
            return null;
        }

        var entry = leaderboard.Entries
            .Where(e => e.PlayerName == playerName)
            .OrderBy(e => e.Rank)
            .FirstOrDefault();

        return entry?.Rank;
    }

    /// <summary>
    /// Get player's best entries across all categories
    /// </summary>
    public Dictionary<LeaderboardCategory, LeaderboardEntry> GetPlayerBestEntries(string playerName)
    {
        var bestEntries = new Dictionary<LeaderboardCategory, LeaderboardEntry>();

        foreach (var (category, leaderboard) in _leaderboards.Leaderboards)
        {
            var playerBest = leaderboard.Entries
                .Where(e => e.PlayerName == playerName)
                .OrderBy(e => e.Rank)
                .FirstOrDefault();

            if (playerBest != null)
            {
                bestEntries[category] = playerBest;
            }
        }

        return bestEntries;
    }

    /// <summary>
    /// Create contextual stats for leaderboard entry
    /// </summary>
    private Dictionary<string, object> CreateStatsContext(SessionStatisticsData sessionData, LeaderboardCategory category)
    {
        var stats = new Dictionary<string, object>
        {
            ["Kills"] = sessionData.TotalKills,
            ["Deaths"] = sessionData.TotalDeaths,
            ["KDRatio"] = sessionData.KillDeathRatio,
            ["Levels"] = sessionData.LevelsCompleted,
            ["Items"] = sessionData.ItemsCollected,
            ["Playtime"] = sessionData.TotalPlaytime.ToString(@"hh\:mm\:ss")
        };

        // Add category-specific highlights
        switch (category)
        {
            case LeaderboardCategory.TotalScore:
                stats["DamageDealt"] = sessionData.TotalDamageDealt;
                stats["Achievements"] = sessionData.AchievementsUnlocked;
                break;

            case LeaderboardCategory.FastestCompletion:
                stats["AvgTimePerLevel"] = sessionData.LevelsCompleted > 0
                    ? TimeSpan.FromSeconds(sessionData.TotalPlaytime.TotalSeconds / sessionData.LevelsCompleted).ToString(@"mm\:ss")
                    : "N/A";
                break;
        }

        return stats;
    }

    /// <summary>
    /// Get current player profile
    /// </summary>
    public PlayerProfile GetPlayerProfile() => _playerProfile;

    /// <summary>
    /// Update player profile with session data
    /// </summary>
    public void UpdatePlayerProfile(SessionStatisticsData sessionData, List<AchievementDefinition> newAchievements)
    {
        _playerProfile.LastPlayedAt = DateTime.UtcNow;
        _playerProfile.TotalSessions++;
        _playerProfile.TotalPlaytime += sessionData.TotalPlaytime;
        _playerProfile.TotalKills += sessionData.TotalKills;
        _playerProfile.TotalDeaths += sessionData.TotalDeaths;
        _playerProfile.TotalItemsCollected += sessionData.ItemsCollected;
        _playerProfile.TotalLevelsCompleted += sessionData.LevelsCompleted;

        // Add achievements
        foreach (var achievement in newAchievements)
        {
            if (!_playerProfile.UnlockedAchievements.Any(a => a.AchievementId == achievement.Id))
            {
                _playerProfile.UnlockedAchievements.Add(new AchievementUnlock
                {
                    AchievementId = achievement.Id,
                    UnlockTime = DateTime.UtcNow,
                    SessionId = sessionData.SessionId
                });
                _playerProfile.TotalAchievementPoints += achievement.Points;
            }
        }

        // Update personal bests
        var scores = CalculateScores(sessionData);
        foreach (var (category, score) in scores)
        {
            if (!_playerProfile.PersonalBests.ContainsKey(category) || score > _playerProfile.PersonalBests[category])
            {
                _playerProfile.PersonalBests[category] = score;
            }
        }

        // Add to session history (keep last 50)
        _playerProfile.RecentSessions.Add(new SessionSummary
        {
            SessionId = sessionData.SessionId,
            StartTime = sessionData.SessionStartTime,
            Duration = sessionData.TotalPlaytime,
            Kills = sessionData.TotalKills,
            Deaths = sessionData.TotalDeaths,
            LevelsCompleted = sessionData.LevelsCompleted,
            ItemsCollected = sessionData.ItemsCollected,
            AchievementsUnlocked = newAchievements.Count,
            TotalScore = scores.GetValueOrDefault(LeaderboardCategory.TotalScore, 0)
        });

        if (_playerProfile.RecentSessions.Count > 50)
        {
            _playerProfile.RecentSessions = _playerProfile.RecentSessions
                .OrderByDescending(s => s.StartTime)
                .Take(50)
                .ToList();
        }

        _persistenceService.SavePlayerProfile(_playerProfile);

        _logger.LogInformation("Updated player profile: {Sessions} total sessions, {Points} achievement points",
            _playerProfile.TotalSessions, _playerProfile.TotalAchievementPoints);
    }

    /// <summary>
    /// Get all leaderboards
    /// </summary>
    public LeaderboardCollection GetAllLeaderboards() => _leaderboards;

    /// <summary>
    /// Clear specific leaderboard
    /// </summary>
    public void ClearLeaderboard(LeaderboardCategory category)
    {
        if (_leaderboards.Leaderboards.TryGetValue(category, out var leaderboard))
        {
            leaderboard.Entries.Clear();
            leaderboard.LastUpdated = DateTime.UtcNow;
            _persistenceService.SaveLeaderboards(_leaderboards);
            _logger.LogInformation("Cleared {Category} leaderboard", category);
        }
    }
}
