using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Contracts.Localization;
using LablabBean.Contracts.Localization.Services;
using LablabBean.Plugins.Localization.Json.Providers;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Localization.Json.Services;

public class LocalizationService : IService, IDisposable
{
    private readonly ILogger _logger;
    private readonly JsonLocalizationProvider _provider;
    private readonly Dictionary<string, LocaleInfo> _availableLocales = new();
    private readonly List<LocalizationError> _recentErrors = new();
    private readonly Dictionary<string, int> _keyAccessCounts = new();
    private readonly Dictionary<string, DateTime> _keyLastAccessed = new();

    private LocaleInfo _currentLocale;
    private LocaleInfo _defaultLocale;
    private Dictionary<string, string> _currentTranslations = new();
    private bool _isReady;
    private int _localeChanges;
    private int _reloadOps;
    private int _missingTranslations;
    private TimeSpan _lastLocaleChangeTime;
    private TimeSpan _lastReloadTime;

    public LocaleInfo CurrentLocale => _currentLocale;
    public IReadOnlyList<LocaleInfo> AvailableLocales => _availableLocales.Values.ToList();
    public LocaleInfo DefaultLocale => _defaultLocale;
    public bool IsReady => _isReady;
    public bool IsRightToLeft => _currentLocale.IsRightToLeft;

    public event EventHandler<LocaleChangedEvent>? LocaleChanged;
    public event EventHandler<LocalizationReloadedEvent>? LocalizationReloaded;
    public event EventHandler<LocalizationError>? ErrorOccurred;
    public event EventHandler<MissingTranslationEvent>? MissingTranslation;

    public LocalizationService(ILogger logger)
    {
        _logger = logger;
        _provider = new JsonLocalizationProvider(logger);

        _defaultLocale = new LocaleInfo("en-US", "English (United States)");
        _currentLocale = _defaultLocale;

        InitializeDefaultLocales();
        LoadDefaultLocale();
    }

    private void InitializeDefaultLocales()
    {
        _availableLocales["en-US"] = new LocaleInfo("en-US", "English (United States)");
        _availableLocales["ja-JP"] = new LocaleInfo("ja-JP", "Japanese (Japan)", "日本語");
        _availableLocales["es-ES"] = new LocaleInfo("es-ES", "Spanish (Spain)", "Español");
        _availableLocales["fr-FR"] = new LocaleInfo("fr-FR", "French (France)", "Français");
        _availableLocales["de-DE"] = new LocaleInfo("de-DE", "German (Germany)", "Deutsch");
        _availableLocales["zh-CN"] = new LocaleInfo("zh-CN", "Chinese (Simplified)", "简体中文");
        _availableLocales["ar-SA"] = new LocaleInfo("ar-SA", "Arabic (Saudi Arabia)", "العربية") { IsRightToLeft = true };
    }

