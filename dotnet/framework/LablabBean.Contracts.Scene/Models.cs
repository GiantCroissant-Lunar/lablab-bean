namespace LablabBean.Contracts.Scene;

/// <summary>
/// Camera position and zoom level.
/// </summary>
/// <param name="Position">Camera position in world coordinates.</param>
/// <param name="Zoom">Zoom level (1.0 = normal, >1.0 = zoomed in, <1.0 = zoomed out).</param>
public record Camera(Position Position, float Zoom = 1.0f);

/// <summary>
/// Viewport dimensions for rendering.
/// </summary>
/// <param name="Width">Viewport width in tiles or pixels.</param>
/// <param name="Height">Viewport height in tiles or pixels.</param>
public record Viewport(int Width, int Height);

/// <summary>
/// Camera viewport combining camera and viewport.
/// </summary>
/// <param name="Camera">Camera position and zoom.</param>
/// <param name="Viewport">Viewport dimensions.</param>
public record CameraViewport(Camera Camera, Viewport Viewport);

/// <summary>
/// Position in 2D space.
/// </summary>
/// <param name="X">X coordinate.</param>
/// <param name="Y">Y coordinate.</param>
public record Position(int X, int Y);

/// <summary>
/// Snapshot of an entity for rendering.
/// </summary>
/// <param name="EntityId">Unique entity identifier.</param>
/// <param name="Position">Entity position.</param>
/// <param name="Glyph">Character to render.</param>
/// <param name="ForegroundColor">Foreground color (RGB hex, e.g., "#FFFFFF").</param>
/// <param name="BackgroundColor">Background color (RGB hex, e.g., "#000000").</param>
public record EntitySnapshot(
    Guid EntityId,
    Position Position,
    char Glyph,
    string ForegroundColor,
    string BackgroundColor
);
