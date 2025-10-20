namespace LablabBean.Core.Interfaces;

public interface IApplicationService
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
