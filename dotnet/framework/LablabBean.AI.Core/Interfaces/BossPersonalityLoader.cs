using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.Extensions.Logging;
using LablabBean.AI.Core.Models;

namespace LablabBean.AI.Core.Interfaces;

/// <summary>
/// Loads and manages boss personality configurations from YAML files
/// </summary>
public sealed class BossPersonalityLoader
{
    private readonly ILogger<BossPersonalityLoader> _logger;
    private readonly IDeserializer _deserializer;
    private readonly Dictionary<string, BossPersonality> _cache;

    public BossPersonalityLoader(ILogger<BossPersonalityLoader> logger)
    {
        _logger = logger;
        _cache = new Dictionary<string, BossPersonality>();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// Load personality from YAML file
    /// </summary>
    public async Task<BossPersonality> LoadFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation($"Loading personality from: {filePath}");

            if (!File.Exists(filePath))
            {
                _logger.LogError($"Personality file not found: {filePath}");
                throw new FileNotFoundException($"Personality file not found: {filePath}");
            }

            // Check cache
            var cacheKey = Path.GetFullPath(filePath);
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                _logger.LogDebug($"Returning cached personality: {cached.Name}");
                return cached;
            }

            // Load and parse YAML
            var yaml = await File.ReadAllTextAsync(filePath, cancellationToken);
            var personality = _deserializer.Deserialize<BossPersonality>(yaml);

            // Validate
            ValidatePersonality(personality);

            // Cache
            _cache[cacheKey] = personality;

            _logger.LogInformation($"Personality loaded successfully: {personality.Name} v{personality.Version}");

            return personality;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading personality from {filePath}");
            throw;
        }
    }

    /// <summary>
    /// Load personality from YAML string
    /// </summary>
    public BossPersonality LoadFromString(string yaml)
    {
        try
        {
            _logger.LogInformation("Loading personality from string");

            var personality = _deserializer.Deserialize<BossPersonality>(yaml);
            ValidatePersonality(personality);

            _logger.LogInformation($"Personality loaded: {personality.Name}");

            return personality;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading personality from string");
            throw;
        }
    }

    /// <summary>
    /// Load default boss personality
    /// </summary>
    public async Task<BossPersonality> LoadDefaultAsync(
        string personalitiesPath,
        CancellationToken cancellationToken = default)
    {
        var defaultPath = Path.Combine(personalitiesPath, "boss-default.yaml");
        return await LoadFromFileAsync(defaultPath, cancellationToken);
    }

    /// <summary>
    /// Create a default boss personality programmatically (no file required)
    /// </summary>
    public BossPersonality CreateDefault()
    {
        _logger.LogInformation("Creating default boss personality programmatically");

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
                SystemPrompt = "You are a fair but demanding boss. Balance business success with employee development.",
                DecisionTemplate = "Context: {context}\nState: {state}\nMemory: {memory}\n\nMake a decision.",
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

    /// <summary>
    /// List available personality files
    /// </summary>
    public async Task<List<string>> ListAvailablePersonalitiesAsync(
        string personalitiesPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(personalitiesPath))
            {
                _logger.LogWarning($"Personalities directory not found: {personalitiesPath}");
                return new List<string>();
            }

            var files = Directory.GetFiles(personalitiesPath, "boss-*.yaml")
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .Cast<string>()
                .ToList();

            _logger.LogInformation($"Found {files.Count} personality files");

            return await Task.FromResult(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing personalities");
            return new List<string>();
        }
    }

    /// <summary>
    /// Clear personality cache
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
        _logger.LogInformation("Personality cache cleared");
    }

    /// <summary>
    /// Validate personality configuration
    /// </summary>
    private void ValidatePersonality(BossPersonality personality)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(personality.Name))
            errors.Add("Personality name is required");

        if (string.IsNullOrWhiteSpace(personality.Version))
            errors.Add("Personality version is required");

        // Validate trait ranges
        ValidateRange(personality.Traits.Leadership, nameof(personality.Traits.Leadership), errors);
        ValidateRange(personality.Traits.Strictness, nameof(personality.Traits.Strictness), errors);
        ValidateRange(personality.Traits.Fairness, nameof(personality.Traits.Fairness), errors);
        ValidateRange(personality.Traits.Empathy, nameof(personality.Traits.Empathy), errors);

        // Validate priorities sum to ~1.0
        var prioritiesSum = personality.Priorities.BusinessGoals +
                           personality.Priorities.EmployeeWellbeing +
                           personality.Priorities.Efficiency +
                           personality.Priorities.Innovation;

        if (Math.Abs(prioritiesSum - 1.0f) > 0.01f)
            errors.Add($"Priorities must sum to 1.0 (current: {prioritiesSum:F2})");

        // Validate required prompts
        if (string.IsNullOrWhiteSpace(personality.Prompts.SystemPrompt))
            errors.Add("System prompt is required");

        if (errors.Any())
        {
            var errorMessage = $"Personality validation failed:\n{string.Join("\n", errors)}";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
    }

    private void ValidateRange(float value, string name, List<string> errors)
    {
        if (value < 0f || value > 1f)
            errors.Add($"{name} must be between 0 and 1 (current: {value})");
    }
}
