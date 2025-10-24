using LablabBean.DependencyInjection.Exceptions;
using LablabBean.DependencyInjection.Diagnostics;
using System.Diagnostics;

namespace LablabBean.DependencyInjection;

/// <summary>
/// Hierarchical service provider implementation supporting parent-child container relationships.
/// </summary>
public sealed class HierarchicalServiceProvider : IHierarchicalServiceProvider, ISupportRequiredService, IServiceScopeFactory, IServiceProviderIsService
{
    private const int MaxDepth = 10;
    private readonly IServiceProvider _innerProvider;
    private readonly List<HierarchicalServiceProvider> _children = new();
    private bool _isDisposed;
    internal Guid Id { get; } = Guid.NewGuid();

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

        // Emit diagnostic event after construction completes
        HierarchicalContainerDiagnostics.RaiseContainerCreated(this);
    }

    public string Name { get; }
    public IHierarchicalServiceProvider? Parent { get; }
    public IReadOnlyList<IHierarchicalServiceProvider> Children => _children.AsReadOnly();
    public int Depth { get; }
    public bool IsDisposed => _isDisposed;

    public object? GetService(Type serviceType)
    {
        ThrowIfDisposed();
        var serviceTypeName = serviceType.FullName ?? serviceType.Name;

        // Emit start diagnostics
        HierarchicalContainerEventSource.Log.ResolveStart(Id, Name, Depth, serviceTypeName);

        Activity? activity = null;
        if (HierarchicalContainerActivity.Source.HasListeners())
        {
            activity = HierarchicalContainerActivity.Source.StartActivity("di.resolve", ActivityKind.Internal);
            if (activity is not null)
            {
                activity.SetTag("di.service.type", serviceTypeName);
                activity.SetTag("di.container.name", Name);
                activity.SetTag("di.container.depth", Depth);
                activity.SetTag("di.container.id", Id);
            }
        }

        var service = ResolveInternal(serviceType, out var resolvedDepth);

        var outcome = service is not null
            ? (resolvedDepth == Depth ? ResolveOutcome.LocalHit : ResolveOutcome.ParentHit)
            : ResolveOutcome.NotFound;

        // Emit stop diagnostics with outcome
        HierarchicalContainerEventSource.Log.ResolveStop(Id, Name, Depth, serviceTypeName, (int)outcome, service is null ? -1 : resolvedDepth);

        if (activity is not null)
        {
            activity.SetTag("di.resolve.outcome", outcome.ToString());
            if (service is not null)
            {
                activity.SetTag("di.resolved.depth", resolvedDepth);
            }
            activity.Dispose();
        }

        return service;
    }

    public object GetRequiredService(Type serviceType)
    {
        ThrowIfDisposed();

        var service = GetService(serviceType);
        if (service == null)
        {
            var ex = new ServiceResolutionException(serviceType, Name);
            var serviceTypeName = serviceType.FullName ?? serviceType.Name;
            HierarchicalContainerDiagnostics.RaiseResolveFailure(this, serviceTypeName, ex);
            throw ex;
        }

        return service;
    }

    private object? ResolveInternal(Type serviceType, out int resolvedDepth)
    {
        // Try local
        var local = _innerProvider.GetService(serviceType);
        if (local is not null)
        {
            resolvedDepth = Depth;
            return local;
        }

        // Try parent chain without re-emitting diagnostics
        if (Parent is HierarchicalServiceProvider parent)
        {
            return parent.ResolveInternal(serviceType, out resolvedDepth);
        }
        else if (Parent is not null)
        {
            var svc = Parent.GetService(serviceType);
            resolvedDepth = svc is not null ? Parent.Depth : -1;
            return svc;
        }

        resolvedDepth = -1;
        return null;
    }

    public bool IsService(Type serviceType)
    {
        ThrowIfDisposed();

        // Prefer the inner provider's non-instantiating check when available
        if (_innerProvider is IServiceProviderIsService isService)
        {
            if (isService.IsService(serviceType))
            {
                return true;
            }
        }
        else
        {
            // Fallback: attempt a best-effort check (may instantiate)
            var local = _innerProvider.GetService(serviceType);
            if (local is not null)
            {
                return true;
            }
        }

        // Delegate to parent if not available locally
        return Parent is IServiceProviderIsService parentIsService && parentIsService.IsService(serviceType);
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
        HierarchicalContainerDiagnostics.RaiseChildAdded(child);

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

        // Emit diagnostic event after disposal completes
        HierarchicalContainerDiagnostics.RaiseContainerDisposed(this);

        // If we have a parent, remove ourselves from its children and emit event
        if (Parent is HierarchicalServiceProvider parent)
        {
            if (parent._children.Remove(this))
            {
                HierarchicalContainerDiagnostics.RaiseChildRemoved(this);
            }
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ContainerDisposedException(Name);
        }
    }
}
