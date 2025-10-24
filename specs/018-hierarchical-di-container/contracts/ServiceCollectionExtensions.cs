namespace LablabBean.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to create hierarchical service providers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Builds a hierarchical service provider from the service collection.
    /// </summary>
    /// <param name="services">The service collection to build from.</param>
    /// <param name="name">Optional name for the root container.</param>
    /// <returns>
    /// A new <see cref="IHierarchicalServiceProvider"/> that can create child containers.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <code>
    /// var factory = new HierarchicalServiceProviderFactory();
    /// var containerBuilder = factory.CreateBuilder(services);
    /// return factory.CreateServiceProvider(containerBuilder);
    /// </code>
    /// </para>
    /// <para><strong>Example:</strong></para>
    /// <code>
    /// var services = new ServiceCollection();
    /// services.AddSingleton&lt;ISaveSystem, SaveSystem&gt;();
    /// services.AddSingleton&lt;IAudioManager, AudioManager&gt;();
    ///
    /// var globalContainer = services.BuildHierarchicalServiceProvider("Global");
    ///
    /// // Create child container
    /// var sceneContainer = globalContainer.CreateChildContainer(s =>
    /// {
    ///     s.AddScoped&lt;ISceneService, SceneService&gt;();
    /// }, "MainMenu");
    /// </code>
    /// </remarks>
    public static IHierarchicalServiceProvider BuildHierarchicalServiceProvider(
        this IServiceCollection services,
        string? name = null);

    /// <summary>
    /// Builds a hierarchical service provider with validation enabled.
    /// </summary>
    /// <param name="services">The service collection to build from.</param>
    /// <param name="validateScopes">
    /// Whether to validate that scoped services are not resolved from root provider.
    /// </param>
    /// <param name="name">Optional name for the root container.</param>
    /// <returns>
    /// A new <see cref="IHierarchicalServiceProvider"/> with validation enabled.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// When <paramref name="validateScopes"/> is true, the provider will throw an
    /// <see cref="InvalidOperationException"/> if a scoped service is resolved from
    /// the root provider instead of from a scope. This is useful for detecting
    /// incorrect service lifetime usage during development.
    /// </para>
    /// <para>
    /// Scope validation should generally be enabled in development and disabled
    /// in production for performance reasons.
    /// </para>
    /// </remarks>
    public static IHierarchicalServiceProvider BuildHierarchicalServiceProvider(
        this IServiceCollection services,
        bool validateScopes,
        string? name = null);
}