    private async void LoadDefaultLocale()
    {
        try
        {
            _currentTranslations = await _provider.LoadLocaleAsync(_defaultLocale.Code, CancellationToken.None);
            _isReady = true;
            _logger.LogInformation("Default locale {Locale} loaded successfully", _defaultLocale.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load default locale");
            _currentTranslations = new Dictionary<string, string>();
            _isReady = true;
        }
    }

    public string GetString(string key, string? fallbackValue)
    {
        TrackKeyAccess(key);

        if (_currentTranslations.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        _missingTranslations++;
        var evt = new MissingTranslationEvent(key, _currentLocale, fallbackValue);
        MissingTranslation?.Invoke(this, evt);

        return fallbackValue ?? $"[{key}]";
    }

    public string GetFormattedString(string key, params object[] args)
    {
        var template = GetString(key, null);

        try
        {
            return string.Format(template, args);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to format string for key {Key}", key);
            return template;
        }
    }

    public string GetPluralString(string key, int count, params object[] args)
    {
        var pluralKey = count == 1 ? $"{key}.singular" : $"{key}.plural";
        var template = GetString(pluralKey, GetString(key, null));

        try
        {
            var allArgs = new object[] { count }.Concat(args).ToArray();
            return string.Format(template, allArgs);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to format plural string for key {Key}", key);
            return template;
        }
    }

    public bool HasKey(string key)
    {
        return _currentTranslations.ContainsKey(key);
    }

    public IReadOnlyCollection<string> GetAllKeys()
    {
        return _currentTranslations.Keys;
    }

    public IReadOnlyCollection<string> GetKeysWithPrefix(string prefix)
    {
        return _currentTranslations.Keys.Where(k => k.StartsWith(prefix)).ToList();
    }

    public async Task<bool> SetLocaleAsync(string localeCode, CancellationToken cancellationToken)
    {
        if (!_availableLocales.TryGetValue(localeCode, out var locale))
        {
            _logger.LogWarning("Locale {Locale} not available", localeCode);
            return false;
        }

        return await SetLocaleAsync(locale, cancellationToken);
    }

    public async Task<bool> SetLocaleAsync(LocaleInfo locale, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var previousLocale = _currentLocale;

        try
        {
            _logger.LogInformation("Changing locale from {Previous} to {New}", previousLocale.Code, locale.Code);

            var translations = await _provider.LoadLocaleAsync(locale.Code, cancellationToken);
            _currentTranslations = translations;
            _currentLocale = locale;
            _localeChanges++;

            var changeTime = DateTime.UtcNow - startTime;
            _lastLocaleChangeTime = changeTime;

            var evt = new LocaleChangedEvent(previousLocale, locale, changeTime);
            LocaleChanged?.Invoke(this, evt);

            return true;
        }
        catch (Exception ex)
        {
            var error = new LocalizationError(
                "Failed to change locale",
                "LOCALE_CHANGE_FAILED",
                locale,
                ex,
                LocalizationErrorLevel.Error
            );

            _recentErrors.Add(error);
            ErrorOccurred?.Invoke(this, error);

            _logger.LogError(ex, "Failed to change locale to {Locale}", locale.Code);
            return false;
        }
    }

    public async Task ReloadAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Reloading locale {Locale}", _currentLocale.Code);

            _provider.ClearCache(_currentLocale.Code);
            _currentTranslations = await _provider.LoadLocaleAsync(_currentLocale.Code, cancellationToken);
            _reloadOps++;

            var loadTime = DateTime.UtcNow - startTime;
            _lastReloadTime = loadTime;

            var evt = new LocalizationReloadedEvent(_currentLocale, _currentTranslations.Count, loadTime);
            LocalizationReloaded?.Invoke(this, evt);
        }
        catch (Exception ex)
        {
            var error = new LocalizationError(
                "Failed to reload locale",
                "LOCALE_RELOAD_FAILED",
                _currentLocale,
                ex,
                LocalizationErrorLevel.Error
            );

            _recentErrors.Add(error);
            ErrorOccurred?.Invoke(this, error);

            _logger.LogError(ex, "Failed to reload locale {Locale}", _currentLocale.Code);
        }
    }

    public async Task PreloadLocaleAsync(string localeCode, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Preloading locale {Locale}", localeCode);
            await _provider.LoadLocaleAsync(localeCode, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to preload locale {Locale}", localeCode);
        }
    }

    public LocalizationMetadata GetMetadata(string? localeCode)
    {
        var code = localeCode ?? _currentLocale.Code;
        return _provider.GetMetadata(code, _currentTranslations);
    }

    public LocalizationStatistics GetStatistics()
    {
        return new LocalizationStatistics
        {
            CurrentLocale = _currentLocale,
            AvailableLocales = _availableLocales.Count,
            TotalTranslationKeys = _currentTranslations.Count,
            LoadedTranslations = _currentTranslations.Count(kv => !string.IsNullOrWhiteSpace(kv.Value)),
            MissingTranslations = _missingTranslations,
            CachedAssets = 0,
            MemoryUsageBytes = EstimateMemoryUsage(),
            LastLocaleChangeTime = _lastLocaleChangeTime,
            LastReloadTime = _lastReloadTime,
            LocaleChanges = _localeChanges,
            ReloadOperations = _reloadOps,
            KeyAccessCounts = _keyAccessCounts.ToDictionary(kv => kv.Key, kv => kv.Value),
            KeyLastAccessed = _keyLastAccessed.ToDictionary(kv => kv.Key, kv => kv.Value)
        };
    }

    public LocalizationDebugInfo GetDebugInfo()
    {
        return new LocalizationDebugInfo
        {
            CurrentLocale = _currentLocale,
            DefaultLocale = _defaultLocale,
            AvailableLocales = _availableLocales.Values.ToList(),
            LoadedProviders = new Dictionary<string, string> { { "JsonLocalizationProvider", "Active" } },
            MissingKeys = new List<string>(),
            LocaleMetadata = _availableLocales.ToDictionary(
                kv => kv.Key,
                kv => GetMetadata(kv.Key)
            ),
            Statistics = GetStatistics(),
            RecentErrors = _recentErrors.ToList(),
            LastDebugUpdate = DateTime.UtcNow
        };
    }

    private void TrackKeyAccess(string key)
    {
        _keyAccessCounts[key] = _keyAccessCounts.GetValueOrDefault(key) + 1;
        _keyLastAccessed[key] = DateTime.UtcNow;
    }

    private long EstimateMemoryUsage()
    {
        long size = 0;
        foreach (var kv in _currentTranslations)
        {
            size += (kv.Key.Length + kv.Value.Length) * 2;
        }
        return size;
    }

    public void Dispose()
    {
        _provider.ClearCache();
    }
}
