using LablabBean.Contracts.Game.Events;
using LablabBean.Contracts.Recording.Events;
using LablabBean.Contracts.Video.Events;
using LablabBean.Contracts.Video.Models;
using LablabBean.Contracts.Video.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Video.FFmpeg.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Video.FFmpeg;

/// <summary>
/// Comprehensive FFmpeg video plugin providing recording, playback, and conversion capabilities
/// </summary>
public class FFmpegVideoPlugin : IPlugin
{
    private ILogger? _logger;
    private IEventBus? _eventBus;
    private FFmpegVideoService? _videoService;
    private string? _currentRecordingSession;

    public string Id => "lablab-bean.video.ffmpeg";
    public string Name => "FFmpeg Video Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _eventBus = context.Registry.Get<IEventBus>();

        // Create the unified video service
        _videoService = new FFmpegVideoService(context.Logger);

        // Register as both video service and recording service
        context.Registry.Register<IVideoService>(
            _videoService,
            new ServiceMetadata
            {
                Priority = 100,
                Name = "FFmpegVideoService",
                Version = "1.0.0"
            }
        );

        context.Registry.Register<IVideoRecordingService>(
            _videoService,
            new ServiceMetadata
            {
                Priority = 100,
                Name = "FFmpegVideoRecordingService",
                Version = "1.0.0"
            }
        );

        // Also register for backward compatibility with recording contracts
        context.Registry.Register<LablabBean.Contracts.Recording.Services.IService>(
            new RecordingServiceAdapter(_videoService, context.Logger),
            new ServiceMetadata
            {
                Priority = 85, // Lower than dedicated recording services
                Name = "FFmpegRecordingAdapter",
                Version = "1.0.0"
            }
        );

        // Subscribe to game events for automatic recording
        _eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
        _eventBus.Subscribe<GameEndedEvent>(OnGameEnded);

        _logger.LogInformation("FFmpeg video plugin initialized - video and recording services registered");
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

            // Configure recording options
            var options = new VideoRecordingOptions
            {
                Title = $"Game Session - {evt.GameMode}",
                FrameRate = 30,
                Quality = 23,
                Preset = "fast",
                Source = VideoRecordingSource.Screen,
                MaxDurationSeconds = 3600 // 1 hour max
            };

            // Start recording
            _currentRecordingSession = await _videoService!.StartRecordingAsync(outputPath, options);

            _logger?.LogInformation(
                "Auto-started video recording for game session {GameId} -> {SessionId} ({OutputPath})",
                evt.GameId, _currentRecordingSession, outputPath);

            // Publish events
            await _eventBus!.PublishAsync(new VideoRecordingStartedEvent
            {
                SessionId = _currentRecordingSession,
                OutputPath = outputPath,
                Options = options
            });

            await _eventBus.PublishAsync(new RecordingStartedEvent
            {
                SessionId = _currentRecordingSession,
                OutputPath = outputPath,
                Title = options.Title
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to auto-start video recording for game session {GameId}", evt.GameId);
        }
    }

    private async Task OnGameEnded(GameEndedEvent evt)
    {
        if (string.IsNullOrEmpty(_currentRecordingSession))
        {
            return;
        }

        try
        {
            var startTime = DateTime.UtcNow; // Should track this properly
            await _videoService!.StopRecordingAsync(_currentRecordingSession);

            _logger?.LogInformation(
                "Auto-stopped video recording for game session {GameId} -> {SessionId}",
                evt.GameId, _currentRecordingSession);

            // Publish events
            await _eventBus!.PublishAsync(new VideoRecordingStoppedEvent
            {
                SessionId = _currentRecordingSession,
                OutputPath = "", // Should track this properly
                Duration = DateTime.UtcNow - startTime,
                WasSuccessful = true
            });

            await _eventBus.PublishAsync(new RecordingStoppedEvent
            {
                SessionId = _currentRecordingSession,
                OutputPath = "",
                Duration = DateTime.UtcNow - startTime,
                WasSuccessful = true
            });

            _currentRecordingSession = null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to auto-stop video recording for game session {GameId}", evt.GameId);
        }
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("FFmpeg video plugin started - ready for video operations");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        // Stop any active recording
        if (!string.IsNullOrEmpty(_currentRecordingSession))
        {
            try
            {
                await _videoService!.StopRecordingAsync(_currentRecordingSession, ct);
                _logger?.LogInformation("Stopped active recording session during plugin shutdown");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error stopping recording session during shutdown");
            }
        }

        _logger?.LogInformation("FFmpeg video plugin stopped");
    }
}

