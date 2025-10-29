# Tasks: Kitty Graphics Protocol for High-Quality Scene Rendering

**Input**: Design documents from `specs/025-kitty-graphics-scene-rendering/`
**Prerequisites**: spec.md (user stories), plan.md (implementation phases)

**Tests**: Tests are not explicitly requested in the spec. Manual testing and scenario validation will be used instead.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

This project uses .NET solution structure:
- **Framework**: `dotnet/framework/` (shared contracts and libraries)
- **Plugins**: `dotnet/plugins/` (plugin implementations)
- **Console App**: `dotnet/console-app/` (application adapters)
- **Specs**: `specs/025-kitty-graphics-scene-rendering/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create new projects and basic structure needed for Kitty graphics support

- [ ] T001 Create new project `dotnet/framework/LablabBean.Rendering.Terminal.Kitty` targeting netstandard2.1
- [ ] T002 Add project reference from `LablabBean.Rendering.Terminal.Kitty` to solution file
- [ ] T003 [P] Create project structure (Properties, AssemblyInfo) for Kitty library

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core rendering contracts and Kitty protocol that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### TileBuffer Extension (FR-001)

- [ ] T004 Extend `TileBuffer` class in `dotnet/framework/LablabBean.Rendering.Contracts/TileBuffer.cs` to add `IsImageMode` property
- [ ] T005 Add `byte[]? PixelData` property to `TileBuffer` for RGBA pixel buffer
- [ ] T006 [P] Add `ImageTile[,]? ImageTiles` property to `TileBuffer` for tile-based representation
- [ ] T007 [P] Create `ImageTile` record in `dotnet/framework/LablabBean.Rendering.Contracts/ImageTile.cs` with properties: TileId (int), TintColor (uint?), Alpha (byte)
- [ ] T008 Add constructor overload to `TileBuffer` supporting image mode: `TileBuffer(int width, int height, bool imageMode)`

### Kitty Protocol Encoder (FR-002)

- [ ] T009 Create `KittyGraphicsProtocol` class in `dotnet/framework/LablabBean.Rendering.Terminal.Kitty/KittyGraphicsProtocol.cs`
- [ ] T010 Implement `Encode(byte[] rgba, int width, int height)` method with base64 encoding and control data
- [ ] T011 [P] Create `KittyOptions` class in `dotnet/framework/LablabBean.Rendering.Terminal.Kitty/KittyOptions.cs` with properties: TransmissionMode, ChunkedTransmission, PlacementId
- [ ] T012 Add support for transmission mode `a=T` (direct) in Kitty escape sequence generation
- [ ] T013 [P] Add support for placement IDs `i=<id>` for efficient re-rendering
- [ ] T014 [P] Add support for chunked transmission `m=0` (last chunk) and `m=1` (more chunks)
- [ ] T015 Implement base64 encoding for pixel data in `Encode` method

### ISceneRenderer Enhancement (FR-006)

- [ ] T016 Add `bool SupportsImageMode { get; }` property to `ISceneRenderer` interface in `dotnet/framework/LablabBean.Rendering.Contracts/ISceneRenderer.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - High-quality scene rendering in WezTerm (Priority: P0) 🎯 MVP

**Goal**: Enable Kitty graphics protocol rendering for tile-based scene graphics in WezTerm, achieving visual parity with SadConsole

**Independent Test**: Launch console in WezTerm with a map containing walls, floors, and entities. Verify tiles render as images via Kitty protocol, not as glyphs. Take screenshot and compare with SadConsole rendering.

**Acceptance Criteria**:
1. Tiles appear as pixel graphics (not runes/glyphs) in WezTerm
2. Logs show "Using Kitty graphics protocol for high-quality rendering"
3. Visual quality similar to Windows SadConsole version

### Capability Detection Integration (FR-003)

- [ ] T017 [US1] Add field `_supportsKittyGraphics` to `TerminalSceneRenderer` in `dotnet/plugins/LablabBean.Plugins.Rendering.Terminal/TerminalSceneRenderer.cs`
- [ ] T018 [US1] Inject `ITerminalCapabilityDetector` into `TerminalRenderingPlugin` constructor in `dotnet/plugins/LablabBean.Plugins.Rendering.Terminal/TerminalRenderingPlugin.cs`
- [ ] T019 [US1] In `TerminalRenderingPlugin.ConfigureServices`, resolve `ITerminalCapabilityDetector` and detect Kitty support
- [ ] T020 [US1] Pass Kitty capability flag to `TerminalSceneRenderer` during registration
- [ ] T021 [US1] Log capability detection result: "Terminal capabilities: Kitty={Kitty}, Sixel={Sixel}, TrueColor={TrueColor}"

### Kitty Rendering Path Implementation (FR-004)

