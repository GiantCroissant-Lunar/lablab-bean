namespace LablabBean.Contracts.Config;

/// <summary>
/// Configuration section representing a hierarchical group of values.
/// </summary>
public interface IConfigSection
{
    /// <summary>
    /// Get a value from this section.
    /// </summary>
    /// <param name="key">Key within this section.</param>
    /// <returns>Value or null if not found.</returns>
    string? Get(string key);

    /// <summary>
    /// Get a typed value from this section.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    /// <param name="key">Key within this section.</param>
    /// <returns>Converted value or default(T).</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Get a child section.
    /// </summary>
    /// <param name="key">Section key.</param>
    /// <returns>Child configuration section.</returns>
    IConfigSection GetSection(string key);

    /// <summary>
    /// Get all keys in this section.
    /// </summary>
    /// <returns>Collection of keys.</returns>
    IReadOnlyCollection<string> GetKeys();

    /// <summary>
    /// Section path (e.g., "game:graphics").
    /// </summary>
    string Path { get; }
}
