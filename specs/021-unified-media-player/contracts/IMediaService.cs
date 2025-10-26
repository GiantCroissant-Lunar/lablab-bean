/// <summary>
/// Core media playback service interface for unified audio/video player.
/// Orchestrates media loading, playback control, and renderer selection.
/// </summary>
/// <remarks>
/// This service is technology-agnostic and relies on plugin implementations of
/// IMediaPlaybackEngine (decoding) and IMediaRenderer (rendering).
///
/// Usage Example:
/// <code>
/// var mediaService = serviceProvider.GetRequiredService&lt;IMediaService&gt;();
///
/// // Load media file
/// var mediaInfo = await mediaService.LoadAsync("video.mp4");
///
/// // Subscribe to state changes
/// mediaService.PlaybackState.Subscribe(state => Console.WriteLine($"State: {state}"));
///
/// // Start playback
/// await mediaService.PlayAsync();
///
/// // Seek to position
/// await mediaService.SeekAsync(TimeSpan.FromSeconds(30));
///
/// // Stop playback
/// await mediaService.StopAsync();
/// </code>
/// </remarks>
public interface IMediaService
{
    /// <summary>
    /// Observable stream of playback state changes (Stopped, Playing, Paused, etc.)
    /// </summary>
    /// <remarks>
    /// Emits new state whenever playback status changes. Subscribers can react
    /// to state changes for UI updates or business logic.
    /// Thread-safe: can be observed from any thread.
    /// </remarks>
    IObservable<PlaybackState> PlaybackState { get; }

    /// <summary>
    /// Observable stream of current playback position updates
    /// </summary>
    /// <remarks>
    /// Emits position updates at ~10 Hz during playback.
    /// Position resets to 00:00:00 when media stops.
    /// </remarks>
    IObservable<TimeSpan> Position { get; }

    /// <summary>
    /// Observable stream of total media duration
    /// </summary>
    /// <remarks>
    /// Emits once after media is loaded. Duration is 00:00:00 if unknown.
    /// For live streams, duration may be TimeSpan.MaxValue.
    /// </remarks>
    IObservable<TimeSpan> Duration { get; }

    /// <summary>
    /// Observable stream of volume level changes (0.0 = mute, 1.0 = max)
    /// </summary>
    IObservable<float> Volume { get; }

    /// <summary>
    /// Load a media file and prepare for playback
    /// </summary>
    /// <param name="path">Absolute path to media file</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Media metadata including duration, format, and stream info</returns>
    /// <exception cref="FileNotFoundException">File does not exist</exception>
    /// <exception cref="NotSupportedException">File format not supported</exception>
    /// <exception cref="InvalidOperationException">File is corrupted or unreadable</exception>
    /// <remarks>
    /// This method:
    /// 1. Opens the file and extracts metadata via IMediaPlaybackEngine
    /// 2. Detects terminal capabilities via ITerminalCapabilityDetector
    /// 3. Selects optimal renderer based on format + terminal capabilities
    /// 4. Transitions state to Stopped (ready to play)
    ///
    /// Does NOT start playback automatically. Call PlayAsync() after loading.
    /// </remarks>
    Task<MediaInfo> LoadAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Start or resume media playback
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="InvalidOperationException">No media loaded (call LoadAsync first)</exception>
    /// <remarks>
    /// Starts decoding frames on a background thread and rendering to the active view.
    /// If already playing, this method is idempotent (no effect).
    /// If paused, resumes from current position.
    /// </remarks>
    Task PlayAsync(CancellationToken ct = default);

    /// <summary>
    /// Pause media playback at current position
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Not currently playing</exception>
    /// <remarks>
    /// Pauses decoding but maintains current position. Call PlayAsync() to resume.
    /// If already paused, this method is idempotent.
    /// </remarks>
    Task PauseAsync(CancellationToken ct = default);

    /// <summary>
    /// Stop playback and reset position to beginning
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <remarks>
    /// Stops decoding, releases frame buffers, and resets position to 00:00:00.
    /// Media remains loaded (can call PlayAsync() to restart from beginning).
    /// Safe to call from any state (idempotent if already stopped).
    /// </remarks>
    Task StopAsync(CancellationToken ct = default);

    /// <summary>
    /// Seek to a specific position in the media
    /// </summary>
    /// <param name="position">Target position (must be >= 0 and <= Duration)</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="ArgumentOutOfRangeException">Position is negative or beyond duration</exception>
    /// <exception cref="NotSupportedException">Seeking not supported for this media</exception>
    /// <remarks>
    /// Seeks to the nearest keyframe at or before the requested position.
    /// Actual position may differ by up to 1-2 seconds for video files with infrequent keyframes.
    /// Playback state is preserved (playing stays playing, paused stays paused).
    /// </remarks>
    Task SeekAsync(TimeSpan position, CancellationToken ct = default);

    /// <summary>
    /// Set playback volume level
    /// </summary>
    /// <param name="volume">Volume level (0.0 = mute, 1.0 = maximum, clamped to range)</param>
    /// <param name="ct">Cancellation token</param>
    /// <remarks>
    /// Volume changes take effect immediately during playback.
    /// Volume setting persists across media files (stored in user preferences).
    /// </remarks>
    Task SetVolumeAsync(float volume, CancellationToken ct = default);

    /// <summary>
    /// Currently loaded media metadata (null if no media loaded)
    /// </summary>
    MediaInfo? CurrentMedia { get; }

    /// <summary>
    /// Currently active renderer (null if no media loaded)
    /// </summary>
    /// <remarks>
    /// Renderer is selected automatically during LoadAsync() based on:
    /// 1. Media format (audio vs video)
    /// 2. Terminal capabilities (SIXEL, Kitty, braille)
    /// 3. Renderer priority (highest priority wins)
    /// </remarks>
    IMediaRenderer? ActiveRenderer { get; }
}
