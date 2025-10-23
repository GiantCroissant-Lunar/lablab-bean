using LablabBean.Contracts.Game.Events;
using LablabBean.Contracts.Recording.Events;
using LablabBean.Contracts.Recording.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Recording.Asciinema.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Recording.Asciinema;

/// <summary>
/// Recording plugin that provides asciinema-based terminal recording capabilities.
/// Integrates with game events to automatically record gameplay sessions.
/// </summary>
public class RecordingPlugin : IPlugin
{
    private ILogger? _logger;
    private IEventBus? _eventBus;
    private AsciinemaRecordingService? _recordingService;
    private string? _currentGameSession;

    public string Id => "lablab-bean.recording.asciinema";
    public string Name => "Asciinema Recording Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _eventBus = context.Registry.Get<IEventBus>();

        // Register recording service
        _recordingService = new AsciinemaRecordingService(context.Logger);
        context.Registry.Register<LablabBean.Contracts.Recording.Services.IService>(
            _recordingService,
            new ServiceMetadata
            {
                Priority = 100,
                Name = "AsciinemaRecordingService",
                Version = "1.0.0"
            }
        );

        // Subscribe to game events for automatic recording
        _eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
        _eventBus.Subscribe<GameEndedEvent>(OnGameEnded);

        _logger.LogInformation("Recording plugin initialized - asciinema service registered");
        return Task.CompletedTask;
    }

    private async Task OnGameStarted(GameStartedEvent evt)
    {
        try
        {
            // Create recordings directory if it doesn't exist
            var recordingsDir = Path.Combine(Environment.CurrentDirectory, "recordings");
            Directory.CreateDirectory(recordingsDir);

            // Generate recording filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"game_session_{timestamp}.cast";
            var outputPath = Path.Combine(recordingsDir, filename);

            // Start recording
            _currentGameSession = await _recordingService!.StartRecordingAsync(
                outputPath,
                $"Game Session - {evt.GameMode}",
                CancellationToken.None);

            _logger?.LogInformation(
                "Auto-started recording for game session {GameId} -> {SessionId} ({OutputPath})",
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
            _logger?.LogError(ex, "Failed to auto-start recording for game session {GameId}", evt.GameId);
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
                "Auto-stopped recording for game session {GameId} -> {SessionId}",
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
            _logger?.LogError(ex, "Failed to auto-stop recording for game session {GameId}", evt.GameId);
        }
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Recording plugin started - ready to record terminal sessions");
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
                _logger?.LogInformation("Stopped active recording session during plugin shutdown");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error stopping recording session during shutdown");
            }
        }

        _logger?.LogInformation("Recording plugin stopped");
    }
}
