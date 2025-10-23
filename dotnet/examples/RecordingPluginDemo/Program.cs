using LablabBean.Contracts.Recording.Services;
using LablabBean.Plugins.Recording.Asciinema.Services;
using Microsoft.Extensions.Logging;

namespace RecordingPluginDemo;

/// <summary>
/// Demo application showing how to use the Recording plugin with asciinema
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== LablabBean Recording Plugin Demo ===");
        Console.WriteLine("This demo shows how to use asciinema recording capabilities.");
        Console.WriteLine();

        // Create a simple logger
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        var logger = loggerFactory.CreateLogger("RecordingDemo");

        // Check if asciinema is installed
        IRecordingService recordingService;
        if (await IsAsciinemaInstalled())
        {
            Console.WriteLine("✅ asciinema is available - using real recording service");
            recordingService = new AsciinemaRecordingService(logger);
        }
        else
        {
            Console.WriteLine("⚠️ asciinema is not installed - using test recording service");
            Console.WriteLine("💡 Install asciinema for real terminal recording:");
            Console.WriteLine("  - On Ubuntu/Debian: sudo apt install asciinema");
            Console.WriteLine("  - On macOS: brew install asciinema");
            Console.WriteLine("  - On Windows: pip install asciinema");
            Console.WriteLine("  - Or visit: https://asciinema.org/docs/installation");
            recordingService = new TestRecordingService(logger);
        }
        Console.WriteLine();

        Console.WriteLine("🎬 Recording Service created successfully!");
        Console.WriteLine();

        // Demo menu
        while (true)
        {
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Start manual recording");
            Console.WriteLine("2. Stop recording");
            Console.WriteLine("3. List active sessions");
            Console.WriteLine("4. Play recording");
            Console.WriteLine("5. Exit");
            Console.Write("Enter choice (1-5): ");

            var choice = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await StartManualRecording(recordingService);
                        break;
                    case "2":
                        await StopRecording(recordingService);
                        break;
                    case "3":
                        ListActiveSessions(recordingService);
                        break;
                    case "4":
                        await PlayRecording(recordingService);
                        break;
                    case "5":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    static async Task<bool> IsAsciinemaInstalled()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "asciinema",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    static async Task StartManualRecording(IRecordingService recordingService)
    {
        Console.Write("Enter output filename (without extension): ");
        var filename = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(filename))
        {
            Console.WriteLine("❌ Invalid filename");
            return;
        }

        Console.Write("Enter recording title (optional): ");
        var title = Console.ReadLine();

        var outputPath = Path.Combine("recordings", $"{filename}.cast");
        var sessionId = await recordingService.StartRecordingAsync(outputPath, title);

        Console.WriteLine($"🎬 Started recording session: {sessionId}");
        Console.WriteLine($"📁 Output: {outputPath}");
        Console.WriteLine("💡 The recording will capture this terminal session.");
        Console.WriteLine("   Press Ctrl+C in the terminal to stop recording.");
    }

    static async Task StopRecording(IRecordingService recordingService)
    {
        var sessions = recordingService.GetActiveSessions().ToList();

        if (!sessions.Any())
        {
            Console.WriteLine("❌ No active recording sessions");
            return;
        }

        Console.WriteLine("Active sessions:");
        for (int i = 0; i < sessions.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {sessions[i]}");
        }

        Console.Write("Enter session number to stop: ");
        if (int.TryParse(Console.ReadLine(), out var index) &&
            index > 0 && index <= sessions.Count)
        {
            var sessionId = sessions[index - 1];
            await recordingService.StopRecordingAsync(sessionId);
            Console.WriteLine($"⏹️ Stopped recording session: {sessionId}");
        }
        else
        {
            Console.WriteLine("❌ Invalid session number");
        }
    }

    static void ListActiveSessions(IRecordingService recordingService)
    {
        var sessions = recordingService.GetActiveSessions().ToList();

        if (!sessions.Any())
        {
            Console.WriteLine("📭 No active recording sessions");
            return;
        }

        Console.WriteLine($"📹 Active recording sessions ({sessions.Count}):");
        foreach (var session in sessions)
        {
            var isActive = recordingService.IsRecording(session);
            var status = isActive ? "🔴 Recording" : "⏸️ Stopped";
            Console.WriteLine($"  {session} - {status}");
        }
    }



    static async Task PlayRecording(IRecordingService recordingService)
    {
        var recordingsDir = "recordings";

        if (!Directory.Exists(recordingsDir))
        {
            Console.WriteLine("❌ No recordings directory found");
            return;
        }

        var recordings = Directory.GetFiles(recordingsDir, "*.cast");

        if (!recordings.Any())
        {
            Console.WriteLine("❌ No recordings found in recordings/ directory");
            return;
        }

        Console.WriteLine("Available recordings:");
        for (int i = 0; i < recordings.Length; i++)
        {
            var filename = Path.GetFileName(recordings[i]);
            var fileInfo = new FileInfo(recordings[i]);
            Console.WriteLine($"{i + 1}. {filename} ({fileInfo.Length} bytes, {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm})");
        }

        Console.Write("Enter recording number to play: ");
        if (int.TryParse(Console.ReadLine(), out var index) &&
            index > 0 && index <= recordings.Length)
        {
            var recordingPath = recordings[index - 1];

            Console.Write("Enter playback speed (default 1.0): ");
            var speedInput = Console.ReadLine();
            var speed = double.TryParse(speedInput, out var s) ? s : 1.0;

            Console.WriteLine($"▶️ Playing {Path.GetFileName(recordingPath)} at {speed}x speed...");
            Console.WriteLine("💡 Press 'q' during playback to quit");

            await recordingService.PlayRecordingAsync(recordingPath, speed);
            Console.WriteLine("✅ Playback completed");
        }
        else
        {
            Console.WriteLine("❌ Invalid recording number");
        }
    }
}
