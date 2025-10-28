---
doc_id: DOC-2025-00083
title: Unified Media Player - Architecture and Implementation Plan
doc_type: plan
status: draft
canonical: true
created: 2025-10-26
tags: [media-player, video, audio, terminal-gui, plugins, mvvm, reactiveui]
summary: Technical design and implementation plan for unified media player with Terminal.Gui and plugin architecture
---

## Unified Media Player - Architecture and Implementation Plan

## Executive Summary

This document outlines the design and implementation plan for a unified audio/video media player system for the Lablab-Bean project. The player will support multiple rendering backends that adapt to different terminal capabilities, using a plugin-based architecture with ReactiveUI MVVM patterns.

## Goals

### Primary Objectives

1. **Unified Playback**: Single player service that handles both audio and video content
2. **Adaptive Rendering**: Automatically select optimal renderer based on terminal capabilities
3. **Plugin Architecture**: Hot-pluggable renderer implementations with priority-based selection
4. **MVVM Separation**: ReactiveUI-based ViewModels for UI-agnostic business logic
5. **Terminal.Gui Integration**: Native TUI views with rich controls and keyboard shortcuts
6. **Cross-Platform Support**: Works on Windows Terminal, Linux terminals, macOS, WezTerm, Kitty

### Secondary Objectives

1. Audio visualization (spectrum analyzer, waveform display)
2. Playlist management with shuffle/repeat modes
3. CLI commands for media playback
4. Optional Windows UI integration via SadConsole

## Architecture Overview

### High-Level Component Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                          UI Layer (Terminal.Gui)                    │
├─────────────────────────────────────────────────────────────────────┤
│  MediaPlayerView  │  MediaControlsView  │  PlaylistView            │
└──────────┬──────────────────────┬────────────────────────────────────┘
           │ Binds to             │
           ▼                      ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     ViewModels (ReactiveUI)                         │
├─────────────────────────────────────────────────────────────────────┤
│  MediaPlayerViewModel  │  PlaylistViewModel  │  VisualizerViewModel │
│  - [Reactive] State    │  - ObservableItems  │  - SpectrumData      │
│  - PlayCommand         │  - NextCommand      │  - WaveformData      │
│  - PauseCommand        │  - ShuffleCommand   │                      │
└──────────┬──────────────────────────────────────────────────────────┘
           │ Calls
           ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      IMediaService (Core)                           │
├─────────────────────────────────────────────────────────────────────┤
│  MediaService                                                       │
│  - LoadAsync()  ──► Terminal Capability Detection                   │
│  - PlayAsync()  ──► Renderer Selection (Priority-based)             │
│  - PauseAsync() ──► Playback Engine Coordination                    │
└──────────┬────────────────────┬─────────────────────────────────────┘
           │                    │
           ▼                    ▼
┌──────────────────────┐  ┌─────────────────────────────────────────┐
│ IMediaPlaybackEngine │  │       IMediaRenderer (Plugins)          │
├──────────────────────┤  ├─────────────────────────────────────────┤
│ FFmpegPlaybackEngine │  │ Priority 100: KittyGraphicsRenderer     │
│ - DecodeNextFrame()  │  │ Priority 50:  SixelRenderer             │
│ - SeekAsync()        │  │ Priority 30:  CacaRenderer              │
│ - FrameStream (Rx)   │  │ Priority 10:  BrailleRenderer (fallback)│
└──────────────────────┘  └─────────────────────────────────────────┘
```

### Layer Breakdown

#### Layer 1: Media Contracts (`LablabBean.Contracts.Media`)

**Purpose**: Define interfaces and data contracts for media services

**Key Interfaces**:

- `IMediaService` - Main service for playback control
- `IMediaRenderer` - Renderer plugin interface for different terminal types
- `IMediaPlaybackEngine` - Decoder interface (FFmpeg, NAudio, etc.)
- `ITerminalCapabilityDetector` - Terminal capability detection

**Data Transfer Objects**:

- `MediaInfo` - Media file metadata (duration, format, codecs)
- `MediaFrame` - Single frame of video/audio data
- `PlaybackState` - Current playback state (playing, paused, stopped, etc.)
- `RenderContext` - Rendering context (target view, viewport size)
- `TerminalInfo` - Detected terminal capabilities

**Enumerations**:

- `TerminalCapability` - Flags for terminal features (TrueColor, SIXEL, Kitty, Unicode, ASCII)
- `MediaFormat` - Audio, Video, or Both
- `FrameType` - Video or Audio frame

#### Layer 2: ViewModels (`LablabBean.Reactive/ViewModels/Media`)

**Purpose**: ReactiveUI-based ViewModels for MVVM pattern

**Classes**:

- `MediaPlayerViewModel` - Main player ViewModel with playback controls
  - Observable properties: `State`, `Position`, `Duration`, `Volume`
  - Reactive commands: `LoadMediaCommand`, `PlayCommand`, `PauseCommand`, `StopCommand`, `SeekCommand`
  - Binds to `IMediaService` observables

- `PlaylistViewModel` - Playlist management
  - `ObservableCollection<MediaInfo>` for playlist items
  - Commands: `AddCommand`, `RemoveCommand`, `NextCommand`, `PreviousCommand`, `ShuffleCommand`

- `AudioVisualizerViewModel` - Audio visualization data
  - `SpectrumData` (float array for FFT analysis)
  - `WaveformData` (float array for waveform display)
  - `VisualizationMode` (Spectrum, Waveform, VU Meter)

#### Layer 3: Core Plugin (`LablabBean.Plugins.MediaPlayer.Core`)

**Purpose**: Core media player service implementation

**Components**:

- `MediaPlayerPlugin` - Plugin entry point, registers services with DI
- `MediaService` - Implements `IMediaService`
  - Manages playback state machine
  - Selects renderer based on terminal capabilities
  - Coordinates decoder and renderer
  - Publishes reactive streams for UI binding

- `TerminalCapabilityDetector` - Implements `ITerminalCapabilityDetector`
  - Checks environment variables (`TERM`, `COLORTERM`)
  - Probes terminal with Device Attributes (DA1) queries
  - Detects SIXEL, Kitty Graphics Protocol, TrueColor support

**Key Logic**:

```csharp
// Renderer selection algorithm
var renderers = _registry.GetAll<IMediaRenderer>()
    .Where(r => r.SupportedFormats.Contains(mediaInfo.Format))
    .Where(r => r.RequiredCapabilities.All(terminalInfo.Capabilities.Contains))
    .OrderByDescending(r => r.Priority);

