using System;

namespace LablabBean.Contracts.ObjectPool;

/// <summary>
/// Statistics for an individual object pool
/// </summary>
public readonly struct PoolStatistics
{
    public readonly string Identifier;
    public readonly Type? ObjectType;
    public readonly int MaxSize;
    public readonly int TotalObjects;
    public readonly int ActiveObjects;
    public readonly int InactiveObjects;
    public readonly long GetOperations;
    public readonly long ReturnOperations;
    public readonly long ObjectsCreated;
    public readonly long ObjectsDestroyed;
    public readonly long EstimatedMemoryUsage;
    public readonly DateTime CreatedAt;
    public readonly DateTime LastGetAt;
    public readonly DateTime LastReturnAt;

    public PoolStatistics(
        string identifier,
        Type? objectType,
        int maxSize,
        int totalObjects,
        int activeObjects,
        int inactiveObjects,
        long getOperations,
        long returnOperations,
        long objectsCreated,
        long objectsDestroyed,
        long estimatedMemoryUsage,
        DateTime createdAt,
        DateTime lastGetAt,
        DateTime lastReturnAt)
    {
        Identifier = identifier;
        ObjectType = objectType;
        MaxSize = maxSize;
        TotalObjects = totalObjects;
        ActiveObjects = activeObjects;
        InactiveObjects = inactiveObjects;
        GetOperations = getOperations;
        ReturnOperations = returnOperations;
        ObjectsCreated = objectsCreated;
        ObjectsDestroyed = objectsDestroyed;
        EstimatedMemoryUsage = estimatedMemoryUsage;
        CreatedAt = createdAt;
        LastGetAt = lastGetAt;
        LastReturnAt = lastReturnAt;
    }

    public float UtilizationPercentage => TotalObjects > 0 ? (float)ActiveObjects / TotalObjects * 100f : 0f;
    public float FillPercentage => MaxSize > 0 ? (float)TotalObjects / MaxSize * 100f : 0f;
    public float HitRatio => GetOperations > 0 ? (float)(GetOperations - ObjectsCreated) / GetOperations * 100f : 0f;
    public float MemoryUsageMB => EstimatedMemoryUsage / 1024f / 1024f;
    public TimeSpan Age => DateTime.UtcNow - CreatedAt;
    public TimeSpan TimeSinceLastGet => DateTime.UtcNow - LastGetAt;
    public TimeSpan TimeSinceLastReturn => DateTime.UtcNow - LastReturnAt;

    public override string ToString()
    {
        return $"PoolStats({ObjectType?.Name ?? "Unknown"}[{Identifier}]: {ActiveObjects}/{TotalObjects}, " +
               $"Util: {UtilizationPercentage:F1}%, Hit: {HitRatio:F1}%, Memory: {MemoryUsageMB:F2}MB)";
    }
}
