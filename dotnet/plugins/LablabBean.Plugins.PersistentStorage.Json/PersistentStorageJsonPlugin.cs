using LablabBean.Contracts.PersistentStorage.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.PersistentStorage.Json.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.PersistentStorage.Json;

public class PersistentStorageJsonPlugin : IPlugin
{
    private PersistentStorageService? _storageService;

    public string Id => "lablab-bean.persistentstorage-json";
    public string Name => "PersistentStorage JSON";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _storageService = new PersistentStorageService(context.Logger);

        context.Registry.Register<IService>(
            _storageService,
            new ServiceMetadata
            {
                Priority = 100,
                Name = "PersistentStorageService",
                Version = "1.0.0"
            }
        );

        context.Logger.LogInformation("PersistentStorage JSON service registered");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _storageService?.Dispose();
        return Task.CompletedTask;
    }
}
