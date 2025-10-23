using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.NPC.Data;
using LablabBean.Plugins.NPC.Systems;

namespace LablabBean.Plugins.NPC.Services;

/// <summary>
/// Service for NPC interactions and dialogue management
/// Enhanced for US3: Full dialogue tree navigation and state management
/// </summary>
public class NPCService
{
    private readonly World _world;
    private readonly NPCSystem _npcSystem;
    private readonly DialogueSystem _dialogueSystem;

    public NPCService(World world, NPCSystem npcSystem, DialogueSystem dialogueSystem)
    {
        _world = world;
        _npcSystem = npcSystem;
        _dialogueSystem = dialogueSystem;
    }

    /// <summary>
    /// Loads a dialogue tree into the dialogue system
    /// </summary>
    public void LoadDialogueTree(DialogueTree tree)
    {
        _dialogueSystem.LoadDialogueTree(tree);
    }

    /// <summary>
    /// Starts dialogue with an NPC
    /// </summary>
    public bool StartDialogue(Entity playerEntity, Entity npcEntity)
    {
        if (!playerEntity.IsAlive() || !npcEntity.IsAlive())
            return false;

        if (!_npcSystem.CanInteract(npcEntity, playerEntity))
            return false;

        var npc = npcEntity.Get<Components.NPC>();

        if (string.IsNullOrEmpty(npc.DialogueTreeId))
            return false;

        var dialogueEntity = _dialogueSystem.StartDialogue(playerEntity, npcEntity, npc.DialogueTreeId);
        return dialogueEntity != null;
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
    /// Selects a dialogue choice (enhanced for US3 with full tree navigation)
    /// </summary>
    public void SelectChoice(Entity playerEntity, string choiceId)
    {
        var dialogueEntity = _dialogueSystem.GetActiveDialogue(playerEntity);

        if (dialogueEntity != null)
        {
            _dialogueSystem.SelectChoice(dialogueEntity.Value, choiceId);
        }
    }

    /// <summary>
    /// Gets the current dialogue node for the player
    /// </summary>
    public DialogueNode? GetCurrentDialogueNode(Entity playerEntity)
    {
        var dialogueEntity = _dialogueSystem.GetActiveDialogue(playerEntity);

        if (dialogueEntity == null)
            return null;

        return _dialogueSystem.GetCurrentNode(dialogueEntity.Value);
    }

    /// <summary>
    /// Gets available choices for the current dialogue node
    /// Filtered by conditions (level, items, quest progress, etc.)
    /// </summary>
    public List<DialogueChoice> GetAvailableChoices(Entity playerEntity)
    {
        var dialogueEntity = _dialogueSystem.GetActiveDialogue(playerEntity);

        if (dialogueEntity == null)
            return new List<DialogueChoice>();

        return _dialogueSystem.GetAvailableChoices(dialogueEntity.Value);
    }

    /// <summary>
    /// Checks if player is currently in dialogue
    /// </summary>
    public bool IsInDialogue(Entity playerEntity)
    {
        return _dialogueSystem.IsInDialogue(playerEntity);
    }

    /// <summary>
    /// Creates a new NPC
    /// </summary>
    public Entity CreateNPC(string id, string name, string role, string? dialogueTreeId = null)
    {
        return _npcSystem.CreateNPC(id, name, role, dialogueTreeId);
    }

    /// <summary>
    /// Finds an NPC by ID
    /// </summary>
    public Entity? FindNPCById(string npcId)
    {
        return _npcSystem.FindNPCById(npcId);
    }

    /// <summary>
    /// Sets NPC state variable (key-value pair)
    /// Used for tracking player choices and NPC memory
    /// </summary>
    public void SetNPCState(Entity npcEntity, string key, string value)
    {
        if (!npcEntity.IsAlive() || !npcEntity.Has<Components.NPC>())
            return;

        ref var npc = ref npcEntity.Get<Components.NPC>();
        npc.SetState(key, value);
    }

    /// <summary>
    /// Gets NPC state variable value
    /// </summary>
    public string? GetNPCState(Entity npcEntity, string key)
    {
        if (!npcEntity.IsAlive() || !npcEntity.Has<Components.NPC>())
            return null;

        ref var npc = ref npcEntity.Get<Components.NPC>();
        return npc.GetState(key);
    }

    /// <summary>
    /// Checks if NPC has a specific state variable
    /// </summary>
    public bool HasNPCState(Entity npcEntity, string key)
    {
        if (!npcEntity.IsAlive() || !npcEntity.Has<Components.NPC>())
            return false;

        ref var npc = ref npcEntity.Get<Components.NPC>();
        return npc.HasState(key);
    }

    /// <summary>
    /// Clears all NPC state variables
    /// </summary>
    public void ClearNPCState(Entity npcEntity)
    {
        if (!npcEntity.IsAlive() || !npcEntity.Has<Components.NPC>())
            return;

        ref var npc = ref npcEntity.Get<Components.NPC>();
        npc.ClearState();
    }

    /// <summary>
    /// Evaluates a dialogue condition string (Simple DSL parser)
    /// Examples: "level >= 5", "hasItem('key')", "npcState('met') == true"
    /// </summary>
    public bool EvaluateCondition(string condition, Entity playerEntity, Entity npcEntity)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return true;

        condition = condition.Trim();

        // Handle AND logic
        if (condition.Contains(" AND "))
        {
            var parts = condition.Split(new[] { " AND " }, System.StringSplitOptions.None);
            foreach (var part in parts)
            {
                if (!EvaluateCondition(part.Trim(), playerEntity, npcEntity))
                    return false;
            }
            return true;
        }

        // Handle OR logic
        if (condition.Contains(" OR "))
        {
            var parts = condition.Split(new[] { " OR " }, System.StringSplitOptions.None);
            foreach (var part in parts)
            {
                if (EvaluateCondition(part.Trim(), playerEntity, npcEntity))
                    return true;
            }
            return false;
        }

        // Handle specific conditions
        if (condition.StartsWith("npcState("))
        {
            return EvaluateNPCStateCondition(condition, npcEntity);
        }

        if (condition.StartsWith("level "))
        {
            return EvaluateLevelCondition(condition, playerEntity);
        }

        if (condition.StartsWith("hasItem("))
        {
            return EvaluateHasItemCondition(condition, playerEntity);
        }

        if (condition.StartsWith("hasQuest("))
        {
            return EvaluateHasQuestCondition(condition, playerEntity);
        }

        if (condition.StartsWith("questComplete("))
        {
            return EvaluateQuestCompleteCondition(condition, playerEntity);
        }

        // Default to true for unknown conditions
        return true;
    }

