using System.Collections.Generic;

namespace LablabBean.Plugins.NPC.Data;

/// <summary>
/// Represents a player choice option in a dialogue node
/// </summary>
public class DialogueChoice
{
    /// <summary>
    /// Unique identifier for this choice (optional, for tracking)
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The text displayed to the player for this choice
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// ID of the next dialogue node to transition to when this choice is selected
    /// If null or empty, dialogue ends after this choice
    /// </summary>
    public string? NextNodeId { get; set; }

    /// <summary>
    /// Optional condition that must be met for this choice to be available
    /// Format: Simple DSL like "level >= 5", "hasItem('key')", "hasQuest('q1')"
    /// If null or empty, choice is always available
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Optional actions to execute when this choice is selected
    /// (e.g., AcceptQuest, GiveItem, SetNPCState)
    /// </summary>
    public List<DialogueAction> Actions { get; set; } = new();

    /// <summary>
    /// Optional flag indicating this choice ends the dialogue
    /// If true, dialogue ends even if NextNodeId is set
    /// </summary>
    public bool EndsDialogue { get; set; }

    /// <summary>
    /// Optional visual indicator for choice type (quest, trade, lore, goodbye)
    /// </summary>
    public string? ChoiceType { get; set; }

    /// <summary>
    /// Optional requirements description shown to player (e.g., "Requires: Level 5")
    /// </summary>
    public string? RequirementsText { get; set; }

    /// <summary>
    /// Checks if this choice has a condition that needs evaluation
    /// </summary>
    public bool HasCondition => !string.IsNullOrEmpty(Condition);

    /// <summary>
    /// Checks if this choice has any actions to execute
    /// </summary>
    public bool HasActions => Actions.Count > 0;
}