_activeRenderer = renderers.FirstOrDefault();
```

#### Layer 4: Playback Engines

##### FFmpeg Engine (`LablabBean.Plugins.MediaPlayer.FFmpeg`)

**Purpose**: Decode video/audio using FFmpeg/OpenCvSharp

**Implementation**:

- Uses `OpenCvSharp.VideoCapture` for video decoding
- Alternatively: `FFmpeg.AutoGen` for more control
- Decodes frames on background thread
- Publishes frames via `IObservable<MediaFrame>`
- Supports seeking, pause/resume

**Priority**: 100 (highest)

##### NAudio Engine (`LablabBean.Plugins.MediaPlayer.NAudio`) - Optional

**Purpose**: Audio-only playback with visualization

**Implementation**:

- Uses NAudio for audio decoding
- Extracts waveform and FFT data
- Lower priority than FFmpeg (audio-only fallback)

**Priority**: 50

#### Layer 5: Terminal Renderers

##### Braille Renderer (`LablabBean.Plugins.MediaPlayer.Terminal.Braille`)

**Priority**: 10 (lowest - fallback)

**Required Capabilities**: `UnicodeBlockDrawing`

**Implementation**:

- Converts frames to braille characters (U+2800–U+28FF)
- Each character = 2×4 pixel block
- Luminance threshold determines dot pattern
- Supports 16-color mode (ANSI colors)
- Optimized for performance (SIMD if possible)

**Rendering Quality**: Low (monochrome/16-color, low resolution)

**Compatibility**: Universal (all modern terminals with UTF-8)

##### SIXEL Renderer (`LablabBean.Plugins.MediaPlayer.Terminal.Sixel`)

**Priority**: 50 (medium)

**Required Capabilities**: `Sixel`

**Implementation**:

- Generates SIXEL escape sequences
- Palette optimization (max 256 colors)
- Downscales images to terminal size
- Writes escape codes directly to console

**Rendering Quality**: Medium (256 colors, good resolution)

**Compatibility**: xterm, mlterm, WezTerm, mintty

##### Kitty Graphics Renderer (`LablabBean.Plugins.MediaPlayer.Terminal.Kitty`)

**Priority**: 100 (highest)

**Required Capabilities**: `KittyGraphics`

**Implementation**:

- Uses Kitty Graphics Protocol (OSC sequences)
- Base64-encodes frame data
- Caches frame IDs for efficiency
- Supports alpha channel blending
- Fast upload and placement

**Rendering Quality**: High (24-bit color, full resolution, alpha)

**Compatibility**: Kitty, WezTerm (partial)

##### Libcaca Renderer (`LablabBean.Plugins.MediaPlayer.Terminal.Caca`) - Optional

**Priority**: 30

**Required Capabilities**: `UnicodeBlockDrawing` or `AsciiOnly`

**Implementation**:

- P/Invoke to libcaca library (if available)
- Fallback to external `cacaview` process
- Color ASCII art rendering

**Rendering Quality**: Medium (color ASCII)

**Compatibility**: Wide (any terminal)

#### Layer 6: UI Views (`LablabBean.Console/Views/Media`)

##### MediaPlayerView (Terminal.Gui)

**Purpose**: Main player window container

**Layout**:

```
┌─────────────────────────────────────────────────────────┐
│ Media Player                                      [X]   │
├──────────────────────────────┬──────────────────────────┤
│                              │ Playlist                 │
│                              │ ┌──────────────────────┐ │
│                              │ │ 1. video.mp4    3:45 │ │
│    Video Viewport            │ │ 2. song.mp3     2:30 │ │
│    (Renderer Output)         │ │ 3. movie.mkv   45:00 │ │
│                              │ └──────────────────────┘ │
│                              │                          │
├──────────────────────────────┴──────────────────────────┤
│ ▶ Play  ⏸ Pause  ⏹ Stop  [====|----------] 00:45/03:20│
│ Volume: [||||||||    ] 80%                              │
└─────────────────────────────────────────────────────────┘
```

**Components**:

- Video viewport (70% width, full height minus controls)
- Playlist sidebar (30% width)
- Transport controls (bottom, 3 rows)
- Status bar with position/duration

##### MediaControlsView

**Purpose**: Playback transport controls

**Features**:

- Play/Pause/Stop buttons
- Volume slider
- Progress bar with time display
- Seek bar (click to jump)
- Keyboard shortcuts

##### PlaylistView

**Purpose**: Playlist management UI

**Features**:

- Scrollable list of media items
- Current item highlight
- Drag-drop file loading (if supported)
- Context menu (remove, move up/down)

##### AudioVisualizerView - Optional

**Purpose**: Visual audio feedback

**Modes**:

- Spectrum analyzer (frequency bars)
- Waveform display
- VU meter
- All rendered with braille/block characters

## File Structure

```
dotnet/
├── framework/
│   ├── LablabBean.Contracts.Media/          # NEW
│   │   ├── IMediaService.cs
│   │   ├── IMediaRenderer.cs
│   │   ├── IMediaPlaybackEngine.cs
│   │   ├── ITerminalCapabilityDetector.cs
│   │   ├── DTOs/
│   │   │   ├── MediaInfo.cs
│   │   │   ├── MediaFrame.cs
│   │   │   ├── PlaybackState.cs
│   │   │   ├── RenderContext.cs
│   │   │   └── TerminalInfo.cs
│   │   └── Enums/
│   │       ├── TerminalCapability.cs
│   │       ├── MediaFormat.cs
│   │       └── FrameType.cs
│   │
│   └── LablabBean.Reactive/
│       └── ViewModels/Media/                 # NEW
│           ├── MediaPlayerViewModel.cs
│           ├── PlaylistViewModel.cs
│           └── AudioVisualizerViewModel.cs
│
├── plugins/
│   ├── LablabBean.Plugins.MediaPlayer.Core/         # NEW
│   │   ├── MediaPlayerPlugin.cs
│   │   ├── Services/
│   │   │   ├── MediaService.cs
│   │   │   ├── PlaybackEngine.cs
│   │   │   ├── RendererSelector.cs
│   │   │   └── TerminalCapabilityDetector.cs
│   │   └── Tests/
│   │       ├── MediaServiceTests.cs
│   │       └── RendererSelectionTests.cs
│   │
│   ├── LablabBean.Plugins.MediaPlayer.FFmpeg/       # NEW
│   │   ├── FFmpegPlaybackPlugin.cs
│   │   ├── FFmpegPlaybackEngine.cs
│   │   ├── Decoders/
│   │   │   ├── VideoDecoder.cs
│   │   │   └── AudioDecoder.cs
│   │   └── Tests/
│   │       └── FFmpegPlaybackTests.cs
│   │
│   ├── LablabBean.Plugins.MediaPlayer.NAudio/       # NEW (optional)
│   │   ├── NAudioPlugin.cs
│   │   └── NAudioEngine.cs
│   │
│   ├── LablabBean.Plugins.MediaPlayer.Terminal.Braille/   # NEW
│   │   ├── BrailleRendererPlugin.cs
│   │   ├── BrailleRenderer.cs
│   │   ├── Converters/
│   │   │   ├── BrailleConverter.cs
│   │   │   └── ColorQuantizer.cs
│   │   └── Tests/
│   │       └── BrailleConverterTests.cs
│   │
│   ├── LablabBean.Plugins.MediaPlayer.Terminal.Sixel/     # NEW
│   │   ├── SixelRendererPlugin.cs
│   │   ├── SixelRenderer.cs
│   │   └── SixelEncoder.cs
│   │
│   ├── LablabBean.Plugins.MediaPlayer.Terminal.Kitty/     # NEW
│   │   ├── KittyGraphicsPlugin.cs
│   │   ├── KittyGraphicsRenderer.cs
│   │   └── KittyProtocolHandler.cs
│   │
│   ├── LablabBean.Plugins.MediaPlayer.Terminal.Caca/      # NEW (optional)
│   │   ├── CacaRendererPlugin.cs
│   │   └── CacaRenderer.cs
│   │
│   └── LablabBean.Plugins.MediaPlayer.SadConsole/         # NEW (Windows)
│       ├── SadConsoleRendererPlugin.cs
│       └── SadConsoleRenderer.cs
│
└── console-app/
    └── LablabBean.Console/
        ├── Commands/
        │   └── MediaCommand.cs          # NEW
        └── Views/Media/                 # NEW
            ├── MediaPlayerView.cs
            ├── MediaControlsView.cs
            ├── PlaylistView.cs
            └── AudioVisualizerView.cs
