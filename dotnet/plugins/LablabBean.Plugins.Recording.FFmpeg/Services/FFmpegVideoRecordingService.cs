using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LablabBean.Contracts.Recording.Models;
using LablabBean.Contracts.Recording.Services;
using LablabBean.Plugins.Recording.FFmpeg.Configuration;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Recording.FFmpeg.Services;

/// <summary>
/// Video recording service implementation using FFmpeg
/// </summary>
public class FFmpegVideoRecordingService : IRecordingService, LablabBean.Contracts.Recording.Services.IService
{
    private readonly ILogger _logger;
    private readonly FFmpegRecordingOptions _options;
    private readonly ConcurrentDictionary<string, (RecordingSession Session, Process Process)> _activeSessions = new();

    public FFmpegVideoRecordingService(ILogger logger, FFmpegRecordingOptions? options = null)
    {
        _logger = logger;
        _options = options ?? new FFmpegRecordingOptions();
    }

    public async Task<string> StartRecordingAsync(string outputPath, string? title = null, CancellationToken ct = default)
    {
        var sessionId = Guid.NewGuid().ToString("N")[..8];

        try
        {
            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Build FFmpeg command for screen recording
            var args = BuildFFmpegArgs(outputPath, _options);

            // Start FFmpeg process
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = !_options.ShowFFmpegOutput,
                RedirectStandardError = !_options.ShowFFmpegOutput,
                CreateNoWindow = !_options.ShowFFmpegOutput,
                RedirectStandardInput = true // Needed for graceful shutdown
            };

            var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start FFmpeg process");
            }

            var session = new RecordingSession
            {
                Id = sessionId,
                OutputPath = outputPath,
                Title = title,
                ProcessId = process.Id,
                StartTime = DateTime.UtcNow,
                IsActive = true
            };

            _activeSessions[sessionId] = (session, process);

            _logger.LogInformation(
                "Started video recording session {SessionId} to {OutputPath} (PID: {ProcessId})",
                sessionId, outputPath, process.Id);

