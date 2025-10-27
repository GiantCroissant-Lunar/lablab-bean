namespace LablabBean.Rendering.Contracts;

/// <summary>
/// A character/glyph rendering primitive (for terminal-like renderers).
/// </summary>
public readonly record struct Glyph(char Rune, ColorRef Foreground, ColorRef Background);
