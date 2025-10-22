using LablabBean.Plugins.Contracts;
using LablabBean.Reporting.Contracts.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Reporting.Csv;

/// <summary>
/// Plugin that provides CSV report rendering capabilities.
/// </summary>
public class CsvReportingPlugin : IPlugin
{
    private ILogger? _logger;

    public string Id => "lablab-bean.reporting.csv";
    public string Name => "CSV Reporting Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        
        // Register the CSV renderer with the registry
        var loggerFactory = context.Host.Services.GetRequiredService<ILoggerFactory>();
        var renderer = new CsvReportRenderer(loggerFactory.CreateLogger<CsvReportRenderer>());
        
        context.Registry.Register<IReportRenderer>(renderer);
        
        _logger.LogInformation("CSV reporting plugin initialized - registered IReportRenderer");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("CSV reporting plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("CSV reporting plugin stopped");
        return Task.CompletedTask;
    }
}
