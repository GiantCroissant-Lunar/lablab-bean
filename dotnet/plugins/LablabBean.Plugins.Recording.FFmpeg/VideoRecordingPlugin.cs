using LablabBean.Contracts.Game.Events;
using LablabBean.Contracts.Recording.Events;
using LablabBean.Contracts.Recording.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Recording.FFmpeg.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Recording.FFmpeg;

/// <summary>
/// Video recording plugin that provides FFmpeg-based screen recording capabilities.
/// Integrates with game events to automatically record gameplay sessions as video.
/// </summary>
public class VideoRecordingPlugin : IPlugin
{
    private ILogger? _logger;
    private IEventBus? _eventBus;
    private FFmpegVideoRecordingService? _recordingService;
    private string? _currentGameSession;

    public string Id => "lablab-bean.recording.ffmpeg";
    public string Name => "FFmpeg Video Recording Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _eventBus = context.Registry.Get<IEventBus>();

        // Register video recording service
        _recordingService = new FFmpegVideoRecordingService(context.Logger);
        context.Registry.Register<LablabBean.Contracts.Recording.Services.IService>(
            _recordingService,
            new ServiceMetadata
            {
                Priority = 90, // Lower priority than asciinema to avoid conflicts
                Name = "FFmpegVideoRecordingService",
                Version = "1.0.0"
            }
        );

        // Subscribe to game events for automatic recording
        _eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
        _eventBus.Subscribe<GameEndedEvent>(OnGameEnded);

        _logger.LogInformation("Video recording plugin initialized - FFmpeg service registered");
        return Task.CompletedTask;
    }

    private async Task OnGameStarted(GameStartedEvent evt)
    {
        try
        {
            // Create recordings directory if it doesn't exist
            var recordingsDir = Path.Combine(Environment.CurrentDirectory, "recordings", "video");
            Directory.CreateDirectory(recordingsDir);

            // Generate recording filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"game_session_{timestamp}.mp4";
            var outputPath = Path.Combine(recordingsDir, filename);

            // Start video recording
            _currentGameSession = await _recordingService!.StartRecordingAsync(
                outputPath,
                $"Game Session Video - {evt.GameMode}",
                CancellationToken.None);

            _logger?.LogInformation(
                "Auto-started video recording for game session {GameId} -> {SessionId} ({OutputPath})",
                evt.GameId, _currentGameSession, outputPath);

            // Publish recording started event
            await _eventBus!.PublishAsync(new RecordingStartedEvent
            {
                SessionId = _currentGameSession,
                OutputPath = outputPath,
                Title = $"Game Session Video - {evt.GameMode}"
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to auto-start video recording for game session {GameId}", evt.GameId);
        }
    }

    private async Task OnGameEnded(GameEndedEvent evt)
    {
        if (string.IsNullOrEmpty(_currentGameSession))
        {
            return;
        }

        try
        {
            var startTime = DateTime.UtcNow; // We should track this properly
            await _recordingService!.StopRecordingAsync(_currentGameSession, CancellationToken.None);

            _logger?.LogInformation(
                "Auto-stopped video recording for game session {GameId} -> {SessionId}",
                evt.GameId, _currentGameSession);

            // Publish recording stopped event
            await _eventBus!.PublishAsync(new RecordingStoppedEvent
            {
                SessionId = _currentGameSession,
                OutputPath = "", // We should track this properly
                Duration = DateTime.UtcNow - startTime,
                WasSuccessful = true
            });

            _currentGameSession = null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to auto-stop video recording for game session {GameId}", evt.GameId);
        }
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Video recording plugin started - ready to record screen sessions");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        // Stop any active recording sessions
        if (!string.IsNullOrEmpty(_currentGameSession))
        {
            try
            {
                await _recordingService!.StopRecordingAsync(_currentGameSession, ct);
                _logger?.LogInformation("Stopped active video recording session during plugin shutdown");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error stopping video recording session during shutdown");
            }
        }

        _logger?.LogInformation("Video recording plugin stopped");
    }
}
