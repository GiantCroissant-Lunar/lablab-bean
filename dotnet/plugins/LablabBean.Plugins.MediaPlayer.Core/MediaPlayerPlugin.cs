using LablabBean.Contracts.Media;
using LablabBean.Plugins.MediaPlayer.Core.Detectors;
using LablabBean.Plugins.MediaPlayer.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.Plugins.MediaPlayer.Core;

/// <summary>
/// Plugin registration for core media player services
/// </summary>
public class MediaPlayerPlugin
{
    public static void RegisterServices(IServiceCollection services)
    {
        // Register terminal capability detector
        services.AddSingleton<ITerminalCapabilityDetector, TerminalCapabilityDetector>();

        // Register MediaService as singleton
        services.AddSingleton<IMediaService, MediaService>();
    }
}
