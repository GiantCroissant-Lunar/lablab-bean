using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LablabBean.Contracts.AI.Memory
{
    /// <summary>
    /// Service interface for knowledge base RAG (Retrieval Augmented Generation).
    /// Enables NPCs to query indexed reference documents for behavior grounding.
    /// </summary>
    public interface IKnowledgeBaseService
    {
        /// <summary>
        /// Indexes a document into the knowledge base with semantic embeddings.
        /// Documents are tagged by role for access control.
        /// </summary>
        /// <param name="document">Knowledge base document to index</param>
        /// <returns>Task representing the async indexing operation</returns>
        /// <exception cref="ArgumentNullException">If document is null</exception>
        /// <exception cref="ArgumentException">If document validation fails</exception>
        Task IndexDocumentAsync(KnowledgeBaseDocument document);

        /// <summary>
        /// Queries the knowledge base with a natural language question.
        /// Returns grounded answer with source citations.
        /// </summary>
        /// <param name="question">Natural language question to answer</param>
        /// <param name="roleFilter">Filter documents by role tags (e.g., "boss", "employee")</param>
        /// <param name="categoryFilter">Optional additional category filter (e.g., "policy")</param>
        /// <returns>Knowledge base answer with source citations</returns>
        /// <exception cref="ArgumentNullException">If question is null</exception>
        Task<KnowledgeBaseAnswer> QueryKnowledgeBaseAsync(
            string question,
            string roleFilter,
            string? categoryFilter = null);

        /// <summary>
        /// Updates an existing document in the knowledge base.
        /// Automatically re-indexes with new embeddings.
        /// </summary>
        /// <param name="documentId">Identifier of document to update</param>
        /// <param name="updatedDocument">New document content and metadata</param>
        /// <returns>Task representing the async update operation</returns>
        Task UpdateDocumentAsync(string documentId, KnowledgeBaseDocument updatedDocument);

        /// <summary>
        /// Deletes a document from the knowledge base.
        /// </summary>
        /// <param name="documentId">Identifier of document to delete</param>
        /// <returns>Task representing the async deletion operation</returns>
        Task DeleteDocumentAsync(string documentId);

        /// <summary>
        /// Lists all indexed documents, optionally filtered by role or category.
        /// Used for knowledge base management and debugging.
        /// </summary>
        /// <param name="roleFilter">Optional filter by role tag</param>
        /// <param name="categoryFilter">Optional filter by category tag</param>
        /// <returns>List of knowledge base document metadata (not full content)</returns>
        Task<IEnumerable<KnowledgeBaseDocumentMetadata>> ListDocumentsAsync(
            string? roleFilter = null,
            string? categoryFilter = null);

        /// <summary>
        /// Retrieves the full content of a specific document.
        /// Used for review or editing purposes.
        /// </summary>
        /// <param name="documentId">Identifier of document to retrieve</param>
        /// <returns>Full knowledge base document with content</returns>
        /// <exception cref="KeyNotFoundException">If document not found</exception>
        Task<KnowledgeBaseDocument> GetDocumentAsync(string documentId);
    }
}
