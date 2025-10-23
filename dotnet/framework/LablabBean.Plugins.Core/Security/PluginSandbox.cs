using System.Diagnostics;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Core.Security;

/// <summary>
/// Provides sandboxing for plugin execution
/// </summary>
public class PluginSandbox : IDisposable
{
    private readonly string _pluginId;
    private readonly PluginSecurityProfile _securityProfile;
    private readonly PluginSecurityManager _securityManager;
    private readonly ILogger<PluginSandbox> _logger;
    private readonly AssemblyLoadContext? _loadContext;
    private readonly CancellationTokenSource _executionCts = new();
    private readonly System.Threading.Timer _resourceMonitor;
    private bool _disposed;

    public PluginSandbox(
        string pluginId,
        PluginSecurityProfile securityProfile,
        PluginSecurityManager securityManager,
        ILogger<PluginSandbox> logger,
        AssemblyLoadContext? loadContext = null)
    {
        _pluginId = pluginId;
        _securityProfile = securityProfile;
        _securityManager = securityManager;
        _logger = logger;
        _loadContext = loadContext;

        // Start resource monitoring
        _resourceMonitor = new System.Threading.Timer(
            MonitorResources,
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5));

        _logger.LogInformation(
            "Created sandbox for plugin {PluginId} with trust level {TrustLevel}",
            pluginId, securityProfile.TrustLevel);
    }

    /// <summary>
    /// Execute code within the sandbox with permission checks
    /// </summary>
    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        PluginPermission requiredPermission,
        CancellationToken ct = default)
    {
        // Check permission
        var permissionCheck = _securityManager.CheckPermission(_pluginId, requiredPermission);
        if (!permissionCheck.IsAllowed)
        {
            throw new PluginSecurityException(
                $"Permission denied: {permissionCheck.DenialReason}",
                _pluginId,
                requiredPermission);
        }

        // Check resource limits before execution
        var limitCheck = _securityManager.CheckResourceLimits(_pluginId);
        if (!limitCheck.IsWithinLimits)
        {
            throw new PluginResourceLimitException(
                $"Resource limits exceeded: {string.Join(", ", limitCheck.Violations)}",
                _pluginId,
                limitCheck.Violations);
        }

        // Execute with timeout
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _executionCts.Token);
        linkedCts.CancelAfter(_securityProfile.ResourceLimits.MaxExecutionTime);

        try
        {
            var startTime = DateTime.UtcNow;
            var result = await operation();
            var executionTime = DateTime.UtcNow - startTime;

            _securityManager.RecordResourceUsage(_pluginId, usage =>
            {
                usage.TotalExecutionTime += executionTime;
            });

            return result;
        }
        catch (OperationCanceledException) when (linkedCts.Token.IsCancellationRequested)
        {
            throw new PluginExecutionTimeoutException(
                $"Plugin execution exceeded time limit of {_securityProfile.ResourceLimits.MaxExecutionTime}",
                _pluginId,
                _securityProfile.ResourceLimits.MaxExecutionTime);
        }
    }

    /// <summary>
    /// Execute code within the sandbox (void return)
    /// </summary>
    public async Task ExecuteAsync(
        Func<Task> operation,
        PluginPermission requiredPermission,
        CancellationToken ct = default)
    {
        await ExecuteAsync(async () =>
        {
            await operation();
            return true;
        }, requiredPermission, ct);
    }

    /// <summary>
    /// Monitor resource usage periodically
    /// </summary>
    private void MonitorResources(object? state)
    {
        if (_disposed) return;

        try
        {
            // Get current process for memory tracking
            var currentMemory = GC.GetTotalMemory(false);

            // Update resource usage
            _securityManager.RecordResourceUsage(_pluginId, usage =>
            {
                usage.MemoryUsage = currentMemory;
                usage.ThreadCount = Process.GetCurrentProcess().Threads.Count;
            });

            // Check limits
            var limitCheck = _securityManager.CheckResourceLimits(_pluginId);
            if (!limitCheck.IsWithinLimits)
            {
                _logger.LogWarning(
                    "Plugin {PluginId} resource limit violations: {Violations}",
                    _pluginId, string.Join(", ", limitCheck.Violations));

                // Could trigger automatic plugin shutdown here
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring resources for plugin {PluginId}", _pluginId);
        }
    }

    /// <summary>
    /// Terminate sandbox execution
    /// </summary>
    public void Terminate()
    {
        _logger.LogWarning("Terminating sandbox for plugin {PluginId}", _pluginId);
        _executionCts.Cancel();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _resourceMonitor?.Dispose();
        _executionCts?.Dispose();

        _disposed = true;
    }
}

/// <summary>
/// Exception thrown when a plugin violates security policy
/// </summary>
public class PluginSecurityException : Exception
{
    public string PluginId { get; }
    public PluginPermission RequiredPermission { get; }

    public PluginSecurityException(string message, string pluginId, PluginPermission requiredPermission)
        : base(message)
    {
        PluginId = pluginId;
        RequiredPermission = requiredPermission;
    }
}

/// <summary>
/// Exception thrown when a plugin exceeds resource limits
/// </summary>
public class PluginResourceLimitException : Exception
{
    public string PluginId { get; }
    public List<string> Violations { get; }

    public PluginResourceLimitException(string message, string pluginId, List<string> violations)
        : base(message)
    {
        PluginId = pluginId;
        Violations = violations;
    }
}

/// <summary>
/// Exception thrown when a plugin execution times out
/// </summary>
public class PluginExecutionTimeoutException : Exception
{
    public string PluginId { get; }
    public TimeSpan TimeLimit { get; }

    public PluginExecutionTimeoutException(string message, string pluginId, TimeSpan timeLimit)
        : base(message)
    {
        PluginId = pluginId;
        TimeLimit = timeLimit;
    }
}
