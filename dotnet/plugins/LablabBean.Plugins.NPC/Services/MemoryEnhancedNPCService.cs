using System.Threading.Tasks;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Contracts.AI.Memory;
using LablabBean.Plugins.NPC.Data;
using LablabBean.Plugins.NPC.Systems;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.NPC.Services;

/// <summary>
/// Enhanced NPC Service with memory integration
/// Provides context-aware NPC interactions using semantic memory
/// </summary>
public class MemoryEnhancedNPCService
{
    private readonly World _world;
    private readonly NPCSystem _npcSystem;
    private readonly MemoryEnhancedDialogueSystem _dialogueSystem;
    private readonly ILogger<MemoryEnhancedNPCService> _logger;

    public MemoryEnhancedNPCService(
        World world,
        NPCSystem npcSystem,
        MemoryEnhancedDialogueSystem dialogueSystem,
        ILogger<MemoryEnhancedNPCService> logger)
    {
        _world = world;
        _npcSystem = npcSystem;
        _dialogueSystem = dialogueSystem;
        _logger = logger;
    }

    /// <summary>
    /// Loads a dialogue tree into the dialogue system
    /// </summary>
    public void LoadDialogueTree(DialogueTree tree)
    {
        _dialogueSystem.LoadDialogueTree(tree);
    }

    /// <summary>
    /// Starts dialogue with an NPC (memory-enhanced)
    /// </summary>
    public async Task<Entity?> StartDialogueAsync(
        Entity playerEntity,
        Entity npcEntity,
        string? playerId = null)
    {
        if (!playerEntity.IsAlive() || !npcEntity.IsAlive())
            return null;

        if (!_npcSystem.CanInteract(npcEntity, playerEntity))
            return null;

        var npc = npcEntity.Get<Components.NPC>();

        if (string.IsNullOrEmpty(npc.DialogueTreeId))
            return null;

        // Get memory context before starting dialogue
        if (playerId != null)
        {
            var memories = await _dialogueSystem.GetNpcMemoriesAsync(playerId, npc.Id, limit: 3);

            if (memories.Count > 0)
            {
                _logger.LogInformation(
                    "Player {PlayerId} has {Count} relevant memories with NPC {NpcName}",
                    playerId, memories.Count, npc.Name);
            }
        }

        return await _dialogueSystem.StartDialogueAsync(playerEntity, npcEntity, npc.DialogueTreeId, playerId);
    }

    /// <summary>
    /// Ends active dialogue for a player
    /// </summary>
    public void EndDialogue(Entity playerEntity)
    {
        var dialogueEntity = _dialogueSystem.GetActiveDialogue(playerEntity);

        if (dialogueEntity != null)
        {
            _dialogueSystem.EndDialogue(dialogueEntity.Value);
        }
    }

    /// <summary>
    /// Selects a dialogue choice (memory-enhanced)
    /// </summary>
    public async Task SelectChoiceAsync(
        Entity playerEntity,
        string choiceId,
        string? playerId = null)
    {
        var dialogueEntity = _dialogueSystem.GetActiveDialogue(playerEntity);

        if (dialogueEntity != null)
        {
            await _dialogueSystem.SelectChoiceAsync(dialogueEntity.Value, choiceId, playerId);
        }
    }

    /// <summary>
    /// Gets available choices for the current dialogue node
    /// </summary>
    public System.Collections.Generic.List<DialogueChoice> GetAvailableChoices(Entity playerEntity)
    {
        var dialogueEntity = _dialogueSystem.GetActiveDialogue(playerEntity);

        if (dialogueEntity != null)
        {
            return _dialogueSystem.GetAvailableChoices(dialogueEntity.Value);
        }

        return new System.Collections.Generic.List<DialogueChoice>();
    }

    /// <summary>
    /// Gets the current dialogue node
    /// </summary>
    public DialogueNode? GetCurrentDialogueNode(Entity playerEntity)
    {
        var dialogueEntity = _dialogueSystem.GetActiveDialogue(playerEntity);

        if (dialogueEntity != null)
        {
            return _dialogueSystem.GetCurrentNode(dialogueEntity.Value);
        }

        return null;
    }

    /// <summary>
    /// Gets relationship insights for a player and NPC
    /// </summary>
    public async Task<NpcRelationshipInsights> GetRelationshipInsightsAsync(
        string playerId,
        string npcId)
    {
        return await _dialogueSystem.AnalyzeRelationshipAsync(playerId, npcId);
    }

    /// <summary>
    /// Gets recent dialogue history for context
    /// </summary>
    public async Task<System.Collections.Generic.IReadOnlyList<MemoryResult>> GetRecentDialogueHistoryAsync(
        string playerId,
        int limit = 5)
    {
        return await _dialogueSystem.GetRecentDialogueMemoriesAsync(
            playerId,
            timeWindow: System.TimeSpan.FromHours(1),
            limit: limit);
    }
}