/// <summary>
/// Adapter to provide backward compatibility with the recording service interface
/// </summary>
internal class RecordingServiceAdapter : LablabBean.Contracts.Recording.Services.IService
{
    private readonly IVideoRecordingService _videoRecordingService;
    private readonly IVideoService _videoService;
    private readonly ILogger _logger;

    public RecordingServiceAdapter(FFmpegVideoService videoService, ILogger logger)
    {
        _videoRecordingService = videoService;
        _videoService = videoService;
        _logger = logger;
    }

    public TResult ExecuteAction<TResult>(string actionName, params object[] parameters)
    {
        return actionName.ToLowerInvariant() switch
        {
            "startrecording" when parameters.Length >= 1 =>
                (TResult)(object)_videoRecordingService.StartRecordingAsync(
                    parameters[0].ToString()!,
                    parameters.Length > 1 ? new VideoRecordingOptions { Title = parameters[1]?.ToString() } : null
                ).Result,
            "isrecording" when parameters.Length >= 1 =>
                (TResult)(object)_videoRecordingService.IsRecording(parameters[0].ToString()!),
            "getactivesessions" =>
                (TResult)(object)_videoRecordingService.GetActiveRecordingSessions().ToArray(),
            "getvideinfo" when parameters.Length >= 1 =>
                (TResult)(object)_videoService.GetVideoInfoAsync(parameters[0].ToString()!).Result,
            _ => throw new NotSupportedException($"Action '{actionName}' is not supported")
        };
    }

    public void ExecuteAction(string actionName, params object[] parameters)
    {
        switch (actionName.ToLowerInvariant())
        {
            case "stoprecording" when parameters.Length >= 1:
                _videoRecordingService.StopRecordingAsync(parameters[0].ToString()!).Wait();
                break;
            case "playrecording" when parameters.Length >= 1:
                var speed = parameters.Length > 1 ? Convert.ToDouble(parameters[1]) : 1.0;
                var options = new VideoPlaybackOptions { Speed = speed };
                _videoService.PlayVideoAsync(parameters[0].ToString()!, options).Wait();
                break;
            case "convertvideo" when parameters.Length >= 2:
                var conversionOptions = parameters.Length > 2 ?
                    new VideoConversionOptions { Quality = Convert.ToInt32(parameters[2]) } : null;
                _videoService.ConvertVideoAsync(parameters[0].ToString()!, parameters[1].ToString()!, conversionOptions).Wait();
                break;
            default:
                throw new NotSupportedException($"Action '{actionName}' is not supported");
        }
    }

    public bool SupportsAction(string actionName)
    {
        return actionName.ToLowerInvariant() switch
        {
            "startrecording" or "stoprecording" or "isrecording" or "getactivesessions" or
            "playrecording" or "getvideinfo" or "convertvideo" => true,
            _ => false
        };
    }

    public IEnumerable<LablabBean.Contracts.Recording.Services.ActionInfo> GetSupportedActions()
    {
        return new[]
        {
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "StartRecording",
                Description = "Start video recording",
                ParameterTypes = new[] { typeof(string), typeof(string) },
                ReturnType = typeof(string)
            },
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "StopRecording",
                Description = "Stop video recording",
                ParameterTypes = new[] { typeof(string) }
            },
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "IsRecording",
                Description = "Check if recording",
                ParameterTypes = new[] { typeof(string) },
                ReturnType = typeof(bool)
            },
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "GetActiveSessions",
                Description = "Get active recording sessions",
                ReturnType = typeof(string[])
            },
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "PlayRecording",
                Description = "Play video recording",
                ParameterTypes = new[] { typeof(string), typeof(double) }
            },
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "GetVideoInfo",
                Description = "Get video file information",
                ParameterTypes = new[] { typeof(string) },
                ReturnType = typeof(VideoInfo)
            },
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "ConvertVideo",
                Description = "Convert video format",
                ParameterTypes = new[] { typeof(string), typeof(string), typeof(int) }
            }
        };
    }
}
