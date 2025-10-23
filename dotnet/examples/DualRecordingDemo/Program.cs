using LablabBean.Plugins.Recording.Asciinema.Services;
using LablabBean.Plugins.Recording.FFmpeg.Services;
using LablabBean.Plugins.Recording.FFmpeg.Configuration;
using Microsoft.Extensions.Logging;

namespace LablabBean.Examples.DualRecordingDemo;

/// <summary>
/// Demonstrates using both Asciinema (terminal) and FFmpeg (video) recording services together
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("=== Dual Recording Demo (Terminal + Video) ===");

        // Create both recording services
        var terminalService = new AsciinemaRecordingService(logger);

        var videoOptions = new FFmpegRecordingOptions
        {
            FrameRate = 30,
            CRF = 23,
            Preset = "fast",
            ShowFFmpegOutput = false
        };
        var videoService = new FFmpegVideoRecordingService(logger, videoOptions);

        try
        {
            await DemoDualRecording(terminalService, videoService, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Demo failed");
        }

        logger.LogInformation("Demo completed. Press any key to exit...");
        Console.ReadKey();
    }

    static async Task DemoDualRecording(
        AsciinemaRecordingService terminalService,
        FFmpegVideoRecordingService videoService,
        ILogger logger)
    {
        logger.LogInformation("\n--- Dual Recording Demo ---");

        try
        {
            // Create output directory
            var outputDir = Path.Combine(Environment.CurrentDirectory, "dual-recordings");
            Directory.CreateDirectory(outputDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var terminalPath = Path.Combine(outputDir, $"terminal_{timestamp}.cast");
            var videoPath = Path.Combine(outputDir, $"video_{timestamp}.mp4");

            logger.LogInformation("Starting dual recording (terminal + video)...");

            // Start both recordings simultaneously
            var terminalSessionId = await terminalService.StartRecordingAsync(
                terminalPath, "Dual Demo - Terminal");

            var videoSessionId = await videoService.StartRecordingAsync(
                videoPath, "Dual Demo - Video");

            logger.LogInformation($"Terminal recording: {terminalSessionId}");
            logger.LogInformation($"Video recording: {videoSessionId}");

            // Simulate some activity
            logger.LogInformation("Recording for 15 seconds...");
            logger.LogInformation("You can interact with your terminal and desktop now!");

            // Show recording status every 3 seconds
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(3000);

                var terminalActive = terminalService.IsRecording(terminalSessionId);
                var videoActive = videoService.IsRecording(videoSessionId);

                logger.LogInformation($"Status check {i + 1}/5 - Terminal: {(terminalActive ? "Recording" : "Stopped")}, Video: {(videoActive ? "Recording" : "Stopped")}");
            }

            // Stop both recordings
            logger.LogInformation("Stopping recordings...");

            await terminalService.StopRecordingAsync(terminalSessionId);
            await videoService.StopRecordingAsync(videoSessionId);

            logger.LogInformation("Both recordings stopped successfully!");

            // Show file information
            if (File.Exists(terminalPath))
            {
                var terminalSize = new FileInfo(terminalPath).Length;
                logger.LogInformation($"Terminal recording: {terminalPath} ({terminalSize} bytes)");
            }

            if (File.Exists(videoPath))
            {
                var videoSize = new FileInfo(videoPath).Length;
                logger.LogInformation($"Video recording: {videoPath} ({videoSize / 1024 / 1024:F2} MB)");
            }

            // Demonstrate playback capabilities
            logger.LogInformation("\nPlayback options:");
            logger.LogInformation($"Terminal: asciinema play \"{terminalPath}\"");
            logger.LogInformation($"Video: ffplay \"{videoPath}\" (or any video player)");

            // Optional: Demonstrate programmatic playback
            logger.LogInformation("\nWould you like to play back the terminal recording? (y/n)");
            var response = Console.ReadLine();

            if (response?.ToLower() == "y")
            {
                logger.LogInformation("Playing terminal recording...");
                await terminalService.PlayRecordingAsync(terminalPath, 2.0); // 2x speed
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dual recording demo failed");
        }
    }
}
