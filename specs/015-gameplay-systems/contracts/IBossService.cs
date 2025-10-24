// Plugin Service Contract: Boss Encounter System
// Exposes boss spawning, phase management, and special abilities via IPluginContext

using System;
using System.Collections.Generic;

namespace LablabBean.Plugins.Boss.Contracts
{
    /// <summary>
    /// Service for managing boss encounters and special abilities.
    /// Registered in IPluginContext as "BossService".
    /// </summary>
    public interface IBossService
    {
        // Boss Management

        /// <summary>
        /// Spawns a boss on a specific dungeon level.
        /// </summary>
        /// <param name="bossId">Boss template ID</param>
        /// <param name="dungeonLevel">Level to spawn on</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Entity ID of spawned boss</returns>
        Guid SpawnBoss(Guid bossId, int dungeonLevel, int x, int y);

        /// <summary>
        /// Checks if a boss should spawn on a level.
        /// Typically every 5 levels (5, 10, 15, 20).
        /// </summary>
        bool IsBossLevel(int dungeonLevel);

        /// <summary>
        /// Gets the appropriate boss for a dungeon level.
        /// </summary>
        Guid? GetBossForLevel(int dungeonLevel);

        /// <summary>
        /// Marks a boss as defeated (prevents respawn).
        /// </summary>
        void MarkBossDefeated(Guid bossId);

        /// <summary>
        /// Checks if a boss has been defeated in this session.
        /// </summary>
        bool IsBossDefeated(Guid bossId);

        // Phase Management

        /// <summary>
        /// Checks if boss should transition to next phase.
        /// Called after boss takes damage.
        /// </summary>
        void CheckPhaseTransition(Guid bossEntityId);

        /// <summary>
        /// Gets current phase for a boss entity.
        /// </summary>
        int GetCurrentPhase(Guid bossEntityId);

        /// <summary>
        /// Gets phase information.
        /// </summary>
        BossPhaseInfo GetPhaseInfo(Guid bossEntityId, int phaseIndex);

        // Abilities

        /// <summary>
        /// Attempts to use a boss ability.
        /// Checks cooldown and probability.
        /// </summary>
        /// <returns>True if ability was used</returns>
        bool TryUseAbility(Guid bossEntityId);

        /// <summary>
        /// Manually triggers a specific boss ability (for testing/scripting).
        /// </summary>
        void TriggerAbility(Guid bossEntityId, Guid abilityId, Guid? targetId = null);

        // Queries

        /// <summary>
        /// Gets boss information.
        /// </summary>
        BossInfo GetBossInfo(Guid bossEntityId);

        /// <summary>
        /// Gets all bosses on the current level.
        /// </summary>
        IEnumerable<BossInfo> GetBossesOnLevel(int dungeonLevel);

        /// <summary>
        /// Checks if an entity is a boss.
        /// </summary>
        bool IsBoss(Guid entityId);
    }

    /// <summary>
    /// DTO for boss information.
    /// </summary>
    public record BossInfo(
        Guid EntityId,
        string Name,
        int CurrentPhase,
        int TotalPhases,
        float HealthPercent
    );

    /// <summary>
    /// DTO for boss phase information.
    /// </summary>
    public record BossPhaseInfo(
        int PhaseNumber,
        float HealthThreshold,
        string BehaviorType,
        List<string> AbilityNames
    );
}
