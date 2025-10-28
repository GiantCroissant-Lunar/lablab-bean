# Implementation Plan: Kitty Graphics Protocol for High-Quality Scene Rendering

**Branch**: `025-kitty-graphics-scene-rendering` (to be created)
**Date**: 2025-10-28
**Spec**: specs/025-kitty-graphics-scene-rendering/spec.md
**Input**: Implementation strategy for adding Kitty graphics protocol support to terminal scene rendering, achieving visual parity with SadConsole.

## Prerequisites

**⚠️ BLOCKERS - Do not start implementation until complete:**
1. ✅ Spec-024 merged to main
2. ✅ `ITerminalRenderBinding` contract available
3. ✅ WezTerm launcher and Terminal.Gui v2 working
4. ✅ Terminal capability detection functional

## Summary

Add high-quality tile/image rendering to terminal environments via Kitty graphics protocol:
- Extend `TileBuffer` to support pixel/image data
- Implement Kitty protocol encoder (`KittyGraphicsProtocol`)
- Enhance `TerminalSceneRenderer` with Kitty rendering path
- Add tileset asset loading and rasterization
- Update adapters to build image buffers when Kitty available
- Complete media player `KittyRenderer` implementation
- Maintain glyph-based fallback for non-Kitty terminals

## Phases & Deliverables

### Phase 1: Foundation - TileBuffer & Kitty Protocol (2-3 days)

**Goal**: Extend rendering contracts and implement Kitty escape sequence encoding.

**Tasks**:
1. **Extend TileBuffer** (FR-001)
   - File: `dotnet/framework/LablabBean.Rendering.Contracts/TileBuffer.cs`
   - Add `IsImageMode: bool` property
   - Add `byte[] PixelData` property for RGBA buffer
   - Add `ImageTile[,]? ImageTiles` property
   - Define `ImageTile` record: `(int TileId, uint? TintColor, byte Alpha)`
   - Maintain backward compatibility: existing glyph-only code unaffected

2. **Create Kitty Protocol Encoder** (FR-002)
   - New project: `dotnet/framework/LablabBean.Rendering.Terminal.Kitty/` (netstandard2.1)
   - File: `KittyGraphicsProtocol.cs`
   - Method: `string Encode(byte[] rgba, int width, int height, KittyOptions opts)`
   - Support transmission mode: `a=T` (direct), `a=t` (temp file)
   - Support chunked transmission: `m=1` (more chunks), `m=0` (last chunk)
   - Support placement IDs: `i=<id>` for efficient re-rendering
   - Encode pixel data as base64
   - Format: `\x1b_G<control>;base64data\x1b\\`

3. **Unit Tests**
   - Test TileBuffer modes (glyph, image, mixed)
   - Test Kitty encoding: small image (16x16), large image (800x600), chunking
   - Test placement ID generation

**Deliverables**:
- Extended TileBuffer contract
- Kitty protocol encoder library
- Unit tests passing

---

### Phase 2: Renderer Enhancement - Capability Detection & Kitty Path (3-4 days)

**Goal**: Add Kitty rendering support to `TerminalSceneRenderer` with automatic capability detection.

**Tasks**:
1. **Integrate Capability Detection** (FR-003)
   - File: `dotnet/plugins/LablabBean.Plugins.Rendering.Terminal/TerminalRenderingPlugin.cs`
   - Resolve `ITerminalCapabilityDetector` from DI
   - Detect Kitty support: check `TERM_PROGRAM`, `TERM`, WezTerm flags
   - Store in renderer: `_supportsKittyGraphics`, `_supportsSixel`
   - Log result: "Terminal capabilities: Kitty=YES, Sixel=NO, TrueColor=YES"

2. **Add ISceneRenderer.SupportsImageMode** (FR-006)
   - File: `dotnet/framework/LablabBean.Rendering.Contracts/ISceneRenderer.cs`
   - Add property: `bool SupportsImageMode { get; }`
   - Returns true if Kitty or Sixel available

3. **Implement Kitty Rendering Path** (FR-004)
   - File: `dotnet/plugins/LablabBean.Plugins.Rendering.Terminal/TerminalSceneRenderer.cs`
   - Method: `private Task RenderViaKittyGraphics(TileBuffer buffer)`
   - Check: `buffer.IsImageMode && buffer.PixelData != null`
   - Encode via `KittyGraphicsProtocol.Encode()`
   - Write to terminal: `Console.Write(escapeSequence)`
   - Handle viewport positioning: move cursor to render area before output

4. **Update RenderAsync Decision Tree**
   ```csharp
   public Task RenderAsync(TileBuffer buffer, CancellationToken ct)
   {
       if (buffer.IsImageMode && buffer.PixelData != null)
       {
           if (_supportsKittyGraphics)
               return RenderViaKittyGraphics(buffer);
           else if (_supportsSixel)
               return RenderViaSixel(buffer); // Future
       }

       // Fallback to glyph mode
       if (buffer.IsGlyphMode && buffer.Glyphs != null)
           RenderGlyphsToTerminal(buffer);

       return Task.CompletedTask;
   }
   ```

