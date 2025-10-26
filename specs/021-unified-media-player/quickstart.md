# Quick Start Guide: Unified Media Player

**Feature**: 021-unified-media-player
**Date**: 2025-10-26
**Audience**: Developers implementing or using the media player

## Prerequisites

- .NET 8.0 SDK or later
- Terminal emulator with UTF-8 support (for braille fallback)
- Optional: Terminal with SIXEL or Kitty Graphics Protocol support for higher quality

## Installation

### 1. Add NuGet Packages

```bash
# Core media contracts
dotnet add package LablabBean.Contracts.Media

# Media player core service
dotnet add package LablabBean.Plugins.MediaPlayer.Core

# FFmpeg playback engine
dotnet add package LablabBean.Plugins.MediaPlayer.FFmpeg
dotnet add package OpenCvSharp4
dotnet add package OpenCvSharp4.runtime.win  # or .ubuntu, .osx

# Terminal renderers (choose based on target terminals)
dotnet add package LablabBean.Plugins.MediaPlayer.Terminal.Braille  # Universal fallback
dotnet add package LablabBean.Plugins.MediaPlayer.Terminal.Sixel    # Optional
dotnet add package LablabBean.Plugins.MediaPlayer.Terminal.Kitty    # Optional

# ReactiveUI (for ViewModels)
dotnet add package ReactiveUI
dotnet add package ReactiveUI.Fody
dotnet add package System.Reactive

# Terminal.Gui (for console UI)
dotnet add package Terminal.Gui --version 2.x
```

### 2. Configure Services

In your `Program.cs` or service configuration:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Add infrastructure
        services.AddLablabBeanInfrastructure(context.Configuration);
        services.AddLablabBeanReactive();

        // Add plugin system
        services.AddPluginSystem(context.Configuration);

        // Media player plugins will auto-register via plugin loader
        // No manual registration needed - plugins are discovered automatically

        // Add Terminal.Gui service
        services.AddSingleton<ITerminalGuiService, TerminalGuiService>();

        // Add ViewModels
        services.AddTransient<MediaPlayerViewModel>();
    })
    .Build();

await host.RunAsync();
```

### 3. Configure appsettings.json

```json
{
  "MediaPlayer": {
    "DefaultVolume": 0.8,
    "PreferredRenderer": "auto",
    "MaxFrameRate": 30,
    "BufferSize": 60,
    "SupportedFormats": [
      "mp4",
      "mkv",
      "avi",
      "mov",
      "webm",
      "mp3",
      "flac",
      "wav",
      "ogg",
      "aac"
    ]
  },
  "Plugins": {
    "SearchPaths": [
      "./plugins"
    ],
    "AutoLoad": true
  }
}
```

## Basic Usage

### Scenario 1: Simple Video Playback (Programmatic)

```csharp
using LablabBean.Contracts.Media;
using Microsoft.Extensions.DependencyInjection;

// Get service from DI container
var mediaService = serviceProvider.GetRequiredService<IMediaService>();

// Subscribe to state changes
mediaService.PlaybackState.Subscribe(state =>
{
    Console.WriteLine($"Playback state: {state}");
});

// Load and play media
var mediaInfo = await mediaService.LoadAsync("path/to/video.mp4");
Console.WriteLine($"Loaded: {mediaInfo.Duration}");

await mediaService.PlayAsync();

// Wait for user input to pause/stop
Console.ReadKey();
await mediaService.PauseAsync();

Console.ReadKey();
await mediaService.StopAsync();
```

### Scenario 2: Terminal.Gui Media Player View

```csharp
using Terminal.Gui;
using LablabBean.Reactive.ViewModels.Media;

// Create ViewModel
var viewModel = new MediaPlayerViewModel(mediaService);

// Create and show media player view
Application.Init();
var top = Application.Top;

var playerView = new MediaPlayerView(viewModel)
{
    X = 0,
    Y = 0,
    Width = Dim.Fill(),
    Height = Dim.Fill()
};

