using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Game.Core.Events;
using LablabBean.Plugins.Quest.Components;
using LablabBean.Plugins.Progression.Services;

namespace LablabBean.Plugins.Quest.Systems;

/// <summary>
/// System that grants rewards when quests are completed
/// </summary>
public class QuestRewardSystem
{
    private readonly World _world;
    private Action<QuestCompletedEvent>? _onQuestCompleted;
    private ProgressionService? _progressionService;

    public QuestRewardSystem(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Sets the progression service for awarding XP
    /// </summary>
    public void SetProgressionService(ProgressionService progressionService)
    {
        _progressionService = progressionService;
    }

    /// <summary>
    /// Registers callback for quest completion events
    /// </summary>
    public void OnQuestCompleted(Action<QuestCompletedEvent> callback)
    {
        _onQuestCompleted = callback;
    }

    /// <summary>
    /// Processes quest completion and grants rewards
    /// </summary>
    public void CompleteQuest(Entity questEntity, Entity playerEntity)
    {
        if (!questEntity.Has<Components.Quest>() || !questEntity.Has<QuestRewards>())
            return;

        ref var quest = ref questEntity.Get<Components.Quest>();

        if (quest.State != QuestState.Completed)
            return;

        ref var rewards = ref questEntity.Get<QuestRewards>();

        // Grant gold
        if (rewards.Gold > 0 && playerEntity.Has<Gold>())
        {
            ref var gold = ref playerEntity.Get<Gold>();
            gold.Add(rewards.Gold);
        }

        // Grant experience points via Progression system
        if (rewards.ExperiencePoints > 0 && _progressionService != null)
        {
            // TODO: Proper entity ID mapping when available
            // For now, use a placeholder GUID
            var playerId = Guid.Empty;
            bool leveledUp = _progressionService.AwardExperience(playerId, rewards.ExperiencePoints);

            // TODO: Show level-up notification if leveledUp is true
        }

        // Mark quest as rewarded in quest log
        if (playerEntity.Has<QuestLog>())
        {
            ref var questLog = ref playerEntity.Get<QuestLog>();
            questLog.CompleteQuest(quest.Id, questEntity.Id);
        }

        // Publish quest completed event
        _onQuestCompleted?.Invoke(new QuestCompletedEvent(quest.Id, playerEntity.Id));
    }

    /// <summary>
    /// Checks if all objectives are complete and grants rewards if so
    /// </summary>
    public void ProcessQuestCompletion(Entity playerEntity)
    {
        if (!playerEntity.Has<QuestLog>())
            return;

        ref var questLog = ref playerEntity.Get<QuestLog>();
        var completedQuests = new List<int>();

        foreach (var questEntityId in questLog.ActiveQuests)
        {
            var questEntity = FindEntityById(questEntityId);
            if (questEntity == null || !questEntity.Value.Has<Components.Quest>())
                continue;

            ref var quest = ref questEntity.Value.Get<Components.Quest>();

            if (quest.State == QuestState.Completed)
            {
                completedQuests.Add(questEntityId);
                CompleteQuest(questEntity.Value, playerEntity);
            }
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
    /// Creates reward component for a quest entity
    /// </summary>
    public void AddRewards(Entity questEntity, int experiencePoints, int gold, List<string>? itemIds = null)
    {
        var rewards = new QuestRewards(experiencePoints, gold, itemIds);
        questEntity.Add(rewards);
    }
}
