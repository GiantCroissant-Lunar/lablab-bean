using System.Collections.Generic;

namespace LablabBean.Plugins.Quest.Components;

/// <summary>
/// Quest prerequisites component - defines requirements to start a quest
/// </summary>
public struct QuestPrerequisites
{
    /// <summary>
    /// Minimum player level required
    /// </summary>
    public int MinLevel { get; set; }

    /// <summary>
    /// List of quest IDs that must be completed before this quest
    /// </summary>
    public List<string> RequiredQuests { get; set; }

    /// <summary>
    /// List of item IDs required in inventory to start quest
    /// </summary>
    public List<string> RequiredItems { get; set; }

    public QuestPrerequisites(int minLevel = 1, List<string>? requiredQuests = null, List<string>? requiredItems = null)
    {
        MinLevel = minLevel;
        RequiredQuests = requiredQuests ?? new List<string>();
        RequiredItems = requiredItems ?? new List<string>();
    }

    /// <summary>
    /// Checks if there are any prerequisites
    /// </summary>
    public bool HasPrerequisites => MinLevel > 1 || RequiredQuests.Count > 0 || RequiredItems.Count > 0;
}
