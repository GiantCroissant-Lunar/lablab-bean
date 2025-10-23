using System.Collections.Concurrent;
using LablabBean.Contracts.Recording.Models;
using LablabBean.Contracts.Recording.Services;
using LablabBean.Contracts.Video.Models;
using LablabBean.Contracts.Video.Services;
using LablabBean.Plugins.Recording.Video.Configuration;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Recording.Video.Services;

/// <summary>
/// Recording service that uses the video service for actual recording operations
/// This provides a clean separation between recording workflow management and video processing
/// </summary>
public class VideoRecordingService : IRecordingService, LablabBean.Contracts.Recording.Services.IService
{
    private readonly ILogger _logger;
    private readonly IVideoRecordingService _videoRecordingService;
    private readonly IVideoService _videoService;
    private readonly RecordingConfiguration _configuration;
    private readonly ConcurrentDictionary<string, RecordingSessionInfo> _recordingSessions = new();

    public VideoRecordingService(
        ILogger logger,
        IVideoRecordingService videoRecordingService,
        IVideoService videoService,
        RecordingConfiguration? configuration = null)
    {
        _logger = logger;
        _videoRecordingService = videoRecordingService;
        _videoService = videoService;
        _configuration = configuration ?? new RecordingConfiguration();
    }

    public async Task<string> StartRecordingAsync(string outputPath, string? title = null, CancellationToken ct = default)
    {
        return await StartRecordingAsync(outputPath, title, RecordingType.Manual, ct);
    }

    public async Task<string> StartGameRecordingAsync(string outputPath, string? title = null, CancellationToken ct = default)
    {
        return await StartRecordingAsync(outputPath, title, RecordingType.Game, ct);
    }

