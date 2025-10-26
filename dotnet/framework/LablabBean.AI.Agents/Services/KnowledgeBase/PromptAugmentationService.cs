using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Core.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.AI.Agents.Services.KnowledgeBase;

/// <summary>
/// Service for augmenting prompts with retrieved knowledge base context
/// </summary>
public class PromptAugmentationService : IPromptAugmentationService
{
    private readonly ILogger<PromptAugmentationService> _logger;
    private readonly IKnowledgeBaseService _knowledgeBase;

    public PromptAugmentationService(
        ILogger<PromptAugmentationService> logger,
        IKnowledgeBaseService knowledgeBase)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _knowledgeBase = knowledgeBase ?? throw new ArgumentNullException(nameof(knowledgeBase));
    }

    public async Task<RagContext> AugmentQueryAsync(
        string query,
        int topK = 5,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        _logger.LogDebug("Augmenting query with knowledge base context: {Query}", query);

        var context = new RagContext
        {
            Query = query
        };

        try
        {
            // Retrieve relevant documents
            var searchResults = await _knowledgeBase.SearchAsync(
                query: query,
                topK: topK,
                category: category,
                cancellationToken: cancellationToken);

            context.RetrievedDocuments = searchResults;

            // Format context for prompt injection
            if (searchResults.Any())
            {
                context.FormatForPrompt();

                _logger.LogInformation(
                    "Retrieved {Count} relevant documents for query (avg score: {AvgScore:P1})",
                    searchResults.Count,
                    searchResults.Average(r => r.Score));

                context.Metadata["retrieved_count"] = searchResults.Count;
                context.Metadata["avg_relevance"] = searchResults.Average(r => r.Score);
                context.Metadata["max_relevance"] = searchResults.Max(r => r.Score);
            }
            else
            {
                _logger.LogDebug("No relevant documents found for query: {Query}", query);
                context.Metadata["retrieved_count"] = 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to augment query: {Query}", query);
            context.Metadata["error"] = ex.Message;
        }

        return context;
    }

    public string BuildAugmentedPrompt(
        string systemPrompt,
        string userQuery,
        RagContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(systemPrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(userQuery);
        ArgumentNullException.ThrowIfNull(context);

        if (!context.RetrievedDocuments.Any())
        {
            // No context available, return original prompt
            return $"{systemPrompt}\n\nUser: {userQuery}";
        }

        // Build augmented prompt with context
        var augmentedPrompt = $@"{systemPrompt}

# Instructions for Using Knowledge Base Context

You have been provided with relevant information from the knowledge base below. Use this information to:
1. Provide accurate, factually grounded responses
2. Cite your sources when using information from the knowledge base
3. Clearly indicate if information is not in the knowledge base

{context.FormattedContext}

# Important Guidelines

- Stay in character while using the knowledge base information
- If the knowledge base doesn't contain relevant information, acknowledge this and respond based on your character
- When using knowledge base information, mention the source naturally in your response
- Do not invent facts not present in the provided context

---

User: {userQuery}";

        var citations = context.GetCitations();
        if (!string.IsNullOrWhiteSpace(citations))
        {
            augmentedPrompt += $"\n\n[Available Sources: {citations}]";
        }

        return augmentedPrompt;
    }
}
