namespace LablabBean.DependencyInjection.Exceptions;

/// <summary>
/// Exception thrown when attempting to use a disposed hierarchical service provider container.
/// </summary>
/// <remarks>
/// This exception is thrown when operations are attempted on a container after <see cref="IDisposable.Dispose"/> has been called.
/// Once a container is disposed, all service resolution attempts will fail with this exception.
/// </remarks>
public class ContainerDisposedException : ObjectDisposedException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerDisposedException"/> class.
    /// </summary>
    /// <param name="containerName">Optional name of the disposed container.</param>
    public ContainerDisposedException(string? containerName = null)
        : base(containerName ?? "HierarchicalServiceProvider",
               BuildMessage(containerName))
    {
        ContainerName = containerName;
    }

    /// <summary>
    /// Gets the name of the disposed container.
    /// </summary>
    public string? ContainerName { get; }

    private static string BuildMessage(string? containerName)
    {
        var container = containerName != null ? $" '{containerName}'" : "";
        return $"Cannot access disposed container{container}.";
    }
}
