---
title: Unified Media Player - Technical Research Report
type: research
status: complete
created: 2025-10-26
updated: 2025-10-26
author: Claude
tags:
  - media-player
  - ffmpeg
  - reactiveui
  - terminal-gui
  - audio-visualization
  - research
related:
  - UNIFIED_MEDIA_PLAYER_PLAN.md
  - LablabBean.Plugins.MediaPlayer.Core
  - LablabBean.Reactive
priority: high
---

## Unified Media Player - Technical Research Report

## Executive Summary

This document provides comprehensive technical research for implementing a unified audio/video media player for the Lablab-Bean project. The research covers three critical areas:

1. **FFmpeg Integration via OpenCvSharp** - Frame decoding, audio extraction, seeking, and memory management
2. **ReactiveUI Integration with Terminal.Gui** - MVVM patterns, property binding, and thread-safe UI updates
3. **Audio Visualization Algorithms** - FFT implementation, spectrum analysis, and terminal rendering

This research was conducted to inform the implementation plan outlined in `UNIFIED_MEDIA_PLAYER_PLAN.md`.

---

## Table of Contents

1. [FFmpeg Integration via OpenCvSharp](#1-ffmpeg-integration-via-opencvsharp)
2. [ReactiveUI Integration with Terminal.Gui](#2-reactiveui-integration-with-terminalgui)
3. [Audio Visualization Algorithms](#3-audio-visualization-algorithms)
4. [Implementation Recommendations](#4-implementation-recommendations)
5. [References](#5-references)

---

## 1. FFmpeg Integration via OpenCvSharp

### 1.1 OpenCvSharp VideoCapture API for Frame Decoding

#### Overview

OpenCvSharp provides a C# wrapper around OpenCV's `VideoCapture` class, enabling video file reading and frame extraction. The class inherits from `DisposableCvObject` and manages unmanaged OpenCV resources.

#### Core Methods

**Frame Acquisition:**

```csharp
// Combines grab and decode operations
bool Read(Mat frame);

// Fetch next frame without decoding (multi-camera sync)
bool Grab();

// Decode previously grabbed frame
bool Retrieve(Mat frame, int channel = 0);
```

**Resource Management:**

```csharp
// Open video file or camera
bool Open(string path, VideoCaptureAPIs api = VideoCaptureAPIs.ANY);
bool Open(int cameraIndex, VideoCaptureAPIs api = VideoCaptureAPIs.ANY);

// Release video source
void Release();

// Verify initialization status
bool IsOpened();
```

**Configuration:**

```csharp
// Set capture properties
bool Set(VideoCaptureProperties property, double value);

// Get current property values
double Get(VideoCaptureProperties property);
```

#### C# Example - Basic Frame Extraction

```csharp
using OpenCvSharp;

// Open video file
using var capture = new VideoCapture(@"D:\videos\movie.mp4");

if (!capture.IsOpened())
{
    throw new InvalidOperationException("Failed to open video file");
}

// Get video metadata
int frameCount = (int)capture.Get(VideoCaptureProperties.FrameCount);
double fps = capture.Get(VideoCaptureProperties.Fps);
int width = (int)capture.Get(VideoCaptureProperties.FrameWidth);
int height = (int)capture.Get(VideoCaptureProperties.FrameHeight);

Console.WriteLine($"Video: {width}x{height} @ {fps} FPS, {frameCount} frames");

// Extract frames
using var frame = new Mat();
int frameIndex = 0;

while (capture.Read(frame))
{
    if (frame.Empty())
        break;

    // Process frame (e.g., save as PNG)
    frame.SaveImage($@"D:\frames\frame_{frameIndex:D6}.png");
    frameIndex++;

    // Optional: Display progress
    if (frameIndex % 100 == 0)
        Console.WriteLine($"Processed {frameIndex}/{frameCount} frames");
}

Console.WriteLine($"Extracted {frameIndex} frames");
```

#### Multi-Camera Synchronization Pattern

For synchronized multi-camera capture, use the "grab-then-retrieve" workflow:

```csharp
using var camera1 = new VideoCapture(0);
using var camera2 = new VideoCapture(1);

using var frame1 = new Mat();
using var frame2 = new Mat();

while (true)
{
    // Grab frames from both cameras simultaneously
    bool grabbed1 = camera1.Grab();
    bool grabbed2 = camera2.Grab();

    if (!grabbed1 || !grabbed2)
        break;

    // Retrieve (decode) frames
    camera1.Retrieve(frame1);
    camera2.Retrieve(frame2);

    // Process synchronized frames
    ProcessSyncedFrames(frame1, frame2);
}
```

#### Essential Properties

```csharp
// Frame geometry
int width = (int)capture.Get(VideoCaptureProperties.FrameWidth);
int height = (int)capture.Get(VideoCaptureProperties.FrameHeight);

// Temporal control
int currentFrame = (int)capture.Get(VideoCaptureProperties.PosFrames);
double positionMs = capture.Get(VideoCaptureProperties.PosMsec);
double fps = capture.Get(VideoCaptureProperties.Fps);
int totalFrames = (int)capture.Get(VideoCaptureProperties.FrameCount);

// Quality settings (camera-specific)
capture.Set(VideoCaptureProperties.Brightness, 0.5);
capture.Set(VideoCaptureProperties.Contrast, 0.7);
capture.Set(VideoCaptureProperties.Saturation, 0.6);
```

### 1.2 Audio Stream Extraction from Video Files

#### Important Limitation

**OpenCvSharp's VideoCapture class DOES NOT handle audio extraction.** It only processes video frames. For complete multimedia processing, you must use separate libraries.

#### Recommended Libraries for Audio Extraction

##### Option 1: FFMediaToolkit (Recommended)

FFMediaToolkit is a cross-platform .NET library that uses native FFmpeg libraries and supports both video and audio extraction.

**Installation:**

```bash
dotnet add package FFMediaToolkit
```

**C# Example - Extract Audio:**

```csharp
using FFMediaToolkit;
using FFMediaToolkit.Decoding;

// Open media file
var file = await MediaFile.Open(@"D:\videos\movie.mp4");

// Get audio stream info
var audioInfo = file.Audio.Info;
Console.WriteLine($"Audio: {audioInfo.SampleRate} Hz, {audioInfo.Channels} channels");

// Extract audio samples
var audioBuffer = new float[audioInfo.FrameSize * audioInfo.Channels];

while (file.Audio.TryGetNextFrame(audioBuffer))
{
    // Process audio samples
    ProcessAudioSamples(audioBuffer);
}

file.Dispose();
```

**Combined Video + Audio Extraction:**

```csharp
using FFMediaToolkit;
using FFMediaToolkit.Decoding;
using SkiaSharp;

var mediaFile = await MediaFile.Open(@"D:\videos\movie.mp4",
    new MediaOptions {
        VideoPixelFormat = ImagePixelFormat.Rgba32,
        StreamsToLoad = MediaMode.AudioVideo // Load both streams
    });

// Create reusable buffers
var videoBuffer = new byte[mediaFile.Video.FrameByteCount];
var audioBuffer = new float[mediaFile.Audio.Info.FrameSize * mediaFile.Audio.Info.Channels];

// Process video and audio together
while (mediaFile.Video.TryGetNextFrame(videoBuffer))
{
    // Get corresponding audio frame
    if (mediaFile.Audio.TryGetNextFrame(audioBuffer))
    {
        ProcessVideoAndAudio(videoBuffer, audioBuffer);
    }
}

mediaFile.Dispose();
```

##### Option 2: NAudio + MediaToolkit

```csharp
// First, extract audio to WAV using MediaToolkit
var inputFile = new MediaFile(@"D:\videos\movie.mp4");
var outputFile = new MediaFile(@"D:\audio\output.wav");

using (var engine = new Engine())
{
    engine.GetMetadata(inputFile);
    engine.Convert(inputFile, outputFile);
}

// Then read audio with NAudio
using (var reader = new WaveFileReader(@"D:\audio\output.wav"))
{
    var buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
    int bytesRead;

    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
    {
        ProcessAudioBytes(buffer, bytesRead);
    }
}
```

##### Option 3: Xabe.FFmpeg

```csharp
using Xabe.FFmpeg;

// Extract audio track to separate file
var conversion = await FFmpeg.Conversions.FromSnippet
    .ExtractAudio(@"D:\videos\movie.mp4", @"D:\audio\output.mp3");

await conversion.Start();
```

### 1.3 Seek Implementation Strategies

#### Frame-Accurate vs. Keyframe Seeking

**OpenCV's `Set(VideoCaptureProperties.PosFrames)` Behavior:**

- Historically positioned to the "nearest keyframe"
- Recently "fixed" but still has accuracy limitations
- Seek time is proportional to frame number (seeking to frame 200,000 can take ~30 seconds)

**Recommendation:** Use a hybrid approach depending on use case.

#### Strategy 1: Keyframe Seeking (Fast, Less Accurate)

```csharp
using var capture = new VideoCapture(@"video.mp4");

// Seek to approximate position (fast, keyframe-aligned)
int targetFrame = 1000;
capture.Set(VideoCaptureProperties.PosFrames, targetFrame);

// Read next frame (may not be exactly frame 1000)
using var frame = new Mat();
capture.Read(frame);

// Verify actual position
int actualFrame = (int)capture.Get(VideoCaptureProperties.PosFrames);
Console.WriteLine($"Target: {targetFrame}, Actual: {actualFrame}");
```

#### Strategy 2: Frame-Accurate Seeking (Slow, Accurate)

```csharp
public static Mat SeekToFrameAccurate(VideoCapture capture, int targetFrame)
{
    // Step 1: Seek to nearest keyframe before target
    int seekFrame = Math.Max(0, targetFrame - 50); // Seek back 50 frames
    capture.Set(VideoCaptureProperties.PosFrames, seekFrame);

    // Step 2: Read frames sequentially until target
    using var tempFrame = new Mat();
    int currentFrame = (int)capture.Get(VideoCaptureProperties.PosFrames);

    while (currentFrame < targetFrame)
    {
        if (!capture.Read(tempFrame))
            throw new InvalidOperationException("Failed to read frame");

        currentFrame++;
    }

    // Step 3: Return exact frame
    var targetMat = new Mat();
    capture.Read(targetMat);
    return targetMat;
}
```

#### Strategy 3: Timestamp-Based Seeking

```csharp
// Seek by timestamp (milliseconds)
double targetTimeMs = 5000.0; // 5 seconds
capture.Set(VideoCaptureProperties.PosMsec, targetTimeMs);

// Verify actual position
double actualTimeMs = capture.Get(VideoCaptureProperties.PosMsec);
Console.WriteLine($"Seeked to {actualTimeMs} ms");
```

#### Performance Optimization for Seeking

```csharp
public class VideoSeeker
{
    private readonly VideoCapture _capture;
    private readonly Dictionary<int, int> _keyframeCache = new();

    public VideoSeeker(VideoCapture capture)
    {
        _capture = capture;
        BuildKeyframeIndex();
    }

    private void BuildKeyframeIndex()
    {
        // Pre-compute keyframe positions during initial load
        // This is expensive but makes seeking much faster

        int frameIndex = 0;
        using var frame = new Mat();

        while (_capture.Read(frame))
        {
            // Detect keyframes (I-frames) using frame properties
            // Store keyframe positions
            if (IsKeyframe(frame))
            {
                _keyframeCache[frameIndex] = frameIndex;
            }

            frameIndex++;

            if (frameIndex % 1000 == 0)
                Console.WriteLine($"Indexed {frameIndex} frames...");
        }

        // Reset to beginning
        _capture.Set(VideoCaptureProperties.PosFrames, 0);
    }

    public Mat SeekToFrame(int targetFrame)
    {
        // Find nearest keyframe before target
        int nearestKeyframe = _keyframeCache.Keys
            .Where(k => k <= targetFrame)
            .OrderByDescending(k => k)
            .FirstOrDefault();

        // Seek to keyframe, then read sequentially
        _capture.Set(VideoCaptureProperties.PosFrames, nearestKeyframe);

        using var tempFrame = new Mat();
        for (int i = nearestKeyframe; i < targetFrame; i++)
        {
            _capture.Read(tempFrame);
        }

        var result = new Mat();
        _capture.Read(result);
        return result;
    }

    private bool IsKeyframe(Mat frame)
    {
        // Keyframe detection logic (simplified)
        // In practice, you'd use codec-specific methods
        return false; // Placeholder
    }
}
```

### 1.4 Memory Management Best Practices

#### Always Dispose Unmanaged Resources

```csharp
// GOOD: Using statement ensures disposal
using (var capture = new VideoCapture(@"video.mp4"))
{
    using (var frame = new Mat())
    {
        while (capture.Read(frame))
        {
            ProcessFrame(frame);
        }
    } // Mat disposed here
} // VideoCapture disposed here

// BAD: Manual disposal prone to leaks
var capture = new VideoCapture(@"video.mp4");
var frame = new Mat();
// ... code that might throw exception ...
frame.Dispose(); // Might not be reached
capture.Dispose();
```

#### Memory Pool Pattern with ArrayPool

```csharp
using System.Buffers;

public class VideoFrameProcessor
{
    private readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

    public void ProcessVideoFrames(VideoCapture capture)
    {
        int frameSize = (int)capture.Get(VideoCaptureProperties.FrameWidth) *
                       (int)capture.Get(VideoCaptureProperties.FrameHeight) * 3; // RGB

        byte[] buffer = _bufferPool.Rent(frameSize);

        try
        {
            using var frame = new Mat();

            while (capture.Read(frame))
            {
                // Copy frame data to pooled buffer
                Marshal.Copy(frame.Data, buffer, 0, frameSize);

                // Process buffer
                ProcessFrameBuffer(buffer, frameSize);
            }
        }
        finally
        {
            // IMPORTANT: Return buffer to pool
            _bufferPool.Return(buffer);
        }
    }

    private void ProcessFrameBuffer(byte[] buffer, int size)
    {
        // Process frame data
    }
}
```

#### Explicit Release of Large Objects

```csharp
public class VideoPlayer
{
    private Mat? _currentFrame;
    private Mat? _previousFrame;

    public void LoadNextFrame(VideoCapture capture)
    {
        // Release previous frames before allocating new ones
        _previousFrame?.Dispose();
        _previousFrame = _currentFrame;

        _currentFrame = new Mat();
        capture.Read(_currentFrame);

        // If memory pressure is high, force GC
        if (ShouldCollect())
        {
            GC.Collect(2, GCCollectionMode.Forced);
        }
    }

    private bool ShouldCollect()
    {
        // Collect if allocated >500MB
        return GC.GetTotalMemory(false) > 500_000_000;
    }
}
```

#### Use cv::UMat for GPU Acceleration (Advanced)

```csharp
// Note: OpenCvSharp supports UMat for GPU memory sharing
using var uFrame = new UMat();
capture.Read(uFrame); // Reads directly to GPU memory

// Process on GPU if OpenCL is available
Cv2.CvtColor(uFrame, uFrame, ColorConversionCodes.BGR2GRAY);

// Convert to CPU Mat only when needed
var cpuFrame = uFrame.GetMat(AccessFlag.READ);
```

#### Avoid Unnecessary Copies

```csharp
// BAD: Creates unnecessary copy
Mat frame1 = new Mat();
capture.Read(frame1);
Mat frame2 = frame1.Clone(); // Expensive full copy
ProcessFrame(frame2);

// GOOD: Use reference or pointer if read-only
Mat frame = new Mat();
capture.Read(frame);
ProcessFrame(frame); // Pass by reference, no copy
```

#### Memory-Efficient Video Loading

```csharp
public class MemoryEfficientVideoLoader
{
    public IEnumerable<Mat> LoadFramesLazy(string videoPath)
    {
        using var capture = new VideoCapture(videoPath);
        using var frame = new Mat();

        while (capture.Read(frame))
        {
            // Yield returns frames one at a time
            // Caller controls when next frame is loaded
            yield return frame.Clone(); // Clone for safety
        }
    }

    public void ProcessVideo(string videoPath)
    {
        // Frames are loaded on-demand, not all at once
        foreach (var frame in LoadFramesLazy(videoPath))
        {
            using (frame) // Dispose each frame after processing
            {
                ProcessFrame(frame);
            }
        }
    }
}
```

### 1.5 Thread Safety Considerations

#### VideoCapture is NOT Thread-Safe

**Key Rule:** VideoCapture cannot be safely accessed from multiple threads without synchronization.

#### Recommended Pattern: Producer-Consumer with Thread-Safe Queue

```csharp
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class ThreadSafeVideoDecoder
{
    private readonly ConcurrentQueue<Mat> _frameQueue = new();
    private readonly SemaphoreSlim _queueSemaphore = new(0);
    private readonly int _maxQueueSize = 60; // Buffer 60 frames
    private CancellationTokenSource _cts = new();

    public void StartDecoding(string videoPath)
    {
        _cts = new CancellationTokenSource();

        // Producer thread: Decode frames
        Task.Run(() => DecodeFrames(videoPath, _cts.Token));
    }

    private void DecodeFrames(string videoPath, CancellationToken ct)
    {
        using var capture = new VideoCapture(videoPath);

        while (!ct.IsCancellationRequested)
        {
            // Wait if queue is full (backpressure)
            while (_frameQueue.Count >= _maxQueueSize && !ct.IsCancellationRequested)
            {
                Thread.Sleep(10);
            }

            var frame = new Mat();
            if (!capture.Read(frame))
            {
                frame.Dispose();
                break; // End of video
            }

            // Deep copy required for thread safety
            var frameCopy = frame.Clone();
            frame.Dispose();

            _frameQueue.Enqueue(frameCopy);
            _queueSemaphore.Release();
        }
    }

    public async Task<Mat?> GetNextFrameAsync(CancellationToken ct)
    {
        // Consumer thread: Get frames from queue
        await _queueSemaphore.WaitAsync(ct);

        if (_frameQueue.TryDequeue(out var frame))
        {
            return frame;
        }

        return null;
    }

    public void Stop()
    {
        _cts.Cancel();
    }
}
```

#### Using Mutex for Shared Access

```csharp
public class MutexProtectedVideoCapture
{
    private readonly VideoCapture _capture;
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public MutexProtectedVideoCapture(string videoPath)
    {
        _capture = new VideoCapture(videoPath);
    }

    public async Task<Mat?> ReadFrameAsync(CancellationToken ct)
    {
        await _mutex.WaitAsync(ct);

        try
        {
            var frame = new Mat();
            if (_capture.Read(frame))
            {
                return frame.Clone(); // Return deep copy
            }

            frame.Dispose();
            return null;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task SeekAsync(int frameNumber, CancellationToken ct)
    {
        await _mutex.WaitAsync(ct);

        try
        {
            _capture.Set(VideoCaptureProperties.PosFrames, frameNumber);
        }
        finally
        {
            _mutex.Release();
        }
    }
}
```

#### IMPORTANT: Clone Frames for Thread Safety

```csharp
// BAD: Shared Mat reference across threads
Mat sharedFrame = new Mat();
capture.Read(sharedFrame);

Task.Run(() => ProcessFrame(sharedFrame)); // UNSAFE!
Task.Run(() => ProcessFrame(sharedFrame)); // UNSAFE!

// GOOD: Deep copy for each thread
Mat originalFrame = new Mat();
capture.Read(originalFrame);

Mat frameCopy1 = originalFrame.Clone();
Mat frameCopy2 = originalFrame.Clone();

Task.Run(() => { using (frameCopy1) ProcessFrame(frameCopy1); });
Task.Run(() => { using (frameCopy2) ProcessFrame(frameCopy2); });

originalFrame.Dispose();
```

### 1.6 Error Handling for Corrupted or Unsupported Formats

#### Check if Video Opened Successfully

```csharp
public static bool TryOpenVideo(string path, out VideoCapture? capture)
{
    capture = new VideoCapture(path);

    if (!capture.IsOpened)
    {
        capture.Dispose();
        capture = null;
        return false;
    }

    return true;
}

// Usage
if (!TryOpenVideo(@"video.mp4", out var capture))
{
    Console.WriteLine("Failed to open video file");
    return;
}

using (capture)
{
    // Process video
}
```

#### Validate Frame Reading

```csharp
public static bool TryReadFrame(VideoCapture capture, out Mat? frame)
{
    frame = new Mat();

    if (!capture.Read(frame))
    {
        frame.Dispose();
        frame = null;
        return false;
    }

    if (frame.Empty())
    {
        frame.Dispose();
        frame = null;
        return false;
    }

    return true;
}

// Usage
while (TryReadFrame(capture, out var frame))
{
    using (frame)
    {
        ProcessFrame(frame);
    }
}
```

#### Common Causes of Errors

**1. Missing FFmpeg DLLs:**

```csharp
public static bool CheckFFmpegAvailable()
{
    try
    {
        // Try to create a VideoCapture instance
        using var testCapture = new VideoCapture();
        return true;
    }
    catch (DllNotFoundException ex)
    {
        Console.WriteLine($"FFmpeg DLLs not found: {ex.Message}");
        Console.WriteLine("Please install opencv_ffmpeg247.dll in your application directory");
        return false;
    }
}
```

**2. Codec Issues:**

```csharp
public static void ValidateCodecSupport(VideoCapture capture)
{
    // Get codec information
    double fourcc = capture.Get(VideoCaptureProperties.FourCC);

    // Convert FOURCC to readable string
    string codec = FourCCToString((int)fourcc);

    Console.WriteLine($"Video codec: {codec}");

    // Known problematic codecs
    var unsupportedCodecs = new[] { "H265", "HEVC", "VP9" };

    if (unsupportedCodecs.Contains(codec))
    {
        Console.WriteLine($"Warning: Codec {codec} may not be fully supported");
        Console.WriteLine("Consider re-encoding with XVID (AVI) or H264 (MP4)");
    }
}

private static string FourCCToString(int fourcc)
{
    return new string(new[]
    {
        (char)(fourcc & 0xFF),
        (char)((fourcc >> 8) & 0xFF),
        (char)((fourcc >> 16) & 0xFF),
        (char)((fourcc >> 24) & 0xFF)
    });
}
```

**3. Timeout for Network Streams:**

```csharp
public static async Task<VideoCapture?> OpenWithTimeoutAsync(
    string path,
    TimeSpan timeout)
{
    var cts = new CancellationTokenSource(timeout);

    try
    {
        return await Task.Run(() =>
        {
            var capture = new VideoCapture(path);

            if (!capture.IsOpened)
            {
                capture.Dispose();
                throw new InvalidOperationException("Failed to open video");
            }

            return capture;
        }, cts.Token);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine($"Timeout opening video: {path}");
        return null;
    }
}
```

**4. Graceful Degradation:**

```csharp
public class RobustVideoLoader
{
    public VideoCapture? LoadVideo(string path)
    {
        // Try primary method
        var capture = new VideoCapture(path);

        if (capture.IsOpened)
            return capture;

        capture.Dispose();

        // Try alternative APIs
        foreach (var api in new[]
        {
            VideoCaptureAPIs.FFMPEG,
            VideoCaptureAPIs.MSMF,    // Windows Media Foundation
            VideoCaptureAPIs.DSHOW,   // DirectShow
            VideoCaptureAPIs.ANY
        })
        {
            capture = new VideoCapture(path, api);

            if (capture.IsOpened)
            {
                Console.WriteLine($"Opened with API: {api}");
                return capture;
            }

            capture.Dispose();
        }

        Console.WriteLine($"Failed to open video with any API: {path}");
        return null;
    }
}
```

### 1.7 Performance Considerations

#### Frame Rate Pacing

```csharp
public class FrameRatePacer
{
    private readonly double _targetFps;
    private readonly Stopwatch _stopwatch = new();
    private readonly TimeSpan _frameInterval;

    public FrameRatePacer(double targetFps)
    {
        _targetFps = targetFps;
        _frameInterval = TimeSpan.FromMilliseconds(1000.0 / targetFps);
    }

    public void Start()
    {
        _stopwatch.Restart();
    }

    public async Task WaitForNextFrameAsync()
    {
        var elapsed = _stopwatch.Elapsed;
        var delay = _frameInterval - elapsed;

        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay);
        }

        _stopwatch.Restart();
    }
}

// Usage
var pacer = new FrameRatePacer(30.0); // 30 FPS

using var capture = new VideoCapture(@"video.mp4");
using var frame = new Mat();

pacer.Start();

while (capture.Read(frame))
{
    ProcessFrame(frame);
    await pacer.WaitForNextFrameAsync();
}
```

#### Parallel Frame Processing

```csharp
public async Task ProcessVideoParallel(string videoPath)
{
    using var capture = new VideoCapture(videoPath);
    var frames = new ConcurrentBag<Mat>();

    // Step 1: Load all frames (memory-intensive)
    using var tempFrame = new Mat();
    while (capture.Read(tempFrame))
    {
        frames.Add(tempFrame.Clone());
    }

    // Step 2: Process frames in parallel
    await Parallel.ForEachAsync(frames, async (frame, ct) =>
    {
        using (frame) // Dispose after processing
        {
            await ProcessFrameAsync(frame, ct);
        }
    });
}
```

---

## 2. ReactiveUI Integration with Terminal.Gui

### 2.1 Property Binding Patterns Without XAML

#### Overview

ReactiveUI provides MVVM patterns for non-XAML UI frameworks like Terminal.Gui. Instead of declarative bindings in XAML, you use code-based subscriptions with `WhenAnyValue` and `BindTo`.

#### Setting Up ReactiveUI with Terminal.Gui

```csharp
using ReactiveUI;
using Terminal.Gui;

// Initialize Terminal.Gui with ReactiveUI schedulers
Application.Init();
RxApp.MainThreadScheduler = TerminalScheduler.Default;
RxApp.TaskpoolScheduler = TaskPoolScheduler.Default;

Application.Run(new RootView(new RootViewModel()));
```

**Purpose of Schedulers:**

- `TerminalScheduler.Default` ensures observable operations return to the UI thread
- `TaskPoolScheduler.Default` provides background thread pool for async operations

### 2.2 ViewModel Implementation

#### Basic ReactiveObject ViewModel

```csharp
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;

public class MediaPlayerViewModel : ReactiveObject
{
    // Reactive properties with automatic INotifyPropertyChanged
    [Reactive] public PlaybackState State { get; private set; }
    [Reactive] public TimeSpan Position { get; private set; }
    [Reactive] public TimeSpan Duration { get; private set; }
    [Reactive] public double Volume { get; set; } = 0.8;
    [Reactive] public string CurrentFile { get; private set; } = string.Empty;

    // Reactive commands
    public ReactiveCommand<Unit, Unit> PlayCommand { get; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; }
    public ReactiveCommand<TimeSpan, Unit> SeekCommand { get; }

    private readonly IMediaService _mediaService;

    public MediaPlayerViewModel(IMediaService mediaService)
    {
        _mediaService = mediaService;

        // Define when commands can execute
        var canPlay = this.WhenAnyValue(
            x => x.State,
            state => state == PlaybackState.Paused || state == PlaybackState.Stopped);

        var canPause = this.WhenAnyValue(
            x => x.State,
            state => state == PlaybackState.Playing);

        var canStop = this.WhenAnyValue(
            x => x.State,
            state => state != PlaybackState.Stopped);

        // Create commands
        PlayCommand = ReactiveCommand.CreateFromTask(
            _mediaService.PlayAsync, canPlay);

        PauseCommand = ReactiveCommand.CreateFromTask(
            _mediaService.PauseAsync, canPause);

        StopCommand = ReactiveCommand.CreateFromTask(
            _mediaService.StopAsync, canStop);

        SeekCommand = ReactiveCommand.CreateFromTask<TimeSpan>(
            position => _mediaService.SeekAsync(position));

        // Subscribe to service state changes
        _mediaService.PlaybackState
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(state => State = state);

        _mediaService.Position
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(pos => Position = pos);

        _mediaService.Duration
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(dur => Duration = dur);
    }
}
```

### 2.3 One-Way Binding (ViewModel → View)

#### Basic WhenAnyValue Binding

```csharp
public class MediaPlayerView : Window
{
    private readonly MediaPlayerViewModel _viewModel;
    private readonly TextField _positionLabel;
    private readonly ProgressBar _progressBar;
    private readonly Label _stateLabel;

    public MediaPlayerView(MediaPlayerViewModel viewModel)
    {
        _viewModel = viewModel;

        // Create UI controls
        _positionLabel = new TextField { X = 0, Y = 0, Width = 20 };
        _progressBar = new ProgressBar { X = 0, Y = 1, Width = Dim.Fill() };
        _stateLabel = new Label { X = 0, Y = 2, Width = 20 };

        Add(_positionLabel, _progressBar, _stateLabel);

        // Bind ViewModel properties to View controls
        SetupBindings();
    }

    private void SetupBindings()
    {
        // Bind Position property to text field
        _viewModel
            .WhenAnyValue(vm => vm.Position)
            .Select(pos => pos.ToString(@"mm\:ss"))
            .BindTo(_positionLabel, view => view.Text);

        // Bind progress bar
        _viewModel
            .WhenAnyValue(
                vm => vm.Position,
                vm => vm.Duration,
                (pos, dur) => dur.TotalSeconds > 0
                    ? (float)(pos.TotalSeconds / dur.TotalSeconds)
                    : 0f)
            .BindTo(_progressBar, view => view.Fraction);

        // Bind state label
        _viewModel
            .WhenAnyValue(vm => vm.State)
            .Select(state => state.ToString())
            .BindTo(_stateLabel, view => view.Text);
    }
}
```

### 2.4 One-Way-To-Source Binding (View → ViewModel)

#### Using Pharmacist for Event Wrapping

**Install Pharmacist:**

```bash
dotnet add package Pharmacist.MSBuild
```

**Binding TextField TextChanged to ViewModel:**

```csharp
using ReactiveMarbles.ObservableEvents;

public class SearchView : Window
{
    private readonly SearchViewModel _viewModel;
    private readonly TextField _searchInput;

    public SearchView(SearchViewModel viewModel)
    {
        _viewModel = viewModel;
        _searchInput = new TextField { X = 0, Y = 0, Width = 40 };

        Add(_searchInput);
        SetupBindings();
    }

    private void SetupBindings()
    {
        // Bind TextField changes to ViewModel property
        _searchInput
            .Events()  // Generated by Pharmacist
            .TextChanged
            .Select(args => _searchInput.Text.ToString())
            .DistinctUntilChanged()
            .BindTo(_viewModel, vm => vm.SearchText);
    }
}

public class SearchViewModel : ReactiveObject
{
    [Reactive] public string SearchText { get; set; } = string.Empty;

    public SearchViewModel()
    {
        // Trigger search when text changes
        this.WhenAnyValue(x => x.SearchText)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Throttle(TimeSpan.FromMilliseconds(300))
            .InvokeCommand(SearchCommand);
    }

    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
}
```

### 2.5 Binding ReactiveCommands to Terminal.Gui Button.Clicked Events

#### Method 1: Using BindCommand

```csharp
public class MediaControlsView : View
{
    private readonly MediaPlayerViewModel _viewModel;
    private readonly Button _playButton;
    private readonly Button _pauseButton;
    private readonly Button _stopButton;

    public MediaControlsView(MediaPlayerViewModel viewModel)
    {
        _viewModel = viewModel;

        _playButton = new Button("Play") { X = 0, Y = 0 };
        _pauseButton = new Button("Pause") { X = 10, Y = 0 };
        _stopButton = new Button("Stop") { X = 20, Y = 0 };

        Add(_playButton, _pauseButton, _stopButton);
        SetupBindings();
    }

    private void SetupBindings()
    {
        // Bind commands to button click events
        this.BindCommand(
            _viewModel,
            vm => vm.PlayCommand,
            view => view._playButton,
            nameof(Button.Clicked));

        this.BindCommand(
            _viewModel,
            vm => vm.PauseCommand,
            view => view._pauseButton,
            nameof(Button.Clicked));

        this.BindCommand(
            _viewModel,
            vm => vm.StopCommand,
            view => view._stopButton,
            nameof(Button.Clicked));

        // Enable/disable buttons based on CanExecute
        _viewModel.PlayCommand.CanExecute
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(canExecute => _playButton.Enabled = canExecute);

        _viewModel.PauseCommand.CanExecute
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(canExecute => _pauseButton.Enabled = canExecute);
    }
}
```

#### Method 2: Manual Event Subscription with InvokeCommand

```csharp
private void SetupBindings()
{
    // Using Events() extension from Pharmacist
    _playButton
        .Events()
        .Clicked
        .Select(_ => Unit.Default)
        .InvokeCommand(_viewModel, vm => vm.PlayCommand);

    _pauseButton
        .Events()
        .Clicked
        .Select(_ => Unit.Default)
        .InvokeCommand(_viewModel, vm => vm.PauseCommand);

    _stopButton
        .Events()
        .Clicked
        .Select(_ => Unit.Default)
        .InvokeCommand(_viewModel, vm => vm.StopCommand);
}
```

#### Method 3: Direct Observable Subscription

```csharp
private void SetupBindings()
{
    // Create observable from button click event
    Observable
        .FromEventPattern(
            h => _playButton.Clicked += h,
            h => _playButton.Clicked -= h)
        .Select(_ => Unit.Default)
        .InvokeCommand(_viewModel, vm => vm.PlayCommand);
}
```

### 2.6 Using Application.MainLoop.Invoke() for Thread-Safe UI Updates

#### Background Thread to UI Thread Pattern

```csharp
public class MediaPlayerService : IMediaService
{
    private readonly Subject<TimeSpan> _positionSubject = new();
    private CancellationTokenSource? _playbackCts;

    public IObservable<TimeSpan> Position => _positionSubject.AsObservable();

    public Task PlayAsync(CancellationToken ct = default)
    {
        _playbackCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // Start background playback task
        Task.Run(async () =>
        {
            await PlaybackLoopAsync(_playbackCts.Token);
        }, _playbackCts.Token);

        return Task.CompletedTask;
    }

    private async Task PlaybackLoopAsync(CancellationToken ct)
    {
        var position = TimeSpan.Zero;
        var interval = TimeSpan.FromMilliseconds(100);

        while (!ct.IsCancellationRequested)
        {
            // Update position on background thread
            position += interval;

            // Marshal to UI thread for safe updates
            Application.MainLoop.Invoke(() =>
            {
                _positionSubject.OnNext(position);
            });

            await Task.Delay(interval, ct);
        }
    }
}
```

#### Updating Terminal.Gui Views from Background Threads

```csharp
public class VideoRendererView : View
{
    private readonly IMediaService _mediaService;
    private string _currentFrame = string.Empty;

    public VideoRendererView(IMediaService mediaService)
    {
        _mediaService = mediaService;
        SetupFrameUpdates();
    }

    private void SetupFrameUpdates()
    {
        _mediaService.FrameStream
            .ObserveOn(RxApp.MainThreadScheduler) // Important: Switch to UI thread
            .Subscribe(frame =>
            {
                // Now safe to update UI
                UpdateFrameDisplay(frame);
            });
    }

    private void UpdateFrameDisplay(MediaFrame frame)
    {
        // Update view state
        _currentFrame = ConvertFrameToAscii(frame);

        // Request redraw
        SetNeedsDisplay();
    }

    public override void OnDrawContent(Rect viewport)
    {
        base.OnDrawContent(viewport);

        // Draw current frame
        if (!string.IsNullOrEmpty(_currentFrame))
        {
            Move(0, 0);
            Driver.AddStr(_currentFrame);
        }
    }
}
```

#### Manual MainLoop.Invoke Pattern

```csharp
public class PlaybackController
{
    private readonly Label _statusLabel;

    public PlaybackController(Label statusLabel)
    {
        _statusLabel = statusLabel;
    }

    public void StartPlayback()
    {
        Task.Run(async () =>
        {
            for (int i = 0; i < 100; i++)
            {
                var status = $"Frame {i}/100";

                // IMPORTANT: Update UI on main thread
                Application.MainLoop.Invoke(() =>
                {
                    _statusLabel.Text = status;
                    _statusLabel.SetNeedsDisplay();
                });

                await Task.Delay(33); // ~30 FPS
            }

            // Playback complete
            Application.MainLoop.Invoke(() =>
            {
                _statusLabel.Text = "Playback Complete";
                _statusLabel.SetNeedsDisplay();
            });
        });
    }
}
```

### 2.7 Performance Considerations and Best Practices

#### 1. Minimize UI Thread Work

```csharp
// BAD: Heavy computation on UI thread
_viewModel
    .WhenAnyValue(vm => vm.RawData)
    .Select(data => HeavyProcessing(data)) // Blocks UI!
    .BindTo(_view, v => v.ProcessedData);

// GOOD: Offload to background thread
_viewModel
    .WhenAnyValue(vm => vm.RawData)
    .ObserveOn(RxApp.TaskpoolScheduler) // Switch to background
    .Select(data => HeavyProcessing(data))
    .ObserveOn(RxApp.MainThreadScheduler) // Switch back to UI
    .BindTo(_view, v => v.ProcessedData);
```

#### 2. Use Throttle/Debounce for High-Frequency Updates

```csharp
// Video position updates every 16ms (60 FPS)
// Don't update UI that frequently!

_mediaService.Position
    .Sample(TimeSpan.FromMilliseconds(100)) // Update UI at 10 Hz
    .ObserveOn(RxApp.MainThreadScheduler)
    .Subscribe(pos => UpdatePositionDisplay(pos));
```

#### 3. Dispose Subscriptions Properly

```csharp
public class MediaPlayerView : Window, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    private void SetupBindings()
    {
        // Store disposable subscriptions
        _viewModel
            .WhenAnyValue(vm => vm.Position)
            .BindTo(_positionLabel, v => v.Text)
            .DisposeWith(_disposables);

        _viewModel.PlayCommand.CanExecute
            .Subscribe(canExecute => _playButton.Enabled = canExecute)
            .DisposeWith(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
```

#### 4. Avoid Memory Leaks with WhenActivated

```csharp
public class MediaPlayerView : Window, IViewFor<MediaPlayerViewModel>
{
    public MediaPlayerViewModel? ViewModel { get; set; }
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (MediaPlayerViewModel?)value; }

    public MediaPlayerView()
    {
        this.WhenActivated(disposables =>
        {
            // Bindings are automatically disposed when view is deactivated
            this.OneWayBind(ViewModel, vm => vm.Position, v => v.PositionLabel.Text)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.PlayCommand, v => v.PlayButton)
                .DisposeWith(disposables);
        });
    }
}
```

#### 5. Batch Updates with ObserveOn

```csharp
// Combine multiple property updates
Observable.CombineLatest(
    _viewModel.WhenAnyValue(vm => vm.Position),
    _viewModel.WhenAnyValue(vm => vm.Duration),
    _viewModel.WhenAnyValue(vm => vm.State),
    (pos, dur, state) => new { Position = pos, Duration = dur, State = state })
    .ObserveOn(RxApp.MainThreadScheduler)
    .Subscribe(data =>
    {
        // Single UI update for all three properties
        UpdateDisplay(data.Position, data.Duration, data.State);
    });
```

---

## 3. Audio Visualization Algorithms

### 3.1 FFT (Fast Fourier Transform) Implementation

#### Overview

FFT converts time-domain audio signals into frequency-domain representation, enabling spectrum analysis for visualization.

#### Using FftSharp Library

**Installation:**

```bash
dotnet add package FftSharp
dotnet add package NAudio
```

#### Basic FFT Example with NAudio

```csharp
using NAudio.Wave;
using FftSharp;
using System.Numerics;

public class AudioSpectrumAnalyzer
{
    private readonly int _sampleRate = 44100;
    private readonly int _fftSize = 2048; // Must be power of 2
    private readonly double[] _audioBuffer;
    private double[] _fftMagnitude;
    private double[] _frequencies;

    public AudioSpectrumAnalyzer()
    {
        _audioBuffer = new double[_fftSize];
        _fftMagnitude = new double[_fftSize / 2]; // Only half (Nyquist)
        _frequencies = FftSharp.Transform.FFTfreq(_sampleRate, _fftSize);
    }

    public void ProcessAudioData(float[] samples)
    {
        // Convert float samples to double
        for (int i = 0; i < Math.Min(samples.Length, _audioBuffer.Length); i++)
        {
            _audioBuffer[i] = samples[i];
        }

        // Apply Hann window to reduce spectral leakage
        var window = new FftSharp.Windows.Hanning();
        window.ApplyInPlace(_audioBuffer);

        // Perform FFT
        Complex[] spectrum = FftSharp.Transform.FFT(_audioBuffer);

        // Calculate magnitude (power spectrum)
        _fftMagnitude = FftSharp.Transform.FFTmagnitude(spectrum);

        // Alternative: Get power in dB
        // double[] powerDb = FftSharp.Transform.FFTpower(spectrum);
    }

    public double[] GetMagnitudeSpectrum() => _fftMagnitude;
    public double[] GetFrequencies() => _frequencies;
}
```

#### Real-Time Microphone Spectrum Analysis

```csharp
using NAudio.Wave;
using FftSharp;

public class RealTimeSpectrumAnalyzer : IDisposable
{
    private readonly WaveInEvent _waveIn;
    private readonly int _fftSize = 4096;
    private readonly double[] _audioBuffer;
    private readonly object _lock = new();

    public double[] CurrentSpectrum { get; private set; }
    public event EventHandler<double[]>? SpectrumUpdated;

    public RealTimeSpectrumAnalyzer()
    {
        _audioBuffer = new double[_fftSize];
        CurrentSpectrum = new double[_fftSize / 2];

        // Setup audio capture
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(44100, 1), // 44.1kHz, Mono
            BufferMilliseconds = 50
        };

        _waveIn.DataAvailable += OnDataAvailable;
    }

    public void Start()
    {
        _waveIn.StartRecording();
    }

    public void Stop()
    {
        _waveIn.StopRecording();
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        // Convert byte buffer to float samples
        var samples = new float[e.BytesRecorded / 4]; // 4 bytes per float
        Buffer.BlockCopy(e.Buffer, 0, samples, 0, e.BytesRecorded);

        // Copy to FFT buffer
        lock (_lock)
        {
            for (int i = 0; i < Math.Min(samples.Length, _audioBuffer.Length); i++)
            {
                _audioBuffer[i] = samples[i];
            }
        }

        // Process FFT
        ComputeSpectrum();
    }

    private void ComputeSpectrum()
    {
        double[] bufferCopy;

        lock (_lock)
        {
            bufferCopy = (double[])_audioBuffer.Clone();
        }

        // Apply Hann window
        var window = new FftSharp.Windows.Hanning();
        window.ApplyInPlace(bufferCopy);

        // Compute FFT
        var fftResult = FftSharp.Transform.FFT(bufferCopy);
        var magnitude = FftSharp.Transform.FFTmagnitude(fftResult);

        // Update spectrum (only first half due to Nyquist)
        CurrentSpectrum = magnitude.Take(_fftSize / 2).ToArray();

        // Notify listeners
        SpectrumUpdated?.Invoke(this, CurrentSpectrum);
    }

    public void Dispose()
    {
        _waveIn?.Dispose();
    }
}
```

### 3.2 Extracting Frequency Data from Audio Samples

#### Frequency Binning

```csharp
public class FrequencyBinAnalyzer
{
    private readonly int _sampleRate;
    private readonly int _fftSize;
    private readonly double _frequencyResolution;

    public FrequencyBinAnalyzer(int sampleRate, int fftSize)
    {
        _sampleRate = sampleRate;
        _fftSize = fftSize;
        _frequencyResolution = (double)sampleRate / fftSize;
    }

    public int FrequencyToBin(double frequency)
    {
        return (int)(frequency / _frequencyResolution);
    }

    public double BinToFrequency(int bin)
    {
        return bin * _frequencyResolution;
    }

    public double[] GetFrequencyRange(double[] fftMagnitude, double minFreq, double maxFreq)
    {
        int minBin = FrequencyToBin(minFreq);
        int maxBin = FrequencyToBin(maxFreq);

        return fftMagnitude.Skip(minBin).Take(maxBin - minBin).ToArray();
    }

    // Get common frequency bands
    public Dictionary<string, double> GetFrequencyBands(double[] fftMagnitude)
    {
        return new Dictionary<string, double>
        {
            ["Sub-Bass (20-60 Hz)"] = GetBandAverage(fftMagnitude, 20, 60),
            ["Bass (60-250 Hz)"] = GetBandAverage(fftMagnitude, 60, 250),
            ["Low Midrange (250-500 Hz)"] = GetBandAverage(fftMagnitude, 250, 500),
            ["Midrange (500-2000 Hz)"] = GetBandAverage(fftMagnitude, 500, 2000),
            ["Upper Midrange (2k-4k Hz)"] = GetBandAverage(fftMagnitude, 2000, 4000),
            ["Presence (4k-6k Hz)"] = GetBandAverage(fftMagnitude, 4000, 6000),
            ["Brilliance (6k-20k Hz)"] = GetBandAverage(fftMagnitude, 6000, 20000)
        };
    }

    private double GetBandAverage(double[] fftMagnitude, double minFreq, double maxFreq)
    {
        var range = GetFrequencyRange(fftMagnitude, minFreq, maxFreq);
        return range.Length > 0 ? range.Average() : 0;
    }
}
```

#### Logarithmic Frequency Scaling

```csharp
public class LogarithmicFrequencyScaler
{
    public static double[] ScaleToLogarithmic(double[] linearSpectrum, int outputBins, int sampleRate)
    {
        var logSpectrum = new double[outputBins];
        double minFreq = 20.0;  // 20 Hz (human hearing minimum)
        double maxFreq = sampleRate / 2.0; // Nyquist frequency

        for (int i = 0; i < outputBins; i++)
        {
            // Calculate logarithmic frequency range for this bin
            double freqRatio = (double)i / outputBins;
            double freq = minFreq * Math.Pow(maxFreq / minFreq, freqRatio);

            // Map to linear bin
            int linearBin = (int)(freq / (sampleRate / (double)linearSpectrum.Length));
            linearBin = Math.Min(linearBin, linearSpectrum.Length - 1);

            logSpectrum[i] = linearSpectrum[linearBin];
        }

        return logSpectrum;
    }
}
```

### 3.3 Waveform Sampling and Downsampling Techniques

#### Min-Max Downsampling (Peak Detection)

```csharp
public class WaveformDownsampler
{
    public static float[] DownsampleMinMax(float[] samples, int targetWidth)
    {
        if (samples.Length <= targetWidth)
            return samples;

        var downsampled = new float[targetWidth * 2]; // Min and max for each pixel
        int samplesPerPixel = samples.Length / targetWidth;

        for (int i = 0; i < targetWidth; i++)
        {
            int start = i * samplesPerPixel;
            int end = Math.Min(start + samplesPerPixel, samples.Length);

            float min = float.MaxValue;
            float max = float.MinValue;

            for (int j = start; j < end; j++)
            {
                if (samples[j] < min) min = samples[j];
                if (samples[j] > max) max = samples[j];
            }

            downsampled[i * 2] = min;
            downsampled[i * 2 + 1] = max;
        }

        return downsampled;
    }
}
```

#### Average Downsampling

```csharp
public static float[] DownsampleAverage(float[] samples, int targetWidth)
{
    if (samples.Length <= targetWidth)
        return samples;

    var downsampled = new float[targetWidth];
    int samplesPerPixel = samples.Length / targetWidth;

    for (int i = 0; i < targetWidth; i++)
    {
        int start = i * samplesPerPixel;
        int end = Math.Min(start + samplesPerPixel, samples.Length);

        float sum = 0;
        for (int j = start; j < end; j++)
        {
            sum += Math.Abs(samples[j]); // Use absolute value for waveform
        }

        downsampled[i] = sum / (end - start);
    }

    return downsampled;
}
```

#### RMS Downsampling (Root Mean Square)

```csharp
public static float[] DownsampleRMS(float[] samples, int targetWidth)
{
    if (samples.Length <= targetWidth)
        return samples;

    var downsampled = new float[targetWidth];
    int samplesPerPixel = samples.Length / targetWidth;

    for (int i = 0; i < targetWidth; i++)
    {
        int start = i * samplesPerPixel;
        int end = Math.Min(start + samplesPerPixel, samples.Length);

        double sumSquares = 0;
        for (int j = start; j < end; j++)
        {
            sumSquares += samples[j] * samples[j];
        }

        downsampled[i] = (float)Math.Sqrt(sumSquares / (end - start));
    }

    return downsampled;
}
```

### 3.4 Rendering with Braille or Block Characters

#### Braille Pattern Calculation

```csharp
public class BrailleRenderer
{
    // Unicode braille pattern base: U+2800
    private const int BRAILLE_BASE = 0x2800;

    // Dot positions (bits)
    private static readonly int[,] DotPositions =
    {
        { 0, 3 }, // Dots 1, 4
        { 1, 4 }, // Dots 2, 5
        { 2, 5 }, // Dots 3, 6
        { 6, 7 }  // Dots 7, 8
    };

    public static char GetBrailleChar(bool[,] pixels)
    {
        // pixels is 2x4 array (width x height)
        if (pixels.GetLength(0) != 2 || pixels.GetLength(1) != 4)
            throw new ArgumentException("Braille pixel array must be 2x4");

        int pattern = 0;

        // Map pixels to braille dots
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                if (pixels[x, y])
                {
                    int dotIndex = y * 2 + x;
                    pattern |= (1 << dotIndex);
                }
            }
        }

        return (char)(BRAILLE_BASE + pattern);
    }

    public static char GetBrailleFromByte(byte pattern)
    {
        // Direct mapping from 8-bit pattern to braille character
        return (char)(BRAILLE_BASE + pattern);
    }

    // Convert frequency bar height (0-8) to braille character
    public static char GetBrailleBar(int height)
    {
        // height: 0-8 (0 = empty, 8 = full)
        height = Math.Clamp(height, 0, 8);

        // Braille patterns for vertical bars
        var patterns = new[]
        {
            0x00, // Empty: ⠀
            0x40, // 1/8:   ⡀
            0x44, // 2/8:   ⡄
            0x46, // 3/8:   ⡆
            0x47, // 4/8:   ⡇
            0xC7, // 5/8:   ⣇
            0xCF, // 6/8:   ⣏
            0xDF, // 7/8:   ⣟
            0xFF  // 8/8:   ⣿ (full)
        };

        return (char)(BRAILLE_BASE + patterns[height]);
    }
}
```

#### Block Character Rendering

```csharp
public class BlockCharacterRenderer
{
    // Unicode block elements
    private static readonly char[] VerticalBlocks =
    {
        ' ',  // 0/8 empty
        '▁',  // 1/8
        '▂',  // 2/8
        '▃',  // 3/8
        '▄',  // 4/8
        '▅',  // 5/8
        '▆',  // 6/8
        '▇',  // 7/8
        '█'   // 8/8 full
    };

    public static char GetVerticalBlock(double value)
    {
        // value: 0.0 to 1.0
        int index = (int)(value * 8);
        index = Math.Clamp(index, 0, 8);
        return VerticalBlocks[index];
    }

    public static char GetHorizontalBlock(double value)
    {
        // Horizontal blocks (left to right)
        var blocks = new[] { ' ', '▏', '▎', '▍', '▌', '▋', '▊', '▉', '█' };
        int index = (int)(value * 8);
        index = Math.Clamp(index, 0, 8);
        return blocks[index];
    }
}
```

#### Spectrum Visualizer with Braille Characters

```csharp
public class BrailleSpectrumVisualizer
{
    private readonly int _width;
    private readonly int _height;

    public BrailleSpectrumVisualizer(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public string RenderSpectrum(double[] spectrum)
    {
        // Downsample spectrum to terminal width
        var bars = DownsampleSpectrum(spectrum, _width);

        // Normalize to 0-1 range
        double max = bars.Max();
        if (max > 0)
        {
            for (int i = 0; i < bars.Length; i++)
                bars[i] /= max;
        }

        // Render using braille bars
        var sb = new StringBuilder();

        for (int y = _height - 1; y >= 0; y--)
        {
            for (int x = 0; x < _width; x++)
            {
                double barHeight = bars[x] * _height;
                int charHeight = (int)((barHeight - y) * 8);
                charHeight = Math.Clamp(charHeight, 0, 8);

                char brailleChar = BrailleRenderer.GetBrailleBar(charHeight);
                sb.Append(brailleChar);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private double[] DownsampleSpectrum(double[] spectrum, int targetWidth)
    {
        // Use logarithmic scaling for better visualization
        return LogarithmicFrequencyScaler.ScaleToLogarithmic(
            spectrum, targetWidth, 44100);
    }
}
```

#### Waveform Visualizer with Block Characters

```csharp
public class WaveformVisualizer
{
    private readonly int _width;
    private readonly int _height;

    public WaveformVisualizer(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public string RenderWaveform(float[] samples)
    {
        // Downsample to terminal width
        var downsampled = WaveformDownsampler.DownsampleMinMax(samples, _width);

        var sb = new StringBuilder();
        int centerY = _height / 2;

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                float min = downsampled[x * 2];
                float max = downsampled[x * 2 + 1];

                // Scale to terminal height
                int minY = (int)((min + 1.0f) / 2.0f * _height);
                int maxY = (int)((max + 1.0f) / 2.0f * _height);

                if (y >= minY && y <= maxY)
                    sb.Append('█');
                else if (y == centerY)
                    sb.Append('─'); // Center line
                else
                    sb.Append(' ');
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
```

### 3.5 Update Rate Considerations for Smooth Visualization

#### Frame Rate Management

```csharp
public class VisualizationFrameRateManager
{
    private readonly int _targetFps;
    private readonly TimeSpan _frameInterval;
    private readonly Stopwatch _stopwatch = new();
    private DateTime _lastUpdate = DateTime.MinValue;

    public VisualizationFrameRateManager(int targetFps = 30)
    {
        _targetFps = targetFps;
        _frameInterval = TimeSpan.FromMilliseconds(1000.0 / targetFps);
    }

    public bool ShouldUpdate()
    {
        var now = DateTime.UtcNow;
        var elapsed = now - _lastUpdate;

        if (elapsed >= _frameInterval)
        {
            _lastUpdate = now;
            return true;
        }

        return false;
    }

    public async Task WaitForNextFrameAsync()
    {
        var now = DateTime.UtcNow;
        var elapsed = now - _lastUpdate;
        var delay = _frameInterval - elapsed;

        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay);
        }

        _lastUpdate = DateTime.UtcNow;
    }
}
```

#### Throttled Audio Visualization

```csharp
public class ThrottledAudioVisualizer : IDisposable
{
    private readonly RealTimeSpectrumAnalyzer _analyzer;
    private readonly BrailleSpectrumVisualizer _visualizer;
    private readonly VisualizationFrameRateManager _frameRateManager;
    private readonly IDisposable _subscription;

    public event EventHandler<string>? VisualizationUpdated;

    public ThrottledAudioVisualizer(int terminalWidth, int terminalHeight, int targetFps = 30)
    {
        _analyzer = new RealTimeSpectrumAnalyzer();
        _visualizer = new BrailleSpectrumVisualizer(terminalWidth, terminalHeight);
        _frameRateManager = new VisualizationFrameRateManager(targetFps);

        // Subscribe to spectrum updates with throttling
        _subscription = Observable
            .FromEventPattern<double[]>(
                h => _analyzer.SpectrumUpdated += h,
                h => _analyzer.SpectrumUpdated -= h)
            .Sample(TimeSpan.FromMilliseconds(1000.0 / targetFps)) // Throttle updates
            .ObserveOn(TaskPoolScheduler.Default) // Process on background thread
            .Subscribe(evt =>
            {
                var visualization = _visualizer.RenderSpectrum(evt.EventArgs);
                VisualizationUpdated?.Invoke(this, visualization);
            });
    }

    public void Start() => _analyzer.Start();
    public void Stop() => _analyzer.Stop();

    public void Dispose()
    {
        _subscription?.Dispose();
        _analyzer?.Dispose();
    }
}
```

#### Adaptive Frame Rate Based on Performance

```csharp
public class AdaptiveFrameRateVisualizer
{
    private int _currentFps = 30;
    private readonly Queue<TimeSpan> _renderTimes = new();
    private const int SampleCount = 10;

    public void RecordRenderTime(TimeSpan renderTime)
    {
        _renderTimes.Enqueue(renderTime);

        if (_renderTimes.Count > SampleCount)
            _renderTimes.Dequeue();

        AdjustFrameRate();
    }

    private void AdjustFrameRate()
    {
        if (_renderTimes.Count < SampleCount)
            return;

        var avgRenderTime = TimeSpan.FromTicks((long)_renderTimes.Average(t => t.Ticks));
        var targetFrameTime = TimeSpan.FromMilliseconds(1000.0 / _currentFps);

        // If rendering takes >80% of frame time, reduce FPS
        if (avgRenderTime > targetFrameTime * 0.8)
        {
            _currentFps = Math.Max(10, _currentFps - 5);
            Console.WriteLine($"Reducing FPS to {_currentFps}");
        }
        // If rendering takes <50% of frame time, increase FPS
        else if (avgRenderTime < targetFrameTime * 0.5)
        {
            _currentFps = Math.Min(60, _currentFps + 5);
            Console.WriteLine($"Increasing FPS to {_currentFps}");
        }
    }

    public int GetCurrentFps() => _currentFps;
}
```

---

## 4. Implementation Recommendations

### 4.1 FFmpeg/OpenCvSharp Integration

**Recommended Approach:**

1. **Use FFMediaToolkit instead of OpenCvSharp** for unified video+audio extraction
   - Better audio support
   - More control over pixel formats
   - Cross-platform compatibility

2. **Implement producer-consumer pattern** for thread-safe frame decoding
   - Decode on background thread
   - Use `ConcurrentQueue<Mat>` for frame buffering
   - Marshal frames to UI thread using `Application.MainLoop.Invoke()`

3. **Use ArrayPool<byte>** for frame buffer management
   - Reduces GC pressure
   - Improves performance for high-resolution video

4. **Implement hybrid seeking**
   - Use keyframe seeking for fast scrubbing
   - Use sequential frame reading for accurate positioning

**Code Template:**

```csharp
public class VideoDecoderService
{
    private readonly ConcurrentQueue<MediaFrame> _frameQueue = new();
    private readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;
    private CancellationTokenSource? _decodeCts;

    public async Task StartDecodingAsync(string videoPath, CancellationToken ct)
    {
        _decodeCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        await Task.Run(() =>
        {
            using var file = MediaFile.Open(videoPath);
            var buffer = _bufferPool.Rent(file.Video.FrameByteCount);

            try
            {
                while (file.Video.TryGetNextFrame(buffer) && !_decodeCts.Token.IsCancellationRequested)
                {
                    var frameCopy = new byte[file.Video.FrameByteCount];
                    Buffer.BlockCopy(buffer, 0, frameCopy, 0, frameCopy.Length);

                    var frame = new MediaFrame(frameCopy, /* ... */);
                    _frameQueue.Enqueue(frame);

                    // Backpressure: wait if queue is full
                    while (_frameQueue.Count > 60 && !_decodeCts.Token.IsCancellationRequested)
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            finally
            {
                _bufferPool.Return(buffer);
            }
        }, _decodeCts.Token);
    }
}
```

### 4.2 ReactiveUI + Terminal.Gui Integration

**Recommended Patterns:**

1. **Initialize ReactiveUI schedulers** at application startup
2. **Use `[Reactive]` attributes** for ViewModel properties (requires Fody)
3. **Use `WhenAnyValue` for property observation**
4. **Use `BindTo` for one-way bindings**
5. **Use `InvokeCommand` for command execution**
6. **Always marshal UI updates** to main thread with `ObserveOn(RxApp.MainThreadScheduler)`

**Code Template:**

```csharp
// Startup
Application.Init();
RxApp.MainThreadScheduler = TerminalScheduler.Default;
RxApp.TaskpoolScheduler = TaskPoolScheduler.Default;

// ViewModel
public class MediaPlayerViewModel : ReactiveObject
{
    [Reactive] public PlaybackState State { get; private set; }

    public ReactiveCommand<Unit, Unit> PlayCommand { get; }

    public MediaPlayerViewModel(IMediaService mediaService)
    {
        var canPlay = this.WhenAnyValue(x => x.State, s => s == PlaybackState.Stopped);
        PlayCommand = ReactiveCommand.CreateFromTask(mediaService.PlayAsync, canPlay);

        mediaService.PlaybackState
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(state => State = state);
    }
}

// View
public class MediaPlayerView : Window
{
    private void SetupBindings()
    {
        _viewModel.WhenAnyValue(vm => vm.State)
            .Select(s => s.ToString())
            .BindTo(_stateLabel, v => v.Text);

        this.BindCommand(_viewModel, vm => vm.PlayCommand, v => v._playButton, nameof(Button.Clicked));
    }
}
```

### 4.3 Audio Visualization

**Recommended Approach:**

1. **Use FftSharp** for FFT calculations (MIT license, no dependencies)
2. **Apply Hann window** before FFT to reduce spectral leakage
3. **Use logarithmic frequency scaling** for better visualization
4. **Throttle updates to 30 FPS** for smooth rendering
5. **Use braille characters** for fallback, block characters for better terminals

**Code Template:**

```csharp
public class AudioVisualizerService
{
    private readonly int _fftSize = 2048;
    private readonly BrailleSpectrumVisualizer _visualizer;

    public IObservable<string> VisualizationStream { get; }

    public AudioVisualizerService(int terminalWidth, int terminalHeight)
    {
        _visualizer = new BrailleSpectrumVisualizer(terminalWidth, terminalHeight);

        VisualizationStream = Observable
            .Interval(TimeSpan.FromMilliseconds(33)) // 30 FPS
            .Select(_ => CaptureAudioSamples())
            .Select(samples => ComputeFFT(samples))
            .Select(spectrum => _visualizer.RenderSpectrum(spectrum))
            .ObserveOn(RxApp.MainThreadScheduler);
    }

    private double[] ComputeFFT(float[] samples)
    {
        var audioBuffer = samples.Select(s => (double)s).ToArray();
        var window = new FftSharp.Windows.Hanning();
        window.ApplyInPlace(audioBuffer);

        var fft = FftSharp.Transform.FFT(audioBuffer);
        return FftSharp.Transform.FFTmagnitude(fft);
    }
}
```

### 4.4 Performance Optimization Checklist

- [ ] Use `ArrayPool<byte>.Shared` for frame buffers
- [ ] Implement frame queue with backpressure (max 60 frames)
- [ ] Throttle UI updates to 30 FPS using `Sample()`
- [ ] Offload heavy processing to background threads with `ObserveOn(TaskPoolScheduler.Default)`
- [ ] Always dispose `Mat` objects and `MediaFile` instances
- [ ] Use logarithmic frequency scaling for spectrum visualization
- [ ] Apply windowing function (Hann) before FFT
- [ ] Clone `Mat` objects when passing between threads
- [ ] Use `Application.MainLoop.Invoke()` for Terminal.Gui updates from background threads
- [ ] Dispose ReactiveUI subscriptions with `CompositeDisposable`

### 4.5 Common Pitfalls and How to Avoid Them

| Pitfall | Problem | Solution |
|---------|---------|----------|
| **OpenCvSharp memory leaks** | `Mat` objects not disposed | Use `using` statements or explicit `Dispose()` |
| **Thread safety violations** | VideoCapture accessed from multiple threads | Use mutex or producer-consumer pattern |
| **UI thread blocking** | Heavy FFT computation on UI thread | Use `ObserveOn(TaskPoolScheduler.Default)` |
| **Excessive GC pressure** | Creating new byte arrays for each frame | Use `ArrayPool<byte>.Shared` |
| **Jerky visualization** | Too frequent UI updates | Throttle with `Sample()` or `Throttle()` |
| **Spectral leakage** | No windowing before FFT | Apply Hann or Hamming window |
| **Subscription leaks** | ReactiveUI subscriptions not disposed | Use `DisposeWith()` or `WhenActivated()` |
| **Terminal.Gui crashes** | UI updated from background thread | Use `Application.MainLoop.Invoke()` |
| **Inaccurate seeking** | Relying on keyframe seeking | Use hybrid approach with sequential reads |
| **Missing FFmpeg DLLs** | OpenCvSharp can't load codecs | Check for DLL availability at startup |

---

## 5. References

### Libraries

- **OpenCvSharp**: <https://github.com/shimat/opencvsharp>
- **FFMediaToolkit**: <https://github.com/radek-k/FFMediaToolkit>
- **NAudio**: <https://github.com/naudio/NAudio>
- **FftSharp**: <https://github.com/swharden/FftSharp>
- **ReactiveUI**: <https://www.reactiveui.net/>
- **Terminal.Gui**: <https://github.com/gui-cs/Terminal.Gui>
- **Drawille.NET**: <https://github.com/Jjagg/Drawille.NET>

### Documentation

- **OpenCvSharp VideoCapture API**: <https://shimat.github.io/opencvsharp_docs/html/a520e720-deb7-5610-0375-a619170376d0.htm>
- **ReactiveUI with Terminal.Gui Example**: <https://github.com/gui-cs/Terminal.Gui/tree/main/ReactiveExample>
- **FFT Tutorial for C#**: <https://swharden.com/csdv/audio/fft/>
- **Braille Patterns Unicode**: <https://en.wikipedia.org/wiki/Braille_Patterns>
- **ArrayPool Documentation**: <https://learn.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1>

### Research Articles

- **Understanding FFT Windows**: <https://download.ni.com/evaluation/pxi/Understanding%20FFTs%20and%20Windowing.pdf>
- **Terminal Graphics with Braille**: <https://medium.com/@kevrone/using-go-and-braille-to-render-images-and-video-to-a-terminal-edc8ecfba50>
- **Memory Optimization in .NET**: <https://en.ittrip.xyz/c-sharp/csharp-memory-optimization>

---

**Document Version**: 1.0.0
**Last Updated**: 2025-10-26
**Author**: Claude
**Status**: Complete
**Related Documents**: UNIFIED_MEDIA_PLAYER_PLAN.md
