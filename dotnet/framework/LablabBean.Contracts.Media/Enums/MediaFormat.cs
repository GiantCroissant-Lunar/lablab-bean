namespace LablabBean.Contracts.Media;

/// <summary>
/// Media format classification
/// </summary>
public enum MediaFormat
{
    /// <summary>
    /// Audio-only file (mp3, wav, flac, etc.)
    /// </summary>
    Audio,

    /// <summary>
    /// Video-only file (no audio track)
    /// </summary>
    Video,

    /// <summary>
    /// File with both video and audio tracks
    /// </summary>
    Both
}
