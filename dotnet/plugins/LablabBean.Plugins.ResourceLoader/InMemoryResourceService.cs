using LablabBean.Contracts.Resource;
using LablabBean.Contracts.Resource.Services;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LablabBean.Plugins.ResourceLoader;

/// <summary>
/// Simple in-memory resource loader with simulated async loading.
/// </summary>
public class InMemoryResourceService : IService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly Dictionary<string, object> _cache = new();
    private readonly Dictionary<string, ResourceMetadata> _metadata = new();

    public InMemoryResourceService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T> LoadAsync<T>(string resourceId, IProgress<LoadProgress>? progress = null, CancellationToken ct = default) where T : class
    {
        if (string.IsNullOrEmpty(resourceId))
            throw new ArgumentException("Resource ID cannot be null or empty", nameof(resourceId));

        // Check cache first
        if (_cache.TryGetValue(resourceId, out var cached))
        {
            _logger.LogDebug("Resource {ResourceId} loaded from cache", resourceId);
            return (T)cached;
        }

        var sw = Stopwatch.StartNew();

        // Publish start event
        await _eventBus.PublishAsync(new ResourceLoadStartedEvent(resourceId));
        _logger.LogInformation("Loading resource: {ResourceId}", resourceId);

        try
        {
            // Simulate loading with progress
            var totalBytes = 1024L;
            for (int i = 0; i <= 10; i++)
            {
                ct.ThrowIfCancellationRequested();

                var bytesLoaded = (long)(totalBytes * i / 10.0);
                var percentComplete = i * 10.0f;

                progress?.Report(new LoadProgress(resourceId, bytesLoaded, totalBytes, percentComplete));

                await Task.Delay(10, ct); // Simulate I/O
            }

            // Create dummy resource (in real implementation, load from disk/network)
            var resource = CreateDummyResource<T>(resourceId);

            // Cache it
            _cache[resourceId] = resource;
            _metadata[resourceId] = new ResourceMetadata(resourceId, typeof(T).Name, totalBytes, $"/{resourceId}");

            sw.Stop();

            // Publish completion event
            await _eventBus.PublishAsync(new ResourceLoadCompletedEvent(resourceId, sw.ElapsedMilliseconds));
            _logger.LogInformation("Resource loaded: {ResourceId} in {Ms}ms", resourceId, sw.ElapsedMilliseconds);

            return resource;
        }
        catch (Exception ex)
        {
            sw.Stop();

            // Publish failure event
            await _eventBus.PublishAsync(new ResourceLoadFailedEvent(resourceId, ex));
            _logger.LogError(ex, "Failed to load resource: {ResourceId}", resourceId);
            throw;
        }
    }

    public async Task PreloadAsync(IEnumerable<string> resourceIds, IProgress<LoadProgress>? progress = null, CancellationToken ct = default)
    {
        var ids = resourceIds.ToList();
        _logger.LogInformation("Preloading {Count} resources", ids.Count);

        for (int i = 0; i < ids.Count; i++)
        {
            await LoadAsync<string>(ids[i], progress, ct);

            // Report overall progress
            var overallProgress = (i + 1) * 100.0f / ids.Count;
            progress?.Report(new LoadProgress($"Batch-{i}", i + 1, ids.Count, overallProgress));
        }

        _logger.LogInformation("Preload complete: {Count} resources", ids.Count);
    }

    public void Unload(string resourceId)
    {
        if (_cache.Remove(resourceId))
        {
            _metadata.Remove(resourceId);
            _eventBus.PublishAsync(new ResourceUnloadedEvent(resourceId)).GetAwaiter().GetResult();
            _logger.LogInformation("Unloaded resource: {ResourceId}", resourceId);
        }
    }

    public bool IsLoaded(string resourceId)
    {
        return _cache.ContainsKey(resourceId);
    }

    public long GetCacheSize()
    {
        return _metadata.Values.Sum(m => m.SizeBytes);
    }

    public void ClearCache()
    {
        var count = _cache.Count;
        _cache.Clear();
        _metadata.Clear();
        _logger.LogInformation("Cache cleared: {Count} resources removed", count);
    }

    private T CreateDummyResource<T>(string resourceId) where T : class
    {
        // In real implementation, load actual resource from disk/network
        // For demo, just create a string representation
        if (typeof(T) == typeof(string))
        {
            return ($"Resource:{resourceId}" as T)!;
        }

        throw new NotSupportedException($"Resource type {typeof(T).Name} not supported in this demo");
    }
}
