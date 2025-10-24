namespace LablabBean.DependencyInjection;

/// <summary>
/// Manages the lifecycle of hierarchical service containers across game scenes.
/// </summary>
/// <remarks>
/// This manager provides a centralized registry for scene-based containers,
/// enabling automatic parent-child relationships and proper disposal during
/// scene transitions.
///
/// <para><strong>Typical Usage:</strong></para>
/// <code>
/// // 1. Bootstrap: Initialize global container
/// var manager = new SceneContainerManager();
/// manager.InitializeGlobalContainer(services =>
/// {
///     services.AddSingleton&lt;ISaveSystem, SaveSystem&gt;();
///     services.AddSingleton&lt;IAudioManager, AudioManager&gt;();
/// });
///
/// // 2. Scene Load: Create scene container
/// var dungeonContainer = manager.CreateSceneContainer("Dungeon", services =>
/// {
///     services.AddSingleton&lt;IDungeonState, DungeonState&gt;();
/// });
///
/// // 3. Scene Unload: Clean up
/// manager.UnloadScene("Dungeon"); // Disposes container
/// </code>
/// </remarks>
public interface ISceneContainerManager
{
    /// <summary>
    /// Gets the global (root) container that serves as the parent for all scene containers.
    /// </summary>
    /// <value>
    /// The global container, or null if not yet initialized.
    /// </value>
    IHierarchicalServiceProvider? GlobalContainer { get; }

    /// <summary>
    /// Gets a value indicating whether the global container has been initialized.
    /// </summary>
    /// <value>
    /// true if <see cref="InitializeGlobalContainer"/> has been called; otherwise, false.
    /// </value>
    bool IsInitialized { get; }

    /// <summary>
    /// Initializes the global (root) container with the specified services.
    /// </summary>
    /// <param name="configure">
    /// An action to configure the global services available to all scenes.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="configure"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the global container has already been initialized.
    /// </exception>
    /// <remarks>
    /// This method should be called once during application startup, before any
    /// scene containers are created. The global container serves as the root of
    /// the hierarchy and is never disposed until application shutdown.
    /// </remarks>
    void InitializeGlobalContainer(Action<IServiceCollection> configure);

    /// <summary>
    /// Creates a new scene container as a child of the specified parent scene.
    /// </summary>
    /// <param name="sceneName">
    /// Unique identifier for the scene. Must not already exist in the manager.
    /// </param>
    /// <param name="configure">
    /// An action to configure the scene-specific services.
    /// </param>
    /// <param name="parentSceneName">
    /// Optional name of the parent scene. If null, the global container is used as parent.
    /// </param>
    /// <returns>
    /// The newly created <see cref="IHierarchicalServiceProvider"/> for the scene.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="sceneName"/> or <paramref name="configure"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if:
    /// <list type="bullet">
    /// <item>The global container has not been initialized</item>
    /// <item>A scene with the same name already exists</item>
    /// <item>The parent scene name is specified but does not exist</item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// <para>
    /// The created container is registered with the manager and can be retrieved
    /// using <see cref="GetSceneContainer"/>. It will remain active until explicitly
    /// unloaded via <see cref="UnloadScene"/>.
    /// </para>
    /// <para><strong>Example:</strong></para>
    /// <code>
    /// // Create dungeon scene as child of global
    /// var dungeonContainer = manager.CreateSceneContainer("Dungeon", services =>
    /// {
    ///     services.AddSingleton&lt;IDungeonState, DungeonState&gt;();
    /// });
    ///
    /// // Create floor as child of dungeon
    /// var floorContainer = manager.CreateSceneContainer("DungeonFloor1", services =>
    /// {
    ///     services.AddScoped&lt;IFloorGenerator, Floor1Generator&gt;();
    /// }, parentSceneName: "Dungeon");
    /// </code>
    /// </remarks>
    IHierarchicalServiceProvider CreateSceneContainer(
        string sceneName,
        Action<IServiceCollection> configure,
        string? parentSceneName = null);

    /// <summary>
    /// Retrieves the container for the specified scene.
    /// </summary>
    /// <param name="sceneName">
    /// The name of the scene to retrieve.
    /// </param>
    /// <returns>
    /// The <see cref="IHierarchicalServiceProvider"/> for the scene, or null if not found.
    /// </returns>
    /// <remarks>
    /// This method returns null if the scene does not exist or has been unloaded.
    /// To get the global container, use the <see cref="GlobalContainer"/> property instead.
    /// </remarks>
    IHierarchicalServiceProvider? GetSceneContainer(string sceneName);

    /// <summary>
    /// Unloads a scene by disposing its container and removing it from the manager.
    /// </summary>
    /// <param name="sceneName">
    /// The name of the scene to unload.
    /// </param>
    /// <returns>
    /// true if the scene was found and unloaded; false if the scene did not exist.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method disposes the scene container (and all its children recursively)
    /// and removes it from the manager's registry. Subsequent calls to
    /// <see cref="GetSceneContainer"/> for this scene will return null.
    /// </para>
    /// <para>
    /// If the scene has child scenes, they are automatically disposed as part of
    /// the container's disposal process.
    /// </para>
    /// </remarks>
    bool UnloadScene(string sceneName);

    /// <summary>
    /// Gets all currently registered scene names.
    /// </summary>
    /// <returns>
    /// A read-only collection of scene names currently managed by this instance.
    /// </returns>
    IReadOnlyCollection<string> GetSceneNames();
}
