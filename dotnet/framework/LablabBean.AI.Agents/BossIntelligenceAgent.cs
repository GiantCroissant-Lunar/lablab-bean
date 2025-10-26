using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Core.Models;
using LablabBean.AI.Core.Events;
using LablabBean.Contracts.AI.Memory;

namespace LablabBean.AI.Agents;

/// <summary>
/// Boss Intelligence Agent - integrates Semantic Kernel for decision-making and dialogue
/// </summary>
public sealed class BossIntelligenceAgent : IIntelligenceAgent
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly BossPersonality _personality;
    private readonly ILogger<BossIntelligenceAgent> _logger;
    private readonly ChatHistory _chatHistory;
    private readonly TacticsAgent? _tacticsAgent;
    private readonly IMemoryService? _memoryService;
    private readonly IPromptAugmentationService? _promptAugmentation;

    public string AgentId { get; }
    public string AgentType => "Boss";
    public bool HasTacticalCapability => _tacticsAgent != null;
    public bool HasKnowledgeBase => _promptAugmentation != null;

    public BossIntelligenceAgent(
        Kernel kernel,
        BossPersonalityLoader personalityLoader,
        ILogger<BossIntelligenceAgent> logger,
        string agentId,
        TacticsAgent? tacticsAgent = null,
        IMemoryService? memoryService = null,
        IPromptAugmentationService? promptAugmentation = null)
    {
        _kernel = kernel;
        _personality = personalityLoader.CreateDefault();
        _logger = logger;
        AgentId = agentId;
        _tacticsAgent = tacticsAgent;
        _memoryService = memoryService;
        _promptAugmentation = promptAugmentation;

        _chatService = kernel.GetRequiredService<IChatCompletionService>();
        _chatHistory = new ChatHistory(_personality.Prompts.SystemPrompt);

        _logger.LogInformation($"BossIntelligenceAgent initialized: {agentId} (Tactical: {HasTacticalCapability}, SemanticMemory: {_memoryService != null}, KnowledgeBase: {HasKnowledgeBase})");
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation($"BossIntelligenceAgent initialization complete: {AgentId}");
        await Task.CompletedTask;
    }

    public async Task<AIDecision> GetDecisionAsync(
        AvatarContext context,
        AvatarState state,
        AvatarMemory memory)
    {
        try
        {
            _logger.LogDebug($"Making decision for context: {context.EntityId}");

            // Build decision prompt with semantic memory retrieval (T025)
            var prompt = await BuildDecisionPromptWithSemanticMemoryAsync(context, state, memory);

            // Get AI response
            _chatHistory.AddUserMessage(prompt);
            var response = await _chatService.GetChatMessageContentAsync(
                _chatHistory,
                kernel: _kernel
            );

            var decision = ParseDecisionResponse(response.Content ?? string.Empty, context, state);

            // Add to history
            _chatHistory.AddAssistantMessage(response.Content ?? string.Empty);

            // Trim history if too long
            TrimChatHistory();

            _logger.LogInformation($"Decision made: {decision.Action} (confidence: {decision.Confidence})");

            return decision;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making decision");

            return new AIDecision
            {
                DecisionType = "error",
                Action = "Error occurred",
                Reasoning = ex.Message,
                Confidence = 0.0f,
                Parameters = new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<string> GenerateDialogueAsync(DialogueContext context)
    {
        try
        {
            _logger.LogDebug($"Generating dialogue for {context.ListenerId}");

            // T079: Retrieve relationship history before generating dialogue
            string? relationshipContext = null;
            if (_memoryService != null && !string.IsNullOrEmpty(context.ListenerId))
            {
                try
                {
                    var relationshipHistory = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
                        entity1Id: AgentId,
                        entity2Id: context.ListenerId,
                        query: context.ConversationTopic,
                        maxResults: 3
                    );

                    if (relationshipHistory.Any())
                    {
                        relationshipContext = FormatRelationshipContext(relationshipHistory);
                        _logger.LogInformation(
                            "Retrieved {Count} relationship memories with {Listener}. Relevance scores: {Scores}",
                            relationshipHistory.Count, context.ListenerId,
                            string.Join(", ", relationshipHistory.Select(h => $"{h.RelevanceScore:F2}")));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve relationship history, continuing without context");
                }
            }

            // Check if this is a question that could benefit from knowledge base
            var useKnowledgeBase = ShouldUseKnowledgeBase(context);

            // Build dialogue prompt (with or without RAG and relationship context)
            string prompt;
            if (useKnowledgeBase && _promptAugmentation != null)
            {
                prompt = await BuildDialoguePromptWithRagAsync(context, relationshipContext);
            }
            else
            {
                prompt = BuildDialoguePrompt(context, relationshipContext);
            }

            // Create temporary chat history for dialogue
            var dialogueHistory = new ChatHistory(_personality.Prompts.SystemPrompt);
            dialogueHistory.AddUserMessage(prompt);

            // Get AI response
            var response = await _chatService.GetChatMessageContentAsync(
                dialogueHistory,
                kernel: _kernel
            );

            var dialogue = response.Content ?? "...";

            _logger.LogInformation($"Dialogue generated for {context.ListenerId} (RAG: {useKnowledgeBase})");

            return dialogue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dialogue");
            return "I need a moment to think...";
        }
    }

    /// <summary>
    /// Additional helper methods for extended functionality
    /// </summary>
    public async Task<Core.Models.MemoryEntry> ProcessMemoryAsync(
        string content,
        float emotionalIntensity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug($"Processing memory: {content[..Math.Min(50, content.Length)]}...");

            // Create memory entry
            var memory = new Core.Models.MemoryEntry
            {
                EventType = "processed_memory",
                Description = content,
                Timestamp = DateTime.UtcNow,
                Importance = emotionalIntensity,
                Metadata = new Dictionary<string, object>
                {
                    ["processed"] = true,
                    ["agent_id"] = AgentId
                }
            };

            // T027: Dual-write to semantic memory if available
            if (_memoryService != null)
            {
                try
                {
                    var semanticMemory = new Contracts.AI.Memory.MemoryEntry
                    {
                        Id = $"{AgentId}_{DateTime.UtcNow:yyyyMMddHHmmss}",
                        Content = content,
                        EntityId = AgentId,
                        MemoryType = "processed_memory",
                        Importance = emotionalIntensity,
                        Timestamp = DateTimeOffset.UtcNow,
                        Tags = new Dictionary<string, string>
                        {
                            { "agent_type", AgentType }
                        }
                    };

                    await _memoryService.StoreMemoryAsync(semanticMemory, cancellationToken);
                    _logger.LogDebug("Dual-wrote processed memory to semantic memory");
                }
                catch (Exception ex)
                {
                    // T028: Fallback - log error but don't fail
                    _logger.LogWarning(ex, "Failed to dual-write processed memory to semantic store");
                }
            }

            _logger.LogInformation("Memory processed successfully");
            return await Task.FromResult(memory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing memory");
            throw;
        }
    }

    public async Task<float> EvaluateRelationshipAsync(
        AvatarRelationship relationship,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug($"Evaluating relationship with {relationship.TargetEntityId}");

            // Calculate relationship score based on personality traits
            var trustWeight = _personality.Relationships.TrustBuildRate;
            var authorityWeight = _personality.Relationships.AuthorityImportance;

            // Convert affinity (-100 to +100) to normalized score (0 to 1)
            var affinityScore = (relationship.Affinity + 100f) / 200f;

            // Factor in interaction history
            var historyBonus = Math.Min(relationship.SharedHistory.Count * 0.02f, 0.2f);

            var score = Math.Clamp(affinityScore + historyBonus, 0f, 1f);

            _logger.LogInformation($"Relationship evaluated: {score:F2}");

            return await Task.FromResult(score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating relationship");
            return 0.5f;
        }
    }

    /// <summary>
    /// Track player behavior for tactical learning
    /// </summary>
    public void TrackPlayerBehavior(string playerId, PlayerBehaviorType behaviorType, float intensity)
    {
        _tacticsAgent?.TrackPlayerBehavior(playerId, behaviorType, intensity);
        _logger.LogDebug($"Tracked player behavior: {behaviorType} (intensity: {intensity:F2})");
    }

    /// <summary>
    /// Generate tactical plan based on observed player behavior
    /// </summary>
    public async Task<TacticalPlan?> CreateTacticalPlanAsync(
        AvatarContext context,
        string playerId,
        CancellationToken cancellationToken = default)
    {
        if (_tacticsAgent == null)
        {
            _logger.LogWarning("Tactical planning requested but TacticsAgent not available");
            return null;
        }

        try
        {
            var plan = await _tacticsAgent.CreateTacticalPlanAsync(context, playerId, cancellationToken);
            _logger.LogInformation(
                $"Tactical plan created: {plan.PrimaryTactic} (confidence: {plan.Confidence:F2})");
            return plan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tactical plan");
            return null;
        }
    }

    #region Helper Methods

    private async Task<string> BuildDecisionPromptWithSemanticMemoryAsync(
        AvatarContext context,
        AvatarState state,
        AvatarMemory memory)
    {
        string recentMemories;

        // T025: Use semantic retrieval if available, fallback to legacy (T028)
        if (_memoryService != null)
        {
            try
            {
                // Build query from current context
                var query = $"{state.CurrentBehavior} {string.Join(" ", context.EnvironmentFactors.Values)}";

                var options = new MemoryRetrievalOptions
                {
                    EntityId = context.EntityId,
                    Limit = 5,
                    MinRelevanceScore = 0.7,
                    MinImportance = 0.3
                };

                var semanticMemories = await _memoryService.RetrieveRelevantMemoriesAsync(query, options);

                if (semanticMemories.Any())
                {
                    recentMemories = string.Join("\n",
                        semanticMemories.Select(m => $"- {m.Memory.Content} (relevance: {m.RelevanceScore:F2})"));
                    _logger.LogDebug($"Using {semanticMemories.Count} semantic memories for decision");
                }
                else
                {
                    // Fallback to legacy memory
                    recentMemories = string.Join("\n", memory.GetRecentMemories(5).Select(m => $"- {m.Description}"));
                    _logger.LogDebug("No semantic memories found, using legacy memory");
                }
            }
            catch (Exception ex)
            {
                // T028: Fallback to legacy on error
                _logger.LogWarning(ex, "Semantic memory retrieval failed, falling back to legacy memory");
                recentMemories = string.Join("\n", memory.GetRecentMemories(5).Select(m => $"- {m.Description}"));
            }
        }
        else
        {
            // No semantic memory service, use legacy
            recentMemories = string.Join("\n", memory.GetRecentMemories(5).Select(m => $"- {m.Description}"));
        }

        return BuildDecisionPrompt(context, state, recentMemories);
    }

    private string BuildDecisionPrompt(AvatarContext context, AvatarState state, string recentMemories)
    {
        var template = _personality.Prompts.DecisionTemplate;

        // Extract values from CurrentState dictionary
        var stateInfo = $"Emotion: {state.EmotionalState}, " +
                       $"Behavior: {state.CurrentBehavior}, " +
                       $"Health: {state.HealthPercentage:P0}";

        var environment = string.Join("\n", context.EnvironmentFactors.Select(kv =>
            $"- {kv.Key}: {kv.Value}"));

        return template
            .Replace("{context}", environment)
            .Replace("{state}", stateInfo)
            .Replace("{memory}", recentMemories);
    }

    private string BuildDialoguePrompt(DialogueContext context, string? relationshipContext = null)
    {
        var template = _personality.Prompts.DialogueTemplate;

        var conversationHistory = string.Join("\n", context.ConversationHistory);

        // Calculate relationship level from context if available
        var relationshipLevel = context.ContextVariables.TryGetValue("relationship_level", out var rel)
            ? rel.ToString() ?? "0.5"
            : "0.5";

        var prompt = template
            .Replace("{employee_name}", context.ListenerId)
            .Replace("{relationship_level}", relationshipLevel)
            .Replace("{context}", context.ConversationTopic)
            .Replace("{current_mood}", context.SpeakerEmotionalState);

        // T079: Add relationship history context if available
        if (!string.IsNullOrEmpty(relationshipContext))
        {
            prompt += $"\n\nRelationship History:\n{relationshipContext}";
        }

        return prompt;
    }

    /// <summary>
    /// Build dialogue prompt with RAG context from knowledge base
    /// </summary>
    private async Task<string> BuildDialoguePromptWithRagAsync(DialogueContext context, string? relationshipContext = null)
    {
        if (_promptAugmentation == null)
        {
            return BuildDialoguePrompt(context, relationshipContext);
        }

        try
        {
            _logger.LogDebug("Augmenting dialogue with knowledge base context");

            // Get RAG context for the conversation topic
            var ragContext = await _promptAugmentation.AugmentQueryAsync(
                query: context.ConversationTopic,
                topK: 3,
                category: DetermineCategory(context.ConversationTopic));

            if (!ragContext.RetrievedDocuments.Any())
            {
                _logger.LogDebug("No relevant knowledge base documents found, using standard prompt");
                return BuildDialoguePrompt(context, relationshipContext);
            }

            // Build augmented prompt
            var systemPrompt = _personality.Prompts.SystemPrompt + "\n\n" +
                "You have access to factual knowledge about the game world. " +
                "Use this information to provide accurate, grounded responses.";

            var userQuery = BuildDialoguePrompt(context, relationshipContext);

            var augmentedPrompt = _promptAugmentation.BuildAugmentedPrompt(
                systemPrompt,
                userQuery,
                ragContext);

            _logger.LogInformation($"Augmented prompt with {ragContext.RetrievedDocuments.Count} knowledge base documents");

            return augmentedPrompt;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to augment prompt with knowledge base, falling back to standard prompt");
            return BuildDialoguePrompt(context, relationshipContext);
        }
    }

    /// <summary>
    /// Determine if the dialogue should use knowledge base
    /// </summary>
    private bool ShouldUseKnowledgeBase(DialogueContext context)
    {
        if (_promptAugmentation == null)
            return false;

        var topic = context.ConversationTopic.ToLowerInvariant();

        // Question keywords that indicate the player is asking for information
        var questionKeywords = new[]
        {
            "what", "where", "who", "when", "why", "how",
            "tell me", "know about", "explain", "describe",
            "quest", "location", "dragon", "kingdom", "city",
            "history", "ancient", "legend"
        };

        return questionKeywords.Any(keyword => topic.Contains(keyword));
    }

    /// <summary>
    /// Determine knowledge base category from conversation topic
    /// </summary>
    private string? DetermineCategory(string topic)
    {
        var lowerTopic = topic.ToLowerInvariant();

        if (lowerTopic.Contains("quest") || lowerTopic.Contains("mission") || lowerTopic.Contains("contract"))
            return "quest";

        if (lowerTopic.Contains("city") || lowerTopic.Contains("location") || lowerTopic.Contains("place") ||
            lowerTopic.Contains("where"))
            return "location";

        if (lowerTopic.Contains("history") || lowerTopic.Contains("ancient") || lowerTopic.Contains("dragon") ||
            lowerTopic.Contains("legend") || lowerTopic.Contains("kingdom"))
            return "lore";

        return null; // Search all categories
    }

    private AIDecision ParseDecisionResponse(string response, AvatarContext context, AvatarState state)
    {
        // Simple parsing for MVP - extract decision and reasoning
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var decision = lines.FirstOrDefault() ?? "No decision";
        var reasoning = lines.Length > 1 ? string.Join(" ", lines.Skip(1)) : "No reasoning provided";

        // Calculate confidence based on response length and personality
        var confidence = Math.Min(0.9f, 0.5f + (lines.Length * 0.1f));
        confidence *= _personality.Behavior.DecisionSpeed; // Adjust by personality

        var aiDecision = new AIDecision
        {
            DecisionType = "boss_decision",
            Action = decision,
            Reasoning = reasoning,
            Confidence = confidence,
            Parameters = new Dictionary<string, object>
            {
                ["personality"] = _personality.Name,
                ["emotion"] = state.EmotionalState,
                ["health"] = state.HealthPercentage
            },
            Timestamp = DateTime.UtcNow
        };

        return aiDecision;
    }

    private void TrimChatHistory()
    {
        // Keep system message + last N messages
        const int maxMessages = 20;

        if (_chatHistory.Count > maxMessages + 1) // +1 for system message
        {
            var toRemove = _chatHistory.Count - maxMessages - 1;
            for (var i = 0; i < toRemove; i++)
            {
                _chatHistory.RemoveAt(1); // Skip system message at index 0
            }

            _logger.LogDebug($"Trimmed chat history to {_chatHistory.Count} messages");
        }
    }

    #endregion

    #region Relationship Memory (T078-T079)

    /// <summary>
    /// Format relationship history for inclusion in prompts (T079)
    /// </summary>
    private string FormatRelationshipContext(IReadOnlyList<MemoryResult> relationshipHistory)
    {
        var context = "Past interactions:\n";
        foreach (var memory in relationshipHistory.Take(3))
        {
            var tags = memory.Memory.Tags;
            var sentiment = tags.GetValueOrDefault("sentiment", "neutral");
            var interaction = tags.GetValueOrDefault("interaction", "interaction");
            var ageInDays = (DateTimeOffset.UtcNow - memory.Memory.Timestamp).TotalDays;

            context += $"- {interaction} ({sentiment}, {ageInDays:F0} days ago): {memory.Memory.Content}\n";
        }
        return context;
    }

    /// <summary>
    /// Store relationship memory after dialogue interaction (T078)
    /// </summary>
    public async Task StoreDialogueInteractionAsync(
        string listenerId,
        string conversationTopic,
        string dialogueGenerated,
        float relationshipLevel)
    {
        if (_memoryService == null || string.IsNullOrEmpty(listenerId))
            return;

        try
        {
            var interactionType = DetermineInteractionType(conversationTopic);
            var sentiment = DetermineSentiment(relationshipLevel, conversationTopic);

            var relationshipMemory = new RelationshipMemory
            {
                Entity1Id = AgentId,
                Entity2Id = listenerId,
                InteractionType = interactionType,
                Sentiment = sentiment,
                Description = $"Discussed '{conversationTopic}'. Boss said: \"{dialogueGenerated.Substring(0, Math.Min(100, dialogueGenerated.Length))}...\"",
                Timestamp = DateTimeOffset.UtcNow
            };

            await _memoryService.StoreRelationshipMemoryAsync(relationshipMemory);

            _logger.LogInformation(
                "Stored relationship memory: {AgentId} ↔ {Listener}, Type={Type}, Sentiment={Sentiment}",
                AgentId, listenerId, interactionType, sentiment);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to store relationship memory, continuing without storage");
        }
    }

    /// <summary>
    /// Determine interaction type from conversation topic
    /// </summary>
    private InteractionType DetermineInteractionType(string topic)
    {
        var lowerTopic = topic.ToLowerInvariant();

        if (lowerTopic.Contains("trade") || lowerTopic.Contains("buy") || lowerTopic.Contains("sell"))
            return InteractionType.Trade;

        if (lowerTopic.Contains("fight") || lowerTopic.Contains("combat") || lowerTopic.Contains("attack"))
            return InteractionType.Combat;

        if (lowerTopic.Contains("help") || lowerTopic.Contains("work together") || lowerTopic.Contains("collaborate"))
            return InteractionType.Collaboration;

        if (lowerTopic.Contains("gift") || lowerTopic.Contains("present") || lowerTopic.Contains("give"))
            return InteractionType.Gift;

        if (lowerTopic.Contains("quest") || lowerTopic.Contains("mission") || lowerTopic.Contains("task"))
            return InteractionType.Quest;

        if (lowerTopic.Contains("betray") || lowerTopic.Contains("lie") || lowerTopic.Contains("deceive"))
            return InteractionType.Betrayal;

        return InteractionType.Conversation;
    }

    /// <summary>
    /// Determine sentiment from relationship level and topic
    /// </summary>
    private string DetermineSentiment(float relationshipLevel, string topic)
    {
        var lowerTopic = topic.ToLowerInvariant();

        // Negative keywords override relationship level
        if (lowerTopic.Contains("angry") || lowerTopic.Contains("upset") ||
            lowerTopic.Contains("fight") || lowerTopic.Contains("betray"))
            return "negative";

        // Positive keywords boost sentiment
        if (lowerTopic.Contains("thank") || lowerTopic.Contains("appreciate") ||
            lowerTopic.Contains("grateful") || lowerTopic.Contains("help"))
            return "positive";

        // Otherwise use relationship level
        if (relationshipLevel >= 0.6f)
            return "positive";
        else if (relationshipLevel <= 0.4f)
            return "negative";
        else
            return "neutral";
    }

    #endregion

    public void Dispose()
    {
        _logger.LogInformation($"BossIntelligenceAgent disposed: {AgentId}");
    }
}
