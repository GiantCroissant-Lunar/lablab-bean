# LablabBean Recording Plugin (Asciinema)

A plugin that provides terminal recording capabilities using [asciinema](https://asciinema.org/), allowing you to record and playback terminal sessions during gameplay.

## Features

- **Automatic Recording**: Automatically starts/stops recording when game sessions begin/end
- **Manual Recording**: Start and stop recordings on demand
- **Session Management**: Track multiple concurrent recording sessions
- **Playback Support**: Play back recorded sessions with configurable speed
- **Event Integration**: Publishes recording events for other plugins to consume

## Prerequisites

### Install asciinema

The plugin requires asciinema to be installed and available in your system PATH:

**Ubuntu/Debian:**

```bash
sudo apt install asciinema
```

**macOS:**

```bash
brew install asciinema
```

**Windows:**

```bash
pip install asciinema
```

**Other platforms:**
Visit [asciinema installation guide](https://asciinema.org/docs/installation)

### Verify Installation

```bash
asciinema --version
```

## Usage

### Plugin Registration

The plugin automatically registers the `IRecordingService` when loaded:

```csharp
// The plugin registers itself and provides IRecordingService
var recordingService = context.Registry.Get<IRecordingService>();
```

### Manual Recording

```csharp
// Start recording
var sessionId = await recordingService.StartRecordingAsync(
    outputPath: "recordings/my-session.cast",
    title: "My Game Session"
);

// Stop recording
await recordingService.StopRecordingAsync(sessionId);

// Check if recording is active
bool isActive = recordingService.IsRecording(sessionId);

// Get all active sessions
var activeSessions = recordingService.GetActiveSessions();
```

### Automatic Recording

The plugin automatically listens for game lifecycle events:

- **GameStartedEvent**: Starts recording to `recordings/game_session_{timestamp}.cast`
- **GameEndedEvent**: Stops the current recording

### Playback

```csharp
// Play recording at normal speed
await recordingService.PlayRecordingAsync("recordings/my-session.cast");

// Play at 2x speed
await recordingService.PlayRecordingAsync("recordings/my-session.cast", speed: 2.0);
```

## Events

The plugin publishes the following events:

### RecordingStartedEvent

```csharp
public record RecordingStartedEvent
{
    public required string SessionId { get; init; }
    public required string OutputPath { get; init; }
    public string? Title { get; init; }
    public DateTime StartTime { get; init; }
}
```

### RecordingStoppedEvent

```csharp
public record RecordingStoppedEvent
{
    public required string SessionId { get; init; }
    public required string OutputPath { get; init; }
    public DateTime StopTime { get; init; }
    public TimeSpan Duration { get; init; }
    public bool WasSuccessful { get; init; }
}
```

## File Format

Recordings are saved in asciinema's `.cast` format, which is:

- **Lightweight**: Text-based JSON format
- **Portable**: Can be shared and played on any system with asciinema
- **Web-compatible**: Can be embedded in web pages using asciinema player

## Demo Application

Run the demo to see the plugin in action:

```bash
cd examples/RecordingPluginDemo
dotnet run
```

The demo provides an interactive menu to:

- Start/stop manual recordings
- List active sessions
- Simulate game sessions with auto-recording
- Play back existing recordings

## Configuration

### Output Directory

By default, recordings are saved to a `recordings/` directory in the current working directory. The directory is created automatically if it doesn't exist.

### File Naming

- **Manual recordings**: Use the path you specify
- **Auto recordings**: `game_session_{timestamp}.cast`

### Recording Options

The plugin supports additional asciinema options through the `RecordingOptions` model:

```csharp
public record RecordingOptions
{
    public int MaxDurationSeconds { get; init; } = 0; // 0 = unlimited
    public bool OverwriteExisting { get; init; } = false;
    public string[] AdditionalOptions { get; init; } = Array.Empty<string>();
}
```

## Troubleshooting

### "asciinema not found"

- Ensure asciinema is installed and in your PATH
- Try running `asciinema --version` in your terminal

### "Permission denied"

- Check that the output directory is writable
- Ensure you have permissions to create files in the target location

### Recording not stopping

- The plugin sends SIGTERM to the asciinema process
- If the process doesn't respond, it will be force-killed after a timeout

### Playback issues

- Ensure the recording file exists and is not corrupted
- Check that the file has the `.cast` extension
- Verify the file was created successfully (non-zero size)

## Integration with Other Plugins

The recording plugin works well with:

- **Analytics Plugin**: Track recording metrics and usage patterns
- **Reporting Plugin**: Include recording links in game reports
- **Config Plugin**: Store recording preferences and settings

## Architecture

```
RecordingPlugin
├── AsciinemaRecordingService (IRecordingService)
├── Event Handlers (GameStarted/GameEnded)
└── Process Management (asciinema processes)
```

The plugin follows the standard LablabBean plugin architecture:

- Implements `IPlugin` interface
- Registers services with the plugin registry
- Subscribes to relevant events
- Provides clean startup/shutdown lifecycle

## License

This plugin is part of the LablabBean project and follows the same licensing terms.
