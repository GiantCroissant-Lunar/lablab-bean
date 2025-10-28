# Feature Specification: Kitty Graphics Protocol for High-Quality Scene Rendering

**Feature Branch**: `025-kitty-graphics-scene-rendering` (to be created)
**Created**: 2025-10-28
**Status**: Draft - Blocked by Spec-024
**Input**: Enable high-quality tile/image rendering in terminal environments using Kitty graphics protocol, achieving visual parity with Windows SadConsole rendering. Favor Kitty graphics whenever available, with graceful fallback to glyph-based rendering.

## Dependencies

**BLOCKED BY**: Spec-024 (Terminal.Gui v2 Binding + Aspire/WezTerm Stabilization)
- ✅ Requires: `ITerminalRenderBinding` contract established
- ✅ Requires: Stable WezTerm launcher and Terminal.Gui v2 initialization
- ✅ Requires: Plugin capability detection working
- ⚠️ **DO NOT BEGIN IMPLEMENTATION UNTIL SPEC-024 MERGES TO MAIN**

## Vision

Unify rendering quality across console (WezTerm) and Windows (SadConsole) platforms by leveraging the Kitty graphics protocol for high-quality tile/sprite rendering in terminal environments. Ship with WezTerm as the primary terminal, making Kitty graphics the default high-quality rendering path.

### Quality Tiers

```
┌─────────────────────────────────────────────┐
│  TIER 1: HIGH QUALITY (Primary Path)       │
│  - Console: Kitty Graphics Protocol         │
│  - Windows: SadConsole Native Tiles         │
│  - Visual Parity: ≈ Identical               │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│  TIER 2: LOW QUALITY (Fallback)            │
│  - Console: Glyph/Nerd Font Characters      │
│  - Use Case: SSH, Non-Kitty Terminals       │
└─────────────────────────────────────────────┘
```

## User Scenarios & Testing (mandatory)

### User Story 1 - High-quality scene rendering in WezTerm (Priority: P0)

As a player running the console version in WezTerm, when the game world renders, then I see tile-based graphics with similar visual quality to the Windows SadConsole version, not just ASCII characters.

Why this priority: This is the core value proposition of the feature and the reason for shipping with WezTerm.

Independent Test: Launch console in WezTerm with a map containing walls, floors, and entities. Verify tiles render as images via Kitty protocol, not as glyphs. Take screenshot and compare with SadConsole rendering.

Acceptance Scenarios:

1. Given WezTerm with Kitty support, when map renders, then tiles appear as pixel graphics (not runes/glyphs).
2. Given the same tileset asset (`./assets/tiles.png`), when rendered in console vs Windows, then visual appearance is nearly identical (same sprites, colors).
3. Given terminal capability detection running, when Kitty support is detected, then logs show "Using Kitty graphics protocol for high-quality rendering".

---

### User Story 2 - Graceful fallback to glyph rendering (Priority: P1)

As a player running on a terminal without Kitty support (SSH, standard xterm), when the game launches, then scene rendering automatically falls back to glyph-based rendering without errors.

Why this priority: Ensures accessibility and prevents breaking existing setups.

Independent Test: Set `TERM=xterm` and launch console. Verify glyph rendering activates and no Kitty escape sequences are sent. Check logs for fallback message.

Acceptance Scenarios:

1. Given terminal without Kitty support, when renderer initializes, then glyph mode is selected and logged.
2. Given fallback to glyph mode, when game runs, then performance is acceptable and UI is readable.
3. Given SSH session to remote server, when console launches, then glyph rendering works correctly.

---

### User Story 3 - Unified tileset across platforms (Priority: P1)

As a developer, I can use the same tileset PNG file for both console (Kitty) and Windows (SadConsole) rendering, configured once in `appsettings.json`.

Why this priority: Reduces asset duplication and ensures consistency.

Independent Test: Configure `Rendering:Tileset: "./assets/tiles.png"` in settings. Launch both console and Windows apps. Verify both load and use the same tileset file.

