using Microsoft.Extensions.Logging;

namespace LablabBean.Rendering.Contracts;

/// <summary>
/// Composites individual tiles from a tileset into a single RGBA pixel buffer.
/// </summary>
public class TileRasterizer
{
    private readonly ILogger<TileRasterizer> _logger;

    public TileRasterizer(ILogger<TileRasterizer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Rasterizes an ImageTile array into a single RGBA pixel buffer.
    /// </summary>
    /// <param name="tiles">2D array of ImageTiles to render.</param>
    /// <param name="tileset">Tileset containing tile sprites.</param>
    /// <returns>RGBA pixel buffer, or null if rasterization failed.</returns>
    public byte[]? Rasterize(ImageTile[,] tiles, Tileset tileset)
    {
        if (tiles == null || tileset == null)
        {
            _logger.LogWarning("Cannot rasterize: tiles or tileset is null");
            return null;
        }

        int tilesHeight = tiles.GetLength(0);
        int tilesWidth = tiles.GetLength(1);
        int pixelWidth = tilesWidth * tileset.TileSize;
        int pixelHeight = tilesHeight * tileset.TileSize;

        var buffer = new byte[pixelWidth * pixelHeight * 4]; // RGBA

        try
        {
            for (int tileY = 0; tileY < tilesHeight; tileY++)
            {
                for (int tileX = 0; tileX < tilesWidth; tileX++)
                {
                    var tile = tiles[tileY, tileX];
                    var tilePixels = tileset.GetTile(tile.TileId);

                    if (tilePixels == null)
                    {
                        _logger.LogTrace("Tile ID {TileId} not found in tileset, skipping", tile.TileId);
                        continue;
                    }

                    CompositeTile(buffer, tilePixels, tileX, tileY, tileset.TileSize, pixelWidth, tile);
                }
            }

            _logger.LogTrace("Rasterized {Width}x{Height} tiles into {PixelWidth}x{PixelHeight} buffer",
                tilesWidth, tilesHeight, pixelWidth, pixelHeight);

            return buffer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rasterize tiles");
            return null;
        }
    }

    private void CompositeTile(byte[] buffer, byte[] tilePixels, int tileX, int tileY, int tileSize, int bufferWidth, ImageTile tile)
    {
        int startX = tileX * tileSize;
        int startY = tileY * tileSize;
        bool hasTint = tile.TintColor.HasValue;
        var tint = tile.TintColor ?? 0xFFFFFFFF;

        // Extract tint RGB components (ignore alpha in tint)
        float tintR = ((tint >> 16) & 0xFF) / 255f;
        float tintG = ((tint >> 8) & 0xFF) / 255f;
        float tintB = (tint & 0xFF) / 255f;
        float alpha = tile.Alpha;

        int srcIndex = 0;

        for (int y = 0; y < tileSize; y++)
        {
            for (int x = 0; x < tileSize; x++)
            {
                int destX = startX + x;
                int destY = startY + y;
                int destIndex = (destY * bufferWidth + destX) * 4;

                byte r = tilePixels[srcIndex++];
                byte g = tilePixels[srcIndex++];
                byte b = tilePixels[srcIndex++];
                byte a = tilePixels[srcIndex++];

                // Apply tint color if specified
                if (hasTint)
                {
                    r = (byte)(r * tintR);
                    g = (byte)(g * tintG);
                    b = (byte)(b * tintB);
                }

                // Apply alpha
                if (alpha < 1.0f)
                {
                    a = (byte)(a * alpha);
                }

                buffer[destIndex] = r;
                buffer[destIndex + 1] = g;
                buffer[destIndex + 2] = b;
                buffer[destIndex + 3] = a;
            }
        }
    }
}
