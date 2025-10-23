using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Contracts.PersistentStorage;
using LablabBean.Contracts.PersistentStorage.Services;
using LablabBean.Plugins.PersistentStorage.Json.Providers;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.PersistentStorage.Json.Services;

public class PersistentStorageService : IService
{
    private readonly ILogger _logger;
    private readonly JsonFileStorageProvider _jsonProvider;
    private readonly ConcurrentDictionary<string, object> _cache = new();
    private int _saveOps, _loadOps, _deleteOps;
    private DateTime _lastSave = DateTime.MinValue;
    private DateTime _lastLoad = DateTime.MinValue;
    private DateTime _lastSync = DateTime.MinValue;
    private string? _lastError;

    public bool IsHealthy => _lastError == null;

    public event Action<string>? DataSaved;
    public event Action<string>? DataLoaded;
    public event Action<string>? DataDeleted;
    public event Action<string>? StorageError;

    public PersistentStorageService(ILogger logger)
    {
        _logger = logger;
        _jsonProvider = new JsonFileStorageProvider(logger);
    }

    public async Task SaveAsync<T>(string key, T data, StorageProviderType providerType, CancellationToken cancellationToken)
    {
        if (providerType != StorageProviderType.JsonFile)
            throw new NotSupportedException($"Provider {providerType} not supported");

        try
        {
            await _jsonProvider.SaveAsync(key, data, cancellationToken);
            _cache[key] = data!;
            Interlocked.Increment(ref _saveOps);
            _lastSave = DateTime.UtcNow;
            DataSaved?.Invoke(key);
            _logger.LogInformation("Saved data for key {Key}", key);
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            StorageError?.Invoke($"Save failed for {key}: {ex.Message}");
            _logger.LogError(ex, "Failed to save data for key {Key}", key);
            throw;
        }
    }

    public Task SaveAsync<T>(StorageRequest request, T data)
    {
        return SaveAsync(request.Key, data, request.ProviderType, request.CancellationToken);
    }

    public async Task<T> LoadAsync<T>(string key, StorageProviderType providerType, CancellationToken cancellationToken)
    {
        if (providerType != StorageProviderType.JsonFile)
            throw new NotSupportedException($"Provider {providerType} not supported");

        try
        {
            if (_cache.TryGetValue(key, out var cached))
            {
                _logger.LogDebug("Loaded data for key {Key} from cache", key);
                return (T)cached;
            }

            var data = await _jsonProvider.LoadAsync<T>(key, cancellationToken);
            _cache[key] = data!;
            Interlocked.Increment(ref _loadOps);
            _lastLoad = DateTime.UtcNow;
            DataLoaded?.Invoke(key);
            _logger.LogInformation("Loaded data for key {Key}", key);
            return data;
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            StorageError?.Invoke($"Load failed for {key}: {ex.Message}");
            _logger.LogError(ex, "Failed to load data for key {Key}", key);
            throw;
        }
    }

    public async Task<T> LoadAsync<T>(string key, T defaultValue, StorageProviderType providerType, CancellationToken cancellationToken)
    {
        try
        {
            return await LoadAsync<T>(key, providerType, cancellationToken);
        }
        catch
        {
            _logger.LogWarning("Failed to load key {Key}, returning default value", key);
            return defaultValue;
        }
    }

    public Task<T> LoadAsync<T>(StorageRequest request)
    {
        return LoadAsync<T>(request.Key, request.ProviderType, request.CancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, StorageProviderType providerType, CancellationToken cancellationToken)
    {
        if (providerType != StorageProviderType.JsonFile)
            throw new NotSupportedException($"Provider {providerType} not supported");

        return await _jsonProvider.ExistsAsync(key, cancellationToken);
    }

    public async Task DeleteAsync(string key, StorageProviderType providerType, CancellationToken cancellationToken)
    {
        if (providerType != StorageProviderType.JsonFile)
            throw new NotSupportedException($"Provider {providerType} not supported");

        try
        {
            await _jsonProvider.DeleteAsync(key, cancellationToken);
            _cache.TryRemove(key, out _);
            Interlocked.Increment(ref _deleteOps);
            DataDeleted?.Invoke(key);
            _logger.LogInformation("Deleted data for key {Key}", key);
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            StorageError?.Invoke($"Delete failed for {key}: {ex.Message}");
            _logger.LogError(ex, "Failed to delete data for key {Key}", key);
            throw;
        }
    }

    public async Task<IReadOnlyList<string>> GetKeysAsync(string? prefix, StorageProviderType providerType, CancellationToken cancellationToken)
    {
        if (providerType != StorageProviderType.JsonFile)
            throw new NotSupportedException($"Provider {providerType} not supported");

        return await _jsonProvider.GetKeysAsync(prefix, cancellationToken);
    }

    public async Task ClearAsync(string? prefix, StorageProviderType providerType, CancellationToken cancellationToken)
    {
        if (providerType != StorageProviderType.JsonFile)
            throw new NotSupportedException($"Provider {providerType} not supported");

        var keys = await GetKeysAsync(prefix, providerType, cancellationToken);
        foreach (var key in keys)
        {
            await DeleteAsync(key, providerType, cancellationToken);
        }
        _logger.LogInformation("Cleared {Count} keys with prefix {Prefix}", keys.Count, prefix ?? "(all)");
    }

    public async Task<long> GetSizeAsync(StorageProviderType providerType, CancellationToken cancellationToken)
    {
        if (providerType != StorageProviderType.JsonFile)
            throw new NotSupportedException($"Provider {providerType} not supported");

        return await _jsonProvider.GetSizeAsync(cancellationToken);
    }

    public Task BackupAsync(string filePath, IEnumerable<string>? keys, StorageProviderType providerType, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Backup not implemented in JSON provider");
    }

    public Task RestoreAsync(string filePath, bool overwrite, StorageProviderType providerType, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Restore not implemented in JSON provider");
    }

    public Task SyncAsync(StorageProviderType providerType, CancellationToken cancellationToken)
    {
        _lastSync = DateTime.UtcNow;
        _logger.LogInformation("Storage synced");
        return Task.CompletedTask;
    }

    public StorageStatistics GetStatistics()
    {
        return new StorageStatistics(
            _cache.Count,
            0,
            1,
            _lastSave,
            _lastLoad,
            _lastSync,
            _saveOps,
            _loadOps,
            _deleteOps
        );
    }

    public StorageDebugInfo GetDebugInfo()
    {
        return new StorageDebugInfo(
            IsHealthy,
            _lastError,
            _cache.Count,
            1,
            DateTime.UtcNow,
            new[] { "JsonFile" },
            new Dictionary<string, object> { ["CacheKeys"] = _cache.Keys.ToArray() }
        );
    }

    public void Dispose()
    {
        _cache.Clear();
    }
}
