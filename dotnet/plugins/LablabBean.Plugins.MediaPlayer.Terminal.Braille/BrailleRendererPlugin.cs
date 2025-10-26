using LablabBean.Contracts.Media;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.Plugins.MediaPlayer.Terminal.Braille;

/// <summary>
/// Plugin registration for Braille renderer
/// </summary>
public class BrailleRendererPlugin
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMediaRenderer, BrailleRenderer>();
    }
}
