using System;

namespace LablabBean.Contracts.ObjectPool;

/// <summary>
/// Global settings for the object pool system
/// </summary>
[Serializable]
public class ObjectPoolGlobalSettings
{
    /// <summary>
    /// Default maximum size for new pools (0 = unlimited)
    /// </summary>
    public int DefaultMaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Default preallocate count for new pools
    /// </summary>
    public int DefaultPreallocateCount { get; set; } = 0;

    /// <summary>
    /// Whether to automatically clean up unused pools
    /// </summary>
    public bool AutoCleanupEnabled { get; set; } = true;

    /// <summary>
    /// How long a pool must be unused before it's considered for cleanup (in seconds)
    /// </summary>
    public float UnusedPoolCleanupThreshold { get; set; } = 300f;

    /// <summary>
    /// How often to perform automatic cleanup (in seconds)
    /// </summary>
    public float AutoCleanupInterval { get; set; } = 60f;

    /// <summary>
    /// Whether to track detailed statistics (may have performance impact)
    /// </summary>
    public bool EnableDetailedStatistics { get; set; } = true;

    /// <summary>
    /// Whether to log pool operations for debugging
    /// </summary>
    public bool EnableDebugLogging { get; set; } = false;

    /// <summary>
    /// Maximum memory usage before triggering aggressive cleanup (in MB, 0 = no limit)
    /// </summary>
    public float MaxMemoryUsageMB { get; set; } = 0f;

    /// <summary>
    /// Whether to warn when pools exceed their maximum size
    /// </summary>
    public bool WarnOnPoolOverflow { get; set; } = true;

    /// <summary>
    /// Whether to automatically expand pools beyond their maximum size when needed
    /// </summary>
    public bool AllowPoolExpansion { get; set; } = false;

    /// <summary>
    /// Create default settings
    /// </summary>
    public static ObjectPoolGlobalSettings Default => new();

    /// <summary>
    /// Create performance-optimized settings (less tracking, more aggressive cleanup)
    /// </summary>
    public static ObjectPoolGlobalSettings Performance => new()
    {
        DefaultMaxPoolSize = 50,
        DefaultPreallocateCount = 5,
        AutoCleanupEnabled = true,
        UnusedPoolCleanupThreshold = 120f,
        AutoCleanupInterval = 30f,
        EnableDetailedStatistics = false,
        EnableDebugLogging = false,
        MaxMemoryUsageMB = 100f,
        WarnOnPoolOverflow = false,
        AllowPoolExpansion = true
    };

    /// <summary>
    /// Create memory-conscious settings (smaller pools, aggressive cleanup)
    /// </summary>
    public static ObjectPoolGlobalSettings MemoryConscious => new()
    {
        DefaultMaxPoolSize = 25,
        DefaultPreallocateCount = 0,
        AutoCleanupEnabled = true,
        UnusedPoolCleanupThreshold = 60f,
        AutoCleanupInterval = 15f,
        EnableDetailedStatistics = true,
        EnableDebugLogging = false,
        MaxMemoryUsageMB = 50f,
        WarnOnPoolOverflow = true,
        AllowPoolExpansion = false
    };

    /// <summary>
    /// Create development settings (detailed tracking, logging enabled)
    /// </summary>
    public static ObjectPoolGlobalSettings Development => new()
    {
        DefaultMaxPoolSize = 20,
        DefaultPreallocateCount = 2,
        AutoCleanupEnabled = false,
        UnusedPoolCleanupThreshold = 600f,
        AutoCleanupInterval = 120f,
        EnableDetailedStatistics = true,
        EnableDebugLogging = true,
        MaxMemoryUsageMB = 0f,
        WarnOnPoolOverflow = true,
        AllowPoolExpansion = true
    };

    public override string ToString()
    {
        return $"ObjectPoolGlobalSettings(MaxSize: {DefaultMaxPoolSize}, Prealloc: {DefaultPreallocateCount}, " +
               $"AutoCleanup: {AutoCleanupEnabled}, Statistics: {EnableDetailedStatistics})";
    }
}
