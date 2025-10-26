namespace LablabBean.Contracts.Media;

/// <summary>
/// Type of decoded media frame
/// </summary>
public enum FrameType
{
    /// <summary>
    /// Video frame (image data)
    /// </summary>
    Video,

    /// <summary>
    /// Audio frame (PCM samples)
    /// </summary>
    Audio
}
