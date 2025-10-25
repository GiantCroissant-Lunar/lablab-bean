using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Contracts.Diagnostic;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Diagnostic.OpenTelemetry.Services;

/// <summary>
/// OpenTelemetry diagnostic provider for distributed tracing and metrics.
/// </summary>
public class OpenTelemetryDiagnosticProvider : IDiagnosticProvider
{
    private readonly ILogger _logger;
    private bool _isInitialized;
    private bool _isEnabled = true;
    private object? _configuration;
    private bool _isHealthy = true;
    private DateTime _startTime;
    private readonly ActivitySource _activitySource;
    private readonly List<Activity> _activeActivities = new();

    public string Name => "OpenTelemetry";
    public ProviderType Type => ProviderType.OpenTelemetry;
    public bool IsEnabled { get => _isEnabled; set => _isEnabled = value; }
    public bool IsHealthy => _isHealthy;
    public object? Configuration { get => _configuration; set => _configuration = value; }

    public OpenTelemetryDiagnosticProvider(ILogger logger)
    {
        _logger = logger;
        _activitySource = new ActivitySource("LablabBean.Diagnostics", "1.0.0");
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            _logger.LogWarning("OpenTelemetry provider already initialized");
            return Task.CompletedTask;
        }

        try
        {
            _logger.LogInformation("Initializing OpenTelemetry diagnostic provider");

            // In a real implementation, you would configure OpenTelemetry here
            // Example:
            // var builder = Sdk.CreateTracerProviderBuilder()
            //     .AddSource(_activitySource.Name)
            //     .AddConsoleExporter()
            //     .Build();

            _startTime = DateTime.UtcNow;
            _isInitialized = true;
            _isHealthy = true;

            _logger.LogInformation("OpenTelemetry diagnostic provider initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize OpenTelemetry diagnostic provider");
            _isHealthy = false;
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("Provider must be initialized before starting");
        }

        _logger.LogInformation("Starting OpenTelemetry diagnostic provider monitoring");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping OpenTelemetry diagnostic provider monitoring");

        // Stop all active activities
        foreach (var activity in _activeActivities)
        {
            activity?.Stop();
        }
        _activeActivities.Clear();

