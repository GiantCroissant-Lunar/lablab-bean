namespace LablabBean.Contracts.Media.DTOs;

/// <summary>
/// Rendering context information
/// </summary>
/// <param name="TargetView">Target Terminal.Gui view for rendering</param>
/// <param name="ViewportSize">Rendering viewport dimensions</param>
/// <param name="TerminalInfo">Terminal capability metadata</param>
public record RenderContext(
    object TargetView,
    (int Width, int Height) ViewportSize,
    IReadOnlyDictionary<string, object> TerminalInfo
);
