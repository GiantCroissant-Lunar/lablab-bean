using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.NPC.Systems;

namespace LablabBean.Plugins.NPC.Services;

/// <summary>
/// Service for NPC interactions and dialogue management
/// Minimal implementation for US1 (Quest) - will be enhanced in US3
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
    /// Selects a dialogue choice
    /// For US1, this handles quest acceptance/decline
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
    /// Sets NPC state data
    /// </summary>
    public void SetNPCState(Entity npcEntity, string stateData)
    {
        _npcSystem.SetNPCState(npcEntity, stateData);
    }

    /// <summary>
    /// Gets NPC state data
    /// </summary>
    public string? GetNPCState(Entity npcEntity)
    {
        return _npcSystem.GetNPCState(npcEntity);
    }
}
