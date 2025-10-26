# Data Model: Unified Media Player

**Feature**: 021-unified-media-player
**Date**: 2025-10-26
**Phase**: 1 - Design & Contracts

## Overview

This document defines the data entities, relationships, and state machines for the unified media player feature. All entities are technology-agnostic and align with the functional requirements in spec.md.

---

## Core Entities

### 1. MediaInfo

**Purpose**: Represents metadata for an audio or video file

**Attributes**:

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| Path | string | Yes | Absolute file path to media file |
| Format | MediaFormat | Yes | Audio, Video, or Both |
| Duration | TimeSpan | Yes | Total playback duration |
| Video | VideoInfo? | Conditional | Video stream metadata (null for audio-only) |
| Audio | AudioInfo? | Conditional | Audio stream metadata (null if no audio) |
| Metadata | Dictionary<string, string> | No | Additional metadata (title, artist, album, etc.) |

**Validation Rules**:

- Path must be valid file path
- Duration must be positive
- At least one of Video or Audio must be non-null
- If Format = Video, Video must be non-null
- If Format = Audio, Audio must be non-null

**Example**:

```
MediaInfo(
    Path: "D:/videos/sample.mp4",
    Format: MediaFormat.Video,
    Duration: 00:03:45,
    Video: VideoInfo(1920, 1080, 24.0, "h264"),
    Audio: AudioInfo(44100, 2, "aac"),
    Metadata: { "title": "Sample Video", "year": "2024" }
)
```

---

### 2. VideoInfo

**Purpose**: Video stream metadata

**Attributes**:

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| Width | int | Yes | Video width in pixels |
| Height | int | Yes | Video height in pixels |
| FrameRate | double | Yes | Frames per second |
| Codec | string | Yes | Video codec identifier (h264, vp9, etc.) |
| BitRate | int | No | Video bitrate in bps |

**Validation Rules**:

- Width and Height must be positive
- FrameRate must be > 0
- Common resolutions: 640×480, 1280×720, 1920×1080, 3840×2160

---

### 3. AudioInfo

**Purpose**: Audio stream metadata

**Attributes**:

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| SampleRate | int | Yes | Samples per second (Hz) |
| Channels | int | Yes | Number of audio channels (1=mono, 2=stereo) |
| Codec | string | Yes | Audio codec identifier (aac, mp3, flac, etc.) |
| BitRate | int | No | Audio bitrate in bps |

**Validation Rules**:

- SampleRate typically 44100 or 48000 Hz
- Channels typically 1 or 2 (up to 8 for surround)

---

### 4. MediaFrame

**Purpose**: Single decoded frame of video or audio data

**Attributes**:

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| Data | byte[] | Yes | Raw frame data (RGB pixels or PCM samples) |
| Timestamp | TimeSpan | Yes | Position in media timeline |
| Type | FrameType | Yes | Video or Audio |
| Width | int | Conditional | Frame width (video only) |
| Height | int | Conditional | Frame height (video only) |
| PixelFormat | PixelFormat | Conditional | RGB24, RGBA32, etc. (video only) |

**Validation Rules**:

- Data length must match expected size
  - Video: Width × Height × BytesPerPixel
  - Audio: SampleCount × Channels × BytesPerSample
- Timestamp must be >= 0 and <= Duration

---

### 5. Playlist

**Purpose**: Ordered collection of media files for sequential playback

**Attributes**:

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| Name | string | Yes | Playlist name |
| Items | List<MediaInfo> | Yes | Ordered list of media files |
| CurrentIndex | int | Yes | Index of currently playing/selected item (-1 if none) |
| ShuffleEnabled | bool | Yes | Whether shuffle mode is active |
| RepeatMode | RepeatMode | Yes | Off, Single (repeat one), All (repeat playlist) |
| CreatedAt | DateTime | Yes | Playlist creation timestamp |
| ModifiedAt | DateTime | Yes | Last modification timestamp |

**Validation Rules**:

- Name must not be empty
- Items list can be empty (valid empty playlist)
- CurrentIndex must be -1 or valid index into Items (0 to Items.Count-1)
- ModifiedAt >= CreatedAt

**Relationships**:

- Contains 0..N MediaInfo entities
- One item is "current" (CurrentIndex)

**Operations**:

