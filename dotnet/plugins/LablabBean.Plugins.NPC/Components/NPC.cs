namespace LablabBean.Plugins.NPC.Components;

/// <summary>
/// NPC component - represents a non-player character
/// </summary>
public struct NPC
{
    /// <summary>
    /// Unique identifier for the NPC
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of the NPC
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// NPC role/type (e.g., "QuestGiver", "Merchant", "Guard")
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// ID of the dialogue tree associated with this NPC
    /// </summary>
    public string? DialogueTreeId { get; set; }

    /// <summary>
    /// Whether the NPC can be interacted with
    /// </summary>
    public bool IsInteractable { get; set; }

    /// <summary>
    /// Custom state data for the NPC (JSON string)
    /// </summary>
    public string? StateData { get; set; }

    public NPC(string id, string name, string role, string? dialogueTreeId = null)
    {
        Id = id;
        Name = name;
        Role = role;
        DialogueTreeId = dialogueTreeId;
        IsInteractable = true;
        StateData = null;
    }
}