5. **Preserve Glyph Fallback**
   - Ensure existing `RenderGlyphsToTerminal()` unchanged
   - Test fallback: force `_supportsKittyGraphics = false`

**Deliverables**:
- Capability detection integrated
- Kitty rendering path implemented
- Glyph fallback preserved
- Logs show capability and rendering mode

---

### Phase 3: Tileset Pipeline - Asset Loading & Rasterization (3-4 days)

**Goal**: Load PNG tilesets and convert tile arrays to pixel buffers.

**Tasks**:
1. **Define Tileset Configuration** (FR-005)
   - File: `dotnet/console-app/LablabBean.Console/appsettings.json`
   - Add configuration:
     ```json
     "Rendering": {
       "Terminal": {
         "PreferHighQuality": true,
         "Tileset": "./assets/tiles.png",
         "TileSize": 16
       }
     }
     ```

2. **Create Tileset Loader**
   - New file: `dotnet/framework/LablabBean.Rendering.Contracts/TilesetLoader.cs`
   - Method: `Tileset Load(string path, int tileSize)`
   - Use `System.Drawing` or `ImageSharp` to load PNG
   - Parse into grid: `Dictionary<int, byte[]>` (tileId → RGBA pixels)
   - Cache tiles for reuse

3. **Implement Rasterizer**
   - New file: `dotnet/framework/LablabBean.Rendering.Contracts/TileRasterizer.cs`
   - Method: `byte[] Rasterize(ImageTile[,] tiles, Tileset tileset)`
   - Iterate over `ImageTile` array
   - Look up each tile sprite from tileset
   - Apply tint color if specified
   - Composite into single RGBA pixel buffer
   - Return as `TileBuffer.PixelData`

4. **Integrate into Renderer Initialization**
   - File: `TerminalSceneRenderer.cs`
   - In `InitializeAsync()`, load tileset if configured and Kitty available
   - Store tileset for use in rasterization

**Deliverables**:
- Tileset loader working with PNG files
- Rasterizer converts tile arrays to pixel buffers
- Configuration schema defined

---

### Phase 4: Adapter Integration - Quality Selection (2-3 days)

**Goal**: Update adapters to build image buffers when high-quality rendering is available.

**Tasks**:
1. **Check Renderer Capability** (FR-006)
   - File: `dotnet/console-app/LablabBean.Game.TerminalUI/TerminalUiAdapter.cs`
   - Check `_sceneRenderer.SupportsImageMode` at initialization
   - Log: "Renderer supports image mode: YES/NO"

2. **Add Image Buffer Builder**
   - New method: `private TileBuffer BuildImageTileBuffer(World world, DungeonMap map)`
   - Allocate `ImageTile[,]` array
   - Map game tiles to tile IDs:
     - Floor → TileId = 0
     - Wall → TileId = 1
     - Player → TileId = 10
     - Enemy → TileId = 11
   - Apply entity colors as tint
   - Rasterize via `TileRasterizer.Rasterize()`
   - Return TileBuffer with `IsImageMode = true`, `PixelData` set

3. **Update RenderFrame Logic**
   ```csharp
   public void RenderFrame()
   {
       TileBuffer buffer;

       if (_sceneRenderer.SupportsImageMode && _preferHighQuality)
           buffer = BuildImageTileBuffer(world, map);
       else
           buffer = BuildGlyphBuffer(world, map);

       await _sceneRenderer.RenderAsync(buffer);
   }
   ```

4. **Configuration**
   - Read `Rendering:Terminal:PreferHighQuality` from config
   - Default: `true` (favor high quality when available)

**Deliverables**:
- Adapter builds image buffers when Kitty available
- Gracefully falls back to glyph buffers
- Configuration controls behavior

---

### Phase 5: Media Player Completion (2 days)

**Goal**: Complete `KittyRenderer` for video playback, reusing Kitty protocol encoder.

**Tasks**:
1. **Implement KittyRenderer.RenderFrameAsync()** (FR-007)
   - File: `dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Kitty/KittyRenderer.cs`
   - Reference `LablabBean.Rendering.Terminal.Kitty` library
   - Extract RGBA pixels from `MediaFrame.Data`
   - Encode via `KittyGraphicsProtocol.Encode()`
   - Use placement ID for animation: `i=1` (reuse same ID for updates)
   - Write to terminal via `Console.Write()`

2. **Optimize for Animation**
   - Use placement IDs to avoid re-transmitting unchanged regions
   - Consider chunking for large frames

3. **Test Video Playback**
   - Play test video in WezTerm
   - Verify frame rate > 24 FPS
   - Verify fallback to Braille when Kitty unavailable

