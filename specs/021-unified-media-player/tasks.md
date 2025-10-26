# Tasks: Unified Media Player

**Feature Branch**: `021-unified-media-player`
**Input**: Design documents from `/specs/021-unified-media-player/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: No explicit test requirements in spec.md - tests are OPTIONAL and not included in this task list.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create project directory structure per plan.md (7 new projects + existing extensions)
- [ ] T002 Create LablabBean.Contracts.Media.csproj in dotnet/framework/LablabBean.Contracts.Media/
- [ ] T003 Create LablabBean.Plugins.MediaPlayer.Core.csproj in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Core/
- [ ] T004 Create LablabBean.Plugins.MediaPlayer.FFmpeg.csproj in dotnet/plugins/LablabBean.Plugins.MediaPlayer.FFmpeg/
- [ ] T005 Create LablabBean.Plugins.MediaPlayer.Terminal.Braille.csproj in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Braille/
- [ ] T006 Create LablabBean.Plugins.MediaPlayer.Terminal.Sixel.csproj in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Sixel/
- [ ] T007 Create LablabBean.Plugins.MediaPlayer.Terminal.Kitty.csproj in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Kitty/
- [ ] T008 [P] Add NuGet package references to LablabBean.Contracts.Media (System.Reactive, Microsoft.Extensions.DependencyInjection)
- [ ] T009 [P] Add NuGet package references to FFmpeg plugin (OpenCvSharp4, OpenCvSharp4.runtime.win)
- [ ] T010 [P] Add NuGet package references to all renderer plugins (Terminal.Gui v2, ReactiveUI, ReactiveUI.Fody)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core contracts and infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Contracts and DTOs

- [ ] T011 [P] Copy IMediaService.cs from specs/021-unified-media-player/contracts/ to dotnet/framework/LablabBean.Contracts.Media/
- [ ] T012 [P] Copy IMediaRenderer.cs from specs/021-unified-media-player/contracts/ to dotnet/framework/LablabBean.Contracts.Media/
- [ ] T013 [P] Copy IMediaPlaybackEngine.cs from specs/021-unified-media-player/contracts/ to dotnet/framework/LablabBean.Contracts.Media/
- [ ] T014 [P] Copy ITerminalCapabilityDetector.cs from specs/021-unified-media-player/contracts/ to dotnet/framework/LablabBean.Contracts.Media/
- [ ] T015 [P] Create MediaInfo.cs record in dotnet/framework/LablabBean.Contracts.Media/DTOs/ (path, format, duration, video, audio, metadata)
- [ ] T016 [P] Create VideoInfo.cs record in dotnet/framework/LablabBean.Contracts.Media/DTOs/ (width, height, frameRate, codec, bitRate)
- [ ] T017 [P] Create AudioInfo.cs record in dotnet/framework/LablabBean.Contracts.Media/DTOs/ (sampleRate, channels, codec, bitRate)
- [ ] T018 [P] Create MediaFrame.cs record in dotnet/framework/LablabBean.Contracts.Media/DTOs/ (data, timestamp, type, width, height, pixelFormat)
- [ ] T019 [P] Create PlaybackState.cs record in dotnet/framework/LablabBean.Contracts.Media/DTOs/ (status, position, duration, volume, currentMedia, activePlaylist, errorMessage)
- [ ] T020 [P] Create Playlist.cs class in dotnet/framework/LablabBean.Contracts.Media/DTOs/ (name, items, currentIndex, shuffleEnabled, repeatMode, createdAt, modifiedAt)
- [ ] T021 [P] Create RenderContext.cs record in dotnet/framework/LablabBean.Contracts.Media/DTOs/ (targetView, viewportSize, terminalInfo)

### Enumerations

- [ ] T022 [P] Create MediaFormat.cs enum in dotnet/framework/LablabBean.Contracts.Media/Enums/ (Audio, Video, Both)
- [ ] T023 [P] Create FrameType.cs enum in dotnet/framework/LablabBean.Contracts.Media/Enums/ (Video, Audio)
- [ ] T024 [P] Create PlaybackStatus.cs enum in dotnet/framework/LablabBean.Contracts.Media/Enums/ (Stopped, Loading, Playing, Paused, Buffering, Error)
- [ ] T025 [P] Create RepeatMode.cs enum in dotnet/framework/LablabBean.Contracts.Media/Enums/ (Off, Single, All)
- [ ] T026 [P] Create PixelFormat.cs enum in dotnet/framework/LablabBean.Contracts.Media/Enums/ (RGB24, RGBA32, BGR24, BGRA32, PCM16)

### Infrastructure

- [ ] T027 Add LablabBean.Contracts.Media project reference to LablabBean.Reactive.csproj
- [ ] T028 Create ViewModels/Media/ directory in dotnet/framework/LablabBean.Reactive/
- [ ] T029 [P] Update appsettings.json with MediaPlayer configuration section (defaultVolume, preferredRenderer, maxFrameRate, bufferSize, supportedFormats)
- [ ] T030 [P] Add Plugins.SearchPaths configuration for media player plugins in appsettings.json

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Basic Media Playback (Priority: P1) 🎯 MVP

**Goal**: Enable users to play video and audio files directly in terminal with basic playback controls (play, pause, stop, volume)

**Independent Test**: Load a sample video file and verify it plays with visual/audio output in the terminal. Test basic controls work (play, pause, stop, volume adjustment).

### Core Service Implementation

- [ ] T031 [US1] Create MediaService.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Core/Services/ (implement IMediaService)
- [ ] T032 [US1] Implement LoadAsync method in MediaService.cs (file validation, metadata extraction, renderer selection)
- [ ] T033 [US1] Implement PlayAsync method in MediaService.cs (start background decoding, frame streaming)
- [ ] T034 [US1] Implement PauseAsync method in MediaService.cs (pause decoding, maintain position)
- [ ] T035 [US1] Implement StopAsync method in MediaService.cs (stop decoding, reset position, release resources)
- [ ] T036 [US1] Implement SetVolumeAsync method in MediaService.cs (volume adjustment, persistence)
- [ ] T037 [US1] Implement PlaybackState observable in MediaService.cs (state change notifications)
- [ ] T038 [US1] Implement Position observable in MediaService.cs (10 Hz position updates during playback)
- [ ] T039 [US1] Implement Duration observable in MediaService.cs (emit after media load)
- [ ] T040 [US1] Implement Volume observable in MediaService.cs (emit on volume changes)

### FFmpeg Playback Engine

- [ ] T041 [P] [US1] Create FFmpegPlaybackEngine.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.FFmpeg/ (implement IMediaPlaybackEngine)
- [ ] T042 [US1] Implement OpenAsync in FFmpegPlaybackEngine.cs (VideoCapture initialization, metadata extraction)
- [ ] T043 [US1] Implement DecodeNextFrameAsync in FFmpegPlaybackEngine.cs (frame decoding, BGR to RGB conversion)
- [ ] T044 [US1] Implement DecodeLoop background task in FFmpegPlaybackEngine.cs (frame rate pacing, frame stream publishing)
- [ ] T045 [US1] Implement FrameStream observable in FFmpegPlaybackEngine.cs (publish decoded frames)
- [ ] T046 [US1] Implement CloseAsync in FFmpegPlaybackEngine.cs (release VideoCapture, cleanup resources)
- [ ] T047 [US1] Create FFmpegPlaybackPlugin.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.FFmpeg/ (plugin registration)

### Braille Renderer (Universal Fallback)

- [ ] T048 [P] [US1] Create BrailleRenderer.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Braille/ (implement IMediaRenderer)
- [ ] T049 [US1] Implement CanRenderAsync in BrailleRenderer.cs (check Unicode support)
- [ ] T050 [US1] Implement InitializeAsync in BrailleRenderer.cs (allocate rune buffers)
- [ ] T051 [US1] Implement RenderFrameAsync in BrailleRenderer.cs (convert RGB to braille, marshal to UI thread)
- [ ] T052 [US1] Create BrailleConverter.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Braille/Converters/ (2×4 pixel grid encoding)
- [ ] T053 [US1] Create ColorQuantizer.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Braille/Converters/ (RGB to 16-color ANSI)
- [ ] T054 [US1] Implement CleanupAsync in BrailleRenderer.cs (release buffers)
- [ ] T055 [US1] Create BrailleRendererPlugin.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Braille/ (plugin registration, priority 10)

### ViewModels

- [ ] T056 [P] [US1] Create MediaPlayerViewModel.cs in dotnet/framework/LablabBean.Reactive/ViewModels/Media/ (ReactiveUI ViewModel)
- [ ] T057 [US1] Add reactive properties to MediaPlayerViewModel.cs ([Reactive] State, Position, Duration, Volume)
- [ ] T058 [US1] Create PlayCommand as ReactiveCommand in MediaPlayerViewModel.cs (with can-execute logic)
- [ ] T059 [US1] Create PauseCommand as ReactiveCommand in MediaPlayerViewModel.cs
- [ ] T060 [US1] Create StopCommand as ReactiveCommand in MediaPlayerViewModel.cs
- [ ] T061 [US1] Create LoadMediaCommand as ReactiveCommand in MediaPlayerViewModel.cs
- [ ] T062 [US1] Subscribe to IMediaService observables in MediaPlayerViewModel constructor (PlaybackState, Position, Duration, Volume)
- [ ] T063 [US1] Implement Volume property binding with throttled service updates in MediaPlayerViewModel.cs

### Terminal.Gui Views

- [ ] T064 [P] [US1] Create Views/Media/ directory in dotnet/console-app/LablabBean.Console/
- [ ] T065 [US1] Create MediaPlayerView.cs in dotnet/console-app/LablabBean.Console/Views/Media/ (FrameView with layout)
- [ ] T066 [US1] Create MediaControlsView.cs in dotnet/console-app/LablabBean.Console/Views/Media/ (play, pause, stop buttons)
- [ ] T067 [US1] Bind MediaControlsView button events to ViewModel commands (PlayCommand, PauseCommand, StopCommand)
- [ ] T068 [US1] Create VolumeSlider in MediaControlsView.cs bound to ViewModel.Volume
- [ ] T069 [US1] Add position label to MediaControlsView.cs with WhenAnyValue binding to ViewModel.Position
- [ ] T070 [US1] Wire up Application.MainLoop.Invoke for thread-safe UI updates in MediaPlayerView.cs
- [ ] T071 [US1] Add keyboard shortcut handlers in MediaPlayerView.cs (Space for play/pause, Esc for stop)

### Plugin Registration

- [ ] T072 [US1] Create MediaPlayerPlugin.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Core/ (register IMediaService)
- [ ] T073 [US1] Register FFmpegPlaybackEngine in service collection via MediaPlayerPlugin
- [ ] T074 [US1] Register BrailleRenderer in service collection via BrailleRendererPlugin
- [ ] T075 [US1] Update plugin loader in Program.cs to discover media player plugins

### CLI Integration

- [ ] T076 [P] [US1] Create MediaCommand.cs in dotnet/console-app/LablabBean.Console/Commands/ (CLI command handler)
- [ ] T077 [US1] Implement "media play <file>" command in MediaCommand.cs
- [ ] T078 [US1] Implement "media stop" command in MediaCommand.cs
- [ ] T079 [US1] Register MediaCommand in CommandService

**Checkpoint**: User Story 1 complete - basic media playback functional with braille fallback renderer

---

## Phase 4: User Story 4 - Seek and Navigation (Priority: P2)

**Goal**: Enable users to seek to specific positions and see playback progress for navigation within media files

**Independent Test**: Play a media file, use seek controls to jump to different timestamps, verify position updates correctly

### Seek Implementation

- [ ] T080 [US4] Implement SeekAsync in FFmpegPlaybackEngine.cs (VideoCapture.Set PosFrames, keyframe alignment)
- [ ] T081 [US4] Implement SeekAsync in MediaService.cs (delegate to engine, preserve playback state)
- [ ] T082 [US4] Add SeekCommand to MediaPlayerViewModel.cs (ReactiveCommand with position parameter)
- [ ] T083 [US4] Add SeekForwardCommand (+5s) to MediaPlayerViewModel.cs
- [ ] T084 [US4] Add SeekBackwardCommand (-5s) to MediaPlayerViewModel.cs

### Progress UI

- [ ] T085 [P] [US4] Create ProgressBar/Slider control in MediaControlsView.cs
- [ ] T086 [US4] Bind ProgressBar to ViewModel.Position and ViewModel.Duration
- [ ] T087 [US4] Add drag handler on ProgressBar to trigger SeekCommand
- [ ] T088 [US4] Add timestamp display (current/total) to MediaControlsView.cs
- [ ] T089 [US4] Add keyboard shortcuts for seek in MediaPlayerView.cs (← for backward, → for forward)
- [ ] T090 [US4] Implement position clamping (0 to Duration) in SeekAsync methods

**Checkpoint**: Seek and navigation functional - users can jump to any position in media

---

## Phase 5: User Story 2 - Terminal-Adaptive Rendering (Priority: P2)

**Goal**: Automatically detect terminal capabilities and use the best available rendering method (Kitty, SIXEL, or Braille)

**Independent Test**: Run the same media file in different terminal emulators (Windows Terminal, Kitty, xterm) and verify each uses the appropriate renderer

### Terminal Capability Detection

- [ ] T091 [P] [US2] Create TerminalCapabilityDetector.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Core/Services/ (implement ITerminalCapabilityDetector)
- [ ] T092 [US2] Implement DetectCapabilities in TerminalCapabilityDetector.cs (environment variable checks: TERM, COLORTERM, TERM_PROGRAM)
- [ ] T093 [US2] Implement Kitty terminal detection in TerminalCapabilityDetector.cs (TERM=xterm-kitty)
- [ ] T094 [US2] Implement WezTerm detection in TerminalCapabilityDetector.cs (TERM_PROGRAM=WezTerm)
- [ ] T095 [US2] Implement iTerm2 detection in TerminalCapabilityDetector.cs (TERM_PROGRAM=iTerm.app)
- [ ] T096 [US2] Implement Windows Terminal detection in TerminalCapabilityDetector.cs (WT_SESSION present)
- [ ] T097 [US2] Implement Unicode support detection in TerminalCapabilityDetector.cs (Console.OutputEncoding)
- [ ] T098 [US2] Implement ProbeSixelSupport in TerminalCapabilityDetector.cs (DA1 query with 100ms timeout)
- [ ] T099 [US2] Implement ProbeCapabilityAsync in TerminalCapabilityDetector.cs (active terminal probing)
- [ ] T100 [US2] Implement SupportsCapability helper method in TerminalCapabilityDetector.cs

### Renderer Selection Logic

- [ ] T101 [US2] Create RendererSelector.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Core/Services/
- [ ] T102 [US2] Implement SelectOptimalRenderer in RendererSelector.cs (filter by format, filter by capabilities, order by priority, select first)
- [ ] T103 [US2] Integrate RendererSelector into MediaService.LoadAsync (use selected renderer for playback)
- [ ] T104 [US2] Add renderer logging in MediaService.LoadAsync (log selected renderer name and priority)

### SIXEL Renderer

- [ ] T105 [P] [US2] Create SixelRenderer.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Sixel/ (implement IMediaRenderer)
- [ ] T106 [US2] Implement CanRenderAsync in SixelRenderer.cs (check Sixel capability)
- [ ] T107 [US2] Implement InitializeAsync in SixelRenderer.cs (allocate palette, set up escape sequences)
- [ ] T108 [US2] Create SixelEncoder.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Sixel/ (image to SIXEL escape sequence)
- [ ] T109 [US2] Implement palette quantization in SixelEncoder.cs (RGB to 256 colors)
- [ ] T110 [US2] Implement 6-pixel vertical encoding in SixelEncoder.cs (0x3F-0x7E range)
- [ ] T111 [US2] Implement RLE compression in SixelEncoder.cs (!N repeat syntax)
- [ ] T112 [US2] Implement RenderFrameAsync in SixelRenderer.cs (encode frame, write SIXEL to terminal)
- [ ] T113 [US2] Implement CleanupAsync in SixelRenderer.cs (clear graphics state)
- [ ] T114 [US2] Create SixelRendererPlugin.cs (plugin registration, priority 50)

### Kitty Graphics Renderer

- [ ] T115 [P] [US2] Create KittyGraphicsRenderer.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Kitty/ (implement IMediaRenderer)
- [ ] T116 [US2] Implement CanRenderAsync in KittyGraphicsRenderer.cs (check KittyGraphics capability)
- [ ] T117 [US2] Implement InitializeAsync in KittyGraphicsRenderer.cs (setup OSC sequence state)
- [ ] T118 [US2] Create KittyProtocolHandler.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Kitty/
- [ ] T119 [US2] Implement EncodeImage in KittyProtocolHandler.cs (RGB to base64, optional zlib compression)
- [ ] T120 [US2] Implement 4096-byte chunking in KittyProtocolHandler.cs (m=1 for continuation, m=0 for last)
- [ ] T121 [US2] Implement DisplayImage in KittyProtocolHandler.cs (placement command for cached images)
- [ ] T122 [US2] Implement RenderFrameAsync in KittyGraphicsRenderer.cs (encode frame, write OSC sequence to terminal)
- [ ] T123 [US2] Implement frame caching in KittyGraphicsRenderer.cs (reuse image IDs for keyframes)
- [ ] T124 [US2] Implement CleanupAsync in KittyGraphicsRenderer.cs (delete images, reset state)
- [ ] T125 [US2] Create KittyGraphicsPlugin.cs (plugin registration, priority 100)

### Integration

- [ ] T126 [US2] Register TerminalCapabilityDetector in MediaPlayerPlugin
- [ ] T127 [US2] Register RendererSelector in MediaPlayerPlugin
- [ ] T128 [US2] Register SixelRenderer in SixelRendererPlugin
- [ ] T129 [US2] Register KittyGraphicsRenderer in KittyGraphicsPlugin
- [ ] T130 [US2] Update MediaService to use TerminalCapabilityDetector during LoadAsync

### CLI Enhancements

- [ ] T131 [P] [US2] Implement "media list-renderers" command in MediaCommand.cs (show available renderers)
- [ ] T132 [P] [US2] Implement "media test-capabilities" command in MediaCommand.cs (show detected terminal capabilities)
- [ ] T133 [US2] Add --renderer flag to "media play" command for manual renderer selection

**Checkpoint**: Terminal-adaptive rendering complete - player automatically selects best renderer for each terminal

---

## Phase 6: User Story 3 - Playlist Management (Priority: P3)

**Goal**: Enable users to create and manage playlists for continuous playback of multiple files

**Independent Test**: Add multiple files to a queue, play through them, use next/previous controls, verify auto-advance and repeat/shuffle modes work

### Playlist Service

- [ ] T134 [P] [US3] Create PlaylistService.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Core/Services/
- [ ] T135 [US3] Implement AddItem method in PlaylistService.cs
- [ ] T136 [US3] Implement RemoveItem method in PlaylistService.cs (adjust currentIndex if needed)
- [ ] T137 [US3] Implement Move method in PlaylistService.cs (reorder items)
- [ ] T138 [US3] Implement Clear method in PlaylistService.cs
- [ ] T139 [US3] Implement Shuffle method in PlaylistService.cs (randomize order, preserve currentIndex to same item)
- [ ] T140 [US3] Implement Next method in PlaylistService.cs (advance with repeat mode logic)
- [ ] T141 [US3] Implement Previous method in PlaylistService.cs
- [ ] T142 [US3] Implement auto-advance logic on playback completion in MediaService

### Playlist Persistence

- [ ] T143 [P] [US3] Create PlaylistRepository.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Core/
- [ ] T144 [US3] Implement SavePlaylist method in PlaylistRepository.cs (serialize to JSON)
- [ ] T145 [US3] Implement LoadPlaylist method in PlaylistRepository.cs (deserialize from JSON)
- [ ] T146 [US3] Add playlist file path resolution (use app data directory)
- [ ] T147 [US3] Implement validation on load (check files still exist)

### Playlist ViewModel

- [ ] T148 [P] [US3] Create PlaylistViewModel.cs in dotnet/framework/LablabBean.Reactive/ViewModels/Media/
- [ ] T149 [US3] Add reactive properties to PlaylistViewModel.cs ([Reactive] Items, CurrentIndex, ShuffleEnabled, RepeatMode)
- [ ] T150 [US3] Create AddItemCommand in PlaylistViewModel.cs
- [ ] T151 [US3] Create RemoveItemCommand in PlaylistViewModel.cs
- [ ] T152 [US3] Create NextCommand in PlaylistViewModel.cs
- [ ] T153 [US3] Create PreviousCommand in PlaylistViewModel.cs
- [ ] T154 [US3] Create ShuffleCommand in PlaylistViewModel.cs
- [ ] T155 [US3] Create ToggleRepeatCommand in PlaylistViewModel.cs (cycle Off → Single → All)
- [ ] T156 [US3] Create SavePlaylistCommand in PlaylistViewModel.cs
- [ ] T157 [US3] Create LoadPlaylistCommand in PlaylistViewModel.cs

### Playlist View

- [ ] T158 [P] [US3] Create PlaylistView.cs in dotnet/console-app/LablabBean.Console/Views/Media/ (ListView with items)
- [ ] T159 [US3] Bind PlaylistView.Items to ViewModel.Items with ObservableCollection
- [ ] T160 [US3] Highlight current item (ViewModel.CurrentIndex) in PlaylistView
- [ ] T161 [US3] Add context menu to PlaylistView (remove item, move up/down)
- [ ] T162 [US3] Add shuffle button to PlaylistView bound to ShuffleCommand
- [ ] T163 [US3] Add repeat mode button to PlaylistView bound to ToggleRepeatCommand
- [ ] T164 [US3] Add save/load buttons to PlaylistView
- [ ] T165 [US3] Add keyboard shortcuts in PlaylistView (N for next, P for previous, S for shuffle, R for repeat)
- [ ] T166 [US3] Integrate PlaylistView into MediaPlayerView layout

### Integration with MediaService

- [ ] T167 [US3] Add ActivePlaylist property to MediaService (expose current playlist)
- [ ] T168 [US3] Implement playlist auto-advance in MediaService (on playback completion, call Next, load next item, play)
- [ ] T169 [US3] Update MediaPlayerViewModel to bind ActivePlaylist from MediaService
- [ ] T170 [US3] Add playlist-related commands to MediaCommand.cs ("media playlist add", "media playlist save", "media playlist load")

**Checkpoint**: Playlist management complete - users can queue multiple files and navigate with next/previous/shuffle/repeat

---

## Phase 7: User Story 5 - Audio Visualization (Priority: P4)

**Goal**: Provide visual feedback (spectrum analyzer, waveform) for audio-only files

**Independent Test**: Play an audio file and verify visual elements display and update in sync with audio

### Audio Visualizer Service

- [ ] T171 [P] [US5] Create AudioVisualizerService.cs in dotnet/plugins/LablabBean.Plugins.MediaPlayer.Core/Services/
- [ ] T172 [US5] Add Math.NET Numerics NuGet package for FFT (or alternative FFT library)
- [ ] T173 [US5] Implement ComputeSpectrum in AudioVisualizerService.cs (1024-sample FFT with Hamming window)
- [ ] T174 [US5] Implement GroupToLogBands in AudioVisualizerService.cs (convert linear FFT to 64 logarithmic bands)
- [ ] T175 [US5] Implement RenderSpectrum in AudioVisualizerService.cs (draw frequency bars using block/braille characters)
- [ ] T176 [US5] Implement ComputeWaveform in AudioVisualizerService.cs (amplitude over time)
- [ ] T177 [US5] Implement RenderWaveform in AudioVisualizerService.cs (draw waveform using line characters)
- [ ] T178 [US5] Add visualization mode enum (Spectrum, Waveform, VUMeter)

### Audio Visualizer ViewModel

- [ ] T179 [P] [US5] Create AudioVisualizerViewModel.cs in dotnet/framework/LablabBean.Reactive/ViewModels/Media/
- [ ] T180 [US5] Add reactive properties to AudioVisualizerViewModel.cs ([Reactive] VisualizationMode, SpectrumData, WaveformData)
- [ ] T181 [US5] Subscribe to MediaService.FrameStream for audio frames in AudioVisualizerViewModel constructor
- [ ] T182 [US5] Process audio frames with AudioVisualizerService and update reactive properties
- [ ] T183 [US5] Create SwitchVisualizationCommand in AudioVisualizerViewModel.cs (cycle modes)
- [ ] T184 [US5] Throttle visualization updates to 30 Hz for smooth rendering

### Audio Visualizer View

- [ ] T185 [P] [US5] Create AudioVisualizerView.cs in dotnet/console-app/LablabBean.Console/Views/Media/ (custom View)
- [ ] T186 [US5] Implement spectrum bar rendering in AudioVisualizerView.cs (draw vertical bars)
- [ ] T187 [US5] Implement waveform rendering in AudioVisualizerView.cs (draw amplitude line)
- [ ] T188 [US5] Bind to ViewModel.SpectrumData and ViewModel.WaveformData with WhenAnyValue
- [ ] T189 [US5] Add visualization mode switcher button bound to SwitchVisualizationCommand
- [ ] T190 [US5] Add keyboard shortcut for visualization mode switch (V key)
- [ ] T191 [US5] Integrate AudioVisualizerView into MediaPlayerView (show when audio-only file playing)

### Integration

- [ ] T192 [US5] Register AudioVisualizerService in MediaPlayerPlugin
- [ ] T193 [US5] Update MediaPlayerView to conditionally show AudioVisualizerView for audio files
- [ ] T194 [US5] Update MediaPlayerViewModel to expose IsAudioOnly property
- [ ] T195 [US5] Wire up visualizer enable/disable based on MediaInfo.Format (Audio vs Video)

**Checkpoint**: Audio visualization complete - users see engaging visual feedback while playing audio files

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final quality checks

- [ ] T196 [P] Add comprehensive XML documentation comments to all public APIs in LablabBean.Contracts.Media
- [ ] T197 [P] Add error handling and logging (Serilog) across all services (MediaService, PlaylistService, renderers)
- [ ] T198 [P] Add user preference persistence (volume, visualization mode, repeat/shuffle state) in MediaService
- [ ] T199 [P] Add recent files tracking in MediaService (persist to appsettings.json)
- [ ] T200 [P] Implement graceful error messages for unsupported formats in MediaService.LoadAsync
- [ ] T201 [P] Implement graceful error messages for corrupted files in FFmpegPlaybackEngine.OpenAsync
- [ ] T202 [P] Add file size validation (default 10GB limit, configurable) in MediaService.LoadAsync
- [ ] T203 [P] Add terminal resize handling in all renderers (adapt to new viewport size)
- [ ] T204 [P] Code cleanup: remove TODO comments, unused using statements, apply consistent formatting
- [ ] T205 [P] Run Roslynator analysis and fix warnings across all media player projects
- [ ] T206 Run dotnet build on entire solution and fix any compilation errors
- [ ] T207 Test quickstart.md walkthrough scenarios (programmatic usage, Terminal.Gui view, CLI commands)
- [ ] T208 [P] Update root README.md with media player feature overview and link to quickstart.md
- [ ] T209 [P] Move research.md to docs/_inbox/ per R-DOC-002 (documentation inbox rule)
- [ ] T210 Validate all documentation has proper YAML front-matter per DOCUMENTATION-SCHEMA.md
- [ ] T211 Run python scripts/validate_docs.py and fix any documentation schema violations
- [ ] T212 Final smoke test: play video in 3 different terminal emulators (Windows Terminal, Kitty if available, standard terminal)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - US1 (Phase 3 - P1): Can start after Foundational - No dependencies on other stories ✅ **MVP**
  - US4 (Phase 4 - P2): Depends on US1 completion (extends MediaService and FFmpegPlaybackEngine)
  - US2 (Phase 5 - P2): Can start after Foundational - No dependencies on other stories (adds renderers)
  - US3 (Phase 6 - P3): Depends on US1 completion (extends MediaService with playlist logic)
  - US5 (Phase 7 - P4): Depends on US1 completion (uses FrameStream from MediaService)
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

```
Foundational (Phase 2) ── BLOCKS ALL ──┐
                                       │
                                       ├──> US1 (Phase 3 - P1) ✅ MVP
                                       │     │
                                       │     ├──> US4 (Phase 4 - P2) - Seek and Navigation
                                       │     ├──> US3 (Phase 6 - P3) - Playlist Management
                                       │     └──> US5 (Phase 7 - P4) - Audio Visualization
                                       │
                                       └──> US2 (Phase 5 - P2) - Terminal-Adaptive Rendering (independent)
