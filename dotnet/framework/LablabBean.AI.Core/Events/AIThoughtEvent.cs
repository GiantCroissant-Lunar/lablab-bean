namespace LablabBean.AI.Core.Events;

/// <summary>
/// Event emitted when an AI avatar generates a thought or internal reasoning
/// </summary>
public class AIThoughtEvent
{
    public string EntityId { get; set; } = string.Empty;
    public string Thought { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