    private async Task<string> StartRecordingAsync(string outputPath, string? title, RecordingType recordingType, CancellationToken ct = default)
    {
        var sessionId = Guid.NewGuid().ToString("N")[..8];

        try
        {
            // Create video recording options based on configuration and recording type
            var videoOptions = recordingType switch
            {
                RecordingType.Game => CreateGameRecordingOptions(title),
                RecordingType.Manual => CreateManualRecordingOptions(title),
                _ => throw new ArgumentException($"Unknown recording type: {recordingType}")
            };

            // Start video recording using the video service
            var videoSessionId = await _videoRecordingService.StartRecordingAsync(outputPath, videoOptions, ct);

            // Create recording session info
            var sessionInfo = new RecordingSessionInfo
            {
                RecordingSessionId = sessionId,
                VideoSessionId = videoSessionId,
                OutputPath = outputPath,
                Title = title,
                StartTime = DateTime.UtcNow
            };

            _recordingSessions[sessionId] = sessionInfo;

            _logger.LogInformation(
                "Started recording session {RecordingSessionId} -> video session {VideoSessionId} to {OutputPath}",
                sessionId, videoSessionId, outputPath);

            return sessionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start recording session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task StopRecordingAsync(string sessionId, CancellationToken ct = default)
    {
        if (!_recordingSessions.TryGetValue(sessionId, out var sessionInfo))
        {
            _logger.LogWarning("Recording session {SessionId} not found", sessionId);
            return;
        }

        try
        {
            // Stop the underlying video recording
            await _videoRecordingService.StopRecordingAsync(sessionInfo.VideoSessionId, ct);

            // Remove from our tracking
            _recordingSessions.TryRemove(sessionId, out _);

            _logger.LogInformation(
                "Stopped recording session {RecordingSessionId} (video session {VideoSessionId})",
                sessionId, sessionInfo.VideoSessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording session {SessionId}", sessionId);
            throw;
        }
    }

    public bool IsRecording(string sessionId)
    {
        if (!_recordingSessions.TryGetValue(sessionId, out var sessionInfo))
        {
            return false;
        }

        // Check if the underlying video recording is still active
        var isVideoRecording = _videoRecordingService.IsRecording(sessionInfo.VideoSessionId);

        // Clean up if video recording has stopped
        if (!isVideoRecording)
        {
            _recordingSessions.TryRemove(sessionId, out _);
        }

        return isVideoRecording;
    }

    public IEnumerable<string> GetActiveSessions()
    {
        // Clean up dead sessions
        var deadSessions = _recordingSessions
            .Where(kvp => !IsRecording(kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var deadSession in deadSessions)
        {
            _recordingSessions.TryRemove(deadSession, out _);
        }

        return _recordingSessions.Keys.ToList();
    }

    public async Task PlayRecordingAsync(string recordingPath, double speed = 1.0, CancellationToken ct = default)
    {
        if (!File.Exists(recordingPath))
        {
            throw new FileNotFoundException($"Recording file not found: {recordingPath}");
        }

        try
        {
            // Use video service for playback
            var playbackOptions = new VideoPlaybackOptions
            {
                Speed = speed,
                WindowTitle = $"Recording Playback - {Path.GetFileName(recordingPath)}"
            };

            var playbackSessionId = await _videoService.PlayVideoAsync(recordingPath, playbackOptions, ct);

            _logger.LogInformation(
                "Started playback of recording {RecordingPath} at {Speed}x speed (session {PlaybackSessionId})",
                recordingPath, speed, playbackSessionId);

            // Note: We don't wait for playback to complete as it's typically a fire-and-forget operation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing recording {RecordingPath}", recordingPath);
            throw;
        }
    }

    // IService implementation for action-based interface
    public TResult ExecuteAction<TResult>(string actionName, params object[] parameters)
    {
        return actionName.ToLowerInvariant() switch
        {
            "startrecording" when parameters.Length >= 1 =>
                (TResult)(object)StartRecordingAsync(parameters[0].ToString()!,
                    parameters.Length > 1 ? parameters[1]?.ToString() : null).Result,
            "isrecording" when parameters.Length >= 1 =>
                (TResult)(object)IsRecording(parameters[0].ToString()!),
            "getactivesessions" =>
                (TResult)(object)GetActiveSessions().ToArray(),
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
                StopRecordingAsync(parameters[0].ToString()!).Wait();
                break;
            case "playrecording" when parameters.Length >= 1:
                var speed = parameters.Length > 1 ? Convert.ToDouble(parameters[1]) : 1.0;
                PlayRecordingAsync(parameters[0].ToString()!, speed).Wait();
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
                Description = "Start video recording using video service",
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
                Description = "Check if recording is active",
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

    private VideoRecordingOptions CreateGameRecordingOptions(string? title)
    {
        var gameOptions = _configuration.GameRecording;
        return new VideoRecordingOptions
        {
            Title = title,
            Source = gameOptions.Source,
            FrameRate = gameOptions.FrameRate,
            Quality = gameOptions.Quality,
            Preset = gameOptions.Preset,
            VideoCodec = gameOptions.VideoCodec,
            RecordAudio = gameOptions.RecordAudio,
            MaxDurationSeconds = gameOptions.MaxDurationSeconds,
            Resolution = gameOptions.Resolution
        };
    }

    private VideoRecordingOptions CreateManualRecordingOptions(string? title)
    {
        var manualOptions = _configuration.ManualRecording;
        return new VideoRecordingOptions
        {
            Title = title,
            Source = manualOptions.Source,
            FrameRate = manualOptions.FrameRate,
            Quality = manualOptions.Quality,
            Preset = manualOptions.Preset,
            VideoCodec = manualOptions.VideoCodec,
            RecordAudio = manualOptions.RecordAudio,
            MaxDurationSeconds = manualOptions.MaxDurationSeconds,
            Resolution = manualOptions.Resolution
        };
    }
}

/// <summary>
/// Type of recording being performed
/// </summary>
internal enum RecordingType
{
    Manual,
    Game
}

/// <summary>
/// Internal tracking information for recording sessions
/// </summary>
internal record RecordingSessionInfo
{
    public required string RecordingSessionId { get; init; }
    public required string VideoSessionId { get; init; }
    public required string OutputPath { get; init; }
    public string? Title { get; init; }
    public DateTime StartTime { get; init; }
    public RecordingType Type { get; init; }
}
