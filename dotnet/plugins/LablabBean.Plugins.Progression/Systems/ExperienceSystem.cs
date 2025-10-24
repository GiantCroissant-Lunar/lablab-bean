// ExperienceSystem: Tracks XP accumulation and detects level-up thresholds
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Progression.Components;
using LablabBean.Game.Core.Events;

namespace LablabBean.Plugins.Progression.Systems;

/// <summary>
/// Processes experience accumulation and detects when level-up is ready.
/// Use ProgressionService.AwardExperience() to add XP.
/// </summary>
public class ExperienceSystem
{
    private readonly World _world;

    // XP formula configuration
    private const int BaseXP = 100;
    private const float XPScaling = 1.8f;
    private const int MaxLevel = 50;

    public ExperienceSystem(World world)
    {
        _world = world;
    }

        /// <summary>
    /// Awards XP to an entity and handles level-up cascading.
    /// Returns true if at least one level-up occurred.
    /// </summary>
    public bool AwardXP(Entity entity, int xpAmount)
    {
        if (!entity.Has<Experience>())
        {
            return false;
        }

        if (xpAmount <= 0)
        {
            return false;
        }

        ref var exp = ref entity.Get<Experience>();

        // Check for max level
        if (exp.Level >= MaxLevel)
        {
            return false;
        }

        int overflow = exp.AddXP(xpAmount);
        bool leveledUp = false;

        // Handle cascading level-ups if overflow XP causes multiple levels
        while (exp.IsLevelUpReady() && exp.Level < MaxLevel)
        {
            int oldLevel = exp.Level;
            int nextLevelXP = CalculateXPRequired(exp.Level + 1);

            overflow = exp.LevelUp(nextLevelXP);
            leveledUp = true;

            // Publish level-up event
            PublishLevelUpEvent(entity, exp.Level, oldLevel);

            // Apply overflow XP
            if (overflow > 0 && exp.Level < MaxLevel)
            {
                exp.AddXP(overflow);
            }
        }

        return leveledUp;
    }

        /// <summary>
        /// Calculates XP required to reach a specific level.
        /// Formula: BaseXP * (level ^ XPScaling)
        /// Example: Level 2 = 100 * (2^1.8) ≈ 348 XP
        /// </summary>
        public int CalculateXPRequired(int level)
        {
            if (level <= 1) return 0;
            if (level > MaxLevel) return int.MaxValue;

            return (int)(BaseXP * Math.Pow(level, XPScaling));
        }

        /// <summary>
    /// Manually triggers a level-up (for testing/admin commands).
    /// </summary>
    public bool ForceLevelUp(Entity entity)
    {
        if (!entity.Has<Experience>())
        {
            return false;
        }

        ref var exp = ref entity.Get<Experience>();

        if (exp.Level >= MaxLevel)
        {
            return false;
        }

        int oldLevel = exp.Level;
        int nextLevelXP = CalculateXPRequired(exp.Level + 1);

        exp.LevelUp(nextLevelXP);

        PublishLevelUpEvent(entity, exp.Level, oldLevel);

        return true;
    }

    /// <summary>
    /// Publishes level-up event to event bus.
    /// </summary>
    private void PublishLevelUpEvent(Entity entity, int newLevel, int oldLevel)
    {
        // TODO: Integrate with actual event bus when available
        // Placeholder: var evt = new PlayerLevelUpEvent(entity.Id, newLevel, oldLevel);
        // EventBus.Publish(evt);
    }

    /// <summary>
    /// Gets current XP info for an entity.
    /// </summary>
    public (int currentXP, int level, int xpToNext, int totalXP) GetExperienceInfo(Entity entity)
    {
        if (!entity.Has<Experience>())
        {
            return (0, 0, 0, 0);
        }

        var exp = entity.Get<Experience>();
        return (exp.CurrentXP, exp.Level, exp.XPToNextLevel, exp.TotalXPGained);
    }
}
