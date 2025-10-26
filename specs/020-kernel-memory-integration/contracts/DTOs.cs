using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.AI.Memory
{
    #region Memory Core DTOs

    /// <summary>
    /// Represents a single NPC memory entry with metadata.
    /// </summary>
    public class MemoryEntry
    {
        /// <summary>
        /// Unique identifier of the NPC who owns this memory.
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Category of memory (e.g., "decision", "interaction", "event").
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Natural language description of the memory (used for semantic search).
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// When the memory was created.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Importance score for retention prioritization (0.0-1.0).
        /// </summary>
        public float Importance { get; set; } = 0.5f;

        /// <summary>
        /// Additional context (emotion, location, target entities, etc.).
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Options for configuring memory retrieval queries.
    /// </summary>
    public class MemoryRetrievalOptions
    {
        /// <summary>
        /// Filter by specific event types (e.g., ["interaction", "decision"]).
        /// Null/empty = all types.
        /// </summary>
        public List<string>? MemoryTypes { get; set; }

        /// <summary>
        /// Minimum importance threshold (0.0-1.0).
        /// Null = no importance filter.
        /// </summary>
        public float? MinImportance { get; set; }

        /// <summary>
        /// Retrieve memories created after this time.
        /// Null = no time filter.
        /// </summary>
        public DateTime? TimestampFrom { get; set; }

        /// <summary>
        /// Retrieve memories created before this time.
        /// Null = no time filter.
        /// </summary>
        public DateTime? TimestampTo { get; set; }

        /// <summary>
        /// Maximum number of memories to retrieve.
        /// Default: 5
        /// </summary>
        public int Limit { get; set; } = 5;

        /// <summary>
        /// Minimum semantic similarity score (0.0-1.0).
        /// Default: 0.7 (70% similarity required)
        /// </summary>
        public float MinRelevance { get; set; } = 0.7f;
    }

    /// <summary>
    /// Represents a retrieved memory with relevance scoring.
    /// </summary>
    public class MemoryResult
    {
        /// <summary>
        /// The original memory entry.
        /// </summary>
        public MemoryEntry Memory { get; set; } = new();

        /// <summary>
        /// Semantic similarity to the query (0.0-1.0).
        /// Higher scores indicate stronger match.
        /// </summary>
        public float RelevanceScore { get; set; }

        /// <summary>
        /// Kernel Memory document ID for tracing/debugging.
        /// </summary>
        public string DocumentId { get; set; } = string.Empty;

        /// <summary>
        /// For knowledge base queries: which document section matched.
        /// Null for regular memory queries.
        /// </summary>
        public string? SourcePartition { get; set; }
    }

    #endregion

    #region Knowledge Base DTOs

    /// <summary>
    /// Represents an indexed reference document for NPC behavior grounding.
    /// </summary>
    public class KnowledgeBaseDocument
    {
        /// <summary>
        /// Unique identifier for the document.
        /// </summary>
        public string DocumentId { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable document title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Full document text content (max 50,000 characters).
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Which NPC roles can access this document (e.g., ["boss", "employee"]).
        /// Must have at least one role tag.
        /// </summary>
        public List<string> RoleTags { get; set; } = new();

        /// <summary>
        /// Additional categorization (e.g., ["policy", "lore"]).
        /// </summary>
        public List<string> CategoryTags { get; set; } = new();

        /// <summary>
        /// When the document was last modified.
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Metadata for a knowledge base document (without full content).
    /// Used for listing and management operations.
    /// </summary>
    public class KnowledgeBaseDocumentMetadata
    {
        public string DocumentId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<string> RoleTags { get; set; } = new();
        public List<string> CategoryTags { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public int ContentLength { get; set; } // Character count
    }

    /// <summary>
    /// Answer from knowledge base query with source citations.
    /// </summary>
    public class KnowledgeBaseAnswer
    {
        /// <summary>
        /// Natural language answer to the question.
        /// </summary>
        public string Answer { get; set; } = string.Empty;

        /// <summary>
        /// Source citations showing which documents grounded the answer.
        /// </summary>
        public List<Citation> Citations { get; set; } = new();

        /// <summary>
        /// Confidence score (0.0-1.0) in the answer quality.
        /// </summary>
        public float ConfidenceScore { get; set; }

        /// <summary>
        /// True if no relevant knowledge was found.
        /// </summary>
        public bool NoRelevantKnowledge { get; set; } = false;
    }

    /// <summary>
    /// Source citation for knowledge base answer.
    /// </summary>
    public class Citation
    {
        /// <summary>
        /// Which document answered the question.
        /// </summary>
        public string SourceDocumentId { get; set; } = string.Empty;

        /// <summary>
        /// Document title for human readability.
        /// </summary>
        public string SourceTitle { get; set; } = string.Empty;

        /// <summary>
        /// Which section/partition of the document matched.
        /// </summary>
        public string Partition { get; set; } = string.Empty;

        /// <summary>
        /// Relevance score of this citation (0.0-1.0).
        /// </summary>
        public float RelevanceScore { get; set; }

        /// <summary>
        /// Excerpt from the source document.
        /// </summary>
        public string Excerpt { get; set; } = string.Empty;
    }

    #endregion

    #region Tactical Memory DTOs

    /// <summary>
    /// Player combat behavior patterns observed by tactical enemies.
    /// </summary>
    public enum PlayerBehavior
    {
        AggressiveRush,         // Player charges directly
        DefensivePositioning,   // Player maintains distance
        HitAndRun,              // Player engages briefly then retreats
        AbilityCombo,           // Player uses specific ability sequence
        Flanking,               // Player attempts to circle around
        Kiting                  // Player attacks while moving away
    }

    /// <summary>
    /// Tactical observation for combat behavior learning.
    /// </summary>
    public class TacticalObservation
    {
        /// <summary>
        /// Which enemy observed this behavior.
        /// </summary>
        public string EnemyEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Type of player behavior observed.
        /// </summary>
        public PlayerBehavior PlayerBehavior { get; set; }

        /// <summary>
        /// Combat context description.
        /// </summary>
        public string Situation { get; set; } = string.Empty;

        /// <summary>
        /// How well the observed behavior worked for player (0.0-1.0).
        /// </summary>
        public float EffectivenessRating { get; set; }

        /// <summary>
        /// Tactic used to counter (for learning).
        /// </summary>
        public string? CounterTactic { get; set; }

        /// <summary>
        /// When observation occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional context (location, health, enemy count, etc.).
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    #endregion

    #region Relationship DTOs

    /// <summary>
    /// Outcome type for relationship events.
    /// </summary>
    public enum OutcomeType
    {
        Positive,   // Successful, beneficial outcome
        Neutral,    // No significant impact
        Negative    // Conflict, failure, problematic
    }

    /// <summary>
    /// Records interaction between two entities for relationship tracking.
    /// </summary>
    public class RelationshipEvent
    {
        /// <summary>
        /// Unique identifier for this event.
        /// </summary>
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// First entity involved in the interaction.
        /// </summary>
        public string Participant1Id { get; set; } = string.Empty;

        /// <summary>
        /// Second entity involved in the interaction.
        /// </summary>
        public string Participant2Id { get; set; } = string.Empty;

        /// <summary>
        /// Type of interaction (e.g., "conflict", "collaboration", "customer_service").
        /// </summary>
        public string InteractionType { get; set; } = string.Empty;

        /// <summary>
        /// Natural language description of the interaction.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Result of the interaction.
        /// </summary>
        public OutcomeType Outcome { get; set; }

        /// <summary>
        /// Emotional effect (e.g., "frustrated", "pleased", "neutral").
        /// </summary>
        public string? EmotionalImpact { get; set; }

        /// <summary>
        /// When the interaction occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    #endregion
}
