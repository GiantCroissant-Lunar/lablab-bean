namespace LablabBean.Contracts.AI.Memory;

/// <summary>
/// Service for managing semantic memory storage and retrieval using Kernel Memory
/// </summary>
public interface IMemoryService
{
    /// <summary>
    /// Store a new memory entry
    /// </summary>
    /// <param name="memory">The memory entry to store</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ID of the stored memory</returns>
    Task<string> StoreMemoryAsync(MemoryEntry memory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve memories relevant to the given query text
    /// </summary>
    /// <param name="queryText">The text to search for relevant memories</param>
    /// <param name="options">Options for filtering and limiting results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of relevant memory results ordered by relevance</returns>
    Task<IReadOnlyList<MemoryResult>> RetrieveRelevantMemoriesAsync(
        string queryText,
        MemoryRetrievalOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve a specific memory by ID
    /// </summary>
    /// <param name="memoryId">The unique identifier of the memory</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The memory entry if found, null otherwise</returns>
    Task<MemoryEntry?> GetMemoryByIdAsync(string memoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the importance score of an existing memory
    /// </summary>
    /// <param name="memoryId">The unique identifier of the memory</param>
    /// <param name="importance">New importance score (0.0-1.0)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateMemoryImportanceAsync(string memoryId, double importance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a specific memory by ID
    /// </summary>
    /// <param name="memoryId">The unique identifier of the memory to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteMemoryAsync(string memoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete memories older than the specified age
    /// </summary>
    /// <param name="entityId">Entity ID to clean memories for</param>
    /// <param name="olderThan">Delete memories older than this timespan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of memories deleted</returns>
    Task<int> CleanOldMemoriesAsync(string entityId, TimeSpan olderThan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the memory service is available and operational
    /// </summary>
    /// <returns>True if the service is healthy, false otherwise</returns>
    Task<bool> IsHealthyAsync();
}
