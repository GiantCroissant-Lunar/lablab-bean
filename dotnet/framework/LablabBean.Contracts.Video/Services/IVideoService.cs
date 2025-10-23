using LablabBean.Contracts.Video.Models;

namespace LablabBean.Contracts.Video.Services;

/// <summary>
/// Core video service interface for video operations
/// </summary>
public interface IVideoService
{
    /// <summary>
    /// Play a video file
    /// </summary>
    /// <param name="videoPath">Path to the video file</param>
    /// <param name="options">Playback options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Playback session ID</returns>
    Task<string> PlayVideoAsync(string videoPath, VideoPlaybackOptions? options = null, CancellationToken ct = default);

    /// <summary>
    /// Stop video playback
    /// </summary>
    /// <param name="sessionId">Playback session ID</param>
    /// <param name="ct">Cancellation token</param>
    Task StopPlaybackAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Check if video is currently playing
    /// </summary>
    /// <param name="sessionId">Playback session ID</param>
    /// <returns>True if playing</returns>
    bool IsPlaying(string sessionId);

    /// <summary>
    /// Get video information
    /// </summary>
    /// <param name="videoPath">Path to the video file</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Video information</returns>
    Task<VideoInfo> GetVideoInfoAsync(string videoPath, CancellationToken ct = default);

    /// <summary>
    /// Convert video to different format
    /// </summary>
    /// <param name="inputPath">Input video path</param>
    /// <param name="outputPath">Output video path</param>
    /// <param name="options">Conversion options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Conversion session ID</returns>
    Task<string> ConvertVideoAsync(string inputPath, string outputPath, VideoConversionOptions? options = null, CancellationToken ct = default);

    /// <summary>
    /// Get all active video sessions (playback, conversion, etc.)
    /// </summary>
    /// <returns>Collection of active session IDs</returns>
    IEnumerable<string> GetActiveSessions();
}

/// <summary>
/// Video recording service interface
/// </summary>
public interface IVideoRecordingService
{
    /// <summary>
    /// Start recording video
    /// </summary>
    /// <param name="outputPath">Output file path</param>
    /// <param name="options">Recording options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Recording session ID</returns>
    Task<string> StartRecordingAsync(string outputPath, VideoRecordingOptions? options = null, CancellationToken ct = default);

    /// <summary>
    /// Stop video recording
    /// </summary>
    /// <param name="sessionId">Recording session ID</param>
    /// <param name="ct">Cancellation token</param>
    Task StopRecordingAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Check if recording is active
    /// </summary>
    /// <param name="sessionId">Recording session ID</param>
    /// <returns>True if recording</returns>
    bool IsRecording(string sessionId);

    /// <summary>
    /// Get all active recording sessions
    /// </summary>
    /// <returns>Collection of active recording session IDs</returns>
    IEnumerable<string> GetActiveRecordingSessions();
}
