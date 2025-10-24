namespace LablabBean.DependencyInjection.Exceptions;

/// <summary>
/// Exception thrown when service resolution fails in a hierarchical service provider.
/// </summary>
/// <remarks>
/// This exception is thrown when:
/// <list type="bullet">
/// <item>A required service is not registered in the container or its parent hierarchy</item>
/// <item>Service activation fails due to constructor dependency resolution failure</item>
/// <item>Circular dependencies are detected during service resolution</item>
/// </list>
/// </remarks>
public class ServiceResolutionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResolutionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ServiceResolutionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResolutionException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ServiceResolutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResolutionException"/> class for a specific service type and container.
    /// </summary>
    /// <param name="serviceType">The type of service that failed to resolve.</param>
    /// <param name="containerName">Optional name of the container where resolution failed.</param>
    public ServiceResolutionException(Type serviceType, string? containerName = null)
        : base(BuildMessage(serviceType, containerName))
    {
        ServiceType = serviceType;
        ContainerName = containerName;
    }

    /// <summary>
    /// Gets the type of service that failed to resolve.
    /// </summary>
    public Type? ServiceType { get; }

    /// <summary>
    /// Gets the name of the container where the resolution failure occurred.
    /// </summary>
    public string? ContainerName { get; }

    private static string BuildMessage(Type serviceType, string? containerName)
    {
        var container = containerName != null ? $" in container '{containerName}'" : "";
        return $"Failed to resolve service of type '{serviceType.FullName}'{container}.";
    }
}
