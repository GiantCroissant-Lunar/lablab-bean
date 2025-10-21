// Contract: DifficultyScalingSystem
// Purpose: Defines the interface for difficulty scaling calculations
// Location: LablabBean.Game.Core/Systems/DifficultyScalingSystem.cs

namespace LablabBean.Game.Core.Systems;

using Arch.Core;
using LablabBean.Game.Core.Components;

/// <summary>
/// System responsible for calculating difficulty scaling for enemies and loot.
/// Applies exponential scaling based on dungeon level.
/// </summary>
public class DifficultyScalingSystem
{
    /// <summary>Scaling factor per level (12% increase)</summary>
    private const double ScalingFactor = 1.12;
    
    /// <summary>Base loot drop rate at level 1</summary>
    private const double BaseLootDropRate = 0.10;
    
    /// <summary>Loot drop rate increase per level</summary>
    private const double LootDropRatePerLevel = 0.05;
    
    /// <summary>Maximum loot drop rate (cap)</summary>
    private const double MaxLootDropRate = 0.60;
    
    /// <summary>Maximum level for scaling (prevents overflow)</summary>
    private const int MaxScalingLevel = 30;
    
    /// <summary>Feet per dungeon level for depth display</summary>
    private const int FeetPerLevel = 30;
    
    private readonly World _world;
    private readonly Random _random;

    public DifficultyScalingSystem(World world, Random? random = null)
    {
        _world = world;
        _random = random ?? new Random();
    }

    // ═══════════════════════════════════════════════════════════════
    // STAT SCALING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates scaled stat value for a given level.
    /// </summary>
    /// <param name="baseStat">Base stat value at level 1</param>
    /// <param name="level">Current dungeon level</param>
    /// <returns>Scaled stat value</returns>
    /// <remarks>
    /// Formula: scaledStat = baseStat × (1.12 ^ (level - 1))
    /// 
    /// Examples:
    /// - Level 1: 20 HP → 20 HP (1.00x)
    /// - Level 5: 20 HP → 32 HP (1.57x)
    /// - Level 10: 20 HP → 52 HP (2.77x)
    /// - Level 20: 20 HP → 165 HP (8.61x)
    /// - Level 30+: Capped at level 30 multiplier
    /// </remarks>
    public int CalculateScaledStat(int baseStat, int level);

    /// <summary>
    /// Calculates scaling multiplier for a given level.
    /// </summary>
    /// <param name="level">Current dungeon level</param>
    /// <returns>Scaling multiplier (1.0 at level 1, increases exponentially)</returns>
    public double CalculateScalingMultiplier(int level);

    /// <summary>
    /// Applies difficulty scaling to an enemy entity.
    /// </summary>
    /// <param name="enemyEntity">Enemy entity to scale</param>
    /// <param name="level">Current dungeon level</param>
    /// <remarks>
    /// Scales the following components:
    /// - Health: Current and Maximum
    /// - Combat: Attack and Defense
    /// 
    /// Preserves:
    /// - Enemy type and behavior
    /// - Position
    /// - Other non-stat components
    /// </remarks>
    public void ApplyEnemyScaling(Entity enemyEntity, int level);

    /// <summary>
    /// Calculates scaled health for an enemy.
    /// </summary>
    /// <param name="baseHealth">Base health at level 1</param>
    /// <param name="level">Current dungeon level</param>
    /// <returns>Scaled health value</returns>
    public int CalculateScaledHealth(int baseHealth, int level);

    /// <summary>
    /// Calculates scaled attack for an enemy.
    /// </summary>
    /// <param name="baseAttack">Base attack at level 1</param>
    /// <param name="level">Current dungeon level</param>
    /// <returns>Scaled attack value</returns>
    public int CalculateScaledAttack(int baseAttack, int level);

    /// <summary>
    /// Calculates scaled defense for an enemy.
    /// </summary>
    /// <param name="baseDefense">Base defense at level 1</param>
    /// <param name="level">Current dungeon level</param>
    /// <returns>Scaled defense value</returns>
    public int CalculateScaledDefense(int baseDefense, int level);

    // ═══════════════════════════════════════════════════════════════
    // LOOT SCALING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates loot drop rate for a given level.
    /// </summary>
    /// <param name="level">Current dungeon level</param>
    /// <returns>Drop rate (0.0 to 1.0)</returns>
    /// <remarks>
    /// Formula: dropRate = min(0.10 + (level × 0.05), 0.60)
    /// 
    /// Examples:
    /// - Level 1: 10% drop rate
    /// - Level 5: 30% drop rate
    /// - Level 10: 60% drop rate (capped)
    /// - Level 20: 60% drop rate (capped)
    /// </remarks>
    public double CalculateLootDropRate(int level);

    /// <summary>
    /// Determines if loot should drop based on level scaling.
    /// </summary>
    /// <param name="level">Current dungeon level</param>
    /// <returns>True if loot should drop</returns>
    public bool ShouldDropLoot(int level);

