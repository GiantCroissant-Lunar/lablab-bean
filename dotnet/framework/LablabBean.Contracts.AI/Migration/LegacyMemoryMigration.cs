using LablabBean.AI.Core.Models;
using LablabBean.Contracts.AI.Memory;
using Microsoft.Extensions.Logging;

namespace LablabBean.Contracts.AI.Migration;

/// <summary>
/// Migrates legacy AvatarMemory (short/long term lists) to semantic memory with Qdrant persistence
/// </summary>
public class LegacyMemoryMigration
{
    private readonly IMemoryService _memoryService;
    private readonly ILogger<LegacyMemoryMigration> _logger;

    public LegacyMemoryMigration(
        IMemoryService memoryService,
        ILogger<LegacyMemoryMigration> logger)
    {
        _memoryService = memoryService ?? throw new ArgumentNullException(nameof(memoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Migrates legacy avatar memories to the new semantic memory system
    /// </summary>
    /// <param name="legacyMemory">The legacy avatar memory to migrate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Migration result with statistics</returns>
    public async Task<MigrationResult> MigrateAvatarMemoryAsync(
        AvatarMemory legacyMemory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(legacyMemory);

        _logger.LogInformation(
            "Starting memory migration for entity {EntityId} - {ShortTermCount} short-term, {LongTermCount} long-term memories",
            legacyMemory.EntityId,
            legacyMemory.ShortTermMemory.Count,
            legacyMemory.LongTermMemory.Count);

        var result = new MigrationResult
        {
            EntityId = legacyMemory.EntityId,
            StartTime = DateTimeOffset.UtcNow
        };

        try
        {
            // Migrate short-term memories
            foreach (var memory in legacyMemory.ShortTermMemory)
            {
                try
                {
                    var migrated = await MigrateMemoryEntryAsync(
                        memory,
                        legacyMemory.EntityId,
                        isShortTerm: true,
                        cancellationToken);

                    if (migrated)
                        result.MigratedCount++;
                    else
                        result.SkippedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to migrate short-term memory for entity {EntityId}: {Description}",
                        legacyMemory.EntityId, memory.Description);
                    result.FailedCount++;
                    result.Errors.Add($"Short-term: {memory.Description} - {ex.Message}");
                }
            }

            // Migrate long-term memories
            foreach (var memory in legacyMemory.LongTermMemory)
            {
                try
                {
                    var migrated = await MigrateMemoryEntryAsync(
                        memory,
                        legacyMemory.EntityId,
                        isShortTerm: false,
                        cancellationToken);

                    if (migrated)
                        result.MigratedCount++;
                    else
                        result.SkippedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to migrate long-term memory for entity {EntityId}: {Description}",
                        legacyMemory.EntityId, memory.Description);
                    result.FailedCount++;
                    result.Errors.Add($"Long-term: {memory.Description} - {ex.Message}");
                }
            }

            result.EndTime = DateTimeOffset.UtcNow;
            result.Success = result.FailedCount == 0;

            _logger.LogInformation(
                "Memory migration completed for entity {EntityId} - Migrated: {Migrated}, Skipped: {Skipped}, Failed: {Failed}, Duration: {Duration}ms",
                legacyMemory.EntityId,
                result.MigratedCount,
                result.SkippedCount,
                result.FailedCount,
                (result.EndTime - result.StartTime).TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Memory migration failed for entity {EntityId}",
                legacyMemory.EntityId);

            result.Success = false;
            result.EndTime = DateTimeOffset.UtcNow;
            result.Errors.Add($"Fatal error: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Migrates a single legacy memory entry
    /// </summary>
    private async Task<bool> MigrateMemoryEntryAsync(
        LablabBean.AI.Core.Models.MemoryEntry legacyEntry,
        string entityId,
        bool isShortTerm,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(legacyEntry.Description))
        {
            _logger.LogDebug("Skipping empty memory entry");
            return false;
        }

        var semanticEntry = new LablabBean.Contracts.AI.Memory.MemoryEntry
        {
            Id = $"{entityId}_{legacyEntry.Timestamp:yyyyMMddHHmmss}_{Guid.NewGuid():N}",
            EntityId = entityId,
            Content = legacyEntry.Description,
            MemoryType = legacyEntry.EventType,
            Importance = legacyEntry.Importance,
            Timestamp = new DateTimeOffset(legacyEntry.Timestamp),
            Tags = new Dictionary<string, string>
            {
                { "migrated_from", isShortTerm ? "short_term" : "long_term" },
                { "migration_date", DateTimeOffset.UtcNow.ToString("O") }
            }
        };

        // Add metadata as tags
        foreach (var meta in legacyEntry.Metadata)
        {
            if (meta.Value != null)
            {
                semanticEntry.Tags[$"meta_{meta.Key}"] = meta.Value.ToString()!;
            }
        }

        await _memoryService.StoreMemoryAsync(semanticEntry, cancellationToken);

        _logger.LogDebug(
            "Migrated memory: {MemoryId} for entity {EntityId} from {Source}",
            semanticEntry.Id, entityId, isShortTerm ? "short-term" : "long-term");

        return true;
    }

    /// <summary>
    /// Migrates multiple avatar memories in batch
    /// </summary>
    /// <param name="legacyMemories">Collection of legacy memories to migrate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Summary of all migrations</returns>
    public async Task<BatchMigrationResult> MigrateBatchAsync(
        IEnumerable<AvatarMemory> legacyMemories,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(legacyMemories);

        var batchResult = new BatchMigrationResult
        {
            StartTime = DateTimeOffset.UtcNow
        };

        _logger.LogInformation("Starting batch memory migration");

        foreach (var legacyMemory in legacyMemories)
        {
            try
            {
                var result = await MigrateAvatarMemoryAsync(legacyMemory, cancellationToken);
                batchResult.Results.Add(result);

                batchResult.TotalMigrated += result.MigratedCount;
                batchResult.TotalSkipped += result.SkippedCount;
                batchResult.TotalFailed += result.FailedCount;

                if (!result.Success)
                    batchResult.FailedEntities.Add(legacyMemory.EntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Critical failure migrating entity {EntityId}",
                    legacyMemory.EntityId);

                batchResult.FailedEntities.Add(legacyMemory.EntityId);
            }
        }

        batchResult.EndTime = DateTimeOffset.UtcNow;
        batchResult.Success = batchResult.FailedEntities.Count == 0;

        _logger.LogInformation(
            "Batch migration completed - Entities: {Count}, Migrated: {Migrated}, Skipped: {Skipped}, Failed: {Failed}, Duration: {Duration}s",
            batchResult.Results.Count,
            batchResult.TotalMigrated,
            batchResult.TotalSkipped,
            batchResult.TotalFailed,
            (batchResult.EndTime - batchResult.StartTime).TotalSeconds);

        return batchResult;
    }
}

/// <summary>
/// Result of migrating a single avatar's memories
/// </summary>
public class MigrationResult
{
    public string EntityId { get; set; } = string.Empty;
    public int MigratedCount { get; set; }
    public int SkippedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}

/// <summary>
/// Result of batch migration
/// </summary>
public class BatchMigrationResult
{
    public List<MigrationResult> Results { get; set; } = new();
    public int TotalMigrated { get; set; }
    public int TotalSkipped { get; set; }
    public int TotalFailed { get; set; }
    public List<string> FailedEntities { get; set; } = new();
    public bool Success { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}
