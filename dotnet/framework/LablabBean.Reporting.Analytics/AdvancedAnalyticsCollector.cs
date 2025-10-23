using LablabBean.Reporting.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Analytics;

/// <summary>
/// Advanced analytics collector for detailed game metrics
/// Tracks items, enemies, time, and combat statistics
/// </summary>
public class AdvancedAnalyticsCollector
{
    private readonly ILogger<AdvancedAnalyticsCollector> _logger;
    private readonly DateTime _sessionStart;

    // Item tracking
    private readonly Dictionary<ItemType, int> _itemsByType = new();

    // Enemy tracking
    private readonly Dictionary<EnemyType, int> _killsByEnemyType = new();

    // Time tracking
    private DateTime? _currentLevelStartTime;
    private DateTime? _currentDungeonStartTime;
    private readonly List<TimeSpan> _levelDurations = new();
    private readonly List<TimeSpan> _dungeonDurations = new();

    // Combat statistics
    private int _damageDealt;
    private int _damageTaken;
    private int _healingReceived;
    private int _criticalHits;
    private int _perfectDodges;
    private readonly List<int> _damagePerHit = new();

    public AdvancedAnalyticsCollector(ILogger<AdvancedAnalyticsCollector> logger)
    {
        _logger = logger;
        _sessionStart = DateTime.UtcNow;

        // Initialize all item types to 0
        foreach (ItemType type in Enum.GetValues<ItemType>())
        {
            _itemsByType[type] = 0;
        }

        // Initialize all enemy types to 0
        foreach (EnemyType type in Enum.GetValues<EnemyType>())
        {
            _killsByEnemyType[type] = 0;
        }

        _logger.LogInformation("Advanced analytics collector initialized");
    }

    #region Item Tracking

    /// <summary>
    /// Records an item collection
    /// </summary>
    public void RecordItemCollected(ItemType itemType)
    {
        _itemsByType[itemType]++;
        _logger.LogDebug("Item collected: {ItemType}", itemType);
    }

    /// <summary>
    /// Gets item type breakdown
    /// </summary>
    public List<ItemTypeData> GetItemTypeBreakdown()
    {
        var total = _itemsByType.Values.Sum();
        if (total == 0) return new List<ItemTypeData>();

        return _itemsByType
            .Select(kvp => new ItemTypeData
            {
                Type = kvp.Key,
                Count = kvp.Value,
                Percentage = (double)kvp.Value / total * 100
            })
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    #endregion

    #region Enemy Tracking

    /// <summary>
    /// Records an enemy kill
    /// </summary>
    public void RecordEnemyKilled(EnemyType enemyType)
    {
        _killsByEnemyType[enemyType]++;
        _logger.LogDebug("Enemy killed: {EnemyType}", enemyType);
    }

    /// <summary>
    /// Gets enemy type distribution
    /// </summary>
    public List<EnemyTypeData> GetEnemyTypeDistribution()
    {
        var total = _killsByEnemyType.Values.Sum();
        if (total == 0) return new List<EnemyTypeData>();

        return _killsByEnemyType
            .Select(kvp => new EnemyTypeData
            {
                Type = kvp.Key,
                Kills = kvp.Value,
                Percentage = (double)kvp.Value / total * 100
            })
            .OrderByDescending(x => x.Kills)
            .ToList();
    }

    #endregion

    #region Time Tracking

    /// <summary>
    /// Starts tracking a new level
    /// </summary>
    public void StartLevel()
    {
        _currentLevelStartTime = DateTime.UtcNow;
        _logger.LogDebug("Level started at {Time}", _currentLevelStartTime);
    }

    /// <summary>
    /// Ends current level tracking
    /// </summary>
    public void EndLevel()
    {
        if (_currentLevelStartTime.HasValue)
        {
            var duration = DateTime.UtcNow - _currentLevelStartTime.Value;
            _levelDurations.Add(duration);
            _logger.LogDebug("Level completed in {Duration:mm\\:ss}", duration);
            _currentLevelStartTime = null;
        }
    }

    /// <summary>
    /// Starts tracking a new dungeon
    /// </summary>
    public void StartDungeon()
    {
        _currentDungeonStartTime = DateTime.UtcNow;
        _logger.LogDebug("Dungeon started at {Time}", _currentDungeonStartTime);
    }

    /// <summary>
    /// Ends current dungeon tracking
    /// </summary>
    public void EndDungeon()
    {
        if (_currentDungeonStartTime.HasValue)
        {
            var duration = DateTime.UtcNow - _currentDungeonStartTime.Value;
            _dungeonDurations.Add(duration);
            _logger.LogDebug("Dungeon completed in {Duration:mm\\:ss}", duration);
            _currentDungeonStartTime = null;
        }
    }

    /// <summary>
    /// Gets time analytics
    /// </summary>
    public TimeAnalyticsData GetTimeAnalytics()
    {
        var totalPlaytime = DateTime.UtcNow - _sessionStart;
        var avgTimePerLevel = _levelDurations.Any()
            ? TimeSpan.FromTicks((long)_levelDurations.Average(d => d.Ticks))
            : TimeSpan.Zero;
        var avgTimePerDungeon = _dungeonDurations.Any()
            ? TimeSpan.FromTicks((long)_dungeonDurations.Average(d => d.Ticks))
            : TimeSpan.Zero;

        return new TimeAnalyticsData
        {
            TotalPlaytime = totalPlaytime,
            AverageTimePerLevel = avgTimePerLevel,
            AverageTimePerDungeon = avgTimePerDungeon,
            SessionStartTime = _sessionStart,
            SessionEndTime = DateTime.UtcNow
        };
    }

    #endregion

    #region Combat Statistics

    /// <summary>
    /// Records damage dealt to an enemy
    /// </summary>
    public void RecordDamageDealt(int damage, bool isCritical = false)
    {
        _damageDealt += damage;
        _damagePerHit.Add(damage);

        if (isCritical)
        {
            _criticalHits++;
            _logger.LogDebug("Critical hit! Damage: {Damage}", damage);
        }
    }

    /// <summary>
    /// Records damage taken from an enemy
    /// </summary>
    public void RecordDamageTaken(int damage)
    {
        _damageTaken += damage;
        _logger.LogDebug("Damage taken: {Damage}", damage);
    }

    /// <summary>
    /// Records healing received
    /// </summary>
    public void RecordHealing(int amount)
    {
        _healingReceived += amount;
        _logger.LogDebug("Healing received: {Amount}", amount);
    }

    /// <summary>
    /// Records a perfect dodge
    /// </summary>
    public void RecordPerfectDodge()
    {
        _perfectDodges++;
        _logger.LogDebug("Perfect dodge!");
    }

    /// <summary>
    /// Gets detailed combat statistics
    /// </summary>
    public CombatStatisticsData GetCombatStatistics(int totalKills, int totalDeaths)
    {
        var avgDamagePerHit = _damagePerHit.Any()
            ? _damagePerHit.Average()
            : 0;

        var survivalRate = totalKills + totalDeaths > 0
            ? (double)totalKills / (totalKills + totalDeaths) * 100
            : 0;

        return new CombatStatisticsData
        {
            TotalKills = totalKills,
            TotalDeaths = totalDeaths,
            KDRatio = totalDeaths > 0 ? (double)totalKills / totalDeaths : totalKills,
            DamageDealt = _damageDealt,
            DamageTaken = _damageTaken,
            HealingReceived = _healingReceived,
            CriticalHits = _criticalHits,
            PerfectDodges = _perfectDodges,
            AverageDamagePerHit = avgDamagePerHit,
            SurvivalRate = survivalRate
        };
    }

    #endregion
}
