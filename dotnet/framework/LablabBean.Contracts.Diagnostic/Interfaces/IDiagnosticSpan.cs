using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Interface for diagnostic spans to track operations.
/// </summary>
public interface IDiagnosticSpan : IDisposable
{
    /// <summary>
    /// Span ID.
    /// </summary>
    string SpanId { get; }

    /// <summary>
    /// Parent span ID (if any).
    /// </summary>
    string? ParentSpanId { get; }

    /// <summary>
    /// Operation name.
    /// </summary>
    string OperationName { get; }

    /// <summary>
    /// Span start time.
    /// </summary>
    DateTime StartTime { get; }

    /// <summary>
    /// Span end time (null if still active).
    /// </summary>
    DateTime? EndTime { get; }

    /// <summary>
    /// Span duration (null if still active).
    /// </summary>
    TimeSpan? Duration { get; }

    /// <summary>
    /// Whether the span is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Span status.
    /// </summary>
    SpanStatus Status { get; }

    /// <summary>
    /// Span tags.
    /// </summary>
    IReadOnlyDictionary<string, string> Tags { get; }

    /// <summary>
    /// Span events.
    /// </summary>
    IReadOnlyList<SpanEvent> Events { get; }

    /// <summary>
    /// Set a tag on the span.
    /// </summary>
    /// <param name="key">Tag key</param>
    /// <param name="value">Tag value</param>
    void SetTag(string key, string value);

    /// <summary>
    /// Set multiple tags on the span.
    /// </summary>
    /// <param name="tags">Tags to set</param>
    void SetTags(Dictionary<string, string> tags);

    /// <summary>
    /// Add an event to the span.
    /// </summary>
    /// <param name="name">Event name</param>
    /// <param name="attributes">Event attributes</param>
    void AddEvent(string name, Dictionary<string, object>? attributes = null);

    /// <summary>
    /// Add an exception event to the span.
    /// </summary>
    /// <param name="exception">Exception to add</param>
    void AddException(Exception exception);

    /// <summary>
    /// Set the span status.
    /// </summary>
    /// <param name="status">Span status</param>
    /// <param name="description">Status description</param>
    void SetStatus(SpanStatus status, string? description = null);

    /// <summary>
    /// Finish the span.
    /// </summary>
    void Finish();

    /// <summary>
    /// Create a child span.
    /// </summary>
    /// <param name="operationName">Child operation name</param>
    /// <param name="tags">Child span tags</param>
    /// <returns>Child span</returns>
    IDiagnosticSpan CreateChild(string operationName, Dictionary<string, string>? tags = null);
}

/// <summary>
/// Span status enumeration.
/// </summary>
public enum SpanStatus
{
    /// <summary>
    /// Operation completed successfully.
    /// </summary>
    Ok,

    /// <summary>
    /// Operation was cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Operation failed with an error.
    /// </summary>
    Error,

    /// <summary>
    /// Operation timed out.
    /// </summary>
    Timeout,

    /// <summary>
    /// Operation status is unknown.
    /// </summary>
    Unknown
}

/// <summary>
/// Event within a span.
/// </summary>
public class SpanEvent
{
    /// <summary>
    /// Event name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Event timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Event attributes.
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; } = new();

    /// <summary>
    /// Create a span event.
    /// </summary>
    public static SpanEvent Create(string name, Dictionary<string, object>? attributes = null)
    {
        return new SpanEvent
        {
            Name = name,
            Attributes = attributes ?? new()
        };
    }

    /// <summary>
    /// Create a span event from an exception.
    /// </summary>
    public static SpanEvent FromException(Exception exception)
    {
        return new SpanEvent
        {
            Name = "exception",
            Attributes = new Dictionary<string, object>
            {
                ["exception.type"] = exception.GetType().Name,
                ["exception.message"] = exception.Message,
                ["exception.stacktrace"] = exception.StackTrace ?? ""
            }
        };
    }
}
