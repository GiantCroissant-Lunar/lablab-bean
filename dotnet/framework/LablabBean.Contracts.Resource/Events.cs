using System;

namespace LablabBean.Contracts.Resource;

/// <summary>
/// Event published when a resource starts loading.
/// </summary>
/// <param name="ResourceId">Identifier of the resource.</param>
/// <param name="Timestamp">When loading started.</param>
public record ResourceLoadStartedEvent(string ResourceId, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    /// <param name="resourceId">Identifier of the resource.</param>
    public ResourceLoadStartedEvent(string resourceId)
        : this(resourceId, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Event published when a resource completes loading.
/// </summary>
/// <param name="ResourceId">Identifier of the resource.</param>
/// <param name="LoadTimeMs">Time taken to load in milliseconds.</param>
/// <param name="Timestamp">When loading completed.</param>
public record ResourceLoadCompletedEvent(string ResourceId, long LoadTimeMs, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    /// <param name="resourceId">Identifier of the resource.</param>
    /// <param name="loadTimeMs">Time taken to load.</param>
    public ResourceLoadCompletedEvent(string resourceId, long loadTimeMs)
        : this(resourceId, loadTimeMs, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Event published when a resource fails to load.
/// </summary>
/// <param name="ResourceId">Identifier of the resource.</param>
/// <param name="Error">Exception that caused the failure.</param>
/// <param name="Timestamp">When the failure occurred.</param>
public record ResourceLoadFailedEvent(string ResourceId, Exception Error, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    /// <param name="resourceId">Identifier of the resource.</param>
    /// <param name="error">Exception that caused the failure.</param>
    public ResourceLoadFailedEvent(string resourceId, Exception error)
        : this(resourceId, error, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Event published when a resource is unloaded.
/// </summary>
/// <param name="ResourceId">Identifier of the resource.</param>
/// <param name="Timestamp">When the resource was unloaded.</param>
public record ResourceUnloadedEvent(string ResourceId, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    /// <param name="resourceId">Identifier of the resource.</param>
    public ResourceUnloadedEvent(string resourceId)
        : this(resourceId, DateTimeOffset.UtcNow)
    {
    }
}
