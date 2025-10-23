using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.NPC.Components;
using LablabBean.Plugins.NPC.Data;

namespace LablabBean.Plugins.NPC.Systems;

/// <summary>
/// System that manages dialogue interactions between players and NPCs
/// Enhanced for US3: Full branching dialogue trees with conditions and actions
/// </summary>
public class DialogueSystem
{
    private readonly World _world;
    private readonly Dictionary<string, DialogueTree> _loadedTrees = new();

    public DialogueSystem(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Loads a dialogue tree into memory
    /// </summary>
    public void LoadDialogueTree(DialogueTree tree)
    {
        if (tree.Validate(out var errors))
        {
            _loadedTrees[tree.Id] = tree;
        }
        else
        {
            throw new InvalidOperationException($"Invalid dialogue tree '{tree.Id}': {string.Join(", ", errors)}");
        }
    }

    /// <summary>
    /// Gets a loaded dialogue tree by ID
    /// </summary>
    public DialogueTree? GetDialogueTree(string treeId)
    {
        return _loadedTrees.TryGetValue(treeId, out var tree) ? tree : null;
    }

    /// <summary>
    /// Starts a dialogue between player and NPC
    /// </summary>
    public Entity? StartDialogue(Entity playerEntity, Entity npcEntity, string dialogueTreeId)
    {
        if (!playerEntity.IsAlive() || !npcEntity.IsAlive())
            return null;

        if (!npcEntity.Has<Components.NPC>())
            return null;

        var tree = GetDialogueTree(dialogueTreeId);
        if (tree == null)
            return null;

        var startNode = tree.GetStartNode();
        if (startNode == null)
            return null;

        var dialogueState = new DialogueState(
            dialogueTreeId,
            startNode.Id,
            playerEntity.Id,
            npcEntity.Id
        );

        var dialogueEntity = _world.Create(dialogueState);

        // Execute on-enter actions for start node
        ExecuteNodeActions(startNode, playerEntity, npcEntity);

        return dialogueEntity;
    }

    /// <summary>
    /// Ends an active dialogue
    /// </summary>
    public void EndDialogue(Entity dialogueEntity)
    {
        if (!dialogueEntity.IsAlive() || !dialogueEntity.Has<DialogueState>())
            return;

        ref var dialogueState = ref dialogueEntity.Get<DialogueState>();
        dialogueState.End();

        _world.Destroy(dialogueEntity);
    }

    /// <summary>
    /// Processes a player's choice selection and moves to next node
    /// </summary>
    public void SelectChoice(Entity dialogueEntity, string choiceId)
    {
        if (!dialogueEntity.IsAlive() || !dialogueEntity.Has<DialogueState>())
            return;

        ref var dialogueState = ref dialogueEntity.Get<DialogueState>();

        var tree = GetDialogueTree(dialogueState.DialogueTreeId);
        if (tree == null)
            return;

        var currentNode = tree.GetNode(dialogueState.CurrentNodeId);
        if (currentNode == null)
            return;

        // Find the selected choice
        var choice = currentNode.Choices.FirstOrDefault(c => c.Id == choiceId);
        if (choice == null)
            return;

        var playerEntity = FindEntityById(dialogueState.PlayerEntityId);
        var npcEntity = FindEntityById(dialogueState.NPCEntityId);

        if (playerEntity == null || npcEntity == null)
            return;

        // Execute choice actions
        ExecuteChoiceActions(choice, playerEntity.Value, npcEntity.Value);

        // Move to next node or end dialogue
        if (choice.EndsDialogue || string.IsNullOrEmpty(choice.NextNodeId))
        {
            EndDialogue(dialogueEntity);
        }
        else
        {
            var nextNode = tree.GetNode(choice.NextNodeId);
            if (nextNode != null)
            {
                dialogueState.MoveToNode(nextNode.Id);
                ExecuteNodeActions(nextNode, playerEntity.Value, npcEntity.Value);
            }
            else
            {
                EndDialogue(dialogueEntity);
            }
        }
    }

    /// <summary>
    /// Gets the current dialogue node for an active dialogue
    /// </summary>
    public DialogueNode? GetCurrentNode(Entity dialogueEntity)
    {
        if (!dialogueEntity.IsAlive() || !dialogueEntity.Has<DialogueState>())
            return null;

        ref var dialogueState = ref dialogueEntity.Get<DialogueState>();
        var tree = GetDialogueTree(dialogueState.DialogueTreeId);

        return tree?.GetNode(dialogueState.CurrentNodeId);
    }

    /// <summary>
    /// Gets available choices for current node, filtered by conditions
    /// </summary>
    public List<DialogueChoice> GetAvailableChoices(Entity dialogueEntity)
    {
        var currentNode = GetCurrentNode(dialogueEntity);
        if (currentNode == null)
            return new List<DialogueChoice>();

        if (!dialogueEntity.Has<DialogueState>())
            return new List<DialogueChoice>();

        ref var dialogueState = ref dialogueEntity.Get<DialogueState>();
        var playerEntity = FindEntityById(dialogueState.PlayerEntityId);
        var npcEntity = FindEntityById(dialogueState.NPCEntityId);

        if (playerEntity == null || npcEntity == null)
            return new List<DialogueChoice>();

        // Filter choices based on conditions
        return currentNode.Choices
            .Where(choice => EvaluateCondition(choice.Condition, playerEntity.Value, npcEntity.Value))
            .ToList();
    }

    /// <summary>
    /// Gets active dialogue for a player
    /// </summary>
    public Entity? GetActiveDialogue(Entity playerEntity)
    {
        Entity? result = null;
        var query = new QueryDescription().WithAll<DialogueState>();

        _world.Query(in query, (Entity entity, ref DialogueState state) =>
        {
            if (state.IsActive && state.PlayerEntityId == playerEntity.Id)
            {
                result = entity;
            }
        });

        return result;
    }

    /// <summary>
    /// Checks if player is in any dialogue
    /// </summary>
    public bool IsInDialogue(Entity playerEntity)
    {
        return GetActiveDialogue(playerEntity) != null;
    }

    // Private helper methods

    private void ExecuteNodeActions(DialogueNode node, Entity playerEntity, Entity npcEntity)
    {
        foreach (var action in node.OnEnterActions)
        {
            ExecuteAction(action, playerEntity, npcEntity);
        }
    }

    private void ExecuteChoiceActions(DialogueChoice choice, Entity playerEntity, Entity npcEntity)
    {
        foreach (var action in choice.Actions)
        {
            ExecuteAction(action, playerEntity, npcEntity);
        }
    }

    private void ExecuteAction(DialogueAction action, Entity playerEntity, Entity npcEntity)
    {
        // Check action condition if present
        if (action.HasCondition && !EvaluateCondition(action.Condition, playerEntity, npcEntity))
            return;

        switch (action.Type)
        {
            case DialogueActionType.SetNPCState:
                SetNPCState(npcEntity, action.GetString("key"), action.Parameters.GetValueOrDefault("value"));
                break;

            case DialogueActionType.SetPlayerState:
                SetPlayerState(playerEntity, action.GetString("key"), action.Parameters.GetValueOrDefault("value"));
                break;

            // Other action types will be implemented when their systems are available
            // For now, we just handle state management
            case DialogueActionType.AcceptQuest:
            case DialogueActionType.CompleteQuest:
            case DialogueActionType.GiveItem:
            case DialogueActionType.TakeItem:
            case DialogueActionType.GiveGold:
            case DialogueActionType.TakeGold:
            case DialogueActionType.OpenTrade:
            case DialogueActionType.TriggerEvent:
            case DialogueActionType.StartCombat:
                // TODO: Implement when respective systems are available
                break;
        }
    }

    private bool EvaluateCondition(string? condition, Entity playerEntity, Entity npcEntity)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return true;

        // Simple condition parser
        // Supports: "level >= 5", "hasItem('key')", "npcState('met') == true"
        // This is a basic implementation - can be enhanced later

        condition = condition.Trim();

        // Handle basic comparisons
        if (condition.Contains(">=") || condition.Contains("<=") ||
            condition.Contains("==") || condition.Contains("!=") ||
            condition.Contains(">") || condition.Contains("<"))
        {
            // TODO: Implement comparison evaluation
            return true; // Placeholder
        }

        // Handle function calls
        if (condition.StartsWith("hasItem(") || condition.StartsWith("hasQuest(") ||
            condition.StartsWith("questComplete(") || condition.StartsWith("npcState("))
        {
            // TODO: Implement function evaluation
            return true; // Placeholder
        }

        // Handle AND/OR logic
        if (condition.Contains(" AND "))
        {
            var parts = condition.Split(new[] { " AND " }, StringSplitOptions.None);
            return parts.All(part => EvaluateCondition(part, playerEntity, npcEntity));
        }

        if (condition.Contains(" OR "))
        {
            var parts = condition.Split(new[] { " OR " }, StringSplitOptions.None);
            return parts.Any(part => EvaluateCondition(part, playerEntity, npcEntity));
        }

        // Default to true for unknown conditions
        return true;
    }

    private void SetNPCState(Entity npcEntity, string? key, object? value)
    {
        if (string.IsNullOrEmpty(key) || !npcEntity.Has<Components.NPC>())
            return;

        ref var npc = ref npcEntity.Get<Components.NPC>();
        npc.SetState(key, value?.ToString() ?? string.Empty);
    }

    private void SetPlayerState(Entity playerEntity, string? key, object? value)
    {
        // TODO: Implement player state storage when available
        // For now, this is a placeholder
    }

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
}
