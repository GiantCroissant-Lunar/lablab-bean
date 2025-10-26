using System.Reactive.Linq;
using System.Reactive.Subjects;
using LablabBean.Contracts.Media;
using LablabBean.Contracts.Media.DTOs;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.MediaPlayer.Core.Services;

/// <summary>
/// Core media playback service implementation.
/// Orchestrates playback engines, renderers, and state management.
/// </summary>
public class MediaService : IMediaService, IDisposable
{
    private readonly IEnumerable<IMediaPlaybackEngine> _engines;
    private readonly IEnumerable<IMediaRenderer> _renderers;
    private readonly ITerminalCapabilityDetector _terminalCapabilities;
    private readonly ILogger<MediaService> _logger;

    private readonly BehaviorSubject<PlaybackState> _playbackState;
    private readonly BehaviorSubject<TimeSpan> _position;
    private readonly BehaviorSubject<TimeSpan> _duration;
    private readonly BehaviorSubject<float> _volume;

    private IMediaPlaybackEngine? _activeEngine;
    private IMediaRenderer? _activeRenderer;
    private MediaInfo? _currentMedia;
    private CancellationTokenSource? _playbackCts;
    private Task? _playbackTask;

    private readonly object _stateLock = new();
    private PlaybackStatus _currentStatus = PlaybackStatus.Stopped;
    private TimeSpan _currentPosition = TimeSpan.Zero;
    private TimeSpan _currentDuration = TimeSpan.Zero;
    private float _currentVolume = 1.0f;

    public MediaService(
        IEnumerable<IMediaPlaybackEngine> engines,
        IEnumerable<IMediaRenderer> renderers,
        ITerminalCapabilityDetector terminalCapabilities,
        ILogger<MediaService> logger)
    {
        _engines = engines;
        _renderers = renderers;
        _terminalCapabilities = terminalCapabilities;
        _logger = logger;

        _playbackState = new BehaviorSubject<PlaybackState>(CreateCurrentState());
        _position = new BehaviorSubject<TimeSpan>(TimeSpan.Zero);
        _duration = new BehaviorSubject<TimeSpan>(TimeSpan.Zero);
        _volume = new BehaviorSubject<float>(1.0f);
    }

    public IObservable<PlaybackState> PlaybackState => _playbackState.AsObservable();
    public IObservable<TimeSpan> Position => _position.AsObservable();
    public IObservable<TimeSpan> Duration => _duration.AsObservable();
    public IObservable<float> Volume => _volume.AsObservable();

    public MediaInfo? CurrentMedia => _currentMedia;
    public IMediaRenderer? ActiveRenderer => _activeRenderer;

    public async Task<MediaInfo> LoadAsync(string path, CancellationToken ct = default)
    {
        _logger.LogInformation("Loading media: {Path}", path);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Media file not found: {path}", path);

        UpdateStatus(PlaybackStatus.Loading);

        try
        {
            // Stop any current playback
            await StopAsync(ct);

            // Select playback engine based on file extension
            var extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            _activeEngine = SelectEngine(extension);

            if (_activeEngine == null)
                throw new NotSupportedException($"No playback engine found for file type: {extension}");

            _logger.LogInformation("Selected engine: {EngineName}", _activeEngine.Name);

            // Open media and extract metadata
            _currentMedia = await _activeEngine.OpenAsync(path, ct);

            lock (_stateLock)
            {
                _currentDuration = _currentMedia.Duration;
            }
            _duration.OnNext(_currentMedia.Duration);

            // Select renderer based on media format and terminal capabilities
            _activeRenderer = await SelectRendererAsync(_currentMedia, ct);

            if (_activeRenderer == null)
                throw new InvalidOperationException("No suitable renderer found for this media");

            _logger.LogInformation("Selected renderer: {RendererName}", _activeRenderer.Name);

            // Initialize renderer
            var renderContext = await CreateRenderContextAsync(ct);
            await _activeRenderer.InitializeAsync(renderContext, ct);

            UpdateStatus(PlaybackStatus.Stopped);

            _logger.LogInformation("Media loaded successfully: {Duration}", _currentMedia.Duration);

            return _currentMedia;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load media: {Path}", path);
            UpdateStatus(PlaybackStatus.Error, ex.Message);
            throw;
        }
    }

    public async Task PlayAsync(CancellationToken ct = default)
    {
        if (_activeEngine == null || _currentMedia == null)
            throw new InvalidOperationException("No media loaded. Call LoadAsync first.");

        lock (_stateLock)
        {
            if (_currentStatus == PlaybackStatus.Playing)
                return; // Already playing
        }

        _logger.LogInformation("Starting playback");
        UpdateStatus(PlaybackStatus.Playing);

        // Start playback on background thread
        _playbackCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _playbackTask = Task.Run(() => PlaybackLoopAsync(_playbackCts.Token), _playbackCts.Token);

        await Task.CompletedTask;
    }

