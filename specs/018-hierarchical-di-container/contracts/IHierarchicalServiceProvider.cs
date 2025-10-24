namespace LablabBean.DependencyInjection;

/// <summary>
/// Represents a service provider that supports hierarchical parent-child relationships.
/// Child containers can resolve services from their parent container when not found locally.
/// </summary>
/// <remarks>
/// This interface extends <see cref="IServiceProvider"/> to add hierarchy support for
/// multi-scene game development patterns. Services registered in parent containers are
/// accessible to all child containers, while child-specific services remain isolated.
///
/// <para><strong>Hierarchy Example:</strong></para>
/// <code>
/// Global Container (ISaveSystem, IAudioManager)
/// ├── MainMenu Container (IMainMenuController)
/// └── Dungeon Container (IDungeonState)
///     ├── Floor1 Container (IFloorGenerator - Floor1)
///     └── Floor2 Container (IFloorGenerator - Floor2)
/// </code>
///
/// <para><strong>Thread Safety:</strong></para>
/// <list type="bullet">
/// <item>GetService() is thread-safe</item>
/// <item>CreateChildContainer() must be called from a single thread (scene loading)</item>
/// <item>Dispose() is thread-safe</item>
/// </list>
/// </remarks>
public interface IHierarchicalServiceProvider : IServiceProvider, IDisposable
{
    /// <summary>
    /// Gets the optional name identifier for this container (e.g., "Global", "MainMenu", "Dungeon").
    /// </summary>
    /// <value>
    /// The container name, or null if not specified.
    /// Used for diagnostics and scene management.
    /// </value>
    string? Name { get; }

    /// <summary>
    /// Gets the parent container in the hierarchy.
    /// </summary>
    /// <value>
    /// The parent container, or null if this is the root container.
    /// </value>
    IHierarchicalServiceProvider? Parent { get; }

    /// <summary>
    /// Gets the read-only collection of child containers created from this container.
    /// </summary>
    /// <value>
    /// A read-only list of child containers. Empty if no children have been created.
    /// </value>
    IReadOnlyList<IHierarchicalServiceProvider> Children { get; }

    /// <summary>
    /// Gets the depth of this container in the hierarchy (0 = root, 1 = first child, etc.).
    /// </summary>
    /// <value>
    /// The depth level, where 0 represents the root container.
    /// </value>
    int Depth { get; }

    /// <summary>
    /// Gets a value indicating whether this container has been disposed.
    /// </summary>
    /// <value>
    /// true if the container has been disposed; otherwise, false.
    /// </value>
    bool IsDisposed { get; }

    /// <summary>
    /// Creates a new child container with the specified service registrations.
    /// </summary>
    /// <param name="configure">
    /// An action to configure the services for the child container.
    /// </param>
    /// <param name="name">
    /// Optional name for the child container (recommended for diagnostics).
    /// </param>
    /// <returns>
    /// A new <see cref="IHierarchicalServiceProvider"/> instance that is a child of this container.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if this container has been disposed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="configure"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The child container can resolve services from this parent container when not found locally.
    /// Child containers are owned by the parent and will be disposed when the parent is disposed.
    /// </para>
    /// <para><strong>Example:</strong></para>
    /// <code>
    /// var dungeonContainer = globalContainer.CreateChildContainer(services =>
    /// {
    ///     services.AddSingleton&lt;IDungeonState, DungeonState&gt;();
    ///     services.AddScoped&lt;ICombatSystem, CombatSystem&gt;();
    /// }, "Dungeon");
    ///
    /// // DungeonContainer can access global services + dungeon-specific services
    /// var saveSystem = dungeonContainer.GetService&lt;ISaveSystem&gt;(); // From global
    /// var dungeonState = dungeonContainer.GetService&lt;IDungeonState&gt;(); // From dungeon
    /// </code>
    /// </remarks>
    IHierarchicalServiceProvider CreateChildContainer(
        Action<IServiceCollection> configure,
        string? name = null);

    /// <summary>
    /// Gets the full hierarchy path from root to this container.
    /// </summary>
    /// <returns>
    /// A string representing the path, e.g., "Global → Dungeon → Floor1".
    /// </returns>
    /// <remarks>
    /// Useful for diagnostics, logging, and error messages.
    /// </remarks>
    string GetHierarchyPath();
}
