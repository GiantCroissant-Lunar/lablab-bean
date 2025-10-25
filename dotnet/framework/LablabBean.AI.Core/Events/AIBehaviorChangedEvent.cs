namespace LablabBean.AI.Core.Events;

/// <summary>
/// Event emitted when an AI avatar changes behavior
/// </summary>
public class AIBehaviorChangedEvent
{
    public string EntityId { get; set; } = string.Empty;
    public string PreviousBehavior { get; set; } = string.Empty;
    public string NewBehavior { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
