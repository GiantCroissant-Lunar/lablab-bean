# FFmpeg Video Recording Plugin - Installation Guide

This guide will help you install and configure the FFmpeg video recording plugin for Lablab Bean.

## Prerequisites

### 1. Install FFmpeg

The plugin requires FFmpeg to be installed and available in your system PATH.

#### Windows

**Option A: Using Package Managers (Recommended)**

```powershell
# Using Chocolatey
choco install ffmpeg

# Using Scoop
scoop install ffmpeg

# Using winget
winget install Gyan.FFmpeg
```

**Option B: Manual Installation**

1. Download FFmpeg from [https://ffmpeg.org/download.html#build-windows](https://ffmpeg.org/download.html#build-windows)
2. Extract to a folder (e.g., `C:\ffmpeg`)
3. Add `C:\ffmpeg\bin` to your system PATH
4. Restart your terminal/IDE

#### Linux (Ubuntu/Debian)

```bash
sudo apt update
sudo apt install ffmpeg
```

#### Linux (CentOS/RHEL/Fedora)

```bash
# CentOS/RHEL
sudo yum install epel-release
sudo yum install ffmpeg

# Fedora
sudo dnf install ffmpeg
```

#### macOS

```bash
# Using Homebrew (recommended)
brew install ffmpeg

# Using MacPorts
sudo port install ffmpeg
```

### 2. Verify Installation

After installation, verify FFmpeg is working:

```bash
ffmpeg -version
ffplay -version
```

You should see version information for both commands.

## Plugin Installation

### 1. Add Plugin Reference

Add the plugin to your project by referencing it in your `.csproj` file:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/LablabBean.Plugins.Recording.FFmpeg/LablabBean.Plugins.Recording.FFmpeg.csproj" />
</ItemGroup>
```

### 2. Register the Plugin

In your application startup code, register the video recording plugin:

```csharp
using LablabBean.Plugins.Recording.FFmpeg;
using LablabBean.Plugins.Recording.FFmpeg.Configuration;

// Create plugin with default configuration
var videoPlugin = new VideoRecordingPlugin();

// Or with custom configuration
var options = new FFmpegRecordingOptions
{
    FrameRate = 60,
    CRF = 18, // Higher quality
    Preset = "medium"
};
var videoService = new FFmpegVideoRecordingService(logger, options);
```

### 3. Configuration (Optional)

Create a configuration file `appsettings.json` to customize recording settings:

```json
{
  "FFmpegRecording": {
    "VideoCodec": "libx264",
    "Preset": "fast",
    "CRF": 23,
    "FrameRate": 30,
    "PixelFormat": "yuv420p",
    "RecordAudio": false,
    "MaxDurationSeconds": 3600,
    "ShowFFmpegOutput": false,
    "PlatformSettings": {
      "Windows": {
        "ShowCursor": true
      },
      "Linux": {
        "Display": ":0.0",
        "ShowCursor": true
      },
      "MacOS": {
        "ScreenDevice": 1,
        "ShowCursor": true
      }
    }
  }
}
```

## Platform-Specific Setup

### Windows

No additional setup required. The plugin uses `gdigrab` which is built into FFmpeg.

**Permissions**: No special permissions needed for desktop recording.

### Linux

**X11 Display Server** (most common):

- Plugin uses `x11grab` input format
- Works with most Linux distributions using X11

**Wayland Display Server**:

- Limited support - may require additional setup
- Consider using OBS Studio integration instead

**Permissions**:

- Usually no special permissions needed
- Some distributions may require adding user to `video` group:

  ```bash
  sudo usermod -a -G video $USER
  ```

### macOS

**Screen Recording Permission**:

1. Go to System Preferences → Security & Privacy → Privacy
2. Select "Screen Recording" from the left sidebar
3. Add your application to the allowed list
4. Restart your application

**Audio Recording Permission** (if enabled):

1. Go to System Preferences → Security & Privacy → Privacy
2. Select "Microphone" from the left sidebar
3. Add your application to the allowed list

## Testing the Installation

### 1. Run the Demo

Build and run the included demo to test the installation:

```bash
cd dotnet/examples/VideoRecordingDemo
dotnet run
```

### 2. Manual Test

Create a simple test program:

```csharp
using LablabBean.Plugins.Recording.FFmpeg.Services;
using Microsoft.Extensions.Logging;

var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<Program>();
var service = new FFmpegVideoRecordingService(logger);

// Start 5-second recording
var sessionId = await service.StartRecordingAsync("test-recording.mp4", "Test Recording");
Console.WriteLine($"Recording started: {sessionId}");

await Task.Delay(5000);

await service.StopRecordingAsync(sessionId);
Console.WriteLine("Recording stopped. Check test-recording.mp4");
```

## Troubleshooting

### Common Issues

#### 1. "ffmpeg: command not found"

- **Solution**: FFmpeg is not installed or not in PATH
- **Fix**: Install FFmpeg and ensure it's in your system PATH

#### 2. "Permission denied" (Linux/macOS)

- **Solution**: Missing screen recording permissions
- **Fix**: Grant screen recording permissions in system settings

#### 3. "No suitable output format found"

- **Solution**: Invalid output file extension or codec
- **Fix**: Use `.mp4`, `.avi`, or `.mkv` file extensions

#### 4. High CPU usage during recording

- **Solution**: FFmpeg encoding is CPU-intensive
- **Fix**:
  - Use faster preset: `"preset": "ultrafast"`
  - Lower frame rate: `"framerate": 15`
  - Lower quality: `"crf": 28`

#### 5. Large file sizes

- **Solution**: Default settings prioritize quality
- **Fix**:
  - Increase CRF value: `"crf": 28`
  - Use slower preset: `"preset": "slow"`

#### 6. Audio not recording (macOS)

- **Solution**: Missing microphone permissions
- **Fix**: Grant microphone access in System Preferences

### Performance Optimization

#### For Real-time Recording

```json
{
  "VideoCodec": "libx264",
  "Preset": "ultrafast",
  "CRF": 23,
  "FrameRate": 30
}
```

#### For High Quality

```json
{
  "VideoCodec": "libx264",
  "Preset": "slow",
  "CRF": 18,
  "FrameRate": 60
}
```

#### For Small File Size

```json
{
  "VideoCodec": "libx264",
  "Preset": "medium",
  "CRF": 28,
  "FrameRate": 24
}
```

## Advanced Configuration

### Custom FFmpeg Arguments

Add custom FFmpeg arguments for advanced use cases:

```json
{
  "AdditionalArgs": [
    "-tune", "zerolatency",
    "-profile:v", "baseline"
  ]
}
```

### Hardware Acceleration

For systems with hardware encoding support:

```json
{
  "VideoCodec": "h264_nvenc",
  "AdditionalArgs": ["-preset", "p4"]
}
```

Available hardware encoders:

- **NVIDIA**: `h264_nvenc`, `hevc_nvenc`
- **Intel**: `h264_qsv`, `hevc_qsv`
- **AMD**: `h264_amf`, `hevc_amf`

## Support

For issues and questions:

1. Check the [README.md](README.md) for usage examples
2. Review FFmpeg documentation: [https://ffmpeg.org/documentation.html](https://ffmpeg.org/documentation.html)
3. Open an issue in the Lablab Bean repository

## Next Steps

After successful installation:

1. Integrate with your game events for automatic recording
2. Customize recording settings for your use case
3. Set up post-processing workflows if needed
4. Consider implementing recording management UI
