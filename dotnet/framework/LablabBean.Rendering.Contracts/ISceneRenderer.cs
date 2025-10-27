namespace LablabBean.Rendering.Contracts;

/// <summary>
/// Platform-agnostic scene renderer interface.
/// Implementations handle low-level drawing to specific rendering technologies (Terminal.Gui, SadConsole, Unity, etc.).
/// </summary>
public interface ISceneRenderer
{
    /// <summary>
    /// Called once to configure target, palette, etc.
    /// </summary>
    Task InitializeAsync(Palette palette, CancellationToken ct = default);

    /// <summary>
    /// Render a frame into the device (from a prepared buffer).
    /// </summary>
    Task RenderAsync(TileBuffer buffer, CancellationToken ct = default);
}
