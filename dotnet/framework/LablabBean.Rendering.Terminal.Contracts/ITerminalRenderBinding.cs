namespace LablabBean.Rendering.Terminal.Contracts;

/// <summary>
/// Contract for terminal rendering binding between UI and renderer plugins.
/// Implemented by the renderer plugin and consumed by the UI plugin.
/// </summary>
public interface ITerminalRenderBinding
{
    /// <summary>
    /// Sets the render target view for the terminal renderer.
    /// </summary>
    /// <param name="view">The Terminal.Gui view to render into.</param>
    void SetRenderTarget(global::Terminal.Gui.View view);

    /// <summary>
    /// Rebinds the renderer to refresh the connection.
    /// </summary>
    void Rebind();
}