```

## Key Design Decisions

### 1. Unified Service Interface

**Decision**: Single `IMediaService` handles both audio and video

**Rationale**:

- Simplifies UI layer (one ViewModel, one View)
- Renderer selection logic automatically adapts to media type
- Shared playback controls (play/pause/seek work for both)
- Easy to extend to mixed-media playlists

**Trade-off**: Slightly more complex service implementation vs. simpler API

### 2. Priority-Based Renderer Selection

**Decision**: Use existing plugin registry with priority metadata

**Rationale**:

- Leverages existing `ServiceRegistry` pattern from reporting system
- Automatic selection of best renderer for terminal capabilities
- Easy to add new renderers without modifying core service
- Clear fallback chain (Kitty → SIXEL → Caca → Braille)

**Implementation**:

```csharp
var renderers = _registry.GetAll<IMediaRenderer>()
    .Where(r => r.SupportedFormats.Contains(mediaInfo.Format))
    .Where(r => r.RequiredCapabilities.All(terminalInfo.Capabilities.Contains))
    .OrderByDescending(r => r.Priority);
```

### 3. Reactive Streams for Frame Delivery

**Decision**: Use `IObservable<MediaFrame>` for decoder → service → renderer pipeline

**Rationale**:

- Natural fit for streaming data (video frames, audio samples)
- Backpressure handling (throttle if renderer is slow)
- Easy to compose with Rx operators (Buffer, Throttle, Sample)
- Integrates with ReactiveUI ViewModels

**Example**:

```csharp
_playbackEngine.FrameStream
    .Buffer(TimeSpan.FromMilliseconds(1000.0 / targetFps))
    .Subscribe(async frame => await _activeRenderer.RenderFrameAsync(frame, ct));
