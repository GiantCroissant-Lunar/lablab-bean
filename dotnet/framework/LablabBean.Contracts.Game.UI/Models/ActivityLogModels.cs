namespace LablabBean.Contracts.Game.UI.Models;

public sealed class ActivityEntryDto
{
    public DateTimeOffset Timestamp { get; init; }
    public string Message { get; init; } = string.Empty;
    public ActivitySeverity Severity { get; init; }
    public ActivityCategory Category { get; init; }
    public int? OriginEntityId { get; init; }
    public string[]? Tags { get; init; }
    public System.Collections.Generic.Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Suggested icon glyph for rendering this entry (e.g., "⚔", "+", "!", "×")
    /// </summary>
    public string Icon { get; init; } = "·";

    /// <summary>
    /// Suggested color name for rendering this entry (e.g., "Red", "Green", "Yellow", "White")
    /// </summary>
    public string Color { get; init; } = "White";
}
