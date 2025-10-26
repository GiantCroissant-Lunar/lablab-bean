using LablabBean.Contracts.AI.Memory;
using LablabBean.Contracts.Resilience.Services;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;

namespace LablabBean.Plugins.RAG.KernelMemory;

public class RagKernelMemoryPlugin : IPlugin
{
    private IRagService? _ragService;

    public string Id => "lablab-bean.rag.kernel-memory";
    public string Name => "RAG: Kernel Memory";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        var sp = context.Host.Services;

        // Resolve dependencies from host
        var km = sp.GetService<IKernelMemory>();
        if (km is null)
        {
            context.Logger.LogWarning("IKernelMemory not available; RAG service will not be registered");
            return Task.CompletedTask;
        }

        var logger = sp.GetService<ILogger<RagService>>()
                     ?? LoggerFactory.Create(b => b.AddConsole()).CreateLogger<RagService>();
        var resilience = sp.GetService<IService>(); // optional

        _ragService = new RagService(km, logger, resilience);

        // Register into plugin registry with a reasonable priority
        context.Registry.Register<IRagService>(_ragService, new ServiceMetadata
        {
            Priority = 300,
            Name = "KernelMemoryRAG",
            Version = Version
        });

        context.Logger.LogInformation("Registered IRagService (Kernel Memory) with priority {Priority}", 300);

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;

    public Task StopAsync(CancellationToken ct = default)
    {
        _ragService = null;
        return Task.CompletedTask;
    }
}
