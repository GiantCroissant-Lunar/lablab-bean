namespace LablabBean.Contracts.Media.DTOs;

/// <summary>
/// Immutable media file metadata and stream information
/// </summary>
/// <param name="Path">Absolute file path</param>
/// <param name="Format">Media format type (Audio, Video, or Both)</param>
/// <param name="Duration">Total duration (TimeSpan.Zero if unknown)</param>
/// <param name="Video">Video stream info (null for audio-only files)</param>
/// <param name="Audio">Audio stream info (null for video-only files without audio)</param>
/// <param name="Metadata">Additional metadata (title, artist, etc.)</param>
public record MediaInfo(
    string Path,
    MediaFormat Format,
    TimeSpan Duration,
    VideoInfo? Video,
    AudioInfo? Audio,
    IReadOnlyDictionary<string, string> Metadata
);
