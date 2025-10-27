# Rendering DTOs – Draft

**Status**: Draft
**Last Updated**: 2025-10-27

## Scope

- Minimal cross-technology DTOs for Rendering.Contracts to decouple UI from drawing tech.

Draft Types (C# pseudo)

// Represents a color in a limited palette (index), with optional ARGB fallback
public readonly record struct ColorRef(byte Index, uint? Argb = null);

// A character/glyph rendering primitive (for terminal-like renderers)
public readonly record struct Glyph(char Rune, ColorRef Foreground, ColorRef Background);

// A tile rendering primitive (for tile-based renderers)
public readonly record struct Tile(ushort TileId, ColorRef Tint, byte Flags = 0);

// A 2D buffer of tiles or glyphs (exactly one of the two is used per buffer instance)
public sealed class TileBuffer
{
    public int Width { get; }
    public int Height { get; }
    public bool IsGlyphMode { get; }
    public Glyph[,]? Glyphs { get; }
    public Tile[,]? Tiles { get; }
    public TileBuffer(int width, int height, bool glyphMode)
    {
        Width = width; Height = height; IsGlyphMode = glyphMode;
        if (glyphMode) Glyphs = new Glyph[height, width]; else Tiles = new Tile[height, width];
    }
}

// Optional palette description (for terminal color or tileset tinting)
public sealed class Palette
{
    public IReadOnlyList<uint> ArgbColors { get; }
    public Palette(IReadOnlyList<uint> argbColors) => ArgbColors = argbColors;
}

ISceneRenderer (outline)

public interface ISceneRenderer
{
    // Called once to configure target, palette, etc.
    Task InitializeAsync(Palette palette, CancellationToken ct = default);

    // Render a frame into the device (from a prepared buffer)
    Task RenderAsync(TileBuffer buffer, CancellationToken ct = default);
}

Notes

- Start with Glyph mode for Terminal and Tile mode for SadConsole.
- Keep DTOs minimal; add layers, lighting, z-order only when needed.
