// Plugin Service Contract: Environmental Hazards & Traps System
// Exposes trap detection, disarming, and environmental hazards via IPluginContext

using System;
using System.Collections.Generic;

namespace LablabBean.Plugins.Hazards.Contracts
{
    /// <summary>
    /// Service for managing traps and environmental hazards.
    /// Registered in IPluginContext as "HazardService".
    /// </summary>
    public interface IHazardService
    {
        // Trap Management

        /// <summary>
        /// Places a trap at a specific location.
        /// </summary>
        /// <param name="trapType">Type of trap to place</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="detectionDifficulty">Perception DC to detect</param>
        /// <param name="disarmDifficulty">Skill check DC to disarm</param>
        /// <returns>Entity ID of placed trap</returns>
        Guid PlaceTrap(TrapType trapType, int x, int y, int detectionDifficulty, int disarmDifficulty);

        /// <summary>
        /// Attempts to detect a trap.
        /// Uses player's Perception stat.
        /// </summary>
        /// <param name="playerId">Player entity ID</param>
        /// <param name="trapId">Trap entity ID</param>
        /// <returns>True if trap detected</returns>
        bool DetectTrap(Guid playerId, Guid trapId);

        /// <summary>
        /// Attempts to disarm a detected trap.
        /// </summary>
        /// <param name="playerId">Player entity ID</param>
        /// <param name="trapId">Trap entity ID</param>
        /// <returns>True if disarm successful, false if trap triggered</returns>
        bool DisarmTrap(Guid playerId, Guid trapId);

        /// <summary>
        /// Triggers a trap.
        /// Applies damage/effects to entity on tile.
        /// </summary>
        void TriggerTrap(Guid trapId, Guid victimId);

        // Detection

        /// <summary>
        /// Checks for traps in player's perception radius.
        /// Called on player movement.
        /// </summary>
        /// <returns>List of detected trap entity IDs</returns>
        List<Guid> CheckForTraps(Guid playerId, int radius = 1);

        /// <summary>
        /// Gets trap at specific coordinates.
        /// </summary>
        Guid? GetTrapAtPosition(int x, int y);

        // Environmental Hazards

        /// <summary>
        /// Places an environmental hazard on a tile.
        /// </summary>
        /// <param name="hazardType">Type of hazard</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Entity ID of hazard</returns>
        Guid PlaceHazard(HazardType hazardType, int x, int y);

        /// <summary>
        /// Applies damage from hazard to entity standing on tile.
        /// Called each turn.
        /// </summary>
        void ApplyHazardDamage(Guid entityId, Guid hazardId);

        /// <summary>
        /// Gets hazard at specific coordinates.
        /// </summary>
        Guid? GetHazardAtPosition(int x, int y);

        /// <summary>
        /// Checks if a tile has a movement-blocking hazard.
        /// </summary>
        bool IsHazardBlocking(int x, int y);

        // Queries

        /// <summary>
        /// Gets trap information.
        /// </summary>
        TrapInfo GetTrapInfo(Guid trapId);

        /// <summary>
        /// Gets hazard information.
        /// </summary>
        HazardInfo GetHazardInfo(Guid hazardId);

        /// <summary>
        /// Gets all traps on the current level.
        /// </summary>
        IEnumerable<TrapInfo> GetTrapsOnLevel(int dungeonLevel);

        /// <summary>
        /// Gets all hazards on the current level.
        /// </summary>
        IEnumerable<HazardInfo> GetHazardsOnLevel(int dungeonLevel);
    }

    /// <summary>
    /// DTO for trap information.
    /// </summary>
    public record TrapInfo(
        Guid EntityId,
        TrapType Type,
        int X,
        int Y,
        TrapState State,
        int DetectionDifficulty,
        int DisarmDifficulty
    );

    /// <summary>
    /// DTO for environmental hazard information.
    /// </summary>
    public record HazardInfo(
        Guid EntityId,
        HazardType Type,
        int X,
        int Y,
        int DamagePerTurn,
        bool BlocksMovement
    );

    public enum TrapType
    {
        Spike,
        PoisonGas,
        Fire,
        Arrow,
        Net,
        Alarm
    }

    public enum TrapState
    {
        Hidden,
        Detected,
        Triggered,
        Disarmed
    }

    public enum HazardType
    {
        Lava,
        Pit,
        CollapsingFloor,
        PoisonGas
    }
}
