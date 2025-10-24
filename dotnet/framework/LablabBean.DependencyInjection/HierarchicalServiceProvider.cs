using LablabBean.DependencyInjection.Exceptions;

namespace LablabBean.DependencyInjection;

/// <summary>
/// Hierarchical service provider implementation supporting parent-child container relationships.
/// </summary>
public sealed class HierarchicalServiceProvider : IHierarchicalServiceProvider, ISupportRequiredService, IServiceScopeFactory
{
    private const int MaxDepth = 10;
    private readonly IServiceProvider _innerProvider;
    private readonly List<HierarchicalServiceProvider> _children = new();
    private bool _isDisposed;

    public HierarchicalServiceProvider(
        IServiceCollection services,
        string? name = null,
        HierarchicalServiceProvider? parent = null,
        ServiceProviderOptions? options = null)
    {
        Name = name ?? (parent == null ? "Root" : $"Child-{Guid.NewGuid():N}");
        Parent = parent;
        Depth = parent?.Depth + 1 ?? 0;

        // Register self as IHierarchicalServiceProvider for service location pattern
        services.AddSingleton<IHierarchicalServiceProvider>(this);
        services.AddSingleton<IServiceProvider>(this);

        _innerProvider = options != null
            ? services.BuildServiceProvider(options)
            : services.BuildServiceProvider();
    }

    public string Name { get; }
    public IHierarchicalServiceProvider? Parent { get; }
    public IReadOnlyList<IHierarchicalServiceProvider> Children => _children.AsReadOnly();
    public int Depth { get; }
    public bool IsDisposed => _isDisposed;

    public object? GetService(Type serviceType)
    {
        ThrowIfDisposed();

        var service = _innerProvider.GetService(serviceType);
        if (service != null || Parent == null)
        {
            return service;
        }

        return Parent.GetService(serviceType);
    }

    public object GetRequiredService(Type serviceType)
    {
        ThrowIfDisposed();

        var service = GetService(serviceType);
        if (service == null)
        {
            throw new ServiceResolutionException(serviceType, Name);
        }

        return service;
    }

    public IHierarchicalServiceProvider CreateChildContainer(
        Action<IServiceCollection> configureServices,
        string? name = null)
    {
        ThrowIfDisposed();

        if (Depth >= MaxDepth)
        {
            throw new InvalidOperationException(
                $"Cannot create child container: maximum depth ({MaxDepth}) exceeded. Current depth: {Depth}");
        }

        var services = new ServiceCollection();
        configureServices(services);

        var child = new HierarchicalServiceProvider(services, name, this);
        _children.Add(child);

        return child;
    }

    public string GetHierarchyPath()
    {
        if (Parent == null)
        {
            return Name;
        }

        return $"{Parent.GetHierarchyPath()} → {Name}";
    }

    public IServiceScope CreateScope()
    {
        ThrowIfDisposed();

        // ServiceProvider always supports creating scopes via the extension method
        // We just need to get a scope from it
        var scopeFactory = _innerProvider.GetService(typeof(IServiceScopeFactory)) as IServiceScopeFactory;
        if (scopeFactory != null)
        {
            var innerScope = scopeFactory.CreateScope();
            return new HierarchicalServiceScope(innerScope, this);
        }

        // Fallback: ServiceProvider always implements IServiceScopeFactory internally
        // This should never happen with MSDI ServiceProvider
        throw new InvalidOperationException(
            "Inner service provider does not support scopes. " +
            "This should not happen with Microsoft.Extensions.DependencyInjection.");
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        foreach (var child in _children.ToArray())
        {
            child.Dispose();
        }

        _children.Clear();

        if (_innerProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _isDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ContainerDisposedException(Name);
        }
    }
}
