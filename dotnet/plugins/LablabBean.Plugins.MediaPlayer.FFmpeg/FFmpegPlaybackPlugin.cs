using LablabBean.Contracts.Media;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.MediaPlayer.FFmpeg;

/// <summary>
/// Plugin registration for FFmpeg playback engine
/// </summary>
public class FFmpegPlaybackPlugin : IPlugin
{
    private ILogger? _logger;

    public string Id => "media-player-ffmpeg";
    public string Name => "FFmpeg Playback Engine";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _logger.LogInformation("Initializing FFmpeg playback engine plugin");

        // Register FFmpeg playback engine
        var engine = new FFmpegPlaybackEngine();
        context.Registry.Register<IMediaPlaybackEngine>(engine, priority: 100);

        _logger.LogInformation("FFmpeg playback engine registered successfully");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("FFmpeg playback engine plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("FFmpeg playback engine plugin stopped");
        return Task.CompletedTask;
    }
}
