# Implementation Plan: Unified Media Player

**Branch**: `021-unified-media-player` | **Date**: 2025-10-26 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/021-unified-media-player/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This feature implements a unified audio/video media player for terminal environments that automatically adapts rendering quality based on terminal capabilities. The player uses a plugin-based architecture to support multiple rendering backends (high-fidelity graphics protocols like Kitty and SIXEL, down to universal text-based braille rendering) while providing a consistent user experience through ReactiveUI MVVM patterns integrated with Terminal.Gui.

Primary technical approach: Leverage existing plugin system (`LablabBean.Plugins.Core`) for renderer selection via priority-based service registry. Implement terminal capability detection at startup to choose optimal renderer. Use FFmpeg (via OpenCvSharp) for media decoding. Build on existing ReactiveUI infrastructure for ViewModels with reactive property binding to Terminal.Gui views.

## Technical Context

**Language/Version**: C# / .NET 8.0
**Primary Dependencies**:

- Terminal.Gui v2 (console UI framework)
- ReactiveUI + ReactiveUI.Fody (MVVM with reactive properties)
- System.Reactive + R3 (reactive streams)
- OpenCvSharp4 + OpenCvSharp4.runtime.* (FFmpeg wrapper for media decoding)
- MessagePipe (pub/sub messaging)
- Microsoft.Extensions.DependencyInjection (IoC container)

**Storage**: File-based (JSON for playlists, user preferences via appsettings.json)

**Testing**: xUnit + Moq + FluentAssertions (unit tests), xUnit + TestHost (integration tests)

**Target Platform**: Cross-platform console (Windows Terminal, Linux terminals, macOS Terminal.app, Kitty, WezTerm, xterm)

**Project Type**: Plugin-based library with console application integration (follows existing multi-project structure: framework libraries + console-app + plugins)

**Performance Goals**:

- Video playback: 15+ FPS on text-only terminals, 24+ FPS on advanced graphics terminals
- Control responsiveness: <100ms input-to-action latency
- Memory usage: <500MB for HD video playback
- Load time: <2 seconds from file selection to playback start

**Constraints**:

- Terminal rendering limited by refresh rate and escape sequence support
- Must work on terminals without graphics support (text-only fallback mandatory)
- Real-time video decoding CPU-intensive (need background thread for decoding, main thread for rendering)
- Audio/video sync tolerance: ±50ms drift acceptable
- Cross-platform compatibility (Windows, Linux, macOS)

**Scale/Scope**:

- Support 10+ common media formats (MP4, MKV, AVI, MOV, WebM, MP3, FLAC, WAV, OGG, AAC)
- 4 renderer implementations (Braille, SIXEL, Kitty Graphics, Libcaca - optional)
- Playlist capacity: 1000+ items without performance degradation
- File size: Support up to 10GB media files (streaming decode, not full load)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Compliance with Core Principles

✅ **P-1: Documentation-First Development**

- Specification completed (spec.md) before implementation
- Architecture design documented in plan.md
- Will document ADRs for: renderer selection algorithm, terminal capability detection, plugin architecture

✅ **P-2: Clear Code Over Clever Code**

- Plugin architecture uses explicit interfaces (`IMediaRenderer`, `IMediaService`)
- Terminal capability detection logic isolated in dedicated service
- Renderer selection algorithm uses clear LINQ expression (no magic)

✅ **P-3: Testing Matters**

- Unit tests planned for all services (MediaService, renderers, capability detector)
- Integration tests for renderer selection logic
- End-to-end tests with sample media files
- BenchmarkDotNet for performance validation

✅ **P-4: Security Consciousness**

- File path validation to prevent directory traversal
- File size limits (configurable, default 10GB)
- Resource cleanup on errors (using statements, CancellationToken)
- No credentials or secrets (local file playback only)

✅ **P-5: User Experience Focus**

- Keyboard shortcuts for all controls (Space, arrows, Esc)
- Error messages user-friendly (not technical stack traces)
- Graceful degradation (fallback to text rendering if graphics fail)
- Visual feedback for all states (playing, paused, loading, error)

✅ **P-6: Separation of Concerns**

- Media decoding (FFmpegPlaybackEngine) separate from rendering (IMediaRenderer)
- UI (Terminal.Gui views) separate from business logic (ViewModels)
- Service layer (IMediaService) orchestrates without UI dependencies
- Clear layer boundaries: Contracts → Services → ViewModels → Views

✅ **P-7: Performance Awareness**

- Background thread for decoding (non-blocking UI)
- Frame buffering to smooth playback
- Renderer selection based on terminal capabilities (avoid overhead)
- Memory profiling planned (BenchmarkDotNet)

