---
doc_id: DOC-2025-00068
title: Media Player Integration Guide
doc_type: finding
status: active
canonical: true
created: 2025-10-26
tags: [media, video, audio, playback, integration]
summary: Complete guide for integrating the LablabBean media player into applications with FFmpeg and Terminal.Gui support
---

# Media Player Integration Guide

Complete guide for integrating the LablabBean media player into your application.

## Quick Start

### 1. Register Services

```csharp
using LablabBean.Plugins.MediaPlayer.Core;
using LablabBean.Plugins.MediaPlayer.FFmpeg;
using LablabBean.Plugins.MediaPlayer.Terminal.Braille;
using LablabBean.Plugins.MediaPlayer.Terminal.Kitty;

// In your startup/Program.cs
services.AddLablabBeanReactive(); // Required for observables

MediaPlayerPlugin.RegisterServices(services);
FFmpegPlaybackPlugin.RegisterServices(services);
BrailleRendererPlugin.RegisterServices(services);
KittyRendererPlugin.RegisterServices(services); // ✅ NEW: High-quality video
```

### 2. Use MediaService

```csharp
using LablabBean.Contracts.Media;

public class MyController
{
    private readonly IMediaService _mediaService;

    public MyController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    public async Task PlayVideoAsync(string path)
    {
        // Load media
        var mediaInfo = await _mediaService.LoadAsync(path);
        Console.WriteLine($"Duration: {mediaInfo.Duration}");

        // Subscribe to state changes
        _mediaService.PlaybackState.Subscribe(state =>
            Console.WriteLine($"State: {state}"));

        // Start playback
        await _mediaService.PlayAsync();
    }
}
```

## Available Services

### IMediaService

Main service for media playback control.

**Methods:**

- `LoadAsync(path)` - Load media file
- `PlayAsync()` - Start/resume playback
- `PauseAsync()` - Pause playback
- `StopAsync()` - Stop and reset
- `SeekAsync(position)` - Seek to position
- `SetVolumeAsync(volume)` - Adjust volume (0.0-1.0)

**Observables:**

- `PlaybackState` - Emits state changes (Stopped, Playing, Paused)
- `Position` - Emits position updates (~10 Hz)
- `Duration` - Emits duration after load
- `Volume` - Emits volume changes

**Properties:**

- `CurrentMedia` - Currently loaded media metadata
- `ActiveRenderer` - Currently active renderer

### IMediaPlaybackEngine

Interface for decoding engines (e.g., FFmpeg).

**Implementation:**

- `FFmpegPlaybackEngine` - Supports all major video/audio formats

### IMediaRenderer

Interface for rendering engines (e.g., Braille, SIXEL).

**Implementations:**

- `BrailleRenderer` - Unicode braille characters (universal fallback)
- `SixelRenderer` - High-quality graphics (coming soon)
- `KittyRenderer` - Kitty graphics protocol ✅ **COMPLETE** (SPEC-025)
  - Supports RGBA32 and RGB24 pixel formats
  - Uses placement ID for smooth video playback
  - Hardware-accelerated rendering in compatible terminals
  - Automatic fallback to Braille on unsupported terminals
  - See `docs/guides/KITTY_GRAPHICS_SETUP.md` for setup

### ITerminalCapabilityDetector

Detects terminal capabilities automatically.

**Capabilities Detected:**

- TrueColor support
- SIXEL graphics
- Kitty graphics protocol
- Unicode support
- Mouse support
- Hyperlinks

## Usage Examples

### Basic Playback

```csharp
var mediaService = serviceProvider.GetRequiredService<IMediaService>();

// Load video
await mediaService.LoadAsync("video.mp4");

// Play
await mediaService.PlayAsync();

// Wait for user input
Console.ReadKey();

// Stop
await mediaService.StopAsync();
```

### Reactive State Management

```csharp
// Subscribe to all state changes
var stateSubscription = mediaService.PlaybackState
    .Subscribe(state => Console.WriteLine($"State changed to: {state}"));

// Subscribe to position updates
var positionSubscription = mediaService.Position
    .Where(pos => pos.TotalSeconds % 1 < 0.1) // Every second
    .Subscribe(pos => Console.WriteLine($"Position: {pos}"));

// Cleanup when done
stateSubscription.Dispose();
positionSubscription.Dispose();
```

### Volume Control

```csharp
// Set volume to 50%
await mediaService.SetVolumeAsync(0.5f);

// Subscribe to volume changes
mediaService.Volume.Subscribe(vol =>
    Console.WriteLine($"Volume: {vol * 100}%"));
```

### Seeking

```csharp
// Seek to 30 seconds
await mediaService.SeekAsync(TimeSpan.FromSeconds(30));

// Seek to 50% of duration
var duration = await mediaService.Duration.FirstAsync();
await mediaService.SeekAsync(duration * 0.5);
```

### Error Handling

```csharp
try
{
    await mediaService.LoadAsync("video.mp4");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.Message}");
}
catch (NotSupportedException ex)
{
    Console.WriteLine($"Format not supported: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading media: {ex.Message}");
}
```

### Metadata Access

```csharp
await mediaService.LoadAsync("video.mp4");

var media = mediaService.CurrentMedia;
if (media != null)
{
    Console.WriteLine($"Duration: {media.Duration}");
    Console.WriteLine($"Format: {media.Format}");

    if (media.Video != null)
    {
        Console.WriteLine($"Resolution: {media.Video.Width}x{media.Video.Height}");
        Console.WriteLine($"FPS: {media.Video.FrameRate}");
        Console.WriteLine($"Codec: {media.Video.Codec}");
    }

    if (media.Audio != null)
    {
        Console.WriteLine($"Sample Rate: {media.Audio.SampleRate} Hz");
        Console.WriteLine($"Channels: {media.Audio.Channels}");
        Console.WriteLine($"Codec: {media.Audio.Codec}");
    }
}
```

