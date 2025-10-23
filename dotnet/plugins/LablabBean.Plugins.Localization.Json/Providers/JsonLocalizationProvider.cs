using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Contracts.Localization;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Localization.Json.Providers;

public class JsonLocalizationProvider
{
    private readonly ILogger _logger;
    private readonly string _basePath;
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _localeData = new();

    public JsonLocalizationProvider(ILogger logger)
    {
        _logger = logger;
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _basePath = Path.Combine(appDataPath, "LablabBean", "Localization");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<Dictionary<string, string>> LoadLocaleAsync(string localeCode, CancellationToken cancellationToken)
    {
        if (_localeData.TryGetValue(localeCode, out var cached))
        {
            _logger.LogDebug("Loaded locale {Locale} from cache", localeCode);
            return cached;
        }

        var filePath = Path.Combine(_basePath, $"{localeCode}.json");
        
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Locale file not found for {Locale}, creating empty", localeCode);
            var empty = new Dictionary<string, string>();
            await SaveLocaleAsync(localeCode, empty, cancellationToken);
            return empty;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json) 
                       ?? new Dictionary<string, string>();
            
            _localeData[localeCode] = data;
            _logger.LogInformation("Loaded {Count} translations for locale {Locale}", data.Count, localeCode);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load locale {Locale}", localeCode);
            throw;
        }
    }

    public async Task SaveLocaleAsync(string localeCode, Dictionary<string, string> data, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(_basePath, $"{localeCode}.json");
        
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(data, options);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
            
            _localeData[localeCode] = data;
            _logger.LogInformation("Saved {Count} translations for locale {Locale}", data.Count, localeCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save locale {Locale}", localeCode);
            throw;
        }
    }

    public List<string> GetAvailableLocales()
    {
        var files = Directory.GetFiles(_basePath, "*.json");
        return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
    }

    public void ClearCache(string? localeCode = null)
    {
        if (localeCode != null)
        {
            _localeData.TryRemove(localeCode, out _);
        }
        else
        {
            _localeData.Clear();
        }
    }

    public LocalizationMetadata GetMetadata(string localeCode, Dictionary<string, string> data)
    {
        var filePath = Path.Combine(_basePath, $"{localeCode}.json");
        var lastUpdate = File.Exists(filePath) ? File.GetLastWriteTimeUtc(filePath) : DateTime.UtcNow;

        return new LocalizationMetadata
        {
            Locale = new LocaleInfo(localeCode, localeCode),
            TotalKeys = data.Count,
            TranslatedKeys = data.Count(kv => !string.IsNullOrWhiteSpace(kv.Value)),
            LastUpdate = lastUpdate,
            Version = "1.0.0"
        };
    }
}
