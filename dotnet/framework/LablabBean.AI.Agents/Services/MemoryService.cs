using LablabBean.AI.Agents.Configuration;
using LablabBean.Contracts.AI.Memory;
using LablabBean.Contracts.Resilience.Services;
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
    private readonly IService? _resilience;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Threading.SemaphoreSlim> _entityLocks = new(StringComparer.OrdinalIgnoreCase);

    public MemoryService(
        ILogger<MemoryService> logger,
        IKernelMemory memory,
        IOptions<KernelMemoryOptions> options,
        IService? resilience = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _resilience = resilience; // optional; if null, calls proceed without retries
    }

    public virtual async Task<string> StoreMemoryAsync(MemoryEntry memory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(memory);

        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogDebug(
                "Storing memory {MemoryId} for entity {EntityId} with type {MemoryType} and importance {Importance}. Storage: {Provider}",
                memory.Id, memory.EntityId, memory.MemoryType, memory.Importance, _options.Storage.Provider);

            // Build tag collection for filtering and retrieval (T023)
            var tags = BuildTagCollection(memory);

            // Import memory as text document with embeddings
            // Ensure per-entity write serialization
            var (sem, acquired) = await AcquireEntityLockAsync(memory.EntityId, cancellationToken);
            try
            {
                var operation = () => _memory.ImportTextAsync(
                    text: memory.Content,
                    documentId: memory.Id,
                    tags: tags,
                    index: _options.Storage.CollectionName ?? "memories",
                    cancellationToken: cancellationToken);

                var documentId = _resilience != null
                    ? await _resilience.ExecuteWithRetryAsync(operation, retryPolicy: null, cancellationToken)
                    : await operation();

            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogInformation(
                "Successfully stored memory {MemoryId} for entity {EntityId} in {Duration}ms. Provider: {Provider}, Collection: {Collection}",
                memory.Id, memory.EntityId, duration.TotalMilliseconds,
                _options.Storage.Provider,
                _options.Storage.CollectionName ?? "memories");

            return documentId;
            }
            finally
            {
                if (acquired) sem.Release();
            }
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogError(ex,
                "Failed to store memory {MemoryId} for entity {EntityId} after {Duration}ms. Provider: {Provider}, Error: {ErrorMessage}",
                memory.Id, memory.EntityId, duration.TotalMilliseconds, _options.Storage.Provider, ex.Message);
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

        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogDebug(
                "Retrieving memories for query: '{Query}' with options: Entity={EntityId}, Type={MemoryType}, Limit={Limit}, MinRelevance={MinRelevance}. Storage: {Provider}",
                queryText, options.EntityId, options.MemoryType, options.Limit, options.MinRelevanceScore, _options.Storage.Provider);

            // Build filter from options (T023)
            var filter = BuildMemoryFilter(options);

            // Perform semantic search
            var operation = () => _memory.SearchAsync(
                query: queryText,
                index: _options.Storage.CollectionName ?? "memories",
                filter: filter,
                minRelevance: options.MinRelevanceScore,
                limit: options.Limit,
                cancellationToken: cancellationToken);

            var searchResult = _resilience != null
                ? await _resilience.ExecuteWithRetryAsync(operation, retryPolicy: null, cancellationToken)
                : await operation();

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

            var duration = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation(
                "Retrieved {Count} relevant memories for query '{Query}' in {Duration}ms (filtered from {TotalResults} results). Provider: {Provider}, Collection: {Collection}",
                results.Count, queryText, duration.TotalMilliseconds, searchResult.Results.Count,
                _options.Storage.Provider,
                _options.Storage.CollectionName ?? "memories");

            // Log relevance scores for analysis (T029 + T040)
            if (results.Any())
            {
                _logger.LogDebug(
                    "Memory retrieval stats - Top relevance: {TopRelevance:F3}, Average: {AvgRelevance:F3}, Min: {MinRelevance:F3}, Max: {MaxRelevance:F3}",
                    results.First().RelevanceScore,
                    results.Average(r => r.RelevanceScore),
                    results.Min(r => r.RelevanceScore),
                    results.Max(r => r.RelevanceScore));
            }

            return results.AsReadOnly();
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogError(ex,
                "Failed to retrieve memories for query: '{Query}' after {Duration}ms. Provider: {Provider}, Error: {ErrorMessage}",
                queryText, duration.TotalMilliseconds, _options.Storage.Provider, ex.Message);
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

            var operation = () => _memory.DeleteDocumentAsync(
                documentId: memoryId,
                index: _options.Storage.CollectionName ?? "memories",
                cancellationToken: cancellationToken);

            if (_resilience != null)
                await _resilience.ExecuteWithRetryAsync(operation, retryPolicy: null, cancellationToken);
            else
                await operation();

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

        var cutoff = DateTimeOffset.UtcNow - olderThan;

        var (sem, acquired) = await AcquireEntityLockAsync(entityId, cancellationToken);
        try
        {
            // Search broadly for this entity and filter by timestamp tag
            var filter = new MemoryFilter();
            filter.Add("entity_id", entityId);

            var operation = () => _memory.SearchAsync(
                query: entityId, // include entity id in query to bias hits
                index: _options.Storage.CollectionName ?? "memories",
                filter: filter,
                minRelevance: 0.0,
                limit: 1000,
                cancellationToken: cancellationToken);

            var search = _resilience != null
                ? await _resilience.ExecuteWithRetryAsync(operation, retryPolicy: null, cancellationToken)
                : await operation();

            var toDelete = new List<string>();
            foreach (var c in search.Results)
            {
                var p = c.Partitions.FirstOrDefault();
                if (p == null) continue;
                var tsStr = p.Tags.ContainsKey("timestamp") && p.Tags["timestamp"].Count > 0 ? p.Tags["timestamp"][0] : null;
                if (!string.IsNullOrWhiteSpace(tsStr) && DateTimeOffset.TryParse(tsStr, out var ts))
                {
                    if (ts <= cutoff)
                    {
                        toDelete.Add(c.DocumentId ?? c.Link ?? string.Empty);
                    }
                }
            }

            var deleted = 0;
            foreach (var id in toDelete.Where(id => !string.IsNullOrWhiteSpace(id)))
            {
                var delOp = () => _memory.DeleteDocumentAsync(id, _options.Storage.CollectionName ?? "memories", cancellationToken);
                if (_resilience != null)
                    await _resilience.ExecuteWithRetryAsync(delOp, retryPolicy: null, cancellationToken);
                else
                    await delOp();
                deleted++;
            }

            _logger.LogInformation("Deleted {Count} memories for entity {EntityId} older than cutoff {Cutoff}", deleted, entityId, cutoff);
            return deleted;
        }
        finally
        {
            if (acquired) sem.Release();
        }
    }

    public virtual async Task<bool> IsHealthyAsync()
    {
        var startTime = DateTimeOffset.UtcNow;
        _logger.LogDebug("Starting memory service health check for storage provider: {Provider}",
            _options.Storage.Provider);

        try
        {
            // Try a simple search to verify the memory service is working
            var operation = () => _memory.SearchAsync(
                query: "health_check",
                index: _options.Storage.CollectionName ?? "memories",
                limit: 1);
            if (_resilience != null)
                await _resilience.ExecuteWithRetryAsync(operation, retryPolicy: null, cancellationToken: default);
            else
                await operation();

            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogInformation("Memory service health check PASSED in {Duration}ms. Provider: {Provider}, Endpoint: {Endpoint}",
                duration.TotalMilliseconds,
                _options.Storage.Provider,
                _options.Storage.ConnectionString ?? "in-memory");

            return true;
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogError(ex,
                "Memory service health check FAILED after {Duration}ms. Provider: {Provider}, Endpoint: {Endpoint}. Error: {ErrorMessage}",
                duration.TotalMilliseconds,
                _options.Storage.Provider,
                _options.Storage.ConnectionString ?? "in-memory",
                ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Store a tactical observation for enemy learning (T060)
    /// </summary>
    public virtual async Task<string> StoreTacticalObservationAsync(
        string entityId,
        TacticalObservation observation,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
        ArgumentNullException.ThrowIfNull(observation);

        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogDebug(
                "Storing tactical observation for entity {EntityId}: Player {PlayerId}, Behavior {BehaviorType}, Outcome {Outcome}",
                entityId, observation.PlayerId, observation.BehaviorType, observation.Outcome);

            // Serialize observation as content
            var content = System.Text.Json.JsonSerializer.Serialize(observation);

            // Build memory entry with tactical-specific tags
            var memory = new MemoryEntry
            {
                Id = $"tactical_{entityId}_{observation.Timestamp.ToUnixTimeSeconds()}",
                Content = content,
                EntityId = entityId,
                MemoryType = "tactical",
                Importance = observation.Outcome == OutcomeType.Success ? 0.8 :
                            observation.Outcome == OutcomeType.PartialSuccess ? 0.6 : 0.4,
                Timestamp = observation.Timestamp,
                Tags = new Dictionary<string, string>
                {
                    { "player", observation.PlayerId },
                    { "behavior", observation.BehaviorType.ToString() },
                    { "outcome", observation.Outcome.ToString() },
                    { "encounter_context", observation.EncounterContext }
                }
            };

            var memoryId = await StoreMemoryAsync(memory, cancellationToken);

            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogInformation(
                "Successfully stored tactical observation {MemoryId} for entity {EntityId} in {Duration}ms. Behavior: {BehaviorType}, Outcome: {Outcome}",
                memoryId, entityId, duration.TotalMilliseconds,
                observation.BehaviorType, observation.Outcome);

            return memoryId;
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogError(ex,
                "Failed to store tactical observation for entity {EntityId} after {Duration}ms. Behavior: {BehaviorType}, Error: {ErrorMessage}",
                entityId, duration.TotalMilliseconds, observation.BehaviorType, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Retrieve similar tactical observations filtered by behavior type (T061)
    /// </summary>
    public virtual async Task<IReadOnlyList<MemoryResult>> RetrieveSimilarTacticsAsync(
        string entityId,
        LablabBean.AI.Core.Events.PlayerBehaviorType behaviorFilter,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);

        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogDebug(
                "Retrieving similar tactics for entity {EntityId}: Behavior {BehaviorType}, Limit {Limit}",
                entityId, behaviorFilter, limit);

            // Build semantic query about the behavior
            var queryText = $"Player behavior pattern: {behaviorFilter}. What tactics were effective?";

            // Build filter for tactical memories with specific behavior
            var options = new MemoryRetrievalOptions
            {
                EntityId = entityId,
                MemoryType = "tactical",
                Limit = limit,
                MinRelevanceScore = 0.3,  // Lower threshold for tactical memories
                Tags = new Dictionary<string, string>
                {
                    { "behavior", behaviorFilter.ToString() }
                }
            };

            var results = await RetrieveRelevantMemoriesAsync(queryText, options, cancellationToken);

            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogInformation(
                "Retrieved {Count} similar tactics for entity {EntityId} in {Duration}ms. Behavior: {BehaviorType}",
                results.Count, entityId, duration.TotalMilliseconds, behaviorFilter);

            return results;
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogError(ex,
                "Failed to retrieve similar tactics for entity {EntityId} after {Duration}ms. Behavior: {BehaviorType}, Error: {ErrorMessage}",
                entityId, duration.TotalMilliseconds, behaviorFilter, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Migrates legacy memories to Qdrant (T039)
    /// </summary>
    /// <param name="legacyMemories">Collection of legacy memories to migrate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of successfully migrated memories</returns>
    public virtual async Task<int> MigrateLegacyMemoriesAsync(
        IEnumerable<MemoryEntry> legacyMemories,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(legacyMemories);

        var migrationStart = DateTimeOffset.UtcNow;
        var totalCount = legacyMemories.Count();
        var successCount = 0;
        var failureCount = 0;

        _logger.LogInformation(
            "Starting legacy memory migration. Total memories: {TotalCount}, Target: {Provider} ({Endpoint})",
            totalCount,
            _options.Storage.Provider,
            _options.Storage.ConnectionString ?? "in-memory");

        foreach (var memory in legacyMemories)
        {
            try
            {
                await StoreMemoryAsync(memory, cancellationToken);
                successCount++;

                if (successCount % 100 == 0)
                {
                    _logger.LogInformation("Migration progress: {Success}/{Total} memories migrated ({Percentage:F1}%)",
                        successCount, totalCount, (successCount / (double)totalCount) * 100);
                }
            }
            catch (Exception ex)
            {
                failureCount++;
                _logger.LogWarning(ex,
                    "Failed to migrate memory {MemoryId} for entity {EntityId}. Continuing with next memory.",
                    memory.Id, memory.EntityId);
            }
        }

        var duration = DateTimeOffset.UtcNow - migrationStart;
        _logger.LogInformation(
            "Legacy memory migration completed. Success: {Success}, Failures: {Failures}, Duration: {Duration}s, Rate: {Rate:F2} memories/sec",
            successCount,
            failureCount,
            duration.TotalSeconds,
            successCount / Math.Max(duration.TotalSeconds, 0.1));

        return successCount;
    }

    #region Helper Methods

    /// <summary>
    /// Builds tag collection for memory storage (T023)
    /// Tags enable filtering during semantic search
    /// </summary>
    private static TagCollection BuildTagCollection(MemoryEntry memory)
    {
        // Standardize on canonical keys: entity_id, memory_type
        var tags = new TagCollection
        {
            { "entity_id", memory.EntityId },
            { "memory_type", memory.MemoryType },
            { "importance", memory.Importance.ToString("F2") },
            { "timestamp", memory.Timestamp.ToString("O") }
        };

        // Add user-provided tags as-is (no custom_ prefix)
        foreach (var tag in memory.Tags)
        {
            tags.Add(tag.Key, tag.Value);
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
            filter.Add("entity_id", options.EntityId);
            hasFilters = true;
        }

        // Memory type filter
        if (!string.IsNullOrWhiteSpace(options.MemoryType))
        {
            // Support comma-separated types
            var types = options.MemoryType.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // For now, use only the first type. Multi-type OR filtering will be handled client-side
            filter.Add("memory_type", types[0]);
            hasFilters = true;
        }

        // Custom tag filters
        foreach (var tag in options.Tags)
        {
            filter.Add(tag.Key, tag.Value);
            hasFilters = true;
        }

        return hasFilters ? filter : null;
    }

    /// <summary>
    /// Parses MemoryEntry from Kernel Memory partition
    /// </summary>
    private MemoryEntry? ParseMemoryEntryFromPartition(Microsoft.KernelMemory.Citation.Partition partition, string documentId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(partition.Text))
                return null;

            // Extract tags from partition - partition.Tags is Dictionary<string, List<string>>
            // Backward compatibility: support both entity_id/memory_type and entity/type
            var entityId = GetTagValue(partition.Tags, "entity_id")
                           ?? GetTagValue(partition.Tags, "entity")
                           ?? "unknown";
            var memoryType = GetTagValue(partition.Tags, "memory_type")
                             ?? GetTagValue(partition.Tags, "type")
                             ?? "unknown";

            var importanceStr = GetTagValue(partition.Tags, "importance");
            var importance = double.TryParse(importanceStr, out var imp) ? imp : 0.5;

            var timestampStr = GetTagValue(partition.Tags, "timestamp");
            var timestamp = DateTimeOffset.TryParse(timestampStr, out var ts) ? ts : DateTimeOffset.UtcNow;

            // Extract user tags: include all except canonical keys
            var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "entity_id", "entity", "memory_type", "type", "importance", "timestamp"
            };
            var customTags = partition.Tags
                .Where(t => !exclude.Contains(t.Key))
                .ToDictionary(
                    t => t.Key,
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

    #region Relationship Memory (T074-T075)

    public virtual async Task<string> StoreRelationshipMemoryAsync(
        RelationshipMemory relationshipMemory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(relationshipMemory);

        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogDebug(
                "Storing relationship memory between {Entity1} and {Entity2}: Type={InteractionType}, Sentiment={Sentiment}",
                relationshipMemory.Entity1Id, relationshipMemory.Entity2Id,
                relationshipMemory.InteractionType, relationshipMemory.Sentiment);

            var memoryId = $"relationship_{Guid.NewGuid():N}";

            // Build comprehensive tags for relationship tracking
            var tags = new TagCollection
            {
                { "type", "relationship" },
                { "entity1", relationshipMemory.Entity1Id },
                { "entity2", relationshipMemory.Entity2Id },
                { "relationship", $"{relationshipMemory.Entity1Id}_{relationshipMemory.Entity2Id}" },
                { "relationship_reverse", $"{relationshipMemory.Entity2Id}_{relationshipMemory.Entity1Id}" },
                { "interaction", relationshipMemory.InteractionType.ToString().ToLowerInvariant() },
                { "sentiment", relationshipMemory.Sentiment.ToLowerInvariant() },
                { "timestamp", relationshipMemory.Timestamp.ToUnixTimeSeconds().ToString() }
            };

            // Build rich description for semantic search
            var description = $"{relationshipMemory.Entity1Id} and {relationshipMemory.Entity2Id} - " +
                             $"{relationshipMemory.InteractionType}: {relationshipMemory.Description} " +
                             $"(Sentiment: {relationshipMemory.Sentiment})";

            var documentId = await _memory.ImportTextAsync(
                text: description,
                documentId: memoryId,
                tags: tags,
                index: _options.Storage.CollectionName ?? "memories",
                cancellationToken: cancellationToken);

            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogInformation(
                "Successfully stored relationship memory between {Entity1} and {Entity2} in {Duration}ms",
                relationshipMemory.Entity1Id, relationshipMemory.Entity2Id, duration.TotalMilliseconds);

            return documentId;
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogError(ex,
                "Failed to store relationship memory between {Entity1} and {Entity2} after {Duration}ms: {ErrorMessage}",
                relationshipMemory.Entity1Id, relationshipMemory.Entity2Id, duration.TotalMilliseconds, ex.Message);
            throw;
        }
    }

    public virtual async Task<IReadOnlyList<MemoryResult>> RetrieveRelevantRelationshipHistoryAsync(
        string entity1Id,
        string entity2Id,
        string query,
        int maxResults = 5,
        string? sentiment = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entity1Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(entity2Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogDebug(
                "Retrieving relationship history between {Entity1} and {Entity2}: Query='{Query}', Sentiment={Sentiment}, Limit={Limit}",
                entity1Id, entity2Id, query, sentiment ?? "any", maxResults);

            // Build filter for bidirectional relationship
            var filterBuilder = new List<MemoryFilter>();

            // Match either direction of the relationship
            var relationshipKey = $"{entity1Id}_{entity2Id}";
            var reverseKey = $"{entity2Id}_{entity1Id}";

            // Type must be relationship
            filterBuilder.Add(MemoryFilters.ByTag("type", "relationship"));

            // Create OR condition for bidirectional lookup
            // Note: Kernel Memory filter syntax may vary - this is a simplified approach
            // We'll check both relationship tags in post-processing if direct OR isn't supported
            var filter = new MemoryFilter();
            filter.ByTag("type", "relationship");

            // Add entity filters (will match if either entity is in the relationship)
            filter.ByTag("entity1", entity1Id).ByTag("entity2", entity2Id);

            // Add sentiment filter if specified
            if (!string.IsNullOrWhiteSpace(sentiment))
            {
                filter.ByTag("sentiment", sentiment.ToLowerInvariant());
            }

            // Perform semantic search
            var operation = () => _memory.SearchAsync(
                query: $"{query} {entity1Id} {entity2Id}",
                index: _options.Storage.CollectionName ?? "memories",
                filter: filter,
                minRelevance: 0.3,
                limit: maxResults * 2,
                cancellationToken: cancellationToken);

            var searchResult = _resilience != null
                ? await _resilience.ExecuteWithRetryAsync(operation, retryPolicy: null, cancellationToken)
                : await operation();

            // Filter and convert results
            var results = new List<MemoryResult>();
            foreach (var citation in searchResult.Results)
            {
                // Check if this memory involves both entities (bidirectional check)
                var entity1Tag = GetTagValue(citation.Partitions.FirstOrDefault()?.Tags, "entity1");
                var entity2Tag = GetTagValue(citation.Partitions.FirstOrDefault()?.Tags, "entity2");

                var isBetweenEntities =
                    (entity1Tag == entity1Id && entity2Tag == entity2Id) ||
                    (entity1Tag == entity2Id && entity2Tag == entity1Id);

                if (!isBetweenEntities)
                {
                    continue; // Skip memories not between these two entities
                }

                // Build MemoryEntry from partition
                var partition = citation.Partitions.FirstOrDefault();
                if (partition == null)
                    continue;

                var tags = partition.Tags;

                // Parse timestamp
                var timestampStr = GetTagValue(tags, "timestamp");
                var timestamp = long.TryParse(timestampStr, out var ts)
                    ? DateTimeOffset.FromUnixTimeSeconds(ts)
                    : DateTimeOffset.UtcNow;

                var memory = new MemoryEntry
                {
                    Id = citation.Link,
                    Content = partition.Text,
                    EntityId = $"{entity1Id}_{entity2Id}", // Composite entity ID
                    MemoryType = "relationship",
                    Importance = partition.Relevance,
                    Timestamp = timestamp,
                    Tags = tags.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.FirstOrDefault() ?? string.Empty)
                };

                results.Add(new MemoryResult
                {
                    Memory = memory,
                    RelevanceScore = partition.Relevance,
                    Source = _options.Storage.CollectionName ?? "memories"
                });

                if (results.Count >= maxResults)
                {
                    break; // Reached desired limit
                }
            }

            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogInformation(
                "Retrieved {Count} relationship memories between {Entity1} and {Entity2} in {Duration}ms",
                results.Count, entity1Id, entity2Id, duration.TotalMilliseconds);

            return results.AsReadOnly();
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogError(ex,
                "Failed to retrieve relationship history between {Entity1} and {Entity2} after {Duration}ms: {ErrorMessage}",
                entity1Id, entity2Id, duration.TotalMilliseconds, ex.Message);
            throw;
        }
    }

    #endregion

    private async Task<(System.Threading.SemaphoreSlim sem, bool acquired)> AcquireEntityLockAsync(string entityId, CancellationToken ct)
    {
        var sem = _entityLocks.GetOrAdd(entityId, _ => new System.Threading.SemaphoreSlim(1, 1));
        await sem.WaitAsync(ct);
        return (sem, true);
    }
}
