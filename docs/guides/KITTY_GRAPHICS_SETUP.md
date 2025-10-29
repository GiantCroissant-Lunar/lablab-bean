# Kitty Graphics Protocol Setup Guide

**Last Updated**: 2025-10-28  
**Status**: Production Ready

## Overview

This guide explains how to configure and use the Kitty Graphics Protocol for high-quality tile-based rendering in the LablabBean console application. Kitty graphics enables pixel-perfect rendering of game scenes and video playback in compatible terminals.

## What is Kitty Graphics?

Kitty Graphics Protocol is a modern terminal graphics standard that allows applications to display images and animations directly in the terminal window. Unlike traditional ASCII/Unicode rendering, Kitty graphics provides:

- ✅ True pixel-level image rendering
- ✅ Full RGBA color support (millions of colors + transparency)
- ✅ Hardware-accelerated compositing
- ✅ Smooth animations and video playback
- ✅ High performance with minimal overhead

## Compatible Terminals

### Fully Supported ✅

- **WezTerm** (Recommended) - Windows, macOS, Linux
- **Kitty** - macOS, Linux
- **Konsole** (with Kitty mode enabled) - Linux

### Fallback Mode ⚠️

Other terminals (xterm, Windows Terminal, Alacritty, etc.) automatically fall back to ASCII/Unicode rendering with no configuration needed.

## Configuration

### 1. Basic Setup

The Kitty graphics integration is enabled by default. Configuration is in `appsettings.json`:

```json
{
  "Rendering": {
    "Terminal": {
      "Tileset": "./assets/tiles.png",
      "TileSize": 16,
      "PreferHighQuality": true
    }
  }
}
```

**Configuration Options:**

- **Tileset**: Path to tileset PNG file (relative or absolute)
- **TileSize**: Size of each tile in pixels (typically 8, 16, or 32)
- **PreferHighQuality**: If `true`, uses Kitty graphics when available; if `false`, uses ASCII mode

### 2. Creating a Tileset

Your tileset should be a PNG image organized in a grid:

**Requirements:**
- Format: PNG (24-bit RGB or 32-bit RGBA)
- Layout: Grid of square tiles
- Tile IDs: Assigned left-to-right, top-to-bottom (row-major order)

**Example Layout (16x16 tiles):**

```
Row 0: [0] Floor    [1] Wall     [2] Door     [3] ...
Row 1: [16] Unused  [17] Unused  [18] Unused  [19] ...
```

**Standard Tile ID Mappings:**

| Tile ID | Game Element | Typical Glyph |
|---------|--------------|---------------|
| 0       | Floor        | `.`           |
| 1       | Wall         | `#`           |
| 2       | Door         | `+`           |
| 10      | Player       | `@`           |
| 11      | Enemy        | `g`, `o`      |
| 20      | Item         | `i`           |

**Creating Your Tileset:**

1. Use any image editor (Aseprite, GIMP, Photoshop, etc.)
2. Create a canvas sized: `(TileSize × TilesPerRow) × (TileSize × TilesPerColumn)`
3. Draw each tile in its grid position
4. Export as PNG (24-bit or 32-bit)
5. Save to `assets/tiles.png`

**Example Tool:** Aseprite is excellent for pixel art tilesets.

### 3. Verification

Check logs on startup to verify Kitty graphics detection:

```
[INFO] Terminal capabilities detected: Kitty graphics supported
[INFO] Renderer supports image mode: True, PreferHighQuality: True
[INFO] Tileset loaded for image mode rendering
```

## How It Works

### Architecture Overview

```
Game State (ECS)
    ↓
TerminalUiAdapter.BuildImageTileBuffer()
    ↓ (Glyph → Tile ID mapping)
ImageTile[,] array
    ↓
TileRasterizer.Rasterize()
    ↓ (Lookup tiles from tileset, apply tints)
RGBA pixel buffer
    ↓
KittyGraphicsProtocol.Encode()
    ↓ (Base64 + escape sequences)
Terminal Display
```

### Rendering Modes

The system automatically selects the best rendering mode:

1. **Image Mode** (Kitty graphics available + tileset loaded)
   - Uses `BuildImageTileBuffer()` 
   - Rasterizes tiles to RGBA pixels
   - Encodes with Kitty protocol
   - Displays high-quality graphics

2. **Glyph Mode** (fallback)
   - Uses `BuildGlyphBuffer()`
   - Traditional ASCII/Unicode rendering
   - Works on all terminals

### Capability Detection

Detection happens at startup via `ITerminalCapabilityDetector`:

```csharp
// Checks for:
// 1. $TERM environment variable contains "kitty"
// 2. Terminal response to Kitty graphics query
// 3. $TERM_PROGRAM == "WezTerm"
```

## Troubleshooting

### Tileset Not Loading

**Symptom:** ASCII rendering even in WezTerm

**Check:**
1. Verify tileset path in `appsettings.json`
2. Check file exists: `ls -l assets/tiles.png`
3. Review logs for error messages
4. Verify PNG is valid: `file assets/tiles.png` (should show "PNG image data")

**Solution:**
```bash
# Example: copy sample tileset
cp docs/examples/sample-tileset-16x16.png assets/tiles.png
```

### Kitty Graphics Not Detected

**Symptom:** Log shows "Kitty graphics not supported"

**Check:**
1. Verify terminal: `echo $TERM` (should be "xterm-kitty" or "wezterm")
2. Test Kitty support: `printf '\x1b_Ga=q,i=1;\x1b\\'` (should not show garbage)
3. Check WezTerm version (requires v20220101 or newer)

**Solution:**
```bash
# WezTerm: Update to latest version
# Kitty: Ensure TERM=xterm-kitty in environment
export TERM=xterm-kitty
```

