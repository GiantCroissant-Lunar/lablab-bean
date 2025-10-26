using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LablabBean.Plugins.Contracts;

public sealed class VectorSearchResult
{
    public required string Id { get; init; }
    public required float Score { get; init; }
    public required IReadOnlyDictionary<string, string> Payload { get; init; }
}

public interface IVectorStore : IDisposable
{
    Task EnsureCollectionAsync(string collection, CancellationToken ct = default);

    Task UpsertAsync(
        string collection,
        string id,
        float[] vector,
        IDictionary<string, string> payload,
        CancellationToken ct = default);

    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string collection,
        float[] vector,
        int topK,
        CancellationToken ct = default);

    Task DeleteAsync(
        string collection,
        string id,
        CancellationToken ct = default);
}
