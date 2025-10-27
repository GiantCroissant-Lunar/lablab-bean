namespace LablabBean.Plugins.Contracts;

/// <summary>
/// Lightweight service to (re)bind a renderer to its UI surface.
/// UI plugins can register an implementation so hosts can trigger a rebind without
/// referencing plugin instances directly.
/// </summary>
public interface IRenderTargetBinder
{
    /// <summary>
    /// Identifier of the UI this binder belongs to (e.g., "ui-terminal", "ui-sadconsole").
    /// </summary>
    string UiId { get; }

    /// <summary>
    /// Perform (re)binding of the renderer target to the current UI surface.
    /// </summary>
    void Rebind();
}
