using LablabBean.AI.Agents.Configuration;
using LablabBean.Contracts.AI.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;

namespace LablabBean.AI.Agents.Services;

/// <summary>
/// Base implementation of IMemoryService using Kernel Memory
/// </summary>
public class MemoryService : IMemoryService
{
    private readonly ILogger<MemoryService> _logger;
    private readonly IKernelMemory _memory;
    private readonly KernelMemoryOptions _options;

    public MemoryService(
        ILogger<MemoryService> logger,
        IKernelMemory memory,
        IOptions<KernelMemoryOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public virtual async Task<string> StoreMemoryAsync(MemoryEntry memory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(memory);

        try
        {
            _logger.LogDebug(
                "Storing memory {MemoryId} for entity {EntityId} with type {MemoryType} and importance {Importance}",
                memory.Id, memory.EntityId, memory.MemoryType, memory.Importance);

            // Build tag collection for filtering and retrieval (T023)
            var tags = BuildTagCollection(memory);

            // Import memory as text document with embeddings
            var documentId = await _memory.ImportTextAsync(
                text: memory.Content,
                documentId: memory.Id,
                tags: tags,
                index: _options.Storage.CollectionName ?? "memories",
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Successfully stored memory {MemoryId} for entity {EntityId}",
                memory.Id, memory.EntityId);

            return documentId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to store memory {MemoryId} for entity {EntityId}",
                memory.Id, memory.EntityId);
            throw;
        }
    }

