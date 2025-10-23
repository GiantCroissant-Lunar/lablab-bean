using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.NPC.Components;

namespace LablabBean.Plugins.NPC.Systems;

/// <summary>
/// System that manages dialogue interactions between players and NPCs
/// For US1 (Quest), this is minimal - just quest acceptance
/// Will be enhanced in US3 for full branching dialogue
/// </summary>
public class DialogueSystem
{
    private readonly World _world;

    public DialogueSystem(World world)
    {
        _world = world;
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

        var dialogueState = new DialogueState(
            dialogueTreeId,
            "root", // Start at root node
            playerEntity.Id,
            npcEntity.Id
        );

        var dialogueEntity = _world.Create(dialogueState);
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
    /// Moves dialogue to a new node (basic implementation for quest acceptance)
    /// </summary>
    public void SelectChoice(Entity dialogueEntity, string choiceId)
    {
        if (!dialogueEntity.IsAlive() || !dialogueEntity.Has<DialogueState>())
            return;

        ref var dialogueState = ref dialogueEntity.Get<DialogueState>();

        // For US1, we just handle simple quest acceptance
        // US3 will implement full branching dialogue tree navigation
        if (choiceId == "accept_quest")
        {
            dialogueState.MoveToNode("quest_accepted");
        }
        else if (choiceId == "decline_quest")
        {
            dialogueState.MoveToNode("quest_declined");
        }
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
}