✅ **P-8: Build Automation**

- MSBuild integration (.csproj files)
- NuGet package restoration
- Test runner via `dotnet test`
- CI integration (follows existing project pattern)

✅ **P-9: Version Control Hygiene**

- Feature branch: `021-unified-media-player`
- Conventional commit messages: `feat(media): add player service`
- Clear PR description linking to spec
- No binary files in git (sample videos in .gitignore)

✅ **P-10: When in doubt, ask**

- Spec includes 10 documented assumptions
- Edge cases explicitly handled (10 scenarios documented)
- Out of scope section prevents scope creep

### Compliance with Normative Rules

✅ **R-DOC-001 to R-DOC-005**: Documentation rules

- Plan.md includes proper structure
- Research.md will include findings with rationale
- Will check docs/index/registry.json before creating new docs

✅ **R-CODE-001 to R-CODE-003**: Code quality rules

- No secrets (local files only, no API keys)
- Meaningful names: `MediaPlayerViewModel`, `BrailleRenderer`, `TerminalCapabilityDetector`
- Comments for complex algorithms (braille encoding, SIXEL generation)

✅ **R-TST-001 to R-TST-002**: Testing rules

- Critical paths covered: playback state machine, renderer selection, capability detection
- Build validation: `dotnet build && dotnet test` before commits

✅ **R-GIT-001 to R-GIT-002**: Git rules

- Conventional commits enforced
- .gitignore includes sample videos, test artifacts

✅ **R-PRC-001 to R-PRC-002**: Process rules

- ADR for: renderer selection algorithm, terminal capability detection approach
- Breaking changes: new plugin contracts (will document in CHANGELOG.md)

✅ **R-SEC-001 to R-SEC-002**: Security rules

- File path validation, size limits
- Minimum privileges (no admin required)

✅ **R-TOOL-001 to R-TOOL-003**: Tool integration rules

- Following Spec-Kit workflow: specify → plan → tasks → implement
- Will use `task` command if available (check Taskfile.yml)
- New spec (not update) - this is a new major feature

### Gate Evaluation (Initial - Before Phase 0)

**Status**: ✅ **PASSED** - All principles and rules compliant

**No violations requiring justification**

### Gate Re-Evaluation (After Phase 1 Design)

**Status**: ✅ **PASSED** - Design maintains full compliance

**Design Review**:

✅ **P-1: Documentation-First** - Research.md, data-model.md, contracts/, quickstart.md all completed before implementation

✅ **P-2: Clear Code** - Interface contracts are explicit and well-documented with usage examples

✅ **P-3: Testing** - Test strategy defined in spec.md, all components designed for testability

✅ **P-6: Separation of Concerns** - Clear boundaries between layers (contracts → services → ViewModels → Views)

✅ **R-DOC-002: Inbox First** - Technical research document written to docs/_inbox/ as required

✅ **All Other Rules**: No changes to compliance status

**Conclusion**: Design is ready to proceed to Phase 2 (Task Generation via `/speckit.tasks`)

## Project Structure

### Documentation (this feature)

```
specs/021-unified-media-player/
├── spec.md                      # Feature specification (completed)
├── plan.md                      # This file (in progress)
├── research.md                  # Phase 0: Technology research findings
├── data-model.md                # Phase 1: Entity models and state machines
├── quickstart.md                # Phase 1: Getting started guide
├── contracts/                   # Phase 1: API/interface contracts
│   ├── IMediaService.cs        # Core service contract
│   ├── IMediaRenderer.cs       # Renderer plugin contract
│   ├── IMediaPlaybackEngine.cs # Decoder contract
│   └── ITerminalCapabilityDetector.cs # Capability detection contract
├── checklists/
│   └── requirements.md         # Spec quality checklist (completed)
└── tasks.md                    # Phase 2: Task breakdown (NOT created by /speckit.plan)
```

### Source Code (repository root)

