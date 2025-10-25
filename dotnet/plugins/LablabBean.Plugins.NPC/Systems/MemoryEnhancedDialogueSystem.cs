using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Contracts.AI.Memory;
using LablabBean.Plugins.NPC.Components;
using LablabBean.Plugins.NPC.Data;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.NPC.Systems;

/// <summary>
/// Enhanced DialogueSystem with semantic memory integration for context-aware NPCs
/// Phase 4: Dual-write pattern - stores dialogue memories alongside regular dialogue processing
/// </summary>
public class MemoryEnhancedDialogueSystem
{
    private readonly DialogueSystem _baseDialogueSystem;
    private readonly IMemoryService _memoryService;
    private readonly ILogger<MemoryEnhancedDialogueSystem> _logger;
    private readonly World _world;

    public MemoryEnhancedDialogueSystem(
        DialogueSystem baseDialogueSystem,
        IMemoryService memoryService,
        ILogger<MemoryEnhancedDialogueSystem> logger,
        World world)
    {
        _baseDialogueSystem = baseDialogueSystem;
        _memoryService = memoryService;
        _logger = logger;
        _world = world;
    }

    /// <summary>
    /// Starts dialogue with memory context retrieval
    /// </summary>
    public async Task<Entity?> StartDialogueAsync(
        Entity playerEntity,
        Entity npcEntity,
        string dialogueTreeId,
        string? playerId = null)
    {
        // Start regular dialogue
        var dialogueEntity = _baseDialogueSystem.StartDialogue(playerEntity, npcEntity, dialogueTreeId);

        if (dialogueEntity == null)
            return null;

        // Fire-and-forget: Store memory of dialogue start
        _ = Task.Run(async () =>
        {
            try
            {
                var npc = npcEntity.Get<Components.NPC>();
                var effectivePlayerId = playerId ?? playerEntity.Id.ToString();

                await _memoryService.StoreMemoryAsync(new MemoryEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = $"Started conversation with {npc.Name} (ID: {npc.Id})",
                    EntityId = effectivePlayerId,
                    MemoryType = "conversation_start",
                    Importance = 0.5,
                    Tags = new Dictionary<string, string>
                    {
                        { "npc_id", npc.Id },
                        { "npc_name", npc.Name },
                        { "dialogue_tree", dialogueTreeId },
                        { "event_type", "dialogue_start" }
                    }
                });

                _logger.LogDebug("Stored dialogue start memory for player {PlayerId} with NPC {NpcId}",
                    effectivePlayerId, npc.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store dialogue start memory");
            }
        });

        return dialogueEntity;
    }

    /// <summary>
    /// Selects a dialogue choice with memory storage
    /// </summary>
    public async Task SelectChoiceAsync(
        Entity dialogueEntity,
        string choiceId,
        string? playerId = null)
    {
        if (!dialogueEntity.IsAlive() || !dialogueEntity.Has<DialogueState>())
            return;

        ref var dialogueState = ref dialogueEntity.Get<DialogueState>();
        var tree = _baseDialogueSystem.GetDialogueTree(dialogueState.DialogueTreeId);

        if (tree == null)
            return;

        var currentNode = tree.GetNode(dialogueState.CurrentNodeId);
        var choice = currentNode?.Choices.FirstOrDefault(c => c.Id == choiceId);

        if (choice == null)
            return;

        var playerEntityRef = FindEntityById(dialogueState.PlayerEntityId);
        var npcEntityRef = FindEntityById(dialogueState.NPCEntityId);

        if (playerEntityRef == null || npcEntityRef == null)
            return;

        var npc = npcEntityRef.Value.Get<Components.NPC>();
        var effectivePlayerId = playerId ?? playerEntityRef.Value.Id.ToString();

        // Execute base choice logic
        _baseDialogueSystem.SelectChoice(dialogueEntity, choiceId);

        // Fire-and-forget: Store memory of choice
        _ = Task.Run(async () =>
        {
            try
            {
                var importance = CalculateChoiceImportance(choice, currentNode);

                await _memoryService.StoreMemoryAsync(new MemoryEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = $"Chose '{choice.Text}' when talking to {npc.Name}: {currentNode?.Text}",
                    EntityId = effectivePlayerId,
                    MemoryType = "dialogue_choice",
                    Importance = importance,
                    Tags = new Dictionary<string, string>
                    {
                        { "npc_id", npc.Id },
                        { "npc_name", npc.Name },
                        { "choice_id", choiceId },
                        { "choice_text", choice.Text },
                        { "node_id", currentNode?.Id ?? "" },
                        { "event_type", "dialogue_choice" },
                        { "ends_dialogue", choice.EndsDialogue.ToString() }
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        { "choice_action_count", choice.Actions.Count },
                        { "has_conditions", !string.IsNullOrEmpty(choice.Condition) }
                    }
                });

                _logger.LogDebug("Stored dialogue choice memory for player {PlayerId}: {ChoiceText}",
                    effectivePlayerId, choice.Text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store dialogue choice memory");
            }
        });
    }

