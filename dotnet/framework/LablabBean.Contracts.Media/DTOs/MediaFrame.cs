namespace LablabBean.Contracts.Media.DTOs;

/// <summary>
/// Decoded media frame data
/// </summary>
/// <param name="Data">Raw frame data bytes</param>
/// <param name="Timestamp">Presentation timestamp relative to media start</param>
/// <param name="Type">Frame type (Video or Audio)</param>
/// <param name="Width">Frame width in pixels (0 for audio frames)</param>
/// <param name="Height">Frame height in pixels (0 for audio frames)</param>
/// <param name="PixelFormat">Pixel format for video frames</param>
public record MediaFrame(
    byte[] Data,
    TimeSpan Timestamp,
    FrameType Type,
    int Width,
    int Height,
    PixelFormat PixelFormat
);
