using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using LablabBean.Reporting.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Analytics;

/// <summary>
/// Handles persistence of player data, achievements, and leaderboards
/// </summary>
public class PersistenceService
{
    private readonly ILogger<PersistenceService> _logger;
    private readonly string _dataDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string PROFILE_FILE = "player_profile.json";
    private const string LEADERBOARD_FILE = "leaderboards.json";
    private const string BACKUP_SUFFIX = ".backup";

    public PersistenceService(ILogger<PersistenceService> logger, string? dataDirectory = null)
    {
        _logger = logger;
        _dataDirectory = dataDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LablabBean",
            "GameData"
        );

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        EnsureDataDirectory();
    }

    private void EnsureDataDirectory()
    {
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
            _logger.LogInformation("Created data directory: {Directory}", _dataDirectory);
        }
    }

    #region Player Profile

    /// <summary>
    /// Load player profile or create new one
    /// </summary>
    public PlayerProfile LoadPlayerProfile()
    {
        var filePath = Path.Combine(_dataDirectory, PROFILE_FILE);

        if (!File.Exists(filePath))
        {
            _logger.LogInformation("No existing profile found, creating new player profile");
            return CreateNewProfile();
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var profile = JsonSerializer.Deserialize<PlayerProfile>(json, _jsonOptions);

            if (profile == null)
            {
                _logger.LogWarning("Failed to deserialize profile, creating new one");
                return CreateNewProfile();
            }

            _logger.LogInformation("Loaded player profile: {PlayerName}, {Sessions} sessions, {Achievements} achievements",
                profile.PlayerName, profile.TotalSessions, profile.UnlockedAchievements.Count);

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading player profile, attempting backup restore");
            return TryRestoreBackup<PlayerProfile>(PROFILE_FILE) ?? CreateNewProfile();
        }
    }

    /// <summary>
    /// Save player profile with backup
    /// </summary>
    public void SavePlayerProfile(PlayerProfile profile)
    {
        var filePath = Path.Combine(_dataDirectory, PROFILE_FILE);

        try
        {
            // Create backup of existing file
            if (File.Exists(filePath))
            {
                File.Copy(filePath, filePath + BACKUP_SUFFIX, overwrite: true);
            }

            var json = JsonSerializer.Serialize(profile, _jsonOptions);
            File.WriteAllText(filePath, json);

            _logger.LogInformation("Saved player profile: {PlayerName}", profile.PlayerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving player profile");
            throw;
        }
    }

    private PlayerProfile CreateNewProfile()
    {
        return new PlayerProfile
        {
            PlayerName = Environment.UserName,
            CreatedAt = DateTime.UtcNow,
            LastPlayedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region Leaderboards

    /// <summary>
    /// Load all leaderboards or create new collection
    /// </summary>
    public LeaderboardCollection LoadLeaderboards()
    {
        var filePath = Path.Combine(_dataDirectory, LEADERBOARD_FILE);

        if (!File.Exists(filePath))
        {
            _logger.LogInformation("No existing leaderboards found, creating new collection");
            return CreateNewLeaderboardCollection();
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var collection = JsonSerializer.Deserialize<LeaderboardCollection>(json, _jsonOptions);

            if (collection == null)
            {
                _logger.LogWarning("Failed to deserialize leaderboards, creating new collection");
                return CreateNewLeaderboardCollection();
            }

            var totalEntries = collection.Leaderboards.Values.Sum(lb => lb.Entries.Count);
            _logger.LogInformation("Loaded leaderboards: {Count} categories, {Total} total entries",
                collection.Leaderboards.Count, totalEntries);

            return collection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading leaderboards, attempting backup restore");
            return TryRestoreBackup<LeaderboardCollection>(LEADERBOARD_FILE) ?? CreateNewLeaderboardCollection();
        }
    }

    /// <summary>
    /// Save leaderboards with backup
    /// </summary>
    public void SaveLeaderboards(LeaderboardCollection collection)
    {
        var filePath = Path.Combine(_dataDirectory, LEADERBOARD_FILE);

        try
        {
            // Create backup of existing file
            if (File.Exists(filePath))
            {
                File.Copy(filePath, filePath + BACKUP_SUFFIX, overwrite: true);
            }

            collection.LastSaved = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(collection, _jsonOptions);
            File.WriteAllText(filePath, json);

            var totalEntries = collection.Leaderboards.Values.Sum(lb => lb.Entries.Count);
            _logger.LogInformation("Saved leaderboards: {Count} categories, {Total} total entries",
                collection.Leaderboards.Count, totalEntries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving leaderboards");
            throw;
        }
    }

    private LeaderboardCollection CreateNewLeaderboardCollection()
    {
        var collection = new LeaderboardCollection();

        // Initialize all leaderboard categories
        foreach (LeaderboardCategory category in Enum.GetValues(typeof(LeaderboardCategory)))
        {
            collection.Leaderboards[category] = new LeaderboardData
            {
                Category = category,
                LastUpdated = DateTime.UtcNow
            };
        }

        return collection;
    }

    #endregion

    #region Session History Export

    /// <summary>
    /// Export session data to JSON
    /// </summary>
    public void ExportSessionData(SessionStatisticsData sessionData, string? fileName = null)
    {
        var exportDir = Path.Combine(_dataDirectory, "sessions");
        Directory.CreateDirectory(exportDir);

        fileName ??= $"session_{sessionData.SessionId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(exportDir, fileName);

        try
        {
            var json = JsonSerializer.Serialize(sessionData, _jsonOptions);
            File.WriteAllText(filePath, json);

            _logger.LogInformation("Exported session data: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting session data");
        }
    }

    /// <summary>
    /// Get all exported session files
    /// </summary>
    public List<string> GetSessionFiles()
    {
        var exportDir = Path.Combine(_dataDirectory, "sessions");

        if (!Directory.Exists(exportDir))
            return new List<string>();

        return Directory.GetFiles(exportDir, "session_*.json")
            .OrderByDescending(f => File.GetCreationTime(f))
            .ToList();
    }

    #endregion

    #region Backup & Restore

    private T? TryRestoreBackup<T>(string fileName) where T : class
    {
        var backupPath = Path.Combine(_dataDirectory, fileName + BACKUP_SUFFIX);

        if (!File.Exists(backupPath))
        {
            _logger.LogWarning("No backup file found: {BackupPath}", backupPath);
            return null;
        }

        try
        {
            var json = File.ReadAllText(backupPath);
            var data = JsonSerializer.Deserialize<T>(json, _jsonOptions);

            if (data != null)
            {
                _logger.LogInformation("Successfully restored from backup: {BackupPath}", backupPath);
                return data;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring from backup: {BackupPath}", backupPath);
        }

        return null;
    }

    /// <summary>
    /// Create manual backup of all data
    /// </summary>
    public void CreateManualBackup()
    {
        var backupDir = Path.Combine(_dataDirectory, "backups", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(backupDir);

        try
        {
            // Backup profile
            var profilePath = Path.Combine(_dataDirectory, PROFILE_FILE);
            if (File.Exists(profilePath))
            {
                File.Copy(profilePath, Path.Combine(backupDir, PROFILE_FILE));
            }

            // Backup leaderboards
            var leaderboardPath = Path.Combine(_dataDirectory, LEADERBOARD_FILE);
            if (File.Exists(leaderboardPath))
            {
                File.Copy(leaderboardPath, Path.Combine(backupDir, LEADERBOARD_FILE));
            }

            _logger.LogInformation("Created manual backup: {BackupDir}", backupDir);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating manual backup");
        }
    }

    #endregion

    /// <summary>
    /// Get the data directory path
    /// </summary>
    public string GetDataDirectory() => _dataDirectory;

    /// <summary>
    /// Clear all data (use with caution!)
    /// </summary>
    public void ClearAllData()
    {
        try
        {
            if (Directory.Exists(_dataDirectory))
            {
                CreateManualBackup(); // Safety backup
                Directory.Delete(_dataDirectory, recursive: true);
                EnsureDataDirectory();
                _logger.LogWarning("Cleared all game data (backup created)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing data");
            throw;
        }
    }
}
