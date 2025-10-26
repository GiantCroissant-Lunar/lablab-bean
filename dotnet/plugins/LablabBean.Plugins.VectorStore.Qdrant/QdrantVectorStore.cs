using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Plugins.Contracts;

namespace LablabBean.Plugins.VectorStore.Qdrant;

internal sealed class QdrantVectorStore : IVectorStore
{
    private readonly HttpClient _http;
    private readonly string _collection;
    private readonly int _dimension;

    public QdrantVectorStore(Uri baseUri, string? apiKey, string collection, int dimension)
    {
        _http = new HttpClient { BaseAddress = baseUri };
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _http.DefaultRequestHeaders.Add("api-key", apiKey);
        }
        _collection = collection;
        _dimension = dimension;
    }

    public async Task EnsureCollectionAsync(string collection, CancellationToken ct = default)
    {
        var payload = new
        {
            vectors = new { size = _dimension, distance = "Cosine" }
        };
        using var resp = await _http.PutAsJsonAsync($"/collections/{collection}", payload, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }

    public async Task UpsertAsync(string collection, string id, float[] vector, IDictionary<string, string> payload, CancellationToken ct = default)
    {
        await EnsureCollectionAsync(collection, ct).ConfigureAwait(false);
        var body = new
        {
            points = new[]
            {
                new { id, vector, payload }
            }
        };
        using var resp = await _http.PostAsJsonAsync($"/collections/{collection}/points", body, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(string collection, float[] vector, int topK, CancellationToken ct = default)
    {
        var body = new
        {
            vector,
            limit = topK,
            with_payload = true
        };
        using var resp = await _http.PostAsJsonAsync($"/collections/{collection}/points/search", body, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);
        var root = doc.RootElement;
        if (!root.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<VectorSearchResult>();
        }
        var list = new List<VectorSearchResult>();
        foreach (var item in result.EnumerateArray())
        {
            string idStr = item.TryGetProperty("id", out var idEl)
                ? idEl.ValueKind switch
                {
                    JsonValueKind.String => idEl.GetString()!,
                    JsonValueKind.Number => idEl.GetRawText(),
                    _ => Guid.NewGuid().ToString()
                }
                : Guid.NewGuid().ToString();
            float score = item.TryGetProperty("score", out var scoreEl) && scoreEl.TryGetSingle(out var s) ? s : 0f;
            IReadOnlyDictionary<string, string> payload = new Dictionary<string, string>();
            if (item.TryGetProperty("payload", out var payloadEl) && payloadEl.ValueKind == JsonValueKind.Object)
            {
                var dict = new Dictionary<string, string>();
                foreach (var prop in payloadEl.EnumerateObject())
                {
                    dict[prop.Name] = prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString()! : prop.Value.GetRawText();
                }
                payload = dict;
            }
            list.Add(new VectorSearchResult { Id = idStr, Score = score, Payload = payload });
        }
        return list;
    }

    public async Task DeleteAsync(string collection, string id, CancellationToken ct = default)
    {
        var body = new { points = new[] { id } };
        using var resp = await _http.PostAsJsonAsync($"/collections/{collection}/points/delete", body, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        _http.Dispose();
    }
}
