# FFmpeg Video Service Plugin

A comprehensive video service plugin for Lablab Bean that provides recording, playback, conversion, and analysis capabilities using FFmpeg. This plugin supersedes the basic recording-only plugin by offering a complete video processing solution.

## Features

### 🎥 Video Recording

- **Cross-platform screen recording** (Windows, Linux, macOS)
- **Multiple recording sources**: Screen, Window, Region, Camera
- **Configurable quality settings**: Frame rate, codec, bitrate, resolution
- **Automatic game session recording** via event integration
- **Real-time recording monitoring** and session management

### ▶️ Video Playback

- **Advanced playback controls**: Speed, volume, position, looping
- **Fullscreen and windowed playback**
- **Subtitle and audio track selection**
- **Frame-accurate seeking** and trimming

### 🔄 Video Conversion

- **Format conversion** between popular video formats
- **Quality and resolution adjustment**
- **Video trimming and cropping**
- **Batch processing support**
- **Progress monitoring** with real-time updates

### 📊 Video Analysis

- **Comprehensive video information** extraction
- **Metadata parsing** (duration, resolution, codecs, bitrate)
- **Frame analysis** and thumbnail generation
- **Quality assessment** and optimization suggestions

## Architecture

### Service Interfaces

The plugin implements multiple service interfaces for maximum flexibility:

```csharp
// Core video operations
IVideoService
├── PlayVideoAsync()
├── GetVideoInfoAsync()
├── ConvertVideoAsync()
└── GetActiveSessions()

// Recording-specific operations
IVideoRecordingService
├── StartRecordingAsync()
├── StopRecordingAsync()
├── IsRecording()
└── GetActiveRecordingSessions()

// Backward compatibility
IService (Recording Contracts)
├── ExecuteAction<T>()
├── ExecuteAction()
├── SupportsAction()
└── GetSupportedActions()
```

### Unified Service Design

Unlike the previous separate recording service, this plugin provides a single `FFmpegVideoService` that handles all video operations:

```csharp
var videoService = new FFmpegVideoService(logger);

// Use as recording service
var recordingSessionId = await videoService.StartRecordingAsync("output.mp4", options);

// Use as playback service
var playbackSessionId = await videoService.PlayVideoAsync("input.mp4", playbackOptions);

// Use as conversion service
var conversionSessionId = await videoService.ConvertVideoAsync("input.mp4", "output.avi", conversionOptions);

// Get video information
var videoInfo = await videoService.GetVideoInfoAsync("video.mp4");
```

## Usage Examples

### Basic Recording

```csharp
var options = new VideoRecordingOptions
{
    Title = "My Recording",
    FrameRate = 60,
    Quality = 18, // High quality
    Preset = "medium",
    Source = VideoRecordingSource.Screen,
    Resolution = "1920x1080",
    RecordAudio = true
};

var sessionId = await videoService.StartRecordingAsync("recording.mp4", options);
// ... record for some time ...
await videoService.StopRecordingAsync(sessionId);
```

### Advanced Playback

```csharp
var playbackOptions = new VideoPlaybackOptions
{
    Speed = 1.5, // 1.5x speed
    StartPosition = 30, // Start at 30 seconds
    EndPosition = 120, // End at 2 minutes
    Volume = 0.8,
    Fullscreen = true,
    Loop = false
};

var sessionId = await videoService.PlayVideoAsync("video.mp4", playbackOptions);
```

### Video Conversion

```csharp
var conversionOptions = new VideoConversionOptions
{
    VideoCodec = "libx265", // HEVC for better compression
    Quality = 28,
    Resolution = "1280x720", // Downscale to 720p
    FrameRate = 30,
    StartTime = 10, // Trim first 10 seconds
    Duration = 60 // Keep only 1 minute
};

var sessionId = await videoService.ConvertVideoAsync("input.mp4", "output.mp4", conversionOptions);
```

### Video Information

```csharp
var info = await videoService.GetVideoInfoAsync("video.mp4");

Console.WriteLine($"Duration: {info.Duration:F2} seconds");
Console.WriteLine($"Resolution: {info.Width}x{info.Height}");
Console.WriteLine($"Frame Rate: {info.FrameRate:F2} fps");
Console.WriteLine($"File Size: {info.FileSize / 1024 / 1024:F2} MB");
Console.WriteLine($"Video Codec: {info.VideoCodec}");
Console.WriteLine($"Has Audio: {info.HasAudio}");
```

## Configuration

### Recording Options

```csharp
var recordingOptions = new VideoRecordingOptions
{
    // Basic settings
    Title = "Game Session Recording",
    VideoCodec = "libx264", // or "libx265", "h264_nvenc"
    Preset = "fast", // ultrafast, fast, medium, slow, veryslow
    Quality = 23, // CRF value (0-51, lower = better)
    FrameRate = 30,

    // Source configuration
    Source = VideoRecordingSource.Screen,
    Resolution = "1920x1080", // or null for auto-detect

    // Audio settings
    RecordAudio = true,
    AudioDevice = "default", // platform-specific

    // Limits
    MaxDurationSeconds = 3600, // 1 hour max

    // Platform-specific settings
    PlatformSettings = new Dictionary<string, object>
    {
        ["WindowTitle"] = "Game Window", // Windows only
        ["Display"] = ":0.0", // Linux only
        ["ScreenDevice"] = 1 // macOS only
    }
};
```

### Playback Options

```csharp
var playbackOptions = new VideoPlaybackOptions
{
    Speed = 1.0, // Playback speed multiplier
    StartPosition = 0, // Start time in seconds
    EndPosition = 0, // End time (0 = play to end)
    Volume = 1.0, // Volume level (0.0 to 1.0)
    Mute = false,
    Loop = false,
    Fullscreen = false,
    WindowTitle = "Video Player"
};
```

