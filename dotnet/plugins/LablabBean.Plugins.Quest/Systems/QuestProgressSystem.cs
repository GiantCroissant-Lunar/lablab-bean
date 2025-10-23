using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Events;
using LablabBean.Plugins.Quest.Components;

namespace LablabBean.Plugins.Quest.Systems;

/// <summary>
/// Event-driven system that tracks quest objective progress
/// Subscribes to game events and updates quest objectives accordingly
/// </summary>
public class QuestProgressSystem
{
    private readonly World _world;

    public QuestProgressSystem(World world)
    {
        _world = world;
    }

    // Event handlers - commented out until events are properly defined
    // These will be implemented in future phases

    /*
    /// <summary>
    /// Handles enemy killed events and updates kill objectives
    /// </summary>
    public void OnEnemyKilled(EnemyKilledEvent evt)
    {
        // UpdateObjectives(evt.KillerEntity, ObjectiveType.Kill, evt.EnemyType);
    }

    /// <summary>
    /// Handles item collected events and updates collection objectives
    /// </summary>
    public void OnItemCollected(ItemCollectedEvent evt)
    {
        // UpdateObjectives(evt.Entity, ObjectiveType.Collect, evt.ItemId, evt.Quantity);
    }

    /// <summary>
    /// Handles location reached events and updates location objectives
    /// </summary>
    public void OnLocationReached(LocationReachedEvent evt)
    {
        // UpdateObjectives(evt.Entity, ObjectiveType.Reach, evt.LocationId);
    }

    /// <summary>
    /// Handles NPC interaction events and updates talk objectives
    /// </summary>
    public void OnNPCInteraction(NPCInteractionEvent evt)
    {
        // if (evt.InteractionType == "Talk")
        // {
        //     UpdateObjectivesForNPC(evt.PlayerEntity, evt.NPCEntity);
        // }
    }
    */

    /// <summary>
    /// Updates objectives of a specific type and target for the player's active quests
    /// </summary>
    private void UpdateObjectives(Entity playerEntity, ObjectiveType type, string target, int amount = 1)
    {
        if (!playerEntity.Has<QuestLog>())
            return;

        ref var questLog = ref playerEntity.Get<QuestLog>();

        foreach (var questEntityId in questLog.ActiveQuests)
        {
            var questEntity = FindEntityById(questEntityId);
            if (questEntity == null || !questEntity.Value.Has<Components.Quest>())
                continue;

            ref var quest = ref questEntity.Value.Get<Components.Quest>();
            if (quest.State != QuestState.Active)
                continue;

            UpdateQuestObjectives(quest.Id, type, target, amount);
        }
    }

    /// <summary>
    /// Finds an entity by its ID
    /// </summary>
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

    /// <summary>
    /// Updates all matching objectives for a quest
    /// </summary>
    private void UpdateQuestObjectives(string questId, ObjectiveType type, string target, int amount = 1)
    {
        var query = new QueryDescription().WithAll<QuestObjective>();

        _world.Query(in query, (Entity entity, ref QuestObjective objective) =>
        {
            if (objective.QuestId == questId &&
                objective.Type == type &&
                objective.Target == target &&
                !objective.IsCompleted)
            {
                objective.Current = Math.Min(objective.Current + amount, objective.Required);
            }
        });
    }
}
