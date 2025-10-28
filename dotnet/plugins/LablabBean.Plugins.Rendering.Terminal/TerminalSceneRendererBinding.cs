using LablabBean.Rendering.Terminal.Contracts;
using Microsoft.Extensions.Logging;
using TGui = global::Terminal.Gui;

namespace LablabBean.Plugins.Rendering.Terminal;

/// <summary>
/// Binding implementation that connects TerminalSceneRenderer to the UI plugin.
/// </summary>
public class TerminalSceneRendererBinding : ITerminalRenderBinding
{
    private readonly TerminalSceneRenderer _renderer;
    private readonly ILogger<TerminalSceneRendererBinding> _logger;

    public TerminalSceneRendererBinding(TerminalSceneRenderer renderer, ILogger<TerminalSceneRendererBinding> logger)
    {
        _renderer = renderer;
        _logger = logger;
    }

    public void SetRenderTarget(TGui.View view)
    {
        _renderer.SetRenderTarget(view);
        _logger.LogInformation("Terminal render target set via binding interface");
    }

    public void Rebind()
    {
        _logger.LogDebug("Terminal renderer rebind requested");
    }
}
