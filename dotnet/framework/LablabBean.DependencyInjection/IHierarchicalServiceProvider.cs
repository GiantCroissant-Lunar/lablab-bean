namespace LablabBean.DependencyInjection;

/// <summary>
/// Hierarchical service provider that supports parent-child container relationships.
/// Extends IServiceProvider with hierarchy management capabilities.
/// </summary>
public interface IHierarchicalServiceProvider : IServiceProvider, IDisposable
{
    /// <summary>
    /// Gets the name of this container.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the parent container, or null if this is the root.
    /// </summary>
    IHierarchicalServiceProvider? Parent { get; }

    /// <summary>
    /// Gets the read-only collection of child containers.
    /// </summary>
    IReadOnlyList<IHierarchicalServiceProvider> Children { get; }

    /// <summary>
    /// Gets the depth of this container in the hierarchy (0 for root).
    /// </summary>
    int Depth { get; }

    /// <summary>
    /// Gets whether this container has been disposed.
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Creates a child container with additional services.
    /// </summary>
    /// <param name="configureServices">Action to configure child container services.</param>
    /// <param name="name">Optional name for the child container.</param>
    /// <returns>A new child container.</returns>
    IHierarchicalServiceProvider CreateChildContainer(
        Action<IServiceCollection> configureServices,
        string? name = null);

    /// <summary>
    /// Gets the hierarchy path from root to this container (e.g., "Global → Dungeon → Floor1").
    /// </summary>
    string GetHierarchyPath();
}
