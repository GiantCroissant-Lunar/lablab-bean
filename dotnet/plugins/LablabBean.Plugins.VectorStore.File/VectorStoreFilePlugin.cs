using System;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.VectorStore.File;

public sealed class VectorStoreFilePlugin : IPlugin
{
    private FileVectorStore? _store;

    public string Id => "lablab-bean.vectorstore.file";
    public string Name => "Vector Store (File)";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        var root = context.Configuration["VectorStore:File:Root"] ?? "data/vectorstore";
        var preferred = context.Configuration["VectorStore:Preferred"];
        var priority = string.Equals(preferred, "file", StringComparison.OrdinalIgnoreCase) ? 500 : 200;
        _store = new FileVectorStore(root);

        context.Registry.Register<IVectorStore>(_store, new ServiceMetadata
        {
            Priority = priority,
            Name = "FileVectorStore",
            Version = Version
        });

        context.Logger.LogInformation("Registered IVectorStore (File) at root {Root} with priority {Priority}", root, priority);
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;

    public Task StopAsync(CancellationToken ct = default)
    {
        _store?.Dispose();
        _store = null;
        return Task.CompletedTask;
    }
}
