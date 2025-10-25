using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Core.Models;
using LablabBean.Contracts.AI.Memory;

namespace LablabBean.AI.Agents;

/// <summary>
/// Employee Intelligence Agent - integrates Semantic Kernel for decision-making and dialogue
/// </summary>
public sealed class EmployeeIntelligenceAgent : IIntelligenceAgent
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly EmployeePersonality _personality;
    private readonly ILogger<EmployeeIntelligenceAgent> _logger;
    private readonly ChatHistory _chatHistory;
    private readonly IMemoryService? _memoryService;
    private readonly IPromptAugmentationService? _promptAugmentation;

    public string AgentId { get; }
    public string AgentType => "Employee";
    public bool HasKnowledgeBase => _promptAugmentation != null;

    public EmployeeIntelligenceAgent(
        Kernel kernel,
        EmployeePersonalityLoader personalityLoader,
        ILogger<EmployeeIntelligenceAgent> logger,
        string agentId,
        IMemoryService? memoryService = null,
        IPromptAugmentationService? promptAugmentation = null)
    {
        _kernel = kernel;
        _personality = personalityLoader.CreateDefault();
        _logger = logger;
        AgentId = agentId;
        _memoryService = memoryService;
        _promptAugmentation = promptAugmentation;

        _chatService = kernel.GetRequiredService<IChatCompletionService>();
        _chatHistory = new ChatHistory(_personality.Prompts.SystemPrompt);

        _logger.LogInformation($"EmployeeIntelligenceAgent initialized: {agentId} (SemanticMemory: {_memoryService != null}, KnowledgeBase: {HasKnowledgeBase})");
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation($"EmployeeIntelligenceAgent initialization complete: {AgentId}");
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

            // Build decision prompt with semantic memory retrieval (T024)
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

            // Check if this is a question that could benefit from knowledge base
            var useKnowledgeBase = ShouldUseKnowledgeBase(context);

            // Build dialogue prompt (with or without RAG)
            string prompt;
            if (useKnowledgeBase && _promptAugmentation != null)
            {
                prompt = await BuildDialoguePromptWithRagAsync(context);
            }
            else
            {
                prompt = BuildDialoguePrompt(context);
            }

            // Get AI response
            _chatHistory.AddUserMessage(prompt);
            var response = await _chatService.GetChatMessageContentAsync(
                _chatHistory,
                kernel: _kernel
            );

            var dialogue = response.Content ?? "...";

            // Add to history
            _chatHistory.AddAssistantMessage(dialogue);

            // Trim history if too long
            TrimChatHistory();

            _logger.LogInformation($"Dialogue generated: {dialogue[..Math.Min(50, dialogue.Length)]}... (RAG: {useKnowledgeBase})");

            return dialogue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dialogue");
            return "...";
        }
    }

    public async Task UpdateMemoryAsync(AvatarMemory memory)
    {
        // T026: Dual-write to both legacy and new memory systems
        _logger.LogDebug($"Memory update received for {AgentId}");

        if (_memoryService != null && memory.ShortTermMemory.Any())
        {
            try
            {
                // Store most recent memories to semantic memory
                var recentMemories = memory.ShortTermMemory.Take(3);

                foreach (var legacyMemory in recentMemories)
                {
                    // Convert legacy memory to semantic memory format
                    var semanticMemory = new Contracts.AI.Memory.MemoryEntry
                    {
                        Id = $"{memory.EntityId}_{legacyMemory.Timestamp:yyyyMMddHHmmss}",
                        Content = $"{legacyMemory.EventType}: {legacyMemory.Description}",
                        EntityId = memory.EntityId,
                        MemoryType = legacyMemory.EventType,
                        Importance = legacyMemory.Importance,
                        Timestamp = new DateTimeOffset(legacyMemory.Timestamp),
                        Tags = new Dictionary<string, string>
                        {
                            { "agent_type", AgentType }
                        }
                    };

                    await _memoryService.StoreMemoryAsync(semanticMemory);
                    _logger.LogDebug($"Dual-wrote memory {semanticMemory.Id} to semantic memory");
                }
            }
            catch (Exception ex)
            {
                // T028: Fallback - log error but don't fail
                _logger.LogWarning(ex, "Failed to dual-write memory to semantic store, continuing with legacy memory only");
            }
        }
    }

    public async Task ProcessFeedbackAsync(string feedback, float sentiment)
    {
        try
        {
            _logger.LogDebug($"Processing feedback: {feedback} (sentiment: {sentiment})");

            // Add feedback to chat history for context
            _chatHistory.AddSystemMessage($"[FEEDBACK] Sentiment: {sentiment:F2} - {feedback}");

            // Trim history if needed
            TrimChatHistory();

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing feedback");
        }
    }

    // Private helper methods

    private async Task<string> BuildDecisionPromptWithSemanticMemoryAsync(
        AvatarContext context,
        AvatarState state,
        AvatarMemory memory)
    {
        string recentMemories;

        // T024: Use semantic retrieval if available, fallback to legacy (T028)
        if (_memoryService != null)
        {
            try
            {
                // Build query from current context
                var query = $"{state.CurrentBehavior} {context.Name}";

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
                    recentMemories = string.Join("\n",
                        memory.ShortTermMemory.TakeLast(5).Select(m => $"- {m.Description}"));
                    _logger.LogDebug("No semantic memories found, using legacy memory");
                }
            }
            catch (Exception ex)
            {
                // T028: Fallback to legacy on error
                _logger.LogWarning(ex, "Semantic memory retrieval failed, falling back to legacy memory");
                recentMemories = string.Join("\n",
                    memory.ShortTermMemory.TakeLast(5).Select(m => $"- {m.Description}"));
            }
        }
        else
        {
            // No semantic memory service, use legacy
            recentMemories = string.Join("\n",
                memory.ShortTermMemory.TakeLast(5).Select(m => $"- {m.Description}"));
        }

        return BuildDecisionPrompt(context, state, recentMemories);
    }

    private string BuildDecisionPrompt(AvatarContext context, AvatarState state, string recentMemories)
    {
        var emotionalState = state.EmotionalState ?? "neutral";
        var energy = state.Stats.GetValueOrDefault("energy", 1.0f);
        var stress = state.Stats.GetValueOrDefault("stress", 0.0f);
        var motivation = state.Stats.GetValueOrDefault("motivation", 0.7f);

        var prompt = _personality.Prompts.DecisionTemplate
            .Replace("{context}", state.CurrentBehavior)
            .Replace("{situation}", context.Name)
            .Replace("{state}", $"Energy: {energy:F1}, Stress: {stress:F1}, Motivation: {motivation:F1}, Emotion: {emotionalState}")
            .Replace("{memory}", recentMemories)
            .Replace("{entity_id}", context.EntityId)
            .Replace("{timestamp}", DateTime.UtcNow.ToString("HH:mm"));

        // Add personality traits context
        prompt += $"\n\nYour personality traits (0-1 scale):";
        prompt += $"\n- Diligence: {_personality.Traits.Diligence:F2}";
        prompt += $"\n- Friendliness: {_personality.Traits.Friendliness:F2}";
        prompt += $"\n- Adaptability: {_personality.Traits.Adaptability:F2}";
        prompt += $"\n- Creativity: {_personality.Traits.Creativity:F2}";
        prompt += $"\n- Teamwork: {_personality.Traits.Teamwork:F2}";
        prompt += $"\n- AttentionToDetail: {_personality.Traits.AttentionToDetail:F2}";
        prompt += $"\n- Enthusiasm: {_personality.Traits.Enthusiasm:F2}";
        prompt += $"\n- Resilience: {_personality.Traits.Resilience:F2}";

        prompt += $"\n\nProvide: DECISION_TYPE | ACTION | REASONING | CONFIDENCE (0-1)";

        return prompt;
    }

    private string BuildDialoguePrompt(DialogueContext context)
    {
        var relationshipLevel = context.ContextVariables.GetValueOrDefault("relationship_level", 0.5f);
        var listenerRole = context.ContextVariables.GetValueOrDefault("listener_role", "unknown")?.ToString() ?? "unknown";

        var prompt = _personality.Prompts.DialogueTemplate
            .Replace("{listener_name}", context.ListenerId)
            .Replace("{listener_type}", listenerRole)
            .Replace("{relationship_level}", relationshipLevel.ToString())
            .Replace("{context}", context.ConversationTopic)
            .Replace("{emotion}", context.SpeakerEmotionalState ?? "neutral");

        // Add current emotional state
        prompt += $"\n\nYour current state:";
        prompt += $"\n- Emotion: {context.SpeakerEmotionalState ?? "neutral"}";
        prompt += $"\n- Relationship: {relationshipLevel:F2}";

        // Add personality context for dialogue style
        prompt += $"\n\nDialogue style (0-1 scale):";
        prompt += $"\n- Friendliness: {_personality.Traits.Friendliness:F2}";
        prompt += $"\n- Enthusiasm: {_personality.Traits.Enthusiasm:F2}";
        prompt += $"\n- Formality: {_personality.Dialogue.Formality:F2}";
        prompt += $"\n- Verbosity: {_personality.Dialogue.Verbosity:F2}";

        prompt += $"\n\nRespond naturally in 1-2 sentences.";

        return prompt;
    }

    private AIDecision ParseDecisionResponse(string response, AvatarContext context, AvatarState state)
    {
        try
        {
            // Parse structured response: DECISION_TYPE | ACTION | REASONING | CONFIDENCE
            var parts = response.Split('|').Select(p => p.Trim()).ToArray();

            if (parts.Length >= 4)
            {
                return new AIDecision
                {
                    DecisionType = parts[0],
                    Action = parts[1],
                    Reasoning = parts[2],
                    Confidence = float.TryParse(parts[3], out var conf) ? conf : 0.5f,
                    Parameters = new Dictionary<string, object>
                    {
                        ["emotion"] = state.EmotionalState ?? "neutral"
                    },
                    Timestamp = DateTime.UtcNow
                };
            }

            // Fallback: parse unstructured response
            return new AIDecision
            {
                DecisionType = "task_decision",
                Action = ExtractAction(response),
                Reasoning = response,
                Confidence = 0.6f,
                Parameters = new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing decision response, using fallback");

            return new AIDecision
            {
                DecisionType = "fallback",
                Action = "continue_current_task",
                Reasoning = "Could not parse AI response",
                Confidence = 0.3f,
                Parameters = new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private string ExtractAction(string response)
    {
        // Simple keyword extraction
        var lowerResponse = response.ToLower();

        if (lowerResponse.Contains("take break") || lowerResponse.Contains("rest"))
            return "take_break";
        if (lowerResponse.Contains("help") || lowerResponse.Contains("assist"))
            return "help_colleague";
        if (lowerResponse.Contains("talk") || lowerResponse.Contains("speak"))
            return "interact_with_boss";
        if (lowerResponse.Contains("continue") || lowerResponse.Contains("keep"))
            return "continue_current_task";
        if (lowerResponse.Contains("focus") || lowerResponse.Contains("concentrate"))
            return "improve_task_quality";

        return "continue_current_task";
    }

    private void TrimChatHistory()
    {
        // Keep system prompt + last 20 messages
        const int maxMessages = 21;

        if (_chatHistory.Count > maxMessages)
        {
            var systemPrompt = _chatHistory.First();
            var recentMessages = _chatHistory.Skip(_chatHistory.Count - (maxMessages - 1)).ToList();

            _chatHistory.Clear();
            _chatHistory.Add(systemPrompt);

            foreach (var message in recentMessages)
            {
                _chatHistory.Add(message);
            }

            _logger.LogDebug($"Chat history trimmed to {_chatHistory.Count} messages");
        }
    }

    // Specialized employee decision methods

    public async Task<AIDecision> DecideTaskPriorityAsync(
        IEnumerable<string> availableTasks,
        AvatarState state,
        Dictionary<string, float> taskUrgencies)
    {
        try
        {
            var taskList = string.Join("\n", availableTasks.Select((t, i) =>
                $"{i + 1}. {t} (urgency: {(taskUrgencies.ContainsKey(t) ? taskUrgencies[t].ToString("F2") : "unknown")})"));

            var energy = state.Stats.GetValueOrDefault("energy", 1.0f);
            var stress = state.Stats.GetValueOrDefault("stress", 0.0f);
            var motivation = state.Stats.GetValueOrDefault("motivation", 0.7f);

            var prompt = $@"You are an employee deciding which task to prioritize.

Current state:
- Energy: {energy:F2}
- Stress: {stress:F2}
- Motivation: {motivation:F2}

Available tasks:
{taskList}

Your traits:
- Diligence: {_personality.Traits.Diligence:F2}
- Initiative: {_personality.Behavior.InitiativeLevel:F2}
- Task Speed: {_personality.Behavior.TaskCompletionSpeed:F2}

Choose the most appropriate task and explain why.
Format: TASK_PRIORITY | [task_name] | [reasoning] | [confidence]";

            _chatHistory.AddUserMessage(prompt);
            var response = await _chatService.GetChatMessageContentAsync(_chatHistory, kernel: _kernel);
            _chatHistory.AddAssistantMessage(response.Content ?? string.Empty);

            TrimChatHistory();

            var parts = (response.Content ?? "").Split('|').Select(p => p.Trim()).ToArray();

            return new AIDecision
            {
                DecisionType = "task_priority",
                Action = parts.Length > 1 ? parts[1] : availableTasks.FirstOrDefault() ?? "none",
                Reasoning = parts.Length > 2 ? parts[2] : "No specific reasoning",
                Confidence = parts.Length > 3 && float.TryParse(parts[3], out var conf) ? conf : 0.5f,
                Parameters = new Dictionary<string, object> { ["available_tasks"] = availableTasks.ToList() },
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deciding task priority");
            return new AIDecision
            {
                DecisionType = "task_priority",
                Action = availableTasks.FirstOrDefault() ?? "none",
                Reasoning = "Fallback to first available task",
                Confidence = 0.3f,
                Parameters = new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<string> RespondToCustomerAsync(string customerRequest, float relationshipLevel, AvatarState state)
    {
        try
        {
            var stress = state.Stats.GetValueOrDefault("stress", 0.0f);
            var energy = state.Stats.GetValueOrDefault("energy", 1.0f);

            var prompt = $@"Customer: ""{customerRequest}""

Your state:
- Stress: {stress:F2}
- Energy: {energy:F2}
- Friendliness: {_personality.Traits.Friendliness:F2}
- Customer Service Skill: {_personality.Skills.CustomerService:F2}

Relationship with customer: {relationshipLevel:F2}

Respond warmly and professionally in 1-2 sentences.";

            _chatHistory.AddUserMessage(prompt);
            var response = await _chatService.GetChatMessageContentAsync(_chatHistory, kernel: _kernel);
            _chatHistory.AddAssistantMessage(response.Content ?? string.Empty);

            TrimChatHistory();

            return response.Content ?? "How can I help you?";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to customer");
            return "How can I help you today?";
        }
    }

    /// <summary>
    /// Build dialogue prompt with RAG context from knowledge base
    /// </summary>
    private async Task<string> BuildDialoguePromptWithRagAsync(DialogueContext context)
    {
        if (_promptAugmentation == null)
        {
            return BuildDialoguePrompt(context);
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
                return BuildDialoguePrompt(context);
            }

            // Build augmented prompt
            var systemPrompt = _personality.Prompts.SystemPrompt + "\n\n" +
                "You have access to factual knowledge about the game world. " +
                "Use this information to provide helpful, accurate responses.";

            var userQuery = BuildDialoguePrompt(context);

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
            return BuildDialoguePrompt(context);
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
            "history", "ancient", "legend", "help", "need"
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
}
