// Plugin Service Contract: Character Progression System
// Exposes experience, leveling, and stat progression via IPluginContext

using System;

namespace LablabBean.Plugins.Progression.Contracts
{
    /// <summary>
    /// Service for managing player experience and leveling.
    /// Registered in IPluginContext as "ProgressionService".
    /// </summary>
    public interface IProgressionService
    {
        // Experience Management

        /// <summary>
        /// Awards experience points to the player.
        /// Automatically handles level-up if threshold reached.
        /// </summary>
        /// <param name="playerId">Player entity ID</param>
        /// <param name="amount">XP to award</param>
        /// <returns>True if player leveled up</returns>
        bool AwardExperience(Guid playerId, int amount);

        /// <summary>
        /// Gets current experience information.
        /// </summary>
        ExperienceInfo GetExperience(Guid playerId);

        // Leveling

        /// <summary>
        /// Manually triggers a level-up (used for testing/admin).
        /// </summary>
        void LevelUp(Guid playerId);

        /// <summary>
        /// Calculates XP required to reach a specific level.
        /// Formula: BaseXP * (level^1.8)
        /// </summary>
        int CalculateXPRequired(int level);

        /// <summary>
        /// Gets stat increases for a specific level.
        /// </summary>
        LevelUpStatsInfo GetLevelUpStats(int level);

        // Queries

        /// <summary>
        /// Gets current player level.
        /// </summary>
        int GetLevel(Guid playerId);

        /// <summary>
        /// Checks if player meets level requirement.
        /// </summary>
        bool MeetsLevelRequirement(Guid playerId, int requiredLevel);

        /// <summary>
        /// Gets total XP earned (lifetime statistic).
        /// </summary>
        int GetTotalXPGained(Guid playerId);
    }

    /// <summary>
    /// DTO for experience information.
    /// </summary>
    public record ExperienceInfo(
        int CurrentXP,
        int Level,
        int XPToNextLevel,
        int TotalXPGained,
        float ProgressToNextLevel  // 0.0 to 1.0
    );

    /// <summary>
    /// DTO for level-up stat increases.
    /// </summary>
    public record LevelUpStatsInfo(
        int HealthBonus,
        int AttackBonus,
        int DefenseBonus,
        int ManaBonus,
        int SpeedBonus
    );
}