    public async Task PauseAsync(CancellationToken ct = default)
    {
        lock (_stateLock)
        {
            if (_currentStatus != PlaybackStatus.Playing)
                return; // Not playing
        }

        _logger.LogInformation("Pausing playback");

        // Stop playback loop
        _playbackCts?.Cancel();
        if (_playbackTask != null)
        {
            try
            {
                await _playbackTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        UpdateStatus(PlaybackStatus.Paused);
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Stopping playback");

        // Cancel playback loop
        _playbackCts?.Cancel();
        if (_playbackTask != null)
        {
            try
            {
                await _playbackTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        // Reset position
        lock (_stateLock)
        {
            _currentPosition = TimeSpan.Zero;
        }
        _position.OnNext(TimeSpan.Zero);

        // Cleanup renderer
        if (_activeRenderer != null)
        {
            await _activeRenderer.CleanupAsync(ct);
        }

        UpdateStatus(PlaybackStatus.Stopped);
    }

    public async Task SeekAsync(TimeSpan position, CancellationToken ct = default)
    {
        if (_activeEngine == null || _currentMedia == null)
            throw new InvalidOperationException("No media loaded");

        if (position < TimeSpan.Zero || position > _currentDuration)
            throw new ArgumentOutOfRangeException(nameof(position),
                $"Position must be between 0 and {_currentDuration}");

        _logger.LogInformation("Seeking to {Position}", position);

        var wasPlaying = false;
        lock (_stateLock)
        {
            wasPlaying = _currentStatus == PlaybackStatus.Playing;
        }

        // Pause if playing
        if (wasPlaying)
            await PauseAsync(ct);

        // Perform seek
        var actualPosition = await _activeEngine.SeekAsync(position, ct);

        lock (_stateLock)
        {
            _currentPosition = actualPosition;
        }
        _position.OnNext(actualPosition);

        // Resume if was playing
        if (wasPlaying)
            await PlayAsync(ct);
    }

    public async Task SetVolumeAsync(float volume, CancellationToken ct = default)
    {
        volume = Math.Clamp(volume, 0.0f, 1.0f);

        lock (_stateLock)
        {
            _currentVolume = volume;
        }

        _volume.OnNext(volume);
        _playbackState.OnNext(CreateCurrentState());

        _logger.LogDebug("Volume set to {Volume}", volume);

        await Task.CompletedTask;
    }

    private async Task PlaybackLoopAsync(CancellationToken ct)
    {
        if (_activeEngine == null || _activeRenderer == null || _currentMedia == null)
            return;

        try
        {
            var frameInterval = TimeSpan.FromSeconds(1.0 / 30.0); // 30 FPS target
            var positionUpdateInterval = TimeSpan.FromMilliseconds(100); // 10 Hz position updates
            var lastPositionUpdate = DateTime.UtcNow;

            while (!ct.IsCancellationRequested)
            {
                var frameStart = DateTime.UtcNow;

                try
                {
                    // Decode next frame
                    var frame = await _activeEngine.DecodeNextFrameAsync(ct);

                    // Update position
                    lock (_stateLock)
                    {
                        _currentPosition = frame.Timestamp;
                    }

                    // Emit position update at ~10 Hz
                    if (DateTime.UtcNow - lastPositionUpdate >= positionUpdateInterval)
                    {
                        _position.OnNext(frame.Timestamp);
                        lastPositionUpdate = DateTime.UtcNow;
                    }

                    // Render frame
                    await _activeRenderer.RenderFrameAsync(frame, ct);

                    // Frame rate pacing
                    var elapsed = DateTime.UtcNow - frameStart;
                    var delay = frameInterval - elapsed;
                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, ct);
                    }
                }
                catch (EndOfStreamException)
                {
                    _logger.LogInformation("Reached end of media");
                    break;
                }
            }

            // Playback finished
            await StopAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Expected during pause/stop
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during playback");
            UpdateStatus(PlaybackStatus.Error, ex.Message);
        }
    }

    private IMediaPlaybackEngine? SelectEngine(string extension)
    {
        return _engines
            .Where(e => e.SupportedFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            .OrderByDescending(e => e.Priority)
            .FirstOrDefault();
    }

    private async Task<IMediaRenderer?> SelectRendererAsync(MediaInfo media, CancellationToken ct)
    {
        var capabilities = _terminalCapabilities.DetectCapabilities();

        var suitableRenderers = new List<IMediaRenderer>();

        foreach (var renderer in _renderers)
        {
            if (await renderer.CanRenderAsync(media, capabilities, ct))
            {
                suitableRenderers.Add(renderer);
            }
        }

        return suitableRenderers
            .OrderByDescending(r => r.Priority)
            .FirstOrDefault();
    }

    private Task<RenderContext> CreateRenderContextAsync(CancellationToken ct)
    {
        var capabilities = _terminalCapabilities.DetectCapabilities();

        // TODO: Get actual viewport size from Terminal.Gui View
        var viewportSize = (Width: 80, Height: 24);

        var terminalInfoDict = new Dictionary<string, object>
        {
            ["TerminalType"] = capabilities.TerminalType,
            ["Capabilities"] = capabilities.Capabilities,
            ["Width"] = capabilities.Width,
            ["Height"] = capabilities.Height,
            ["SupportsColor"] = capabilities.SupportsColor,
            ["ColorCount"] = capabilities.ColorCount
        };

        return Task.FromResult(new RenderContext(
            TargetView: null!, // Will be set by view layer
            ViewportSize: viewportSize,
            TerminalInfo: terminalInfoDict
        ));
    }

    private void UpdateStatus(PlaybackStatus status, string? errorMessage = null)
    {
        lock (_stateLock)
        {
            _currentStatus = status;
        }

        _playbackState.OnNext(CreateCurrentState(errorMessage));
    }

    private PlaybackState CreateCurrentState(string? errorMessage = null)
    {
        lock (_stateLock)
        {
            return new PlaybackState(
                Status: _currentStatus,
                Position: _currentPosition,
                Duration: _currentDuration,
                Volume: _currentVolume,
                CurrentMedia: _currentMedia,
                ActivePlaylist: null, // Playlist support in later phase
                ErrorMessage: errorMessage
            );
        }
    }

    public void Dispose()
    {
        _playbackCts?.Cancel();
        _playbackCts?.Dispose();

        _playbackState.Dispose();
        _position.Dispose();
        _duration.Dispose();
        _volume.Dispose();
    }
}
