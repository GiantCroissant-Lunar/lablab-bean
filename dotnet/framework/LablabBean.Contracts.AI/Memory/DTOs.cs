namespace LablabBean.Contracts.AI.Memory;

/// <summary>
/// Represents a memory entry stored in the memory system
/// </summary>
public record MemoryEntry
{
    /// <summary>
    /// Unique identifier for the memory entry
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The actual content/text of the memory
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Entity ID this memory is associated with (e.g., NPC ID, player ID)
    /// </summary>
    public required string EntityId { get; init; }

    /// <summary>
    /// Type of memory (e.g., "conversation", "observation", "tactical", "relationship")
    /// </summary>
    public required string MemoryType { get; init; }

    /// <summary>
    /// Importance/priority score (0.0-1.0)
    /// </summary>
    public double Importance { get; init; }

    /// <summary>
    /// When this memory was created
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional tags for filtering
    /// </summary>
    public Dictionary<string, string> Tags { get; init; } = new();

    /// <summary>
    /// Optional metadata for extending memory entries
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Options for retrieving memories
/// </summary>
public record MemoryRetrievalOptions
{
    /// <summary>
    /// Entity ID to retrieve memories for
    /// </summary>
    public string? EntityId { get; init; }

    /// <summary>
    /// Filter by memory type
    /// </summary>
    public string? MemoryType { get; init; }

    /// <summary>
    /// Minimum relevance score threshold (0.0-1.0)
    /// </summary>
    public double MinRelevanceScore { get; init; } = 0.5;

    /// <summary>
    /// Minimum importance score threshold (0.0-1.0)
    /// </summary>
    public double MinImportance { get; init; } = 0.0;

    /// <summary>
    /// Maximum number of memories to retrieve
    /// </summary>
    public int Limit { get; init; } = 10;

    /// <summary>
    /// Optional tags to filter by
    /// </summary>
    public Dictionary<string, string> Tags { get; init; } = new();

    /// <summary>
    /// Optional time range filter - start
    /// </summary>
    public DateTimeOffset? FromTimestamp { get; init; }

    /// <summary>
    /// Optional time range filter - end
    /// </summary>
    public DateTimeOffset? ToTimestamp { get; init; }
}

/// <summary>
/// Result of a memory retrieval operation
/// </summary>
public record MemoryResult
{
    /// <summary>
    /// The memory entry
    /// </summary>
    public required MemoryEntry Memory { get; init; }

    /// <summary>
    /// Relevance score for this memory relative to the query (0.0-1.0)
    /// </summary>
    public double RelevanceScore { get; init; }

    /// <summary>
    /// Source identifier (e.g., collection name, index name)
    /// </summary>
    public string? Source { get; init; }
}

/// <summary>
/// Player behavior patterns for tactical learning
/// </summary>
public enum PlayerBehavior
{
    Aggressive,
    Defensive,
    Stealthy,
    Predictable,
    Erratic,
    Exploitative,
    Cautious
}

/// <summary>
/// Outcome types for tactical memory
/// </summary>
public enum OutcomeType
{
    Success,
    Failure,
    Neutral,
    PartialSuccess
}
