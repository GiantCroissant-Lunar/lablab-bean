using System;
using System.Threading;

using System.Threading.Tasks;
namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Interface for custom diagnostic providers.
/// </summary>
public interface IDiagnosticProvider : IDisposable
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the provider type.
    /// </summary>
    ProviderType Type { get; }

    /// <summary>
    /// Gets whether the provider is enabled.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Gets whether the provider is currently healthy.
    /// </summary>
    bool IsHealthy { get; }

    /// <summary>
    /// Gets the provider configuration.
    /// </summary>
    object? Configuration { get; set; }

    /// <summary>
    /// Initialize the provider.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the initialization</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Start the provider monitoring.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop the provider monitoring.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Collect diagnostic data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collected diagnostic data</returns>
    Task<DiagnosticData> CollectDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform provider-specific health check.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    Task<ProviderHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Log a diagnostic event through this provider.
    /// </summary>
    /// <param name="diagnosticEvent">The diagnostic event to log</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the logging operation</returns>
    Task LogEventAsync(DiagnosticEvent diagnosticEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export data in the provider's native format.
    /// </summary>
    /// <param name="format">Export format</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exported data</returns>
    Task<string> ExportDataAsync(DiagnosticExportFormat format, CancellationToken cancellationToken = default);
}