- [ ] T022 [US1] Implement `SupportsImageMode` property in `TerminalSceneRenderer` to return `_supportsKittyGraphics`
- [ ] T023 [US1] Create private method `RenderViaKittyGraphics(TileBuffer buffer)` in `TerminalSceneRenderer`
- [ ] T024 [US1] In `RenderViaKittyGraphics`, encode pixel data using `KittyGraphicsProtocol.Encode(buffer.PixelData, buffer.Width, buffer.Height)`
- [ ] T025 [US1] Write Kitty escape sequence to terminal using `Console.Write(escapeSequence)`
- [ ] T026 [US1] Add cursor positioning before Kitty output to render at correct viewport location
- [ ] T027 [US1] Update `RenderAsync` method to check `buffer.IsImageMode && buffer.PixelData != null` and call `RenderViaKittyGraphics`

### RenderAsync Decision Tree (FR-004)

- [ ] T028 [US1] Update `RenderAsync` in `TerminalSceneRenderer` to prioritize Kitty graphics when available
- [ ] T029 [US1] Add decision tree: if ImageMode + Kitty support → `RenderViaKittyGraphics`; else if GlyphMode → `RenderGlyphsToTerminal`
- [ ] T030 [US1] Add reference to `LablabBean.Rendering.Terminal.Kitty` project in `LablabBean.Plugins.Rendering.Terminal.csproj`

### Configuration and Logging

- [ ] T031 [US1] Add configuration section `Rendering:Terminal:PreferHighQuality` to `dotnet/console-app/LablabBean.Console/appsettings.json` (default: true)
- [ ] T032 [US1] Log rendering mode selection in `RenderAsync`: "Using Kitty graphics protocol" or "Using glyph-based rendering (fallback)"

**Checkpoint**: At this point, User Story 1 should be fully functional - Kitty rendering works in WezTerm

---

## Phase 4: User Story 2 - Graceful fallback to glyph rendering (Priority: P1)

**Goal**: Ensure automatic fallback to glyph-based rendering on terminals without Kitty support, without errors or visual artifacts

**Independent Test**: Set `TERM=xterm` and launch console. Verify glyph rendering activates and no Kitty escape sequences are sent. Check logs for fallback message.

**Acceptance Criteria**:
1. Glyph mode is selected and logged when Kitty unavailable
2. Performance is acceptable and UI is readable in glyph mode
3. SSH sessions use glyph rendering correctly

### Preserve Glyph Fallback (FR-004)

- [ ] T033 [US2] Verify existing `RenderGlyphsToTerminal` method in `TerminalSceneRenderer` remains unchanged
- [ ] T034 [US2] Test fallback path: set `_supportsKittyGraphics = false` and verify glyph rendering activates
- [ ] T035 [US2] Add fallback logging: "Kitty graphics not available, using glyph-based rendering"

### Error Handling for Kitty Failures

- [ ] T036 [US2] Add try-catch around `RenderViaKittyGraphics` to catch encoding or rendering errors
- [ ] T037 [US2] On Kitty rendering error, log warning and fallback to glyph mode: "Kitty rendering failed, falling back to glyphs"
- [ ] T038 [US2] Set `_supportsKittyGraphics = false` on repeated failures to avoid retry loops

### Capability Detection Edge Cases

- [ ] T039 [US2] Handle case where terminal reports Kitty support but rendering fails (detect and fallback)
- [ ] T040 [US2] Handle SSH sessions: ensure capability detection works over remote connections

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - Kitty in WezTerm, glyphs elsewhere

---

## Phase 5: User Story 3 - Unified tileset across platforms (Priority: P1)

**Goal**: Load and use the same PNG tileset for both console (Kitty) and Windows (SadConsole) rendering, configured in appsettings.json

**Independent Test**: Configure `Rendering:Tileset: "./assets/tiles.png"` in settings. Launch both console and Windows apps. Verify both load and use the same tileset file.

**Acceptance Criteria**:
1. Both renderers load the same PNG tileset file
2. Tileset with 16x16 tiles renders identical sprites in both console and Windows
3. Missing tileset triggers fallback to glyph mode with clear log message

### Tileset Configuration (FR-005)

- [ ] T041 [US3] Add configuration keys to `dotnet/console-app/LablabBean.Console/appsettings.json`: `Rendering:Terminal:Tileset` (path) and `Rendering:Terminal:TileSize` (16)
- [ ] T042 [US3] Add corresponding configuration to `dotnet/windows-app/LablabBean.Game.SadConsole/appsettings.json` for SadConsole
- [ ] T043 [US3] Create sample tileset PNG at `assets/tiles.png` with floor (ID=0) and wall (ID=1) tiles (16x16)

### Tileset Loader (FR-005)

