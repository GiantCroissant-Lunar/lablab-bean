using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Plugins.Contracts;
using LablabBean.Framework.Generated.Models.Qdrant;

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
        var request = new QdrantSearchRequest
        {
            Vector = vector.Select(v => (double)v).ToList(),
            Limit = topK,
            WithPayload = true
        };

        using var resp = await _http.PostAsJsonAsync($"/collections/{collection}/points/search", request, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();

        using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var searchResponse = await JsonSerializer.DeserializeAsync<QdrantSearchResponse>(stream, cancellationToken: ct).ConfigureAwait(false);

        if (searchResponse?.Result == null)
        {
            return Array.Empty<VectorSearchResult>();
        }

        return searchResponse.Result.Select(point => new VectorSearchResult
        {
            Id = point.Id.String ?? point.Id.Integer?.ToString() ?? Guid.NewGuid().ToString(),
            Score = (float)point.Score,
            Payload = ConvertPayload(point.Payload)
        }).ToList();
    }

    private static IReadOnlyDictionary<string, string> ConvertPayload(Dictionary<string, object>? payload)
    {
        if (payload == null)
        {
            return new Dictionary<string, string>();
        }

        var result = new Dictionary<string, string>();
        foreach (var kvp in payload)
        {
            if (kvp.Value is JsonElement jsonElement)
            {
                result[kvp.Key] = jsonElement.ValueKind == JsonValueKind.String
                    ? jsonElement.GetString()!
                    : jsonElement.GetRawText();
            }
            else
            {
                result[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
            }
        }
        return result;
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
