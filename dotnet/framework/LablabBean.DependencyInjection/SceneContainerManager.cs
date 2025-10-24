using System.Collections.Concurrent;

namespace LablabBean.DependencyInjection;

/// <summary>
/// Manages scene-based hierarchical service containers for game lifecycle management.
/// Thread-safe implementation using ConcurrentDictionary for scene registry.
/// </summary>
public class SceneContainerManager : ISceneContainerManager
{
    private readonly ConcurrentDictionary<string, IHierarchicalServiceProvider> _sceneContainers = new();
    private readonly object _globalLock = new();
    private IHierarchicalServiceProvider? _globalContainer;

    /// <inheritdoc />
    public IHierarchicalServiceProvider? GlobalContainer => _globalContainer;

    /// <inheritdoc />
    public bool IsInitialized => _globalContainer is not null;

    /// <inheritdoc />
    public void InitializeGlobalContainer(IServiceCollection services)
    {
        lock (_globalLock)
        {
            if (_globalContainer is not null)
            {
                throw new InvalidOperationException("Global container has already been initialized.");
            }

            _globalContainer = services.BuildHierarchicalServiceProvider("Global");
        }
    }

    /// <inheritdoc />
    public IHierarchicalServiceProvider CreateSceneContainer(
        string sceneName,
        IServiceCollection sceneServices,
        string? parentSceneName = null)
    {
        ArgumentNullException.ThrowIfNull(sceneName);
        ArgumentNullException.ThrowIfNull(sceneServices);

        if (!IsInitialized)
        {
            throw new InvalidOperationException(
                "Global container must be initialized before creating scene containers. Call InitializeGlobalContainer first.");
        }

        // Check for duplicate scene name
        if (_sceneContainers.ContainsKey(sceneName))
        {
            throw new InvalidOperationException($"Scene '{sceneName}' already exists. Scene names must be unique.");
        }

        // Determine parent container
        IHierarchicalServiceProvider parentContainer;
        if (parentSceneName is not null)
        {
            // Validate parent scene exists
            if (!_sceneContainers.TryGetValue(parentSceneName, out var parent))
            {
                throw new InvalidOperationException(
                    $"Parent scene '{parentSceneName}' not found. Ensure the parent scene is created before creating child scenes.");
            }

            parentContainer = parent;
        }
        else
        {
            // Use global container as parent
            parentContainer = _globalContainer!;
        }

        // Create child container
        var sceneContainer = parentContainer.CreateChildContainer(
            services =>
            {
                foreach (var descriptor in sceneServices)
                {
                    services.Add(descriptor);
                }
            },
            sceneName);

        // Register in scene registry
        if (!_sceneContainers.TryAdd(sceneName, sceneContainer!))
        {
            // Unlikely race condition - dispose and throw
            sceneContainer.Dispose();
            throw new InvalidOperationException($"Failed to register scene '{sceneName}' due to concurrent modification.");
        }

        return sceneContainer;
    }

    /// <inheritdoc />
    public IHierarchicalServiceProvider? GetSceneContainer(string sceneName)
    {
        ArgumentNullException.ThrowIfNull(sceneName);

        return _sceneContainers.TryGetValue(sceneName, out var container) ? container : null;
    }

    /// <inheritdoc />
    public void UnloadScene(string sceneName)
    {
        ArgumentNullException.ThrowIfNull(sceneName);

        if (!_sceneContainers.TryRemove(sceneName, out var container))
        {
            throw new InvalidOperationException($"Scene '{sceneName}' not found. Cannot unload a scene that doesn't exist.");
        }

        // Dispose the container (will cascade to children)
        container.Dispose();
    }

    /// <inheritdoc />
    public IEnumerable<string> GetSceneNames()
    {
        return _sceneContainers.Keys.ToList();
    }
}