            // Monitor process completion
            _ = Task.Run(async () =>
            {
                try
                {
                    await process.WaitForExitAsync(ct);
                    _activeSessions.TryRemove(sessionId, out _);

                    _logger.LogInformation(
                        "Video recording session {SessionId} completed with exit code {ExitCode}",
                        sessionId, process.ExitCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error monitoring video recording session {SessionId}", sessionId);
                    _activeSessions.TryRemove(sessionId, out _);
                }
            }, ct);

            return sessionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start video recording session {SessionId}", sessionId);
            throw;
        }
    }

    private string BuildFFmpegArgs(string outputPath, FFmpegRecordingOptions options)
    {
        var args = new List<string>();

        // Add input source based on platform
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            BuildWindowsArgs(args, options);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            BuildLinuxArgs(args, options);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            BuildMacOSArgs(args, options);
        }
        else
        {
            throw new PlatformNotSupportedException("Video recording not supported on this platform");
        }

        // Add encoding options
        args.AddRange(new[]
        {
            "-c:v", options.VideoCodec,
            "-preset", options.Preset,
            "-crf", options.CRF.ToString(),
            "-pix_fmt", options.PixelFormat
        });

        // Add duration limit if specified
        if (options.MaxDurationSeconds > 0)
        {
            args.AddRange(new[] { "-t", options.MaxDurationSeconds.ToString() });
        }

        // Add custom resolution if specified
        if (!string.IsNullOrEmpty(options.Resolution))
        {
            args.AddRange(new[] { "-s", options.Resolution });
        }

        // Add additional arguments
        if (options.AdditionalArgs.Length > 0)
        {
            args.AddRange(options.AdditionalArgs);
        }

        // Add output path
        args.Add($"\"{outputPath}\"");

        return string.Join(" ", args);
    }

    private void BuildWindowsArgs(List<string> args, FFmpegRecordingOptions options)
    {
        args.AddRange(new[] { "-f", "gdigrab" });
        args.AddRange(new[] { "-framerate", options.FrameRate.ToString() });

        var input = "desktop";
        if (!string.IsNullOrEmpty(options.PlatformSettings.Windows.WindowTitle))
        {
            input = $"title={options.PlatformSettings.Windows.WindowTitle}";
        }

        if (options.PlatformSettings.Windows.Offset.HasValue)
        {
            var (x, y) = options.PlatformSettings.Windows.Offset.Value;
            args.AddRange(new[] { "-offset_x", x.ToString(), "-offset_y", y.ToString() });
        }

        if (!options.PlatformSettings.Windows.ShowCursor)
        {
            args.AddRange(new[] { "-draw_mouse", "0" });
        }

        args.AddRange(new[] { "-i", input });
    }

    private void BuildLinuxArgs(List<string> args, FFmpegRecordingOptions options)
    {
        args.AddRange(new[] { "-f", "x11grab" });
        args.AddRange(new[] { "-framerate", options.FrameRate.ToString() });

        if (options.PlatformSettings.Linux.FollowMouse)
        {
            args.AddRange(new[] { "-follow_mouse", "centered" });
        }

        if (!options.PlatformSettings.Linux.ShowCursor)
        {
            args.AddRange(new[] { "-draw_mouse", "0" });
        }

        // Use custom resolution or default
        var resolution = options.Resolution ?? "1920x1080";
        args.AddRange(new[] { "-s", resolution });

        args.AddRange(new[] { "-i", options.PlatformSettings.Linux.Display });
    }

    private void BuildMacOSArgs(List<string> args, FFmpegRecordingOptions options)
    {
        args.AddRange(new[] { "-f", "avfoundation" });
        args.AddRange(new[] { "-framerate", options.FrameRate.ToString() });

        if (options.PlatformSettings.MacOS.CaptureClicks)
        {
            args.AddRange(new[] { "-capture_cursor", "1", "-capture_mouse_clicks", "1" });
        }
        else if (options.PlatformSettings.MacOS.ShowCursor)
        {
            args.AddRange(new[] { "-capture_cursor", "1" });
        }

        var input = options.PlatformSettings.MacOS.ScreenDevice.ToString();

        // Add audio if enabled
        if (options.RecordAudio && !string.IsNullOrEmpty(options.AudioDevice))
        {
            input = $"{options.AudioDevice}:{input}";
        }

        args.AddRange(new[] { "-i", input });
    }

    public async Task StopRecordingAsync(string sessionId, CancellationToken ct = default)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var sessionData))
        {
            _logger.LogWarning("Video recording session {SessionId} not found", sessionId);
            return;
        }

        try
        {
            var (session, process) = sessionData;

            // Send 'q' to FFmpeg to gracefully stop recording
            if (!process.HasExited)
            {
                try
                {
                    if (process.StandardInput != null && process.StandardInput.BaseStream.CanWrite)
                    {
                        await process.StandardInput.WriteLineAsync("q", ct).ConfigureAwait(false);
                        await process.StandardInput.FlushAsync(ct).ConfigureAwait(false);
                    }

                    // Wait a bit for graceful shutdown
                    if (!process.WaitForExit(5000))
                    {
                        // Force kill if graceful shutdown fails
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch
                {
                    // Fallback to force kill
                    process.Kill(entireProcessTree: true);
                }

                await process.WaitForExitAsync(ct).ConfigureAwait(false);
            }

            _activeSessions.TryRemove(sessionId, out _);

            _logger.LogInformation(
                "Stopped video recording session {SessionId} (saved to {OutputPath})",
                sessionId, session.OutputPath);
        }
        catch (ArgumentException)
        {
            // Process already exited
            _activeSessions.TryRemove(sessionId, out _);
            _logger.LogInformation("Video recording session {SessionId} was already stopped", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping video recording session {SessionId}", sessionId);
            throw;
        }
    }

    public bool IsRecording(string sessionId)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var sessionData))
        {
            return false;
        }

        try
        {
            return !sessionData.Process.HasExited;
        }
        catch (InvalidOperationException)
        {
            // Process no longer exists
            _activeSessions.TryRemove(sessionId, out _);
            return false;
        }
    }

    public IEnumerable<string> GetActiveSessions()
    {
        // Clean up dead sessions
        var deadSessions = _activeSessions
            .Where(kvp => !IsRecording(kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var deadSession in deadSessions)
        {
            _activeSessions.TryRemove(deadSession, out _);
        }

        return _activeSessions.Keys.ToList();
    }

    public async Task PlayRecordingAsync(string recordingPath, double speed = 1.0, CancellationToken ct = default)
    {
        if (!File.Exists(recordingPath))
        {
            throw new FileNotFoundException($"Video recording file not found: {recordingPath}");
        }

        try
        {
            var args = new List<string>();

            // Use FFplay for video playback
            if (Math.Abs(speed - 1.0) > 0.001)
            {
                args.AddRange(new[] { "-vf", $"setpts={1.0/speed:F2}*PTS" });
            }

            args.Add($"\"{recordingPath}\"");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffplay",
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                CreateNoWindow = false
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start FFplay process");
            }

            _logger.LogInformation(
                "Started playback of {RecordingPath} at {Speed}x speed",
                recordingPath, speed);

            await process.WaitForExitAsync(ct);

            _logger.LogInformation(
                "Playback of {RecordingPath} completed with exit code {ExitCode}",
                recordingPath, process.ExitCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing video recording {RecordingPath}", recordingPath);
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
            default:
                throw new NotSupportedException($"Action '{actionName}' is not supported");
        }
    }

    public bool SupportsAction(string actionName)
    {
        return actionName.ToLowerInvariant() switch
        {
            "startrecording" or "stoprecording" or "isrecording" or
            "getactivesessions" or "playrecording" => true,
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
                Description = "Start a new video recording session",
                ParameterTypes = new[] { typeof(string), typeof(string) },
                ReturnType = typeof(string)
            },
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "StopRecording",
                Description = "Stop an active video recording session",
                ParameterTypes = new[] { typeof(string) }
            },
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "IsRecording",
                Description = "Check if a session is recording",
                ParameterTypes = new[] { typeof(string) },
                ReturnType = typeof(bool)
            },
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "GetActiveSessions",
                Description = "Get all active video recording sessions",
                ReturnType = typeof(string[])
            },
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "PlayRecording",
                Description = "Play back a video recording",
                ParameterTypes = new[] { typeof(string), typeof(double) }
            }
        };
    }
}
