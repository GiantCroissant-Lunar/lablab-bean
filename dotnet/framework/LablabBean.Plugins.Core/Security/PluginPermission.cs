namespace LablabBean.Plugins.Core.Security;

/// <summary>
/// Defines permissions that can be granted to plugins
/// </summary>
[Flags]
public enum PluginPermission
{
    None = 0,

    // File System
    FileSystemRead = 1 << 0,
    FileSystemWrite = 1 << 1,
    FileSystemDelete = 1 << 2,

    // Network
    NetworkAccess = 1 << 3,
    NetworkListen = 1 << 4,

    // Registry Access
    RegistryRead = 1 << 5,
    RegistryWrite = 1 << 6,

    // Process Management
    ProcessCreate = 1 << 7,
    ProcessKill = 1 << 8,

    // System Resources
    SystemInformation = 1 << 9,
    EnvironmentVariables = 1 << 10,

    // Database Access
    DatabaseRead = 1 << 11,
    DatabaseWrite = 1 << 12,

    // UI Access
    UIDisplay = 1 << 13,
    UIInput = 1 << 14,

    // Inter-Plugin Communication
    PluginCommunication = 1 << 15,

    // Service Registry
    ServiceRegistryRead = 1 << 16,
    ServiceRegistryWrite = 1 << 17,

    // Reflection
    Reflection = 1 << 18,
    UnsafeCode = 1 << 19,

    // Presets
    ReadOnly = FileSystemRead | RegistryRead | SystemInformation | ServiceRegistryRead,
    Standard = ReadOnly | FileSystemWrite | NetworkAccess | UIDisplay | PluginCommunication,
    Elevated = Standard | FileSystemDelete | NetworkListen | DatabaseWrite | ServiceRegistryWrite,
    Admin = ~0  // All permissions
}

/// <summary>
/// Permission request with justification
/// </summary>
public class PermissionRequest
{
    public PluginPermission Permission { get; init; }
    public string Reason { get; init; } = string.Empty;
    public bool Required { get; init; }
}

/// <summary>
/// Defines the security profile for a plugin
/// </summary>
public class PluginSecurityProfile
{
    public string PluginId { get; init; } = string.Empty;
    public PluginPermission GrantedPermissions { get; set; }
    public List<PermissionRequest> RequestedPermissions { get; init; } = new();
    public ResourceLimits ResourceLimits { get; init; } = new();
    public bool IsSandboxed { get; init; }
    public string TrustLevel { get; init; } = "Untrusted";
}

/// <summary>
/// Resource limits for plugin execution
/// </summary>
public class ResourceLimits
{
    public long MaxMemoryBytes { get; init; } = 100 * 1024 * 1024; // 100 MB
    public int MaxThreads { get; init; } = 10;
    public TimeSpan MaxExecutionTime { get; init; } = TimeSpan.FromMinutes(5);
    public long MaxDiskUsageBytes { get; init; } = 500 * 1024 * 1024; // 500 MB
    public int MaxFileHandles { get; init; } = 100;
    public int MaxNetworkConnections { get; init; } = 10;
}

/// <summary>
/// Result of a permission check
/// </summary>
public class PermissionCheckResult
{
    public bool IsAllowed { get; init; }
    public string? DenialReason { get; init; }
    public PluginPermission RequiredPermission { get; init; }
    public PluginPermission GrantedPermissions { get; init; }
}
