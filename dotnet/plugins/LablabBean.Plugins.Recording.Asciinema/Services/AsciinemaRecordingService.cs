using System.Collections.Concurrent;
using System.Diagnostics;
using LablabBean.Contracts.Recording.Models;
using LablabBean.Contracts.Recording.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Recording.Asciinema.Services;

/// <summary>
/// Recording service implementation using asciinema
/// </summary>
public class AsciinemaRecordingService : IRecordingService, LablabBean.Contracts.Recording.Services.IService
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, RecordingSession> _activeSessions = new();

    public AsciinemaRecordingService(ILogger logger)
    {
        _logger = logger;
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

            // Build asciinema command
            var args = new List<string> { "rec", outputPath };

            if (!string.IsNullOrEmpty(title))
            {
                args.AddRange(new[] { "--title", title });
            }

            // Start asciinema process
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "asciinema",
                Arguments = string.Join(" ", args.Select(arg => $"\"{arg}\"")),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start asciinema process");
            }

            var session = new RecordingSession
            {
                Id = sessionId,
                OutputPath = outputPath,
                Title = title,
                ProcessId = process.Id,
                StartTime = DateTime.UtcNow
            };

            _activeSessions[sessionId] = session;

            _logger.LogInformation(
                "Started recording session {SessionId} to {OutputPath} (PID: {ProcessId})",
                sessionId, outputPath, process.Id);

            // Monitor process completion
            _ = Task.Run(async () =>
            {
                try
                {
                    await process.WaitForExitAsync(ct);
                    _activeSessions.TryRemove(sessionId, out _);

                    _logger.LogInformation(
                        "Recording session {SessionId} completed with exit code {ExitCode}",
                        sessionId, process.ExitCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error monitoring recording session {SessionId}", sessionId);
                    _activeSessions.TryRemove(sessionId, out _);
                }
            }, ct);

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
        if (!_activeSessions.TryGetValue(sessionId, out var session))
        {
            _logger.LogWarning("Recording session {SessionId} not found", sessionId);
            return;
        }

        try
        {
            var process = Process.GetProcessById(session.ProcessId);

            // Send Ctrl+C to gracefully stop asciinema
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(ct);
            }

            _activeSessions.TryRemove(sessionId, out _);

            _logger.LogInformation(
                "Stopped recording session {SessionId} (saved to {OutputPath})",
                sessionId, session.OutputPath);
        }
        catch (ArgumentException)
        {
            // Process already exited
            _activeSessions.TryRemove(sessionId, out _);
            _logger.LogInformation("Recording session {SessionId} was already stopped", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording session {SessionId}", sessionId);
            throw;
        }
    }

    public bool IsRecording(string sessionId)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var session))
        {
            return false;
        }

        try
        {
            var process = Process.GetProcessById(session.ProcessId);
            return !process.HasExited;
        }
        catch (ArgumentException)
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
            throw new FileNotFoundException($"Recording file not found: {recordingPath}");
        }

        try
        {
            var args = new List<string> { "play", recordingPath };

            if (Math.Abs(speed - 1.0) > 0.001)
            {
                args.AddRange(new[] { "--speed", speed.ToString("F2") });
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "asciinema",
                Arguments = string.Join(" ", args.Select(arg => $"\"{arg}\"")),
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                CreateNoWindow = false
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start asciinema playback process");
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
                Description = "Start a new recording session",
                ParameterTypes = new[] { typeof(string), typeof(string) },
                ReturnType = typeof(string)
            },
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "StopRecording",
                Description = "Stop an active recording session",
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
                Description = "Get all active recording sessions",
                ReturnType = typeof(string[])
            },
            new LablabBean.Contracts.Recording.Services.ActionInfo
            {
                Name = "PlayRecording",
                Description = "Play back a recording",
                ParameterTypes = new[] { typeof(string), typeof(double) }
            }
        };
    }
}
