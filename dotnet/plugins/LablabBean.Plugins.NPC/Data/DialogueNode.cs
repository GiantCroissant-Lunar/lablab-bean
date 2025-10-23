using System.Collections.Generic;

namespace LablabBean.Plugins.NPC.Data;

/// <summary>
/// Represents a single node in a dialogue tree
/// Contains NPC text and player choice options
/// </summary>
public class DialogueNode
{
    /// <summary>
    /// Unique identifier for this node within the dialogue tree
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the NPC speaking (for display purposes)
    /// </summary>
    public string NpcName { get; set; } = string.Empty;

    /// <summary>
    /// The text that the NPC says at this node
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Optional emotion/expression for the NPC (happy, sad, angry, neutral)
    /// Can be used for avatar/portrait changes in UI
    /// </summary>
    public string? Emotion { get; set; }

    /// <summary>
    /// Available player response choices at this node
    /// Empty list means this is an end node (dialogue concludes)
    /// </summary>
    public List<DialogueChoice> Choices { get; set; } = new();

    /// <summary>
    /// Optional actions to execute when entering this node
    /// (e.g., set state, give item)
    /// </summary>
    public List<DialogueAction> OnEnterActions { get; set; } = new();

    /// <summary>
    /// Optional tags for categorizing nodes (e.g., "quest", "lore", "trade")
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Checks if this is an end node (no choices available)
    /// </summary>
    public bool IsEndNode => Choices.Count == 0;

    /// <summary>
    /// Gets all available choices (filtering out those with unmet conditions happens at runtime)
    /// </summary>
    public List<DialogueChoice> GetAvailableChoices()
    {
        return new List<DialogueChoice>(Choices);
    }
}
