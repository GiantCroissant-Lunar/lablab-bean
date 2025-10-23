# FFmpeg Video Recording Plugin

This plugin provides video recording capabilities for the Lablab Bean project using FFmpeg. It can record the entire screen during gameplay sessions and supports cross-platform recording.

## Features

- **Cross-platform screen recording** using FFmpeg
- **Automatic game session recording** - starts/stops with game events
- **Manual recording control** via service interface
- **Video playback** using FFplay
- **Configurable quality settings** (framerate, codec, etc.)

## Platform Support

- **Windows**: Uses `gdigrab` for desktop capture
- **Linux**: Uses `x11grab` for display capture
- **macOS**: Uses `avfoundation` for screen capture

## Prerequisites

### FFmpeg Installation

The plugin requires FFmpeg to be installed and available in the system PATH.

#### Windows

```bash
# Using Chocolatey
choco install ffmpeg

# Using Scoop
scoop install ffmpeg

# Or download from https://ffmpeg.org/download.html
```

#### Linux (Ubuntu/Debian)

```bash
sudo apt update
sudo apt install ffmpeg
```

#### macOS

```bash
# Using Homebrew
brew install ffmpeg

# Using MacPorts
sudo port install ffmpeg
```

### Verify Installation

```bash
ffmpeg -version
ffplay -version
```

## Usage

### Automatic Recording

The plugin automatically starts video recording when a game session begins and stops when it ends. Videos are saved to `recordings/video/` directory.

### Manual Recording

```csharp
// Get the recording service
var recordingService = serviceRegistry.Get<IService>();

// Start recording
var sessionId = await recordingService.ExecuteAction<string>(
    "StartRecording",
    "path/to/output.mp4",
    "My Recording Title"
);

// Check if recording
var isRecording = recordingService.ExecuteAction<bool>("IsRecording", sessionId);

// Stop recording
recordingService.ExecuteAction("StopRecording", sessionId);

// Play recording
recordingService.ExecuteAction("PlayRecording", "path/to/recording.mp4", 1.5); // 1.5x speed
```

## Configuration

### Video Quality Settings

The plugin uses these default settings:

- **Framerate**: 30 fps
- **Codec**: H.264 (libx264)
- **Preset**: fast
- **CRF**: 23 (good quality/size balance)
- **Pixel Format**: yuv420p (compatible with most players)

### Platform-Specific Settings

#### Windows (gdigrab)

- Captures entire desktop
- Automatic resolution detection

#### Linux (x11grab)

- Default resolution: 1920x1080
- Display: :0.0 (primary display)

#### macOS (avfoundation)

- Screen capture device: 1
- Automatic resolution detection

## Output Formats

- **Default**: MP4 with H.264 encoding
- **Compatibility**: Works with most video players and browsers
- **File naming**: `game_session_YYYYMMDD_HHMMSS.mp4`

## Troubleshooting

### Common Issues

1. **FFmpeg not found**
   - Ensure FFmpeg is installed and in PATH
   - Try running `ffmpeg -version` in terminal

2. **Permission denied (Linux/macOS)**
   - May need to grant screen recording permissions
   - Check system privacy settings

3. **High CPU usage**
   - Consider lowering framerate or changing preset to "ultrafast"
   - Adjust CRF value (higher = lower quality/size)

4. **Large file sizes**
   - Increase CRF value (e.g., 28 for smaller files)
   - Use "slower" preset for better compression

### Performance Tuning

For better performance, you can modify the FFmpeg arguments in `BuildFFmpegArgs()`:

```csharp
// Lower quality, smaller files
args.AddRange(new[] { "-crf", "28" });

// Faster encoding, larger files
args.AddRange(new[] { "-preset", "ultrafast" });

// Lower framerate
args.AddRange(new[] { "-framerate", "15" });
```

## Integration

The plugin integrates with:

- **Game Events**: Automatic recording on game start/end
- **Recording Contracts**: Standard recording service interface
- **Event Bus**: Publishing recording events
- **Service Registry**: Discoverable recording service

## Development

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Adding Custom Formats

To support additional video formats, modify the `BuildFFmpegArgs()` method to accept format parameters or create format-specific methods.

## License

Part of the Lablab Bean project. See main project license for details.