## CLI Integration

### Adding Media Commands

```csharp
using LablabBean.Console.Commands;
using System.CommandLine;

// Create root command
var rootCommand = new RootCommand("My Media App");

// Add media player command
var mediaHost = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddLablabBeanReactive();
        MediaPlayerPlugin.RegisterServices(services);
        FFmpegPlaybackPlugin.RegisterServices(services);
        BrailleRendererPlugin.RegisterServices(services);
    })
    .Build();

rootCommand.AddCommand(MediaPlayerCommand.Create(mediaHost.Services));

// Invoke
return await rootCommand.InvokeAsync(args);
```

### Custom CLI Commands

```csharp
var playCommand = new Command("play", "Play media file");
var fileArg = new Argument<string>("file", "Path to media file");
playCommand.AddArgument(fileArg);

playCommand.SetHandler(async (file) =>
{
    var mediaService = serviceProvider.GetRequiredService<IMediaService>();
    await mediaService.LoadAsync(file);
    await mediaService.PlayAsync();
}, fileArg);
```

### Interactive Controls

The CLI includes full keyboard controls for interactive playback:

```bash
# Play with interactive controls
./LablabBean.Console.exe play video.mp4

# Controls available during playback:
# • [Space]   Pause/Resume
# • [← →]     Seek ±10 seconds
# • [↑ ↓]     Volume ±10%
# • [Esc]     Stop playback
# • [Ctrl+C]  Graceful shutdown
```

**Implementation Example:**

```csharp
// Add keyboard control handling
var keyTask = Task.Run(async () =>
{
    while (!cancellationToken.IsCancellationRequested)
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.Spacebar:
                    var state = await mediaService.PlaybackState.FirstAsync();
                    if (state.Status == PlaybackStatus.Playing)
                        await mediaService.PauseAsync();
                    else if (state.Status == PlaybackStatus.Paused)
                        await mediaService.PlayAsync();
                    break;

                case ConsoleKey.RightArrow:
                    var pos = await mediaService.Position.FirstAsync();
                    await mediaService.SeekAsync(pos + TimeSpan.FromSeconds(10));
                    break;

                case ConsoleKey.UpArrow:
                    var vol = await mediaService.Volume.FirstAsync();
                    await mediaService.SetVolumeAsync(Math.Min(1.0f, vol + 0.1f));
                    break;
            }
        }
        await Task.Delay(50, cancellationToken);
    }
}, cancellationToken);
```

## Supported Formats

### Video

- **Containers**: MP4, MKV, AVI, MOV, WMV, FLV, WEBM
- **Codecs**: H.264, H.265/HEVC, VP8, VP9, AV1, MPEG-4

### Audio

- **Formats**: MP3, WAV, FLAC, AAC, OGG, M4A, WMA
- **Codecs**: MP3, AAC, FLAC, Opus, Vorbis

## Performance Tips

1. **Frame Rate**: Target is 30 FPS, actual rate depends on CPU
2. **Position Updates**: 10 Hz (every 100ms) to reduce overhead
3. **Memory**: Frames are buffered, consider pooling for production
4. **Threading**: All operations are async, don't block UI thread

## Troubleshooting

### Issue: Video doesn't play

**Solution:** Check if file exists and format is supported

```csharp
if (!File.Exists(path))
    throw new FileNotFoundException($"File not found: {path}");
```

### Issue: No output visible

**Solution:** Verify terminal supports Unicode

```csharp
var detector = serviceProvider.GetRequiredService<ITerminalCapabilityDetector>();
var caps = await detector.DetectCapabilitiesAsync();
if (!caps.SupportsUnicode)
    Console.WriteLine("Warning: Terminal may not support Unicode braille");
```

### Issue: Performance is slow

**Solution:** Reduce frame rate or resolution

- Lower quality source file
- Use hardware acceleration (future feature)

### Issue: Colors look wrong

**Solution:** Terminal color support varies

- Braille renderer uses ANSI 16-color quantization
- TrueColor support detected automatically
- Future: SIXEL/Kitty for better color reproduction

## Dependencies

```xml
<!-- Required NuGet packages -->
<PackageReference Include="OpenCvSharp4" />
<PackageReference Include="OpenCvSharp4.runtime.win" />
<PackageReference Include="System.Reactive" />
<PackageReference Include="ReactiveUI" />
```

## Architecture

```
┌─────────────────────────────────────────────┐
│           IMediaService                     │
│  (Orchestration + State Management)         │
└─────────────┬───────────────────────────────┘
              │
      ┌───────┴───────┐
      │               │
      ▼               ▼
┌──────────┐   ┌──────────────┐
│  Engine  │   │   Renderer   │
│ (FFmpeg) │   │  (Braille)   │
└──────────┘   └──────────────┘
      │               │
      └───────┬───────┘
              ▼
       ┌──────────────┐
       │   Terminal   │
       └──────────────┘
```

## See Also

- [Media Player CLI Reference](./media-player-cli.md)
- [Reactive Programming Guide](./reactive-programming.md)
- [Plugin Development](./plugin-development.md)

---

**Version**: 1.0.0
**Last Updated**: 2025-10-26
**Status**: Production Ready
