namespace LablabBean.Rendering.Contracts;

/// <summary>
/// A 2D buffer of tiles or glyphs for rendering.
///
/// <para>In image mode:</para>
/// <list type="bullet">
/// <item>Width/Height are in PIXEL coordinates (e.g., 160x160 pixels)</item>
/// <item>PixelData contains RGBA bytes (Width * Height * 4 bytes)</item>
/// <item>ImageTiles may contain tile-space data (optional)</item>
/// </list>
///
/// <para>In glyph/tile mode:</para>
/// <list type="bullet">
/// <item>Width/Height are in CHARACTER/TILE coordinates (e.g., 10x10 tiles)</item>
/// <item>Glyphs or Tiles arrays are populated</item>
/// </list>
/// </summary>
public sealed class TileBuffer
{
    public int Width { get; }
    public int Height { get; }
    public bool IsGlyphMode { get; }
    public bool IsImageMode { get; }
    public Glyph[,]? Glyphs { get; }
    public Tile[,]? Tiles { get; }
    public byte[]? PixelData { get; set; }
    public ImageTile[,]? ImageTiles { get; set; }

    /// <summary>
    /// Creates a glyph or tile mode buffer.
    /// </summary>
    /// <param name="width">Width in characters/tiles.</param>
    /// <param name="height">Height in characters/tiles.</param>
    /// <param name="glyphMode">True for glyph mode, false for tile mode.</param>
    public TileBuffer(int width, int height, bool glyphMode)
    {
        Width = width;
        Height = height;
        IsGlyphMode = glyphMode;
        IsImageMode = false;
        if (glyphMode)
            Glyphs = new Glyph[height, width];
        else
            Tiles = new Tile[height, width];
    }

    /// <summary>
    /// Creates an image mode buffer for Kitty graphics rendering.
    /// </summary>
    /// <param name="widthInPixels">Width in pixels (not tiles).</param>
    /// <param name="heightInPixels">Height in pixels (not tiles).</param>
    /// <returns>TileBuffer configured for image mode.</returns>
    public static TileBuffer CreateImageBuffer(int widthInPixels, int heightInPixels)
    {
        return new TileBuffer(widthInPixels, heightInPixels, TileBufferMode.Image);
    }

    /// <summary>
    /// Internal constructor for image mode (use CreateImageBuffer factory method).
    /// </summary>
    private TileBuffer(int width, int height, TileBufferMode mode)
    {
        Width = width;
        Height = height;
        IsGlyphMode = mode == TileBufferMode.Glyph;
        IsImageMode = mode == TileBufferMode.Image;

        if (mode == TileBufferMode.Image)
        {
            // PixelData will be set externally after rasterization
            ImageTiles = null; // Optional, usually not needed
        }
    }
}

/// <summary>
/// Rendering mode for TileBuffer.
/// </summary>
public enum TileBufferMode
{
    Glyph,
    Tile,
    Image
}
