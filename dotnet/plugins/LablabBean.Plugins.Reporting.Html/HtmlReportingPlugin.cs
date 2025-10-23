using LablabBean.Plugins.Contracts;
using LablabBean.Reporting.Contracts.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Reporting.Html;

/// <summary>
/// Plugin that provides HTML report rendering capabilities.
/// </summary>
public class HtmlReportingPlugin : IPlugin
{
    private ILogger? _logger;

    public string Id => "lablab-bean.reporting.html";
    public string Name => "HTML Reporting Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;

        // Register the HTML renderer with the registry
        var loggerFactory = context.Host.Services.GetRequiredService<ILoggerFactory>();
        var renderer = new HtmlReportRenderer(loggerFactory.CreateLogger<HtmlReportRenderer>());

        context.Registry.Register<IReportRenderer>(renderer);

        _logger.LogInformation("HTML reporting plugin initialized - registered IReportRenderer");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("HTML reporting plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("HTML reporting plugin stopped");
        return Task.CompletedTask;
    }
}
