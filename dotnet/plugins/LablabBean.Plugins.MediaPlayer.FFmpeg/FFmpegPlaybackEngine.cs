using System.Reactive.Linq;
using System.Reactive.Subjects;
using LablabBean.Contracts.Media;
using LablabBean.Contracts.Media.DTOs;
using OpenCvSharp;

namespace LablabBean.Plugins.MediaPlayer.FFmpeg;

/// <summary>
/// FFmpeg-based playback engine using OpenCvSharp for decoding.
/// Supports most common video and audio formats.
/// </summary>
public class FFmpegPlaybackEngine : IMediaPlaybackEngine, IDisposable
{
    private VideoCapture? _capture;
    private readonly Subject<MediaFrame> _frameStream;
    private string? _currentPath;
    private TimeSpan _duration;
    private int _frameWidth;
    private int _frameHeight;
    private double _fps;
    private bool _isOpen;

    public string Name => "FFmpeg Playback Engine";
    public int Priority => 100; // Highest priority - general purpose engine

    public IEnumerable<string> SupportedFileExtensions => new[]
    {
        // Video formats
        "mp4", "mkv", "avi", "mov", "webm", "flv", "wmv", "m4v", "mpg", "mpeg", "3gp",
        // Audio formats
        "mp3", "wav", "flac", "aac", "ogg", "m4a", "wma", "opus"
    };

    public IObservable<MediaFrame> FrameStream => _frameStream.AsObservable();

    public FFmpegPlaybackEngine()
    {
        _frameStream = new Subject<MediaFrame>();
    }

    public Task<MediaInfo> OpenAsync(string path, CancellationToken ct = default)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Media file not found: {path}", path);

        try
        {
            _capture = new VideoCapture(path);

            if (!_capture.IsOpened())
                throw new InvalidDataException($"Failed to open media file: {path}");

            _currentPath = path;
            _isOpen = true;

            // Extract metadata
            _frameWidth = (int)_capture.Get(VideoCaptureProperties.FrameWidth);
            _frameHeight = (int)_capture.Get(VideoCaptureProperties.FrameHeight);
            _fps = _capture.Get(VideoCaptureProperties.Fps);
            var frameCount = _capture.Get(VideoCaptureProperties.FrameCount);

            // Calculate duration (may be 0 for audio-only or live streams)
            _duration = _fps > 0 && frameCount > 0
                ? TimeSpan.FromSeconds(frameCount / _fps)
                : TimeSpan.Zero;

            // Determine format
            var extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            var audioFormats = new[] { "mp3", "wav", "flac", "aac", "ogg", "m4a", "wma", "opus" };
            var isAudioOnly = audioFormats.Contains(extension);

            var format = isAudioOnly ? MediaFormat.Audio :
                         _frameWidth > 0 ? MediaFormat.Video : MediaFormat.Audio;

            // Create media info
            VideoInfo? videoInfo = null;
            if (format == MediaFormat.Video && _frameWidth > 0)
            {
                videoInfo = new VideoInfo(
                    Width: _frameWidth,
                    Height: _frameHeight,
                    FrameRate: _fps,
                    Codec: "Unknown", // OpenCV doesn't provide easy codec detection
                    BitRate: 0
                );
            }

            AudioInfo? audioInfo = null;
            if (format == MediaFormat.Audio || format == MediaFormat.Both)
            {
                audioInfo = new AudioInfo(
                    SampleRate: 44100, // Default assumption
                    Channels: 2,
                    Codec: "Unknown",
                    BitRate: 0
                );
            }

            var metadata = new Dictionary<string, string>
            {
                ["FileName"] = Path.GetFileName(path),
                ["FileSize"] = new FileInfo(path).Length.ToString()
            };

            return Task.FromResult(new MediaInfo(
                Path: path,
                Format: format,
                Duration: _duration,
                Video: videoInfo,
                Audio: audioInfo,
                Metadata: metadata
            ));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open media file: {ex.Message}", ex);
        }
    }

    public Task<MediaFrame> DecodeNextFrameAsync(CancellationToken ct = default)
    {
        if (!_isOpen || _capture == null)
            throw new InvalidOperationException("No media file is open. Call OpenAsync first.");

        ct.ThrowIfCancellationRequested();

        using var mat = new Mat();
        var success = _capture.Read(mat);

        if (!success || mat.Empty())
            throw new EndOfStreamException("Reached end of media stream");

        // Get current timestamp
        var positionMs = _capture.Get(VideoCaptureProperties.PosMsec);
        var timestamp = TimeSpan.FromMilliseconds(positionMs);

        // Convert BGR (OpenCV default) to RGB
        using var rgbMat = new Mat();
        Cv2.CvtColor(mat, rgbMat, ColorConversionCodes.BGR2RGB);

        // Convert to byte array
        var data = new byte[rgbMat.Total() * rgbMat.ElemSize()];
        System.Runtime.InteropServices.Marshal.Copy(rgbMat.Data, data, 0, data.Length);

        var frame = new MediaFrame(
            Data: data,
            Timestamp: timestamp,
            Type: FrameType.Video,
            Width: rgbMat.Width,
            Height: rgbMat.Height,
            PixelFormat: PixelFormat.RGB24
        );

        // Publish to frame stream
        _frameStream.OnNext(frame);

        return Task.FromResult(frame);
    }

    public Task<TimeSpan> SeekAsync(TimeSpan position, CancellationToken ct = default)
    {
        if (!_isOpen || _capture == null)
            throw new InvalidOperationException("No media file is open");

        if (position < TimeSpan.Zero || position > _duration)
            throw new ArgumentOutOfRangeException(nameof(position),
                $"Position must be between 0 and {_duration}");

        try
        {
            // Set position in milliseconds
            _capture.Set(VideoCaptureProperties.PosMsec, position.TotalMilliseconds);

            // Get actual position after seek
            var actualPositionMs = _capture.Get(VideoCaptureProperties.PosMsec);
            var actualPosition = TimeSpan.FromMilliseconds(actualPositionMs);

            return Task.FromResult(actualPosition);
        }
        catch (Exception ex)
        {
            throw new NotSupportedException($"Seeking not supported for this media: {ex.Message}", ex);
        }
    }

    public Task CloseAsync(CancellationToken ct = default)
    {
        if (_capture != null)
        {
            _capture.Release();
            _capture.Dispose();
            _capture = null;
        }

        _isOpen = false;
        _currentPath = null;
        _frameStream.OnCompleted();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        CloseAsync().GetAwaiter().GetResult();
        _frameStream.Dispose();
    }
}
