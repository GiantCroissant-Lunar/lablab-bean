using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.PersistentStorage.Json.Providers;

public class JsonFileStorageProvider
{
    private readonly ILogger _logger;
    private readonly string _basePath;
    private readonly JsonSerializerOptions _options;

    public JsonFileStorageProvider(ILogger logger, string? basePath = null)
    {
        _logger = logger;
        _basePath = basePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LablabBean", "Storage");
        Directory.CreateDirectory(_basePath);

        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    private string GetFilePath(string key) => Path.Combine(_basePath, $"{SanitizeKey(key)}.json");

    private string SanitizeKey(string key)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", key.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    public async Task SaveAsync<T>(string key, T data, CancellationToken cancellationToken)
    {
        var filePath = GetFilePath(key);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        try
        {
            var json = JsonSerializer.Serialize(data, _options);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
            _logger.LogDebug("Saved {Key} to {FilePath}", key, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save {Key} to {FilePath}", key, filePath);
            throw;
        }
    }

    public async Task<T> LoadAsync<T>(string key, CancellationToken cancellationToken)
    {
        var filePath = GetFilePath(key);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Storage file not found for key: {key}", filePath);
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var data = JsonSerializer.Deserialize<T>(json, _options);
            _logger.LogDebug("Loaded {Key} from {FilePath}", key, filePath);
            return data ?? throw new InvalidOperationException($"Deserialization resulted in null for key: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load {Key} from {FilePath}", key, filePath);
            throw;
        }
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken)
    {
        var filePath = GetFilePath(key);
        return Task.FromResult(File.Exists(filePath));
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        var filePath = GetFilePath(key);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogDebug("Deleted {Key} at {FilePath}", key, filePath);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetKeysAsync(string? prefix, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_basePath))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var files = Directory.GetFiles(_basePath, "*.json", SearchOption.AllDirectories);
        var keys = files
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Where(k => string.IsNullOrEmpty(prefix) || k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(keys);
    }

    public Task<long> GetSizeAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_basePath))
        {
            return Task.FromResult(0L);
        }

        var files = Directory.GetFiles(_basePath, "*.json", SearchOption.AllDirectories);
        var totalSize = files.Sum(f => new FileInfo(f).Length);
        return Task.FromResult(totalSize);
    }
}
