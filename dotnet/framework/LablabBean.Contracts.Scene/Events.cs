namespace LablabBean.Contracts.Scene;

/// <summary>
/// Event published when a scene is successfully loaded.
/// </summary>
/// <param name="SceneId">Unique identifier of the loaded scene.</param>
/// <param name="Timestamp">When the scene was loaded.</param>
public record SceneLoadedEvent(string SceneId, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    /// <param name="sceneId">Unique identifier of the loaded scene.</param>
    public SceneLoadedEvent(string sceneId)
        : this(sceneId, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Event published when a scene is unloaded.
/// </summary>
/// <param name="SceneId">Unique identifier of the unloaded scene.</param>
/// <param name="Timestamp">When the scene was unloaded.</param>
public record SceneUnloadedEvent(string SceneId, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    /// <param name="sceneId">Unique identifier of the unloaded scene.</param>
    public SceneUnloadedEvent(string sceneId)
        : this(sceneId, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Event published when a scene fails to load.
/// </summary>
/// <param name="SceneId">Unique identifier of the scene that failed to load.</param>
/// <param name="Error">Exception that caused the failure.</param>
/// <param name="Timestamp">When the failure occurred.</param>
public record SceneLoadFailedEvent(string SceneId, Exception Error, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    /// <param name="sceneId">Unique identifier of the scene that failed to load.</param>
    /// <param name="error">Exception that caused the failure.</param>
    public SceneLoadFailedEvent(string sceneId, Exception error)
        : this(sceneId, error, DateTimeOffset.UtcNow)
    {
    }
}
