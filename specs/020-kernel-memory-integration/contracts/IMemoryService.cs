using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LablabBean.Contracts.AI.Memory
{
    /// <summary>
    /// Service interface for semantic memory storage and retrieval for NPC entities.
    /// Provides contextually relevant memory access based on semantic similarity.
    /// </summary>
    public interface IMemoryService
    {
        /// <summary>
        /// Stores a memory entry with semantic embeddings for later retrieval.
        /// This operation is async and non-blocking; memory is queued for background indexing.
        /// </summary>
        /// <param name="entityId">Unique identifier of the NPC who owns this memory</param>
        /// <param name="memory">Memory entry to store</param>
        /// <returns>Task representing the async operation</returns>
        /// <exception cref="ArgumentNullException">If entityId or memory is null</exception>
        /// <exception cref="ArgumentException">If memory validation fails</exception>
        Task StoreMemoryAsync(string entityId, MemoryEntry memory);

        /// <summary>
        /// Retrieves semantically relevant memories based on a context query.
        /// Results are ranked by relevance score (semantic similarity to the query).
        /// </summary>
        /// <param name="entityId">Filter to specific NPC's memories</param>
        /// <param name="contextQuery">Natural language description of the context or question</param>
        /// <param name="options">Optional retrieval options (filters, limits, thresholds)</param>
        /// <returns>List of memory results ranked by relevance score (descending)</returns>
        /// <exception cref="ArgumentNullException">If entityId or contextQuery is null</exception>
        Task<IEnumerable<MemoryResult>> RetrieveRelevantMemoriesAsync(
            string entityId,
            string contextQuery,
            MemoryRetrievalOptions? options = null);

        /// <summary>
        /// Stores a tactical observation for combat behavior learning.
        /// Specialized storage for enemy AI to learn player patterns.
        /// </summary>
        /// <param name="enemyId">Unique identifier of the enemy entity</param>
        /// <param name="observation">Tactical observation to store</param>
        /// <returns>Task representing the async operation</returns>
        Task StoreTacticalObservationAsync(string enemyId, TacticalObservation observation);

        /// <summary>
        /// Retrieves tactical observations similar to the current combat situation.
        /// Used by tactical enemies to adapt strategies based on learned player behavior.
        /// </summary>
        /// <param name="currentSituation">Natural language description of current combat context</param>
        /// <param name="behaviorFilter">Optional filter by specific player behavior type</param>
        /// <param name="limit">Maximum number of observations to retrieve (default: 10)</param>
        /// <returns>List of tactical observations ranked by relevance</returns>
        Task<IEnumerable<TacticalObservation>> RetrieveSimilarTacticsAsync(
            string currentSituation,
            PlayerBehavior? behaviorFilter = null,
            int limit = 10);

        /// <summary>
        /// Stores a relationship event between two entities.
        /// Used to build rich interaction history for dialogue and behavior.
        /// </summary>
        /// <param name="relationshipEvent">Relationship event to store</param>
        /// <returns>Task representing the async operation</returns>
        Task StoreRelationshipEventAsync(RelationshipEvent relationshipEvent);

        /// <summary>
        /// Retrieves relationship history between two specific entities.
        /// Can be filtered by interaction type or outcome.
        /// </summary>
        /// <param name="participant1Id">First entity identifier</param>
        /// <param name="participant2Id">Second entity identifier</param>
        /// <param name="interactionTypeFilter">Optional filter by interaction type</param>
        /// <param name="limit">Maximum number of events to retrieve (default: 5)</param>
        /// <returns>List of relationship events ranked by relevance or recency</returns>
        Task<IEnumerable<RelationshipEvent>> GetRelationshipHistoryAsync(
            string participant1Id,
            string participant2Id,
            string? interactionTypeFilter = null,
            int limit = 5);

        /// <summary>
        /// Migrates legacy AvatarMemory entries to the new semantic memory system.
        /// One-time operation for each NPC during transition period.
        /// </summary>
        /// <param name="entityId">NPC entity identifier</param>
        /// <param name="legacyMemories">Collection of legacy memory entries</param>
        /// <returns>Task representing the async migration operation</returns>
        Task MigrateLegacyMemoriesAsync(string entityId, IEnumerable<MemoryEntry> legacyMemories);

        /// <summary>
        /// Deletes all memories for a specific entity.
        /// Used for cleanup or entity removal.
        /// </summary>
        /// <param name="entityId">NPC entity identifier</param>
        /// <returns>Task representing the async deletion operation</returns>
        Task DeleteEntityMemoriesAsync(string entityId);
    }
}
