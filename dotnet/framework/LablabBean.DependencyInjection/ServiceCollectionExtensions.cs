namespace LablabBean.DependencyInjection;

/// <summary>
/// Extension methods for IServiceCollection to build hierarchical service providers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Builds a hierarchical service provider from the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">Optional name for the root container.</param>
    /// <returns>A new hierarchical service provider.</returns>
    public static IHierarchicalServiceProvider BuildHierarchicalServiceProvider(
        this IServiceCollection services,
        string? name = null)
    {
        return new HierarchicalServiceProvider(services, name);
    }

    /// <summary>
    /// Builds a hierarchical service provider from the service collection with options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">Service provider options (e.g., scope validation).</param>
    /// <param name="name">Optional name for the root container.</param>
    /// <returns>A new hierarchical service provider.</returns>
    public static IHierarchicalServiceProvider BuildHierarchicalServiceProvider(
        this IServiceCollection services,
        ServiceProviderOptions options,
        string? name = null)
    {
        return new HierarchicalServiceProvider(services, name, null, options);
    }

    /// <summary>
    /// Builds a hierarchical service provider with scope validation enabled.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="validateScopes">Whether to validate scopes.</param>
    /// <param name="name">Optional name for the root container.</param>
    /// <returns>A new hierarchical service provider.</returns>
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
