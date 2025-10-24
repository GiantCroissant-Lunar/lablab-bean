using SadRogue.Primitives;

namespace LablabBean.Contracts.UI.Models;

public enum ActivitySeverity
{
    Info,
    Success,
    Warning,
    Error,
    Combat,
    Loot,
    System
}

public enum ActivityCategory
{
    System,
    Combat,
    Movement,
    Items,
    Level,
    Quest,
    Dialogue,
    Analytics,
    UI,
    Misc
}

public readonly struct ActivityEntry
{
    public DateTimeOffset Timestamp { get; }
    public string Message { get; }
    public ActivitySeverity Severity { get; }
    public ActivityCategory Category { get; }
    public int? OriginEntityId { get; }
    public Point? Position { get; }
    public string[]? Tags { get; }
    public char? IconGlyph { get; }
    public Color? IconColor { get; }
    public System.Collections.Generic.Dictionary<string, object>? Metadata { get; }

    public ActivityEntry(
        string message,
        ActivitySeverity severity,
        DateTimeOffset? timestamp = null,
        ActivityCategory category = ActivityCategory.System,
        int? originEntityId = null,
        Point? position = null,
        string[]? tags = null,
        char? iconGlyph = null,
        Color? iconColor = null,
        System.Collections.Generic.Dictionary<string, object>? metadata = null)
    {
        Message = message;
        Severity = severity;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        Category = category;
        OriginEntityId = originEntityId;
        Position = position;
        Tags = tags;
        IconGlyph = iconGlyph;
        IconColor = iconColor;
        Metadata = metadata;
    }
}
