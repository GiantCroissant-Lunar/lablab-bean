using System;
using System.Collections.Generic;
using System.Threading;

namespace LablabBean.Contracts.PersistentStorage;

public readonly struct StorageRequest : IEquatable<StorageRequest>
{
    public readonly string Key;
    public readonly StorageProviderType ProviderType;
    public readonly StorageOperationType OperationType;
    public readonly StoragePriority Priority;
    public readonly bool Encrypt;
    public readonly bool Compress;
    public readonly CancellationToken CancellationToken;
    public readonly IReadOnlyDictionary<string, object> Metadata;

    public StorageRequest(
        string key,
        StorageProviderType providerType,
        StorageOperationType operationType,
        StoragePriority priority = StoragePriority.Normal,
        bool encrypt = false,
        bool compress = false,
        CancellationToken cancellationToken = default,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        Key = key;
        ProviderType = providerType;
        OperationType = operationType;
        Priority = priority;
        Encrypt = encrypt;
        Compress = compress;
        CancellationToken = cancellationToken;
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    public static StorageRequest Save(string key, StorageProviderType providerType = StorageProviderType.LiteDB)
        => new(key, providerType, StorageOperationType.Save);

    public static StorageRequest Load(string key, StorageProviderType providerType = StorageProviderType.LiteDB)
        => new(key, providerType, StorageOperationType.Load);

    public static StorageRequest Delete(string key, StorageProviderType providerType = StorageProviderType.LiteDB)
        => new(key, providerType, StorageOperationType.Delete);

    public static StorageRequest SaveEncrypted(string key, StorageProviderType providerType = StorageProviderType.LiteDB)
        => new(key, providerType, StorageOperationType.Save, encrypt: true);

    public bool Equals(StorageRequest other)
        => Key == other.Key && ProviderType == other.ProviderType &&
           OperationType == other.OperationType && Priority == other.Priority &&
           Encrypt == other.Encrypt && Compress == other.Compress;

    public override bool Equals(object? obj) => obj is StorageRequest other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Key, (int)ProviderType, (int)OperationType, (int)Priority);
    public override string ToString() => $"StorageRequest(Key: {Key}, Provider: {ProviderType}, Operation: {OperationType}, Priority: {Priority})";
}

public readonly struct StorageStatistics
{
    public readonly int TotalKeys;
    public readonly long TotalSizeBytes;
    public readonly int ActiveProviders;
    public readonly DateTime LastSave;
    public readonly DateTime LastLoad;
    public readonly DateTime LastSync;
    public readonly int SaveOperations;
    public readonly int LoadOperations;
    public readonly int DeleteOperations;
    public readonly IReadOnlyDictionary<StorageProviderType, int> OperationsByProvider;
    public readonly IReadOnlyDictionary<StorageProviderType, long> SizeByProvider;
    public readonly IReadOnlyDictionary<StorageProviderType, bool> HealthByProvider;

    public StorageStatistics(
        int totalKeys, long totalSizeBytes, int activeProviders,
        DateTime lastSave, DateTime lastLoad, DateTime lastSync,
        int saveOperations, int loadOperations, int deleteOperations,
        IReadOnlyDictionary<StorageProviderType, int>? operationsByProvider = null,
        IReadOnlyDictionary<StorageProviderType, long>? sizeByProvider = null,
        IReadOnlyDictionary<StorageProviderType, bool>? healthByProvider = null)
    {
        TotalKeys = totalKeys;
        TotalSizeBytes = totalSizeBytes;
        ActiveProviders = activeProviders;
        LastSave = lastSave;
        LastLoad = lastLoad;
        LastSync = lastSync;
        SaveOperations = saveOperations;
        LoadOperations = loadOperations;
        DeleteOperations = deleteOperations;
        OperationsByProvider = operationsByProvider ?? new Dictionary<StorageProviderType, int>();
        SizeByProvider = sizeByProvider ?? new Dictionary<StorageProviderType, long>();
        HealthByProvider = healthByProvider ?? new Dictionary<StorageProviderType, bool>();
    }

    public float TotalSizeMB => TotalSizeBytes / 1024f / 1024f;
    public int TotalOperations => SaveOperations + LoadOperations + DeleteOperations;
    public float AverageOperationsPerProvider => ActiveProviders > 0 ? (float)TotalOperations / ActiveProviders : 0f;

    public override string ToString()
        => $"StorageStats(Keys: {TotalKeys}, Size: {TotalSizeMB:F2}MB, Providers: {ActiveProviders}, Operations: {TotalOperations})";
}

public readonly struct StorageDebugInfo
{
    public readonly bool IsHealthy;
    public readonly string? LastError;
    public readonly int LoadedKeys;
    public readonly int ActiveProviders;
    public readonly DateTime LastOperation;
    public readonly IReadOnlyList<string> ProviderNames;
    public readonly IReadOnlyDictionary<string, object> ProviderInfo;
    public readonly long CacheMemoryUsage;
    public readonly int PendingOperations;
    public readonly IReadOnlyDictionary<StorageProviderType, bool> ProviderAvailability;

    public StorageDebugInfo(
        bool isHealthy, string? lastError, int loadedKeys, int activeProviders, DateTime lastOperation,
        IReadOnlyList<string>? providerNames = null, IReadOnlyDictionary<string, object>? providerInfo = null,
        long cacheMemoryUsage = 0, int pendingOperations = 0,
        IReadOnlyDictionary<StorageProviderType, bool>? providerAvailability = null)
    {
        IsHealthy = isHealthy;
        LastError = lastError;
        LoadedKeys = loadedKeys;
        ActiveProviders = activeProviders;
        LastOperation = lastOperation;
        ProviderNames = providerNames ?? Array.Empty<string>();
        ProviderInfo = providerInfo ?? new Dictionary<string, object>();
        CacheMemoryUsage = cacheMemoryUsage;
        PendingOperations = pendingOperations;
        ProviderAvailability = providerAvailability ?? new Dictionary<StorageProviderType, bool>();
    }

    public float CacheMemoryUsageMB => CacheMemoryUsage / 1024f / 1024f;
    public bool HasAvailableProviders => ActiveProviders > 0;

    public override string ToString()
    {
        var healthStatus = IsHealthy ? "Healthy" : $"Unhealthy ({LastError})";
        return $"StorageDebug({healthStatus}, Providers: {ActiveProviders}, Keys: {LoadedKeys}, Cache: {CacheMemoryUsageMB:F2}MB)";
    }
}
