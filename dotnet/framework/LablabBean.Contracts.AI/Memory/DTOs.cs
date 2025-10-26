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

/// <summary>
/// Represents a document to be indexed in the knowledge base
/// </summary>
public record KnowledgeBaseDocument
{
    /// <summary>
    /// Unique identifier for the document
    /// </summary>
    public required string DocumentId { get; init; }

    /// <summary>
    /// Display name of the document
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Full text content of the document
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Category/type of document (e.g., "policy", "handbook", "lore")
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Target role(s) for this document (e.g., "employee", "boss", "all")
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Optional tags for filtering
    /// </summary>
    public Dictionary<string, string> Tags { get; init; } = new();

    /// <summary>
    /// When the document was indexed
    /// </summary>
    public DateTimeOffset IndexedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Answer from a knowledge base query with supporting citations
/// </summary>
public record KnowledgeBaseAnswer
{
    /// <summary>
    /// The synthesized answer text
    /// </summary>
    public required string Answer { get; init; }

    /// <summary>
    /// Original query that produced this answer
    /// </summary>
    public required string Query { get; init; }

    /// <summary>
    /// Supporting citations from source documents
    /// </summary>
    public required IReadOnlyList<Citation> Citations { get; init; }

    /// <summary>
    /// Confidence score of the answer (0.0-1.0)
    /// </summary>
    public double ConfidenceScore { get; init; }

    /// <summary>
    /// Whether the answer has sufficient grounding in the knowledge base
    /// </summary>
    public bool IsGrounded => Citations.Count > 0 && ConfidenceScore >= 0.5;
}

/// <summary>
/// Citation referencing a source document
/// </summary>
public record Citation
{
    /// <summary>
    /// Document identifier
    /// </summary>
    public required string DocumentId { get; init; }

    /// <summary>
    /// Document title
    /// </summary>
    public required string DocumentTitle { get; init; }

    /// <summary>
    /// Relevant text excerpt from the document
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Relevance score for this citation (0.0-1.0)
    /// </summary>
    public double RelevanceScore { get; init; }

    /// <summary>
    /// Optional partition key (for chunked documents)
    /// </summary>
    public string? PartitionKey { get; init; }

    /// <summary>
    /// Optional tags from the source document
    /// </summary>
    public Dictionary<string, string> Tags { get; init; } = new();
}

/// <summary>
/// Tactical observation for enemy learning and adaptation
/// Records player behavior patterns and tactic effectiveness for future encounters
/// </summary>
public record TacticalObservation
{
    /// <summary>
    /// Player identifier whose behavior was observed
    /// </summary>
    public required string PlayerId { get; init; }

    /// <summary>
    /// Type of behavior exhibited by the player
    /// </summary>
    public required LablabBean.AI.Core.Events.PlayerBehaviorType BehaviorType { get; init; }

    /// <summary>
    /// Contextual description of the encounter
    /// </summary>
    public required string EncounterContext { get; init; }

    /// <summary>
    /// Dictionary mapping tactic names to their effectiveness scores (0.0-1.0)
    /// </summary>
    public required Dictionary<string, float> TacticEffectiveness { get; init; }

    /// <summary>
    /// Overall outcome of the encounter
    /// </summary>
    public OutcomeType Outcome { get; init; }

    /// <summary>
    /// When this observation was recorded
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Types of interactions between entities in relationship tracking
/// </summary>
public enum InteractionType
{
    /// <summary>
    /// Verbal communication or dialogue
    /// </summary>
    Conversation,

    /// <summary>
    /// Exchange of items or resources
    /// </summary>
    Trade,

    /// <summary>
    /// Hostile engagement or conflict
    /// </summary>
    Combat,

    /// <summary>
    /// Working together towards a common goal
    /// </summary>
    Collaboration,

    /// <summary>
    /// Violation of trust or hostile action
    /// </summary>
    Betrayal,

    /// <summary>
    /// One-way transfer of items or favors
    /// </summary>
    Gift,

    /// <summary>
    /// Shared mission or objective completion
    /// </summary>
    Quest
}

/// <summary>
/// Relationship memory tracking interactions between two entities
/// Supports semantic search for contextually relevant relationship history
/// </summary>
public record RelationshipMemory
{
    /// <summary>
    /// First entity in the relationship (typically the observer or initiator)
    /// </summary>
    public required string Entity1Id { get; init; }

    /// <summary>
    /// Second entity in the relationship (typically the observed or recipient)
    /// </summary>
    public required string Entity2Id { get; init; }

    /// <summary>
    /// Type of interaction that occurred
    /// </summary>
    public required InteractionType InteractionType { get; init; }

    /// <summary>
    /// Emotional tone of the interaction: "positive", "negative", or "neutral"
    /// </summary>
    public required string Sentiment { get; init; }

    /// <summary>
    /// Detailed description of what happened during the interaction
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// When this interaction occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional metadata for extending relationship memories
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}
