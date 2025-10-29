namespace LablabBean.Rendering.Contracts;

/// <summary>
/// Represents a tile in image mode with tile ID, optional tint color, and alpha.
/// </summary>
public record ImageTile(int TileId, uint? TintColor, byte Alpha);
