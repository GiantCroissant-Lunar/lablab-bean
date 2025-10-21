namespace LablabBean.Contracts.Config;

/// <summary>
/// Event published when a configuration value changes.
/// </summary>
/// <param name="Key">Configuration key that changed.</param>
/// <param name="OldValue">Previous value (null if key didn't exist).</param>
/// <param name="NewValue">New value (null if key was removed).</param>
/// <param name="Timestamp">When the change occurred.</param>
public record ConfigChangedEvent(
    string Key,
    string? OldValue,
    string? NewValue,
    DateTimeOffset Timestamp
)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    /// <param name="key">Configuration key that changed.</param>
    /// <param name="oldValue">Previous value.</param>
    /// <param name="newValue">New value.</param>
    public ConfigChangedEvent(string key, string? oldValue, string? newValue)
        : this(key, oldValue, newValue, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Event published when configuration is reloaded.
/// </summary>
/// <param name="Timestamp">When the reload occurred.</param>
public record ConfigReloadedEvent(DateTimeOffset Timestamp)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    public ConfigReloadedEvent()
        : this(DateTimeOffset.UtcNow)
    {
    }
}
