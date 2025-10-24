namespace LablabBean.DependencyInjection.Exceptions;

/// <summary>
/// Exception thrown when attempting to use a disposed container.
/// </summary>
public class ContainerDisposedException : ObjectDisposedException
{
    public ContainerDisposedException(string? containerName = null)
        : base(containerName ?? "HierarchicalServiceProvider",
               BuildMessage(containerName))
    {
        ContainerName = containerName;
    }

    public string? ContainerName { get; }

    private static string BuildMessage(string? containerName)
    {
        var container = containerName != null ? $" '{containerName}'" : "";
        return $"Cannot access disposed container{container}.";
    }
}
