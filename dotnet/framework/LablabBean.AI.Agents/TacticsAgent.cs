using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using LablabBean.AI.Core.Models;
using LablabBean.AI.Core.Events;
using LablabBean.Contracts.AI.Memory;

namespace LablabBean.AI.Agents;

/// <summary>
/// Tactical planning agent for combat scenarios.
/// Analyzes player behavior patterns and generates adaptive tactics.
/// Integrates with IMemoryService for persistent tactical learning across sessions.
/// </summary>
public class TacticsAgent
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly ILogger<TacticsAgent> _logger;
    private readonly IMemoryService? _memoryService;
    private readonly Dictionary<string, PlayerBehaviorProfile> _behaviorProfiles;

    public TacticsAgent(
        Kernel kernel,
        ILogger<TacticsAgent> logger,
        IMemoryService? memoryService = null)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memoryService = memoryService; // Optional for backward compatibility
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
        _behaviorProfiles = new Dictionary<string, PlayerBehaviorProfile>();
    }

    /// <summary>
    /// Track player behavior patterns for tactical adaptation
    /// </summary>
    public void TrackPlayerBehavior(string playerId, PlayerBehaviorType behaviorType, float intensity)
    {
        if (!_behaviorProfiles.ContainsKey(playerId))
        {
            _behaviorProfiles[playerId] = new PlayerBehaviorProfile(playerId);
        }

        var profile = _behaviorProfiles[playerId];
        profile.RecordBehavior(behaviorType, intensity);

        _logger.LogDebug(
            "Tracked player {PlayerId} behavior: {BehaviorType} (intensity: {Intensity:F2})",
            playerId, behaviorType, intensity);
    }

    /// <summary>
    /// Create tactical plan based on player behavior analysis
    /// Integrates with IMemoryService for learning from past encounters (T062-T065)
    /// </summary>
    public async Task<TacticalPlan> CreateTacticalPlanAsync(
        AvatarContext context,
        string playerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = _behaviorProfiles.GetValueOrDefault(playerId);

            // T063: Retrieve similar past encounters from memory service
            List<TacticalObservation>? pastObservations = null;
            if (_memoryService != null && profile != null)
            {
                var dominantBehavior = profile.GetDominantBehavior();
                pastObservations = await RetrievePastTacticalObservationsAsync(
                    context.EntityId,
                    dominantBehavior,
                    cancellationToken);

                _logger.LogInformation(
                    "Retrieved {Count} past tactical observations for entity {EntityName} against behavior {BehaviorType}",
                    pastObservations.Count, context.Name, dominantBehavior);
            }

            if (profile == null)
            {
                _logger.LogWarning("No behavior profile for player {PlayerId}, using default tactics", playerId);
                return CreateDefaultTacticalPlan();
            }

            // T064: Analyze past observations and select counter-tactics
            TacticalPlan plan;
            if (pastObservations != null && pastObservations.Count > 0)
            {
                plan = await CreateMemoryInformedTacticalPlanAsync(
                    context,
                    profile,
                    pastObservations,
                    cancellationToken);

                _logger.LogInformation(
                    "Created memory-informed tactical plan for {EntityName}: {Tactic} (confidence: {Confidence:F2}, based on {ObservationCount} past encounters)",
                    context.Name, plan.PrimaryTactic, plan.Confidence, pastObservations.Count);
            }
            else
            {
                plan = await CreateStandardTacticalPlanAsync(context, profile, cancellationToken);

                _logger.LogInformation(
                    "Created standard tactical plan for {EntityName}: {Tactic} (confidence: {Confidence:F2})",
                    context.Name, plan.PrimaryTactic, plan.Confidence);
            }

            // T062: Store tactical observation for future learning
            if (_memoryService != null && profile != null)
            {
                await StoreTacticalObservationAsync(
                    context.EntityId,
                    playerId,
                    profile.GetDominantBehavior(),
                    plan,
                    cancellationToken);
            }

            return plan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tactical plan, using fallback");
            return CreateDefaultTacticalPlan();
        }
    }

    /// <summary>
    /// Create coordinated tactics for multiple enemies
    /// </summary>
    public async Task<GroupTacticalPlan> CreateGroupTacticsPlanAsync(
        List<AvatarContext> enemyContexts,
        string playerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = _behaviorProfiles.GetValueOrDefault(playerId);
            if (profile == null || enemyContexts.Count < 2)
            {
                return new GroupTacticalPlan
                {
                    IndividualPlans = enemyContexts
                        .Select(ctx => (ctx.EntityId, CreateDefaultTacticalPlan()))
                        .ToDictionary(x => x.EntityId, x => x.Item2),
                    Coordination = CoordinationType.None
                };
            }

            var prompt = BuildGroupTacticsPrompt(enemyContexts, profile);
            var settings = new PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object>
                {
                    ["temperature"] = 0.8,
                    ["max_tokens"] = 800,
                    ["response_format"] = new { type = "json_object" }
                }
            };

            var response = await _chatService.GetChatMessageContentAsync(
                prompt,
                settings,
                _kernel,
                cancellationToken);

            var groupPlan = ParseGroupTacticsResponse(response.Content ?? "{}", enemyContexts);

            _logger.LogInformation(
                "Created group tactical plan: {Coordination} for {EnemyCount} enemies",
                groupPlan.Coordination, enemyContexts.Count);

            return groupPlan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create group tactics, using fallback");
            return new GroupTacticalPlan
            {
                IndividualPlans = enemyContexts
                    .Select(ctx => (ctx.EntityId, CreateDefaultTacticalPlan()))
                    .ToDictionary(x => x.EntityId, x => x.Item2),
                Coordination = CoordinationType.None
            };
        }
    }

    private string BuildTacticsPrompt(AvatarContext context, PlayerBehaviorProfile profile)
    {
        var dominantBehavior = profile.GetDominantBehavior();
        var behaviorAnalysis = profile.GetBehaviorSummary();

        // Extract health from context state with defaults
        var currentHealth = context.GetStateValue<int>("health");
        var maxHealth = context.GetStateValue<int>("maxHealth");
        int healthCurrent = (currentHealth is int h) ? h : 100;
        int healthMax = (maxHealth is int mh) ? mh : 100;
        var healthPercent = healthMax > 0 ? ((float)healthCurrent / (float)healthMax * 100f) : 100f;
        var location = context.GetStateValue<string>("location") ?? "Unknown";
        var currentState = context.GetStateValue<string>("state") ?? "Idle";

        return $$$"""
You are a tactical AI analyzing combat patterns to create adaptive enemy tactics.

## Enemy Status
- Name: {{{context.Name}}}
- Health: {{{healthCurrent}}}/{{{healthMax}}} ({{{healthPercent:F0}}}%)
- Location: {{{location}}}
- Current State: {{{currentState}}}

## Player Behavior Analysis
{{{behaviorAnalysis}}}

Dominant Behavior: {{{dominantBehavior}}}

## Task
Generate an adaptive tactical plan to counter the player's behavior patterns.

**Available Tactics**:
- CloseDistance: Rush toward player to counter ranged attacks
- CutOffEscape: Position to block retreat paths
- AggressivePressure: Constant attacks to prevent healing
- Flanking: Circle to attack from sides/behind
- FocusFire: Coordinate with allies to eliminate target quickly
- DefensiveRetreat: Fall back and regroup
- PatternBreak: Use unpredictable movements

**Response Format** (JSON):
{
  "primaryTactic": "CloseDistance | CutOffEscape | AggressivePressure | Flanking | FocusFire | DefensiveRetreat | PatternBreak",
  "secondaryTactic": "optional alternative tactic",
  "reasoning": "brief explanation of why this tactic counters player behavior",
  "targetPosition": "where enemy should position (front/back/left/right/current)",
  "aggression": 0.0-1.0,
  "caution": 0.0-1.0,
  "confidence": 0.0-1.0
}
""";
    }

    private string BuildGroupTacticsPrompt(List<AvatarContext> contexts, PlayerBehaviorProfile profile)
    {
        var enemyList = string.Join("\n", contexts.Select(ctx =>
        {
            var h = ctx.GetStateValue<int>("health");
            var mh = ctx.GetStateValue<int>("maxHealth");
            int health = (h is int hv) ? hv : 100;
            int maxHealth = (mh is int mhv) ? mhv : 100;
            string location = ctx.GetStateValue<string>("location") ?? "Unknown";
            return $"- {ctx.Name}: Health {health}/{maxHealth}, Location {location}";
        }));
        var behaviorAnalysis = profile.GetBehaviorSummary();

        return $$$"""
You are a tactical AI coordinating multiple enemies against a player.

## Enemy Group
{{{enemyList}}}

## Player Behavior Analysis
{{{behaviorAnalysis}}}

## Task
Create coordinated tactics for the enemy group to counter player behavior.

**Coordination Types**:
- None: Independent actions
- Flanking: Surround player from multiple sides
- FocusFire: All attack same target
- TagTeam: Take turns attacking while others recover
- Distraction: One enemy draws attention while others attack

**Response Format** (JSON):
{
  "coordination": "None | Flanking | FocusFire | TagTeam | Distraction",
  "individualTactics": {
    "entityId1": "tactic name",
    "entityId2": "tactic name"
  },
  "reasoning": "brief explanation",
  "priority": "which enemy takes lead role"
}
""";
    }

    private TacticalPlan ParseTacticalResponse(string jsonResponse)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            return new TacticalPlan
            {
                PrimaryTactic = Enum.TryParse<TacticType>(
                    root.GetProperty("primaryTactic").GetString(),
                    true,
                    out var primary) ? primary : TacticType.PatternBreak,
                SecondaryTactic = root.TryGetProperty("secondaryTactic", out var secondary) &&
                    Enum.TryParse<TacticType>(secondary.GetString(), true, out var sec) ? sec : null,
                Reasoning = root.GetProperty("reasoning").GetString() ?? "",
                TargetPosition = root.GetProperty("targetPosition").GetString() ?? "current",
                Aggression = (float)root.GetProperty("aggression").GetDouble(),
                Caution = (float)root.GetProperty("caution").GetDouble(),
                Confidence = (float)root.GetProperty("confidence").GetDouble()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse tactical response, using default");
            return CreateDefaultTacticalPlan();
        }
    }

    private GroupTacticalPlan ParseGroupTacticsResponse(string jsonResponse, List<AvatarContext> contexts)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var coordination = Enum.TryParse<CoordinationType>(
                root.GetProperty("coordination").GetString(),
                true,
                out var coord) ? coord : CoordinationType.None;

            var individualPlans = new Dictionary<string, TacticalPlan>();
            if (root.TryGetProperty("individualTactics", out var tactics))
            {
                foreach (var context in contexts)
                {
                    var entityIdStr = context.EntityId;
                    if (tactics.TryGetProperty(entityIdStr, out var tacticElement))
                    {
                        var tacticName = tacticElement.GetString();
                        if (Enum.TryParse<TacticType>(tacticName, true, out var tactic))
                        {
                            individualPlans[context.EntityId] = new TacticalPlan
                            {
                                PrimaryTactic = tactic,
                                Reasoning = root.GetProperty("reasoning").GetString() ?? "",
                                Confidence = 0.8f
                            };
                        }
                    }
                }
            }

            // Fill missing plans with defaults
            foreach (var context in contexts)
            {
                if (!individualPlans.ContainsKey(context.EntityId))
                {
                    individualPlans[context.EntityId] = CreateDefaultTacticalPlan();
                }
            }

            return new GroupTacticalPlan
            {
                Coordination = coordination,
                IndividualPlans = individualPlans,
                PriorityEntityId = root.TryGetProperty("priority", out var priority) ?
                    contexts.FirstOrDefault(c => c.Name.Contains(priority.GetString() ?? ""))?.EntityId ?? contexts[0].EntityId :
                    contexts[0].EntityId
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse group tactics, using default");
            return new GroupTacticalPlan
            {
                IndividualPlans = contexts
                    .Select(ctx => (ctx.EntityId, CreateDefaultTacticalPlan()))
                    .ToDictionary(x => x.EntityId, x => x.Item2),
                Coordination = CoordinationType.None
            };
        }
    }

    private TacticalPlan CreateDefaultTacticalPlan()
    {
        return new TacticalPlan
        {
            PrimaryTactic = TacticType.AggressivePressure,
            Reasoning = "Default aggressive tactic",
            TargetPosition = "front",
            Aggression = 0.7f,
            Caution = 0.3f,
            Confidence = 0.5f
        };
    }

    #region Tactical Memory Integration (T062-T066)

    /// <summary>
    /// Retrieve past tactical observations from memory service (T063)
    /// </summary>
    private async Task<List<TacticalObservation>> RetrievePastTacticalObservationsAsync(
        string entityId,
        PlayerBehaviorType behaviorType,
        CancellationToken cancellationToken)
    {
        try
        {
            var results = await _memoryService!.RetrieveSimilarTacticsAsync(
                entityId,
                behaviorType,
                limit: 10,
                cancellationToken);

            var observations = new List<TacticalObservation>();
            foreach (var result in results)
            {
                try
                {
                    var observation = JsonSerializer.Deserialize<TacticalObservation>(result.Memory.Content);
                    if (observation != null)
                    {
                        observations.Add(observation);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize tactical observation from memory {MemoryId}", result.Memory.Id);
                }
            }

            _logger.LogDebug(
                "Retrieved {Count} tactical observations for entity {EntityId} with behavior {BehaviorType}",
                observations.Count, entityId, behaviorType);

            return observations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve past tactical observations");
            return new List<TacticalObservation>();
        }
    }

    /// <summary>
    /// Create tactical plan informed by past observations (T064)
    /// </summary>
    private async Task<TacticalPlan> CreateMemoryInformedTacticalPlanAsync(
        AvatarContext context,
        PlayerBehaviorProfile profile,
        List<TacticalObservation> pastObservations,
        CancellationToken cancellationToken)
    {
        // T064: Aggregate tactic effectiveness from past observations
        var tacticEffectiveness = AggregateTacticEffectiveness(pastObservations);

        // T065: Analyze pattern frequency
        var patternAnalysis = AnalyzePatternFrequency(pastObservations);

        // Build enhanced prompt with historical data
        var prompt = BuildMemoryInformedTacticsPrompt(context, profile, tacticEffectiveness, patternAnalysis);

        var settings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = 0.6,  // Lower temperature for more consistent adaptation
                ["max_tokens"] = 600,
                ["response_format"] = new { type = "json_object" }
            }
        };

        var response = await _chatService.GetChatMessageContentAsync(
            prompt,
            settings,
            _kernel,
            cancellationToken);

        var plan = ParseTacticalResponse(response.Content ?? "{}");

        // Boost confidence based on data quality
        if (pastObservations.Count >= 5)
        {
            plan.Confidence = Math.Min(plan.Confidence * 1.2f, 1.0f);
        }

        return plan;
    }

    /// <summary>
    /// Create standard tactical plan without memory (fallback)
    /// </summary>
    private async Task<TacticalPlan> CreateStandardTacticalPlanAsync(
        AvatarContext context,
        PlayerBehaviorProfile profile,
        CancellationToken cancellationToken)
    {
        var prompt = BuildTacticsPrompt(context, profile);
        var settings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = 0.7,
                ["max_tokens"] = 500,
                ["response_format"] = new { type = "json_object" }
            }
        };

        var response = await _chatService.GetChatMessageContentAsync(
            prompt,
            settings,
            _kernel,
            cancellationToken);

        return ParseTacticalResponse(response.Content ?? "{}");
    }

    /// <summary>
    /// Aggregate tactic effectiveness from observations (T064)
    /// </summary>
    private Dictionary<string, float> AggregateTacticEffectiveness(List<TacticalObservation> observations)
    {
        var aggregated = new Dictionary<string, List<float>>();

        foreach (var observation in observations)
        {
            foreach (var kvp in observation.TacticEffectiveness)
            {
                if (!aggregated.ContainsKey(kvp.Key))
                {
                    aggregated[kvp.Key] = new List<float>();
                }
                aggregated[kvp.Key].Add(kvp.Value);
            }
        }

        // Calculate weighted average (more recent = higher weight)
        var result = new Dictionary<string, float>();
        foreach (var kvp in aggregated)
        {
            var values = kvp.Value;
            var weightedSum = 0f;
            var weightSum = 0f;

            for (int i = 0; i < values.Count; i++)
            {
                var weight = 1.0f + (i * 0.1f); // Recent observations weighted higher
                weightedSum += values[i] * weight;
                weightSum += weight;
            }

            result[kvp.Key] = weightSum > 0 ? weightedSum / weightSum : 0f;
        }

        _logger.LogDebug(
            "Aggregated tactic effectiveness from {Count} observations: {Tactics}",
            observations.Count,
            string.Join(", ", result.Select(kvp => $"{kvp.Key}={kvp.Value:F2}")));

        return result;
    }

    /// <summary>
    /// Analyze pattern frequency to identify dominant behaviors (T065)
    /// </summary>
    private string AnalyzePatternFrequency(List<TacticalObservation> observations)
    {
        if (observations.Count == 0)
            return "No pattern data available.";

        var behaviorCounts = new Dictionary<PlayerBehaviorType, int>();
        foreach (var observation in observations)
        {
            if (!behaviorCounts.ContainsKey(observation.BehaviorType))
            {
                behaviorCounts[observation.BehaviorType] = 0;
            }
            behaviorCounts[observation.BehaviorType]++;
        }

        var total = observations.Count;
        var sorted = behaviorCounts.OrderByDescending(kvp => kvp.Value).ToList();

        var analysis = $"Observed {total} encounters:\n";
        foreach (var kvp in sorted)
        {
            var percentage = (kvp.Value / (float)total) * 100f;
            var marker = percentage >= 80f ? " (DOMINANT)" : percentage >= 50f ? " (FREQUENT)" : "";
            analysis += $"  - {kvp.Key}: {kvp.Value} times ({percentage:F0}%){marker}\n";
        }

        _logger.LogDebug("Pattern analysis: {Analysis}", analysis.Replace("\n", " | "));

        return analysis;
    }

    /// <summary>
    /// Build tactics prompt with memory-informed insights
    /// </summary>
    private string BuildMemoryInformedTacticsPrompt(
        AvatarContext context,
        PlayerBehaviorProfile profile,
        Dictionary<string, float> tacticEffectiveness,
        string patternAnalysis)
    {
        var basePrompt = BuildTacticsPrompt(context, profile);

        var effectivenessData = string.Join("\n", tacticEffectiveness
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .Select(kvp => $"  - {kvp.Key}: {kvp.Value:F2} effectiveness"));

        var enhancedPrompt = basePrompt + $"\n\n" +
            $"## Historical Tactical Data (Past Encounters)\n\n" +
            $"{patternAnalysis}\n" +
            $"**Proven Tactic Effectiveness**:\n{effectivenessData}\n\n" +
            $"**Instructions**: Use this historical data to inform your tactical selection. " +
            $"Prioritize tactics with proven effectiveness (>0.7) against this behavior pattern. " +
            $"Avoid tactics with low effectiveness (<0.4).";

        return enhancedPrompt;
    }

    /// <summary>
    /// Store tactical observation after encounter (T062)
    /// </summary>
    private async Task StoreTacticalObservationAsync(
        string entityId,
        string playerId,
        PlayerBehaviorType behaviorType,
        TacticalPlan plan,
        CancellationToken cancellationToken)
    {
        try
        {
            // Build tactic effectiveness estimate based on plan confidence
            var tacticEffectiveness = new Dictionary<string, float>
            {
                [plan.PrimaryTactic.ToString()] = plan.Confidence
            };

            if (plan.SecondaryTactic.HasValue)
            {
                tacticEffectiveness[plan.SecondaryTactic.Value.ToString()] = plan.Confidence * 0.8f;
            }

            var observation = new TacticalObservation
            {
                PlayerId = playerId,
                BehaviorType = behaviorType,
                EncounterContext = plan.Reasoning,
                TacticEffectiveness = tacticEffectiveness,
                Outcome = plan.Confidence >= 0.7f ? OutcomeType.Success :
                         plan.Confidence >= 0.5f ? OutcomeType.PartialSuccess : OutcomeType.Neutral,
                Timestamp = DateTimeOffset.UtcNow
            };

            await _memoryService!.StoreTacticalObservationAsync(entityId, observation, cancellationToken);

            _logger.LogInformation(
                "Stored tactical observation for entity {EntityId}: Behavior {BehaviorType}, Tactic {Tactic}, Outcome {Outcome}",
                entityId, behaviorType, plan.PrimaryTactic, observation.Outcome);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store tactical observation for entity {EntityId}", entityId);
        }
    }

    #endregion
}