- [ ] T044 [US3] Create `TilesetLoader` class in `dotnet/framework/LablabBean.Rendering.Contracts/TilesetLoader.cs`
- [ ] T045 [US3] Implement `Load(string path, int tileSize)` method using `System.Drawing.Image` or `ImageSharp` to load PNG
- [ ] T046 [US3] Parse PNG into grid: extract individual tile sprites as `Dictionary<int, byte[]>` (tileId → RGBA pixels)
- [ ] T047 [US3] Cache parsed tiles in `Tileset` class for reuse
- [ ] T048 [US3] Add error handling: if file not found, return null and log "Tileset not found, falling back to glyph mode"

### Tileset Rasterizer (FR-005)

- [ ] T049 [US3] Create `TileRasterizer` class in `dotnet/framework/LablabBean.Rendering.Contracts/TileRasterizer.cs`
- [ ] T050 [US3] Implement `Rasterize(ImageTile[,] tiles, Tileset tileset, int viewportWidth, int viewportHeight)` method
- [ ] T051 [US3] Iterate over `ImageTile[,]` array and look up each tile sprite from tileset by TileId
- [ ] T052 [US3] Apply tint color if `ImageTile.TintColor` is specified (multiply RGB channels)
- [ ] T053 [US3] Composite tiles into single RGBA pixel buffer matching viewport dimensions
- [ ] T054 [US3] Return byte[] as `TileBuffer.PixelData`

### Renderer Integration (FR-005)

- [ ] T055 [US3] In `TerminalSceneRenderer.InitializeAsync`, read tileset config from `IConfiguration`
- [ ] T056 [US3] Load tileset using `TilesetLoader.Load()` if configured and Kitty available
- [ ] T057 [US3] Store loaded `Tileset` instance in `TerminalSceneRenderer` for use during rendering
- [ ] T058 [US3] Log tileset load result: "Tileset loaded: {path}" or "Tileset not configured, using glyph mode"

**Checkpoint**: All user stories should now be independently functional - Kitty rendering uses tilesets

---

## Phase 6: Adapter Integration - Build Image Buffers (FR-006)

**Purpose**: Update TerminalUiAdapter to build image-mode TileBuffers when high-quality rendering is available

- [ ] T059 Check `_sceneRenderer.SupportsImageMode` in `TerminalUiAdapter` initialization in `dotnet/console-app/LablabBean.Game.TerminalUI/TerminalUiAdapter.cs`
- [ ] T060 Read `Rendering:Terminal:PreferHighQuality` config value (default: true)
- [ ] T061 Log: "Renderer supports image mode: {yes/no}"
- [ ] T062 Create private method `BuildImageTileBuffer(World world, DungeonMap map)` in `TerminalUiAdapter`
- [ ] T063 In `BuildImageTileBuffer`, allocate `ImageTile[,]` array matching viewport dimensions
- [ ] T064 Map game tiles to tile IDs: Floor → 0, Wall → 1, Player → 10, Enemy → 11
- [ ] T065 Apply entity colors as tint colors in `ImageTile` instances
- [ ] T066 Resolve `Tileset` from renderer and call `TileRasterizer.Rasterize(imageTiles, tileset)`
- [ ] T067 Return `TileBuffer` with `IsImageMode = true` and `PixelData` set
- [ ] T068 Update `RenderFrame()` logic: if `SupportsImageMode && PreferHighQuality` then `BuildImageTileBuffer`, else `BuildGlyphBuffer`

---

## Phase 7: Media Player Integration (FR-007)

**Purpose**: Complete KittyRenderer for video playback, reusing Kitty protocol encoder

- [ ] T069 Add project reference from `LablabBean.Plugins.MediaPlayer.Terminal.Kitty` to `LablabBean.Rendering.Terminal.Kitty`
- [ ] T070 Implement `KittyRenderer.RenderFrameAsync()` in `dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Kitty/KittyRenderer.cs`
- [ ] T071 Extract RGBA pixels from `MediaFrame.Data` property
- [ ] T072 Encode via `KittyGraphicsProtocol.Encode(pixels, frame.Width, frame.Height, options)`
- [ ] T073 Use placement ID `i=1` in `KittyOptions` for animation (reuse same ID for video updates)
- [ ] T074 Write escape sequence to terminal via `Console.Write()`
- [ ] T075 Add error handling: on encoding failure, log error and skip frame

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Testing, documentation, and performance optimization

### Scenario Testing

- [ ] T076 Test in WezTerm: launch console, verify Kitty rendering for scene and video
- [ ] T077 Test in xterm (TERM=xterm): verify glyph fallback activates, no Kitty escape sequences sent
- [ ] T078 Test via SSH: verify remote session uses glyph rendering correctly
- [ ] T079 Test with missing tileset: delete `assets/tiles.png`, verify fallback to glyphs with log message
- [ ] T080 Test with corrupted tileset: create invalid PNG, verify error handling and fallback

### Performance Testing

