namespace LablabBean.Plugins.Quest.Components;

/// <summary>
/// Quest component - represents a quest in the game
/// </summary>
public struct Quest
{
    /// <summary>
    /// Unique identifier for the quest
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of the quest
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Quest description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Current state of the quest
    /// </summary>
    public QuestState State { get; set; }

    /// <summary>
    /// Entity ID of the quest giver NPC
    /// </summary>
    public int QuestGiverId { get; set; }

    /// <summary>
    /// Flavor text shown when quest is active
    /// </summary>
    public string? FlavorText { get; set; }

    public Quest(string id, string name, string description, int questGiverId)
    {
        Id = id;
        Name = name;
        Description = description;
        State = QuestState.NotStarted;
        QuestGiverId = questGiverId;
        FlavorText = null;
    }
}

/// <summary>
/// Quest state enumeration
/// </summary>
public enum QuestState
{
    NotStarted,
    Active,
    Completed,
    Failed,
    Abandoned
}
