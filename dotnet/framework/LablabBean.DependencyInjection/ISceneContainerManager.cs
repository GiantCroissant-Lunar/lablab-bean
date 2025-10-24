namespace LablabBean.DependencyInjection;

/// <summary>
/// Manages scene-based hierarchical service containers for game lifecycle management.
/// </summary>
public interface ISceneContainerManager
{
    /// <summary>
    /// Gets the global (root) service container.
    /// </summary>
    IHierarchicalServiceProvider? GlobalContainer { get; }

    /// <summary>
    /// Gets a value indicating whether the global container has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Initializes the global (root) service container.
    /// </summary>
    /// <param name="services">The service collection for global services.</param>
    /// <exception cref="InvalidOperationException">Thrown if already initialized.</exception>
    void InitializeGlobalContainer(IServiceCollection services);

    /// <summary>
    /// Creates a new scene container with optional parent scene.
    /// </summary>
    /// <param name="sceneName">The unique name for the scene.</param>
    /// <param name="sceneServices">The service collection for scene-specific services.</param>
    /// <param name="parentSceneName">Optional parent scene name. If null, uses global container as parent.</param>
    /// <returns>The created hierarchical service provider for the scene.</returns>
    /// <exception cref="InvalidOperationException">Thrown if global container not initialized, scene name already exists, or parent scene not found.</exception>
    IHierarchicalServiceProvider CreateSceneContainer(
        string sceneName,
        IServiceCollection sceneServices,
        string? parentSceneName = null);

    /// <summary>
    /// Gets a scene container by name.
    /// </summary>
    /// <param name="sceneName">The scene name.</param>
    /// <returns>The scene container, or null if not found.</returns>
    IHierarchicalServiceProvider? GetSceneContainer(string sceneName);

    /// <summary>
    /// Unloads a scene by disposing and removing its container.
    /// </summary>
    /// <param name="sceneName">The scene name to unload.</param>
    /// <exception cref="InvalidOperationException">Thrown if scene not found.</exception>
    void UnloadScene(string sceneName);

    /// <summary>
    /// Gets all registered scene names.
    /// </summary>
    /// <returns>An enumerable of scene names.</returns>
    IEnumerable<string> GetSceneNames();
}
