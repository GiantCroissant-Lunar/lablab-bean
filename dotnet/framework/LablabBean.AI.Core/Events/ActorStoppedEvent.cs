namespace LablabBean.AI.Core.Events;

/// <summary>
/// Event emitted when an actor is stopped
/// </summary>
public class ActorStoppedEvent
{
    public string ActorPath { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
