using System.Collections.Generic;

namespace LablabBean.Plugins.Quest.Components;

/// <summary>
/// Quest log component - tracks all quests for a player entity
/// </summary>
public struct QuestLog
{
    /// <summary>
    /// List of active quest entity IDs
    /// </summary>
    public List<int> ActiveQuests { get; set; }

    /// <summary>
    /// List of completed quest IDs
    /// </summary>
    public List<string> CompletedQuests { get; set; }

    /// <summary>
    /// List of failed quest IDs
    /// </summary>
    public List<string> FailedQuests { get; set; }

    public QuestLog()
    {
        ActiveQuests = new List<int>();
        CompletedQuests = new List<string>();
        FailedQuests = new List<string>();
    }

    /// <summary>
    /// Adds a quest to the active quests list
    /// </summary>
    public void AddActiveQuest(int questEntityId)
    {
        if (!ActiveQuests.Contains(questEntityId))
        {
            ActiveQuests.Add(questEntityId);
        }
    }

    /// <summary>
    /// Removes a quest from the active quests list
    /// </summary>
    public void RemoveActiveQuest(int questEntityId)
    {
        ActiveQuests.Remove(questEntityId);
    }

    /// <summary>
    /// Marks a quest as completed
    /// </summary>
    public void CompleteQuest(string questId, int questEntityId)
    {
        RemoveActiveQuest(questEntityId);
        if (!CompletedQuests.Contains(questId))
        {
            CompletedQuests.Add(questId);
        }
    }

    /// <summary>
    /// Marks a quest as failed
    /// </summary>
    public void FailQuest(string questId, int questEntityId)
    {
        RemoveActiveQuest(questEntityId);
        if (!FailedQuests.Contains(questId))
        {
            FailedQuests.Add(questId);
        }
    }

    /// <summary>
    /// Checks if a quest has been completed
    /// </summary>
    public bool HasCompleted(string questId)
    {
        return CompletedQuests.Contains(questId);
    }

    /// <summary>
    /// Checks if a quest is currently active
    /// </summary>
    public bool IsActive(int questEntityId)
    {
        return ActiveQuests.Contains(questEntityId);
    }
}
