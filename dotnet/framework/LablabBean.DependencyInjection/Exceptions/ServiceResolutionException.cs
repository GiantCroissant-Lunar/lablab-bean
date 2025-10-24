namespace LablabBean.DependencyInjection.Exceptions;

/// <summary>
/// Exception thrown when service resolution fails.
/// </summary>
public class ServiceResolutionException : Exception
{
    public ServiceResolutionException(string message) : base(message)
    {
    }

    public ServiceResolutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ServiceResolutionException(Type serviceType, string? containerName = null)
        : base(BuildMessage(serviceType, containerName))
    {
        ServiceType = serviceType;
        ContainerName = containerName;
    }

    public Type? ServiceType { get; }
    public string? ContainerName { get; }

    private static string BuildMessage(Type serviceType, string? containerName)
    {
        var container = containerName != null ? $" in container '{containerName}'" : "";
        return $"Failed to resolve service of type '{serviceType.FullName}'{container}.";
    }
}
