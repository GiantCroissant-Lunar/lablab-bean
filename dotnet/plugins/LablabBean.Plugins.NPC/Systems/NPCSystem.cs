using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.NPC.Components;

namespace LablabBean.Plugins.NPC.Systems;

/// <summary>
/// System that manages NPC entities - spawning, interaction checks, and basic behavior
/// </summary>
public class NPCSystem
{
    private readonly World _world;

    public NPCSystem(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Creates a new NPC entity
    /// </summary>
    public Entity CreateNPC(string id, string name, string role, string? dialogueTreeId = null)
    {
        var npc = new Components.NPC(id, name, role, dialogueTreeId);
        return _world.Create(npc);
    }

    /// <summary>
    /// Checks if a player can interact with an NPC
    /// </summary>
    public bool CanInteract(Entity npcEntity, Entity playerEntity)
    {
        if (!npcEntity.IsAlive() || !npcEntity.Has<Components.NPC>())
            return false;

        ref var npc = ref npcEntity.Get<Components.NPC>();
        return npc.IsInteractable;
    }

    /// <summary>
    /// Initiates interaction between player and NPC
    /// </summary>
    public bool StartInteraction(Entity npcEntity, Entity playerEntity)
    {
        if (!CanInteract(npcEntity, playerEntity))
            return false;

        ref var npc = ref npcEntity.Get<Components.NPC>();

        if (string.IsNullOrEmpty(npc.DialogueTreeId))
            return false;

        return true;
    }

    /// <summary>
    /// Finds an NPC by ID
    /// </summary>
    public Entity? FindNPCById(string npcId)
    {
        Entity? result = null;
        var query = new QueryDescription().WithAll<Components.NPC>();

        _world.Query(in query, (Entity entity, ref Components.NPC npc) =>
        {
            if (npc.Id == npcId)
            {
                result = entity;
            }
        });

        return result;
    }

    /// <summary>
    /// Updates NPC state
    /// </summary>
    public void SetNPCState(Entity npcEntity, string stateData)
    {
        if (!npcEntity.IsAlive() || !npcEntity.Has<Components.NPC>())
            return;

        ref var npc = ref npcEntity.Get<Components.NPC>();
        npc.StateData = stateData;
    }

    /// <summary>
    /// Gets NPC state
    /// </summary>
    public string? GetNPCState(Entity npcEntity)
    {
        if (!npcEntity.IsAlive() || !npcEntity.Has<Components.NPC>())
            return null;

        ref var npc = ref npcEntity.Get<Components.NPC>();
        return npc.StateData;
    }

    /// <summary>
    /// Updates all NPCs (placeholder for future AI/behavior)
    /// </summary>
    public void Update()
    {
        var query = new QueryDescription().WithAll<Components.NPC>();

        _world.Query(in query, (Entity entity, ref Components.NPC npc) =>
        {
            // Future: Add NPC AI behavior, patrol routes, etc.
        });
    }
}
