using LablabBean.Contracts.Diagnostic;
using LablabBean.Contracts.Diagnostic.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Diagnostic.OpenTelemetry.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Diagnostic.OpenTelemetry;

public class OpenTelemetryPlugin : IPlugin
{
    private OpenTelemetryDiagnosticProvider? _provider;
    private ILogger? _logger;

    public string Id => "lablab-bean.diagnostic-opentelemetry";
    public string Name => "Diagnostic OpenTelemetry";
    public string Version => "1.0.0";

    public async Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _logger.LogInformation("Initializing OpenTelemetry diagnostic plugin");

        // Register the diagnostic provider
        _provider = new OpenTelemetryDiagnosticProvider(_logger);

        // Register provider into host registry
        context.Registry.Register<IDiagnosticProvider>(_provider, new ServiceMetadata
        {
            Priority = 200,
            Name = Name,
            Version = Version
        });

        await _provider.InitializeAsync(ct);

        _logger.LogInformation("OpenTelemetry diagnostic plugin initialized successfully");
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_provider != null)
        {
            await _provider.StartAsync(ct);
            _logger?.LogInformation("OpenTelemetry diagnostic plugin started");
        }
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (_provider != null)
        {
            await _provider.StopAsync(ct);
            _logger?.LogInformation("OpenTelemetry diagnostic plugin stopped");
        }
    }
}
