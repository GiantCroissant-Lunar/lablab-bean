using LablabBean.Contracts.Recording.Services;
using Microsoft.Extensions.Logging;

namespace RecordingPluginDemo;

/// <summary>
/// Test implementation of recording service that doesn't require asciinema
/// </summary>
public class TestRecordingService : IRecordingService
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, TestSession> _sessions = new();

    public TestRecordingService(ILogger logger)
    {
        _logger = logger;
    }

    public Task<string> StartRecordingAsync(string outputPath, string? title = null, CancellationToken ct = default)
    {
        var sessionId = Guid.NewGuid().ToString("N")[..8];
        var session = new TestSession
        {
            Id = sessionId,
            OutputPath = outputPath,
            Title = title,
            StartTime = DateTime.UtcNow
        };

        _sessions[sessionId] = session;
        _logger.LogInformation("Started test recording session {SessionId} to {OutputPath}", sessionId, outputPath);

        return Task.FromResult(sessionId);
    }

    public Task StopRecordingAsync(string sessionId, CancellationToken ct = default)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            _sessions.Remove(sessionId);
            _logger.LogInformation("Stopped test recording session {SessionId}", sessionId);

            // Create a dummy file to simulate the recording
            Directory.CreateDirectory(Path.GetDirectoryName(session.OutputPath)!);
            File.WriteAllText(session.OutputPath, $"Test recording from {session.StartTime} to {DateTime.UtcNow}");
        }

        return Task.CompletedTask;
    }

    public bool IsRecording(string sessionId)
    {
        return _sessions.ContainsKey(sessionId);
    }

    public IEnumerable<string> GetActiveSessions()
    {
        return _sessions.Keys.ToList();
    }

    public Task PlayRecordingAsync(string recordingPath, double speed = 1.0, CancellationToken ct = default)
    {
        if (!File.Exists(recordingPath))
        {
            throw new FileNotFoundException($"Recording file not found: {recordingPath}");
        }

        _logger.LogInformation("Playing test recording {RecordingPath} at {Speed}x speed", recordingPath, speed);
        Console.WriteLine($"📺 Playing: {Path.GetFileName(recordingPath)}");
        Console.WriteLine($"📄 Content: {File.ReadAllText(recordingPath)}");

        return Task.CompletedTask;
    }

    private record TestSession
    {
        public required string Id { get; init; }
        public required string OutputPath { get; init; }
        public string? Title { get; init; }
        public DateTime StartTime { get; init; }
    }
}
