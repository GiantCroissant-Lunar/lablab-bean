using LablabBean.Contracts.Media;
using LablabBean.Contracts.Media.DTOs;
using LablabBean.Rendering.Terminal.Kitty;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LablabBean.Plugins.MediaPlayer.Terminal.Kitty;

/// <summary>
/// Renderer implementation using Kitty graphics protocol
/// </summary>
public class KittyRenderer : IMediaRenderer
{
    private readonly ILogger<KittyRenderer>? _logger;
    private readonly KittyGraphicsProtocol _kittyProtocol;

    public string Name => "Kitty Graphics Protocol";
    public int Priority => 90;

    public IEnumerable<MediaFormat> SupportedFormats => new[]
    {
        MediaFormat.Video,
        MediaFormat.Both
    };

    public IEnumerable<TerminalCapability> RequiredCapabilities => new[]
    {
        TerminalCapability.KittyGraphics
    };

    private bool _isInitialized;

    public KittyRenderer(ILogger<KittyRenderer>? logger = null)
    {
        _logger = logger;
        _kittyProtocol = new KittyGraphicsProtocol();
    }

    public Task<bool> CanRenderAsync(MediaInfo media, TerminalInfo terminal, CancellationToken ct = default)
    {
        // Check if terminal supports Kitty graphics protocol
        var supportsKitty = terminal.Capabilities.HasFlag(TerminalCapability.KittyGraphics);

        // Check if media has video
        var hasVideo = media.Format == MediaFormat.Video || media.Format == MediaFormat.Both;

        return Task.FromResult(supportsKitty && hasVideo);
    }

    public Task InitializeAsync(RenderContext context, CancellationToken ct = default)
    {
        _isInitialized = true;
        return Task.CompletedTask;
    }

    public Task RenderFrameAsync(MediaFrame frame, CancellationToken ct = default)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Renderer not initialized. Call InitializeAsync first.");

        if (frame.Type != FrameType.Video)
            return Task.CompletedTask; // Skip audio frames

        try
        {
            // T071: Extract RGBA pixels from MediaFrame.Data
            byte[] pixelData = ConvertToRGBA(frame);

            // T072, T073: Encode via Kitty protocol with placement ID for animation
            var options = new KittyOptions
            {
                PlacementId = 1, // Reuse same ID for video updates (T073)
                TransmissionMode = 'd', // Direct transmission
                ChunkedTransmission = false
            };

            string escapeSequence = _kittyProtocol.Encode(pixelData, frame.Width, frame.Height, options);

            // T074: Write escape sequence to terminal
            Console.Write(escapeSequence);
            Console.Out.Flush();

            _logger?.LogTrace("Rendered frame: {Width}x{Height}, timestamp: {Timestamp}",
                frame.Width, frame.Height, frame.Timestamp);
        }
        catch (Exception ex)
        {
            // T075: Error handling - log and skip frame
            _logger?.LogError(ex, "Failed to render video frame at {Timestamp}, skipping", frame.Timestamp);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Converts MediaFrame data to RGBA format if needed.
    /// </summary>
    private byte[] ConvertToRGBA(MediaFrame frame)
    {
        // If already RGBA, return as-is
        if (frame.PixelFormat == PixelFormat.RGBA32)
        {
            return frame.Data;
        }

        // If RGB24, convert to RGBA32 by adding alpha channel
        if (frame.PixelFormat == PixelFormat.RGB24)
        {
            int pixelCount = frame.Width * frame.Height;
            byte[] rgba = new byte[pixelCount * 4];

            for (int i = 0; i < pixelCount; i++)
            {
                int srcIdx = i * 3;
                int dstIdx = i * 4;
                rgba[dstIdx] = frame.Data[srcIdx];     // R
                rgba[dstIdx + 1] = frame.Data[srcIdx + 1]; // G
                rgba[dstIdx + 2] = frame.Data[srcIdx + 2]; // B
                rgba[dstIdx + 3] = 255; // Alpha = opaque
            }

            return rgba;
        }

        // Unsupported format
        throw new NotSupportedException($"Pixel format {frame.PixelFormat} is not supported");
    }

    public Task CleanupAsync(CancellationToken ct = default)
    {
        _isInitialized = false;
        return Task.CompletedTask;
    }
}
