using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using LablabBean.AI.Core.Models;
using LablabBean.AI.Core.Events;

namespace LablabBean.AI.Agents;

/// <summary>
/// Tactical planning agent for combat scenarios.
/// Analyzes player behavior patterns and generates adaptive tactics.
/// </summary>
public class TacticsAgent
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly ILogger<TacticsAgent> _logger;
    private readonly Dictionary<string, PlayerBehaviorProfile> _behaviorProfiles;

    public TacticsAgent(
        Kernel kernel,
        ILogger<TacticsAgent> logger)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    /// </summary>
    public async Task<TacticalPlan> CreateTacticalPlanAsync(
        AvatarContext context,
        string playerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = _behaviorProfiles.GetValueOrDefault(playerId);
            if (profile == null)
            {
                _logger.LogWarning("No behavior profile for player {PlayerId}, using default tactics", playerId);
                return CreateDefaultTacticalPlan();
            }

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

            var plan = ParseTacticalResponse(response.Content ?? "{}");

            _logger.LogInformation(
                "Created tactical plan for {EntityName}: {Tactic} (confidence: {Confidence:F2})",
                context.Name, plan.PrimaryTactic, plan.Confidence);

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
