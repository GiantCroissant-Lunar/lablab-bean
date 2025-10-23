using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.ObjectPool;

/// <summary>
/// Overall object pool system statistics
/// </summary>
public readonly struct ObjectPoolStatistics
{
    public readonly int TotalPools;
    public readonly int TotalObjects;
    public readonly int TotalActiveObjects;
    public readonly int TotalInactiveObjects;
    public readonly long EstimatedMemoryUsage;
    public readonly IReadOnlyDictionary<string, PoolStatistics> PoolStats;
    public readonly long TotalGetOperations;
    public readonly long TotalReturnOperations;
    public readonly long TotalObjectsCreated;
    public readonly long TotalObjectsDestroyed;
    public readonly DateTime Timestamp;

    public ObjectPoolStatistics(
        int totalPools,
        int totalObjects,
        int totalActiveObjects,
        int totalInactiveObjects,
        long estimatedMemoryUsage,
        IReadOnlyDictionary<string, PoolStatistics> poolStats,
        long totalGetOperations,
        long totalReturnOperations,
        long totalObjectsCreated,
        long totalObjectsDestroyed,
        DateTime timestamp)
    {
        TotalPools = totalPools;
        TotalObjects = totalObjects;
        TotalActiveObjects = totalActiveObjects;
        TotalInactiveObjects = totalInactiveObjects;
        EstimatedMemoryUsage = estimatedMemoryUsage;
        PoolStats = poolStats ?? new Dictionary<string, PoolStatistics>();
        TotalGetOperations = totalGetOperations;
        TotalReturnOperations = totalReturnOperations;
        TotalObjectsCreated = totalObjectsCreated;
        TotalObjectsDestroyed = totalObjectsDestroyed;
        Timestamp = timestamp;
    }

    public float MemoryUsageMB => EstimatedMemoryUsage / 1024f / 1024f;
    public float UtilizationPercentage => TotalObjects > 0 ? (float)TotalActiveObjects / TotalObjects * 100f : 0f;
    public float EfficiencyPercentage => TotalObjectsCreated > 0 ? (float)(TotalGetOperations - TotalObjectsCreated) / TotalGetOperations * 100f : 0f;

    public override string ToString()
    {
        return $"ObjectPoolStats(Pools: {TotalPools}, Objects: {TotalObjects}, Active: {TotalActiveObjects}, " +
               $"Utilization: {UtilizationPercentage:F1}%, Efficiency: {EfficiencyPercentage:F1}%, Memory: {MemoryUsageMB:F2}MB)";
    }
}