```
dotnet/
├── framework/                                  # Shared libraries
│   ├── LablabBean.Contracts.Media/            # NEW - Media contracts
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
│   ├── LablabBean.Reactive/                   # EXISTING - Extend with ViewModels
│   │   └── ViewModels/Media/                  # NEW
│   │       ├── MediaPlayerViewModel.cs
│   │       ├── PlaylistViewModel.cs
│   │       └── AudioVisualizerViewModel.cs
│   │
│   └── [other existing framework projects]
│
├── plugins/                                    # Plugin implementations
│   ├── LablabBean.Plugins.MediaPlayer.Core/   # NEW - Core service plugin
│   │   ├── MediaPlayerPlugin.cs
│   │   ├── Services/
│   │   │   ├── MediaService.cs
│   │   │   ├── PlaybackStateMachine.cs
│   │   │   ├── RendererSelector.cs
│   │   │   └── TerminalCapabilityDetector.cs
│   │   └── Tests/
│   │       ├── MediaServiceTests.cs
│   │       ├── RendererSelectionTests.cs
│   │       └── CapabilityDetectorTests.cs
│   │
│   ├── LablabBean.Plugins.MediaPlayer.FFmpeg/ # NEW - FFmpeg decoder plugin
│   │   ├── FFmpegPlaybackPlugin.cs
│   │   ├── FFmpegPlaybackEngine.cs
│   │   ├── Decoders/
│   │   │   ├── VideoDecoder.cs
│   │   │   └── AudioDecoder.cs
│   │   └── Tests/
│   │       └── FFmpegDecoderTests.cs
│   │
│   ├── LablabBean.Plugins.MediaPlayer.Terminal.Braille/ # NEW - Braille renderer
│   │   ├── BrailleRendererPlugin.cs
│   │   ├── BrailleRenderer.cs
│   │   ├── Converters/
│   │   │   ├── BrailleConverter.cs
│   │   │   └── ColorQuantizer.cs
│   │   └── Tests/
│   │       └── BrailleConverterTests.cs
│   │
│   ├── LablabBean.Plugins.MediaPlayer.Terminal.Sixel/   # NEW - SIXEL renderer
│   │   ├── SixelRendererPlugin.cs
│   │   ├── SixelRenderer.cs
│   │   ├── SixelEncoder.cs
│   │   └── Tests/
│   │       └── SixelEncoderTests.cs
│   │
│   ├── LablabBean.Plugins.MediaPlayer.Terminal.Kitty/   # NEW - Kitty Graphics renderer
│   │   ├── KittyGraphicsPlugin.cs
│   │   ├── KittyGraphicsRenderer.cs
│   │   ├── KittyProtocolHandler.cs
│   │   └── Tests/
│   │       └── KittyProtocolTests.cs
│   │
│   └── [other existing plugins]
│
├── console-app/                                # Console application
│   └── LablabBean.Console/                    # EXISTING - Extend with views
│       ├── Commands/
│       │   └── MediaCommand.cs                # NEW - CLI media commands
│       ├── Views/Media/                       # NEW
│       │   ├── MediaPlayerView.cs
│       │   ├── MediaControlsView.cs
│       │   ├── PlaylistView.cs
│       │   └── AudioVisualizerView.cs
│       └── [existing console files]
│
└── [other existing directories]

tests/                                          # Test projects
└── LablabBean.AI.Agents.Tests/                # EXISTING - Extend with media tests
    ├── Integration/
    │   ├── MediaPlayerIntegrationTests.cs     # NEW
    │   ├── RendererSelectionIntegrationTests.cs # NEW
    │   └── PlaylistIntegrationTests.cs        # NEW
    └── [existing tests]
```

**Structure Decision**: Multi-project plugin architecture (Option 1 variant)

This feature follows the existing project structure with:

- **Contracts project** (`LablabBean.Contracts.Media`) for interfaces and DTOs
- **ViewModels** added to existing `LablabBean.Reactive` project
- **Multiple plugin projects** for different components (core service, FFmpeg engine, renderers)
- **Console app extension** for Terminal.Gui views and CLI commands
- **Test integration** with existing test project structure

Rationale: Aligns with existing plugin system pattern (see `LablabBean.Plugins.Reporting.*` for reference). Allows independent development, testing, and versioning of renderers.

## Complexity Tracking

*No violations - this section is empty.*

All architecture decisions comply with project principles. The plugin-based approach follows existing patterns (`LablabBean.Plugins.Reporting.Html`, `LablabBean.Plugins.Video.FFmpeg`). Service registry pattern already established in `LablabBean.Plugins.Core.ServiceRegistry`.

---

## Phase 0: Outline & Research

**Status**: ✅ **COMPLETED** - 2025-10-26

**Generated Artifact**: `research.md` (comprehensive technology research)

**Research Completed:**

1. ✅ **Terminal Capability Detection Methods**
   - Multi-method detection strategy (environment variables + DA1 queries)
   - Environment variable patterns by terminal emulator
   - C# implementation with timeout handling
   - Terminal compatibility matrix (10+ terminals documented)

2. ✅ **Braille Character Encoding**
   - Unicode braille pattern block (U+2800-U+28FF) bit mapping
   - 2×4 pixel grid encoding algorithm
   - C# BrailleVideoEncoder implementation with color quantization
   - Performance benchmarks (~500 FPS theoretical)