### Corrupted Display

**Symptom:** Screen shows artifacts or garbage characters

**Possible Causes:**
1. Tileset dimensions mismatch (width/height not multiples of TileSize)
2. Terminal size changed during rendering
3. Buffer overflow in pixel data

**Solution:**
1. Verify tileset dimensions: `identify assets/tiles.png`
2. Restart application
3. Check logs for encoding errors

### Performance Issues

**Symptom:** Slow frame rates, stuttering

**Check:**
1. Viewport size (larger viewports = more pixels to encode)
2. TileSize (larger tiles = more data per frame)
3. System resources (CPU, RAM)

**Optimization:**
```json
{
  "Rendering": {
    "Terminal": {
      "TileSize": 8,  // Smaller tiles = faster encoding
      "PreferHighQuality": false  // Force ASCII mode for testing
    }
  }
}
```

### SSH / Remote Sessions

**Expected Behavior:** Automatically falls back to ASCII rendering

Remote sessions typically don't support Kitty graphics. The system detects this and uses glyph mode automatically.

To force Kitty mode over SSH (if your terminal supports it):
```bash
ssh -o "SetEnv TERM=xterm-kitty" user@host
```

## Performance Benchmarks

### Target Performance

| Scenario | Target | Typical |
|----------|--------|---------|
| 80x24 viewport encoding | < 33ms | ~2-3ms |
| 160x48 viewport encoding | < 50ms | ~8-10ms |
| Video playback (320x240) | > 24 FPS | ~30 FPS |

### Optimization Tips

1. **Use appropriate TileSize**: 16x16 is a good balance
2. **Minimize viewport changes**: Resizing triggers full re-render
3. **Pre-load tilesets**: Loaded once at startup, cached forever
4. **Use placement IDs**: Video playback reuses same placement for efficiency

## Advanced Topics

### Custom Tile Mappings

Modify `TerminalUiAdapter.cs` to customize tile ID assignments:

```csharp
private int MapGlyphToTileId(char glyph)
{
    return glyph switch
    {
        '.' => 0,   // Floor
        '#' => 1,   // Wall
        '+' => 2,   // Door
        '~' => 3,   // Water (custom)
        '^' => 4,   // Mountain (custom)
        _ => 0
    };
}
```

### Multiple Tilesets

Currently, one tileset per application. To support multiple:

1. Extend `TilesetLoader` to accept tileset ID
2. Store multiple tilesets in `TerminalSceneRenderer`
3. Pass tileset ID in `ImageTile` record
4. Update `TileRasterizer` to handle multi-tileset lookups

### Color Tinting

Entities can have custom colors applied as tints:

```csharp
// In ECS, set Renderable component:
renderable.Foreground = new Color(255, 100, 100); // Reddish tint

// Rendered tile will multiply RGB channels:
// tintedR = tileR * (255 / 255)
// tintedG = tileG * (100 / 255)
// tintedB = tileB * (100 / 255)
```

## Examples

### Example 1: Basic Game Scene

```
############
#..........#
#..@.......#
#.....g....#
############
```

With tileset:
- Walls render as stone texture
- Floor renders as dirt/grass
- Player `@` renders as character sprite with color tint
- Goblin `g` renders as enemy sprite with color tint

### Example 2: Video Playback

```csharp
// KittyRenderer automatically:
// 1. Converts video frame to RGBA
// 2. Encodes with Kitty protocol
// 3. Uses placement ID=1 for smooth updates
// 4. Writes to terminal

// No additional configuration needed!
```

## Logging

Enable detailed logging for troubleshooting:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "LablabBean.Rendering": "Debug",
        "LablabBean.Plugins.Rendering": "Debug"
      }
    }
  }
}
```

**Key Log Messages:**

- `Tileset loaded: {path}` - Tileset successfully loaded
- `Tileset not found at path: {path}` - File missing
- `Renderer supports image mode: {True/False}` - Capability detection result
- `Using Kitty graphics protocol` - Image mode active
- `Falling back to glyph mode` - ASCII rendering

## FAQ

**Q: Can I use Kitty graphics over SSH?**  
A: Generally no - most SSH terminals don't support it. The system automatically falls back to ASCII.

**Q: Do I need to create a tileset?**  
A: No - if tileset is missing, the application uses ASCII rendering automatically.

**Q: What happens if the terminal doesn't support Kitty?**  
A: Automatic fallback to ASCII/Unicode rendering with no errors or configuration needed.

**Q: Can I mix Kitty graphics and ASCII in the same view?**  
A: No - the entire viewport uses one mode. However, HUD/logs can remain ASCII while the world view uses Kitty.

**Q: How do I disable Kitty graphics?**  
A: Set `PreferHighQuality: false` in configuration or run on a non-Kitty terminal.

**Q: Does this work on Windows?**  
A: Yes, with WezTerm. Windows Terminal doesn't support Kitty graphics yet.

## Resources

- [Kitty Graphics Protocol Specification](https://sw.kovidgoyal.net/kitty/graphics-protocol/)
- [WezTerm Documentation](https://wezfurlong.org/wezterm/)
- [Spec-025: Kitty Graphics Implementation](../../specs/025-kitty-graphics-scene-rendering/spec.md)
- [Sample Tileset Tutorial](./creating-tilesets.md) *(TODO)*

## Support

For issues or questions:
1. Check logs with `Debug` level enabled
2. Review this troubleshooting section
3. Check SPEC-025 implementation notes
4. File a GitHub issue with logs and terminal info

---

**Version**: 1.0.0  
**Spec**: SPEC-025  
**Last Verified**: 2025-10-28