    /// <summary>
    /// Retrieves relevant memories about an NPC for context-aware responses
    /// </summary>
    public async Task<IReadOnlyList<MemoryResult>> GetNpcMemoriesAsync(
        string playerId,
        string npcId,
        int limit = 5)
    {
        try
        {
            var memories = await _memoryService.RetrieveRelevantMemoriesAsync(
                $"interactions with NPC {npcId}",
                new MemoryRetrievalOptions
                {
                    EntityId = playerId,
                    MemoryType = null, // All types
                    MinRelevanceScore = 0.6,
                    MinImportance = 0.5,
                    Limit = limit,
                    Tags = new Dictionary<string, string>
                    {
                        { "npc_id", npcId }
                    }
                }
            );

            _logger.LogDebug(
                "Retrieved {CountValue} memories for player {PlayerIdValue} about NPC {NpcIdValue}",
                memories.Count, playerId, npcId);

            return memories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to retrieve NPC memories");
            return (IReadOnlyList<MemoryResult>)Array.Empty<MemoryResult>();
        }
    }

    /// <summary>
    /// Retrieves recent dialogue memories for context
    /// </summary>
    public async Task<IReadOnlyList<MemoryResult>> GetRecentDialogueMemoriesAsync(
        string playerId,
        TimeSpan? timeWindow = null,
        int limit = 10)
    {
        try
        {
            var since = timeWindow.HasValue
                ? DateTimeOffset.UtcNow - timeWindow.Value
                : DateTimeOffset.UtcNow.AddHours(-2);

            var memories = await _memoryService.RetrieveRelevantMemoriesAsync(
                "recent conversations and dialogue choices",
                new MemoryRetrievalOptions
                {
                    EntityId = playerId,
                    MemoryType = "dialogue_choice",
                    MinRelevanceScore = 0.5,
                    MinImportance = 0.4,
                    Limit = limit,
                    FromTimestamp = since
                }
            );

            _logger.LogDebug(
                "Retrieved {CountValue} recent dialogue memories for player {PlayerIdValue}",
                memories.Count, playerId);

            return memories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to retrieve recent dialogue memories");
            return (IReadOnlyList<MemoryResult>)Array.Empty<MemoryResult>();
        }
    }

    /// <summary>
    /// Analyzes player's relationship with an NPC based on memories
    /// </summary>
    public async Task<NpcRelationshipInsights> AnalyzeRelationshipAsync(
        string playerId,
        string npcId)
    {
        try
        {
            var memories = await GetNpcMemoriesAsync(playerId, npcId, limit: 20);

            var insights = new NpcRelationshipInsights
            {
                NpcId = npcId,
                InteractionCount = memories.Count,
                TotalImportance = memories.Sum(m => m.Memory.Importance),
                LastInteraction = memories.Any()
                    ? memories.OrderByDescending(m => m.Memory.Timestamp).First().Memory.Timestamp
                    : null,
                RelationshipLevel = CalculateRelationshipLevel(memories)
            };

            _logger.LogDebug("Analyzed relationship for player {PlayerId} with NPC {NpcId}: {Level}",
                playerId, npcId, insights.RelationshipLevel);

            return insights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze relationship");
            return new NpcRelationshipInsights { NpcId = npcId };
        }
    }

