using LablabBean.Contracts.Serialization.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Serialization.Json.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Serialization.Json;

public class SerializationJsonPlugin : IPlugin
{
    private SerializationService? _serializationService;

    public string Id => "lablab-bean.serialization-json";
    public string Name => "Serialization JSON";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _serializationService = new SerializationService(context.Logger);

        context.Registry.Register<IService>(
            _serializationService,
            new ServiceMetadata
            {
                Priority = 100,
                Name = "SerializationService",
                Version = "1.0.0"
            }
        );

        context.Logger.LogInformation("Serialization JSON service registered");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
