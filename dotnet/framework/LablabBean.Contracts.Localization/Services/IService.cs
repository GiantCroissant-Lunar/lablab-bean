using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LablabBean.Contracts.Localization.Services;

/// <summary>
/// Localization service interface for multi-language text and asset localization.
/// Provides access to localized strings, plurals, formatting, and asset management.
/// </summary>
public interface IService
{
    /// <summary>
    /// Current active locale/language
    /// </summary>
    LocaleInfo CurrentLocale { get; }

    /// <summary>
    /// All available locales in the system
    /// </summary>
    IReadOnlyList<LocaleInfo> AvailableLocales { get; }

    /// <summary>
    /// Default/fallback locale
    /// </summary>
    LocaleInfo DefaultLocale { get; }

    /// <summary>
    /// Whether the localization system is ready to use
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Whether the current locale is right-to-left (RTL)
    /// </summary>
    bool IsRightToLeft { get; }

    /// <summary>
    /// Get a localized string by key
    /// </summary>
    /// <param name="key">Localization key</param>
    /// <param name="fallbackValue">Value to return if key not found</param>
    /// <returns>Localized string or fallback</returns>
    string GetString(string key, string? fallbackValue);

    /// <summary>
    /// Get a localized string with format arguments
    /// </summary>
    /// <param name="key">Localization key</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    string GetFormattedString(string key, params object[] args);

    /// <summary>
    /// Get a localized plural string based on count
    /// </summary>
    /// <param name="key">Localization key</param>
    /// <param name="count">Count for plural rules</param>
    /// <param name="args">Additional format arguments</param>
    /// <returns>Plural-appropriate localized string</returns>
    string GetPluralString(string key, int count, params object[] args);

    /// <summary>
    /// Check if a localization key exists
    /// </summary>
    /// <param name="key">Localization key to check</param>
    /// <returns>True if key exists in current or fallback locale</returns>
    bool HasKey(string key);

    /// <summary>
    /// Get all localization keys for current locale
    /// </summary>
    /// <returns>Collection of all available keys</returns>
    IReadOnlyCollection<string> GetAllKeys();

    /// <summary>
    /// Get all localization keys with a specific prefix
    /// </summary>
    /// <param name="prefix">Key prefix to filter by</param>
    /// <returns>Collection of keys matching prefix</returns>
    IReadOnlyCollection<string> GetKeysWithPrefix(string prefix);

    /// <summary>
    /// Change the current locale
    /// </summary>
    /// <param name="localeCode">Locale code (e.g., "en-US", "ja-JP")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when locale is changed</returns>
    Task<bool> SetLocaleAsync(string localeCode, CancellationToken cancellationToken);

    /// <summary>
    /// Change the current locale
    /// </summary>
    /// <param name="locale">Locale information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when locale is changed</returns>
    Task<bool> SetLocaleAsync(LocaleInfo locale, CancellationToken cancellationToken);

    /// <summary>
    /// Reload localization data for current locale
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when reload is finished</returns>
    Task ReloadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Preload localization data for a specific locale
    /// </summary>
    /// <param name="localeCode">Locale code to preload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when preload is finished</returns>
    Task PreloadLocaleAsync(string localeCode, CancellationToken cancellationToken);

    /// <summary>
    /// Get localization metadata (completion percentage, last update, etc.)
    /// </summary>
    /// <param name="localeCode">Locale code (null for current)</param>
    /// <returns>Localization metadata</returns>
    LocalizationMetadata GetMetadata(string? localeCode);

    /// <summary>
    /// Get localization statistics
    /// </summary>
    /// <returns>Localization statistics</returns>
    LocalizationStatistics GetStatistics();

    /// <summary>
    /// Get localization debug information
    /// </summary>
    /// <returns>Debug information</returns>
    LocalizationDebugInfo GetDebugInfo();

    /// <summary>
    /// Event handler for locale changes
    /// </summary>
    event EventHandler<LocaleChangedEvent>? LocaleChanged;

    /// <summary>
    /// Event handler for localization reloads
    /// </summary>
    event EventHandler<LocalizationReloadedEvent>? LocalizationReloaded;

    /// <summary>
    /// Event handler for localization errors
    /// </summary>
    event EventHandler<LocalizationError>? ErrorOccurred;

    /// <summary>
    /// Event handler for missing translations
    /// </summary>
    event EventHandler<MissingTranslationEvent>? MissingTranslation;
}