    // Delegate methods to base system
    public void LoadDialogueTree(DialogueTree tree) => _baseDialogueSystem.LoadDialogueTree(tree);
    public DialogueTree? GetDialogueTree(string treeId) => _baseDialogueSystem.GetDialogueTree(treeId);
    public void EndDialogue(Entity dialogueEntity) => _baseDialogueSystem.EndDialogue(dialogueEntity);
    public DialogueNode? GetCurrentNode(Entity dialogueEntity) => _baseDialogueSystem.GetCurrentNode(dialogueEntity);
    public List<DialogueChoice> GetAvailableChoices(Entity dialogueEntity) => _baseDialogueSystem.GetAvailableChoices(dialogueEntity);
    public Entity? GetActiveDialogue(Entity playerEntity) => _baseDialogueSystem.GetActiveDialogue(playerEntity);

    #region Helper Methods

    private Entity? FindEntityById(int entityId)
    {
        Entity? result = null;
        var query = new QueryDescription();

        _world.Query(in query, (Entity entity) =>
        {
            if (entity.Id == entityId)
            {
                result = entity;
            }
        });

        return result;
    }

    private double CalculateChoiceImportance(DialogueChoice choice, DialogueNode? node)
    {
        double importance = 0.6; // Base importance

        // Increase importance for dialogue-ending choices
        if (choice.EndsDialogue)
            importance += 0.2;

        // Increase importance for choices with actions
        if (choice.Actions.Count > 0)
            importance += 0.1;

        // Increase importance for conditional choices (they're usually significant)
        if (!string.IsNullOrEmpty(choice.Condition))
            importance += 0.1;

        return Math.Min(1.0, importance);
    }

    private RelationshipLevel CalculateRelationshipLevel(IReadOnlyList<MemoryResult> memories)
    {
        if (memories.Count == 0)
            return RelationshipLevel.Stranger;

        var totalImportance = memories.Sum(m => m.Memory.Importance);
        var interactionCount = memories.Count;

        if (interactionCount >= 15 && totalImportance > 10.0)
            return RelationshipLevel.TrustedFriend;
        if (interactionCount >= 10 && totalImportance > 6.0)
            return RelationshipLevel.CloseFriend;
        if (interactionCount >= 7 && totalImportance > 4.0)
            return RelationshipLevel.GoodFriend;
        if (interactionCount >= 4 && totalImportance > 2.0)
            return RelationshipLevel.Friend;
        if (interactionCount >= 2)
            return RelationshipLevel.Acquaintance;

        return RelationshipLevel.Stranger;
    }

    #endregion
}

/// <summary>
/// Insights about player-NPC relationship based on memories
/// </summary>
public class NpcRelationshipInsights
{
    public required string NpcId { get; init; }
    public int InteractionCount { get; init; }
    public double TotalImportance { get; init; }
    public DateTimeOffset? LastInteraction { get; init; }
    public RelationshipLevel RelationshipLevel { get; init; } = RelationshipLevel.Stranger;

    // Legacy properties for backward compatibility
    public int TotalInteractions => InteractionCount;
    public double AverageImportance => InteractionCount > 0 ? TotalImportance / InteractionCount : 0.0;
    public DateTimeOffset? MostRecentInteraction => LastInteraction;
}

/// <summary>
/// Relationship levels for NPC interactions
/// </summary>
public enum RelationshipLevel
{
    Stranger = 0,
    Acquaintance = 1,
    Friend = 2,
    GoodFriend = 3,
    CloseFriend = 4,
    TrustedFriend = 5
}