- Add(MediaInfo item) - Append to end
- Remove(int index) - Remove item, adjust CurrentIndex if necessary
- Move(int fromIndex, int toIndex) - Reorder items
- Clear() - Remove all items, set CurrentIndex = -1
- Shuffle() - Randomize order (preserve CurrentIndex to same item)
- Next() - Advance CurrentIndex (wrap if RepeatMode = All)
- Previous() - Decrement CurrentIndex

---

### 6. PlaybackState

**Purpose**: Current state of media playback

**Attributes**:

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| Status | PlaybackStatus | Yes | Stopped, Loading, Playing, Paused, Buffering, Error |
| Position | TimeSpan | Yes | Current playback position |
| Duration | TimeSpan | Yes | Total media duration (0 if unknown) |
| Volume | float | Yes | Volume level (0.0 = mute, 1.0 = max) |
| CurrentMedia | MediaInfo? | No | Currently loaded media (null if none) |
| ActivePlaylist | Playlist? | No | Current playlist (null if single file) |
| ErrorMessage | string? | No | Error description (if Status = Error) |

**Validation Rules**:

- Position must be >= 0 and <= Duration
- Volume must be >= 0.0 and <= 1.0
- If Status != Stopped, CurrentMedia should be non-null
- If Status = Error, ErrorMessage should be non-null

---

### 7. TerminalInfo

**Purpose**: Detected terminal capabilities

**Attributes**:

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| TerminalType | string | Yes | Value of TERM environment variable |
| Capabilities | TerminalCapability | Yes | Flags enum of detected features |
| Width | int | Yes | Terminal width in characters |
| Height | int | Yes | Terminal height in characters |
| SupportsColor | bool | Yes | Whether terminal supports color |
| ColorCount | int | Yes | Number of colors (2, 16, 256, 16777216) |

**Validation Rules**:

- Width and Height must be positive
- ColorCount must be 2, 16, 256, or 16777216
- If Capabilities includes TrueColor, ColorCount must be 16777216

