using System.Collections.Generic;

namespace LablabBean.Plugins.Quest.Components;

/// <summary>
/// Quest rewards component - defines rewards granted upon quest completion
/// </summary>
public struct QuestRewards
{
    /// <summary>
    /// Experience points granted
    /// </summary>
    public int ExperiencePoints { get; set; }

    /// <summary>
    /// Gold amount granted
    /// </summary>
    public int Gold { get; set; }

    /// <summary>
    /// List of item IDs to grant as rewards
    /// </summary>
    public List<string> ItemIds { get; set; }

    public QuestRewards(int experiencePoints = 0, int gold = 0, List<string>? itemIds = null)
    {
        ExperiencePoints = experiencePoints;
        Gold = gold;
        ItemIds = itemIds ?? new List<string>();
    }

    /// <summary>
    /// Checks if there are any rewards
    /// </summary>
    public bool HasRewards => ExperiencePoints > 0 || Gold > 0 || ItemIds.Count > 0;
}
