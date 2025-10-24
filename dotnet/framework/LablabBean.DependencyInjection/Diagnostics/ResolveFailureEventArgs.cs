namespace LablabBean.DependencyInjection.Diagnostics;

/// <summary>
/// Event data for required service resolution failures.
/// </summary>
public sealed class ResolveFailureEventArgs : EventArgs
{
    public ResolveFailureEventArgs(Guid containerId, string containerName, int depth, string serviceType, Exception exception)
    {
        ContainerId = containerId;
        ContainerName = containerName;
        Depth = depth;
        ServiceType = serviceType;
        Exception = exception;
        TimestampUtc = DateTime.UtcNow;
    }

    public Guid ContainerId { get; }
    public string ContainerName { get; }
    public int Depth { get; }
    public string ServiceType { get; }
    public Exception Exception { get; }
    public DateTime TimestampUtc { get; }
}
