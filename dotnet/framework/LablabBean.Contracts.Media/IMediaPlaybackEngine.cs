using LablabBean.Contracts.Media.DTOs;

namespace LablabBean.Contracts.Media;

/// <summary>
/// Interface for media playback engines that decode audio/video files into raw frames.
/// Typical implementations use FFmpeg, OpenCV, NAudio, or similar decoding libraries.
/// </summary>
/// <remarks>
/// The playback engine is responsible for:
/// - Opening media files and extracting metadata
/// - Decoding compressed audio/video streams into raw data
/// - Seeking to specific positions
/// - Frame-rate pacing (delivering frames at correct timing)
/// - Publishing frames via observable stream
///
/// Example Implementation (FFmpeg via OpenCvSharp):
/// <code>
/// public class FFmpegPlaybackEngine : IMediaPlaybackEngine
/// {
///     private VideoCapture? _capture;
///     private readonly Subject&lt;MediaFrame&gt; _frameStream = new();
///
///     public IObservable&lt;MediaFrame&gt; FrameStream => _frameStream.AsObservable();
///
///     public async Task&lt;MediaInfo&gt; OpenAsync(string path, CancellationToken ct)
///     {
///         _capture = new VideoCapture(path);
///         int width = (int)_capture.Get(VideoCaptureProperties.FrameWidth);
///         int height = (int)_capture.Get(VideoCaptureProperties.FrameHeight);
///         double fps = _capture.Get(VideoCaptureProperties.Fps);
///         // ... extract metadata
///         return new MediaInfo(path, MediaFormat.Video, duration, videoInfo, null);
///     }
///
///     public async Task&lt;MediaFrame&gt; DecodeNextFrameAsync(CancellationToken ct)
///     {
///         using var mat = new Mat();
///         _capture.Read(mat);
///         byte[] data = MatToByteArray(mat);
///         return new MediaFrame(data, GetTimestamp(), FrameType.Video, mat.Width, mat.Height, PixelFormat.RGB24);
///     }
/// }
/// </code>
/// </remarks>
public interface IMediaPlaybackEngine
{
    /// <summary>
    /// Human-readable engine name for logging and diagnostics
    /// </summary>
    /// <example>"FFmpeg Engine", "NAudio Engine", "OpenCV Engine"</example>
    string Name { get; }

    /// <summary>
    /// Engine priority for automatic selection (higher values preferred)
    /// </summary>
    /// <remarks>
    /// Use different priorities for specialized engines:
    /// - 100: General-purpose (FFmpeg) - handles all formats
    /// - 50: Specialized (NAudio) - audio-only, optimized for visualization
    /// </remarks>
    int Priority { get; }

    /// <summary>
    /// File extensions supported by this engine (without dot, e.g., "mp4", "mkv")
    /// </summary>
    /// <remarks>
    /// Used for engine selection based on file type.
    /// Common values: ["mp4", "mkv", "avi", "mov", "webm", "mp3", "flac", "wav"]
    /// </remarks>
    IEnumerable<string> SupportedFileExtensions { get; }

    /// <summary>
    /// Open a media file and extract metadata
    /// </summary>
    /// <param name="path">Absolute path to media file</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Media metadata including duration, format, codec info</returns>
    /// <exception cref="FileNotFoundException">File does not exist</exception>
    /// <exception cref="NotSupportedException">File format or codec not supported</exception>
    /// <exception cref="InvalidDataException">File is corrupted or invalid</exception>
    /// <remarks>
    /// This method:
    /// - Opens the file container (MP4, MKV, etc.)
    /// - Probes streams to detect codecs
    /// - Extracts duration, resolution, frame rate
    /// - Validates that required codecs are available
    ///
    /// Does NOT start decoding frames. Call DecodeNextFrameAsync() to decode.
    /// Should complete within 100-200ms for local files.
    /// </remarks>
    Task<MediaInfo> OpenAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Decode the next frame from the media stream
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Decoded frame with raw pixel/audio data</returns>
    /// <exception cref="EndOfStreamException">Reached end of media</exception>
    /// <exception cref="InvalidOperationException">No file is open (call OpenAsync first)</exception>
    /// <remarks>
    /// Returns next frame in sequence. Frames are returned in presentation order.
    ///
    /// Frame data format:
    /// - Video: RGB24 or BGR24 pixel array (width × height × 3 bytes)
    /// - Audio: PCM16 sample array (sample_count × channels × 2 bytes)
    ///
    /// Performance:
    /// - Video decoding: 1-10ms depending on resolution and codec
    /// - Audio decoding: < 1ms for typical chunk sizes
    ///
    /// Memory:
    /// - Caller owns returned byte array
    /// - Consider using ArrayPool for frame buffers
    ///
    /// Thread safety:
    /// - Safe to call from any thread
    /// - Only one thread should call DecodeNextFrameAsync at a time
    /// </remarks>
    Task<MediaFrame> DecodeNextFrameAsync(CancellationToken ct = default);

    /// <summary>
    /// Seek to a specific position in the media
    /// </summary>
    /// <param name="position">Target position (must be >= 0 and <= duration)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Actual position after seek (may differ due to keyframe alignment)</returns>
    /// <exception cref="ArgumentOutOfRangeException">Position is negative or beyond duration</exception>
    /// <exception cref="NotSupportedException">Seeking not supported for this media</exception>
    /// <remarks>
    /// Seeks to the nearest keyframe at or before the requested position.
    /// For video files, actual position may differ by 1-2 seconds depending on keyframe interval.
    ///
    /// After seeking, DecodeNextFrameAsync() will return frames starting from the new position.
    ///
    /// Performance:
    /// - Fast seek (keyframe): 10-50ms
    /// - Slow seek (frame-accurate): 100-500ms (not recommended for video)
    ///
    /// Some formats (e.g., FLV, some AVI files) may not support seeking.
    /// </remarks>
    Task<TimeSpan> SeekAsync(TimeSpan position, CancellationToken ct = default);

    /// <summary>
    /// Close the media file and release resources
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <remarks>
    /// Closes file handles, releases codec resources, frees frame buffers.
    /// Safe to call multiple times (idempotent).
    /// After closing, must call OpenAsync() to open a new file.
    /// </remarks>
    Task CloseAsync(CancellationToken ct = default);

    /// <summary>
    /// Observable stream of decoded frames
    /// </summary>
    /// <remarks>
    /// Alternative to DecodeNextFrameAsync() for reactive push-based model.
    /// Engine publishes frames to this stream as they are decoded.
    ///
    /// Usage:
    /// <code>
    /// engine.FrameStream.Subscribe(async frame => {
    ///     await renderer.RenderFrameAsync(frame, ct);
    /// });
    /// </code>
    ///
    /// Stream completes when:
    /// - End of media reached
    /// - CloseAsync() called
    /// - Fatal error occurs
    ///
    /// Optional: Engines may choose not to implement this (return Observable.Empty).
    /// If implemented, provides better backpressure control and reactive composition.
    /// </remarks>
    IObservable<MediaFrame> FrameStream { get; }
}
