using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;

namespace LablabBean.Contracts.AI.Memory;

/// <summary>
/// Implementation of IMemoryService using Microsoft Kernel Memory
/// </summary>
public class KernelMemoryService : IMemoryService
{
    private readonly IKernelMemory _memory;
    private readonly ILogger<KernelMemoryService> _logger;
    private const string DefaultCollection = "game_memories";

    public KernelMemoryService(
        IKernelMemory memory,
        ILogger<KernelMemoryService> logger)
    {
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> StoreMemoryAsync(MemoryEntry memory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(memory);

        try
        {
            var tags = new TagCollection
            {
                { "entity_id", memory.EntityId },
                { "memory_type", memory.MemoryType },
                { "importance", memory.Importance.ToString("F2") },
                { "timestamp", memory.Timestamp.ToString("O") }
            };

            foreach (var tag in memory.Tags)
            {
                tags.Add(tag.Key, tag.Value);
            }

            var documentId = await _memory.ImportTextAsync(
                text: memory.Content,
                documentId: memory.Id,
                tags: tags,
                index: DefaultCollection,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation(
                "Stored memory {MemoryId} for entity {EntityId} with type {MemoryType}",
                memory.Id, memory.EntityId, memory.MemoryType);

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

    public async Task<IReadOnlyList<MemoryResult>> RetrieveRelevantMemoriesAsync(
        string queryText,
        MemoryRetrievalOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queryText))
            throw new ArgumentException("Query text cannot be empty", nameof(queryText));

        ArgumentNullException.ThrowIfNull(options);

        try
        {
            var filter = new MemoryFilter();

            if (!string.IsNullOrWhiteSpace(options.EntityId))
            {
                filter.Add("entity_id", options.EntityId);
            }

            if (!string.IsNullOrWhiteSpace(options.MemoryType))
            {
                filter.Add("memory_type", options.MemoryType);
            }

            foreach (var tag in options.Tags)
            {
                filter.Add(tag.Key, tag.Value);
            }

            var searchResult = await _memory.SearchAsync(
                query: queryText,
                index: DefaultCollection,
                filter: filter,
                minRelevance: options.MinRelevanceScore,
                limit: options.Limit,
                cancellationToken: cancellationToken
            );

            var results = new List<MemoryResult>();

            foreach (var result in searchResult.Results)
            {
                var partition = result.Partitions.FirstOrDefault();
                if (partition == null)
                    continue;

                var importance = 0.0;
                if (partition.Tags.ContainsKey("importance") && partition.Tags["importance"].Count > 0)
                {
                    double.TryParse(partition.Tags["importance"][0], out importance);
                }

                if (importance < options.MinImportance)
                    continue;

                var timestamp = DateTimeOffset.UtcNow;
                if (partition.Tags.ContainsKey("timestamp") && partition.Tags["timestamp"].Count > 0)
                {
                    DateTimeOffset.TryParse(partition.Tags["timestamp"][0], out timestamp);
                }

                if (options.FromTimestamp.HasValue && timestamp < options.FromTimestamp.Value)
                    continue;

                if (options.ToTimestamp.HasValue && timestamp > options.ToTimestamp.Value)
                    continue;

                var memoryTags = partition.Tags
                    .Where(kvp => !new[] { "entity_id", "memory_type", "importance", "timestamp" }
                        .Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => string.Join(",", kvp.Value));

                var memoryEntry = new MemoryEntry
                {
                    Id = result.Link,
                    Content = partition.Text,
                    EntityId = partition.Tags.ContainsKey("entity_id") && partition.Tags["entity_id"].Count > 0
                        ? partition.Tags["entity_id"][0]! : "",
                    MemoryType = partition.Tags.ContainsKey("memory_type") && partition.Tags["memory_type"].Count > 0
                        ? partition.Tags["memory_type"][0]! : "",
                    Importance = importance,
                    Timestamp = timestamp,
                    Tags = memoryTags
                };

                results.Add(new MemoryResult
                {
                    Memory = memoryEntry,
                    RelevanceScore = partition.Relevance,
                    Source = DefaultCollection
                });
            }

            _logger.LogDebug(
                "Retrieved {Count} memories for query '{Query}' with entity {EntityId}",
                results.Count, queryText, options.EntityId);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to retrieve memories for query '{Query}' with entity {EntityId}",
                queryText, options.EntityId);
            throw;
        }
    }

    public async Task<MemoryEntry?> GetMemoryByIdAsync(string memoryId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(memoryId))
            throw new ArgumentException("Memory ID cannot be empty", nameof(memoryId));

        try
        {
            var searchResult = await _memory.SearchAsync(
                query: memoryId,
                index: DefaultCollection,
                limit: 1,
                cancellationToken: cancellationToken
            );

            if (searchResult.Results.Count == 0)
                return null;

            var result = searchResult.Results[0];
            var partition = result.Partitions.FirstOrDefault();
            if (partition == null || result.Link != memoryId)
                return null;

            var importance = 0.0;
            if (partition.Tags.ContainsKey("importance") && partition.Tags["importance"].Count > 0)
            {
                double.TryParse(partition.Tags["importance"][0], out importance);
            }

            var timestamp = DateTimeOffset.UtcNow;
            if (partition.Tags.ContainsKey("timestamp") && partition.Tags["timestamp"].Count > 0)
            {
                DateTimeOffset.TryParse(partition.Tags["timestamp"][0], out timestamp);
            }

            var memoryTags = partition.Tags
                .Where(kvp => !new[] { "entity_id", "memory_type", "importance", "timestamp" }
                    .Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => string.Join(",", kvp.Value));

            return new MemoryEntry
            {
                Id = result.Link,
                Content = partition.Text,
                EntityId = partition.Tags.ContainsKey("entity_id") && partition.Tags["entity_id"].Count > 0
                    ? partition.Tags["entity_id"][0]! : "",
                MemoryType = partition.Tags.ContainsKey("memory_type") && partition.Tags["memory_type"].Count > 0
                    ? partition.Tags["memory_type"][0]! : "",
                Importance = importance,
                Timestamp = timestamp,
                Tags = memoryTags
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get memory by ID {MemoryId}", memoryId);
            throw;
        }
    }

    public async Task UpdateMemoryImportanceAsync(
        string memoryId,
        double importance,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(memoryId))
            throw new ArgumentException("Memory ID cannot be empty", nameof(memoryId));

        if (importance < 0.0 || importance > 1.0)
            throw new ArgumentOutOfRangeException(nameof(importance), "Importance must be between 0.0 and 1.0");

        try
        {
            var existingMemory = await GetMemoryByIdAsync(memoryId, cancellationToken);
            if (existingMemory == null)
            {
                throw new InvalidOperationException($"Memory with ID {memoryId} not found");
            }

            await _memory.DeleteDocumentAsync(memoryId, DefaultCollection, cancellationToken);

            var updatedMemory = existingMemory with { Importance = importance };
            await StoreMemoryAsync(updatedMemory, cancellationToken);

            _logger.LogInformation("Updated importance for memory {MemoryId} to {Importance}",
                memoryId, importance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update importance for memory {MemoryId}", memoryId);
            throw;
        }
    }

    public async Task DeleteMemoryAsync(string memoryId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(memoryId))
            throw new ArgumentException("Memory ID cannot be empty", nameof(memoryId));

        try
        {
            _logger.LogInformation("Deleting memory {MemoryId}", memoryId);

            await _memory.DeleteDocumentAsync(memoryId, DefaultCollection, cancellationToken);

            _logger.LogInformation("Successfully deleted memory {MemoryId}", memoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete memory {MemoryId}", memoryId);
            throw;
        }
    }

    public async Task<int> CleanOldMemoriesAsync(
        string entityId,
        TimeSpan olderThan,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("Entity ID cannot be empty", nameof(entityId));

        try
        {
            var cutoffTime = DateTimeOffset.UtcNow - olderThan;

            var options = new MemoryRetrievalOptions
            {
                EntityId = entityId,
                ToTimestamp = cutoffTime,
                Limit = 1000
            };

            var oldMemories = await RetrieveRelevantMemoriesAsync("*", options, cancellationToken);

            var deletedCount = 0;
            foreach (var memory in oldMemories)
            {
                await _memory.DeleteDocumentAsync(memory.Memory.Id, DefaultCollection, cancellationToken);
                deletedCount++;
            }

            _logger.LogInformation(
                "Cleaned {Count} old memories for entity {EntityId} older than {OlderThan}",
                deletedCount, entityId, olderThan);

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to clean old memories for entity {EntityId}",
                entityId);
            throw;
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var testQuery = "health_check";
            await _memory.SearchAsync(
                query: testQuery,
                index: DefaultCollection,
                limit: 1
            );

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Memory service health check failed");
            return false;
        }
    }
}
