using Akka.Actor;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using LablabBean.AI.Core.Models;
using LablabBean.AI.Agents.Configuration;
using LablabBean.AI.Actors;
using LablabBean.AI.Actors.Bridges;

namespace LablabBean.AI.Agents;

/// <summary>
/// Factory for creating Employee AI components (Actor + Intelligence Agent)
/// </summary>
public sealed class EmployeeFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly Kernel _kernel;
    private readonly EmployeePersonalityLoader _personalityLoader;

    public EmployeeFactory(
        ILoggerFactory loggerFactory,
        Kernel kernel,
        EmployeePersonalityLoader personalityLoader)
    {
        _loggerFactory = loggerFactory;
        _kernel = kernel;
        _personalityLoader = personalityLoader;
    }

    /// <summary>
    /// Create an Employee actor with intelligence agent
    /// </summary>
    public async Task<(IActorRef actor, EmployeeIntelligenceAgent agent)> CreateEmployeeAsync(
        ActorSystem actorSystem,
        string entityId,
        string? personalityFile = null,
        IActorRef? eventBusAdapter = null,
        CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<EmployeeFactory>();

        try
        {
            logger.LogInformation($"Creating Employee AI for entity: {entityId}");

            // Load personality
            var personality = personalityFile != null
                ? await _personalityLoader.LoadFromFileAsync(personalityFile, cancellationToken)
                : await LoadDefaultPersonalityAsync(cancellationToken);

            logger.LogInformation($"Using personality: {personality.Name} v{personality.Version}");

            // Create intelligence agent
            var agent = new EmployeeIntelligenceAgent(
                _kernel,
                personality,
                _loggerFactory.CreateLogger<EmployeeIntelligenceAgent>(),
                entityId
            );

            // Create event bus adapter if not provided
            var adapter = eventBusAdapter ?? actorSystem.ActorOf(
                Props.Create<EventBusAkkaAdapter>(),
                $"event-bus-adapter-{entityId}"
            );

            // Create actor
            var actor = actorSystem.ActorOf(
                Props.Create<EmployeeActor>(entityId, personality, adapter, agent),
                $"employee-{entityId}"
            );

            logger.LogInformation($"Employee AI created successfully: {entityId}");

            return (actor, agent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error creating Employee AI for {entityId}");
            throw;
        }
    }

    /// <summary>
    /// Create multiple Employee instances
    /// </summary>
    public async Task<List<(IActorRef actor, EmployeeIntelligenceAgent agent)>> CreateEmployeesAsync(
        ActorSystem actorSystem,
        IEnumerable<string> entityIds,
        string? personalityFile = null,
        IActorRef? eventBusAdapter = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<(IActorRef, EmployeeIntelligenceAgent)>();

        foreach (var entityId in entityIds)
        {
            var employee = await CreateEmployeeAsync(
                actorSystem,
                entityId,
                personalityFile,
                eventBusAdapter,
                cancellationToken
            );
            results.Add(employee);
        }

        return results;
    }

    /// <summary>
    /// Create employees with different personalities
    /// </summary>
    public async Task<List<(IActorRef actor, EmployeeIntelligenceAgent agent)>> CreateDiverseEmployeesAsync(
        ActorSystem actorSystem,
        Dictionary<string, string?> entityPersonalityMap,
        IActorRef? eventBusAdapter = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<(IActorRef, EmployeeIntelligenceAgent)>();

        foreach (var (entityId, personalityFile) in entityPersonalityMap)
        {
            var employee = await CreateEmployeeAsync(
                actorSystem,
                entityId,
                personalityFile,
                eventBusAdapter,
                cancellationToken
            );
            results.Add(employee);
        }

        return results;
    }

    /// <summary>
    /// Load default personality
    /// </summary>
    private async Task<EmployeePersonality> LoadDefaultPersonalityAsync(
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
    /// Create default personality programmatically (fallback if YAML not found)
    /// </summary>
    private EmployeePersonality CreateDefaultPersonality()
    {
        return new EmployeePersonality
        {
            Name = "Employee (Default)",
            Version = "1.0.0",
            AvatarType = "employee",
            Traits = new EmployeeTraits
            {
                Diligence = 0.75f,
                Friendliness = 0.70f,
                Adaptability = 0.65f,
                Creativity = 0.60f,
                Teamwork = 0.70f,
                AttentionToDetail = 0.70f,
                Enthusiasm = 0.65f,
                Resilience = 0.65f
            },
            Behavior = new EmployeeBehavior
            {
                TaskCompletionSpeed = 0.70f,
                InitiativeLevel = 0.60f,
                CustomerFocus = 0.75f,
                LearningRate = 0.65f,
                StressTolerance = 0.60f,
                MistakeRecovery = 0.65f
            },
            Skills = new EmployeeSkills
            {
                CoffeeMaking = 0.60f,
                CustomerService = 0.65f,
                CashHandling = 0.55f,
                Cleaning = 0.70f,
                Multitasking = 0.50f,
                ProblemSolving = 0.55f
            },
            Preferences = new WorkPreferences
            {
                PreferredTasks = new List<string> { "coffee_making", "customer_service" },
                DislikedTasks = new List<string> { "cleaning" },
                PreferredShift = "morning",
                TeamWorkPreference = 0.70f
            },
            Growth = new GrowthParameters
            {
                SkillImprovementRate = 0.02f,
                ConfidenceBuildRate = 0.05f,
                ConfidenceDecayRate = 0.02f,
                BurnoutThreshold = 0.80f,
                MotivationBaseline = 0.70f
            },
            Performance = new PerformanceFactors
            {
                BaseEfficiency = 0.70f,
                QualityFocus = 0.65f,
                Consistency = 0.70f,
                PeakHoursBoost = 0.15f,
                FatigueImpact = 0.20f
            },
            Learning = new Dictionary<string, LearningCurve>
            {
                ["coffee_making"] = new LearningCurve { PlateauLevel = 0.90f, PracticeRequired = 100 },
                ["customer_service"] = new LearningCurve { PlateauLevel = 0.85f, PracticeRequired = 80 },
                ["cash_handling"] = new LearningCurve { PlateauLevel = 0.95f, PracticeRequired = 60 },
                ["cleaning"] = new LearningCurve { PlateauLevel = 0.85f, PracticeRequired = 50 },
                ["multitasking"] = new LearningCurve { PlateauLevel = 0.80f, PracticeRequired = 120 },
                ["problem_solving"] = new LearningCurve { PlateauLevel = 0.85f, PracticeRequired = 100 }
            },
            TaskModifiers = new Dictionary<string, TaskModifier>
            {
                ["coffee_making"] = new TaskModifier { BaseTime = 1.0f, QualityBonus = 1.0f, EnergyImpact = 0.1f, BaseSatisfaction = 0.7f, FriendlinessBonus = 0.1f, StressPenalty = 0.05f, DetailBonus = 0.15f, FatiguePenalty = 0.1f },
                ["customer_service"] = new TaskModifier { BaseTime = 0.9f, QualityBonus = 1.1f, EnergyImpact = 0.15f, BaseSatisfaction = 0.6f, FriendlinessBonus = 0.2f, StressPenalty = 0.1f, DetailBonus = 0.1f, FatiguePenalty = 0.15f },
                ["cash_handling"] = new TaskModifier { BaseTime = 1.1f, QualityBonus = 0.9f, EnergyImpact = 0.05f, BaseSatisfaction = 0.5f, FriendlinessBonus = 0.05f, StressPenalty = 0.15f, DetailBonus = 0.2f, FatiguePenalty = 0.05f },
                ["cleaning"] = new TaskModifier { BaseTime = 1.2f, QualityBonus = 0.9f, EnergyImpact = 0.2f, BaseSatisfaction = 0.4f, FriendlinessBonus = 0f, StressPenalty = 0.05f, DetailBonus = 0.1f, FatiguePenalty = 0.2f }
            },
            Memory = new MemoryConfiguration
            {
                ShortTermCapacity = 8,
                LongTermPriority = 0.70f,
                EmotionalWeight = 0.60f
            },
            Dialogue = new DialogueStyle
            {
                Formality = 0.60f,
                Verbosity = 0.55f,
                Positivity = 0.70f,
                Directness = 0.65f
            },
            Relationships = new EmployeeRelationshipDynamics
            {
                BossRespect = 0.65f,
                PeerFriendliness = 0.70f,
                CustomerWarmth = 0.75f,
                ConflictTolerance = 0.50f,
                FeedbackReceptivity = 0.70f
            },
            Prompts = new SystemPrompts
            {
                SystemPrompt = "You are a dedicated coffee shop employee. You care about doing good work, helping customers, and maintaining positive relationships with your boss and coworkers.",
                DecisionTemplate = "Context: {context}\nSituation: {situation}\nYour state: {state}\nRecent memories: {memory}\n\nWhat should you do?",
                DialogueTemplate = "Speaking to: {listener_name} ({listener_type})\nRelationship: {relationship_level}\nSituation: {context}\nEmotion: {emotion}\n\nRespond naturally."
            },
            Emotions = new EmotionalStates
            {
                Default = "neutral",
                Available = new List<string>
                {
                    "neutral", "happy", "content", "focused", "stressed",
                    "tired", "frustrated", "proud", "anxious", "motivated"
                },
                Triggers = new Dictionary<string, List<string>>
                {
                    ["task_completed"] = new List<string> { "proud", "content", "motivated" },
                    ["task_failed"] = new List<string> { "frustrated", "anxious" },
                    ["praised"] = new List<string> { "happy", "proud", "motivated" },
                    ["criticized"] = new List<string> { "anxious", "frustrated" },
                    ["low_energy"] = new List<string> { "tired" },
                    ["high_stress"] = new List<string> { "stressed", "anxious" }
                }
            },
            ResponseTemplates = new Dictionary<string, Dictionary<string, List<string>>>
            {
                ["greetings"] = new Dictionary<string, List<string>>
                {
                    ["morning"] = new List<string> { "Good morning!", "Morning! Ready for the day!", "Hey there! How's your morning?" },
                    ["afternoon"] = new List<string> { "Good afternoon!", "Hey! How's it going?", "Afternoon!" },
                    ["evening"] = new List<string> { "Good evening!", "Hey! Still here?", "Evening shift!" }
                },
                ["task_responses"] = new Dictionary<string, List<string>>
                {
                    ["accept"] = new List<string> { "On it!", "Sure thing!", "I'll get right on that!" },
                    ["hesitant"] = new List<string> { "Okay, but I'm a bit tired...", "I'll try my best.", "Alright..." },
                    ["decline"] = new List<string> { "I really need a break first.", "Can someone else handle this?" }
                }
            },
            Metadata = new PersonalityMetadata
            {
                Author = "LablabBean",
                Created = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Tags = new List<string> { "employee", "default", "balanced" },
                Description = "Default balanced employee personality with moderate skills and positive attitude"
            }
        };
    }

    /// <summary>
    /// Create a personality variant with specific trait modifications
    /// </summary>
    public EmployeePersonality CreatePersonalityVariant(
        string name,
        Dictionary<string, float> traitModifiers)
    {
        var basePersonality = CreateDefaultPersonality();

        // Create new instance with modified name (avoid init-only property issue)
        var variant = new EmployeePersonality
        {
            Name = name,
            Version = basePersonality.Version,
            AvatarType = basePersonality.AvatarType,
            Traits = basePersonality.Traits,
            Behavior = basePersonality.Behavior,
            Skills = basePersonality.Skills,
            Preferences = basePersonality.Preferences,
            Growth = basePersonality.Growth,
            Relationships = basePersonality.Relationships,
            Performance = basePersonality.Performance,
            Memory = basePersonality.Memory,
            Dialogue = basePersonality.Dialogue,
            Prompts = basePersonality.Prompts,
            Emotions = basePersonality.Emotions,
            ResponseTemplates = basePersonality.ResponseTemplates,
            TaskModifiers = basePersonality.TaskModifiers,
            Learning = basePersonality.Learning,
            Metadata = basePersonality.Metadata
        };

        // Apply trait modifications
        foreach (var (trait, modifier) in traitModifiers)
        {
            var property = typeof(EmployeeTraits).GetProperty(trait);
            if (property != null && property.CanWrite)
            {
                var currentValue = (float)(property.GetValue(variant.Traits) ?? 0.5f);
                var newValue = Math.Clamp(currentValue + modifier, 0f, 1f);
                property.SetValue(variant.Traits, newValue);
            }
        }

        return variant;
    }
}
