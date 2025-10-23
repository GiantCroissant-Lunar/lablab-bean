using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Represents a diagnostic event.
/// </summary>
public class DiagnosticEvent
{
    /// <summary>
    /// Event ID.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Event timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Event severity level.
    /// </summary>
    public DiagnosticLevel Level { get; set; }

    /// <summary>
    /// Event message.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Event category.
    /// </summary>
    public string Category { get; set; } = "General";

    /// <summary>
    /// Source of the event.
    /// </summary>
    public string Source { get; set; } = "";

    /// <summary>
    /// Thread ID where the event occurred.
    /// </summary>
    public int ThreadId { get; set; }

    /// <summary>
    /// Process ID.
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    /// Associated exception (if any).
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Stack trace (if available).
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Event tags.
    /// </summary>
    public Dictionary<string, string> Tags { get; set; } = new();

    /// <summary>
    /// Additional event data.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// Performance context at the time of the event.
    /// </summary>
    public PerformanceMetrics? PerformanceContext { get; set; }

    /// <summary>
    /// Memory context at the time of the event.
    /// </summary>
    public MemoryInfo? MemoryContext { get; set; }

    /// <summary>
    /// User context (if available).
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Session ID.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Device fingerprint.
    /// </summary>
    public string? DeviceFingerprint { get; set; }

    /// <summary>
    /// Create a diagnostic event with basic information.
    /// </summary>
    public static DiagnosticEvent Create(DiagnosticLevel level, string message, string category = "General")
    {
        return new DiagnosticEvent
        {
            Level = level,
            Message = message,
            Category = category,
            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
            ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id
        };
    }

    /// <summary>
    /// Create a diagnostic event from an exception.
    /// </summary>
    public static DiagnosticEvent FromException(Exception exception, DiagnosticLevel level = DiagnosticLevel.Error, string category = "Exception")
    {
        return new DiagnosticEvent
        {
            Level = level,
            Message = exception.Message,
            Category = category,
            Exception = exception,
            StackTrace = exception.StackTrace,
            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
            ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id,
            Data = { ["ExceptionType"] = exception.GetType().Name }
        };
    }

    /// <summary>
    /// Add a tag to the event.
    /// </summary>
    public DiagnosticEvent WithTag(string key, string value)
    {
        Tags[key] = value;
        return this;
    }

    /// <summary>
    /// Add data to the event.
    /// </summary>
    public DiagnosticEvent WithData(string key, object value)
    {
        Data[key] = value;
        return this;
    }

    /// <summary>
    /// Set the source of the event.
    /// </summary>
    public DiagnosticEvent WithSource(string source)
    {
        Source = source;
        return this;
    }

    /// <summary>
    /// Set the user context.
    /// </summary>
    public DiagnosticEvent WithUser(string userId)
    {
        UserId = userId;
        return this;
    }

    /// <summary>
    /// Set the session context.
    /// </summary>
    public DiagnosticEvent WithSession(string sessionId)
    {
        SessionId = sessionId;
        return this;
    }
}
