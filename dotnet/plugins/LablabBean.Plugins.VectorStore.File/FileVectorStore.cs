using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Plugins.Contracts;
using LablabBean.Framework.Generated.Models.Qdrant;

namespace LablabBean.Plugins.VectorStore.File;

internal sealed class FileVectorStore : IVectorStore
{
    private readonly string _root;
    private readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _locks = new();
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private sealed class Point
    {
        public required string Id { get; init; }
        public required float[] Vector { get; init; }
        public required Dictionary<string, string> Payload { get; init; }
    }

    public FileVectorStore(string root)
    {
        _root = System.IO.Path.GetFullPath(root);
        Directory.CreateDirectory(_root);
    }

    public Task EnsureCollectionAsync(string collection, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Directory.CreateDirectory(GetCollectionPath(collection));
        return Task.CompletedTask;
    }

    public async Task UpsertAsync(string collection, string id, float[] vector, IDictionary<string, string> payload, CancellationToken ct = default)
    {
        await EnsureCollectionAsync(collection, ct).ConfigureAwait(false);
        var path = GetPointsPath(collection);
        var @lock = _locks.GetOrAdd(collection, _ => new ReaderWriterLockSlim());

        @lock.EnterWriteLock();
        try
        {
            var map = await ReadPointsAsync(path, ct).ConfigureAwait(false);
            map[id] = new Point { Id = id, Vector = vector, Payload = new Dictionary<string, string>(payload) };
            await WritePointsAsync(path, map.Values, ct).ConfigureAwait(false);
        }
        finally
        {
            @lock.ExitWriteLock();
        }
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(string collection, float[] vector, int topK, CancellationToken ct = default)
    {
        var path = GetPointsPath(collection);
        var @lock = _locks.GetOrAdd(collection, _ => new ReaderWriterLockSlim());

        @lock.EnterReadLock();
        try
        {
            var list = (await ReadPointsAsync(path, ct).ConfigureAwait(false)).Values.ToList();
            if (list.Count == 0) return Array.Empty<VectorSearchResult>();

            var qNorm = MathF.Sqrt(vector.Sum(v => v * v));
            if (qNorm == 0) return Array.Empty<VectorSearchResult>();

            var scored = list
                .Select(p => new
                {
                    Point = p,
                    Score = Cosine(vector, qNorm, p.Vector)
                })
                .OrderByDescending(x => x.Score)
                .Take(Math.Max(1, topK))
                .Select(x => MapToSearchResult(x.Point, x.Score))
                .ToList();

            return scored;
        }
        finally
        {
            @lock.ExitReadLock();
        }
    }

    private static VectorSearchResult MapToSearchResult(Point point, float score)
    {
        return new VectorSearchResult
        {
            Id = point.Id,
            Score = score,
            Payload = point.Payload
        };
    }

    public async Task DeleteAsync(string collection, string id, CancellationToken ct = default)
    {
        var path = GetPointsPath(collection);
        var @lock = _locks.GetOrAdd(collection, _ => new ReaderWriterLockSlim());

        @lock.EnterWriteLock();
        try
        {
            var map = await ReadPointsAsync(path, ct).ConfigureAwait(false);
            if (map.Remove(id))
            {
                await WritePointsAsync(path, map.Values, ct).ConfigureAwait(false);
            }
        }
        finally
        {
            @lock.ExitWriteLock();
        }
    }

    private string GetCollectionPath(string collection) => System.IO.Path.Combine(_root, Sanitize(collection));
    private string GetPointsPath(string collection) => System.IO.Path.Combine(GetCollectionPath(collection), "points.json");

    private static string Sanitize(string name)
        => string.Concat(name.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' || ch == '.'));

    private async Task<Dictionary<string, Point>> ReadPointsAsync(string path, CancellationToken ct)
    {
        if (!System.IO.File.Exists(path)) return new();
        await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var points = await JsonSerializer.DeserializeAsync<List<Point>>(fs, _json, ct).ConfigureAwait(false);
        return points?.ToDictionary(p => p.Id) ?? new();
    }

    private async Task WritePointsAsync(string path, IEnumerable<Point> points, CancellationToken ct)
    {
        var dir = System.IO.Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(dir);
        await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(fs, points, _json, ct).ConfigureAwait(false);
    }

    private static float Cosine(float[] a, float aNorm, float[] b)
    {
        if (a.Length != b.Length) return -1f;
        float dot = 0f;
        float bNormSq = 0f;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            bNormSq += b[i] * b[i];
        }
        var bNorm = MathF.Sqrt(bNormSq);
        if (bNorm == 0) return -1f;
        return dot / (aNorm * bNorm);
    }

    public void Dispose()
    {
        foreach (var l in _locks.Values) l.Dispose();
        _locks.Clear();
    }
}
