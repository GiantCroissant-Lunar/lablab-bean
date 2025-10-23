namespace LablabBean.Contracts.Recording.Services;

/// <summary>
/// Service for recording terminal sessions using asciinema
/// </summary>
public interface IRecordingService
{
    /// <summary>
    /// Start recording a terminal session
    /// </summary>
    /// <param name="outputPath">Path where the recording will be saved</param>
    /// <param name="title">Optional title for the recording</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Recording session ID</returns>
    Task<string> StartRecordingAsync(string outputPath, string? title = null, CancellationToken ct = default);

    /// <summary>
    /// Stop the current recording session
    /// </summary>
    /// <param name="sessionId">Recording session ID</param>
    /// <param name="ct">Cancellation token</param>
    Task StopRecordingAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Check if a recording session is currently active
    /// </summary>
    /// <param name="sessionId">Recording session ID</param>
    /// <returns>True if recording is active</returns>
    bool IsRecording(string sessionId);

    /// <summary>
    /// Get all active recording sessions
    /// </summary>
    /// <returns>Collection of active session IDs</returns>
    IEnumerable<string> GetActiveSessions();

    /// <summary>
    /// Play back a recorded session
    /// </summary>
    /// <param name="recordingPath">Path to the recording file</param>
    /// <param name="speed">Playback speed multiplier (default: 1.0)</param>
    /// <param name="ct">Cancellation token</param>
    Task PlayRecordingAsync(string recordingPath, double speed = 1.0, CancellationToken ct = default);
}
