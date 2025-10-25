using Akka.Actor;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using LablabBean.AI.Core.Models;
using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Actors;
using LablabBean.AI.Actors.Bridges;

namespace LablabBean.AI.Agents;

/// <summary>
/// Factory for creating Boss AI components (Actor + Intelligence Agent)
/// </summary>
public sealed class BossFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly Kernel _kernel;
    private readonly BossPersonalityLoader _personalityLoader;

    public BossFactory(
        ILoggerFactory loggerFactory,
        Kernel kernel,
        BossPersonalityLoader personalityLoader)
    {
        _loggerFactory = loggerFactory;
        _kernel = kernel;
        _personalityLoader = personalityLoader;
    }

    /// <summary>
    /// Create a Boss actor with intelligence agent
    /// </summary>
    public async Task<(IActorRef actor, BossIntelligenceAgent agent)> CreateBossAsync(
        ActorSystem actorSystem,
        string entityId,
        string? personalityFile = null,
        IActorRef? eventBusAdapter = null,
        bool enableTactics = false,
        CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<BossFactory>();

        try
        {
            logger.LogInformation($"Creating Boss AI for entity: {entityId} (Tactics: {enableTactics})");

            // Load personality
            var personality = personalityFile != null
                ? await _personalityLoader.LoadFromFileAsync(personalityFile, cancellationToken)
                : await LoadDefaultPersonalityAsync(cancellationToken);

            logger.LogInformation($"Using personality: {personality.Name} v{personality.Version}");

            // Create tactics agent if enabled
            TacticsAgent? tacticsAgent = null;
            if (enableTactics)
            {
                tacticsAgent = new TacticsAgent(
                    _kernel,
                    _loggerFactory.CreateLogger<TacticsAgent>());
                logger.LogInformation("Tactical capabilities enabled");
            }

            // Create intelligence agent with optional tactics
            var agent = new BossIntelligenceAgent(
                _kernel,
                personality,
                _loggerFactory.CreateLogger<BossIntelligenceAgent>(),
                entityId,
                tacticsAgent
            );

            // Create event bus adapter if not provided
            var adapter = eventBusAdapter ?? actorSystem.ActorOf(
                Props.Create<EventBusAkkaAdapter>(),
                $"event-bus-adapter-{entityId}"
            );

            // Create actor - use explicit arguments to avoid lambda closure issues
            var actor = actorSystem.ActorOf(
                Props.Create<BossActor>(entityId, personality, adapter, agent),
                $"boss-{entityId}"
            );

            logger.LogInformation($"Boss AI created successfully: {entityId}");

            return (actor, agent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error creating Boss AI for {entityId}");
            throw;
        }
    }

    /// <summary>
    /// Create multiple Boss instances
    /// </summary>
    public async Task<List<(IActorRef actor, BossIntelligenceAgent agent)>> CreateBossesAsync(
        ActorSystem actorSystem,
        IEnumerable<string> entityIds,
        string? personalityFile = null,
        IActorRef? eventBusAdapter = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<(IActorRef, BossIntelligenceAgent)>();

        foreach (var entityId in entityIds)
        {
            var boss = await CreateBossAsync(
                actorSystem,
                entityId,
                personalityFile,
                eventBusAdapter,
                enableTactics: false,
                cancellationToken
            );
            results.Add(boss);
        }

        return results;
    }

    /// <summary>
    /// Load default personality
    /// </summary>
    private async Task<BossPersonality> LoadDefaultPersonalityAsync(
        CancellationToken cancellationToken)
    {
        // Try to load from standard location
        var personalitiesPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "..",
            "personalities"
        );

        if (Directory.Exists(personalitiesPath))
        {
            return await _personalityLoader.LoadDefaultAsync(personalitiesPath, cancellationToken);
        }

        // Fallback: create default personality programmatically
        return CreateDefaultPersonality();
    }

    /// <summary>
    /// Create default personality programmatically
    /// </summary>
    private BossPersonality CreateDefaultPersonality()
    {
        return new BossPersonality
        {
            Name = "Boss (Default)",
            Version = "1.0.0",
            AvatarType = "boss",
            Traits = new PersonalityTraits
            {
                Leadership = 0.85f,
                Strictness = 0.65f,
                Fairness = 0.75f,
                Empathy = 0.55f,
                Efficiency = 0.80f,
                Humor = 0.45f,
                Patience = 0.60f,
                Innovation = 0.70f
            },
            Behavior = new BehaviorParameters
            {
                DecisionSpeed = 0.70f,
                RiskTolerance = 0.45f,
                Delegation = 0.75f,
                Micromanagement = 0.35f,
                PraiseFrequency = 0.60f,
                CriticismDirectness = 0.70f
            },
            Memory = new MemoryConfiguration
            {
                ShortTermCapacity = 10,
                LongTermPriority = 0.75f,
                EmotionalWeight = 0.65f
            },
            Dialogue = new DialogueStyle
            {
                Formality = 0.70f,
                Verbosity = 0.60f,
                Positivity = 0.65f,
                Directness = 0.75f
            },
            Relationships = new RelationshipDynamics
            {
                TrustBuildRate = 0.50f,
                TrustDecayRate = 0.30f,
                AuthorityImportance = 0.80f,
                TeamBonding = 0.65f
            },
            Priorities = new DecisionPriorities
            {
                BusinessGoals = 0.35f,
                EmployeeWellbeing = 0.25f,
                Efficiency = 0.25f,
                Innovation = 0.15f
            },
            Modifiers = new ContextualModifiers
            {
                StressThreshold = 0.70f,
                FatigueImpact = 0.50f,
                SuccessBoost = 0.20f,
                FailureImpact = 0.30f
            },
            Prompts = new SystemPrompts
            {
                SystemPrompt = "You are a fair but demanding boss in a coffee shop. Balance business success with employee development.",
                DecisionTemplate = "Context: {context}\nState: {state}\nMemory: {memory}\n\nMake a decision considering business and people.",
                DialogueTemplate = "Speaking to: {employee_name}\nRelationship: {relationship_level}\nContext: {context}\n\nRespond naturally."
            },
            Emotions = new EmotionalStates
            {
                Default = "neutral",
                Available = new List<string> { "neutral", "calm", "focused", "pleased", "concerned", "frustrated", "stressed", "motivated" },
                Triggers = new Dictionary<string, List<string>>()
            },
            ResponseTemplates = new Dictionary<string, Dictionary<string, List<string>>>(),
            Metadata = new PersonalityMetadata
            {
                Author = "LablabBean",
                Created = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Tags = new List<string> { "boss", "default" },
                Description = "Default boss personality"
            }
        };
    }
}