/// <summary>
/// Tracks player behavior patterns over time
/// </summary>
public class PlayerBehaviorProfile
{
    private readonly Dictionary<PlayerBehaviorType, float> _behaviorScores;
    private readonly List<BehaviorObservation> _recentObservations;
    private const int MaxObservations = 20;

    public string PlayerId { get; }

    public PlayerBehaviorProfile(string playerId)
    {
        PlayerId = playerId;
        _behaviorScores = new Dictionary<PlayerBehaviorType, float>();
        _recentObservations = new List<BehaviorObservation>();
    }

    public void RecordBehavior(PlayerBehaviorType type, float intensity)
    {
        _recentObservations.Add(new BehaviorObservation(type, intensity, DateTime.UtcNow));
        if (_recentObservations.Count > MaxObservations)
        {
            _recentObservations.RemoveAt(0);
        }

        // Update aggregate scores
        if (!_behaviorScores.ContainsKey(type))
        {
            _behaviorScores[type] = 0;
        }
        _behaviorScores[type] = (_behaviorScores[type] * 0.8f) + (intensity * 0.2f); // Exponential moving average
    }

    public PlayerBehaviorType GetDominantBehavior()
    {
        if (_behaviorScores.Count == 0)
            return PlayerBehaviorType.Unknown;

        return _behaviorScores.OrderByDescending(x => x.Value).First().Key;
    }

