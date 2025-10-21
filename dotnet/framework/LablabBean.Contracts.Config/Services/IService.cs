namespace LablabBean.Contracts.Config.Services;

/// <summary>
/// Configuration service for managing application settings.
/// </summary>
public interface IService
{
    /// <summary>
    /// Get a configuration value as a string.
    /// </summary>
    /// <param name="key">Configuration key (supports hierarchical keys with colon separator, e.g., "game:difficulty").</param>
    /// <returns>Configuration value or null if not found.</returns>
    string? Get(string key);

    /// <summary>
    /// Get a configuration value with automatic type conversion.
    /// </summary>
    /// <typeparam name="T">Target type for conversion.</typeparam>
    /// <param name="key">Configuration key.</param>
    /// <returns>Converted value or default(T) if not found or conversion fails.</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Get a configuration section (hierarchical group of values).
    /// </summary>
    /// <param name="key">Section key.</param>
    /// <returns>Configuration section.</returns>
    IConfigSection GetSection(string key);

    /// <summary>
    /// Set a configuration value.
    /// </summary>
    /// <param name="key">Configuration key.</param>
    /// <param name="value">Value to set.</param>
    void Set(string key, string value);

    /// <summary>
    /// Check if a configuration key exists.
    /// </summary>
    /// <param name="key">Configuration key.</param>
    /// <returns>True if key exists, false otherwise.</returns>
    bool Exists(string key);

    /// <summary>
    /// Reload configuration from source.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task that completes when reload is done.</returns>
    Task ReloadAsync(CancellationToken ct = default);

    /// <summary>
    /// Event fired when any configuration value changes.
    /// </summary>
    event EventHandler<ConfigChangedEventArgs>? ConfigChanged;
}

/// <summary>
/// Event args for configuration changes.
/// </summary>
public class ConfigChangedEventArgs : EventArgs
{
    /// <summary>
    /// Configuration key that changed.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Old value (null if key didn't exist).
    /// </summary>
    public string? OldValue { get; init; }

    /// <summary>
    /// New value (null if key was removed).
    /// </summary>
    public string? NewValue { get; init; }
}
