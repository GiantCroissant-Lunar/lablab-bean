namespace LablabBean.DependencyInjection.Diagnostics;

/// <summary>
/// Diagnostic event source for hierarchical container lifecycle.
/// </summary>
public static class HierarchicalContainerDiagnostics
{
    /// <summary>
    /// Raised when a container is created (root or child).
    /// </summary>
    public static event EventHandler<ContainerEventArgs>? ContainerCreated;

    /// <summary>
    /// Raised when a container is disposed.
    /// </summary>
    public static event EventHandler<ContainerEventArgs>? ContainerDisposed;

    /// <summary>
    /// Raised when a child container is added to a parent.
    /// </summary>
    public static event EventHandler<ContainerEventArgs>? ChildAdded;

    /// <summary>
    /// Raised when a child container is removed from a parent.
    /// </summary>
    public static event EventHandler<ContainerEventArgs>? ChildRemoved;

    /// <summary>
    /// Raised when a required service resolution fails.
    /// </summary>
    public static event EventHandler<ResolveFailureEventArgs>? ResolveFailed;

    internal static void RaiseContainerCreated(HierarchicalServiceProvider provider)
    {
        var args = new ContainerEventArgs(provider.Id, provider.Name, provider.Depth,
            (provider.Parent as HierarchicalServiceProvider)?.Id, provider.Parent?.Name);
        // EventSource emission (no-op unless enabled)
        HierarchicalContainerEventSource.Log.ContainerCreated(
            provider.Id,
            provider.Name,
            provider.Depth,
            (provider.Parent as HierarchicalServiceProvider)?.Id ?? Guid.Empty,
            provider.Parent?.Name ?? string.Empty);
        ContainerCreated?.Invoke(provider, args);
    }

    internal static void RaiseContainerDisposed(HierarchicalServiceProvider provider)
    {
        var args = new ContainerEventArgs(provider.Id, provider.Name, provider.Depth,
            (provider.Parent as HierarchicalServiceProvider)?.Id, provider.Parent?.Name);
        // EventSource emission (no-op unless enabled)
        HierarchicalContainerEventSource.Log.ContainerDisposed(
            provider.Id,
            provider.Name,
            provider.Depth,
            (provider.Parent as HierarchicalServiceProvider)?.Id ?? Guid.Empty,
            provider.Parent?.Name ?? string.Empty);
        ContainerDisposed?.Invoke(provider, args);
    }

    internal static void RaiseChildAdded(HierarchicalServiceProvider child)
    {
        var args = new ContainerEventArgs(child.Id, child.Name, child.Depth,
            (child.Parent as HierarchicalServiceProvider)?.Id, child.Parent?.Name);
        HierarchicalContainerEventSource.Log.ChildAdded(
            child.Id,
            child.Name,
            (child.Parent as HierarchicalServiceProvider)?.Id ?? Guid.Empty,
            child.Parent?.Name ?? string.Empty);
        ChildAdded?.Invoke(child, args);
    }

    internal static void RaiseChildRemoved(HierarchicalServiceProvider child)
    {
        var args = new ContainerEventArgs(child.Id, child.Name, child.Depth,
            (child.Parent as HierarchicalServiceProvider)?.Id, child.Parent?.Name);
        HierarchicalContainerEventSource.Log.ChildRemoved(
            child.Id,
            child.Name,
            (child.Parent as HierarchicalServiceProvider)?.Id ?? Guid.Empty,
            child.Parent?.Name ?? string.Empty);
        ChildRemoved?.Invoke(child, args);
    }

    internal static void RaiseResolveFailure(HierarchicalServiceProvider provider, string serviceType, Exception exception)
    {
        HierarchicalContainerEventSource.Log.ResolveFailure(provider.Id, provider.Name, provider.Depth, serviceType, exception.GetType().FullName ?? exception.GetType().Name, exception.Message);
        ResolveFailed?.Invoke(provider, new ResolveFailureEventArgs(provider.Id, provider.Name, provider.Depth, serviceType, exception));
    }
}
