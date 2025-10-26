using LablabBean.Contracts.Media;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.Plugins.MediaPlayer.FFmpeg;

/// <summary>
/// Plugin registration for FFmpeg playback engine
/// </summary>
public class FFmpegPlaybackPlugin
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMediaPlaybackEngine, FFmpegPlaybackEngine>();
    }
}
