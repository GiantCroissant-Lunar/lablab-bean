namespace LablabBean.Contracts.Media;

/// <summary>
/// Current state of media playback
/// </summary>
public enum PlaybackStatus
{
    /// <summary>
    /// No media loaded or playback stopped
    /// </summary>
    Stopped,

    /// <summary>
    /// Media is loading (file I/O in progress)
    /// </summary>
    Loading,

    /// <summary>
    /// Media is currently playing
    /// </summary>
    Playing,

    /// <summary>
    /// Playback paused at current position
    /// </summary>
    Paused,

    /// <summary>
    /// Buffering media data (temporary pause)
    /// </summary>
    Buffering,

    /// <summary>
    /// Error occurred during playback
    /// </summary>
    Error
}