### Conversion Options

```csharp
var conversionOptions = new VideoConversionOptions
{
    // Output format
    VideoCodec = "libx264",
    AudioCodec = "aac",
    Quality = 23,
    Preset = "medium",

    // Resolution and frame rate
    Resolution = "1280x720",
    FrameRate = 30,

    // Trimming
    StartTime = 0, // Start time in seconds
    Duration = null, // Duration in seconds (null = entire video)

    // Advanced options
    AdditionalArgs = new[] { "-tune", "film" }
};
```

## Platform Support

### Windows

- **Screen Recording**: `gdigrab` input format
- **Window Recording**: Window title-based capture
- **Audio**: DirectShow (`dshow`) input format
- **Hardware Acceleration**: NVENC, QSV, AMF support

### Linux

- **Screen Recording**: `x11grab` input format
- **Display Selection**: Multi-monitor support
- **Audio**: ALSA (`alsa`) input format
- **Wayland**: Limited support (requires additional setup)

### macOS

- **Screen Recording**: `avfoundation` input format
- **Permission Requirements**: Screen recording permission needed
- **Audio**: AVFoundation audio capture
- **Hardware Acceleration**: VideoToolbox support

## Event Integration

The plugin integrates with the Lablab Bean event system:

### Published Events

```csharp
// Video recording events
VideoRecordingStartedEvent
VideoRecordingStoppedEvent

// Video playback events
VideoPlaybackStartedEvent
VideoPlaybackStoppedEvent

// Video conversion events
VideoConversionStartedEvent
VideoConversionCompletedEvent
VideoConversionProgressEvent

// Backward compatibility
RecordingStartedEvent
RecordingStoppedEvent
```

### Game Event Subscription

```csharp
// Automatic recording on game events
GameStartedEvent → Start video recording
GameEndedEvent → Stop video recording
```

## Performance Considerations

### Recording Performance

- **CPU Usage**: Encoding is CPU-intensive, use faster presets for real-time recording
- **Disk I/O**: High bitrate recordings require fast storage
- **Memory**: FFmpeg buffers video data, monitor RAM usage for long recordings

### Optimization Tips

```csharp
// For real-time recording (low CPU usage)
var options = new VideoRecordingOptions
{
    Preset = "ultrafast",
    Quality = 28, // Lower quality
    FrameRate = 24 // Lower frame rate
};

// For high quality (higher CPU usage)
var options = new VideoRecordingOptions
{
    Preset = "slow",
    Quality = 18, // Higher quality
    FrameRate = 60
};

// For hardware acceleration (NVIDIA)
var options = new VideoRecordingOptions
{
    VideoCodec = "h264_nvenc",
    AdditionalArgs = new[] { "-preset", "p4" }
};
```

## Session Management

### Active Session Monitoring

```csharp
// Get all active sessions
var allSessions = videoService.GetActiveSessions();

// Get only recording sessions
var recordingSessions = videoService.GetActiveRecordingSessions();

// Check specific session status
var isRecording = videoService.IsRecording(sessionId);
var isPlaying = videoService.IsPlaying(sessionId);
```

### Session Cleanup

The service automatically cleans up completed sessions, but you can also manually manage them:

```csharp
// Stop all active recordings
foreach (var sessionId in videoService.GetActiveRecordingSessions())
{
    await videoService.StopRecordingAsync(sessionId);
}

// Stop all playback sessions
foreach (var sessionId in videoService.GetActiveSessions())
{
    if (videoService.IsPlaying(sessionId))
    {
        await videoService.StopPlaybackAsync(sessionId);
    }
}
```

## Error Handling

### Common Exceptions

```csharp
try
{
    var sessionId = await videoService.StartRecordingAsync("output.mp4");
}
catch (FileNotFoundException ex)
{
    // FFmpeg not found in PATH
}
catch (PlatformNotSupportedException ex)
{
    // Platform not supported for this operation
}
catch (InvalidOperationException ex)
{
    // FFmpeg process failed to start
}
catch (UnauthorizedAccessException ex)
{
    // Insufficient permissions (screen recording, file access)
}
```

### Logging

The service provides comprehensive logging:

```csharp
// Enable detailed logging
var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

var logger = loggerFactory.CreateLogger<FFmpegVideoService>();
var videoService = new FFmpegVideoService(logger);
```

## Migration from Recording Plugin

If migrating from the basic recording plugin:

### Old API (Recording Plugin)

```csharp
var recordingService = new FFmpegVideoRecordingService(logger);
var sessionId = await recordingService.StartRecordingAsync("output.mp4", "Title");
await recordingService.StopRecordingAsync(sessionId);
```

### New API (Video Service)

```csharp
var videoService = new FFmpegVideoService(logger);
var options = new VideoRecordingOptions { Title = "Title" };
var sessionId = await videoService.StartRecordingAsync("output.mp4", options);
await videoService.StopRecordingAsync(sessionId);
```

### Backward Compatibility

The plugin includes a `RecordingServiceAdapter` that provides backward compatibility with the old recording service interface, so existing code continues to work.

## Installation

See [INSTALLATION.md](../LablabBean.Plugins.Recording.FFmpeg/INSTALLATION.md) for detailed installation instructions.

## Examples

- **[VideoServiceDemo](../../examples/VideoServiceDemo/)** - Comprehensive demo of all video service features
- **[DualRecordingDemo](../../examples/DualRecordingDemo/)** - Shows integration with terminal recording
- **[VideoRecordingDemo](../../examples/VideoRecordingDemo/)** - Basic recording functionality

## License

Part of the Lablab Bean project. See main project license for details.