Acceptance Scenarios:

1. Given a tileset configured at `Assets:Tileset`, when both renderers initialize, then both load the same PNG file.
2. Given tileset with 16x16 tiles, when rendering a floor tile (ID=0), then both renderers show identical sprite.
3. Given no tileset configured, when renderer initializes, then it falls back to glyph mode with clear log message.

---

### Edge Cases

- Terminal reports Kitty support but rendering fails → Detect error and fallback to glyph mode; log warning.
- Tileset file missing or corrupted → Fallback to glyph mode; show error message.
- Very large tilesets (>1MB) → Optimize transmission with Kitty placement IDs and chunking.
- Terminal size smaller than game world → Render visible viewport only; use camera offset.
- Mixed mode (UI=glyphs, scene=Kitty) → Supported; UI uses Terminal.Gui widgets, scene uses Kitty graphics.

## Requirements (mandatory)

### Functional Requirements

- FR-001 (TileBuffer Extension): Extend `TileBuffer` to support image/pixel mode in addition to glyph mode.
  - Add `IsImageMode: bool` property
  - Add `byte[] PixelData` for RGBA pixel buffer
  - Add `ImageTile[,]` for tile-based representation (tile ID, tint, alpha)
  - Maintain backward compatibility with existing glyph mode

- FR-002 (Kitty Protocol Library): Implement Kitty graphics protocol encoder.
  - Create `LablabBean.Rendering.Terminal.Kitty` library (netstandard2.1)
  - Encode RGBA pixel data as Kitty escape sequences
  - Support transmission modes: direct (a=T), chunked (m=1)
  - Support placement IDs for efficient updates
  - Reference: https://sw.kovidgoyal.net/kitty/graphics-protocol/

- FR-003 (Capability Detection): Detect Kitty graphics support at renderer initialization.
  - Integrate with existing `ITerminalCapabilityDetector`
  - Check `TERM_PROGRAM=kitty` or `TERM=xterm-kitty` or WezTerm Kitty support
  - Store capability flags in renderer for render-time decisions

- FR-004 (Enhanced TerminalSceneRenderer): Extend `TerminalSceneRenderer` to support Kitty rendering.
  - Detect Kitty support in `InitializeAsync()`
  - Implement `RenderViaKittyGraphics(TileBuffer)` method
  - Select rendering path based on buffer mode and capabilities: Kitty > Sixel > Glyphs
  - Keep existing glyph rendering as fallback

- FR-005 (Tileset Asset Pipeline): Support tileset images for tile-based rendering.
  - Load PNG tileset at renderer initialization
  - Configuration: `Rendering:Terminal:Tileset`, `Rendering:Terminal:TileSize`
  - Parse tileset into individual tile sprites
  - Rasterize `ImageTile[,]` array into RGBA pixel buffer
  - Share tileset configuration with SadConsole renderer

- FR-006 (Adapter Quality Selection): Update adapters to build image buffers when high-quality rendering available.
  - Add `ISceneRenderer.SupportsImageMode` property
  - In `TerminalUiAdapter`, check if renderer supports image mode
  - Build `TileBuffer` in image mode (with tileset) if supported, else glyph mode
  - Configuration: `Rendering:Terminal:PreferHighQuality: true`

- FR-007 (Media Player Integration): Reuse Kitty protocol encoder for video playback.
  - Complete `KittyRenderer.RenderFrameAsync()` implementation in media player plugin
  - Share `KittyGraphicsProtocol` encoder between scene and media renderers
  - Support animation with placement IDs to reduce bandwidth

### Key Entities

- **TileBuffer** (extended): Dual-mode buffer supporting both glyphs and pixel data
- **ImageTile**: Tile representation (tile ID, tint color, alpha)
- **KittyGraphicsProtocol**: Encoder for Kitty escape sequences
- **Tileset**: PNG image containing tile sprites, loaded at initialization
- **RenderQuality**: Enum (HighQuality=Kitty, LowQuality=Glyphs)

