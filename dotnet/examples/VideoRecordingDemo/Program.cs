using LablabBean.Plugins.Recording.FFmpeg.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Examples.VideoRecordingDemo;

/// <summary>
/// Demonstrates the FFmpeg video recording service capabilities
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("=== FFmpeg Video Recording Demo ===");

        // Create recording service
        var recordingService = new FFmpegVideoRecordingService(logger);

        try
        {
            // Demo 1: Basic recording
            await DemoBasicRecording(recordingService, logger);

            // Demo 2: Multiple sessions
            await DemoMultipleSessions(recordingService, logger);

            // Demo 3: Service interface
            await DemoServiceInterface(recordingService, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Demo failed");
        }

        logger.LogInformation("Demo completed. Press any key to exit...");
        Console.ReadKey();
    }

    static async Task DemoBasicRecording(FFmpegVideoRecordingService service, ILogger logger)
    {
        logger.LogInformation("\n--- Demo 1: Basic Video Recording ---");

        try
        {
            // Create output directory
            var outputDir = Path.Combine(Environment.CurrentDirectory, "demo-recordings");
            Directory.CreateDirectory(outputDir);

            var outputPath = Path.Combine(outputDir, $"demo_basic_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

            // Start recording
            logger.LogInformation("Starting 10-second video recording...");
            var sessionId = await service.StartRecordingAsync(outputPath, "Basic Demo Recording");

            logger.LogInformation($"Recording started with session ID: {sessionId}");
            logger.LogInformation("Recording your screen for 10 seconds...");

            // Record for 10 seconds
            await Task.Delay(10000);

            // Stop recording
            logger.LogInformation("Stopping recording...");
            await service.StopRecordingAsync(sessionId);

            logger.LogInformation($"Recording saved to: {outputPath}");

            // Check if file exists and show size
            if (File.Exists(outputPath))
            {
                var fileInfo = new FileInfo(outputPath);
                logger.LogInformation($"File size: {fileInfo.Length / 1024 / 1024:F2} MB");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Basic recording demo failed");
        }
    }

    static async Task DemoMultipleSessions(FFmpegVideoRecordingService service, ILogger logger)
    {
        logger.LogInformation("\n--- Demo 2: Multiple Recording Sessions ---");

        try
        {
            var outputDir = Path.Combine(Environment.CurrentDirectory, "demo-recordings");
            var sessions = new List<string>();

            // Start multiple sessions
            for (int i = 1; i <= 3; i++)
            {
                var outputPath = Path.Combine(outputDir, $"demo_session_{i}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
                var sessionId = await service.StartRecordingAsync(outputPath, $"Demo Session {i}");
                sessions.Add(sessionId);

                logger.LogInformation($"Started session {i}: {sessionId}");
                await Task.Delay(1000); // Small delay between starts
            }

            // Show active sessions
            var activeSessions = service.GetActiveSessions().ToList();
            logger.LogInformation($"Active sessions: {string.Join(", ", activeSessions)}");

            // Record for 5 seconds
            logger.LogInformation("Recording for 5 seconds...");
            await Task.Delay(5000);

            // Stop all sessions
            logger.LogInformation("Stopping all sessions...");
            foreach (var sessionId in sessions)
            {
                await service.StopRecordingAsync(sessionId);
                logger.LogInformation($"Stopped session: {sessionId}");
            }

            // Verify no active sessions
            activeSessions = service.GetActiveSessions().ToList();
            logger.LogInformation($"Active sessions after cleanup: {activeSessions.Count}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Multiple sessions demo failed");
        }
    }

    static async Task DemoServiceInterface(FFmpegVideoRecordingService service, ILogger logger)
    {
        logger.LogInformation("\n--- Demo 3: Service Interface ---");

        try
        {
            // Show supported actions
            var supportedActions = service.GetSupportedActions();
            logger.LogInformation("Supported actions:");
            foreach (var action in supportedActions)
            {
                var paramTypes = string.Join(", ", action.ParameterTypes.Select(t => t.Name));
                var returnType = action.ReturnType?.Name ?? "void";
                logger.LogInformation($"  - {action.Name}({paramTypes}) -> {returnType}: {action.Description}");
            }

            // Test action support
            logger.LogInformation($"\nSupports 'StartRecording': {service.SupportsAction("StartRecording")}");
            logger.LogInformation($"Supports 'InvalidAction': {service.SupportsAction("InvalidAction")}");

            // Use service interface for recording
            var outputDir = Path.Combine(Environment.CurrentDirectory, "demo-recordings");
            var outputPath = Path.Combine(outputDir, $"demo_service_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

            logger.LogInformation("\nUsing service interface to start recording...");
            var sessionId = service.ExecuteAction<string>("StartRecording", outputPath, "Service Interface Demo");

            logger.LogInformation($"Session started: {sessionId}");

            // Check if recording
            var isRecording = service.ExecuteAction<bool>("IsRecording", sessionId);
            logger.LogInformation($"Is recording: {isRecording}");

            // Get active sessions
            var activeSessions = service.ExecuteAction<string[]>("GetActiveSessions");
            logger.LogInformation($"Active sessions: [{string.Join(", ", activeSessions)}]");

            // Record for 3 seconds
            await Task.Delay(3000);

            // Stop using service interface
            logger.LogInformation("Stopping recording using service interface...");
            service.ExecuteAction("StopRecording", sessionId);

            logger.LogInformation("Service interface demo completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Service interface demo failed");
        }
    }
}
