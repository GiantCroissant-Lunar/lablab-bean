using LablabBean.Contracts.Video.Services;
using LablabBean.Plugins.Recording.Video.Configuration;
using LablabBean.Plugins.Recording.Video.Services;
using LablabBean.Plugins.Video.FFmpeg.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Examples.RecordingServiceDemo;

/// <summary>
/// Demonstrates the new recording service architecture that uses video service
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("=== Recording Service Architecture Demo ===");

        try
        {
            // Demo the new architecture
            await DemoRecordingServiceArchitecture(logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Demo failed");
        }

        logger.LogInformation("Demo completed. Press any key to exit...");
        Console.ReadKey();
    }

    static async Task DemoRecordingServiceArchitecture(ILogger logger)
    {
        logger.LogInformation("\n--- Recording Service Architecture Demo ---");

        // Step 1: Create the video service (foundation layer)
        logger.LogInformation("1. Creating video service (foundation layer)...");
        var videoService = new FFmpegVideoService(logger);
        logger.LogInformation("   ✓ Video service created - provides core video capabilities");

        // Step 2: Create recording configuration
        logger.LogInformation("\n2. Creating recording configuration...");
        var recordingConfig = new RecordingConfiguration
        {
            GameRecording = new GameRecordingOptions
            {
                FrameRate = 30,
                Quality = 25, // Lower quality for game recording
                Preset = "fast",
                RecordAudio = false,
                MaxDurationSeconds = 300 // 5 minutes for demo
            },
            ManualRecording = new ManualRecordingOptions
            {
                FrameRate = 30,
                Quality = 20, // Higher quality for manual recording
                Preset = "medium",
                RecordAudio = true,
                MaxDurationSeconds = 0 // No limit
            },
            Output = new OutputSettings
            {
                BaseDirectory = "demo-recordings",
                VideoSubdirectory = "video",
                GameRecordingPattern = "game_{timestamp}.mp4",
                ManualRecordingPattern = "manual_{timestamp}.mp4"
            },
            AutoRecording = new AutoRecordingSettings
            {
                EnableAutoRecording = true,
                MinimumSessionDuration = 5,
                MaxConcurrentRecordings = 2
            }
        };
        logger.LogInformation("   ✓ Recording configuration created");

        // Step 3: Create recording service (uses video service)
        logger.LogInformation("\n3. Creating recording service (uses video service)...");
        var recordingService = new VideoRecordingService(
            logger,
            videoService, // IVideoRecordingService
            videoService, // IVideoService
            recordingConfig);
        logger.LogInformation("   ✓ Recording service created - delegates to video service");

        // Step 4: Demonstrate the architecture benefits
        await DemoArchitectureBenefits(recordingService, videoService, logger);

        // Step 5: Demonstrate different recording types
        await DemoDifferentRecordingTypes(recordingService, logger);

        // Step 6: Demonstrate service capabilities
        await DemoServiceCapabilities(recordingService, logger);
    }

    static async Task DemoArchitectureBenefits(
        VideoRecordingService recordingService,
        FFmpegVideoService videoService,
        ILogger logger)
    {
        logger.LogInformation("\n--- Architecture Benefits Demo ---");

        try
        {
            // Benefit 1: Recording service focuses on recording workflow
            logger.LogInformation("Benefit 1: Recording service provides recording-focused interface");
            var outputDir = Path.Combine(Environment.CurrentDirectory, "demo-recordings", "video");
            Directory.CreateDirectory(outputDir);

            var recordingPath = Path.Combine(outputDir, $"architecture_demo_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

            // Recording service provides simple, recording-focused methods
            var sessionId = await recordingService.StartRecordingAsync(recordingPath, "Architecture Demo");
            logger.LogInformation($"   ✓ Started recording with simple interface: {sessionId}");

            await Task.Delay(3000); // Record for 3 seconds

            await recordingService.StopRecordingAsync(sessionId);
            logger.LogInformation("   ✓ Stopped recording - workflow managed by recording service");

            // Benefit 2: Video service provides comprehensive video capabilities
            logger.LogInformation("\nBenefit 2: Video service provides comprehensive video capabilities");

            if (File.Exists(recordingPath))
            {
                // Video service can analyze the recording
                var videoInfo = await videoService.GetVideoInfoAsync(recordingPath);
                logger.LogInformation($"   ✓ Video analysis: {videoInfo.Duration:F1}s, {videoInfo.Width}x{videoInfo.Height}");

                // Video service can convert the recording
                var convertedPath = Path.Combine(outputDir, $"converted_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
                var conversionSessionId = await videoService.ConvertVideoAsync(recordingPath, convertedPath);
                logger.LogInformation($"   ✓ Started video conversion: {conversionSessionId}");

                // Wait a bit for conversion to start
                await Task.Delay(2000);

                // Check if conversion is still running
                var activeSessions = videoService.GetActiveSessions();
                logger.LogInformation($"   ✓ Active video sessions: {activeSessions.Count()}");
            }

            // Benefit 3: Clean separation of concerns
            logger.LogInformation("\nBenefit 3: Clean separation of concerns");
            logger.LogInformation("   ✓ Recording service: Workflow management, game integration, recording logic");
            logger.LogInformation("   ✓ Video service: FFmpeg operations, video processing, format handling");
            logger.LogInformation("   ✓ Both services can be used independently or together");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Architecture benefits demo failed");
        }
    }

    static async Task DemoDifferentRecordingTypes(VideoRecordingService recordingService, ILogger logger)
    {
        logger.LogInformation("\n--- Different Recording Types Demo ---");

        try
        {
            var outputDir = Path.Combine(Environment.CurrentDirectory, "demo-recordings", "video");

            // Manual recording (higher quality, with audio)
            logger.LogInformation("Starting manual recording (higher quality, with audio)...");
            var manualPath = Path.Combine(outputDir, $"manual_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
            var manualSessionId = await recordingService.StartRecordingAsync(manualPath, "Manual Recording Demo");

            await Task.Delay(2000);

            await recordingService.StopRecordingAsync(manualSessionId);
            logger.LogInformation("   ✓ Manual recording completed");

            // Game recording (optimized for performance)
            logger.LogInformation("\nStarting game recording (optimized for performance)...");
            var gamePath = Path.Combine(outputDir, $"game_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
            var gameSessionId = await recordingService.StartGameRecordingAsync(gamePath, "Game Recording Demo");

            await Task.Delay(2000);

            await recordingService.StopRecordingAsync(gameSessionId);
            logger.LogInformation("   ✓ Game recording completed");

            // Compare the recordings
            logger.LogInformation("\nRecording comparison:");
            logger.LogInformation("   Manual: Higher quality preset, audio enabled, no time limit");
            logger.LogInformation("   Game: Performance preset, no audio, time limited");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Different recording types demo failed");
        }
    }

    static async Task DemoServiceCapabilities(VideoRecordingService recordingService, ILogger logger)
    {
        logger.LogInformation("\n--- Service Capabilities Demo ---");

        try
        {
            // Show supported actions
            logger.LogInformation("Supported recording service actions:");
            var supportedActions = recordingService.GetSupportedActions();
            foreach (var action in supportedActions)
            {
                var paramTypes = string.Join(", ", action.ParameterTypes.Select(t => t.Name));
                var returnType = action.ReturnType?.Name ?? "void";
                logger.LogInformation($"   - {action.Name}({paramTypes}) -> {returnType}: {action.Description}");
            }

            // Demonstrate action-based interface
            logger.LogInformation("\nDemonstrating action-based interface:");

            var outputDir = Path.Combine(Environment.CurrentDirectory, "demo-recordings", "video");
            var actionPath = Path.Combine(outputDir, $"action_demo_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

            // Start recording via action interface
            var sessionId = recordingService.ExecuteAction<string>("StartRecording", actionPath, "Action Demo");
            logger.LogInformation($"   ✓ Started recording via action: {sessionId}");

            // Check status via action interface
            var isRecording = recordingService.ExecuteAction<bool>("IsRecording", sessionId);
            logger.LogInformation($"   ✓ Recording status via action: {isRecording}");

            await Task.Delay(2000);

            // Stop recording via action interface
            recordingService.ExecuteAction("StopRecording", sessionId);
            logger.LogInformation("   ✓ Stopped recording via action interface");

            // Get active sessions
            var activeSessions = recordingService.ExecuteAction<string[]>("GetActiveSessions");
            logger.LogInformation($"   ✓ Active sessions: {activeSessions.Length}");

            // Show session management
            logger.LogInformation("\nSession management:");
            var allSessions = recordingService.GetActiveSessions();
            logger.LogInformation($"   Active recording sessions: {allSessions.Count()}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Service capabilities demo failed");
        }
    }
}
