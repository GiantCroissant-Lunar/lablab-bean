using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LablabBean.Rendering.Contracts;

/// <summary>
/// Loads PNG tilesets and parses them into individual tile sprites.
/// </summary>
public class TilesetLoader
{
    private readonly ILogger<TilesetLoader> _logger;

    public TilesetLoader(ILogger<TilesetLoader> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Loads a tileset from a PNG file and extracts individual tiles.
    /// </summary>
    /// <param name="path">Path to the PNG file.</param>
    /// <param name="tileSize">Size of each tile in pixels (tiles are square).</param>
    /// <returns>Loaded tileset, or null if loading failed.</returns>
    public Tileset? Load(string path, int tileSize)
    {
        if (!File.Exists(path))
        {
            _logger.LogWarning("Tileset not found at path: {Path}, falling back to glyph mode", path);
            return null;
        }

        try
        {
            _logger.LogInformation("Loading tileset from: {Path} (tile size: {TileSize}px)", path, tileSize);

            using var image = Image.Load<Rgba32>(path);

            int tilesPerRow = image.Width / tileSize;
            int tilesPerColumn = image.Height / tileSize;

            if (tilesPerRow == 0 || tilesPerColumn == 0)
            {
                _logger.LogError("Tileset image too small: {Width}x{Height} with tile size {TileSize}",
                    image.Width, image.Height, tileSize);
                return null;
            }

            var tiles = new Dictionary<int, byte[]>();
            int tileId = 0;

            // Extract each tile from the grid
            for (int row = 0; row < tilesPerColumn; row++)
            {
                for (int col = 0; col < tilesPerRow; col++)
                {
                    var tilePixels = ExtractTile(image, col, row, tileSize);
                    tiles[tileId] = tilePixels;
                    tileId++;
                }
            }

            _logger.LogInformation("Tileset loaded successfully: {TileCount} tiles ({Rows}x{Cols})",
                tiles.Count, tilesPerRow, tilesPerColumn);

            return new Tileset(path, tileSize, tilesPerRow, tilesPerColumn, tiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load tileset from {Path}, falling back to glyph mode", path);
            return null;
        }
    }

    private byte[] ExtractTile(Image<Rgba32> image, int col, int row, int tileSize)
    {
        var pixels = new byte[tileSize * tileSize * 4]; // RGBA
        int pixelIndex = 0;

        int startX = col * tileSize;
        int startY = row * tileSize;

        for (int y = 0; y < tileSize; y++)
        {
            for (int x = 0; x < tileSize; x++)
            {
                var pixel = image[startX + x, startY + y];
                pixels[pixelIndex++] = pixel.R;
                pixels[pixelIndex++] = pixel.G;
                pixels[pixelIndex++] = pixel.B;
                pixels[pixelIndex++] = pixel.A;
            }
        }

        return pixels;
    }
}
