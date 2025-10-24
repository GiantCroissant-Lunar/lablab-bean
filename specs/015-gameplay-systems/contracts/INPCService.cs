// Plugin Service Contract: NPC & Dialogue System
// Exposes NPC interaction and dialogue management via IPluginContext

using System;
using System.Collections.Generic;

namespace LablabBean.Plugins.NPC.Contracts
{
    /// <summary>
    /// Service for managing NPCs and dialogue interactions.
    /// Registered in IPluginContext as "NPCService".
    /// </summary>
    public interface INPCService
    {
        // Dialogue Management

        /// <summary>
        /// Starts a dialogue with an NPC.
        /// </summary>
        /// <param name="playerId">Player entity ID</param>
        /// <param name="npcId">NPC entity ID</param>
        /// <returns>Initial dialogue node</returns>
        DialogueNodeInfo StartDialogue(Guid playerId, Guid npcId);

        /// <summary>
        /// Selects a dialogue choice and advances to the next node.
        /// </summary>
        /// <param name="playerId">Player entity ID</param>
        /// <param name="choiceIndex">Index of choice selected</param>
        /// <returns>Next dialogue node, or null if dialogue ended</returns>
        DialogueNodeInfo? SelectChoice(Guid playerId, int choiceIndex);

        /// <summary>
        /// Ends the current dialogue session.
        /// </summary>
        void EndDialogue(Guid playerId);

        /// <summary>
        /// Gets the current active dialogue state.
        /// </summary>
        /// <returns>Dialogue state, or null if no active dialogue</returns>
        DialogueStateInfo? GetDialogueState(Guid playerId);

        // NPC Queries

        /// <summary>
        /// Gets information about an NPC.
        /// </summary>
        NPCInfo GetNPCInfo(Guid npcId);

        /// <summary>
        /// Checks if player can interact with NPC (adjacent, not hostile, etc.).
        /// </summary>
        bool CanInteract(Guid playerId, Guid npcId);

        /// <summary>
        /// Gets all NPCs on the current level.
        /// </summary>
        IEnumerable<NPCInfo> GetNPCsOnLevel(int dungeonLevel);

        // NPC State

        /// <summary>
        /// Sets a persistent state value for an NPC.
        /// Used by dialogue actions to track NPC memory.
        /// </summary>
        void SetNPCState(Guid npcId, string key, object value);

        /// <summary>
        /// Gets a persistent state value from an NPC.
        /// </summary>
        T? GetNPCState<T>(Guid npcId, string key);

        /// <summary>
        /// Checks if NPC has a specific state flag.
        /// </summary>
        bool HasNPCState(Guid npcId, string key);
    }

    /// <summary>
    /// DTO for NPC information.
    /// </summary>
    public record NPCInfo(
        Guid Id,
        string Name,
        NPCType Type,
        bool IsHostile,
        int X,
        int Y
    );

    /// <summary>
    /// DTO for dialogue node presentation.
    /// </summary>
    public record DialogueNodeInfo(
        Guid NodeId,
        string NPCText,
        List<DialogueChoiceInfo> Choices,
        bool IsTerminal
    );

    /// <summary>
    /// DTO for dialogue choice presentation.
    /// </summary>
    public record DialogueChoiceInfo(
        int Index,
        string PlayerText,
        bool IsAvailable  // False if condition not met
    );

    /// <summary>
    /// DTO for active dialogue state.
    /// </summary>
    public record DialogueStateInfo(
        Guid NPCId,
        string NPCName,
        Guid CurrentNodeId
    );

    public enum NPCType
    {
        QuestGiver,
        Merchant,
        Lore,
        Friendly,
        Neutral,
        Hostile
    }
}