    // Private helper methods for condition evaluation

    private bool EvaluateNPCStateCondition(string condition, Entity npcEntity)
    {
        // Format: npcState('key') == 'value' or npcState('key') == true
        var start = condition.IndexOf('(');
        var end = condition.IndexOf(')');
        if (start < 0 || end < 0)
            return false;

        var keyPart = condition.Substring(start + 1, end - start - 1).Trim('\'', '"');

        if (condition.Contains("=="))
        {
            var valuePart = condition.Substring(condition.IndexOf("==") + 2).Trim().Trim('\'', '"');
            var currentValue = GetNPCState(npcEntity, keyPart);
            return currentValue == valuePart || (valuePart == "true" && !string.IsNullOrEmpty(currentValue));
        }

        // Just checking if state exists
        return HasNPCState(npcEntity, keyPart);
    }

    private bool EvaluateLevelCondition(string condition, Entity playerEntity)
    {
        // Format: level >= 5 or level > 3 or level == 10
        // TODO: Implement when player level system is available
        return true; // Placeholder
    }

    private bool EvaluateHasItemCondition(string condition, Entity playerEntity)
    {
        // Format: hasItem('rusty_key')
        // TODO: Implement when inventory system is available
        return true; // Placeholder
    }

    private bool EvaluateHasQuestCondition(string condition, Entity playerEntity)
    {
        // Format: hasQuest('retrieve_artifact')
        // TODO: Implement when quest system integration is available
        return true; // Placeholder
    }

    private bool EvaluateQuestCompleteCondition(string condition, Entity playerEntity)
    {
        // Format: questComplete('retrieve_artifact')
        // TODO: Implement when quest system integration is available
        return true; // Placeholder
    }
}