**Detection Time**: Startup only (capabilities don't change during runtime)

---

### 8. Renderer

**Purpose**: Metadata about a media renderer plugin

**Attributes**:

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| Name | string | Yes | Human-readable renderer name |
| Priority | int | Yes | Selection priority (higher = preferred) |
| SupportedFormats | List<MediaFormat> | Yes | Audio, Video, or Both |
| RequiredCapabilities | List<TerminalCapability> | Yes | Required terminal features |
| QualityLevel | string | No | "Low", "Medium", "High" (informational) |

**Validation Rules**:

- Priority typically 0-100 range
- At least one SupportedFormat
- At least one RequiredCapability (or empty for universal renderers)

**Priority Mapping**:

- 100: Kitty Graphics (highest quality)
- 50: SIXEL (medium quality)
- 30: Libcaca (color ASCII)
- 10: Braille (lowest quality, universal fallback)

---

## Enumerations

### MediaFormat

```
enum MediaFormat
{
    Audio = 1,
    Video = 2,
    Both = 3
}
```

### FrameType

```
enum FrameType
{
    Video = 1,
    Audio = 2
}
```

### PlaybackStatus

```
enum PlaybackStatus
{
    Stopped = 0,
    Loading = 1,
    Playing = 2,
    Paused = 3,
    Buffering = 4,
    Error = 5
}
```

### RepeatMode

```
enum RepeatMode
{
    Off = 0,      // No repeat
    Single = 1,   // Repeat current item
    All = 2       // Repeat entire playlist
}
```

### TerminalCapability

```
[Flags]
enum TerminalCapability
{
    None = 0,
    TrueColor = 1 << 0,           // 24-bit RGB color
    Sixel = 1 << 1,               // SIXEL graphics protocol
    KittyGraphics = 1 << 2,       // Kitty Graphics Protocol
    UnicodeBlockDrawing = 1 << 3, // Unicode box/braille characters
    AsciiOnly = 1 << 4,           // ASCII-only fallback
    MouseSupport = 1 << 5,        // Mouse input (for UI)
    Hyperlinks = 1 << 6           // OSC 8 hyperlinks (for metadata links)
}
```

### PixelFormat

```
enum PixelFormat
{
    RGB24 = 1,    // 3 bytes per pixel (R, G, B)
    RGBA32 = 2,   // 4 bytes per pixel (R, G, B, A)
    BGR24 = 3,    // 3 bytes per pixel (B, G, R) - OpenCV default
    BGRA32 = 4,   // 4 bytes per pixel (B, G, R, A)
    PCM16 = 10    // 2 bytes per sample (audio)
}
```

---

## State Machines

### Playback State Machine

**States**: Stopped, Loading, Playing, Paused, Buffering, Error

**Transitions**:

```
[Stopped]
  ├─> LoadAsync() → [Loading]
  └─> (terminal state, no media loaded)

[Loading]
  ├─> Load success → [Stopped] (ready to play)
  ├─> Load failure → [Error]
  └─> User cancels → [Stopped]

[Stopped] (media loaded but not playing)
  ├─> PlayAsync() → [Playing]
  ├─> LoadAsync(new file) → [Loading]
  └─> StopAsync() → [Stopped] (idempotent)

[Playing]
  ├─> PauseAsync() → [Paused]
  ├─> StopAsync() → [Stopped]
  ├─> End of media → [Stopped]
  ├─> Buffer underrun → [Buffering]
  ├─> Decoder error → [Error]
  └─> SeekAsync() → [Playing] (seek during playback)

[Paused]
  ├─> PlayAsync() → [Playing]
  ├─> StopAsync() → [Stopped]
  └─> SeekAsync() → [Paused] (seek while paused)

[Buffering]
  ├─> Buffer filled → [Playing]
  ├─> Buffer timeout → [Error]
  └─> StopAsync() → [Stopped]

[Error]
  ├─> StopAsync() → [Stopped] (reset)
  └─> LoadAsync(new file) → [Loading] (recover)
```

**Diagram**:

```
                    ┌─────────┐
                    │ Stopped │◄─────────┐
                    └────┬────┘          │
                         │               │
                    LoadAsync()       StopAsync()
                         │               │
                         ▼               │
                    ┌─────────┐          │
           ┌────────┤ Loading ├──────────┤
           │        └────┬────┘          │
    Load failure         │ Load success  │
           │             ▼               │
           │        ┌─────────┐          │
           │        │ Stopped │──────────┘
           │        │ (ready) │
           │        └────┬────┘
           │             │ PlayAsync()
           │             ▼
           │        ┌─────────┐
           │   ┌───┤ Playing ├───┐
           │   │    └────┬────┘   │
           │   │         │        │
           │   │    PauseAsync()  │ End of media
           │   │         │        │
           │   │    ┌────▼────┐   │
           │   │    │ Paused  │   │
           │   │    └────┬────┘   │
           │   │         │        │
           │   │    PlayAsync()   │
           │   │         │        │
           │   │         ▼        │
           │   │    [Playing]     │
           │   │                  │
           │   │ Buffer underrun  │
           │   │         │        │
           │   │    ┌────▼────────┤
           │   │    │ Buffering   │
           │   │    └────┬────────┘
           │   │         │ Buffer filled
           │   │         ▼
           │   │    [Playing]
           │   │
           │   └─> StopAsync() ──> [Stopped]
           │
           ▼
      ┌───────┐
      │ Error │
      └───┬───┘
          │
     StopAsync() / LoadAsync()
          │
          ▼
     [Stopped] / [Loading]
```

**Invariants**:

- Can only play from Stopped or Paused states
- Can only pause from Playing state
- Can stop from any state except Error (must first acknowledge error)
- SeekAsync() preserves current state (Playing stays Playing, Paused stays Paused)

---

### Playlist Navigation State

**States**: NoPlaylist, FirstItem, MiddleItem, LastItem

**Transitions**:

```
[NoPlaylist] (no playlist loaded or empty)
  └─> Add first item → [FirstItem]

[FirstItem] (CurrentIndex = 0, Items.Count > 0)
  ├─> Next() → [MiddleItem] or [LastItem] (if only 2 items)
  ├─> Remove first item → [NoPlaylist] or new [FirstItem]
  └─> Add item → [FirstItem] (still at index 0)

[MiddleItem] (0 < CurrentIndex < Items.Count - 1)
  ├─> Next() → [MiddleItem] or [LastItem]
  ├─> Previous() → [MiddleItem] or [FirstItem]
  ├─> Remove current → [MiddleItem] (adjust index)
  └─> End of current item (auto-advance) → Next()

[LastItem] (CurrentIndex = Items.Count - 1)
  ├─> Previous() → [MiddleItem] or [FirstItem]
  ├─> Next() with RepeatMode=All → [FirstItem] (wrap around)
  ├─> Next() with RepeatMode=Off → [LastItem] (idempotent, stop playback)
  ├─> End of item with RepeatMode=Single → [LastItem] (restart same item)
  └─> Remove last item → [NoPlaylist] or new [LastItem]
```

---

## Relationships

```
PlaybackState
  │
  ├──> 0..1 MediaInfo (CurrentMedia)
  │       │
  │       ├──> 0..1 VideoInfo
  │       └──> 0..1 AudioInfo
  │
  └──> 0..1 Playlist (ActivePlaylist)
          │
          └──> 0..N MediaInfo (Items)

TerminalInfo
  │
  └──> Detected at startup by TerminalCapabilityDetector

Renderer (multiple instances, registered in plugin registry)
  │
  ├──> Priority (for selection)
  ├──> RequiredCapabilities (match against TerminalInfo)
  └──> SupportedFormats (match against MediaInfo.Format)
```

---

## Data Flow

### 1. Media Loading Flow

```
User selects file
  │
  ▼
MediaService.LoadAsync(path)
  │
  ├──> IMediaPlaybackEngine.OpenAsync(path)
  │      │
  │      ├──> FFmpeg decodes container
  │      └──> Returns MediaInfo
  │
  ├──> ITerminalCapabilityDetector.DetectCapabilities()
  │      │
  │      └──> Returns TerminalInfo
  │
  ├──> Select renderer: IMediaRenderer[] renderers
  │      │
  │      ├──> Filter by SupportedFormats (matches MediaInfo.Format)
  │      ├──> Filter by RequiredCapabilities (matches TerminalInfo.Capabilities)
  │      ├──> OrderByDescending(r => r.Priority)
  │      └──> FirstOrDefault() → activeRenderer
  │
  └──> Update PlaybackState
       │
       ├──> Status = Stopped (ready)
       ├──> CurrentMedia = MediaInfo
       └──> Duration = MediaInfo.Duration
```

### 2. Playback Flow

```
User presses Play
  │
  ▼
MediaService.PlayAsync()
  │
  ├──> Update PlaybackState.Status = Playing
  │
  ├──> Start background decoding task
  │      │
  │      └──> IMediaPlaybackEngine.StartPlaybackAsync()
  │            │
  │            └──> Decode loop (background thread):
  │                 ├──> DecodeNextFrameAsync() → MediaFrame
  │                 ├──> Publish to FrameStream (IObservable<MediaFrame>)
  │                 └──> Delay for frame rate pacing
  │
  └──> Subscribe to FrameStream
       │
       └──> IMediaRenderer.RenderFrameAsync(frame)
            │
            ├──> Convert frame to renderer format (braille/SIXEL/Kitty)
            ├──> Marshal to UI thread (Application.MainLoop.Invoke())
            └──> Draw to Terminal.Gui View
```

### 3. Seek Flow

```
User drags seek slider
  │
  ▼
MediaService.SeekAsync(position)
  │
  ├──> IMediaPlaybackEngine.SeekAsync(position)
  │      │
  │      └──> FFmpeg seeks to nearest keyframe
  │
  ├──> Update PlaybackState.Position = position
  │
  └──> Resume playback at new position
```

---

## Persistence

### Playlist Persistence (JSON)

```json
{
  "name": "My Playlist",
  "items": [
    {
      "path": "D:/videos/video1.mp4",
      "duration": "00:03:45"
    },
    {
      "path": "D:/music/song1.mp3",
      "duration": "00:04:12"
    }
  ],
  "currentIndex": 0,
  "shuffleEnabled": false,
  "repeatMode": "Off",
  "createdAt": "2025-10-26T10:30:00Z",
  "modifiedAt": "2025-10-26T11:00:00Z"
}
```

### User Preferences (appsettings.json)

```json
{
  "MediaPlayer": {
    "DefaultVolume": 0.8,
    "PreferredRenderer": "auto",
    "RepeatMode": "Off",
    "ShuffleEnabled": false,
    "RecentFiles": [
      "D:/videos/recent1.mp4",
      "D:/videos/recent2.mp4"
    ],
    "LastPlaylist": "D:/playlists/default.json"
  }
}
```

---

## Validation Summary

| Entity | Key Validations |
|--------|----------------|
| MediaInfo | Path valid, Duration positive, Format matches streams |
| MediaFrame | Data size matches dimensions, Timestamp in range |
| Playlist | Items valid, CurrentIndex in bounds |
| PlaybackState | Position <= Duration, Volume 0-1, Status matches CurrentMedia |
| TerminalInfo | Dimensions positive, ColorCount valid, Capabilities consistent |
| Renderer | Priority reasonable, Capabilities/Formats non-empty |

---

**Phase 1 Data Model Complete** - Ready for contract generation
