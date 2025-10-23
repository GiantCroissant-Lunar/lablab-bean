using LablabBean.Contracts.Game.Events;
using LablabBean.Contracts.Recording.Events;
using LablabBean.Contracts.Recording.Services;
using LablabBean.Contracts.Video.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Recording.Video.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Recording.Video;

/// <summary>
/// Recording plugin that uses the video service for actual recording operations.
/// This plugin focuses on recording workflow management and game event integration,
/// while delegating video processing to the video service.
/// </summary>
public class VideoRecordingPlugin : IPlugin
{
    private ILogger? _logger;
    private IEventBus? _eventBus;
    private VideoRecordingService? _recordingService;
    private IVideoRecordingService? _videoRecordingService;
    private IVideoService? _videoService;
    private string? _currentGameSession;

    public string Id => "lablab-bean.recording.video";
    public string Name => "Video Recording Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _eventBus = context.Registry.Get<IEventBus>();

        // Get video services from registry (these should be registered by the video plugin)
        _videoRecordingService = context.Registry.Get<IVideoRecordingService>();
        _videoService = context.Registry.Get<IVideoService>();

        if (_videoRecordingService == null)
        {
            throw new InvalidOperationException(
                "IVideoRecordingService not found in registry. Ensure the video plugin is loaded first.");
        }

        if (_videoService == null)
        {
            throw new InvalidOperationException(
                "IVideoService not found in registry. Ensure the video plugin is loaded first.");
        }

        // Create our recording service that uses the video services
        _recordingService = new VideoRecordingService(
            context.Logger,
            _videoRecordingService,
            _videoService);

        // Register our recording service
        context.Registry.Register<LablabBean.Contracts.Recording.Services.IService>(
            _recordingService,
            new ServiceMetadata
            {
                Priority = 95, // Higher priority than direct video service adapter
                Name = "VideoRecordingService",
                Version = "1.0.0"
            }
        );

        // Also register as IRecordingService for direct access
        context.Registry.Register<IRecordingService>(
            _recordingService,
            new ServiceMetadata
            {
                Priority = 95,
                Name = "VideoRecordingService",
                Version = "1.0.0"
            }
        );

        // Subscribe to game events for automatic recording
        _eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
        _eventBus.Subscribe<GameEndedEvent>(OnGameEnded);

        _logger.LogInformation("Video recording plugin initialized - using video service for recording operations");
        return Task.CompletedTask;
    }

    private async Task OnGameStarted(GameStartedEvent evt)
    {
        try
        {
            // Create recordings directory
            var recordingsDir = Path.Combine(Environment.CurrentDirectory, "recordings", "video");
            Directory.CreateDirectory(recordingsDir);

            // Generate recording filename
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"game_session_{timestamp}.mp4";
            var outputPath = Path.Combine(recordingsDir, filename);

            // Start recording using our service (which uses the video service)
            _currentGameSession = await _recordingService!.StartGameRecordingAsync(
                outputPath,
                $"Game Session - {evt.GameMode}");

            _logger?.LogInformation(
                "Auto-started video recording for game session {GameId} -> {SessionId} ({OutputPath})",
                evt.GameId, _currentGameSession, outputPath);

            // Publish recording started event
            await _eventBus!.PublishAsync(new RecordingStartedEvent
            {
                SessionId = _currentGameSession,
                OutputPath = outputPath,
                Title = $"Game Session - {evt.GameMode}"
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
            var startTime = DateTime.UtcNow; // Should track this properly
            await _recordingService!.StopRecordingAsync(_currentGameSession);

            _logger?.LogInformation(
                "Auto-stopped video recording for game session {GameId} -> {SessionId}",
                evt.GameId, _currentGameSession);

            // Publish recording stopped event
            await _eventBus!.PublishAsync(new RecordingStoppedEvent
            {
                SessionId = _currentGameSession,
                OutputPath = "", // Should track this properly
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
        _logger?.LogInformation("Video recording plugin started - ready to record using video service");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        // Stop any active recording
        if (!string.IsNullOrEmpty(_currentGameSession))
        {
            try
            {
                await _recordingService!.StopRecordingAsync(_currentGameSession, ct);
                _logger?.LogInformation("Stopped active recording session during plugin shutdown");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error stopping recording session during shutdown");
            }
        }

        _logger?.LogInformation("Video recording plugin stopped");
    }
}
