# FFmpeg Video Service Architecture Summary

## Overview

Successfully refactored the FFmpeg integration from a basic recording-only plugin into a comprehensive video service that provides recording, playback, conversion, and analysis capabilities. This new architecture separates concerns properly and creates a reusable video service that can be shared across different use cases.

## Architecture Evolution

### Before: Recording-Only Plugin

```
LablabBean.Plugins.Recording.FFmpeg
├── FFmpegVideoRecordingService (IRecordingService)
├── VideoRecordingPlugin
└── Configuration/FFmpegRecordingOptions
```

### After: Comprehensive Video Service

```
LablabBean.Contracts.Video (New Framework)
├── Services/
│   ├── IVideoService
│   └── IVideoRecordingService
├── Models/VideoModels
└── Events/VideoEvents

LablabBean.Plugins.Video.FFmpeg (New Plugin)
├── Services/FFmpegVideoService (IVideoService + IVideoRecordingService)
├── FFmpegVideoPlugin
└── RecordingServiceAdapter (Backward Compatibility)
```

## Key Architectural Improvements

### 1. Unified Service Design

**Single Service, Multiple Interfaces:**

```csharp
public class FFmpegVideoService : IVideoService, IVideoRecordingService
{
    // Handles all video operations in one place
    // - Recording: StartRecordingAsync(), StopRecordingAsync()
    // - Playback: PlayVideoAsync(), StopPlaybackAsync()
    // - Conversion: ConvertVideoAsync()
    // - Analysis: GetVideoInfoAsync()
    // - Session Management: GetActiveSessions()
}
```

**Benefits:**

- Eliminates code duplication between video operations
- Shared FFmpeg process management
- Unified session tracking across all video operations
- Consistent error handling and logging

### 2. Separation of Concerns

**Framework Layer (Contracts):**

- `LablabBean.Contracts.Video` - Pure interfaces and models
- No FFmpeg dependencies
- Reusable across different video implementations

**Implementation Layer (Plugin):**

- `LablabBean.Plugins.Video.FFmpeg` - FFmpeg-specific implementation
- Platform-specific video capture logic
- FFmpeg process management and argument building

### 3. Enhanced Service Interfaces

**IVideoService (General Video Operations):**

```csharp
Task<string> PlayVideoAsync(string videoPath, VideoPlaybackOptions? options = null);
Task<VideoInfo> GetVideoInfoAsync(string videoPath);
Task<string> ConvertVideoAsync(string inputPath, string outputPath, VideoConversionOptions? options = null);
IEnumerable<string> GetActiveSessions();
```

**IVideoRecordingService (Recording-Specific):**

```csharp
Task<string> StartRecordingAsync(string outputPath, VideoRecordingOptions? options = null);
Task StopRecordingAsync(string sessionId);
bool IsRecording(string sessionId);
IEnumerable<string> GetActiveRecordingSessions();
```

### 4. Comprehensive Configuration System

**Structured Options:**

```csharp
VideoRecordingOptions
├── Basic: Title, VideoCodec, Preset, Quality, FrameRate
├── Source: Screen, Window, Region, Camera
├── Audio: RecordAudio, AudioDevice
├── Limits: MaxDurationSeconds, Resolution
├── Platform: PlatformSettings Dictionary
└── Advanced: AdditionalArgs

VideoPlaybackOptions
├── Playback: Speed, StartPosition, EndPosition, Loop
├── Audio: Volume, Mute
├── Display: Fullscreen, WindowTitle
└── Advanced: AdditionalArgs

VideoConversionOptions
├── Codecs: VideoCodec, AudioCodec
├── Quality: Quality, Preset, Resolution, FrameRate
├── Trimming: StartTime, Duration
└── Advanced: AdditionalArgs
```

### 5. Event-Driven Architecture

**Video Events:**

```csharp
// Recording Events
VideoRecordingStartedEvent
VideoRecordingStoppedEvent

// Playback Events
VideoPlaybackStartedEvent
VideoPlaybackStoppedEvent

// Conversion Events
VideoConversionStartedEvent
VideoConversionCompletedEvent
VideoConversionProgressEvent
```

**Backward Compatibility Events:**

```csharp
// Still publishes legacy recording events
RecordingStartedEvent
RecordingStoppedEvent
```

### 6. Session Management

**Unified Session Tracking:**

```csharp
VideoSession
├── Id: Unique session identifier
├── Type: Playback, Recording, Conversion, Analysis
├── FilePath: File being processed
├── ProcessId: FFmpeg process ID
├── Status: Active, Paused, Completed, Failed, Cancelled
├── Progress: Percentage completion (0-100)
└── Data: Additional session-specific data
```

**Session Operations:**

- Automatic cleanup of completed sessions
- Real-time status monitoring
- Progress tracking for long-running operations
- Graceful process termination

## Service Registration Strategy

### Multiple Interface Registration

The plugin registers the same service instance under multiple interfaces:

```csharp
// Primary video service
context.Registry.Register<IVideoService>(videoService, metadata);

// Recording-specific interface
context.Registry.Register<IVideoRecordingService>(videoService, metadata);

// Backward compatibility adapter
context.Registry.Register<IService>(adapter, metadata);
```

### Priority-Based Service Discovery

```csharp
// Video services (highest priority)
IVideoService: Priority 100
IVideoRecordingService: Priority 100

// Backward compatibility (lower priority)
IService (Recording): Priority 85
```