3. ✅ **SIXEL Protocol Specification**
   - DCS escape sequence format with parameters
   - 6-pixel vertical encoding (0x3F-0x7E range)
   - C# SixelEncoder with palette optimization and RLE compression
   - Terminal compatibility matrix with version details

4. ✅ **Kitty Graphics Protocol**
   - OSC sequence structure with action keys (a, f, t, s, v, i, o)
   - Base64 encoding with 4096-byte chunking
   - C# KittyGraphicsEncoder with zlib compression
   - Frame caching and placement commands

5. ✅ **FFmpeg Integration via OpenCvSharp**
   - VideoCapture API usage for frame decoding
   - C# FFmpegPlaybackEngine implementation
   - Background decoding loop with frame rate pacing
   - Seek implementation (keyframe-based)

6. ✅ **ReactiveUI Integration with Terminal.Gui**
   - Manual subscription pattern (no XAML bindings)
   - Thread marshaling via `Application.MainLoop.Invoke()`
   - `WhenAnyValue` for property observation
   - ReactiveCommand integration with Terminal.Gui events

7. ✅ **Audio Visualization Algorithms**
   - FFT-based spectrum analyzer (1024-sample window)
   - C# AudioVisualizerService outline
   - Logarithmic frequency band grouping
   - Braille/block character bar rendering

**Key Decisions Documented:**

- Braille as universal fallback (Priority 10)
- SIXEL for mid-tier terminals (Priority 50)
- Kitty Graphics for highest quality (Priority 100)
- OpenCvSharp chosen over FFmpeg.AutoGen (managed API, simpler)
- ReactiveUI with manual subscriptions (no XAML available)

---

## Phase 1: Design & Contracts

**Status**: ✅ **COMPLETED** - 2025-10-26

**Generated Artifacts:**

1. ✅ **data-model.md**: Entity models, state machines, relationships
   - 8 core entities defined (MediaInfo, VideoInfo, AudioInfo, MediaFrame, Playlist, PlaybackState, TerminalInfo, Renderer)
   - 5 enumerations (MediaFormat, FrameType, PlaybackStatus, RepeatMode, TerminalCapability, PixelFormat)
   - 2 state machines (Playback FSM, Playlist Navigation FSM) with transition diagrams
   - Validation rules and relationships documented

2. ✅ **contracts/** directory: C# Interface definitions
   - `IMediaService.cs` - Core playback service (7 methods, 4 observables)
   - `IMediaRenderer.cs` - Renderer plugin interface (6 methods)
   - `IMediaPlaybackEngine.cs` - Decoder interface (6 methods + FrameStream)
   - `ITerminalCapabilityDetector.cs` - Capability detection (3 methods + TerminalInfo record)
   - All interfaces include comprehensive XML documentation with usage examples

3. ✅ **quickstart.md**: Getting started guide
   - Installation instructions (NuGet packages)
   - Service configuration examples
   - 3 usage scenarios (programmatic, Terminal.Gui, CLI)
   - Terminal compatibility matrix
   - Common tasks and troubleshooting

4. ✅ **Agent Context Updated**
   - Added C# / .NET 8.0 to language context
   - Added File-based storage (JSON) to database context
   - Updated via `.specify/scripts/powershell/update-agent-context.ps1`

**Design Highlights:**

- **Clean Architecture**: Contracts → Services → ViewModels → Views
- **Plugin-Based**: Renderers and decoders are hot-pluggable
- **Reactive Patterns**: IObservable for state changes and frame streams
- **Thread-Safe**: Background decoding, main-thread rendering
- **Well-Documented**: Every interface method has XML docs + examples

**Next**: Phase 2 - Task Generation via `/speckit.tasks`

---

## Notes

**Dependencies on Existing Code:**

- `LablabBean.Plugins.Core.ServiceRegistry` - for renderer registration/selection
- `LablabBean.Reactive.ViewModels.ViewModelBase` - for ViewModel inheritance
- `LablabBean.Infrastructure` - for DI, logging (Serilog), configuration
- `LablabBean.Console` - for Terminal.Gui integration

**Integration Points:**

- Plugin loader in `Program.cs` (existing pattern)
- Service registration in `ConfigureServices` (existing pattern)
- CLI command registration in `CommandService` (existing pattern)

**Risk Areas:**

- Terminal capability detection may fail on exotic terminals (mitigation: universal braille fallback)
- Audio/video sync drift (mitigation: buffer tuning, accept ±50ms tolerance)
- Memory usage with large files (mitigation: streaming decode, configurable buffer limits)
- Cross-platform escape sequence support (mitigation: test on 5+ terminals, document compatibility)
