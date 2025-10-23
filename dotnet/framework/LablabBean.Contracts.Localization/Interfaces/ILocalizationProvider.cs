using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LablabBean.Contracts.Localization.Interfaces;

/// <summary>
/// Localization provider interface for implementing different localization backends
/// </summary>
public interface ILocalizationProvider
{
    /// <summary>
    /// Provider identifier
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Provider display name
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Whether the provider is healthy and operational
    /// </summary>
    bool IsHealthy { get; }

    /// <summary>
    /// Initialize the localization provider
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if initialization successful</returns>
    Task<bool> InitializeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get available locales from this provider
    /// </summary>
    /// <returns>Available locales</returns>
    Task<IReadOnlyList<LocaleInfo>> GetAvailableLocalesAsync();

    /// <summary>
    /// Load translations for a specific locale
    /// </summary>
    /// <param name="locale">Locale to load</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of key-value translations</returns>
    Task<IReadOnlyDictionary<string, string>> LoadTranslationsAsync(LocaleInfo locale, CancellationToken cancellationToken);

    /// <summary>
    /// Get provider-specific metadata for a locale
    /// </summary>
    /// <param name="locale">Target locale</param>
    /// <returns>Localization metadata</returns>
    LocalizationMetadata GetMetadata(LocaleInfo locale);

    /// <summary>
    /// Get provider statistics
    /// </summary>
    /// <returns>Provider statistics</returns>
    LocalizationProviderStatistics GetStatistics();
}
