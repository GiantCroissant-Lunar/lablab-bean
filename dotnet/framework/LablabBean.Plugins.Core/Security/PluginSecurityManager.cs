using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Core.Security;

/// <summary>
/// Manages plugin security, permissions, and sandboxing
/// </summary>
public class PluginSecurityManager
{
    private readonly Dictionary<string, PluginSecurityProfile> _securityProfiles = new();
    private readonly Dictionary<string, ResourceUsage> _resourceUsage = new();
    private readonly ILogger<PluginSecurityManager> _logger;
    private readonly object _lock = new();

    public PluginSecurityManager(ILogger<PluginSecurityManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Register a security profile for a plugin
    /// </summary>
    public void RegisterProfile(PluginSecurityProfile profile)
    {
        lock (_lock)
        {
            _securityProfiles[profile.PluginId] = profile;
            _resourceUsage[profile.PluginId] = new ResourceUsage();

            _logger.LogInformation(
                "Registered security profile for plugin {PluginId}: TrustLevel={TrustLevel}, Sandboxed={Sandboxed}",
                profile.PluginId, profile.TrustLevel, profile.IsSandboxed);
        }
    }

    /// <summary>
    /// Check if a plugin has a specific permission
    /// </summary>
    public PermissionCheckResult CheckPermission(string pluginId, PluginPermission permission)
    {
        lock (_lock)
        {
            if (!_securityProfiles.TryGetValue(pluginId, out var profile))
            {
                _logger.LogWarning("No security profile found for plugin: {PluginId}", pluginId);
                return new PermissionCheckResult
                {
                    IsAllowed = false,
                    DenialReason = "No security profile registered",
                    RequiredPermission = permission,
                    GrantedPermissions = PluginPermission.None
                };
            }

            var isAllowed = profile.GrantedPermissions.HasFlag(permission);

            if (!isAllowed)
            {
                _logger.LogWarning(
                    "Permission denied for plugin {PluginId}: Required={Required}, Granted={Granted}",
                    pluginId, permission, profile.GrantedPermissions);
            }

            return new PermissionCheckResult
            {
                IsAllowed = isAllowed,
                DenialReason = isAllowed ? null : $"Plugin does not have {permission} permission",
                RequiredPermission = permission,
                GrantedPermissions = profile.GrantedPermissions
            };
        }
    }

    /// <summary>
    /// Grant permission to a plugin
    /// </summary>
    public void GrantPermission(string pluginId, PluginPermission permission)
    {
        lock (_lock)
        {
            if (_securityProfiles.TryGetValue(pluginId, out var profile))
            {
                profile.GrantedPermissions |= permission;
                _logger.LogInformation(
                    "Granted permission {Permission} to plugin {PluginId}",
                    permission, pluginId);
            }
        }
    }

    /// <summary>
    /// Revoke permission from a plugin
    /// </summary>
    public void RevokePermission(string pluginId, PluginPermission permission)
    {
        lock (_lock)
        {
            if (_securityProfiles.TryGetValue(pluginId, out var profile))
            {
                profile.GrantedPermissions &= ~permission;
                _logger.LogInformation(
                    "Revoked permission {Permission} from plugin {PluginId}",
                    permission, pluginId);
            }
        }
    }

    /// <summary>
    /// Track resource usage for a plugin
    /// </summary>
    public void RecordResourceUsage(string pluginId, Action<ResourceUsage> updateUsage)
    {
        lock (_lock)
        {
            if (_resourceUsage.TryGetValue(pluginId, out var usage))
            {
                updateUsage(usage);
                usage.LastUpdated = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Check if plugin is within resource limits
    /// </summary>
    public ResourceLimitCheckResult CheckResourceLimits(string pluginId)
    {
        lock (_lock)
        {
            if (!_securityProfiles.TryGetValue(pluginId, out var profile) ||
                !_resourceUsage.TryGetValue(pluginId, out var usage))
            {
                return new ResourceLimitCheckResult { IsWithinLimits = true };
            }

            var limits = profile.ResourceLimits;
            var violations = new List<string>();

            if (usage.MemoryUsage > limits.MaxMemoryBytes)
            {
                violations.Add($"Memory: {usage.MemoryUsage / 1024.0 / 1024.0:F2}MB > {limits.MaxMemoryBytes / 1024.0 / 1024.0:F2}MB");
            }

            if (usage.ThreadCount > limits.MaxThreads)
            {
                violations.Add($"Threads: {usage.ThreadCount} > {limits.MaxThreads}");
            }

            if (usage.FileHandles > limits.MaxFileHandles)
            {
                violations.Add($"File handles: {usage.FileHandles} > {limits.MaxFileHandles}");
            }

            if (usage.NetworkConnections > limits.MaxNetworkConnections)
            {
                violations.Add($"Network connections: {usage.NetworkConnections} > {limits.MaxNetworkConnections}");
            }

            if (violations.Any())
            {
                _logger.LogWarning(
                    "Plugin {PluginId} exceeded resource limits: {Violations}",
                    pluginId, string.Join(", ", violations));
            }

            return new ResourceLimitCheckResult
            {
                IsWithinLimits = !violations.Any(),
                Violations = violations,
                CurrentUsage = usage,
                Limits = limits
            };
        }
    }

    /// <summary>
    /// Get security profile for a plugin
    /// </summary>
    public PluginSecurityProfile? GetProfile(string pluginId)
    {
        lock (_lock)
        {
            return _securityProfiles.TryGetValue(pluginId, out var profile) ? profile : null;
        }
    }

    /// <summary>
    /// Get resource usage for a plugin
    /// </summary>
    public ResourceUsage? GetResourceUsage(string pluginId)
    {
        lock (_lock)
        {
            return _resourceUsage.TryGetValue(pluginId, out var usage) ? usage : null;
        }
    }

    /// <summary>
    /// Get all security profiles
    /// </summary>
    public IReadOnlyDictionary<string, PluginSecurityProfile> GetAllProfiles()
    {
        lock (_lock)
        {
            return new Dictionary<string, PluginSecurityProfile>(_securityProfiles);
        }
    }
}

/// <summary>
/// Tracks actual resource usage by a plugin
/// </summary>
public class ResourceUsage
{
    public long MemoryUsage { get; set; }
    public int ThreadCount { get; set; }
    public long DiskUsage { get; set; }
    public int FileHandles { get; set; }
    public int NetworkConnections { get; set; }
    public TimeSpan TotalExecutionTime { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result of resource limit check
/// </summary>
public class ResourceLimitCheckResult
{
    public bool IsWithinLimits { get; init; }
    public List<string> Violations { get; init; } = new();
    public ResourceUsage? CurrentUsage { get; init; }
    public ResourceLimits? Limits { get; init; }
}
