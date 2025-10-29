namespace LablabBean.Rendering.Contracts;

/// <summary>
/// Represents a loaded tileset with individual tile sprites cached for rendering.
/// </summary>
public class Tileset
{
    /// <summary>
    /// Gets the tile size in pixels (tiles are square).
    /// </summary>
    public int TileSize { get; }

    /// <summary>
    /// Gets the number of tiles per row in the source image.
    /// </summary>
    public int TilesPerRow { get; }

    /// <summary>
    /// Gets the number of tiles per column in the source image.
    /// </summary>
    public int TilesPerColumn { get; }

    /// <summary>
    /// Gets the cached tile sprites: tileId → RGBA pixel data.
    /// </summary>
    public IReadOnlyDictionary<int, byte[]> Tiles { get; }

    /// <summary>
    /// Gets the source file path of the tileset.
    /// </summary>
    public string SourcePath { get; }

    public Tileset(string sourcePath, int tileSize, int tilesPerRow, int tilesPerColumn, Dictionary<int, byte[]> tiles)
    {
        SourcePath = sourcePath;
        TileSize = tileSize;
        TilesPerRow = tilesPerRow;
        TilesPerColumn = tilesPerColumn;
        Tiles = tiles;
    }

    /// <summary>
    /// Gets a tile sprite by ID. Returns null if not found.
    /// </summary>
    public byte[]? GetTile(int tileId)
    {
        return Tiles.TryGetValue(tileId, out var tile) ? tile : null;
    }
}