**Deliverables**:
- Video playback working via Kitty protocol
- Performance acceptable (> 24 FPS)

---

### Phase 6: Testing & Documentation (2 days)

**Goal**: Validate all scenarios and update documentation.

**Tasks**:
1. **Scenario Testing**
   - Test in WezTerm: Verify Kitty rendering for scene and video
   - Test in xterm: Verify glyph fallback
   - Test via SSH: Verify no Kitty escape sequences sent
   - Test with missing tileset: Verify fallback to glyphs

2. **Performance Testing**
   - Measure frame times for 80x24 and 160x48 viewports
   - Target: < 33ms per frame (30 FPS)
   - Profile Kitty encoding overhead

3. **Visual Comparison**
   - Screenshot console (Kitty) rendering
   - Screenshot Windows (SadConsole) rendering
   - Verify visual parity

4. **Update Documentation**
   - File: `docs/ui-rendering-binding.md` (add Kitty section)
   - File: `docs/findings/media-player-integration.md` (update with Kitty status)
   - Add: `docs/guides/KITTY_GRAPHICS_SETUP.md`

**Deliverables**:
- All acceptance scenarios passing
- Performance targets met
- Documentation updated

---

## Source Changes (High-Level)

### New Files
- `dotnet/framework/LablabBean.Rendering.Terminal.Kitty/KittyGraphicsProtocol.cs`
- `dotnet/framework/LablabBean.Rendering.Contracts/TilesetLoader.cs`
- `dotnet/framework/LablabBean.Rendering.Contracts/TileRasterizer.cs`
- `specs/025-kitty-graphics-scene-rendering/spec.md` ✅
- `specs/025-kitty-graphics-scene-rendering/plan.md` ✅
- `docs/guides/KITTY_GRAPHICS_SETUP.md`

### Modified Files
- `dotnet/framework/LablabBean.Rendering.Contracts/TileBuffer.cs` (add image mode)
- `dotnet/framework/LablabBean.Rendering.Contracts/ISceneRenderer.cs` (add SupportsImageMode)
- `dotnet/plugins/LablabBean.Plugins.Rendering.Terminal/TerminalSceneRenderer.cs` (add Kitty path)
- `dotnet/plugins/LablabBean.Plugins.Rendering.Terminal/TerminalRenderingPlugin.cs` (capability detection)
- `dotnet/console-app/LablabBean.Game.TerminalUI/TerminalUiAdapter.cs` (image buffer builder)
- `dotnet/plugins/LablabBean.Plugins.MediaPlayer.Terminal.Kitty/KittyRenderer.cs` (complete impl)
- `dotnet/console-app/LablabBean.Console/appsettings.json` (tileset config)

### Configuration Schema
```json
{
  "Rendering": {
    "Terminal": {
      "PreferHighQuality": true,
      "Tileset": "./assets/tiles.png",
      "TileSize": 16
    }
  }
}
```

## Tests

### Unit Tests
- `TileBufferTests`: Test glyph mode, image mode, property access
- `KittyGraphicsProtocolTests`: Test encoding, chunking, placement IDs
- `TilesetLoaderTests`: Test PNG loading, tile extraction
- `TileRasterizerTests`: Test rasterization, tinting, compositing

### Integration Tests
- `TerminalSceneRendererTests`: Test capability detection, rendering path selection
- `TerminalUiAdapterTests`: Test buffer building (glyph vs image)

### Scenario Tests
- Launch in WezTerm → Kitty rendering
- Launch in xterm → Glyph fallback
- Launch via SSH → Glyph fallback
- Missing tileset → Glyph fallback

## Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Kitty protocol complexity | Reference existing implementations (media player stubs, Terminal.Gui sixel) |
| Performance overhead of encoding | Profile and optimize; use placement IDs; cache static tiles |
| Tileset format compatibility | Use widely supported PNG; validate at load time |
| Breaking existing glyph rendering | Maintain glyph path unchanged; test fallback extensively |
| Terminal size variability | Render visible viewport only; handle resize events |

## Rollout

1. **Feature Branch**: Create `025-kitty-graphics-scene-rendering` from `main` (after spec-024 merge)
2. **Incremental Merge**: Merge phases 1-2 first (foundation), then 3-4 (tileset), then 5 (media)
3. **Validation**: Test on WezTerm, xterm, SSH before merging to main
4. **Tag**: Tag as `v0.0.4-kitty-graphics` after merge

## Dependencies

- **Spec-024**: Must be merged to main first ✅
- **ImageSharp or System.Drawing**: For PNG tileset loading
- **Terminal Capability Detection**: Already implemented in media player plugins
- **WezTerm**: Ships with project as primary terminal

---

**Version**: 1.0.0-draft
**Last Updated**: 2025-10-28
**Status**: Ready for implementation after spec-024 completion