top.Add(playerView);

// Load media
await viewModel.LoadMediaCommand.Execute("path/to/video.mp4");

Application.Run();
Application.Shutdown();
```

### Scenario 3: CLI Usage

```bash
# Play a video file
lablab-bean media play video.mp4

# Play audio with visualization
lablab-bean media play song.mp3

# List available renderers
lablab-bean media list-renderers

# Test terminal capabilities
lablab-bean media test-capabilities

# Play with specific renderer
lablab-bean media play video.mp4 --renderer braille
```

## Key Concepts

### 1. Renderer Selection

The media player automatically selects the best renderer based on:

1. **Terminal capabilities** (detected at startup)
2. **Media format** (audio vs video)
3. **Renderer priority** (higher = better quality)

**Selection Algorithm**:

```
Available Renderers:
1. Kitty Graphics (Priority 100) - requires Kitty terminal
2. SIXEL (Priority 50) - requires SIXEL support
3. Braille (Priority 10) - works everywhere

If terminal = Kitty:
  → Use Kitty Graphics Renderer (best quality)

If terminal = xterm with SIXEL:
  → Use SIXEL Renderer (medium quality)

If terminal = basic UTF-8:
  → Use Braille Renderer (universal fallback)
```

### 2. Reactive Properties

ViewModels use ReactiveUI for automatic property change notifications:

```csharp
public class MediaPlayerViewModel : ViewModelBase
{
    // Automatic change notifications via [Reactive] attribute
    [Reactive] public PlaybackState State { get; private set; }
    [Reactive] public TimeSpan Position { get; private set; }
    [Reactive] public float Volume { get; set; }

    // Bind to these in UI
    _viewModel.WhenAnyValue(x => x.Position)
        .Subscribe(pos => UpdateUI(pos));
}
```

### 3. Thread Safety

- **Decoding**: Runs on background thread (non-blocking)
- **UI Updates**: Must use `Application.MainLoop.Invoke()`

```csharp
// In renderer implementation
public async Task RenderFrameAsync(MediaFrame frame, CancellationToken ct)
{
    // CPU-intensive work on background thread
    var displayData = ConvertFrame(frame);

    // UI update on main thread
    Application.MainLoop.Invoke(() =>
    {
        DrawToView(displayData);
        _view.SetNeedsDisplay();
    });
}
```

## Terminal Compatibility

### Recommended Terminals

| Terminal | Graphics Support | Quality | Notes |
|----------|-----------------|---------|-------|
| Kitty | Kitty Protocol | ⭐⭐⭐⭐⭐ | Best quality, GPU-accelerated |
| WezTerm | Kitty + SIXEL | ⭐⭐⭐⭐ | Good quality, both protocols |
| iTerm2 (macOS) | SIXEL | ⭐⭐⭐ | Good quality, SIXEL 256 colors |
| xterm (patch 370+) | SIXEL | ⭐⭐⭐ | Good quality if compiled with SIXEL |
| Windows Terminal | Braille | ⭐⭐ | Text-based, decent with braille |
| Any UTF-8 terminal | Braille | ⭐⭐ | Universal fallback |

### Checking Your Terminal

```bash
# Check TERM variable
echo $TERM

# Check for Kitty
if [ "$TERM" = "xterm-kitty" ]; then
  echo "Kitty terminal detected"
fi

# Check for true color
if [ "$COLORTERM" = "truecolor" ]; then
  echo "24-bit color supported"
fi

# Test SIXEL (will show test pattern if supported)
printf '\033Pq"1;1;100;100#0;2;0;0;0#1;2;100;100;0#1~~@@vv@@~~@@~~$#0??}}GG}}??}}??-#1!14@\033\\'
```

## Common Tasks

### Load and Play Media

```csharp
var mediaService = services.GetRequiredService<IMediaService>();

// Load
var info = await mediaService.LoadAsync("video.mp4");
Console.WriteLine($"Duration: {info.Duration}, Format: {info.Format}");

