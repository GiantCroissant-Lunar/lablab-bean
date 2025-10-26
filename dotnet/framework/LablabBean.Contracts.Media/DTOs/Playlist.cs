namespace LablabBean.Contracts.Media.DTOs;

/// <summary>
/// Playlist data structure with playback metadata
/// </summary>
public class Playlist
{
    /// <summary>
    /// Unique playlist identifier
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Ordered list of media file paths
    /// </summary>
    public required List<string> Items { get; init; }

    /// <summary>
    /// Current playback index (0-based, -1 if none selected)
    /// </summary>
    public int CurrentIndex { get; set; } = -1;

    /// <summary>
    /// Shuffle mode enabled
    /// </summary>
    public bool ShuffleEnabled { get; set; }

    /// <summary>
    /// Repeat mode
    /// </summary>
    public RepeatMode RepeatMode { get; set; } = RepeatMode.Off;

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Get current media path (null if CurrentIndex is invalid)
    /// </summary>
    public string? CurrentItem => CurrentIndex >= 0 && CurrentIndex < Items.Count
        ? Items[CurrentIndex]
        : null;
}
