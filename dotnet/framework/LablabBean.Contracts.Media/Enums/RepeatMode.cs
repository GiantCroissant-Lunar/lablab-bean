namespace LablabBean.Contracts.Media;

/// <summary>
/// Playlist repeat behavior
/// </summary>
public enum RepeatMode
{
    /// <summary>
    /// Play through playlist once, then stop
    /// </summary>
    Off,

    /// <summary>
    /// Repeat current media item indefinitely
    /// </summary>
    Single,

    /// <summary>
    /// Repeat entire playlist indefinitely
    /// </summary>
    All
}
