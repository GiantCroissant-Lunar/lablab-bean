namespace LablabBean.Rendering.Contracts;

/// <summary>
/// Represents a color in a limited palette (index), with optional ARGB fallback.
/// </summary>
public readonly record struct ColorRef(byte Index, uint? Argb = null);
