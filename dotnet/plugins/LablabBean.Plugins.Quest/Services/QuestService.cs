using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Quest.Components;
using LablabBean.Plugins.Quest.Systems;

namespace LablabBean.Plugins.Quest.Services;

/// <summary>
/// Service for managing quests, objectives, and quest state
/// Implements IQuestService contract
/// </summary>
public class QuestService
{
    private readonly World _world;
    private readonly QuestSystem _questSystem;
    private readonly QuestProgressSystem _questProgressSystem;
    private readonly QuestRewardSystem _questRewardSystem;

    public QuestService(World world, QuestSystem questSystem, QuestProgressSystem questProgressSystem, QuestRewardSystem questRewardSystem)
    {
        _world = world;
        _questSystem = questSystem;
        _questProgressSystem = questProgressSystem;
        _questRewardSystem = questRewardSystem;
    }

    /// <summary>
    /// Attempts to start a quest for the player
    /// </summary>
    public bool StartQuest(Entity playerEntity, string questId)
    {
        if (!playerEntity.IsAlive() || !playerEntity.Has<QuestLog>())
            return false;

        var questEntity = FindQuestEntityById(questId);
        if (questEntity == null || !questEntity.Value.IsAlive())
            return false;

        if (!MeetsPrerequisites(playerEntity, questId))
            return false;

        return _questSystem.StartQuest(questEntity.Value, playerEntity);
    }

    /// <summary>
    /// Completes a quest and grants rewards
    /// </summary>
    public bool CompleteQuest(Entity playerEntity, string questId)
    {
        if (!playerEntity.IsAlive())
            return false;

        var questEntity = FindQuestEntityById(questId);
        if (questEntity == null || !questEntity.Value.IsAlive())
            return false;

        ref var quest = ref questEntity.Value.Get<Components.Quest>();
        if (quest.State != QuestState.Completed)
            return false;

        _questRewardSystem.CompleteQuest(questEntity.Value, playerEntity);
        return true;
    }

    /// <summary>
    /// Fails an active quest
    /// </summary>
    public void FailQuest(Entity playerEntity, string questId)
    {
        var questEntity = FindQuestEntityById(questId);

        if (questEntity != null && questEntity.Value.IsAlive())
        {
            _questSystem.FailQuest(questEntity.Value, playerEntity);
        }
    }

    /// <summary>
    /// Abandons an active quest
    /// </summary>
    public void AbandonQuest(Entity playerEntity, string questId)
    {
        var questEntity = FindQuestEntityById(questId);

        if (questEntity != null && questEntity.Value.IsAlive())
        {
            _questSystem.AbandonQuest(questEntity.Value, playerEntity);
        }
    }

    /// <summary>
    /// Gets all active quests for the player
    /// </summary>
    public List<QuestInfo> GetActiveQuests(Entity playerEntity)
    {
        if (!playerEntity.IsAlive() || !playerEntity.Has<QuestLog>())
            return new List<QuestInfo>();

        ref var questLog = ref playerEntity.Get<QuestLog>();
        var quests = new List<QuestInfo>();

        foreach (var questEntityId in questLog.ActiveQuests)
        {
            var questEntity = FindEntityById(questEntityId);
            if (questEntity != null && questEntity.Value.IsAlive() && questEntity.Value.Has<Components.Quest>())
            {
                quests.Add(CreateQuestInfo(questEntity.Value));
            }
        }

        return quests;
    }

    /// <summary>
    /// Gets all completed quests for the player
    /// </summary>
    public List<QuestInfo> GetCompletedQuests(Entity playerEntity)
    {
        if (!playerEntity.IsAlive() || !playerEntity.Has<QuestLog>())
            return new List<QuestInfo>();

        ref var questLog = ref playerEntity.Get<QuestLog>();
        var quests = new List<QuestInfo>();

        foreach (var questId in questLog.CompletedQuests)
        {
            var questEntity = FindQuestEntityById(questId);
            if (questEntity != null && questEntity.Value.IsAlive())
            {
                quests.Add(CreateQuestInfo(questEntity.Value));
            }
        }

        return quests;
    }