```

### 4. Terminal Capability Detection

**Decision**: Detect capabilities at startup and cache results

**Rationale**:

- Terminal capabilities don't change during execution
- Avoid repeated probing (DA1 queries have latency)
- Fallback to environment variables if probing fails

**Detection Methods**:

1. Check `TERM` environment variable (e.g., "xterm-256color", "kitty")
2. Check `COLORTERM` for "truecolor"/"24bit"
3. Send Device Attributes query (`\x1b[c`) and parse response
4. Cache results in `TerminalInfo` singleton

### 5. MVVM with ReactiveUI

**Decision**: Use ReactiveUI for ViewModels with `[Reactive]` attributes

**Rationale**:

- Already used in project (`LablabBean.Reactive`)
- Automatic property change notifications
- Reactive commands with can-execute logic
- Easy to bind to Terminal.Gui, WPF, or Avalonia

**Example**:

```csharp
[Reactive] public PlaybackState State { get; private set; }

var canPlay = this.WhenAnyValue(x => x.State,
    state => state == PlaybackState.Paused || state == PlaybackState.Stopped);
PlayCommand = ReactiveCommand.CreateFromTask(_mediaService.PlayAsync, canPlay);
```

### 6. Background Decoding, Main Thread Rendering

**Decision**: Decode frames on background thread, render on UI thread

**Rationale**:

- Decoding is CPU-intensive (shouldn't block UI)
- Terminal.Gui requires updates on main thread
- Use `Application.MainLoop.Invoke()` for thread-safe rendering

**Implementation**:

```csharp
// Background thread (decoder)
Task.Run(async () => {
    while (!ct.IsCancellationRequested) {
        var frame = await _playbackEngine.DecodeNextFrameAsync(ct);
        _frameStream.OnNext(frame);
        await Task.Delay(frameDelay, ct);
    }
}, ct);

// UI thread (renderer)
_frameStream.Subscribe(frame => {
    Application.MainLoop.Invoke(() => {
        DrawFrameToView(frame);
        _view.SetNeedsDisplay();
    });
});
```

## Implementation Plan

### Phase 1: Foundation (2-3 days)

**Goals**: Create contracts, ViewModels, terminal detection

**Tasks**:

1. Create `LablabBean.Contracts.Media` project
   - Define all interfaces (`IMediaService`, `IMediaRenderer`, `IMediaPlaybackEngine`)
   - Create DTOs (MediaInfo, MediaFrame, PlaybackState, RenderContext, TerminalInfo)
   - Define enums (TerminalCapability, MediaFormat, FrameType)

2. Create ViewModels in `LablabBean.Reactive/ViewModels/Media/`
   - `MediaPlayerViewModel` with reactive properties and commands
   - `PlaylistViewModel` with ObservableCollection
   - `AudioVisualizerViewModel` for audio visualization

3. Implement `TerminalCapabilityDetector`
   - Environment variable checks
   - Device Attributes probing
   - Unit tests for different terminal types

**Deliverables**:

- `LablabBean.Contracts.Media.csproj`
- 3 ViewModel classes
- `TerminalCapabilityDetector.cs` with tests

### Phase 2: Core Service Plugin (3-4 days)

**Goals**: Implement main media service with renderer selection

**Tasks**:

1. Create `LablabBean.Plugins.MediaPlayer.Core` project
   - `MediaPlayerPlugin` class (implements `IPlugin`)
   - `MediaService` class (implements `IMediaService`)
   - Playback state machine (FSM)
   - Renderer selection algorithm

2. Implement playback control logic
   - `LoadAsync()` - open media, select renderer
   - `PlayAsync()` - start playback loop
   - `PauseAsync()` - pause without stopping decoder
   - `StopAsync()` - cleanup resources
   - `SeekAsync()` - seek to timestamp

3. Add reactive streams
   - `IObservable<PlaybackState>` for state changes
   - `IObservable<TimeSpan>` for position updates
   - `IObservable<TimeSpan>` for duration

4. Write unit tests
   - Mock renderers and decoders
   - Test renderer selection logic
   - Test state machine transitions

**Deliverables**:

- `LablabBean.Plugins.MediaPlayer.Core.csproj`
- `MediaService.cs` with full playback logic
- Unit test suite (>80% coverage)

### Phase 3: FFmpeg Playback Engine (2-3 days)

**Goals**: Implement video/audio decoding with FFmpeg

**Tasks**:

1. Create `LablabBean.Plugins.MediaPlayer.FFmpeg` project
   - Add NuGet: `OpenCvSharp4`, `OpenCvSharp4.runtime.{platform}`
   - Alternative: `FFmpeg.AutoGen` for more control

2. Implement `FFmpegPlaybackEngine`
   - `OpenAsync()` - open video file, extract metadata
   - `DecodeNextFrameAsync()` - read and decode frame
   - `SeekAsync()` - seek to timestamp
   - Frame buffering (circular buffer)

3. Add audio stream extraction
   - Extract audio samples
   - Convert to PCM format
   - Publish via `IObservable<MediaFrame>`

4. Implement frame rate pacing
   - Calculate delay from FPS
   - Use high-resolution timer (`SpinWait.SpinUntil`)

5. Add error handling
   - Corrupted file detection
   - Codec not supported errors
   - Graceful degradation

**Deliverables**:

- `LablabBean.Plugins.MediaPlayer.FFmpeg.csproj`
- `FFmpegPlaybackEngine.cs`
- Sample video tests (use Creative Commons test videos)

### Phase 4: Terminal Renderers (5-7 days)

#### Phase 4.1: Braille Renderer (2 days)

**Tasks**:

1. Create `LablabBean.Plugins.MediaPlayer.Terminal.Braille` project
2. Implement braille encoding
   - Frame to RGB conversion
   - Downscale to viewport size × 2×4
   - Luminance threshold for dot calculation
   - Color quantization to 16 ANSI colors
3. Optimize performance
   - SIMD intrinsics (if applicable)
   - Parallel processing for large frames
4. Write unit tests
   - Test braille encoding correctness
   - Performance benchmarks

**Deliverables**:

- `BrailleRenderer.cs` with full implementation
- Achieves >20 FPS on 80×24 terminal

#### Phase 4.2: SIXEL Renderer (2 days)

**Tasks**:

1. Create `LablabBean.Plugins.MediaPlayer.Terminal.Sixel` project
2. Implement SIXEL encoder
   - Frame to SIXEL escape sequence
   - Palette optimization (max 256 colors)
   - RLE compression for efficiency
3. Add capability detection
   - DA1 query for SIXEL support
   - Fallback if not supported
4. Test on multiple terminals
   - xterm, mlterm, WezTerm

**Deliverables**:

- `SixelRenderer.cs`
- Works on xterm-compatible terminals

#### Phase 4.3: Kitty Graphics Renderer (2 days)

**Tasks**:

1. Create `LablabBean.Plugins.MediaPlayer.Terminal.Kitty` project
2. Implement Kitty Graphics Protocol
   - Base64 encoding
   - OSC sequence generation
   - Frame ID caching
3. Add placement commands
   - Update existing frame vs. new frame
   - Position/size control
4. Test on Kitty/WezTerm

**Deliverables**:

- `KittyGraphicsRenderer.cs`
- Best quality rendering on supported terminals

#### Phase 4.4: Libcaca Renderer (1 day - optional)

**Tasks**:

1. Create `LablabBean.Plugins.MediaPlayer.Terminal.Caca` project
2. P/Invoke to libcaca (if available)
3. Fallback to external `cacaview` process

**Deliverables**:

- `CacaRenderer.cs`

### Phase 5: UI Components (4-5 days)

#### Phase 5.1: Media Player View (2 days)

**Tasks**:

1. Create `Views/Media/MediaPlayerView.cs`
2. Implement layout
   - Video viewport (FrameView)
   - Playlist sidebar (ListView)
   - Controls at bottom
3. Wire up ViewModel bindings
   - ReactiveUI `WhenAnyValue` subscriptions
   - Command bindings to buttons
4. Add keyboard shortcuts
   - Space: play/pause
   - Esc: stop
   - Arrow keys: seek, volume

**Deliverables**:

- `MediaPlayerView.cs`
- Functional player window in Terminal.Gui

#### Phase 5.2: Media Controls (1 day)

**Tasks**:

1. Create `MediaControlsView.cs`
2. Add transport buttons (play, pause, stop)
3. Add volume slider
4. Add progress bar with time display
5. Bind to ViewModel commands

**Deliverables**:

- `MediaControlsView.cs`
- Fully functional controls

#### Phase 5.3: Playlist View (1 day)

**Tasks**:

1. Create `PlaylistView.cs`
2. ListView with media items
3. Highlight current item
4. Add/remove commands
5. Next/previous navigation

**Deliverables**:

- `PlaylistView.cs`

#### Phase 5.4: Audio Visualizer (1 day - optional)

**Tasks**:

1. Create `AudioVisualizerView.cs`
2. Implement FFT for spectrum analysis
3. Render bars with braille/block chars
4. Add waveform mode

**Deliverables**:

- `AudioVisualizerView.cs`

### Phase 6: Integration & Polish (3-4 days)

#### Phase 6.1: CLI Integration (1 day)

**Tasks**:

1. Create `Commands/MediaCommand.cs`
2. Add CLI commands:
   - `lablab-bean media play <file>`
   - `lablab-bean media list-renderers`
   - `lablab-bean media test-capabilities`
3. Integrate with existing CLI system

**Deliverables**:

- `MediaCommand.cs`

#### Phase 6.2: Keyboard Shortcuts (0.5 day)

**Tasks**:

1. Add global key bindings
   - Space: play/pause
   - Esc: stop
   - ←/→: seek ±5s
   - ↑/↓: volume ±10%
   - F: toggle fullscreen
   - N/P: next/previous

**Deliverables**:

- Enhanced `MediaPlayerView` with shortcuts

#### Phase 6.3: Configuration (0.5 day)

**Tasks**:

1. Add `appsettings.json` section:

   ```json
   {
     "MediaPlayer": {
       "DefaultVolume": 0.8,
       "PreferredRenderer": "auto",
       "MaxFrameRate": 30,
       "BufferSize": 60,
       "SupportedFormats": ["mp4", "mkv", "avi", "mp3", "flac"]
     }
   }
   ```

2. Bind to `IOptions<MediaPlayerOptions>`

**Deliverables**:

- Configuration integration

#### Phase 6.4: Testing (2 days)

**Tasks**:

1. Unit tests for all services
2. Integration tests
   - Renderer selection
   - Playback lifecycle
3. End-to-end tests with sample videos
4. Performance benchmarks
   - Frame rate measurement
   - Memory usage profiling

**Deliverables**:

- Comprehensive test suite
- Performance report

### Phase 7: Windows UI Support (2-3 days - optional)

#### Phase 7.1: SadConsole Renderer (2 days)

**Tasks**:

1. Create `LablabBean.Plugins.MediaPlayer.SadConsole` project
2. Render frames to SadConsole texture
3. Use MonoGame sprite system
4. Full color + smooth rendering

**Deliverables**:

- `SadConsoleRenderer.cs`
- High-quality rendering in Windows app

#### Phase 7.2: Windows UI Integration (1 day)

**Tasks**:

1. Add media player window to `LablabBean.Windows`
2. Reuse ViewModels (shared code)
3. Add DirectX/OpenGL video output

**Deliverables**:

- Media player in Windows app

## Timeline Summary

| Phase | Description | Duration | Dependencies |
|-------|-------------|----------|--------------|
| **Phase 1** | Foundation (contracts, VMs, detection) | 2-3 days | None |
| **Phase 2** | Core service plugin | 3-4 days | Phase 1 |
| **Phase 3** | FFmpeg playback engine | 2-3 days | Phase 1 |
| **Phase 4** | Terminal renderers (4 plugins) | 5-7 days | Phase 1, 3 |
| **Phase 5** | UI components (Terminal.Gui views) | 4-5 days | Phase 1, 2 |
| **Phase 6** | Integration, CLI, testing | 3-4 days | Phase 2-5 |
| **Phase 7** | Windows UI (optional) | 2-3 days | Phase 1, 2 |
| **Total** | Core features | **19-26 days** | — |
| **Total** | With Windows UI | **21-29 days** | — |

### Minimal Viable Product (MVP)

**Fast-track to demo**: 11 days

1. Phase 1 (contracts) → 2 days
2. Phase 2 (core service) → 3 days
3. Phase 3 (FFmpeg engine) → 2 days
4. Phase 4.1 (Braille renderer only) → 2 days
5. Phase 5.1 (basic player view) → 2 days

## Dependencies

### NuGet Packages

**Existing**:

- `Terminal.Gui` (v2)
- `ReactiveUI`
- `ReactiveUI.Fody`
- `System.Reactive`
- `MessagePipe`
- `Microsoft.Extensions.DependencyInjection`

**New**:

- `OpenCvSharp4` (FFmpeg playback)
- `OpenCvSharp4.runtime.win` (Windows)
- `OpenCvSharp4.runtime.ubuntu` (Linux)
- `NAudio` (optional - audio-only playback)

### System Requirements

**Terminal Requirements**:

- UTF-8 encoding support (for braille)
- Optional: SIXEL support (xterm, mlterm, WezTerm)
- Optional: Kitty Graphics Protocol (Kitty, WezTerm)

**Runtime Requirements**:

- .NET 8.0+
- FFmpeg libraries (libav\*, included with OpenCvSharp runtime)

## Configuration

### appsettings.json

```json
{
  "MediaPlayer": {
    "DefaultVolume": 0.8,
    "PreferredRenderer": "auto",
    "FallbackRenderer": "braille",
    "MaxFrameRate": 30,
    "MinFrameRate": 10,
    "BufferSize": 60,
    "AutoPlay": false,
    "SupportedFormats": [
      "mp4",
      "mkv",
      "avi",
      "mov",
      "webm",
      "mp3",
      "flac",
      "wav",
      "ogg"
    ],
    "Renderers": {
      "Braille": {
        "Enabled": true,
        "ColorMode": "16color"
      },
      "Sixel": {
        "Enabled": true,
        "MaxColors": 256
      },
      "Kitty": {
        "Enabled": true,
        "Compression": "zlib"
      }
    }
  }
}
```

## Testing Strategy

### Unit Tests

**Scope**: Individual components in isolation

**Coverage**:

- `MediaService` - playback logic, state machine
- `TerminalCapabilityDetector` - capability detection
- `BrailleConverter` - braille encoding correctness
- `SixelEncoder` - SIXEL generation
- ViewModels - reactive properties, commands

**Tools**: xUnit, Moq, FluentAssertions

### Integration Tests

**Scope**: Multiple components together

**Coverage**:

- Renderer selection algorithm
- Decoder → Service → Renderer pipeline
- Plugin loading and registration

**Tools**: xUnit, TestHost

### End-to-End Tests

**Scope**: Full playback scenario

**Coverage**:

- Load video → Play → Pause → Seek → Stop
- Playlist navigation
- Renderer switching

**Tools**: xUnit with sample media files

### Performance Tests

**Metrics**:

- Frame rate (target: >20 FPS)
- Memory usage (target: <500 MB for HD video)
- CPU usage (target: <50% on modern CPU)
- Latency (seek lag, control responsiveness)

**Tools**: BenchmarkDotNet

## Security Considerations

### Input Validation

- Validate file paths (no directory traversal)
- Validate media file formats (allow-list)
- Limit file size (configurable max)

### Resource Limits

- Max concurrent decoders (prevent resource exhaustion)
- Frame buffer size limits
- Timeout for decoder operations

### Error Handling

- Graceful degradation on unsupported formats
- User-friendly error messages
- Logging for debugging (no sensitive data)

## Accessibility

### Keyboard Navigation

- All functions accessible via keyboard
- Tab navigation through controls
- Mnemonics for buttons

### Screen Reader Support

- Descriptive labels for UI elements
- Announce state changes (playing, paused, etc.)

## Future Enhancements

### Phase 8: Advanced Features (future)

1. **Streaming Support**
   - HTTP/HTTPS streaming
   - RTSP/RTMP support
   - Adaptive bitrate

2. **Effects & Filters**
   - Brightness/contrast adjustment
   - Playback speed control
   - Audio equalizer

3. **Subtitles**
   - SRT/ASS subtitle parsing
   - Overlay on video
   - Multiple language support

4. **Recording**
   - Screen recording to video file
   - GIF export
   - Asciinema integration

5. **Network Playback**
   - DLNA/UPnP support
   - Chromecast integration
   - Multi-room audio

## References

### Technical Documentation

- [Terminal.Gui Documentation](https://gui-cs.github.io/Terminal.Gui/)
- [ReactiveUI Documentation](https://www.reactiveui.net/)
- [SIXEL Graphics](https://en.wikipedia.org/wiki/Sixel)
- [Kitty Graphics Protocol](https://sw.kovidgoyal.net/kitty/graphics-protocol/)
- [OpenCvSharp](https://github.com/shimat/opencvsharp)
- [Unicode Braille Patterns](https://en.wikipedia.org/wiki/Braille_Patterns)

### Related Code

- `LablabBean.Plugins.Core` - Plugin system reference
- `LablabBean.Reporting.*` - Renderer pattern reference
- `LablabBean.Reactive` - ViewModel base classes

## Appendix A: Interface Signatures

### IMediaService

```csharp
public interface IMediaService
{
    // Observable properties
    IObservable<PlaybackState> PlaybackState { get; }
    IObservable<TimeSpan> Position { get; }
    IObservable<TimeSpan> Duration { get; }
    IObservable<float> Volume { get; }

    // Playback control
    Task<MediaInfo> LoadAsync(string path, CancellationToken ct = default);
    Task PlayAsync(CancellationToken ct = default);
    Task PauseAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
    Task SeekAsync(TimeSpan position, CancellationToken ct = default);
    Task SetVolumeAsync(float volume, CancellationToken ct = default);

    // Info
    MediaInfo? CurrentMedia { get; }
    IMediaRenderer? ActiveRenderer { get; }
}
```

### IMediaRenderer

```csharp
public interface IMediaRenderer
{
    // Metadata
    string Name { get; }
    int Priority { get; }
    IEnumerable<MediaFormat> SupportedFormats { get; }
    IEnumerable<TerminalCapability> RequiredCapabilities { get; }

    // Lifecycle
    Task<bool> CanRenderAsync(MediaInfo media, TerminalInfo terminal, CancellationToken ct = default);
    Task InitializeAsync(RenderContext context, CancellationToken ct = default);
    Task RenderFrameAsync(MediaFrame frame, CancellationToken ct = default);
    Task CleanupAsync(CancellationToken ct = default);
}
```

### IMediaPlaybackEngine

```csharp
public interface IMediaPlaybackEngine
{
    // Metadata
    string Name { get; }
    int Priority { get; }
    IEnumerable<string> SupportedFileExtensions { get; }

    // Lifecycle
    Task<MediaInfo> OpenAsync(string path, CancellationToken ct = default);
    Task<MediaFrame> DecodeNextFrameAsync(CancellationToken ct = default);
    Task SeekAsync(TimeSpan position, CancellationToken ct = default);
    Task CloseAsync(CancellationToken ct = default);

    // Stream
    IObservable<MediaFrame> FrameStream { get; }
}
```

### ITerminalCapabilityDetector

```csharp
public interface ITerminalCapabilityDetector
{
    TerminalInfo DetectCapabilities();
    bool SupportsCapability(TerminalCapability capability);
    Task<bool> ProbeCapabilityAsync(TerminalCapability capability, CancellationToken ct = default);
}
```

## Appendix B: Data Models

### MediaInfo

```csharp
public record MediaInfo(
    string Path,
    MediaFormat Format,
    TimeSpan Duration,
    VideoInfo? Video,
    AudioInfo? Audio,
    Dictionary<string, string> Metadata
);

public record VideoInfo(
    int Width,
    int Height,
    double FrameRate,
    string Codec,
    int BitRate
);

public record AudioInfo(
    int SampleRate,
    int Channels,
    string Codec,
    int BitRate
);
```

### MediaFrame

```csharp
public record MediaFrame(
    byte[] Data,
    TimeSpan Timestamp,
    FrameType Type,
    int Width,
    int Height,
    PixelFormat PixelFormat
);

public enum FrameType
{
    Video,
    Audio
}

public enum PixelFormat
{
    RGB24,
    RGBA32,
    BGR24,
    BGRA32,
    PCM16
}
```

### PlaybackState

```csharp
public enum PlaybackState
{
    Stopped,
    Loading,
    Playing,
    Paused,
    Buffering,
    Error
}
```

### TerminalInfo

```csharp
public record TerminalInfo(
    string TerminalType,
    TerminalCapability Capabilities,
    int Width,
    int Height,
    bool SupportsColor,
    int ColorCount
);

[Flags]
public enum TerminalCapability
{
    None = 0,
    TrueColor = 1 << 0,           // 24-bit RGB color
    Sixel = 1 << 1,               // SIXEL graphics protocol
    KittyGraphics = 1 << 2,       // Kitty Graphics Protocol
    UnicodeBlockDrawing = 1 << 3, // Unicode box/braille characters
    AsciiOnly = 1 << 4,           // ASCII-only fallback
    MouseSupport = 1 << 5,        // Mouse input
    Hyperlinks = 1 << 6           // OSC 8 hyperlinks
}
```

## Appendix C: Terminal Capability Detection Code

```csharp
public class TerminalCapabilityDetector : ITerminalCapabilityDetector
{
    public TerminalInfo DetectCapabilities()
    {
        var capabilities = TerminalCapability.None;
        var term = Environment.GetEnvironmentVariable("TERM") ?? "";
        var colorterm = Environment.GetEnvironmentVariable("COLORTERM") ?? "";

        // True color support
        if (colorterm.Contains("truecolor") || colorterm.Contains("24bit"))
            capabilities |= TerminalCapability.TrueColor;

        // SIXEL support (common terminals)
        if (term.Contains("xterm") ||
            term.Contains("mlterm") ||
            term.Contains("wezterm") ||
            Environment.GetEnvironmentVariable("TERM_PROGRAM") == "WezTerm")
        {
            capabilities |= TerminalCapability.Sixel;
        }

        // Kitty Graphics Protocol
        if (term.Contains("kitty") ||
            term == "xterm-kitty" ||
            Environment.GetEnvironmentVariable("TERM_PROGRAM") == "WezTerm")
        {
            capabilities |= TerminalCapability.KittyGraphics;
        }

        // Unicode support
        if (Console.OutputEncoding.EncodingName.Contains("UTF"))
            capabilities |= TerminalCapability.UnicodeBlockDrawing;
        else
            capabilities |= TerminalCapability.AsciiOnly;

        // Color count
        int colorCount = 0;
        if (capabilities.HasFlag(TerminalCapability.TrueColor))
            colorCount = 16777216; // 2^24
        else if (term.Contains("256color"))
            colorCount = 256;
        else if (term.Contains("color"))
            colorCount = 16;
        else
            colorCount = 2;

        return new TerminalInfo(
            term,
            capabilities,
            Console.WindowWidth,
            Console.WindowHeight,
            colorCount > 2,
            colorCount
        );
    }

    public async Task<bool> ProbeCapabilityAsync(
        TerminalCapability capability,
        CancellationToken ct = default)
    {
        // Send Device Attributes query (DA1)
        Console.Write("\x1b[c");

        // Read response (timeout after 100ms)
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        try
        {
            var response = await ReadTerminalResponseAsync(cts.Token);

            // Parse CSI ? ... c response
            // Example: CSI ? 6 ; 4 c (SIXEL support indicated by "4")
            return capability switch
            {
                TerminalCapability.Sixel => response.Contains(";4"),
                _ => false
            };
        }
        catch (OperationCanceledException)
        {
            return false; // Timeout = not supported
        }
    }
}
```

---

**Document Version**: 1.0.0
**Last Updated**: 2025-10-26
**Author**: Claude
**Status**: Draft - Ready for Specification
