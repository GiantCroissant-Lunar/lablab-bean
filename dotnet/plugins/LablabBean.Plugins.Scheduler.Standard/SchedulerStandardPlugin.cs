using LablabBean.Contracts.Scheduler.Services;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Scheduler.Standard.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Scheduler.Standard;

public class SchedulerStandardPlugin : IPlugin
{
    private SchedulerService? _schedulerService;

    public string Id => "lablab-bean.scheduler-standard";
    public string Name => "Scheduler Standard";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _schedulerService = new SchedulerService(context.Logger);

        context.Registry.Register<IService>(
            _schedulerService,
            new ServiceMetadata
            {
                Priority = 100,
                Name = "SchedulerService",
                Version = "1.0.0"
            }
        );

        context.Logger.LogInformation("Scheduler Standard service registered");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _schedulerService?.Dispose();
        return Task.CompletedTask;
    }
}