    public string GetBehaviorSummary()
    {
        if (_behaviorScores.Count == 0)
            return "No behavior data available.";

        var sortedBehaviors = _behaviorScores
            .OrderByDescending(x => x.Value)
            .Take(3)
            .Select(x => $"{x.Key}: {x.Value:F2}")
            .ToList();

        return string.Join(", ", sortedBehaviors);
    }

    private record BehaviorObservation(PlayerBehaviorType Type, float Intensity, DateTime Timestamp);
}

/// <summary>
/// Tactical plan for single enemy
/// </summary>
public class TacticalPlan
{
    public TacticType PrimaryTactic { get; set; }
    public TacticType? SecondaryTactic { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public string TargetPosition { get; set; } = "current";
    public float Aggression { get; set; } = 0.5f;
    public float Caution { get; set; } = 0.5f;
    public float Confidence { get; set; } = 0.5f;
}

/// <summary>
/// Group tactical plan for multiple enemies
/// </summary>
public class GroupTacticalPlan
{
    public CoordinationType Coordination { get; set; }
    public Dictionary<string, TacticalPlan> IndividualPlans { get; set; } = new();
    public string PriorityEntityId { get; set; } = string.Empty;
}

public enum TacticType
{
    CloseDistance,
    CutOffEscape,
    AggressivePressure,
    Flanking,
    FocusFire,
    DefensiveRetreat,
    PatternBreak
}

public enum CoordinationType
{
    None,
    Flanking,
    FocusFire,
    TagTeam,
    Distraction
}