## Success Criteria (mandatory)

### Measurable Outcomes

- SC-001: When launched in WezTerm, console app renders scene as pixel graphics (Kitty protocol), verified by screenshot comparison with SadConsole.
- SC-002: When launched in non-Kitty terminal (TERM=xterm), console app falls back to glyph rendering without errors or Kitty escape sequences.
- SC-003: Both console (Kitty) and Windows (SadConsole) load the same tileset PNG and render identical floor/wall tiles.
- SC-004: Renderer initialization logs capability detection result: "Kitty graphics: YES" or "Kitty graphics: NO, using glyphs".
- SC-005: Frame rendering time for Kitty graphics < 33ms (30 FPS) for a 80x24 viewport.
- SC-006: Media player video playback uses Kitty graphics when available, with > 24 FPS on 1080p video downscaled to terminal size.

## Non-Functional Requirements

- NFR-001: Rendering quality MUST favor Kitty graphics when available (configurable via `PreferHighQuality`).
- NFR-002: Fallback to glyph rendering MUST be automatic and require no user intervention.
- NFR-003: Tileset assets MUST be shared between console and Windows renderers (same file path).
- NFR-004: Performance MUST maintain > 30 FPS for typical game scenes (80x24 tiles).
- NFR-005: Implementation MUST NOT break existing glyph-based rendering for non-Kitty terminals.

## Out of Scope

- Sixel protocol implementation (deferred to future spec)
- iTerm2 inline images protocol
- Dynamic tileset hot-reloading
- Sprite animation within tiles (tiles are static images)
- Mouse-driven tile selection/interaction (covered by Terminal.Gui separately)

## Open Questions

- Should we support multiple tilesets (themes) selectable at runtime?
- Should we cache Kitty escape sequences for static tiles to reduce encoding overhead?
- Should we support different tile sizes for console vs Windows (e.g., 8x8 vs 16x16)?
- Should tileset support transparency/compositing for layered rendering?

## Milestones

1. **Foundation** (depends on spec-024 merge)
   - Extend TileBuffer with image mode
   - Implement Kitty protocol encoder library

2. **Renderer Enhancement**
   - Integrate capability detection
   - Implement Kitty rendering path in TerminalSceneRenderer
   - Keep glyph fallback working

3. **Tileset Pipeline**
   - Load PNG tileset at initialization
   - Rasterize tiles to pixel buffer
   - Configure shared tileset for console + Windows

4. **Adapter Integration**
   - Update TerminalUiAdapter to build image buffers
   - Add quality preference configuration

5. **Media Player Completion**
   - Complete KittyRenderer for video playback
   - Test video + scene rendering together

## Validation Plan

- **Visual**: Side-by-side screenshot comparison: Console (Kitty) vs Windows (SadConsole) rendering the same scene.
- **Capability**: Test on 3 terminals: WezTerm (Kitty works), xterm (fallback), SSH (fallback).
- **Performance**: Measure frame times with large maps (160x48 tiles); target < 33ms.
- **Asset Sharing**: Change tileset path in config; verify both renderers use the new tileset.
- **Fallback**: Disable Kitty in config; verify glyph rendering activates correctly.

## Technical References

- Kitty Graphics Protocol: https://sw.kovidgoyal.net/kitty/graphics-protocol/
- WezTerm Kitty Support: https://wezfurlong.org/wezterm/imgcat.html
- Existing Spec-024: `specs/024-name-terminalguibinding-refactor/spec.md`
- Media Player Plugins: `dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.*`
- Scene Renderer: `dotnet/plugins/LablabBean.Plugins.Rendering.Terminal/TerminalSceneRenderer.cs`

---

**Version**: 1.0.0-draft
**Last Updated**: 2025-10-28
**Status**: Draft - Awaiting Spec-024 Completion