```

### Within Each User Story

- Models/DTOs before services
- Services before ViewModels
- ViewModels before Views
- Core implementation before integration
- Plugin implementation before plugin registration

### Parallel Opportunities

**Setup Phase:**

- T002-T007 (all .csproj creation) can run in parallel
- T008-T010 (all NuGet package additions) can run in parallel

**Foundational Phase:**

- T011-T014 (all contract copies) can run in parallel
- T015-T021 (all DTOs) can run in parallel
- T022-T026 (all enums) can run in parallel
- T029-T030 (all configuration) can run in parallel

**Within Each User Story:**

- Tasks marked [P] can run in parallel within that story
- Example US1: T041-T047 (FFmpeg engine), T048-T055 (Braille renderer), T056-T063 (ViewModels), T064-T071 (Views) can all progress in parallel if team capacity allows

**Across User Stories:**

- After Foundational phase completes:
  - US1 (Phase 3) can start immediately ✅ **MVP**
  - US2 (Phase 5) can start immediately (independent)
  - After US1 completes: US4, US3, US5 can start

---

## Parallel Example: User Story 1 Implementation

**Scenario**: Multiple developers working on US1 simultaneously after Foundational phase

```bash
# Developer A: FFmpeg Engine (T041-T047)
Task: "Create FFmpegPlaybackEngine.cs"
Task: "Implement OpenAsync, DecodeNextFrameAsync, DecodeLoop, FrameStream, CloseAsync"

