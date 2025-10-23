using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Localization;

/// <summary>
/// Locale information
/// </summary>
public class LocaleInfo
{
    public string Code { get; set; }
    public string DisplayName { get; set; }
    public string NativeName { get; set; }
    public string LanguageCode { get; set; }
    public string CountryCode { get; set; }
    public bool IsRightToLeft { get; set; }
    public string? FontAssetPath { get; set; }
    public Dictionary<string, object> CustomProperties { get; set; } = new();

    public LocaleInfo(string code, string displayName, string? nativeName = null)
    {
        Code = code;
        DisplayName = displayName;
        NativeName = nativeName ?? displayName;

        var parts = code.Split('-');
        LanguageCode = parts.Length > 0 ? parts[0] : code;
        CountryCode = parts.Length > 1 ? parts[1] : string.Empty;
    }

    public override string ToString() => $"{Code} ({DisplayName})";
    public override bool Equals(object? obj) => obj is LocaleInfo other && Code == other.Code;
    public override int GetHashCode() => Code?.GetHashCode() ?? 0;
}

/// <summary>
/// Locale changed event data
/// </summary>
public class LocaleChangedEvent
{
    public LocaleInfo PreviousLocale { get; }
    public LocaleInfo NewLocale { get; }
    public TimeSpan ChangeTime { get; }
    public DateTime Timestamp { get; }

    public LocaleChangedEvent(LocaleInfo previousLocale, LocaleInfo newLocale, TimeSpan changeTime)
    {
        PreviousLocale = previousLocale;
        NewLocale = newLocale;
        ChangeTime = changeTime;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Localization reloaded event data
/// </summary>
public class LocalizationReloadedEvent
{
    public LocaleInfo Locale { get; }
    public int LoadedKeys { get; }
    public TimeSpan LoadTime { get; }
    public DateTime Timestamp { get; }

    public LocalizationReloadedEvent(LocaleInfo locale, int loadedKeys, TimeSpan loadTime)
    {
        Locale = locale;
        LoadedKeys = loadedKeys;
        LoadTime = loadTime;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Missing translation event data
/// </summary>
public class MissingTranslationEvent
{
    public string Key { get; }
    public LocaleInfo Locale { get; }
    public string? FallbackValue { get; }
    public DateTime Timestamp { get; }

    public MissingTranslationEvent(string key, LocaleInfo locale, string? fallbackValue = null)
    {
        Key = key;
        Locale = locale;
        FallbackValue = fallbackValue;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Localization error information
/// </summary>
public class LocalizationError
{
    public string Message { get; }
    public string? Code { get; }
    public LocaleInfo? Locale { get; }
    public Exception? Exception { get; }
    public DateTime Timestamp { get; }
    public LocalizationErrorLevel Level { get; }

    public LocalizationError(
        string message,
        string? code = null,
        LocaleInfo? locale = null,
        Exception? exception = null,
        LocalizationErrorLevel level = LocalizationErrorLevel.Error)
    {
        Message = message;
        Code = code;
        Locale = locale;
        Exception = exception;
        Level = level;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Localization metadata
/// </summary>
public class LocalizationMetadata
{
    public LocaleInfo Locale { get; set; } = null!;
    public int TotalKeys { get; set; }
    public int TranslatedKeys { get; set; }
    public float CompletionPercentage => TotalKeys > 0 ? (float)TranslatedKeys / TotalKeys * 100f : 0f;
    public DateTime LastUpdate { get; set; }
    public string? Version { get; set; }
    public Dictionary<string, object> CustomMetadata { get; set; } = new();
}

/// <summary>
/// Localization statistics
/// </summary>
public class LocalizationStatistics
{
    public LocaleInfo? CurrentLocale { get; set; }
    public int AvailableLocales { get; set; }
    public int TotalTranslationKeys { get; set; }
    public int LoadedTranslations { get; set; }
    public int MissingTranslations { get; set; }
    public int CachedAssets { get; set; }
    public long MemoryUsageBytes { get; set; }
    public TimeSpan LastLocaleChangeTime { get; set; }
    public TimeSpan LastReloadTime { get; set; }
    public int LocaleChanges { get; set; }
    public int ReloadOperations { get; set; }
    public Dictionary<string, int> KeyAccessCounts { get; set; } = new();
    public Dictionary<string, DateTime> KeyLastAccessed { get; set; } = new();
}

/// <summary>
/// Localization debug information
/// </summary>
public class LocalizationDebugInfo
{
    public LocaleInfo? CurrentLocale { get; set; }
    public LocaleInfo? DefaultLocale { get; set; }
    public List<LocaleInfo> AvailableLocales { get; set; } = new();
    public Dictionary<string, string> LoadedProviders { get; set; } = new();
    public List<string> MissingKeys { get; set; } = new();
    public Dictionary<string, LocalizationMetadata> LocaleMetadata { get; set; } = new();
    public LocalizationStatistics? Statistics { get; set; }
    public List<LocalizationError> RecentErrors { get; set; } = new();
    public DateTime LastDebugUpdate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Localization provider statistics
/// </summary>
public class LocalizationProviderStatistics
{
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public int SupportedLocales { get; set; }
    public int LoadedLocales { get; set; }
    public int TotalTranslations { get; set; }
    public int CachedAssets { get; set; }
    public long MemoryUsageBytes { get; set; }
    public TimeSpan LastLoadTime { get; set; }
    public DateTime LastActivity { get; set; }
    public Dictionary<string, object> CustomData { get; set; } = new();
}
