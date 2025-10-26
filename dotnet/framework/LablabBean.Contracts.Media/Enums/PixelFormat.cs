namespace LablabBean.Contracts.Media;

/// <summary>
/// Pixel format for video frame data
/// </summary>
public enum PixelFormat
{
    /// <summary>
    /// 24-bit RGB (8 bits per channel, no alpha)
    /// </summary>
    RGB24,

    /// <summary>
    /// 32-bit RGBA (8 bits per channel with alpha)
    /// </summary>
    RGBA32,

    /// <summary>
    /// 24-bit BGR (8 bits per channel, reverse order)
    /// </summary>
    BGR24,

    /// <summary>
    /// 32-bit BGRA (8 bits per channel with alpha, reverse order)
    /// </summary>
    BGRA32,

    /// <summary>
    /// 16-bit PCM audio samples (for audio frames)
    /// </summary>
    PCM16
}
