using BossComponent = LablabBean.Plugins.Boss.Components.Boss;
using LablabBean.Plugins.Boss.Components;

namespace LablabBean.Plugins.Boss.Services;

/// <summary>
/// Service for managing boss encounters and mechanics.
/// </summary>
public interface IBossService
{
    /// <summary>
    /// Creates a boss entity with all components configured.
    /// </summary>
    BossComponent CreateBoss(string bossId, int playerLevel);

    /// <summary>
    /// Gets all ability definitions for a boss.
    /// </summary>
    List<BossAbility> GetBossAbilities(string bossId);

    /// <summary>
    /// Triggers phase transition for a boss.
    /// </summary>
    void TriggerPhaseTransition(ref BossComponent boss, int newPhase);

    /// <summary>
    /// Checks if boss can use a specific ability.
    /// </summary>
    bool CanUseAbility(ref BossComponent boss, string abilityId);

    /// <summary>
    /// Gets the next best ability for boss to use.
    /// </summary>
    BossAbility? SelectNextAbility(ref BossComponent boss, float currentHealthPercent);

    /// <summary>
    /// Updates ability cooldowns.
    /// </summary>
    void UpdateCooldowns(ref BossComponent boss, float deltaTime);

    /// <summary>
    /// Checks and updates enrage state.
    /// </summary>
    void CheckEnrage(ref BossComponent boss, float currentHealthPercent);

    /// <summary>
    /// Gets loot table for a boss.
    /// </summary>
    BossLoot GetBossLoot(string bossId);

    /// <summary>
    /// Scales boss stats to player level.
    /// </summary>
    void ScaleBossToLevel(ref BossComponent boss, int playerLevel);
}
