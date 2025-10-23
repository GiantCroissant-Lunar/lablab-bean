# Video Recording Plugin

A recording plugin that uses the video service for actual recording operations. This plugin provides a clean separation between recording workflow management and video processing, focusing on recording-specific concerns while delegating video operations to the dedicated video service.

## Architecture

### Separation of Concerns

This plugin follows the principle of separation of concerns:

- **Recording Plugin** (`LablabBean.Plugins.Recording.Video`): Manages recording workflows, game event integration, and recording-specific logic
- **Video Service** (`LablabBean.Plugins.Video.FFmpeg`): Handles actual video processing, FFmpeg operations, and low-level video functionality

### Dependencies

The recording plugin depends on the video service:

```
LablabBean.Plugins.Recording.Video
├── Depends on: IVideoRecordingService
├── Depends on: IVideoService
└── Uses: LablabBean.Contracts.Video
```

This creates a clean dependency hierarchy where:

1. Video service provides core video capabilities
2. Recording plugin provides recording-specific workflows
3. Both can be used independently or together

## Features

### 🎮 Game Session Recording

- **Automatic recording** on game start/end events
- **Configurable recording settings** for game sessions
- **Game mode filtering** - exclude specific game modes from recording
- **Session duration limits** and post-game recording

### 📹 Manual Recording

- **On-demand recording** with different quality settings
- **Flexible configuration** separate from game recordings
- **Manual control** over recording parameters

### ⚙️ Configuration System

- **Separate settings** for game vs manual recordings
- **Output directory management** with customizable patterns
- **Auto-recording behavior** configuration
- **Quality presets** for different use cases

### 🔄 Service Integration

- **Uses video service** for actual recording operations
- **Provides recording service interface** for compatibility
- **Action-based interface** for dynamic invocation
- **Event publishing** for recording lifecycle

## Configuration

### Basic Configuration

```json
{
  "RecordingConfiguration": {
    "GameRecording": {
      "VideoCodec": "libx264",
      "Preset": "fast",
      "Quality": 23,
      "FrameRate": 30,
      "Source": "Screen",
      "RecordAudio": false,
      "MaxDurationSeconds": 3600
    },
    "ManualRecording": {
      "VideoCodec": "libx264",
      "Preset": "medium",
      "Quality": 20,
      "FrameRate": 30,
      "Source": "Screen",
      "RecordAudio": true,
      "MaxDurationSeconds": 0
    },
    "Output": {
      "BaseDirectory": "recordings",
      "VideoSubdirectory": "video",
      "GameRecordingPattern": "game_session_{timestamp}.mp4",
      "ManualRecordingPattern": "recording_{timestamp}.mp4",
      "TimestampFormat": "yyyyMMdd_HHmmss"
    },
    "AutoRecording": {
      "EnableAutoRecording": true,
      "MinimumSessionDuration": 10,
      "StopOnGameEnd": true,
      "PostGameRecordingSeconds": 0,
      "ExcludedGameModes": ["menu", "loading"],
      "MaxConcurrentRecordings": 1
    }
  }
}
```

### Configuration Options

#### Game Recording Settings

- **Optimized for performance** during gameplay
- **Lower quality settings** to reduce CPU impact
- **No audio by default** to avoid game audio interference
- **Time limits** to prevent excessive file sizes

#### Manual Recording Settings

- **Higher quality settings** for deliberate recordings
- **Audio enabled by default** for comprehensive capture
- **No time limits** for flexible recording duration
- **Better encoding presets** for quality

#### Output Management

- **Customizable file naming** patterns
- **Directory organization** with date-based subdirectories
- **Timestamp formatting** for consistent naming
- **Separate paths** for different recording types

#### Auto-Recording Behavior

- **Game event integration** with configurable triggers
- **Session filtering** by duration and game mode
- **Concurrent recording limits** to manage resources
- **Post-game recording** for capturing results

## Usage

### Programmatic Usage

```csharp
// Get the recording service
var recordingService = serviceRegistry.Get<IRecordingService>();

// Start manual recording
var sessionId = await recordingService.StartRecordingAsync(
    "my_recording.mp4",
    "Manual Recording");

// Check recording status
var isRecording = recordingService.IsRecording(sessionId);

// Stop recording
await recordingService.StopRecordingAsync(sessionId);

// Play back recording
await recordingService.PlayRecordingAsync("my_recording.mp4", 1.5); // 1.5x speed
```

### Game Integration

```csharp
// Automatic recording happens via game events
// No manual intervention required

// Game starts → Recording begins automatically
// Game ends → Recording stops automatically

// Recordings saved to: recordings/video/game_session_YYYYMMDD_HHMMSS.mp4
```

### Action-Based Interface

```csharp
// Dynamic service usage
var service = serviceRegistry.Get<IService>();

// Start recording
var sessionId = service.ExecuteAction<string>("StartRecording", "output.mp4", "Title");

// Get video information
var videoInfo = service.ExecuteAction<VideoInfo>("GetVideoInfo", "recording.mp4");

// Convert video format
service.ExecuteAction("ConvertVideo", "input.mp4", "output.avi", 28);
```

## Service Registration

The plugin registers multiple service interfaces:

