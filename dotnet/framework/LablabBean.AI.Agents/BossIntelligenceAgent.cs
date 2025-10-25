using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Core.Models;
using LablabBean.AI.Core.Events;

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

    public string AgentId { get; }
    public string AgentType => "Boss";
    public bool HasTacticalCapability => _tacticsAgent != null;

    public BossIntelligenceAgent(
        Kernel kernel,
        BossPersonalityLoader personalityLoader,
        ILogger<BossIntelligenceAgent> logger,
        string agentId,
        TacticsAgent? tacticsAgent = null)
    {
        _kernel = kernel;
        _personality = personalityLoader.CreateDefault();
        _logger = logger;
        AgentId = agentId;
        _tacticsAgent = tacticsAgent;

        _chatService = kernel.GetRequiredService<IChatCompletionService>();
        _chatHistory = new ChatHistory(_personality.Prompts.SystemPrompt);

        _logger.LogInformation($"BossIntelligenceAgent initialized: {agentId} (Tactical: {HasTacticalCapability})");
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

            // Build decision prompt
            var prompt = BuildDecisionPrompt(context, state, memory);

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

            // Build dialogue prompt
            var prompt = BuildDialoguePrompt(context);

            // Create temporary chat history for dialogue
            var dialogueHistory = new ChatHistory(_personality.Prompts.SystemPrompt);
            dialogueHistory.AddUserMessage(prompt);

            // Get AI response
            var response = await _chatService.GetChatMessageContentAsync(
                dialogueHistory,
                kernel: _kernel
            );

            var dialogue = response.Content ?? "...";

            _logger.LogInformation($"Dialogue generated for {context.ListenerId}");

            return dialogue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dialogue");
            return "I need a moment to think...";
        }
    }

    // Additional helper methods for extended functionality
    public async Task<MemoryEntry> ProcessMemoryAsync(
        string content,
        float emotionalIntensity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug($"Processing memory: {content[..Math.Min(50, content.Length)]}...");

            // TODO: Use Semantic Kernel for memory embeddings and semantic search
            // For MVP, create simple memory entry
            var memory = new MemoryEntry
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

    private string BuildDecisionPrompt(AvatarContext context, AvatarState state, AvatarMemory memory)
    {
        var template = _personality.Prompts.DecisionTemplate;

        // Extract values from CurrentState dictionary
        var stateInfo = $"Emotion: {state.EmotionalState}, " +
                       $"Behavior: {state.CurrentBehavior}, " +
                       $"Health: {state.HealthPercentage:P0}";

        var recentMemories = string.Join("\n", memory.GetRecentMemories(5).Select(m => $"- {m.Description}"));

        var environment = string.Join("\n", context.EnvironmentFactors.Select(kv =>
            $"- {kv.Key}: {kv.Value}"));

        return template
            .Replace("{context}", environment)
            .Replace("{state}", stateInfo)
            .Replace("{memory}", recentMemories);
    }

    private string BuildDialoguePrompt(DialogueContext context)
    {
        var template = _personality.Prompts.DialogueTemplate;

        var conversationHistory = string.Join("\n", context.ConversationHistory);

        // Calculate relationship level from context if available
        var relationshipLevel = context.ContextVariables.TryGetValue("relationship_level", out var rel)
            ? rel.ToString() ?? "0.5"
            : "0.5";

        return template
            .Replace("{employee_name}", context.ListenerId)
            .Replace("{relationship_level}", relationshipLevel)
            .Replace("{context}", context.ConversationTopic)
            .Replace("{current_mood}", context.SpeakerEmotionalState);
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

    public void Dispose()
    {
        _logger.LogInformation($"BossIntelligenceAgent disposed: {AgentId}");
    }
}
