namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Performance thresholds for triggering alerts.
/// </summary>
public class PerformanceThresholds
{
    /// <summary>
    /// Minimum acceptable frame rate.
    /// </summary>
    public float MinFrameRate { get; set; } = 30.0f;

    /// <summary>
    /// Maximum acceptable frame time in milliseconds.
    /// </summary>
    public float MaxFrameTime { get; set; } = 33.33f; // ~30 FPS

    /// <summary>
    /// Maximum acceptable CPU usage percentage.
    /// </summary>
    public float MaxCpuUsage { get; set; } = 80.0f;

    /// <summary>
    /// Maximum acceptable GPU usage percentage.
    /// </summary>
    public float MaxGpuUsage { get; set; } = 90.0f;

    /// <summary>
    /// Maximum acceptable memory usage in bytes.
    /// </summary>
    public long MaxMemoryUsage { get; set; } = 1024L * 1024L * 1024L; // 1 GB

    /// <summary>
    /// Maximum acceptable graphics memory usage in bytes.
    /// </summary>
    public long MaxGraphicsMemoryUsage { get; set; } = 512L * 1024L * 1024L; // 512 MB

    /// <summary>
    /// Maximum acceptable main thread time in milliseconds.
    /// </summary>
    public float MaxMainThreadTime { get; set; } = 16.67f; // ~60 FPS

    /// <summary>
    /// Maximum acceptable render thread time in milliseconds.
    /// </summary>
    public float MaxRenderThreadTime { get; set; } = 16.67f; // ~60 FPS

    /// <summary>
    /// Maximum acceptable GPU time in milliseconds.
    /// </summary>
    public float MaxGpuTime { get; set; } = 16.67f; // ~60 FPS

    /// <summary>
    /// Maximum acceptable GC time in milliseconds.
    /// </summary>
    public float MaxGcTime { get; set; } = 5.0f;

    /// <summary>
    /// Maximum acceptable number of draw calls.
    /// </summary>
    public int MaxDrawCalls { get; set; } = 2000;

    /// <summary>
    /// Maximum acceptable number of batches.
    /// </summary>
    public int MaxBatches { get; set; } = 1000;

    /// <summary>
    /// Maximum acceptable CPU temperature in Celsius.
    /// </summary>
    public float? MaxCpuTemperature { get; set; } = 85.0f;

    /// <summary>
    /// Maximum acceptable GPU temperature in Celsius.
    /// </summary>
    public float? MaxGpuTemperature { get; set; } = 90.0f;

    /// <summary>
    /// Create default performance thresholds.
    /// </summary>
    public static PerformanceThresholds CreateDefault()
    {
        return new PerformanceThresholds();
    }

    /// <summary>
    /// Create conservative performance thresholds for low-end devices.
    /// </summary>
    public static PerformanceThresholds CreateConservative()
    {
        return new PerformanceThresholds
        {
            MinFrameRate = 20.0f,
            MaxFrameTime = 50.0f, // ~20 FPS
            MaxCpuUsage = 70.0f,
            MaxGpuUsage = 80.0f,
            MaxMemoryUsage = 512L * 1024L * 1024L, // 512 MB
            MaxGraphicsMemoryUsage = 256L * 1024L * 1024L, // 256 MB
            MaxMainThreadTime = 25.0f, // ~40 FPS
            MaxRenderThreadTime = 25.0f, // ~40 FPS
            MaxGpuTime = 25.0f, // ~40 FPS
            MaxGcTime = 10.0f,
            MaxDrawCalls = 1000,
            MaxBatches = 500
        };
    }

    /// <summary>
    /// Create aggressive performance thresholds for high-end devices.
    /// </summary>
    public static PerformanceThresholds CreateAggressive()
    {
        return new PerformanceThresholds
        {
            MinFrameRate = 60.0f,
            MaxFrameTime = 16.67f, // ~60 FPS
            MaxCpuUsage = 90.0f,
            MaxGpuUsage = 95.0f,
            MaxMemoryUsage = 2048L * 1024L * 1024L, // 2 GB
            MaxGraphicsMemoryUsage = 1024L * 1024L * 1024L, // 1 GB
            MaxMainThreadTime = 8.33f, // ~120 FPS
            MaxRenderThreadTime = 8.33f, // ~120 FPS
            MaxGpuTime = 8.33f, // ~120 FPS
            MaxGcTime = 2.0f,
            MaxDrawCalls = 5000,
            MaxBatches = 2000
        };
    }
}
