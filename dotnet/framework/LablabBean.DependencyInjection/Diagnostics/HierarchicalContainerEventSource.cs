using System.Diagnostics.Tracing;

namespace LablabBean.DependencyInjection.Diagnostics;

/// <summary>
/// ETW/EventSource for hierarchical container lifecycle events.
/// Enables structured tracing with minimal overhead when disabled.
/// </summary>
public sealed class HierarchicalContainerEventSource : EventSource
{
    public static readonly HierarchicalContainerEventSource Log = new();

    private HierarchicalContainerEventSource() : base("LablabBean.DependencyInjection.HierarchicalContainer") { }

    private const int ContainerCreatedEventId = 1;
    private const int ContainerDisposedEventId = 2;
    private const int ResolveStartEventId = 3;
    private const int ResolveStopEventId = 4;
    private const int ResolveFailureEventId = 5;
    private const int ChildAddedEventId = 6;
    private const int ChildRemovedEventId = 7;

    [Event(ContainerCreatedEventId, Level = EventLevel.Informational, Message = "Container created: {1} ({0}), depth {2}, parent {4} ({3})")]
    public void ContainerCreated(Guid containerId, string name, int depth, Guid parentId, string parentName)
    {
        if (IsEnabled())
        {
            WriteEvent(ContainerCreatedEventId, containerId, name, depth, parentId, parentName ?? string.Empty);
        }
    }

    [Event(ContainerDisposedEventId, Level = EventLevel.Informational, Message = "Container disposed: {1} ({0}), depth {2}, parent {4} ({3})")]
    public void ContainerDisposed(Guid containerId, string name, int depth, Guid parentId, string parentName)
    {
        if (IsEnabled())
        {
            WriteEvent(ContainerDisposedEventId, containerId, name, depth, parentId, parentName ?? string.Empty);
        }
    }

    [Event(ResolveStartEventId, Level = EventLevel.Verbose, Message = "Resolve start: {3} on {1} ({0}) depth {2}")]
    public void ResolveStart(Guid containerId, string name, int depth, string serviceType)
    {
        if (IsEnabled())
        {
            WriteEvent(ResolveStartEventId, containerId, name, depth, serviceType ?? string.Empty);
        }
    }

    [Event(ResolveStopEventId, Level = EventLevel.Verbose, Message = "Resolve stop: {3} outcome {4} resolvedDepth {5} on {1} ({0}) depth {2}")]
    public void ResolveStop(Guid containerId, string name, int depth, string serviceType, int outcome, int resolvedDepth)
    {
        if (IsEnabled())
        {
            WriteEvent(ResolveStopEventId, containerId, name, depth, serviceType ?? string.Empty, outcome, resolvedDepth);
        }
    }

    [Event(ResolveFailureEventId, Level = EventLevel.Warning, Message = "Resolve failure: {3} on {1} ({0}) depth {2}: {4} - {5}")]
    public void ResolveFailure(Guid containerId, string name, int depth, string serviceType, string exceptionType, string exceptionMessage)
    {
        if (IsEnabled())
        {
            WriteEvent(ResolveFailureEventId, containerId, name, depth, serviceType ?? string.Empty, exceptionType ?? string.Empty, exceptionMessage ?? string.Empty);
        }
    }

    [Event(ChildAddedEventId, Level = EventLevel.Informational, Message = "Child added: {1} ({0}) -> parent {3} ({2})")]
    public void ChildAdded(Guid childId, string childName, Guid parentId, string parentName)
    {
        if (IsEnabled())
        {
            WriteEvent(ChildAddedEventId, childId, childName ?? string.Empty, parentId, parentName ?? string.Empty);
        }
    }

    [Event(ChildRemovedEventId, Level = EventLevel.Informational, Message = "Child removed: {1} ({0}) <- parent {3} ({2})")]
    public void ChildRemoved(Guid childId, string childName, Guid parentId, string parentName)
    {
        if (IsEnabled())
        {
            WriteEvent(ChildRemovedEventId, childId, childName ?? string.Empty, parentId, parentName ?? string.Empty);
        }
    }
}
