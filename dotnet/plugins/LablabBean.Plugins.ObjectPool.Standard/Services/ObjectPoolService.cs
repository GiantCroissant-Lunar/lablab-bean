using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Contracts.ObjectPool;
using LablabBean.Contracts.ObjectPool.Services;
using LablabBean.Plugins.ObjectPool.Standard.Providers;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.ObjectPool.Standard.Services;

public class ObjectPoolService : IService, IDisposable
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, object> _pools = new();
    private ObjectPoolGlobalSettings _globalSettings = ObjectPoolGlobalSettings.Default;

    public ObjectPoolService(ILogger logger)
    {
        _logger = logger;
    }

    public Task<IObjectPool<T>> CreatePoolAsync<T>(
        Func<T> createFunc,
        Action<T>? resetAction,
        Action<T>? destroyAction,
        int maxSize,
        int preallocateCount,
        CancellationToken cancellationToken) where T : class
    {
        var identifier = $"{typeof(T).Name}_{Guid.NewGuid():N}";
        var pool = new StandardObjectPool<T>(
            identifier,
            createFunc,
            resetAction,
            destroyAction,
            maxSize,
            preallocateCount,
            _logger
        );

        _pools[identifier] = pool;
        _logger.LogInformation("Created object pool {Identifier} for type {Type}", identifier, typeof(T).Name);
        
        return Task.FromResult<IObjectPool<T>>(pool);
    }

    public IObjectPool<T>? GetPool<T>(string? identifier) where T : class
    {
        if (string.IsNullOrEmpty(identifier))
        {
            // Find first pool of this type
            var key = _pools.Keys.FirstOrDefault(k => k.StartsWith(typeof(T).Name));
            if (key != null && _pools.TryGetValue(key, out var pool))
            {
                return pool as IObjectPool<T>;
            }
            return null;
        }

        if (_pools.TryGetValue(identifier, out var foundPool))
        {
            return foundPool as IObjectPool<T>;
        }
        
        return null;
    }

    public Task DestroyPoolAsync<T>(string? identifier, CancellationToken cancellationToken) where T : class
    {
        var poolToRemove = GetPool<T>(identifier);
        if (poolToRemove != null)
        {
            var key = poolToRemove.Identifier;
            if (_pools.TryRemove(key, out var pool))
            {
                (pool as IDisposable)?.Dispose();
                _logger.LogInformation("Destroyed object pool {Identifier}", key);
            }
        }
        
        return Task.CompletedTask;
    }

    public Task ClearPoolAsync<T>(string? identifier, CancellationToken cancellationToken) where T : class
    {
        var pool = GetPool<T>(identifier);
        if (pool != null)
        {
            pool.Clear();
            _logger.LogInformation("Cleared object pool {Identifier}", pool.Identifier);
        }
        
        return Task.CompletedTask;
    }

    public ObjectPoolStatistics GetStatistics()
    {
        var totalObjects = 0;
        var totalActiveObjects = 0;
        var totalInactiveObjects = 0;
        var poolStats = new Dictionary<string, PoolStatistics>();

        foreach (var kvp in _pools)
        {
            var pool = kvp.Value;
            // Type check for IObjectPool<T> interface
            var poolType = pool.GetType();
            if (poolType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IObjectPool<>)))
            {
                var countAllProp = poolType.GetProperty("CountAll");
                var countActiveProp = poolType.GetProperty("CountActive");
                var countInactiveProp = poolType.GetProperty("CountInactive");

                if (countAllProp != null && countActiveProp != null && countInactiveProp != null)
                {
                    var countAll = (int)countAllProp.GetValue(pool)!;
                    var countActive = (int)countActiveProp.GetValue(pool)!;
                    var countInactive = (int)countInactiveProp.GetValue(pool)!;

                    totalObjects += countAll;
                    totalActiveObjects += countActive;
                    totalInactiveObjects += countInactive;
                }
            }
        }

        return new ObjectPoolStatistics(
            _pools.Count,
            totalObjects,
            totalActiveObjects,
            totalInactiveObjects,
            0, // estimatedMemoryUsage
            poolStats,
            0, // totalGetOperations
            0, // totalReturnOperations
            0, // totalObjectsCreated
            0, // totalObjectsDestroyed
            DateTime.UtcNow
        );
    }

    public PoolStatistics GetPoolStatistics<T>(string? identifier) where T : class
    {
        var pool = GetPool<T>(identifier);
        if (pool != null)
        {
            return new PoolStatistics(
                pool.Identifier,
                typeof(T),
                pool.MaxSize,
                pool.CountAll,
                pool.CountActive,
                pool.CountInactive,
                0, // getOperations
                0, // returnOperations
                0, // objectsCreated
                0, // objectsDestroyed
                0, // estimatedMemoryUsage
                DateTime.UtcNow,
                DateTime.UtcNow,
                DateTime.UtcNow
            );
        }

        return default;
    }

    public IReadOnlyList<PoolInfo> GetActivePools()
    {
        var pools = new List<PoolInfo>();
        
        foreach (var kvp in _pools)
        {
            var pool = kvp.Value;
            var poolType = pool.GetType();
            
            if (poolType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IObjectPool<>)))
            {
                var identifierProp = poolType.GetProperty("Identifier");
                var objectTypeProp = poolType.GetProperty("ObjectType");

                if (identifierProp != null && objectTypeProp != null)
                {
                    var identifier = (string)identifierProp.GetValue(pool)!;
                    var objectType = (Type)objectTypeProp.GetValue(pool)!;

                    pools.Add(new PoolInfo(identifier, objectType));
                }
            }
        }

        return pools;
    }

    public void ConfigureGlobalSettings(ObjectPoolGlobalSettings settings)
    {
        _globalSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger.LogInformation("Updated global object pool settings: {Settings}", settings);
    }

    public ObjectPoolGlobalSettings GetGlobalSettings()
    {
        return _globalSettings;
    }

    public Task PerformCleanupAsync(bool aggressiveCleanup, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Performing cleanup (aggressive: {Aggressive})", aggressiveCleanup);
        
        // Simple cleanup - clear unused pools
        foreach (var pool in _pools.Values)
        {
            var poolType = pool.GetType();
            var clearMethod = poolType.GetMethod("Clear");
            clearMethod?.Invoke(pool, null);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        foreach (var pool in _pools.Values)
        {
            (pool as IDisposable)?.Dispose();
        }
        _pools.Clear();
    }
}