// Play
await mediaService.PlayAsync();

// Seek to 30 seconds
await mediaService.SeekAsync(TimeSpan.FromSeconds(30));

// Pause
await mediaService.PauseAsync();

// Resume
await mediaService.PlayAsync();

// Stop
await mediaService.StopAsync();
```

### Create Playlist

```csharp
var playlist = new Playlist
{
    Name = "My Playlist",
    Items = new List<MediaInfo>
    {
        await mediaService.LoadAsync("video1.mp4"),
        await mediaService.LoadAsync("song1.mp3"),
        await mediaService.LoadAsync("video2.mkv")
    },
    CurrentIndex = 0,
    RepeatMode = RepeatMode.All
};

// Play first item
await mediaService.LoadAsync(playlist.Items[0].Path);
await mediaService.PlayAsync();

// When current item ends, advance to next
playlist.CurrentIndex++;
if (playlist.CurrentIndex < playlist.Items.Count)
{
    await mediaService.LoadAsync(playlist.Items[playlist.CurrentIndex].Path);
    await mediaService.PlayAsync();
}
```

### Subscribe to Playback Events

```csharp
// Position updates (for progress bar)
mediaService.Position
    .Sample(TimeSpan.FromMilliseconds(100))  // Throttle to 10 Hz
    .Subscribe(pos => Console.WriteLine($"Position: {pos:mm\\:ss}"));

// State changes (for UI updates)
mediaService.PlaybackState
    .Subscribe(state => Console.WriteLine($"State: {state}"));

// Volume changes
mediaService.Volume
    .Subscribe(vol => Console.WriteLine($"Volume: {vol:P0}"));
```

### Error Handling

```csharp
try
{
    await mediaService.LoadAsync("invalid-file.mp4");
}
catch (FileNotFoundException)
{
    Console.WriteLine("File not found");
}
catch (NotSupportedException ex)
{
    Console.WriteLine($"Format not supported: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"File corrupted: {ex.Message}");
}
```

## Keyboard Shortcuts (Terminal.Gui Views)

| Key | Action |
|-----|--------|
| Space | Play/Pause toggle |
| Esc | Stop playback |
| ← | Seek backward 5 seconds |
| → | Seek forward 5 seconds |
| ↑ | Volume up 10% |
| ↓ | Volume down 10% |
| N | Next in playlist |
| P | Previous in playlist |
| S | Toggle shuffle |
| R | Cycle repeat mode |

## Troubleshooting

### Video doesn't display

1. **Check terminal compatibility**:

   ```bash
   lablab-bean media test-capabilities
   ```

2. **Verify braille fallback works** (should always work):
   - Ensure terminal encoding is UTF-8
   - Check: `echo $LANG` (should include UTF-8)

3. **Check logs**:
   - Enable debug logging in appsettings.json
   - Look for renderer selection messages

### Audio plays but no video

- Media file may be audio-only
- Check: `mediaInfo.Video` is not null

### Seek is slow or inaccurate

- Normal for video files (seeks to nearest keyframe)
- Use shorter keyframe intervals when encoding videos for better seek accuracy

### High CPU usage

- Lower frame rate in appsettings.json:

  ```json
  "MaxFrameRate": 15  // Reduce from 30
  ```

- Use lower-priority renderer (braille is fastest)

### Memory usage high

- Reduce buffer size:

  ```json
  "BufferSize": 30  // Reduce from 60 frames
  ```

## Next Steps

- Read [data-model.md](./data-model.md) for entity relationships
- Read [research.md](./research.md) for technical implementation details
- See [contracts/](./contracts/) for interface documentation
- Check [spec.md](./spec.md) for functional requirements and user stories

## Support

- GitHub Issues: <https://github.com/your-org/lablab-bean/issues>
- Documentation: <https://docs.your-org.com/lablab-bean/media-player>
- Examples: See `examples/media-player/` in repository

---

**Version**: 1.0.0
**Last Updated**: 2025-10-26
