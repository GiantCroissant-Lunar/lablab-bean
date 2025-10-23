using LablabBean.Contracts.Diagnostic.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Diagnostic.Firebase.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Diagnostic.Firebase;

public class FirebasePlugin : IPlugin
{
    private FirebaseDiagnosticProvider? _provider;
    private ILogger? _logger;

    public string Id => "lablab-bean.diagnostic-firebase";
    public string Name => "Diagnostic Firebase";
    public string Version => "1.0.0";

    public async Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _logger.LogInformation("Initializing Firebase diagnostic plugin");

        // Register the diagnostic provider
        _provider = new FirebaseDiagnosticProvider(_logger);

        // Register with DI if service collection is available
        if (context.Services is IServiceCollection services)
        {
            services.AddSingleton(_provider);
        }

        await _provider.InitializeAsync(ct);

        _logger.LogInformation("Firebase diagnostic plugin initialized successfully");
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_provider != null)
        {
            await _provider.StartAsync(ct);
            _logger?.LogInformation("Firebase diagnostic plugin started");
        }
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (_provider != null)
        {
            await _provider.StopAsync(ct);
            _logger?.LogInformation("Firebase diagnostic plugin stopped");
        }
    }
}
