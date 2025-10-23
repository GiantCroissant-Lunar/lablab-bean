using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LablabBean.Contracts.PersistentStorage.Services;

public interface IService : IDisposable
{
    bool IsHealthy { get; }

    Task SaveAsync<T>(string key, T data, StorageProviderType providerType, CancellationToken cancellationToken);
    Task SaveAsync<T>(StorageRequest request, T data);
    Task<T> LoadAsync<T>(string key, StorageProviderType providerType, CancellationToken cancellationToken);
    Task<T> LoadAsync<T>(string key, T defaultValue, StorageProviderType providerType, CancellationToken cancellationToken);
    Task<T> LoadAsync<T>(StorageRequest request);
    Task<bool> ExistsAsync(string key, StorageProviderType providerType, CancellationToken cancellationToken);
    Task DeleteAsync(string key, StorageProviderType providerType, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetKeysAsync(string? prefix, StorageProviderType providerType, CancellationToken cancellationToken);
    Task ClearAsync(string? prefix, StorageProviderType providerType, CancellationToken cancellationToken);
    Task<long> GetSizeAsync(StorageProviderType providerType, CancellationToken cancellationToken);
    Task BackupAsync(string filePath, IEnumerable<string>? keys, StorageProviderType providerType, CancellationToken cancellationToken);
    Task RestoreAsync(string filePath, bool overwrite, StorageProviderType providerType, CancellationToken cancellationToken);
    Task SyncAsync(StorageProviderType providerType, CancellationToken cancellationToken);
    StorageStatistics GetStatistics();
    StorageDebugInfo GetDebugInfo();

    event Action<string>? DataSaved;
    event Action<string>? DataLoaded;
    event Action<string>? DataDeleted;
    event Action<string>? StorageError;
}
