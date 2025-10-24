# Hierarchical DI Diagnostics Guide

This guide explains how to observe and trace the Hierarchical DI container using in‑process events, EventSource (ETW), and ActivitySource for OpenTelemetry.

## Overview

- In‑process events (C# events)
- EventSource: structured, low‑overhead tracing when enabled
- ActivitySource: spans for service resolution to integrate with OTEL

Applies to: `LablabBean.DependencyInjection` (net8.0)

## In‑Process Events

Namespace: `LablabBean.DependencyInjection.Diagnostics`

Events on `HierarchicalContainerDiagnostics`:

- `ContainerCreated`
- `ContainerDisposed`
- `ChildAdded`
- `ChildRemoved`
- `ResolveFailed`

Event args:

- `ContainerEventArgs`: `ContainerId`, `Name`, `Depth`, `ParentId`, `ParentName`, `TimestampUtc`
- `ResolveFailureEventArgs`: `ContainerId`, `ContainerName`, `Depth`, `ServiceType`, `Exception`, `TimestampUtc`

Example:

```csharp
using LablabBean.DependencyInjection.Diagnostics;

HierarchicalContainerDiagnostics.ContainerCreated += (_, e) =>
    Console.WriteLine($"[CREATED] {e.ParentName} -> {e.Name} depth={e.Depth} id={e.ContainerId}");

HierarchicalContainerDiagnostics.ResolveFailed += (_, e) =>
    Console.WriteLine($"[RESOLVE-FAIL] {e.ServiceType} in {e.ContainerName}: {e.Exception.Message}");
```

## EventSource (ETW)

Provider name: `LablabBean.DependencyInjection.HierarchicalContainer`

Events:

- `ContainerCreated(id, name, depth, parentId, parentName)`
- `ContainerDisposed(id, name, depth, parentId, parentName)`
- `ChildAdded(childId, childName, parentId, parentName)`
- `ChildRemoved(childId, childName, parentId, parentName)`
- `ResolveStart(containerId, name, depth, serviceType)`
- `ResolveStop(containerId, name, depth, serviceType, outcome, resolvedDepth)`
- `ResolveFailure(containerId, name, depth, serviceType, exceptionType, exceptionMessage)`

Notes:

- `outcome`: 0 = LocalHit, 1 = ParentHit, 2 = NotFound
- Emission is a no‑op unless the EventSource is enabled by a listener/tool

Minimal in‑process listener:

```csharp
using System.Diagnostics.Tracing;
using LablabBean.DependencyInjection.Diagnostics;

sealed class ConsoleEventListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "LablabBean.DependencyInjection.HierarchicalContainer")
        {
            EnableEvents(eventSource, EventLevel.Verbose);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs e)
    {
        Console.WriteLine($"{e.EventName}: {string.Join(",", e.Payload ?? new())}");
    }
}
```

## ActivitySource (OpenTelemetry)

Source name: `LablabBean.DependencyInjection.HierarchicalContainer`

Emitted around `GetService(Type)` resolution:

Tags:

- `di.service.type`
- `di.container.name`
- `di.container.depth`
- `di.container.id`
- `di.resolve.outcome` (LocalHit|ParentHit|NotFound)
- `di.resolved.depth` (when resolved)

Enable in code:

```csharp
using System.Diagnostics;

var listener = new ActivityListener
{
    ShouldListenTo = s => s.Name == "LablabBean.DependencyInjection.HierarchicalContainer",
    Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
};
ActivitySource.AddActivityListener(listener);
```

## Performance

- EventSource methods check `IsEnabled()`; overhead is negligible when disabled.
- Activity spans are created only if `ActivitySource.HasListeners()` is true.
- Resolve events avoid double‑emission across the parent chain.

## Troubleshooting

- Not seeing events? Verify your listener is subscribed to the exact provider/source name and level.
- Excessive logs? Lower EventSource level from `Verbose` to `Informational`.
- OTEL exporter not seeing spans? Ensure sampling includes the DI source, or set `Sample` to record all.
