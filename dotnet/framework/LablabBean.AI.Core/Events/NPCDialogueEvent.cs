namespace LablabBean.AI.Core.Events;

/// <summary>
/// Event emitted when an NPC generates dialogue
/// </summary>
public class NPCDialogueEvent
{
    public string EntityId { get; set; } = string.Empty;
    public string TargetEntityId { get; set; } = string.Empty;
    public string DialogueText { get; set; } = string.Empty;
    public string EmotionalTone { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
