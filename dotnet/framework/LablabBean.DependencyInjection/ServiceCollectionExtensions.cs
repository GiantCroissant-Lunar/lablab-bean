namespace LablabBean.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to build hierarchical service providers.
/// </summary>
/// <remarks>
/// These extension methods provide a familiar API for creating hierarchical service providers
/// that is consistent with the standard Microsoft.Extensions.DependencyInjection patterns.
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Builds a hierarchical service provider from the service collection.
    /// </summary>
    /// <param name="services">The service collection containing service registrations.</param>
    /// <param name="name">Optional name for the root container (defaults to "Root" if not specified).</param>
    /// <returns>A new <see cref="IHierarchicalServiceProvider"/> root container.</returns>
    /// <remarks>
    /// This creates a root-level container with no parent. Use <see cref="IHierarchicalServiceProvider.CreateChildContainer"/>
    /// to create child containers that can resolve services from this parent.
    /// </remarks>
    /// <example>
    /// <code>
    /// var services = new ServiceCollection();
    /// services.AddSingleton&lt;IMyService, MyService&gt;();
    /// var provider = services.BuildHierarchicalServiceProvider("Global");
    /// </code>
    /// </example>
    public static IHierarchicalServiceProvider BuildHierarchicalServiceProvider(
        this IServiceCollection services,
        string? name = null)
    {
        return new HierarchicalServiceProvider(services, name);
    }

    /// <summary>
    /// Builds a hierarchical service provider from the service collection with advanced options.
    /// </summary>
    /// <param name="services">The service collection containing service registrations.</param>
    /// <param name="options">Service provider options (e.g., scope validation, service validation on build).</param>
    /// <param name="name">Optional name for the root container.</param>
    /// <returns>A new <see cref="IHierarchicalServiceProvider"/> root container with the specified options.</returns>
    /// <remarks>
    /// Use this overload when you need fine-grained control over service provider behavior,
    /// such as enabling scope validation in development environments.
    /// </remarks>
    /// <example>
    /// <code>
    /// var options = new ServiceProviderOptions
    /// {
    ///     ValidateScopes = true,
    ///     ValidateOnBuild = true
    /// };
    /// var provider = services.BuildHierarchicalServiceProvider(options, "Global");
    /// </code>
    /// </example>
    public static IHierarchicalServiceProvider BuildHierarchicalServiceProvider(
        this IServiceCollection services,
        ServiceProviderOptions options,
        string? name = null)
    {
        return new HierarchicalServiceProvider(services, name, null, options);
    }

    /// <summary>
    /// Builds a hierarchical service provider with scope validation enabled or disabled.
    /// </summary>
    /// <param name="services">The service collection containing service registrations.</param>
    /// <param name="validateScopes">Whether to validate that scoped services are not resolved from the root provider.</param>
    /// <param name="name">Optional name for the root container.</param>
    /// <returns>A new <see cref="IHierarchicalServiceProvider"/> root container with scope validation configured.</returns>
    /// <remarks>
    /// Scope validation should be enabled in development to catch errors where scoped services
    /// are incorrectly resolved from non-scoped contexts. This can be disabled in production for performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Development: Enable validation
    /// var devProvider = services.BuildHierarchicalServiceProvider(validateScopes: true);
    /// 
    /// // Production: Disable for performance
    /// var prodProvider = services.BuildHierarchicalServiceProvider(validateScopes: false);
    /// </code>
    /// </example>
    public static IHierarchicalServiceProvider BuildHierarchicalServiceProvider(
        this IServiceCollection services,
        bool validateScopes,
        string? name = null)
    {
        var options = new ServiceProviderOptions
        {
            ValidateScopes = validateScopes,
            ValidateOnBuild = validateScopes
        };
        return new HierarchicalServiceProvider(services, name, null, options);
    }
}