- [ ] T081 Measure frame rendering time for 80x24 viewport with Kitty graphics (target: < 33ms)
- [ ] T082 Measure frame rendering time for 160x48 viewport with Kitty graphics
- [ ] T083 Profile Kitty encoding overhead for different buffer sizes
- [ ] T084 Test media player video playback frame rate (target: > 24 FPS)

### Visual Comparison

- [ ] T085 Take screenshot of console (Kitty) rendering a test scene
- [ ] T086 Take screenshot of Windows (SadConsole) rendering the same test scene
- [ ] T087 Compare screenshots for visual parity: verify same tiles, colors, layout

### Documentation

- [ ] T088 [P] Update `docs/ui-rendering-binding.md` with Kitty graphics section
- [ ] T089 [P] Update `docs/findings/media-player-integration.md` with completed Kitty renderer status
- [ ] T090 [P] Create `docs/guides/KITTY_GRAPHICS_SETUP.md` with setup instructions and troubleshooting
- [ ] T091 [P] Add Kitty rendering notes to spec-025 completion document

### Final Validation

- [ ] T092 Run all acceptance scenarios from spec.md and verify success criteria
- [ ] T093 Verify SC-001: WezTerm renders as pixel graphics (screenshot comparison)
- [ ] T094 Verify SC-002: Non-Kitty terminal falls back without errors
- [ ] T095 Verify SC-003: Both console and Windows load same tileset
- [ ] T096 Verify SC-004: Capability detection logs shown
- [ ] T097 Verify SC-005: Frame time < 33ms for 80x24 viewport
- [ ] T098 Verify SC-006: Media player achieves > 24 FPS

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational phase completion
- **User Story 2 (Phase 4)**: Depends on Foundational phase completion, benefits from US1 implementation
- **User Story 3 (Phase 5)**: Depends on Foundational phase completion, requires US1 Kitty path
- **Adapter Integration (Phase 6)**: Depends on US3 completion (needs tileset pipeline)
- **Media Player (Phase 7)**: Depends on Foundational phase (Kitty encoder)
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P0)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - Should preserve US1 functionality
- **User Story 3 (P1)**: Depends on US1 (requires Kitty rendering path) - Adds tileset support

### Within Each User Story

- Capability detection before Kitty rendering path
- Protocol encoding before renderer integration
- Core implementation before adapter integration
- Story complete before moving to next priority

### Parallel Opportunities

- Within Setup (Phase 1): T001-T003 can run in parallel
- Within Foundational (Phase 2):
  - T004-T008 (TileBuffer) can run in parallel with T011, T013-T014 (KittyOptions/features)
  - T009-T015 (Kitty protocol) is a single logical unit
- Within US1 (Phase 3):
  - T017 (field) and T018 (inject) can run in parallel
  - T013-T014 (chunking, placement IDs) can run in parallel
- Within US2 (Phase 4): T033-T035 (preserve), T036-T038 (error handling), T039-T040 (edge cases) can run in parallel
- Within US3 (Phase 5):
  - T041-T043 (config) can run in parallel
  - T044-T048 (loader) and T049-T054 (rasterizer) can run in parallel
- Within Polish (Phase 8): All documentation tasks (T088-T091) can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch capability detection and Kitty options in parallel:
Task: "Add field _supportsKittyGraphics to TerminalSceneRenderer"
Task: "Inject ITerminalCapabilityDetector into TerminalRenderingPlugin"

# Configuration can happen in parallel with capability detection:
Task: "Add configuration section Rendering:Terminal:PreferHighQuality"
Task: "In TerminalRenderingPlugin.ConfigureServices, resolve ITerminalCapabilityDetector"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (High-quality Kitty rendering)
4. **STOP and VALIDATE**: Test User Story 1 independently in WezTerm
5. Deploy/demo if ready

**Result**: MVP delivers Kitty graphics rendering in WezTerm

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test in WezTerm → Demo Kitty rendering (MVP!)
3. Add User Story 2 → Test fallback in xterm → Verify graceful degradation
4. Add User Story 3 → Test tileset loading → Verify unified assets
5. Add Adapter Integration → Test image buffer building
6. Add Media Player → Test video playback with Kitty protocol
7. Polish → Complete testing and documentation

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Kitty rendering path)
   - Developer B: User Story 3 (Tileset pipeline) - can start in parallel
   - Developer C: Media Player Integration (FR-007) - can start in parallel
3. User Story 2 (fallback) integrates after US1 complete
4. All converge for testing and polish

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- **Tech Stack**: .NET 8, C#, netstandard2.1, Terminal.Gui, System.Drawing/ImageSharp
- **Project Structure**: Follows existing dotnet/framework, dotnet/plugins, dotnet/console-app layout
- **Spec Reference**: specs/025-kitty-graphics-scene-rendering/spec.md
- **Plan Reference**: specs/025-kitty-graphics-scene-rendering/plan.md
