using System.Collections.Generic;

namespace LablabBean.Contracts.Audio;

/// <summary>
/// Statistics and metrics for audio system performance
/// </summary>
public readonly struct AudioStats
{
    /// <summary>
    /// Total number of currently playing audio instances
    /// </summary>
    public readonly int ActiveAudioInstances;

    /// <summary>
    /// Number of audio instances by category
    /// </summary>
    public readonly IReadOnlyDictionary<AudioCategory, int> InstancesByCategory;

    /// <summary>
    /// Number of audio instances by source type
    /// </summary>
    public readonly IReadOnlyDictionary<AudioSourceType, int> InstancesBySourceType;

    /// <summary>
    /// Total number of audio sources available
    /// </summary>
    public readonly int TotalAudioSources;

    /// <summary>
    /// Number of audio sources currently in use
    /// </summary>
    public readonly int UsedAudioSources;

    /// <summary>
    /// Current volume levels for each category
    /// </summary>
    public readonly IReadOnlyDictionary<AudioCategory, float> CategoryVolumes;

    /// <summary>
    /// Memory usage in bytes for loaded audio clips
    /// </summary>
    public readonly long MemoryUsageBytes;

    /// <summary>
    /// Number of audio clips currently loaded in memory
    /// </summary>
    public readonly int LoadedAudioClips;

    public AudioStats(
        int activeAudioInstances,
        IReadOnlyDictionary<AudioCategory, int> instancesByCategory,
        IReadOnlyDictionary<AudioSourceType, int> instancesBySourceType,
        int totalAudioSources,
        int usedAudioSources,
        IReadOnlyDictionary<AudioCategory, float> categoryVolumes,
        long memoryUsageBytes,
        int loadedAudioClips)
    {
        ActiveAudioInstances = activeAudioInstances;
        InstancesByCategory = instancesByCategory ?? new Dictionary<AudioCategory, int>();
        InstancesBySourceType = instancesBySourceType ?? new Dictionary<AudioSourceType, int>();
        TotalAudioSources = totalAudioSources;
        UsedAudioSources = usedAudioSources;
        CategoryVolumes = categoryVolumes ?? new Dictionary<AudioCategory, float>();
        MemoryUsageBytes = memoryUsageBytes;
        LoadedAudioClips = loadedAudioClips;
    }

    /// <summary>
    /// Percentage of audio sources currently in use
    /// </summary>
    public float SourceUsagePercentage => TotalAudioSources > 0 ? (float)UsedAudioSources / TotalAudioSources * 100f : 0f;

    /// <summary>
    /// Memory usage in megabytes
    /// </summary>
    public float MemoryUsageMB => MemoryUsageBytes / 1024f / 1024f;

    public override string ToString()
    {
        return $"AudioStats(Active: {ActiveAudioInstances}, Sources: {UsedAudioSources}/{TotalAudioSources}, Memory: {MemoryUsageMB:F2}MB, Clips: {LoadedAudioClips})";
    }
}
