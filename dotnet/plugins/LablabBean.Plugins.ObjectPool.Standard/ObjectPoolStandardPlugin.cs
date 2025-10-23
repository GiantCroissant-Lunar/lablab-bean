using LablabBean.Contracts.ObjectPool.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.ObjectPool.Standard.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.ObjectPool.Standard;

public class ObjectPoolStandardPlugin : IPlugin
{
    private ObjectPoolService? _objectPoolService;

    public string Id => "lablab-bean.objectpool-standard";
    public string Name => "ObjectPool Standard";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _objectPoolService = new ObjectPoolService(context.Logger);

        context.Registry.Register<IService>(
            _objectPoolService,
            new ServiceMetadata
            {
                Priority = 100,
                Name = "ObjectPoolService",
                Version = "1.0.0"
            }
        );

        context.Logger.LogInformation("ObjectPool Standard service registered");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _objectPoolService?.Dispose();
        return Task.CompletedTask;
    }
}
