using Arch.Core;
using LablabBean.Game.Core.Components;

namespace LablabBean.Game.Core.Systems;

/// <summary>
/// System for scaling enemy difficulty and loot based on dungeon level
/// </summary>
public class DifficultyScalingSystem
{
    private readonly World _world;
    private readonly Random _random;

    // Scaling constants
    private const double STAT_SCALING_BASE = 1.12;
    private const int MAX_SCALING_LEVEL = 30;
    private const int BASE_LOOT_RATE = 10;
    private const int LOOT_RATE_PER_LEVEL = 5;
    private const int MAX_LOOT_RATE = 60;
    private const int FEET_PER_LEVEL = 30;

    public DifficultyScalingSystem(World world, int? seed = null)
    {
        _world = world;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Apply difficulty scaling to an enemy entity
    /// </summary>
    public void ApplyEnemyScaling(Entity enemy, int dungeonLevel)
    {
        if (!_world.Has<Health>(enemy) || !_world.Has<Combat>(enemy))
            return;

        var health = _world.Get<Health>(enemy);
        var combat = _world.Get<Combat>(enemy);

        // Scale stats
        int scaledMaxHealth = CalculateScaledStat(health.Maximum, dungeonLevel);
        int scaledAttack = CalculateScaledStat(combat.Attack, dungeonLevel);
        int scaledDefense = CalculateScaledStat(combat.Defense, dungeonLevel);

        // Update components
        _world.Set(enemy, new Health(scaledMaxHealth, scaledMaxHealth));
        _world.Set(enemy, new Combat(scaledAttack, scaledDefense));

        // Scale speed if actor component exists
        if (_world.Has<Actor>(enemy))
        {
            var actor = _world.Get<Actor>(enemy);
            int scaledSpeed = CalculateScaledStat(actor.Speed, dungeonLevel);
            _world.Set(enemy, new Actor(scaledSpeed));
        }
    }

    /// <summary>
    /// Calculate scaled stat based on dungeon level
    /// Formula: base_stat * (STAT_SCALING_BASE ^ min(level, MAX_SCALING_LEVEL))
    /// </summary>
    public int CalculateScaledStat(int baseStat, int dungeonLevel)
    {
        if (dungeonLevel <= 1)
            return baseStat;

        int effectiveLevel = Math.Min(dungeonLevel, MAX_SCALING_LEVEL);
        double multiplier = Math.Pow(STAT_SCALING_BASE, effectiveLevel - 1);
        return (int)Math.Round(baseStat * multiplier);
    }

    /// <summary>
    /// Calculate loot drop rate for a dungeon level
    /// Formula: BASE_LOOT_RATE + (level * LOOT_RATE_PER_LEVEL), capped at MAX_LOOT_RATE
    /// </summary>
    public int CalculateLootDropRate(int dungeonLevel)
    {
        int rate = BASE_LOOT_RATE + (dungeonLevel * LOOT_RATE_PER_LEVEL);
        return Math.Min(rate, MAX_LOOT_RATE);
    }

    /// <summary>
    /// Determine if loot should drop based on dungeon level
    /// </summary>
    public bool ShouldDropLoot(int dungeonLevel)
    {
        int dropRate = CalculateLootDropRate(dungeonLevel);
        return _random.Next(100) < dropRate;
    }

    /// <summary>
    /// Determine if equipment should drop (vs consumables)
    /// Equipment chance increases with level
    /// </summary>
    public bool ShouldDropEquipment(int dungeonLevel)
    {
        int equipmentChance = Math.Min(10 + (dungeonLevel * 2), 50);
        return _random.Next(100) < equipmentChance;
    }

    /// <summary>
    /// Get depth in feet for display
    /// </summary>
    public int GetDepthInFeet(int dungeonLevel)
    {
        return dungeonLevel * FEET_PER_LEVEL;
    }

    /// <summary>
    /// Get formatted depth display string
    /// </summary>
    public string GetDepthDisplayString(int dungeonLevel)
    {
        return $"Depth: -{GetDepthInFeet(dungeonLevel)} ft";
    }

    /// <summary>
    /// Get scaling multiplier for a given level
    /// </summary>
    public double GetScalingMultiplier(int dungeonLevel)
    {
        if (dungeonLevel <= 1)
            return 1.0;

        int effectiveLevel = Math.Min(dungeonLevel, MAX_SCALING_LEVEL);
        return Math.Pow(STAT_SCALING_BASE, effectiveLevel - 1);
    }
}
