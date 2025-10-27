namespace LablabBean.Rendering.Contracts;

/// <summary>
/// A 2D buffer of tiles or glyphs (exactly one of the two is used per buffer instance).
/// </summary>
public sealed class TileBuffer
{
    public int Width { get; }
    public int Height { get; }
    public bool IsGlyphMode { get; }
    public Glyph[,]? Glyphs { get; }
    public Tile[,]? Tiles { get; }

    public TileBuffer(int width, int height, bool glyphMode)
    {
        Width = width;
        Height = height;
        IsGlyphMode = glyphMode;
        if (glyphMode)
            Glyphs = new Glyph[height, width];
        else
            Tiles = new Tile[height, width];
    }
}
