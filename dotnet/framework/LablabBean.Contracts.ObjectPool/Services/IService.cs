using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LablabBean.Contracts.ObjectPool.Services;

/// <summary>
/// Object pooling service interface for efficient object reuse and memory management.
/// Provides pooling for C# objects with automatic lifecycle management.
/// </summary>
public interface IService
{
    /// <summary>
    /// Create a new object pool for the specified type
    /// </summary>
    Task<IObjectPool<T>> CreatePoolAsync<T>(
        Func<T> createFunc,
        Action<T>? resetAction,
        Action<T>? destroyAction,
        int maxSize,
        int preallocateCount,
        CancellationToken cancellationToken) where T : class;

    /// <summary>
    /// Get an existing object pool by type and identifier
    /// </summary>
    IObjectPool<T>? GetPool<T>(string? identifier) where T : class;

    /// <summary>
    /// Remove and destroy an object pool
    /// </summary>
    Task DestroyPoolAsync<T>(string? identifier, CancellationToken cancellationToken) where T : class;

    /// <summary>
    /// Clear all objects from a pool without destroying the pool itself
    /// </summary>
    Task ClearPoolAsync<T>(string? identifier, CancellationToken cancellationToken) where T : class;

    /// <summary>
    /// Get statistics for all managed pools
    /// </summary>
    ObjectPoolStatistics GetStatistics();

    /// <summary>
    /// Get statistics for a specific pool
    /// </summary>
    PoolStatistics GetPoolStatistics<T>(string? identifier) where T : class;

    /// <summary>
    /// Get all active pools managed by this service
    /// </summary>
    IReadOnlyList<PoolInfo> GetActivePools();

    /// <summary>
    /// Configure global object pool settings
    /// </summary>
    void ConfigureGlobalSettings(ObjectPoolGlobalSettings settings);

    /// <summary>
    /// Get current global object pool settings
    /// </summary>
    ObjectPoolGlobalSettings GetGlobalSettings();

    /// <summary>
    /// Perform cleanup operations on all pools
    /// </summary>
    Task PerformCleanupAsync(bool aggressiveCleanup, CancellationToken cancellationToken);
}