```csharp
// Primary recording service interface
context.Registry.Register<IRecordingService>(recordingService, metadata);

// Action-based interface for compatibility
context.Registry.Register<IService>(recordingService, metadata);
```

### Service Priority

```csharp
Priority: 95  // Higher than video service adapter (85)
```

This ensures the recording plugin takes precedence over direct video service adapters for recording operations.

## Event Integration

### Published Events

```csharp
// Standard recording events
RecordingStartedEvent
RecordingStoppedEvent

// Video service events (via delegation)
VideoRecordingStartedEvent
VideoRecordingStoppedEvent
```

### Subscribed Events

```csharp
// Game lifecycle events
GameStartedEvent → Start recording
GameEndedEvent → Stop recording
```

### Event Flow

```
GameStartedEvent
    ↓
Recording Plugin
    ↓
Video Service (StartRecordingAsync)
    ↓
FFmpeg Process
    ↓
VideoRecordingStartedEvent + RecordingStartedEvent
```

## Comparison with Direct Video Service

### Recording Plugin Approach (This Plugin)

**Advantages:**

- **Recording-focused interface** with recording-specific methods
- **Game event integration** built-in
- **Recording workflow management** (session tracking, auto-recording)
- **Configuration separation** between game and manual recordings
- **Recording-specific events** and lifecycle management

**Use Cases:**

- Game session recording
- Automated recording workflows
- Recording management and organization
- Integration with game events

### Direct Video Service Approach

**Advantages:**

- **Full video capabilities** (recording, playback, conversion, analysis)
- **Lower-level control** over video operations
- **More flexible** for custom video workflows
- **Direct access** to all video service features

**Use Cases:**

- Custom video processing workflows
- Video conversion and analysis
- Advanced video operations
- Non-recording video tasks

## Installation and Setup

### Prerequisites

1. **Video Service Plugin** must be installed and registered first
2. **FFmpeg** must be installed and available in PATH
3. **Platform permissions** for screen recording (macOS/Linux)

### Plugin Loading Order

```csharp
// 1. Load video service plugin first
var videoPlugin = new FFmpegVideoPlugin();
await videoPlugin.InitializeAsync(context);

// 2. Load recording plugin second
var recordingPlugin = new VideoRecordingPlugin();
await recordingPlugin.InitializeAsync(context);
```

### Dependency Injection

```csharp
// Video services are automatically resolved from registry
public VideoRecordingPlugin()
{
    // Dependencies injected during InitializeAsync:
    // - IVideoRecordingService (from video plugin)
    // - IVideoService (from video plugin)
}
```

## Error Handling

### Common Scenarios

```csharp
try
{
    var sessionId = await recordingService.StartRecordingAsync("output.mp4");
}
catch (InvalidOperationException ex) when (ex.Message.Contains("IVideoRecordingService not found"))
{
    // Video service plugin not loaded
    logger.LogError("Video service plugin must be loaded before recording plugin");
}
catch (FileNotFoundException ex)
{
    // FFmpeg not found
    logger.LogError("FFmpeg not installed or not in PATH");
}
catch (PlatformNotSupportedException ex)
{
    // Platform not supported
    logger.LogError("Video recording not supported on this platform");
}
```

### Graceful Degradation

```csharp
// Plugin initialization checks for video service availability
if (_videoRecordingService == null)
{
    throw new InvalidOperationException(
        "IVideoRecordingService not found. Ensure video plugin is loaded first.");
}
```

## Performance Considerations

### Resource Management

- **Delegates video processing** to video service (no duplicate FFmpeg processes)
- **Lightweight session tracking** with minimal overhead
- **Efficient event handling** for game integration
- **Configurable limits** to prevent resource exhaustion

### Memory Usage

- **Minimal memory footprint** (only tracks session metadata)
- **No video data buffering** (handled by video service)
- **Automatic cleanup** of completed sessions
- **Shared process management** via video service

## Migration from Direct FFmpeg Plugin

### From `LablabBean.Plugins.Recording.FFmpeg`

**Old Approach:**

```csharp
var recordingService = new FFmpegVideoRecordingService(logger, options);
```

**New Approach:**

```csharp
// Video service provides FFmpeg functionality
var videoService = new FFmpegVideoService(logger);

// Recording service uses video service
var recordingService = new VideoRecordingService(logger, videoService, videoService, config);
```

### Benefits of Migration

1. **Separation of concerns** - recording logic separate from video processing
2. **Shared video service** - multiple plugins can use same video capabilities
3. **Enhanced configuration** - separate settings for different recording types
4. **Better extensibility** - easier to add new recording features
5. **Consistent architecture** - follows plugin dependency patterns

## Future Enhancements

### Planned Features

- **Recording profiles** for different scenarios (streaming, archival, etc.)
- **Batch recording management** for multiple simultaneous recordings
- **Recording scheduling** with time-based triggers
- **Cloud upload integration** for automatic backup
- **Recording analytics** and usage statistics

### Extension Points

- **Custom recording triggers** via event system
- **Recording post-processing** pipelines
- **External storage providers** for recordings
- **Recording metadata** enhancement and tagging

## License

Part of the Lablab Bean project. See main project license for details.
