namespace LablabBean.DependencyInjection;

/// <summary>
/// Factory for creating hierarchical service providers.
/// Implements IServiceProviderFactory for integration with hosting infrastructure.
/// </summary>
public sealed class HierarchicalServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    private readonly ServiceProviderOptions? _options;
    private readonly string? _name;

    /// <summary>
    /// Creates a new factory with default options.
    /// </summary>
    public HierarchicalServiceProviderFactory()
        : this(null, null)
    {
    }

    /// <summary>
    /// Creates a new factory with the specified options.
    /// </summary>
    /// <param name="options">Service provider options.</param>
    /// <param name="name">Optional name for containers created by this factory.</param>
    public HierarchicalServiceProviderFactory(ServiceProviderOptions? options, string? name = null)
    {
        _options = options;
        _name = name;
    }

    /// <summary>
    /// Creates a container builder from a service collection.
    /// </summary>
    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        return services;
    }

    /// <summary>
    /// Creates a hierarchical service provider from the container builder.
    /// </summary>
    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        return new HierarchicalServiceProvider(containerBuilder, _name, null, _options);
    }
}