This ensures new code uses the video interfaces while legacy code continues working.

## Platform Abstraction

### Cross-Platform Video Capture

**Windows (gdigrab):**

```csharp
BuildWindowsArgs()
├── Desktop capture: "-f gdigrab -i desktop"
├── Window capture: "-f gdigrab -i title=WindowName"
├── Cursor control: "-draw_mouse 0/1"
└── Offset positioning: "-offset_x X -offset_y Y"
```

**Linux (x11grab):**

```csharp
BuildLinuxArgs()
├── Display capture: "-f x11grab -i :0.0"
├── Resolution: "-s 1920x1080"
├── Mouse following: "-follow_mouse centered"
└── Cursor control: "-draw_mouse 0/1"
```

**macOS (avfoundation):**

```csharp
BuildMacOSArgs()
├── Screen capture: "-f avfoundation -i 1"
├── Cursor capture: "-capture_cursor 1"
├── Click capture: "-capture_mouse_clicks 1"
└── Audio input: "-i audio_device:screen_device"
```

### Audio Integration

**Platform-Specific Audio Formats:**

- Windows: DirectShow (`dshow`)
- Linux: ALSA (`alsa`)
- macOS: AVFoundation (`avfoundation`)

## Backward Compatibility

### RecordingServiceAdapter

Provides seamless migration from the old recording interface:

```csharp
// Old API still works
var sessionId = service.ExecuteAction<string>("StartRecording", "output.mp4", "Title");
service.ExecuteAction("StopRecording", sessionId);

// New capabilities through old interface
service.ExecuteAction("ConvertVideo", "input.mp4", "output.avi", 28);
var info = service.ExecuteAction<VideoInfo>("GetVideoInfo", "video.mp4");
```

### Migration Path

**Phase 1: Coexistence**

- Both old and new plugins can run simultaneously
- Legacy code continues using old interfaces
- New code adopts video service interfaces

**Phase 2: Migration**

- Update existing code to use new interfaces
- Leverage enhanced capabilities (conversion, analysis)
- Maintain backward compatibility for external integrations

**Phase 3: Deprecation**

- Mark old recording-only plugin as deprecated
- Provide migration guides and tooling
- Eventually remove old plugin

## Performance Optimizations

### Process Management

**Efficient Process Handling:**

- Shared process monitoring across all session types
- Graceful shutdown with fallback to force termination
- Automatic cleanup of dead processes
- Memory-efficient session tracking

**Progress Monitoring:**

- Real-time FFmpeg output parsing
- Conversion progress calculation
- Event-driven progress updates
- Non-blocking progress reporting

### Resource Management

**Memory Optimization:**

- Streaming FFmpeg output processing
- Lazy session cleanup
- Efficient string building for arguments
- Minimal object allocation in hot paths

**CPU Optimization:**

- Configurable encoding presets
- Hardware acceleration support
- Platform-optimized capture methods
- Async/await throughout for non-blocking operations

## Testing Strategy

### Demo Applications

**VideoServiceDemo:**

- Comprehensive demonstration of all capabilities
- Recording, playback, conversion, and analysis
- Session management and monitoring
- Error handling and edge cases

**DualRecordingDemo:**

- Integration with existing terminal recording
- Simultaneous video and terminal capture
- Unified recording management

### Unit Testing Approach

**Service Layer Testing:**

```csharp
FFmpegVideoServiceTests
├── Recording: Start/stop, session management, options
├── Playback: Play/stop, speed control, seeking
├── Conversion: Format conversion, quality settings, progress
├── Analysis: Video info extraction, metadata parsing
└── Sessions: Lifecycle, cleanup, monitoring
```

**Integration Testing:**

```csharp
VideoPluginIntegrationTests
├── Service registration and discovery
├── Event publishing and subscription
├── Game event integration
├── Backward compatibility
└── Cross-platform behavior
```

## Future Enhancements

### Planned Features

**Advanced Recording:**

- Region-based screen capture
- Multi-monitor support
- Camera input integration
- Audio mixing and enhancement

**Enhanced Playback:**

- Subtitle support
- Multiple audio track selection
- Frame-by-frame navigation
- Playlist management

**Conversion Pipeline:**

- Batch conversion processing
- Custom filter chains
- Quality optimization algorithms
- Cloud processing integration

**Analysis Capabilities:**

- Video quality assessment
- Motion detection
- Scene change detection
- Automated highlight extraction

### Extensibility Points

**Plugin Architecture:**

- Custom video sources (plugins)
- Custom encoders and decoders
- Post-processing filters
- Cloud storage integrations

**Configuration Extensions:**

- Profile-based settings
- User preference management
- Dynamic quality adjustment
- Performance monitoring

## Conclusion

The refactored FFmpeg video service provides a solid foundation for all video-related operations in Lablab Bean. The architecture properly separates concerns, provides comprehensive functionality, maintains backward compatibility, and offers clear extension points for future enhancements.

**Key Benefits:**

- **Unified**: Single service handles all video operations
- **Flexible**: Multiple interfaces for different use cases
- **Extensible**: Clear architecture for adding new capabilities
- **Compatible**: Seamless migration from existing recording plugin
- **Performant**: Optimized for real-time video processing
- **Cross-Platform**: Consistent behavior across Windows, Linux, and macOS

This architecture positions Lablab Bean to handle current video needs while providing a foundation for future video-related features and integrations.