        return Task.CompletedTask;
    }

    public Task<DiagnosticData> CollectDataAsync(CancellationToken cancellationToken = default)
    {
        if (!_isEnabled)
        {
            throw new InvalidOperationException("Provider is not enabled");
        }

        var data = new DiagnosticData
        {
            ProviderName = Name,
            ProviderType = Type,
            Timestamp = DateTime.UtcNow,
            Health = _isHealthy ? SystemHealth.Healthy : SystemHealth.Degraded,
            CustomMetrics = new Dictionary<string, object>
            {
                { "UptimeSeconds", (DateTime.UtcNow - _startTime).TotalSeconds },
                { "ActiveSpans", _activeActivities.Count },
                { "TracesExported", 0 }, // Placeholder
                { "MetricsCollected", 0 } // Placeholder
            },
            Metadata = new Dictionary<string, string>
            {
                { "Provider", "OpenTelemetry" },
                { "Version", "1.0.0" },
                { "ActivitySourceName", _activitySource.Name }
            }
        };

        return Task.FromResult(data);
    }

    public Task<ProviderHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var duration = TimeSpan.FromMilliseconds(5); // Simulated
        var result = new ProviderHealthResult
        {
            ProviderName = Name,
            Health = _isHealthy ? SystemHealth.Healthy : SystemHealth.Degraded,
            Timestamp = DateTime.UtcNow,
            Duration = duration,
            IsSuccessful = _isHealthy,
            Details = new List<HealthCheckDetail>
            {
                new HealthCheckDetail { Name = "Initialized", IsSuccessful = _isInitialized, Message = _isInitialized ? "Provider initialized" : "Not initialized" },
                new HealthCheckDetail { Name = "Enabled", IsSuccessful = _isEnabled, Message = _isEnabled ? "Enabled" : "Disabled" },
                new HealthCheckDetail { Name = "UptimeSeconds", IsSuccessful = true, Message = ((DateTime.UtcNow - _startTime).TotalSeconds).ToString("F2") },
                new HealthCheckDetail { Name = "ActiveSpans", IsSuccessful = true, Message = _activeActivities.Count.ToString() }
            }
        };

        return Task.FromResult(result);
    }

    public Task LogEventAsync(DiagnosticEvent diagnosticEvent, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled || !_isInitialized)
        {
            return Task.CompletedTask;
        }

        try
        {
            // Create an activity for the event
            using var activity = _activitySource.StartActivity(diagnosticEvent.Category, ActivityKind.Internal);

            if (activity != null)
            {
                activity.SetTag("level", diagnosticEvent.Level.ToString());
                activity.SetTag("message", diagnosticEvent.Message);
                activity.SetTag("source", diagnosticEvent.Source);

                // Add custom tags
                foreach (var tag in diagnosticEvent.Tags)
                {
                    activity.SetTag(tag.Key, tag.Value);
                }

                // Add exception information if present
                if (diagnosticEvent.Exception != null)
                {
                    activity.SetTag("exception.type", diagnosticEvent.Exception.GetType().Name);
                    activity.SetTag("exception.message", diagnosticEvent.Exception.Message);
                    activity.SetStatus(ActivityStatusCode.Error, diagnosticEvent.Exception.Message);
                }
            }

            _logger.LogDebug("Logged event to OpenTelemetry: {Category} - {Message}",
                diagnosticEvent.Category, diagnosticEvent.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log event to OpenTelemetry");
            _isHealthy = false;
        }

        return Task.CompletedTask;
    }

    public Task<string> ExportDataAsync(DiagnosticExportFormat format, CancellationToken cancellationToken = default)
    {
        var exportData = format switch
        {
            DiagnosticExportFormat.Json => ExportAsJson(),
            DiagnosticExportFormat.Xml => ExportAsXml(),
            DiagnosticExportFormat.Csv => ExportAsCsv(),
            _ => throw new NotSupportedException($"Export format {format} is not supported")
        };

        return Task.FromResult(exportData);
    }

    private string ExportAsJson()
    {
        return $@"{{
  ""provider"": ""{Name}"",
  ""type"": ""{Type}"",
  ""healthy"": {_isHealthy.ToString().ToLower()},
  ""initialized"": {_isInitialized.ToString().ToLower()},
  ""uptime_seconds"": {(DateTime.UtcNow - _startTime).TotalSeconds:F2},
  ""active_spans"": {_activeActivities.Count},
  ""activity_source"": ""{_activitySource.Name}""
}}";
    }

    private string ExportAsXml()
    {
        return $@"<OpenTelemetryProvider>
  <Name>{Name}</Name>
  <Type>{Type}</Type>
  <Healthy>{_isHealthy}</Healthy>
  <Initialized>{_isInitialized}</Initialized>
  <UptimeSeconds>{(DateTime.UtcNow - _startTime).TotalSeconds:F2}</UptimeSeconds>
  <ActiveSpans>{_activeActivities.Count}</ActiveSpans>
  <ActivitySource>{_activitySource.Name}</ActivitySource>
</OpenTelemetryProvider>";
    }

    private string ExportAsCsv()
    {
        return $"Provider,Type,Healthy,Initialized,UptimeSeconds,ActiveSpans\n{Name},{Type},{_isHealthy},{_isInitialized},{(DateTime.UtcNow - _startTime).TotalSeconds:F2},{_activeActivities.Count}";
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing OpenTelemetry diagnostic provider");

        // Stop all active activities
        foreach (var activity in _activeActivities)
        {
            activity?.Stop();
        }
        _activeActivities.Clear();

        _activitySource?.Dispose();
    }
}