    /// <summary>
    /// Checks if all objectives for a quest are complete
    /// </summary>
    public bool AreObjectivesComplete(Entity playerEntity, string questId)
    {
        var questEntity = FindQuestEntityById(questId);
        if (questEntity == null || !questEntity.Value.IsAlive())
            return false;

        var objectives = GetQuestObjectives(questId);
        return objectives.All(o => o.IsCompleted);
    }

    /// <summary>
    /// Checks if player meets prerequisites for a quest
    /// </summary>
    public bool MeetsPrerequisites(Entity playerEntity, string questId)
    {
        var questEntity = FindQuestEntityById(questId);
        if (questEntity == null || !questEntity.Value.IsAlive() || !questEntity.Value.Has<QuestPrerequisites>())
            return true;

        ref var prerequisites = ref questEntity.Value.Get<QuestPrerequisites>();

        if (!playerEntity.IsAlive() || !playerEntity.Has<QuestLog>())
            return false;

        ref var questLog = ref playerEntity.Get<QuestLog>();

        foreach (var requiredQuestId in prerequisites.RequiredQuests)
        {
            if (!questLog.HasCompleted(requiredQuestId))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets available quests (not started, prerequisites met)
    /// </summary>
    public List<QuestInfo> GetAvailableQuests(Entity playerEntity)
    {
        var quests = new List<QuestInfo>();
        var query = new QueryDescription().WithAll<Components.Quest>();

        _world.Query(in query, (Entity entity, ref Components.Quest quest) =>
        {
            if (quest.State == QuestState.NotStarted && MeetsPrerequisites(playerEntity, quest.Id))
            {
                quests.Add(CreateQuestInfo(entity));
            }
        });

        return quests;
    }

    // Helper methods

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

    private Entity? FindQuestEntityById(string questId)
    {
        Entity? result = null;
        var query = new QueryDescription().WithAll<Components.Quest>();

        _world.Query(in query, (Entity entity, ref Components.Quest quest) =>
        {
            if (quest.Id == questId)
            {
                result = entity;
            }
        });

        return result;
    }

    private List<QuestObjective> GetQuestObjectives(string questId)
    {
        var objectives = new List<QuestObjective>();
        var query = new QueryDescription().WithAll<QuestObjective>();

        _world.Query(in query, (Entity entity, ref QuestObjective objective) =>
        {
            if (objective.QuestId == questId)
            {
                objectives.Add(objective);
            }
        });

        return objectives;
    }

    private QuestInfo CreateQuestInfo(Entity questEntity)
    {
        ref var quest = ref questEntity.Get<Components.Quest>();
        var objectives = GetQuestObjectives(quest.Id);

        var objectiveInfos = objectives.Select(o => new ObjectiveInfo(
            o.Description,
            o.Current,
            o.Required,
            o.IsCompleted
        )).ToList();

        var rewards = questEntity.Has<QuestRewards>()
            ? questEntity.Get<QuestRewards>()
            : new QuestRewards();

        var rewardsInfo = new QuestRewardsInfo(
            rewards.ExperiencePoints,
            rewards.Gold,
            rewards.ItemIds
        );

        return new QuestInfo(
            quest.Id,
            quest.Name,
            quest.Description,
            quest.State,
            objectiveInfos,
            rewardsInfo
        );
    }
}

// DTOs for quest information
public record QuestInfo(
    string Id,
    string Name,
    string Description,
    QuestState State,
    List<ObjectiveInfo> Objectives,
    QuestRewardsInfo Rewards
);

public record ObjectiveInfo(
    string Description,
    int Current,
    int Required,
    bool IsCompleted
);

public record QuestRewardsInfo(
    int ExperiencePoints,
    int Gold,
    List<string> ItemIds
);