# Developer B: Braille Renderer (T048-T055)
Task: "Create BrailleRenderer.cs"
Task: "Implement CanRenderAsync, InitializeAsync, RenderFrameAsync, CleanupAsync"
Task: "Create BrailleConverter.cs and ColorQuantizer.cs"

# Developer C: ViewModels (T056-T063)
Task: "Create MediaPlayerViewModel.cs"
Task: "Add reactive properties and commands"
Task: "Subscribe to IMediaService observables"

# Developer D: Views (T064-T071)
Task: "Create MediaPlayerView.cs and MediaControlsView.cs"
Task: "Bind UI to ViewModel"
Task: "Add keyboard shortcuts"

# Developer E: Core Service (T031-T040)
Task: "Create MediaService.cs"
Task: "Implement LoadAsync, PlayAsync, PauseAsync, StopAsync, SetVolumeAsync"
Task: "Implement observables (PlaybackState, Position, Duration, Volume)"
```

All 5 developers can work in parallel, then integrate at T072-T079 (plugin registration and CLI).

---

## Implementation Strategy

### MVP First (Recommended)

**Goal**: Deliver working media player with braille rendering as quickly as possible

1. ✅ Complete Phase 1: Setup (T001-T010)
2. ✅ Complete Phase 2: Foundational (T011-T030) - CRITICAL
3. ✅ Complete Phase 3: US1 - Basic Media Playback (T031-T079) - **MVP COMPLETE**
4. **STOP and VALIDATE**:
   - Test playing video/audio files in terminal
   - Verify play, pause, stop, volume controls work
   - Verify braille rendering displays in any UTF-8 terminal
5. ✅ Optional: Add Phase 4 (US4 - Seek) for better MVP (T080-T090)
6. Deploy/demo MVP ✅

**Timeline Estimate**: 5-7 days (for solo developer)

### Incremental Delivery (Full Feature)

**Goal**: Deliver each user story as an independent increment

1. Foundation: Setup + Foundational → **Foundation Ready**
2. +US1: Basic Playback → **MVP** ✅ (5-7 days)
3. +US4: Seek and Navigation → **Enhanced MVP** (1-2 days)
4. +US2: Terminal-Adaptive Rendering → **Quality Boost** (3-4 days)
5. +US3: Playlist Management → **Convenience Feature** (2-3 days)
6. +US5: Audio Visualization → **Polish Feature** (2-3 days)
7. +Polish: Final touches → **Production Ready** (2-3 days)

**Total Timeline**: 15-22 days (for solo developer)

### Parallel Team Strategy

**Goal**: Maximize throughput with multiple developers

**Prerequisites** (all team together):

- Complete Setup (Phase 1)
- Complete Foundational (Phase 2)

**After Foundational is complete**:

**Sprint 1** (parallel):

- Team A: US1 - Basic Media Playback (P1)
- Team B: US2 - Terminal-Adaptive Rendering (P2)
- Result: After 5-7 days, have MVP + high-quality rendering

**Sprint 2** (parallel):

- Team A: US4 - Seek and Navigation (P2)
- Team B: US3 - Playlist Management (P3)
- Result: After 2-3 days, have full navigation + playlists

**Sprint 3**:

- Team A+B: US5 - Audio Visualization (P4)
- Result: After 2-3 days, have audio visualizations

**Sprint 4**:

- All teams: Polish & testing
- Result: After 2-3 days, production ready

**Total Timeline with 2 teams**: 11-16 days

---

## Task Counts by Phase

- **Phase 1 (Setup)**: 10 tasks
- **Phase 2 (Foundational)**: 20 tasks (CRITICAL PATH)
- **Phase 3 (US1 - Basic Playback)**: 49 tasks ✅ MVP
- **Phase 4 (US4 - Seek)**: 11 tasks
- **Phase 5 (US2 - Adaptive Rendering)**: 43 tasks
- **Phase 6 (US3 - Playlists)**: 36 tasks
- **Phase 7 (US5 - Visualization)**: 25 tasks
- **Phase 8 (Polish)**: 17 tasks

**Total**: 211 tasks

**Parallelizable**: 47 tasks marked [P] (22% can run in parallel)

---

## Notes

- **[P] marker**: Tasks marked [P] can run in parallel (different files, no dependencies within same phase)
- **[Story] marker**: Maps task to specific user story for traceability
- **File paths**: All tasks include exact file paths for clarity
- **User Story Independence**: Each user story (US1-US5) is independently completable and testable
- **MVP Strategy**: Complete US1 first for minimal viable product, then add features incrementally
- **Tests**: No explicit test requirements in spec.md, so no test tasks included
- **Commit Strategy**: Commit after completing each task or logical group (e.g., all DTOs, entire service)
- **Validation**: Stop at each checkpoint to validate story works independently before continuing

---

**Generated**: 2025-10-26
**Based on**: spec.md (5 user stories), plan.md (7 projects), data-model.md (8 entities), contracts/ (4 interfaces)
