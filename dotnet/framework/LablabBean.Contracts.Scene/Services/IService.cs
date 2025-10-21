namespace LablabBean.Contracts.Scene.Services;

/// <summary>
/// Scene service for managing level/dungeon loading, camera, and viewport.
/// </summary>
public interface IService
{
    /// <summary>
    /// Load a scene asynchronously.
    /// </summary>
    /// <param name="sceneId">Unique identifier for the scene to load.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task that completes when scene is loaded.</returns>
    Task LoadSceneAsync(string sceneId, CancellationToken ct = default);

    /// <summary>
    /// Unload a scene asynchronously.
    /// </summary>
    /// <param name="sceneId">Unique identifier for the scene to unload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task that completes when scene is unloaded.</returns>
    Task UnloadSceneAsync(string sceneId, CancellationToken ct = default);

    /// <summary>
    /// Get the current viewport dimensions.
    /// </summary>
    /// <returns>Viewport with width and height.</returns>
    Viewport GetViewport();

    /// <summary>
    /// Get the current camera viewport (camera position + viewport dimensions).
    /// </summary>
    /// <returns>Camera viewport combining camera and viewport.</returns>
    CameraViewport GetCameraViewport();

    /// <summary>
    /// Set the camera position and zoom.
    /// </summary>
    /// <param name="camera">Camera with position and zoom level.</param>
    void SetCamera(Camera camera);

    /// <summary>
    /// Update world entities with new snapshots.
    /// </summary>
    /// <param name="snapshots">Read-only list of entity snapshots.</param>
    void UpdateWorld(IReadOnlyList<EntitySnapshot> snapshots);

    /// <summary>
    /// Event fired when the scene is shutting down.
    /// </summary>
    event EventHandler<SceneShutdownEventArgs>? Shutdown;
}

/// <summary>
/// Event args for scene shutdown.
/// </summary>
public class SceneShutdownEventArgs : EventArgs
{
    /// <summary>
    /// Reason for shutdown.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}
