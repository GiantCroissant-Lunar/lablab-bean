namespace LablabBean.Plugins.Quest.Components;

/// <summary>
/// Quest objective component - represents a single objective within a quest
/// </summary>
public struct QuestObjective
{
    /// <summary>
    /// Unique identifier for the objective
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// ID of the quest this objective belongs to
    /// </summary>
    public string QuestId { get; set; }

    /// <summary>
    /// Type of objective (Kill, Collect, Reach, Talk)
    /// </summary>
    public ObjectiveType Type { get; set; }

    /// <summary>
    /// Description text shown to player
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Target for the objective (e.g., enemy type, item ID, location ID, NPC ID)
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// Current progress count
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// Required count to complete objective
    /// </summary>
    public int Required { get; set; }

    /// <summary>
    /// Whether this objective is completed
    /// </summary>
    public bool IsCompleted => Current >= Required;

    public QuestObjective(string id, string questId, ObjectiveType type, string description, string target, int required)
    {
        Id = id;
        QuestId = questId;
        Type = type;
        Description = description;
        Target = target;
        Current = 0;
        Required = required;
    }
}

/// <summary>
/// Types of quest objectives
/// </summary>
public enum ObjectiveType
{
    Kill,       // Kill X enemies of type Y
    Collect,    // Collect X items of type Y
    Reach,      // Reach location X
    Talk        // Talk to NPC X
}
