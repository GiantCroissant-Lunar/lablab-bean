using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Quest.Components;

namespace LablabBean.Plugins.Quest.Systems;

/// <summary>
/// System that processes quest entities and manages quest lifecycle
/// </summary>
public class QuestSystem
{
    private readonly World _world;

    public QuestSystem(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Updates all active quests
    /// </summary>
    public void Update()
    {
        var query = new QueryDescription().WithAll<Components.Quest>();

        _world.Query(in query, (Entity entity, ref Components.Quest quest) =>
        {
            if (quest.State == QuestState.Active)
            {
                CheckQuestCompletion(entity, ref quest);
            }
        });
    }

    /// <summary>
    /// Checks if all objectives for a quest are complete
    /// </summary>
    private void CheckQuestCompletion(Entity questEntity, ref Components.Quest quest)
    {
        bool allObjectivesComplete = true;
        string questId = quest.Id; // Capture the value, not the ref

        var objectiveQuery = new QueryDescription().WithAll<QuestObjective>();
        _world.Query(in objectiveQuery, (Entity entity, ref QuestObjective objective) =>
        {
            if (objective.QuestId == questId && !objective.IsCompleted)
            {
                allObjectivesComplete = false;
            }
        });

        if (allObjectivesComplete)
        {
            quest.State = QuestState.Completed;
        }
    }

    /// <summary>
    /// Creates a new quest entity
    /// </summary>
    public Entity CreateQuest(string id, string name, string description, int questGiverId)
    {
        var quest = new Components.Quest(id, name, description, questGiverId);
        return _world.Create(quest);
    }

    /// <summary>
    /// Starts a quest for a player
    /// </summary>
    public bool StartQuest(Entity questEntity, Entity playerEntity)
    {
        if (!questEntity.Has<Components.Quest>() || !playerEntity.Has<QuestLog>())
            return false;

        ref var quest = ref questEntity.Get<Components.Quest>();

        if (quest.State != QuestState.NotStarted)
            return false;

        quest.State = QuestState.Active;

        ref var questLog = ref playerEntity.Get<QuestLog>();
        questLog.AddActiveQuest(questEntity.Id);

        return true;
    }

    /// <summary>
    /// Fails a quest
    /// </summary>
    public void FailQuest(Entity questEntity, Entity playerEntity)
    {
        if (!questEntity.Has<Components.Quest>() || !playerEntity.Has<QuestLog>())
            return;

        ref var quest = ref questEntity.Get<Components.Quest>();
        quest.State = QuestState.Failed;

        ref var questLog = ref playerEntity.Get<QuestLog>();
        questLog.FailQuest(quest.Id, questEntity.Id);
    }

    /// <summary>
    /// Abandons a quest
    /// </summary>
    public void AbandonQuest(Entity questEntity, Entity playerEntity)
    {
        if (!questEntity.Has<Components.Quest>() || !playerEntity.Has<QuestLog>())
            return;

        ref var quest = ref questEntity.Get<Components.Quest>();
        quest.State = QuestState.Abandoned;

        ref var questLog = ref playerEntity.Get<QuestLog>();
        questLog.RemoveActiveQuest(questEntity.Id);
    }
}
