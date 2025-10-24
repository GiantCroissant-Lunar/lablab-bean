namespace LablabBean.DependencyInjection.Diagnostics;

/// <summary>
/// Event data for hierarchical container lifecycle events.
/// </summary>
public sealed class ContainerEventArgs : EventArgs
{
    public ContainerEventArgs(Guid containerId, string name, int depth, Guid? parentId, string? parentName)
    {
        ContainerId = containerId;
        Name = name;
        Depth = depth;
        ParentId = parentId;
        ParentName = parentName;
        TimestampUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Name of the container.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Depth in the hierarchy (0 = root).
    /// </summary>
    public int Depth { get; }

    /// <summary>
    /// Unique identifier of the container.
    /// </summary>
    public Guid ContainerId { get; }

    /// <summary>
    /// Unique identifier of the parent container (if any).
    /// </summary>
    public Guid? ParentId { get; }

    /// <summary>
    /// Name of the parent container, if any.
    /// </summary>
    public string? ParentName { get; }

    /// <summary>
    /// When the event was emitted (UTC).
    /// </summary>
    public DateTime TimestampUtc { get; }
}
