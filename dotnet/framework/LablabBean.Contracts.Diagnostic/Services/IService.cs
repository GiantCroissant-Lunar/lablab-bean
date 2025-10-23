using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LablabBean.Contracts.Diagnostic.Services;

/// <summary>
/// Interface for diagnostic service providing comprehensive system monitoring and observability.
/// </summary>
public interface IService
{
    /// <summary>
    /// Current system health status.
    /// </summary>
    SystemHealth CurrentHealth { get; }

    /// <summary>
    /// Whether diagnostic collection is currently active.
    /// </summary>
    bool IsCollecting { get; }

    /// <summary>
    /// Number of registered providers.
    /// </summary>
    int ProviderCount { get; }

    /// <summary>
    /// Start diagnostic data collection.
    /// </summary>
    /// <param name="interval">Collection interval</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    Task StartCollectionAsync(TimeSpan interval, CancellationToken cancellationToken);

    /// <summary>
    /// Stop diagnostic data collection.
    /// </summary>
    /// <returns>Task representing the stop operation</returns>
    Task StopCollectionAsync();

    /// <summary>
    /// Collect diagnostic data once.
    /// </summary>
    /// <param name="providers">Specific providers to collect from (null for all)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collected diagnostic data</returns>
    Task<DiagnosticData[]> CollectDataAsync(string[]? providers, CancellationToken cancellationToken);

    /// <summary>
    /// Log a diagnostic event.
    /// </summary>
    /// <param name="event">Event to log</param>
    /// <returns>Task representing the log operation</returns>
    Task LogEventAsync(DiagnosticEvent @event);

    /// <summary>
    /// Log a diagnostic event with basic information.
    /// </summary>
    /// <param name="level">Event level</param>
    /// <param name="message">Event message</param>
    /// <param name="category">Event category</param>
    /// <param name="source">Event source</param>
    /// <returns>Task representing the log operation</returns>
    Task LogEventAsync(DiagnosticLevel level, string message, string category, string? source);

    /// <summary>
    /// Log an exception as a diagnostic event.
    /// </summary>
    /// <param name="exception">Exception to log</param>
    /// <param name="level">Event level</param>
    /// <param name="category">Event category</param>
    /// <param name="source">Event source</param>
    /// <returns>Task representing the log operation</returns>
    Task LogExceptionAsync(Exception exception, DiagnosticLevel level, string category, string? source);

    /// <summary>
    /// Get current performance metrics.
    /// </summary>
    /// <returns>Current performance metrics</returns>
    PerformanceMetrics GetCurrentPerformanceMetrics();

    /// <summary>
    /// Get current memory information.
    /// </summary>
    /// <returns>Current memory information</returns>
    MemoryInfo GetCurrentMemoryInfo();

    /// <summary>
    /// Get current system information.
    /// </summary>
    /// <returns>Current system information</returns>
    SystemInfo GetCurrentSystemInfo();

    /// <summary>
    /// Export diagnostic data.
    /// </summary>
    /// <param name="format">Export format</param>
    /// <param name="startTime">Start time for data range</param>
    /// <param name="endTime">End time for data range</param>
    /// <param name="providers">Specific providers to export (null for all)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exported data as string</returns>
    Task<string> ExportDataAsync(DiagnosticExportFormat format, DateTime? startTime, DateTime? endTime, string[]? providers, CancellationToken cancellationToken);

    /// <summary>
    /// Configure diagnostic thresholds for alerts.
    /// </summary>
    /// <param name="thresholds">Performance thresholds</param>
    void ConfigureAlertThresholds(PerformanceThresholds thresholds);

    /// <summary>
    /// Get list of available providers.
    /// </summary>
    /// <returns>Provider information</returns>
    ProviderInfo[] GetAvailableProviders();

    /// <summary>
    /// Enable or disable a specific provider.
    /// </summary>
    /// <param name="providerName">Provider name</param>
    /// <param name="enabled">Whether to enable the provider</param>
    /// <returns>Task representing the configuration operation</returns>
    Task ConfigureProviderAsync(string providerName, bool enabled);

    /// <summary>
    /// Configure a provider with specific settings.
    /// </summary>
    /// <param name="providerName">Provider name</param>
    /// <param name="configuration">Provider configuration</param>
    /// <returns>Task representing the configuration operation</returns>
    Task ConfigureProviderAsync(string providerName, Dictionary<string, object> configuration);

    /// <summary>
    /// Get provider health status.
    /// </summary>
    /// <param name="providerName">Provider name (null for all)</param>
    /// <returns>Provider health information</returns>
    ProviderHealthResult[] GetProviderHealth(string? providerName);

    /// <summary>
    /// Force a health check across all providers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Overall health check result</returns>
    Task<HealthCheckResult> PerformHealthCheckAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Start a diagnostic session with automatic data collection.
    /// </summary>
    /// <param name="sessionName">Session name</param>
    /// <param name="configuration">Session configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session ID</returns>
    Task<string> StartSessionAsync(string sessionName, DiagnosticSessionConfig? configuration, CancellationToken cancellationToken);

    /// <summary>
    /// Stop a diagnostic session.
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Session summary</returns>
    Task<DiagnosticSessionSummary> StopSessionAsync(string sessionId);

    /// <summary>
    /// Get statistics about the diagnostic service.
    /// </summary>
    /// <returns>Service statistics</returns>
    DiagnosticServiceStats GetServiceStatistics();

    /// <summary>
    /// Clear collected diagnostic data.
    /// </summary>
    /// <param name="olderThan">Clear data older than this time (null for all)</param>
    /// <returns>Number of records cleared</returns>
    Task<int> ClearDataAsync(DateTime? olderThan);

    /// <summary>
    /// Subscribe to specific diagnostic events.
    /// </summary>
    /// <param name="filter">Event filter</param>
    void SubscribeToEvents(DiagnosticEventFilter filter);

    /// <summary>
    /// Create a custom diagnostic span for tracking operations.
    /// </summary>
    /// <param name="operationName">Operation name</param>
    /// <param name="tags">Optional tags</param>
    /// <returns>Diagnostic span</returns>
    IDiagnosticSpan CreateSpan(string operationName, Dictionary<string, string>? tags);

    /// <summary>
    /// Add a breadcrumb for debugging purposes.
    /// </summary>
    /// <param name="message">Breadcrumb message</param>
    /// <param name="category">Breadcrumb category</param>
    /// <param name="level">Breadcrumb level</param>
    /// <param name="data">Additional data</param>
    void AddBreadcrumb(string message, string category, DiagnosticLevel level, Dictionary<string, object>? data);

    /// <summary>
    /// Set user context for diagnostic tracking.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="properties">User properties</param>
    void SetUserContext(string userId, Dictionary<string, string>? properties);

    /// <summary>
    /// Set tags that will be applied to all diagnostic events.
    /// </summary>
    /// <param name="tags">Global tags</param>
    void SetGlobalTags(Dictionary<string, string> tags);

    /// <summary>
    /// Remove global tags.
    /// </summary>
    /// <param name="keys">Tag keys to remove (null for all)</param>
    void RemoveGlobalTags(string[]? keys);
}
