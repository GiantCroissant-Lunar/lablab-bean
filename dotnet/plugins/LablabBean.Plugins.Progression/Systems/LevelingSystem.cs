// LevelingSystem: Applies stat increases on level-up
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Progression.Components;
using LablabBean.Game.Core.Components;

namespace LablabBean.Plugins.Progression.Systems;

/// <summary>
/// Listens for level-up events and applies stat increases to entities.
/// Works in conjunction with ExperienceSystem.
/// </summary>
public class LevelingSystem
{
    private readonly World _world;

    public LevelingSystem(World world)
    {
        _world = world;
    }

        /// <summary>
    /// Applies stat bonuses to an entity on level-up.
    /// Called by ExperienceSystem or manually.
    /// </summary>
    public void ApplyLevelUpBonuses(Entity entity, int newLevel)
    {
        if (!entity.Has<Experience>())
        {
            return;
        }

        // Get stat bonuses for this level
        var bonuses = GetLevelUpStats(newLevel);

        // Apply health bonus
        if (entity.Has<Health>() && bonuses.HealthBonus > 0)
        {
            ref var health = ref entity.Get<Health>();
            health.Maximum += bonuses.HealthBonus;
            health.Current += bonuses.HealthBonus; // Restore health on level-up
        }

        // Apply attack/defense bonuses
        if (entity.Has<Combat>())
        {
            ref var combat = ref entity.Get<Combat>();

            if (bonuses.AttackBonus > 0)
            {
                combat.Attack += bonuses.AttackBonus;
            }

            if (bonuses.DefenseBonus > 0)
            {
                combat.Defense += bonuses.DefenseBonus;
            }
        }

        // TODO: Apply mana/speed bonuses when those components exist
        // if (entity.Has<Mana>() && bonuses.ManaBonus > 0) { ... }

        // Apply speed bonus (Actor.Speed)
        if (entity.Has<Actor>() && bonuses.SpeedBonus > 0)
        {
            ref var actor = ref entity.Get<Actor>();
            actor.Speed += bonuses.SpeedBonus;
        }
    }

        /// <summary>
    /// Gets stat bonuses for a specific level.
    /// Uses scaled bonuses that increase slightly every 10 levels.
    /// </summary>
    public LevelUpStats GetLevelUpStats(int level)
    {
        // Use scaled stats for better progression feel
        return LevelUpStats.CreateScaled(level);
    }

    /// <summary>
    /// Restores entity to full health/mana on level-up.
    /// Common RPG mechanic.
    /// </summary>
    public void RestoreOnLevelUp(Entity entity)
    {
        // Restore health
        if (entity.Has<Health>())
        {
            ref var health = ref entity.Get<Health>();
            health.Current = health.Maximum;
        }

        // TODO: Restore mana when component exists
        // if (entity.Has<Mana>()) { ... }
    }
}
