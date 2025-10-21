using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Core.Security;

/// <summary>
/// Audit event types
/// </summary>
public enum SecurityAuditEventType
{
    PermissionGranted,
    PermissionDenied,
    PermissionRevoked,
    ResourceLimitExceeded,
    SandboxCreated,
    SandboxTerminated,
    SecurityViolation,
    PluginLoaded,
    PluginUnloaded
}

/// <summary>
/// Security audit event
/// </summary>
public class SecurityAuditEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public SecurityAuditEventType EventType { get; init; }
    public string PluginId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Dictionary<string, object> AdditionalData { get; init; } = new();
    public string Severity { get; init; } = "Info";
}

/// <summary>
/// Security audit log for tracking security-related events
/// </summary>
public class SecurityAuditLog
{
    private readonly List<SecurityAuditEvent> _events = new();
    private readonly ILogger<SecurityAuditLog> _logger;
    private readonly object _lock = new();
    private readonly int _maxEvents;

    public SecurityAuditLog(ILogger<SecurityAuditLog> logger, int maxEvents = 10000)
    {
        _logger = logger;
        _maxEvents = maxEvents;
    }

    /// <summary>
    /// Log a security audit event
    /// </summary>
    public void LogEvent(SecurityAuditEvent auditEvent)
    {
        lock (_lock)
        {
            _events.Add(auditEvent);

            // Trim old events if we exceed max
            if (_events.Count > _maxEvents)
            {
                _events.RemoveRange(0, _events.Count - _maxEvents);
            }

            // Log to standard logger
            var logLevel = auditEvent.Severity switch
            {
                "Critical" => LogLevel.Critical,
                "Error" => LogLevel.Error,
                "Warning" => LogLevel.Warning,
                _ => LogLevel.Information
            };

            _logger.Log(logLevel,
                "[SECURITY AUDIT] {EventType} - Plugin: {PluginId} - {Description}",
                auditEvent.EventType, auditEvent.PluginId, auditEvent.Description);
        }
    }

    /// <summary>
    /// Log permission denied event
    /// </summary>
    public void LogPermissionDenied(string pluginId, PluginPermission permission, string reason)
    {
        LogEvent(new SecurityAuditEvent
        {
            EventType = SecurityAuditEventType.PermissionDenied,
            PluginId = pluginId,
            Description = $"Permission {permission} denied: {reason}",
            Severity = "Warning",
            AdditionalData = new Dictionary<string, object>
            {
                ["permission"] = permission.ToString(),
                ["reason"] = reason
            }
        });
    }

    /// <summary>
    /// Log permission granted event
    /// </summary>
    public void LogPermissionGranted(string pluginId, PluginPermission permission)
    {
        LogEvent(new SecurityAuditEvent
        {
            EventType = SecurityAuditEventType.PermissionGranted,
            PluginId = pluginId,
            Description = $"Permission {permission} granted",
            Severity = "Info",
            AdditionalData = new Dictionary<string, object>
            {
                ["permission"] = permission.ToString()
            }
        });
    }

    /// <summary>
    /// Log resource limit exceeded event
    /// </summary>
    public void LogResourceLimitExceeded(string pluginId, List<string> violations)
    {
        LogEvent(new SecurityAuditEvent
        {
            EventType = SecurityAuditEventType.ResourceLimitExceeded,
            PluginId = pluginId,
            Description = $"Resource limits exceeded: {string.Join(", ", violations)}",
            Severity = "Warning",
            AdditionalData = new Dictionary<string, object>
            {
                ["violations"] = violations
            }
        });
    }

    /// <summary>
    /// Log security violation event
    /// </summary>
    public void LogSecurityViolation(string pluginId, string description, string severity = "Error")
    {
        LogEvent(new SecurityAuditEvent
        {
            EventType = SecurityAuditEventType.SecurityViolation,
            PluginId = pluginId,
            Description = description,
            Severity = severity
        });
    }

    /// <summary>
    /// Get all events
    /// </summary>
    public IReadOnlyList<SecurityAuditEvent> GetAllEvents()
    {
        lock (_lock)
        {
            return _events.ToList();
        }
    }

    /// <summary>
    /// Get events for a specific plugin
    /// </summary>
    public IReadOnlyList<SecurityAuditEvent> GetEventsForPlugin(string pluginId)
    {
        lock (_lock)
        {
            return _events.Where(e => e.PluginId == pluginId).ToList();
        }
    }

    /// <summary>
    /// Get events by type
    /// </summary>
    public IReadOnlyList<SecurityAuditEvent> GetEventsByType(SecurityAuditEventType eventType)
    {
        lock (_lock)
        {
            return _events.Where(e => e.EventType == eventType).ToList();
        }
    }

    /// <summary>
    /// Get events in time range
    /// </summary>
    public IReadOnlyList<SecurityAuditEvent> GetEventsByTimeRange(DateTime start, DateTime end)
    {
        lock (_lock)
        {
            return _events.Where(e => e.Timestamp >= start && e.Timestamp <= end).ToList();
        }
    }

    /// <summary>
    /// Get security violations
    /// </summary>
    public IReadOnlyList<SecurityAuditEvent> GetSecurityViolations()
    {
        return GetEventsByType(SecurityAuditEventType.SecurityViolation);
    }

    /// <summary>
    /// Clear all events
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _events.Clear();
        }
    }

    /// <summary>
    /// Get event count
    /// </summary>
    public int EventCount
    {
        get
        {
            lock (_lock)
            {
                return _events.Count;
            }
        }
    }
}