    /// <summary>
    /// Determines if equipment should drop (vs consumables).
    /// </summary>
    /// <param name="level">Current dungeon level</param>
    /// <returns>True if equipment should drop</returns>
    /// <remarks>
    /// Equipment drop rate scales with level.
    /// Consumables drop more frequently at all levels.
    /// </remarks>
    public bool ShouldDropEquipment(int level);

    /// <summary>
    /// Calculates number of items to drop from an enemy.
    /// </summary>
    /// <param name="level">Current dungeon level</param>
    /// <returns>Number of items to drop (0-3)</returns>
    /// <remarks>
    /// Higher levels have chance for multiple item drops.
    /// - Level 1-5: 0-1 items
    /// - Level 6-15: 0-2 items
    /// - Level 16+: 0-3 items
    /// </remarks>
    public int CalculateLootCount(int level);

    // ═══════════════════════════════════════════════════════════════
    // DEPTH DISPLAY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates depth in feet for a given level.
    /// </summary>
    /// <param name="level">Level number</param>
    /// <returns>Depth in feet (30 feet per level)</returns>
    /// <remarks>
    /// Examples:
    /// - Level 1: -30 ft
    /// - Level 5: -150 ft
    /// - Level 10: -300 ft
    /// - Level 20: -600 ft
    /// </remarks>
    public int CalculateDepthInFeet(int level);

    /// <summary>
    /// Gets formatted depth string for HUD display.
    /// </summary>
    /// <param name="level">Level number</param>
    /// <returns>Formatted string (e.g., "Depth: -150 ft")</returns>
    public string GetDepthDisplayString(int level);

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION & UTILITIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a level is within the scaling cap.
    /// </summary>
    /// <param name="level">Level number to check</param>
    /// <returns>True if level is at or below scaling cap</returns>
    public bool IsWithinScalingCap(int level);

    /// <summary>
    /// Gets the effective level for scaling calculations.
    /// </summary>
    /// <param name="level">Actual level number</param>
    /// <returns>Effective level (capped at MaxScalingLevel)</returns>
    public int GetEffectiveLevel(int level);

    /// <summary>
    /// Gets scaling statistics for a given level.
    /// </summary>
    /// <param name="level">Level number</param>
    /// <returns>Scaling statistics</returns>
    public ScalingStats GetScalingStats(int level);
}

/// <summary>
/// Scaling statistics for a dungeon level.
/// </summary>
public struct ScalingStats
{
    /// <summary>Level number</summary>
    public int Level { get; set; }
    
    /// <summary>Scaling multiplier (e.g., 1.57x for level 5)</summary>
    public double Multiplier { get; set; }
    
    /// <summary>Loot drop rate (0.0 to 1.0)</summary>
    public double LootDropRate { get; set; }
    
    /// <summary>Depth in feet</summary>
    public int DepthInFeet { get; set; }
    
    /// <summary>Example enemy health (20 HP base)</summary>
    public int ExampleHealth { get; set; }
    
    /// <summary>Example enemy attack (5 ATK base)</summary>
    public int ExampleAttack { get; set; }
    
    /// <summary>Example enemy defense (2 DEF base)</summary>
    public int ExampleDefense { get; set; }
}

/// <summary>
/// Predefined scaling reference table.
/// </summary>
public static class ScalingReference
{
    /// <summary>
    /// Gets scaling statistics for common levels.
    /// </summary>
    public static readonly Dictionary<int, ScalingStats> ReferenceTable = new()
    {
        { 1, new ScalingStats { Level = 1, Multiplier = 1.00, LootDropRate = 0.10, DepthInFeet = -30, ExampleHealth = 20, ExampleAttack = 5, ExampleDefense = 2 } },
        { 5, new ScalingStats { Level = 5, Multiplier = 1.57, LootDropRate = 0.30, DepthInFeet = -150, ExampleHealth = 32, ExampleAttack = 9, ExampleDefense = 3 } },
        { 10, new ScalingStats { Level = 10, Multiplier = 2.77, LootDropRate = 0.60, DepthInFeet = -300, ExampleHealth = 52, ExampleAttack = 14, ExampleDefense = 5 } },
        { 15, new ScalingStats { Level = 15, Multiplier = 4.89, LootDropRate = 0.60, DepthInFeet = -450, ExampleHealth = 88, ExampleAttack = 24, ExampleDefense = 9 } },
        { 20, new ScalingStats { Level = 20, Multiplier = 8.61, LootDropRate = 0.60, DepthInFeet = -600, ExampleHealth = 165, ExampleAttack = 45, ExampleDefense = 17 } },
        { 30, new ScalingStats { Level = 30, Multiplier = 28.75, LootDropRate = 0.60, DepthInFeet = -900, ExampleHealth = 537, ExampleAttack = 146, ExampleDefense = 55 } }
    };
}
