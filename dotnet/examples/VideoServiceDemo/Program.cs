using LablabBean.Contracts.Video.Models;
using LablabBean.Plugins.Video.FFmpeg.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Examples.VideoServiceDemo;

/// <summary>
/// Comprehensive demo of the FFmpeg video service capabilities
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("=== FFmpeg Video Service Demo ===");

        // Create the unified video service
        var videoService = new FFmpegVideoService(logger);

        try
        {
            // Demo 1: Video Recording
            await DemoVideoRecording(videoService, logger);

            // Demo 2: Video Playback
            await DemoVideoPlayback(videoService, logger);

            // Demo 3: Video Information
            await DemoVideoInfo(videoService, logger);

            // Demo 4: Video Conversion
            await DemoVideoConversion(videoService, logger);

            // Demo 5: Session Management
            await DemoSessionManagement(videoService, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Demo failed");
        }

        logger.LogInformation("Demo completed. Press any key to exit...");
        Console.ReadKey();
    }

    static async Task DemoVideoRecording(FFmpegVideoService service, ILogger logger)
    {
        logger.LogInformation("\n--- Demo 1: Video Recording ---");

        try
        {
            var outputDir = Path.Combine(Environment.CurrentDirectory, "demo-videos");
            Directory.CreateDirectory(outputDir);

            var outputPath = Path.Combine(outputDir, $"screen_recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

            var recordingOptions = new VideoRecordingOptions
            {
                Title = "Screen Recording Demo",
                FrameRate = 30,
                Quality = 23,
                Preset = "fast",
                Source = VideoRecordingSource.Screen,
                Resolution = "1920x1080",
                MaxDurationSeconds = 10
            };

            logger.LogInformation("Starting 10-second screen recording...");
            var sessionId = await service.StartRecordingAsync(outputPath, recordingOptions);

            logger.LogInformation($"Recording session started: {sessionId}");
            logger.LogInformation("Recording your screen for 10 seconds...");

            // Monitor recording status
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                var isRecording = service.IsRecording(sessionId);
                logger.LogInformation($"Recording status ({i + 1}/10): {(isRecording ? "Active" : "Stopped")}");
            }

            // Stop recording
            await service.StopRecordingAsync(sessionId);
            logger.LogInformation($"Recording saved to: {outputPath}");

            // Check file
            if (File.Exists(outputPath))
            {
                var fileInfo = new FileInfo(outputPath);
                logger.LogInformation($"File size: {fileInfo.Length / 1024 / 1024:F2} MB");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Video recording demo failed");
        }
    }

    static async Task DemoVideoPlayback(FFmpegVideoService service, ILogger logger)
    {
        logger.LogInformation("\n--- Demo 2: Video Playback ---");

        try
        {
            var videoDir = Path.Combine(Environment.CurrentDirectory, "demo-videos");
            var videoFiles = Directory.GetFiles(videoDir, "*.mp4");

            if (videoFiles.Length == 0)
            {
                logger.LogWarning("No video files found for playback demo");
                return;
            }

            var videoPath = videoFiles[0];
            logger.LogInformation($"Playing video: {Path.GetFileName(videoPath)}");

            var playbackOptions = new VideoPlaybackOptions
            {
                Speed = 2.0, // 2x speed
                Volume = 0.5,
                WindowTitle = "Video Service Demo Playback",
                StartPosition = 0,
                EndPosition = 5 // Play first 5 seconds only
            };

            var sessionId = await service.PlayVideoAsync(videoPath, playbackOptions);
            logger.LogInformation($"Playback session started: {sessionId}");

            // Monitor playback
            while (service.IsPlaying(sessionId))
            {
                await Task.Delay(500);
                logger.LogInformation("Playing...");
            }

            logger.LogInformation("Playback completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Video playback demo failed");
        }
    }

    static async Task DemoVideoInfo(FFmpegVideoService service, ILogger logger)
    {
        logger.LogInformation("\n--- Demo 3: Video Information ---");

        try
        {
            var videoDir = Path.Combine(Environment.CurrentDirectory, "demo-videos");
            var videoFiles = Directory.GetFiles(videoDir, "*.mp4");

            if (videoFiles.Length == 0)
            {
                logger.LogWarning("No video files found for info demo");
                return;
            }

            foreach (var videoPath in videoFiles.Take(2)) // Analyze first 2 videos
            {
                logger.LogInformation($"\nAnalyzing: {Path.GetFileName(videoPath)}");

                var videoInfo = await service.GetVideoInfoAsync(videoPath);

                logger.LogInformation($"  Duration: {videoInfo.Duration:F2} seconds");
                logger.LogInformation($"  Resolution: {videoInfo.Width}x{videoInfo.Height}");
                logger.LogInformation($"  Frame Rate: {videoInfo.FrameRate:F2} fps");
                logger.LogInformation($"  Video Codec: {videoInfo.VideoCodec}");
                logger.LogInformation($"  Audio Codec: {videoInfo.AudioCodec ?? "None"}");
                logger.LogInformation($"  File Size: {videoInfo.FileSize / 1024 / 1024:F2} MB");
                logger.LogInformation($"  Bitrate: {videoInfo.Bitrate / 1000:F0} kbps");
                logger.LogInformation($"  Has Audio: {videoInfo.HasAudio}");
                logger.LogInformation($"  Created: {videoInfo.CreationTime}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Video info demo failed");
        }
    }

    static async Task DemoVideoConversion(FFmpegVideoService service, ILogger logger)
    {
        logger.LogInformation("\n--- Demo 4: Video Conversion ---");

        try
        {
            var videoDir = Path.Combine(Environment.CurrentDirectory, "demo-videos");
            var videoFiles = Directory.GetFiles(videoDir, "*.mp4");

            if (videoFiles.Length == 0)
            {
                logger.LogWarning("No video files found for conversion demo");
                return;
            }

            var inputPath = videoFiles[0];
            var outputPath = Path.Combine(videoDir, $"converted_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

            logger.LogInformation($"Converting: {Path.GetFileName(inputPath)}");

            var conversionOptions = new VideoConversionOptions
            {
                VideoCodec = "libx264",
                Quality = 28, // Lower quality for smaller file
                Preset = "fast",
                Resolution = "1280x720", // Downscale to 720p
                FrameRate = 24,
                StartTime = 0,
                Duration = 5 // Convert only first 5 seconds
            };

            var sessionId = await service.ConvertVideoAsync(inputPath, outputPath, conversionOptions);
            logger.LogInformation($"Conversion session started: {sessionId}");

            // Monitor conversion progress
            var startTime = DateTime.Now;
            while (service.GetActiveSessions().Contains(sessionId))
            {
                await Task.Delay(1000);
                var elapsed = DateTime.Now - startTime;
                logger.LogInformation($"Converting... ({elapsed.TotalSeconds:F0}s elapsed)");
            }

            logger.LogInformation($"Conversion completed: {Path.GetFileName(outputPath)}");

            // Compare file sizes
            if (File.Exists(outputPath))
            {
                var originalSize = new FileInfo(inputPath).Length;
                var convertedSize = new FileInfo(outputPath).Length;
                var compressionRatio = (1.0 - (double)convertedSize / originalSize) * 100;

                logger.LogInformation($"Original size: {originalSize / 1024 / 1024:F2} MB");
                logger.LogInformation($"Converted size: {convertedSize / 1024 / 1024:F2} MB");
                logger.LogInformation($"Compression: {compressionRatio:F1}%");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Video conversion demo failed");
        }
    }

    static async Task DemoSessionManagement(FFmpegVideoService service, ILogger logger)
    {
        logger.LogInformation("\n--- Demo 5: Session Management ---");

        try
        {
            var outputDir = Path.Combine(Environment.CurrentDirectory, "demo-videos");
            var sessions = new List<string>();

            // Start multiple recording sessions
            logger.LogInformation("Starting multiple recording sessions...");
            for (int i = 1; i <= 3; i++)
            {
                var outputPath = Path.Combine(outputDir, $"session_{i}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
                var options = new VideoRecordingOptions
                {
                    Title = $"Session {i}",
                    MaxDurationSeconds = 5,
                    Quality = 28 // Lower quality for demo
                };

                var sessionId = await service.StartRecordingAsync(outputPath, options);
                sessions.Add(sessionId);
                logger.LogInformation($"Started session {i}: {sessionId}");

                await Task.Delay(500); // Small delay between starts
            }

            // Show active sessions
            var activeSessions = service.GetActiveSessions().ToList();
            var activeRecordings = service.GetActiveRecordingSessions().ToList();

            logger.LogInformation($"Total active sessions: {activeSessions.Count}");
            logger.LogInformation($"Active recordings: {activeRecordings.Count}");
            logger.LogInformation($"Sessions: [{string.Join(", ", activeSessions)}]");

            // Wait for recordings to complete (they have 5-second limit)
            logger.LogInformation("Waiting for recordings to complete...");
            await Task.Delay(6000);

            // Check final status
            activeSessions = service.GetActiveSessions().ToList();
            activeRecordings = service.GetActiveRecordingSessions().ToList();

            logger.LogInformation($"Final active sessions: {activeSessions.Count}");
            logger.LogInformation($"Final active recordings: {activeRecordings.Count}");

            // Clean up any remaining sessions
            foreach (var sessionId in sessions)
            {
                if (service.IsRecording(sessionId))
                {
                    await service.StopRecordingAsync(sessionId);
                    logger.LogInformation($"Manually stopped session: {sessionId}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Session management demo failed");
        }
    }
}
