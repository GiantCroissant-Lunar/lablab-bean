// ProgressionService: Public API for experience and leveling
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Progression.Components;
using LablabBean.Plugins.Progression.Contracts;
using LablabBean.Plugins.Progression.Systems;

namespace LablabBean.Plugins.Progression.Services;

/// <summary>
/// Service for managing player experience and leveling.
/// Registered in IPluginContext as "ProgressionService".
/// </summary>
public class ProgressionService : IProgressionService
{
    private readonly World _world;
    private readonly ExperienceSystem _experienceSystem;
    private readonly LevelingSystem _levelingSystem;

    public ProgressionService(
        World world,
        ExperienceSystem experienceSystem,
        LevelingSystem levelingSystem)
    {
        _world = world;
        _experienceSystem = experienceSystem;
        _levelingSystem = levelingSystem;
    }

        /// <summary>
    /// Awards experience points to the player.
    /// Automatically handles level-up and stat application.
    /// </summary>
    public bool AwardExperience(Guid playerId, int amount)
    {
        var entity = FindEntity(playerId);
        if (!entity.IsAlive())
        {
            return false;
        }

        if (amount <= 0)
        {
            return false;
        }

        // Award XP (handles level-up detection internally)
        int levelBefore = GetLevel(playerId);
        bool leveledUp = _experienceSystem.AwardXP(entity, amount);

        // Apply stat bonuses if leveled up
        if (leveledUp)
        {
            int levelAfter = GetLevel(playerId);

            // Apply stat bonuses for the new level
            _levelingSystem.ApplyLevelUpBonuses(entity, levelAfter);

            // Optional: Restore health/mana on level-up
            _levelingSystem.RestoreOnLevelUp(entity);
        }

        return leveledUp;
    }

        /// <summary>
        /// Gets current experience information.
        /// </summary>
        public ExperienceInfo GetExperience(Guid playerId)
        {
            var entity = FindEntity(playerId);
            if (!entity.IsAlive() || !entity.Has<Experience>())
            {
                return new ExperienceInfo(0, 0, 0, 0, 0f);
            }

            var (currentXP, level, xpToNext, totalXP) = _experienceSystem.GetExperienceInfo(entity);
            float progress = xpToNext > 0 ? (float)currentXP / xpToNext : 0f;

            return new ExperienceInfo(currentXP, level, xpToNext, totalXP, progress);
        }

        /// <summary>
    /// Manually triggers a level-up (for testing/admin).
    /// </summary>
    public void LevelUp(Guid playerId)
    {
        var entity = FindEntity(playerId);
        if (!entity.IsAlive())
        {
            return;
        }

        int levelBefore = GetLevel(playerId);
        bool success = _experienceSystem.ForceLevelUp(entity);

        if (success)
        {
            int levelAfter = GetLevel(playerId);
            _levelingSystem.ApplyLevelUpBonuses(entity, levelAfter);
            _levelingSystem.RestoreOnLevelUp(entity);
        }
    }

        /// <summary>
        /// Calculates XP required to reach a specific level.
        /// </summary>
        public int CalculateXPRequired(int level)
        {
            return _experienceSystem.CalculateXPRequired(level);
        }

        /// <summary>
        /// Gets stat increases for a specific level.
        /// </summary>
        public LevelUpStatsInfo GetLevelUpStats(int level)
        {
            var stats = _levelingSystem.GetLevelUpStats(level);

            return new LevelUpStatsInfo(
                stats.HealthBonus,
                stats.AttackBonus,
                stats.DefenseBonus,
                stats.ManaBonus,
                stats.SpeedBonus
            );
        }

        /// <summary>
        /// Gets current player level.
        /// </summary>
        public int GetLevel(Guid playerId)
        {
            var entity = FindEntity(playerId);
            if (!entity.IsAlive() || !entity.Has<Experience>())
            {
                return 0;
            }

            return entity.Get<Experience>().Level;
        }

        /// <summary>
        /// Checks if player meets level requirement.
        /// </summary>
        public bool MeetsLevelRequirement(Guid playerId, int requiredLevel)
        {
            return GetLevel(playerId) >= requiredLevel;
        }

        /// <summary>
        /// Gets total XP earned (lifetime statistic).
        /// </summary>
        public int GetTotalXPGained(Guid playerId)
        {
            var entity = FindEntity(playerId);
            if (!entity.IsAlive() || !entity.Has<Experience>())
            {
                return 0;
            }

            return entity.Get<Experience>().TotalXPGained;
        }

        /// <summary>
    /// Finds entity by GUID.
    /// NOTE: This is a placeholder until proper entity ID system is implemented.
    /// </summary>
    private Entity FindEntity(Guid playerId)
    {
        // TODO: Implement proper entity lookup by GUID
        // For now, use a simple query to find the player entity

        var query = new QueryDescription().WithAll<Experience>();
        var entities = new List<Entity>();

        _world.Query(in query, (Entity entity) =>
        {
            entities.Add(entity);
        });

        // Return first entity with Experience (assumes single player)
        return entities.FirstOrDefault();
    }
}
