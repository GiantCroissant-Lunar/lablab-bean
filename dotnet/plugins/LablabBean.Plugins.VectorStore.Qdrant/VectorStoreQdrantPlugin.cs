using System;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.VectorStore.Qdrant;

public sealed class VectorStoreQdrantPlugin : IPlugin
{
    private QdrantVectorStore? _store;

    public string Id => "lablab-bean.vectorstore.qdrant";
    public string Name => "Vector Store (Qdrant)";
    public string Version => "1.0.0";

    public async Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        var cfg = context.Configuration;
        var preferred = cfg["VectorStore:Preferred"];
        var baseUrl = cfg["VectorStore:Qdrant:BaseUrl"] ?? "http://localhost:6333";
        var apiKey = cfg["VectorStore:Qdrant:ApiKey"];
        var collection = cfg["VectorStore:Qdrant:Collection"] ?? "kb";
        var dimStr = cfg["VectorStore:Qdrant:Dimension"] ?? "1536";
        if (!int.TryParse(dimStr, out var dim)) dim = 1536;

        _store = new QdrantVectorStore(new Uri(baseUrl), apiKey, collection, dim);

        // Only perform network initialization when Qdrant is the preferred provider
        var isPreferred = string.Equals(preferred, "qdrant", StringComparison.OrdinalIgnoreCase);
        if (isPreferred)
        {
            await _store.EnsureCollectionAsync(collection, ct);
        }

        var priority = isPreferred ? 500 : 300;
        context.Registry.Register<IVectorStore>(_store, new ServiceMetadata
        {
            Priority = priority,
            Name = "QdrantVectorStore",
            Version = Version
        });

        context.Logger.LogInformation("Registered IVectorStore (Qdrant) at {BaseUrl} collection {Collection} with priority {Priority} (Preferred: {Preferred})", baseUrl, collection, priority, isPreferred);
    }

    public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;

    public Task StopAsync(CancellationToken ct = default)
    {
        _store?.Dispose();
        _store = null;
        return Task.CompletedTask;
    }
}
