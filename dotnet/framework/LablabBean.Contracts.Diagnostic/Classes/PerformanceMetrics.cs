using System;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Performance metrics container.
/// </summary>
public class PerformanceMetrics
{
    /// <summary>
    /// Current frame rate (FPS).
    /// </summary>
    public float FrameRate { get; set; }

    /// <summary>
    /// Frame time in milliseconds.
    /// </summary>
    public float FrameTime { get; set; }

    /// <summary>
    /// CPU usage percentage (0-100).
    /// </summary>
    public float CpuUsage { get; set; }

    /// <summary>
    /// GPU usage percentage (0-100).
    /// </summary>
    public float GpuUsage { get; set; }

    /// <summary>
    /// Memory usage in bytes.
    /// </summary>
    public long MemoryUsage { get; set; }

    /// <summary>
    /// Graphics memory usage in bytes.
    /// </summary>
    public long GraphicsMemoryUsage { get; set; }

    /// <summary>
    /// Number of draw calls in the current frame.
    /// </summary>
    public int DrawCalls { get; set; }

    /// <summary>
    /// Number of vertices rendered in the current frame.
    /// </summary>
    public int Vertices { get; set; }

    /// <summary>
    /// Number of triangles rendered in the current frame.
    /// </summary>
    public int Triangles { get; set; }

    /// <summary>
    /// Number of batches in the current frame.
    /// </summary>
    public int Batches { get; set; }

    /// <summary>
    /// Main thread time in milliseconds.
    /// </summary>
    public float MainThreadTime { get; set; }

    /// <summary>
    /// Render thread time in milliseconds.
    /// </summary>
    public float RenderThreadTime { get; set; }

    /// <summary>
    /// GPU time in milliseconds.
    /// </summary>
    public float GpuTime { get; set; }

    /// <summary>
    /// Audio thread time in milliseconds.
    /// </summary>
    public float AudioThreadTime { get; set; }

    /// <summary>
    /// Physics time in milliseconds.
    /// </summary>
    public float PhysicsTime { get; set; }

    /// <summary>
    /// Garbage collection time in the current frame.
    /// </summary>
    public float GcTime { get; set; }

    /// <summary>
    /// Number of garbage collections since startup.
    /// </summary>
    public int GcCount { get; set; }

    /// <summary>
    /// Total allocated memory since startup.
    /// </summary>
    public long TotalAllocatedMemory { get; set; }

    /// <summary>
    /// Number of active GameObjects.
    /// </summary>
    public int ActiveGameObjects { get; set; }

    /// <summary>
    /// Number of loaded scenes.
    /// </summary>
    public int LoadedScenes { get; set; }

    /// <summary>
    /// Number of loaded assets.
    /// </summary>
    public int LoadedAssets { get; set; }

    /// <summary>
    /// Application uptime.
    /// </summary>
    public TimeSpan Uptime { get; set; }

    /// <summary>
    /// Target frame rate.
    /// </summary>
    public int TargetFrameRate { get; set; }

    /// <summary>
    /// VSync count.
    /// </summary>
    public int VSyncCount { get; set; }

    /// <summary>
    /// Screen resolution width.
    /// </summary>
    public int ScreenWidth { get; set; }

    /// <summary>
    /// Screen resolution height.
    /// </summary>
    public int ScreenHeight { get; set; }

    /// <summary>
    /// Screen refresh rate.
    /// </summary>
    public int ScreenRefreshRate { get; set; }

    /// <summary>
    /// Quality level.
    /// </summary>
    public int QualityLevel { get; set; }

    /// <summary>
    /// Timestamp when metrics were collected.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    }
