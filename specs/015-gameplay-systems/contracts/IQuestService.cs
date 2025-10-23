// Plugin Service Contract: Quest System
// Exposes quest management functionality via IPluginContext

using System;
using System.Collections.Generic;

namespace LablabBean.Plugins.Quest.Contracts
{
    /// <summary>
    /// Service for managing quests, objectives, and quest state.
    /// Registered in IPluginContext as "QuestService".
    /// </summary>
    public interface IQuestService
    {
        // Quest Management

        /// <summary>
        /// Attempts to start a quest for the player.
        /// Validates prerequisites before starting.
        /// </summary>
        /// <param name="playerId">Player entity ID</param>
        /// <param name="questId">Quest to start</param>
        /// <returns>True if quest started successfully, false if prerequisites not met or quest already active</returns>
        bool StartQuest(Guid playerId, Guid questId);

        /// <summary>
        /// Completes a quest and grants rewards.
        /// </summary>
        /// <param name="playerId">Player entity ID</param>
        /// <param name="questId">Quest to complete</param>
        /// <returns>True if quest completed successfully</returns>
        bool CompleteQuest(Guid playerId, Guid questId);

        /// <summary>
        /// Fails an active quest.
        /// </summary>
        void FailQuest(Guid playerId, Guid questId);

        /// <summary>
        /// Abandons an active quest (player choice).
        /// </summary>
        void AbandonQuest(Guid playerId, Guid questId);

        // Quest Progress

        /// <summary>
        /// Updates progress for kill objectives.
        /// Called by event system when enemy is killed.
        /// </summary>
        /// <param name="playerId">Player entity ID</param>
        /// <param name="enemyType">Type of enemy killed</param>
        void OnEnemyKilled(Guid playerId, string enemyType);

        /// <summary>
        /// Updates progress for collection objectives.
        /// Called by event system when item is collected.
        /// </summary>
        void OnItemCollected(Guid playerId, Guid itemId);

        /// <summary>
        /// Updates progress for location objectives.
        /// Called by event system when player moves.
        /// </summary>
        void OnLocationReached(Guid playerId, int level, int x, int y);

        /// <summary>
        /// Updates progress for NPC interaction objectives.
        /// Called by event system when dialogue ends.
        /// </summary>
        void OnNPCTalkedTo(Guid playerId, Guid npcId);

        // Quest Queries

        /// <summary>
        /// Gets all active quests for the player.
        /// </summary>
        IEnumerable<QuestInfo> GetActiveQuests(Guid playerId);

        /// <summary>
        /// Gets all completed quests for the player.
        /// </summary>
        IEnumerable<QuestInfo> GetCompletedQuests(Guid playerId);

        /// <summary>
        /// Checks if all objectives for a quest are complete.
        /// </summary>
        bool AreObjectivesComplete(Guid playerId, Guid questId);

        /// <summary>
        /// Checks if player meets prerequisites for a quest.
        /// </summary>
        bool MeetsPrerequisites(Guid playerId, Guid questId);

        /// <summary>
        /// Gets available quests (not started, prerequisites met).
        /// </summary>
        IEnumerable<QuestInfo> GetAvailableQuests(Guid playerId);
    }

    /// <summary>
    /// DTO for quest information (returned by service queries).
    /// </summary>
    public record QuestInfo(
        Guid Id,
        string Name,
        string Description,
        QuestState State,
        List<ObjectiveInfo> Objectives,
        QuestRewardsInfo Rewards
    );

    public record ObjectiveInfo(
        string Description,
        int Current,
        int Required,
        bool IsCompleted
    );

    public record QuestRewardsInfo(
        int ExperiencePoints,
        int Gold,
        List<Guid> Items
    );

    public enum QuestState
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }
}
