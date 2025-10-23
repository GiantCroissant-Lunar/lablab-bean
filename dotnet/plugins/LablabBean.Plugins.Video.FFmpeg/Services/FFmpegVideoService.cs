using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using LablabBean.Contracts.Video.Models;
using LablabBean.Contracts.Video.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Video.FFmpeg.Services;

/// <summary>
/// Comprehensive FFmpeg-based video service supporting playback, recording, and conversion
/// </summary>
public class FFmpegVideoService : IVideoService, IVideoRecordingService
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, VideoSession> _activeSessions = new();
    private readonly ConcurrentDictionary<string, Process> _activeProcesses = new();

    public FFmpegVideoService(ILogger logger)
    {
        _logger = logger;
    }

    #region IVideoService Implementation

    public async Task<string> PlayVideoAsync(string videoPath, VideoPlaybackOptions? options = null, CancellationToken ct = default)
    {
        if (!File.Exists(videoPath))
        {
            throw new FileNotFoundException($"Video file not found: {videoPath}");
        }

        var sessionId = GenerateSessionId();
        options ??= new VideoPlaybackOptions();

        try
        {
            var args = BuildPlaybackArgs(videoPath, options);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffplay",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                CreateNoWindow = false
            };

            var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start FFplay process");
            }

            var session = new VideoSession
            {
                Id = sessionId,
                Type = VideoSessionType.Playback,
                FilePath = videoPath,
                ProcessId = process.Id,
                Status = VideoSessionStatus.Active
            };

            _activeSessions[sessionId] = session;
            _activeProcesses[sessionId] = process;

            _logger.LogInformation(
                "Started video playback session {SessionId} for {VideoPath} (PID: {ProcessId})",
                sessionId, videoPath, process.Id);

            // Monitor process completion
            _ = MonitorProcessAsync(sessionId, process, ct);

            return sessionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start video playback for {VideoPath}", videoPath);
            throw;
        }
    }

    public async Task StopPlaybackAsync(string sessionId, CancellationToken ct = default)
    {
        if (!_activeProcesses.TryGetValue(sessionId, out var process))
        {
            _logger.LogWarning("Playback session {SessionId} not found", sessionId);
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(ct);
            }

            CleanupSession(sessionId);
            _logger.LogInformation("Stopped playback session {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping playback session {SessionId}", sessionId);
            throw;
        }
    }

    public bool IsPlaying(string sessionId)
    {
        if (!_activeProcesses.TryGetValue(sessionId, out var process))
        {
            return false;
        }

        try
        {
            return !process.HasExited;
        }
        catch (InvalidOperationException)
        {
            CleanupSession(sessionId);
            return false;
        }
    }

    public async Task<VideoInfo> GetVideoInfoAsync(string videoPath, CancellationToken ct = default)
    {
        if (!File.Exists(videoPath))
        {
            throw new FileNotFoundException($"Video file not found: {videoPath}");
        }

        try
        {
            var args = $"-v quiet -print_format json -show_format -show_streams \"{videoPath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start FFprobe process");
            }

            var output = await process.StandardOutput.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(ct);
                throw new InvalidOperationException($"FFprobe failed: {error}");
            }

            return ParseVideoInfo(videoPath, output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get video info for {VideoPath}", videoPath);
            throw;
        }
    }

    public async Task<string> ConvertVideoAsync(string inputPath, string outputPath, VideoConversionOptions? options = null, CancellationToken ct = default)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException($"Input video file not found: {inputPath}");
        }

        var sessionId = GenerateSessionId();
        options ??= new VideoConversionOptions();

        try
        {
            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var args = BuildConversionArgs(inputPath, outputPath, options);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start FFmpeg conversion process");
            }

            var session = new VideoSession
            {
                Id = sessionId,
                Type = VideoSessionType.Conversion,
                FilePath = inputPath,
                ProcessId = process.Id,
                Status = VideoSessionStatus.Active,
                Data = new Dictionary<string, object> { ["OutputPath"] = outputPath }
            };

            _activeSessions[sessionId] = session;
            _activeProcesses[sessionId] = process;

            _logger.LogInformation(
                "Started video conversion session {SessionId}: {InputPath} -> {OutputPath} (PID: {ProcessId})",
                sessionId, inputPath, outputPath, process.Id);

            // Monitor process with progress tracking
            _ = MonitorConversionAsync(sessionId, process, ct);

            return sessionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start video conversion: {InputPath} -> {OutputPath}", inputPath, outputPath);
            throw;
        }
    }

    public IEnumerable<string> GetActiveSessions()
    {
        CleanupDeadSessions();
        return _activeSessions.Keys.ToList();
    }

    #endregion

    #region IVideoRecordingService Implementation

    public async Task<string> StartRecordingAsync(string outputPath, VideoRecordingOptions? options = null, CancellationToken ct = default)
    {
        var sessionId = GenerateSessionId();
        options ??= new VideoRecordingOptions();

        try
        {
            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var args = BuildRecordingArgs(outputPath, options);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                RedirectStandardInput = true
            };

            var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start FFmpeg recording process");
            }

            var session = new VideoSession
            {
                Id = sessionId,
                Type = VideoSessionType.Recording,
                FilePath = outputPath,
                ProcessId = process.Id,
                Status = VideoSessionStatus.Active
            };

            _activeSessions[sessionId] = session;
            _activeProcesses[sessionId] = process;

            _logger.LogInformation(
                "Started video recording session {SessionId} to {OutputPath} (PID: {ProcessId})",
                sessionId, outputPath, process.Id);

            // Monitor process completion
            _ = MonitorProcessAsync(sessionId, process, ct);

            return sessionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start video recording to {OutputPath}", outputPath);
            throw;
        }
    }

    public async Task StopRecordingAsync(string sessionId, CancellationToken ct = default)
    {
        if (!_activeProcesses.TryGetValue(sessionId, out var process))
        {
            _logger.LogWarning("Recording session {SessionId} not found", sessionId);
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                // Send 'q' to FFmpeg for graceful shutdown
                try
                {
                    if (process.StandardInput != null && process.StandardInput.BaseStream.CanWrite)
                    {
                        await process.StandardInput.WriteLineAsync("q");
                        await process.StandardInput.FlushAsync();
                    }

                    if (!process.WaitForExit(5000))
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch
                {
                    process.Kill(entireProcessTree: true);
                }

                await process.WaitForExitAsync(ct);
            }

            if (_activeSessions.TryGetValue(sessionId, out var session))
            {
                _logger.LogInformation(
                    "Stopped video recording session {SessionId} (saved to {OutputPath})",
                    sessionId, session.FilePath);
            }

            CleanupSession(sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording session {SessionId}", sessionId);
            throw;
        }
    }

    public bool IsRecording(string sessionId)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var session) || session.Type != VideoSessionType.Recording)
        {
            return false;
        }

        if (!_activeProcesses.TryGetValue(sessionId, out var process))
        {
            return false;
        }

        try
        {
            return !process.HasExited;
        }
        catch (InvalidOperationException)
        {
            CleanupSession(sessionId);
            return false;
        }
    }

    public IEnumerable<string> GetActiveRecordingSessions()
    {
        CleanupDeadSessions();
        return _activeSessions
            .Where(kvp => kvp.Value.Type == VideoSessionType.Recording)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    #endregion

    #region Private Methods

    private string GenerateSessionId() => Guid.NewGuid().ToString("N")[..8];

    private string BuildPlaybackArgs(string videoPath, VideoPlaybackOptions options)
    {
        var args = new List<string>();

        // Add playback options
        if (options.StartPosition > 0)
        {
            args.AddRange(new[] { "-ss", options.StartPosition.ToString("F2") });
        }

        if (options.EndPosition > 0)
        {
            args.AddRange(new[] { "-t", (options.EndPosition - options.StartPosition).ToString("F2") });
        }

        if (Math.Abs(options.Speed - 1.0) > 0.001)
        {
            args.AddRange(new[] { "-vf", $"setpts={1.0/options.Speed:F2}*PTS" });
        }

        if (options.Loop)
        {
            args.AddRange(new[] { "-loop", "0" });
        }

        if (options.Fullscreen)
        {
            args.Add("-fs");
        }

        if (options.Mute)
        {
            args.Add("-an");
        }
        else if (Math.Abs(options.Volume - 1.0) > 0.001)
        {
            args.AddRange(new[] { "-af", $"volume={options.Volume:F2}" });
        }

        if (!string.IsNullOrEmpty(options.WindowTitle))
        {
            args.AddRange(new[] { "-window_title", $"\"{options.WindowTitle}\"" });
        }

        // Add additional arguments
        if (options.AdditionalArgs.Length > 0)
        {
            args.AddRange(options.AdditionalArgs);
        }

        // Add input file
        args.Add($"\"{videoPath}\"");

        return string.Join(" ", args);
    }

    private string BuildRecordingArgs(string outputPath, VideoRecordingOptions options)
    {
        var args = new List<string>();

        // Add input source based on platform and source type
        switch (options.Source)
        {
            case VideoRecordingSource.Screen:
                BuildScreenCaptureArgs(args, options);
                break;
            case VideoRecordingSource.Window:
                BuildWindowCaptureArgs(args, options);
                break;
            case VideoRecordingSource.Region:
                BuildRegionCaptureArgs(args, options);
                break;
            case VideoRecordingSource.Camera:
                BuildCameraCaptureArgs(args, options);
                break;
            default:
                throw new NotSupportedException($"Recording source {options.Source} not supported");
        }

        // Add encoding options
        args.AddRange(new[]
        {
            "-c:v", options.VideoCodec,
            "-preset", options.Preset,
            "-crf", options.Quality.ToString(),
            "-pix_fmt", "yuv420p"
        });

        // Add resolution if specified
        if (!string.IsNullOrEmpty(options.Resolution))
        {
            args.AddRange(new[] { "-s", options.Resolution });
        }

        // Add duration limit
        if (options.MaxDurationSeconds > 0)
        {
            args.AddRange(new[] { "-t", options.MaxDurationSeconds.ToString() });
        }

        // Add audio recording
        if (options.RecordAudio && !string.IsNullOrEmpty(options.AudioDevice))
        {
            // This would need platform-specific implementation
            args.AddRange(new[] { "-f", GetAudioFormat(), "-i", options.AudioDevice });
        }

        // Add additional arguments
        if (options.AdditionalArgs.Length > 0)
        {
            args.AddRange(options.AdditionalArgs);
        }

        // Add output
        args.Add($"\"{outputPath}\"");

        return string.Join(" ", args);
    }

    private void BuildScreenCaptureArgs(List<string> args, VideoRecordingOptions options)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            args.AddRange(new[] { "-f", "gdigrab", "-framerate", options.FrameRate.ToString(), "-i", "desktop" });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var display = options.PlatformSettings.GetValueOrDefault("Display", ":0.0")?.ToString() ?? ":0.0";
            args.AddRange(new[] { "-f", "x11grab", "-framerate", options.FrameRate.ToString(), "-i", display });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var device = options.PlatformSettings.GetValueOrDefault("ScreenDevice", "1")?.ToString() ?? "1";
            args.AddRange(new[] { "-f", "avfoundation", "-framerate", options.FrameRate.ToString(), "-i", device });
        }
        else
        {
            throw new PlatformNotSupportedException("Screen recording not supported on this platform");
        }
    }

    private void BuildWindowCaptureArgs(List<string> args, VideoRecordingOptions options)
    {
        // Platform-specific window capture implementation
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var windowTitle = options.PlatformSettings.GetValueOrDefault("WindowTitle", "")?.ToString() ?? "";
            args.AddRange(new[] { "-f", "gdigrab", "-framerate", options.FrameRate.ToString(), "-i", $"title={windowTitle}" });
        }
        else
        {
            throw new PlatformNotSupportedException("Window recording not implemented for this platform");
        }
    }

    private void BuildRegionCaptureArgs(List<string> args, VideoRecordingOptions options)
    {
        // Region capture implementation
        throw new NotImplementedException("Region capture not yet implemented");
    }

    private void BuildCameraCaptureArgs(List<string> args, VideoRecordingOptions options)
    {
        // Camera capture implementation
        throw new NotImplementedException("Camera capture not yet implemented");
    }

    private string BuildConversionArgs(string inputPath, string outputPath, VideoConversionOptions options)
    {
        var args = new List<string>();

        // Input
        if (options.StartTime.HasValue)
        {
            args.AddRange(new[] { "-ss", options.StartTime.Value.ToString("F2") });
        }

        args.AddRange(new[] { "-i", $"\"{inputPath}\"" });

        // Duration/trimming
        if (options.Duration.HasValue)
        {
            args.AddRange(new[] { "-t", options.Duration.Value.ToString("F2") });
        }

        // Video encoding
        if (!string.IsNullOrEmpty(options.VideoCodec))
        {
            args.AddRange(new[] { "-c:v", options.VideoCodec });
        }

        if (!string.IsNullOrEmpty(options.AudioCodec))
        {
            args.AddRange(new[] { "-c:a", options.AudioCodec });
        }

        if (options.Quality.HasValue)
        {
            args.AddRange(new[] { "-crf", options.Quality.Value.ToString() });
        }

        if (!string.IsNullOrEmpty(options.Preset))
        {
            args.AddRange(new[] { "-preset", options.Preset });
        }

        if (!string.IsNullOrEmpty(options.Resolution))
        {
            args.AddRange(new[] { "-s", options.Resolution });
        }

        if (options.FrameRate.HasValue)
        {
            args.AddRange(new[] { "-r", options.FrameRate.Value.ToString() });
        }

        // Progress reporting
        args.AddRange(new[] { "-progress", "pipe:1" });

        // Additional arguments
        if (options.AdditionalArgs?.Length > 0)
        {
            args.AddRange(options.AdditionalArgs);
        }

        // Output
        args.Add($"\"{outputPath}\"");

        return string.Join(" ", args);
    }

    private string GetAudioFormat()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "dshow";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "alsa";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "avfoundation";
        else
            throw new PlatformNotSupportedException("Audio recording not supported on this platform");
    }

    private VideoInfo ParseVideoInfo(string filePath, string ffprobeOutput)
    {
        try
        {
            using var document = JsonDocument.Parse(ffprobeOutput);
            var root = document.RootElement;

            var format = root.GetProperty("format");
            var streams = root.GetProperty("streams");

            var videoStream = streams.EnumerateArray()
                .FirstOrDefault(s => s.GetProperty("codec_type").GetString() == "video");

            var audioStream = streams.EnumerateArray()
                .FirstOrDefault(s => s.GetProperty("codec_type").GetString() == "audio");

            var fileInfo = new FileInfo(filePath);

            return new VideoInfo
            {
                FilePath = filePath,
                Duration = double.Parse(format.GetProperty("duration").GetString() ?? "0"),
                Width = videoStream.TryGetProperty("width", out var width) ? width.GetInt32() : 0,
                Height = videoStream.TryGetProperty("height", out var height) ? height.GetInt32() : 0,
                FrameRate = ParseFrameRate(videoStream.GetProperty("r_frame_rate").GetString() ?? "0/1"),
                VideoCodec = videoStream.TryGetProperty("codec_name", out var vcodec) ? vcodec.GetString() : null,
                AudioCodec = audioStream.TryGetProperty("codec_name", out var acodec) ? acodec.GetString() : null,
                FileSize = fileInfo.Length,
                Bitrate = long.Parse(format.GetProperty("bit_rate").GetString() ?? "0"),
                HasAudio = audioStream.ValueKind != JsonValueKind.Undefined,
                CreationTime = fileInfo.CreationTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse video info from FFprobe output");
            throw new InvalidOperationException("Failed to parse video information", ex);
        }
    }

    private double ParseFrameRate(string frameRateString)
    {
        var parts = frameRateString.Split('/');
        if (parts.Length == 2 && double.TryParse(parts[0], out var num) && double.TryParse(parts[1], out var den) && den != 0)
        {
            return num / den;
        }
        return 0;
    }

    private async Task MonitorProcessAsync(string sessionId, Process process, CancellationToken ct)
    {
        try
        {
            await process.WaitForExitAsync(ct);

            if (_activeSessions.TryGetValue(sessionId, out var session))
            {
                _logger.LogInformation(
                    "{SessionType} session {SessionId} completed with exit code {ExitCode}",
                    session.Type, sessionId, process.ExitCode);
            }

            CleanupSession(sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring session {SessionId}", sessionId);
            CleanupSession(sessionId);
        }
    }

    private async Task MonitorConversionAsync(string sessionId, Process process, CancellationToken ct)
    {
        try
        {
            // Monitor FFmpeg progress output
            _ = Task.Run(async () =>
            {
                try
                {
                    using var reader = process.StandardOutput;
                    string? line;
                    while ((line = await reader.ReadLineAsync(ct)) != null)
                    {
                        ParseConversionProgress(sessionId, line);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading conversion progress for session {SessionId}", sessionId);
                }
            }, ct);

            await process.WaitForExitAsync(ct);

            if (_activeSessions.TryGetValue(sessionId, out var session))
            {
                _logger.LogInformation(
                    "Conversion session {SessionId} completed with exit code {ExitCode}",
                    sessionId, process.ExitCode);
            }

            CleanupSession(sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring conversion session {SessionId}", sessionId);
            CleanupSession(sessionId);
        }
    }

    private void ParseConversionProgress(string sessionId, string progressLine)
    {
        // Parse FFmpeg progress output (frame=, fps=, time=, etc.)
        if (progressLine.StartsWith("time=") && _activeSessions.TryGetValue(sessionId, out var session))
        {
            var timeMatch = Regex.Match(progressLine, @"time=(\d+):(\d+):(\d+\.\d+)");
            if (timeMatch.Success)
            {
                var hours = int.Parse(timeMatch.Groups[1].Value);
                var minutes = int.Parse(timeMatch.Groups[2].Value);
                var seconds = double.Parse(timeMatch.Groups[3].Value);
                var currentTime = TimeSpan.FromHours(hours).Add(TimeSpan.FromMinutes(minutes)).Add(TimeSpan.FromSeconds(seconds));

                // Update session progress (would need total duration for percentage)
                var updatedSession = session with { Progress = currentTime.TotalSeconds };
                _activeSessions[sessionId] = updatedSession;
            }
        }
    }

    private void CleanupSession(string sessionId)
    {
        _activeSessions.TryRemove(sessionId, out _);
        _activeProcesses.TryRemove(sessionId, out _);
    }

    private void CleanupDeadSessions()
    {
        var deadSessions = _activeProcesses
            .Where(kvp => kvp.Value.HasExited)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var sessionId in deadSessions)
        {
            CleanupSession(sessionId);
        }
    }

    #endregion
}
