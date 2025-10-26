namespace LablabBean.Contracts.Media.DTOs;

/// <summary>
/// Video stream metadata
/// </summary>
/// <param name="Width">Frame width in pixels</param>
/// <param name="Height">Frame height in pixels</param>
/// <param name="FrameRate">Frames per second</param>
/// <param name="Codec">Video codec name (e.g., "h264", "vp9")</param>
/// <param name="BitRate">Average bit rate in bits/second (0 if unknown)</param>
public record VideoInfo(
    int Width,
    int Height,
    double FrameRate,
    string Codec,
    long BitRate
);
