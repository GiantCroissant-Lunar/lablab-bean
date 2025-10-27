namespace LablabBean.Rendering.Contracts;

/// <summary>
/// A tile rendering primitive (for tile-based renderers).
/// </summary>
public readonly record struct Tile(ushort TileId, ColorRef Tint, byte Flags = 0);