    public virtual async Task<IReadOnlyList<MemoryResult>> RetrieveRelevantMemoriesAsync(
        string queryText,
        MemoryRetrievalOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(queryText);

        try
        {
            _logger.LogDebug(
                "Retrieving memories for query: {Query} with options: Entity={EntityId}, Type={MemoryType}, Limit={Limit}, MinRelevance={MinRelevance}",
                queryText, options.EntityId, options.MemoryType, options.Limit, options.MinRelevanceScore);

            // Build filter from options (T023)
            var filter = BuildMemoryFilter(options);

            // Perform semantic search
            var searchResult = await _memory.SearchAsync(
                query: queryText,
                index: _options.Storage.CollectionName ?? "memories",
                filter: filter,
                minRelevance: options.MinRelevanceScore,
                limit: options.Limit,
                cancellationToken: cancellationToken);

            // Convert results to MemoryResult objects and filter by importance/time
            var results = new List<MemoryResult>();
            foreach (var citation in searchResult.Results)
            {
                var partition = citation.Partitions.FirstOrDefault();
                if (partition == null)
                    continue;

                // Parse memory entry from partition
                var memoryEntry = ParseMemoryEntryFromPartition(partition, citation.DocumentId);
                if (memoryEntry == null)
                    continue;

                // Apply importance filter
                if (memoryEntry.Importance < options.MinImportance)
                    continue;

                // Apply time range filter if specified
                if (options.FromTimestamp.HasValue && memoryEntry.Timestamp < options.FromTimestamp.Value)
                    continue;

                if (options.ToTimestamp.HasValue && memoryEntry.Timestamp > options.ToTimestamp.Value)
                    continue;

                var memoryResult = new MemoryResult
                {
                    Memory = memoryEntry,
                    RelevanceScore = partition.Relevance,
                    Source = _options.Storage.CollectionName ?? "memories"
                };

                results.Add(memoryResult);
            }

            _logger.LogInformation(
                "Retrieved {Count} relevant memories for query '{Query}' (filtered from {TotalResults} results)",
                results.Count, queryText, searchResult.Results.Count);

            // Log relevance scores for analysis (T029)
            if (results.Any())
            {
                _logger.LogDebug(
                    "Top result relevance: {TopRelevance:F3}, Average relevance: {AvgRelevance:F3}",
                    results.First().RelevanceScore,
                    results.Average(r => r.RelevanceScore));
            }

            return results.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to retrieve memories for query: {Query}",
                queryText);
            throw;
        }
    }

    public virtual async Task<MemoryEntry?> GetMemoryByIdAsync(string memoryId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memoryId);

        _logger.LogDebug("Retrieving memory by ID: {MemoryId}", memoryId);

        // Kernel Memory doesn't have direct ID retrieval - need to search
        // This is a limitation of the current implementation
        _logger.LogWarning("GetMemoryByIdAsync not fully implemented - requires document metadata store");
        return await Task.FromResult<MemoryEntry?>(null);
    }

    public virtual async Task UpdateMemoryImportanceAsync(string memoryId, double importance, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memoryId);

        _logger.LogDebug("Updating memory importance: {MemoryId}, Importance: {Importance}", memoryId, importance);

        // Would require re-importing the document with updated tags
        _logger.LogWarning("UpdateMemoryImportanceAsync not fully implemented");
        await Task.CompletedTask;
    }

    public virtual async Task DeleteMemoryAsync(string memoryId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memoryId);

        try
        {
            _logger.LogInformation("Deleting memory: {MemoryId}", memoryId);

            await _memory.DeleteDocumentAsync(
                documentId: memoryId,
                index: _options.Storage.CollectionName ?? "memories",
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully deleted memory: {MemoryId}", memoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete memory: {MemoryId}", memoryId);
            throw;
        }
    }

    public virtual async Task<int> CleanOldMemoriesAsync(string entityId, TimeSpan olderThan, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);

        _logger.LogInformation("Cleaning old memories for entity {EntityId} older than {Age}", entityId, olderThan);

        // This would require listing all memories and filtering by timestamp
        // Not efficiently supported by Kernel Memory's current API
        _logger.LogWarning("CleanOldMemoriesAsync not fully implemented");
        return await Task.FromResult(0);
    }

    public virtual async Task<bool> IsHealthyAsync()
    {
        _logger.LogDebug("Checking memory service health");

        try
        {
            // Try a simple search to verify the memory service is working
            await _memory.SearchAsync(
                query: "health_check",
                index: _options.Storage.CollectionName ?? "memories",
                limit: 1);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Memory service health check failed");
            return false;
        }
    }

    #region Helper Methods

    /// <summary>
    /// Builds tag collection for memory storage (T023)
    /// Tags enable filtering during semantic search
    /// </summary>
    private static TagCollection BuildTagCollection(MemoryEntry memory)
    {
        var tags = new TagCollection
        {
            // Core filtering tags
            { "entity", memory.EntityId },
            { "type", memory.MemoryType },
            { "importance", memory.Importance.ToString("F2") },
            { "timestamp", memory.Timestamp.ToString("O") } // ISO 8601 format
        };

        // Add user-provided tags
        foreach (var tag in memory.Tags)
        {
            tags.Add($"custom_{tag.Key}", tag.Value);
        }

        return tags;
    }

    /// <summary>
    /// Builds memory filter from retrieval options (T023)
    /// </summary>
    private static MemoryFilter? BuildMemoryFilter(MemoryRetrievalOptions options)
    {
        var filter = new MemoryFilter();
        var hasFilters = false;

        // Entity filter
        if (!string.IsNullOrWhiteSpace(options.EntityId))
        {
            filter.Add("entity", options.EntityId);
            hasFilters = true;
        }

        // Memory type filter
        if (!string.IsNullOrWhiteSpace(options.MemoryType))
        {
            // Support comma-separated types
            var types = options.MemoryType.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // For now, use only the first type. Multi-type OR filtering will be handled client-side
            filter.Add("type", types[0]);
            hasFilters = true;
        }

        // Custom tag filters
        foreach (var tag in options.Tags)
        {
            filter.Add($"custom_{tag.Key}", tag.Value);
            hasFilters = true;
        }

        return hasFilters ? filter : null;
    }

    /// <summary>
    /// Parses MemoryEntry from Kernel Memory partition
    /// </summary>
    private MemoryEntry? ParseMemoryEntryFromPartition(Citation.Partition partition, string documentId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(partition.Text))
                return null;

            // Extract tags from partition - partition.Tags is Dictionary<string, List<string>>
            var entityId = GetTagValue(partition.Tags, "entity") ?? "unknown";
            var memoryType = GetTagValue(partition.Tags, "type") ?? "unknown";

            var importanceStr = GetTagValue(partition.Tags, "importance");
            var importance = double.TryParse(importanceStr, out var imp) ? imp : 0.5;

            var timestampStr = GetTagValue(partition.Tags, "timestamp");
            var timestamp = DateTimeOffset.TryParse(timestampStr, out var ts) ? ts : DateTimeOffset.UtcNow;

            // Extract custom tags
            var customTags = partition.Tags
                .Where(t => t.Key.StartsWith("custom_"))
                .ToDictionary(
                    t => t.Key.Substring("custom_".Length),
                    t => t.Value.FirstOrDefault() ?? string.Empty);

            var memoryEntry = new MemoryEntry
            {
                Id = documentId,
                Content = partition.Text,
                EntityId = entityId,
                MemoryType = memoryType,
                Importance = importance,
                Timestamp = timestamp,
                Tags = customTags,
                Metadata = new Dictionary<string, object>
                {
                    { "relevance", partition.Relevance }
                }
            };

            return memoryEntry;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse memory entry from partition for document: {DocumentId}", documentId);
            return null;
        }
    }

    /// <summary>
    /// Helper to get tag value from partition tags
    /// </summary>
    private static string? GetTagValue(TagCollection? tags, string tagKey)
    {
        if (tags != null && tags.ContainsKey(tagKey))
        {
            var values = tags[tagKey];
            if (values != null && values.Count > 0)
            {
                return values[0];
            }
        }
        return null;
    }

    #endregion
}
