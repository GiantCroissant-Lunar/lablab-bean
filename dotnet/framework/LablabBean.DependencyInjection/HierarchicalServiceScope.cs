namespace LablabBean.DependencyInjection;

/// <summary>
/// Represents a scope in the hierarchical service provider.
/// Provides a scoped service provider that maintains hierarchy context.
/// </summary>
public sealed class HierarchicalServiceScope : IServiceScope
{
    private readonly IServiceScope _innerScope;
    private readonly ScopedHierarchicalServiceProvider _scopedProvider;
    private bool _isDisposed;

    internal HierarchicalServiceScope(
        IServiceScope innerScope,
        IHierarchicalServiceProvider parentContainer)
    {
        _innerScope = innerScope ?? throw new ArgumentNullException(nameof(innerScope));
        _scopedProvider = new ScopedHierarchicalServiceProvider(
            innerScope.ServiceProvider,
            parentContainer);
    }

    /// <summary>
    /// Gets the service provider for this scope.
    /// </summary>
    public IServiceProvider ServiceProvider => _scopedProvider;

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _innerScope.Dispose();
        _isDisposed = true;
    }

    /// <summary>
    /// Scoped service provider that wraps the inner scope and maintains hierarchy.
    /// </summary>
    private sealed class ScopedHierarchicalServiceProvider : IHierarchicalServiceProvider
    {
        private readonly IServiceProvider _scopedProvider;
        private readonly IHierarchicalServiceProvider _parentContainer;

        public ScopedHierarchicalServiceProvider(
            IServiceProvider scopedProvider,
            IHierarchicalServiceProvider parentContainer)
        {
            _scopedProvider = scopedProvider;
            _parentContainer = parentContainer;
        }

        public string Name => $"{_parentContainer.Name}[Scope]";
        public IHierarchicalServiceProvider? Parent => _parentContainer.Parent;
        public IReadOnlyList<IHierarchicalServiceProvider> Children => Array.Empty<IHierarchicalServiceProvider>();
        public int Depth => _parentContainer.Depth;
        public bool IsDisposed => false;

        public object? GetService(Type serviceType)
        {
            // Try scoped provider first
            var service = _scopedProvider.GetService(serviceType);
            if (service != null)
            {
                return service;
            }

            // Fallback to parent hierarchy
            return _parentContainer.GetService(serviceType);
        }

        public IHierarchicalServiceProvider CreateChildContainer(
            Action<IServiceCollection> configureServices,
            string? name = null)
        {
            throw new NotSupportedException(
                "Cannot create child containers from a scoped provider. " +
                "Create child containers from the root container instead.");
        }

        public string GetHierarchyPath() => $"{_parentContainer.GetHierarchyPath()}[Scope]";

        public void Dispose()
        {
            // Disposal handled by outer HierarchicalServiceScope
        }
    }
}
