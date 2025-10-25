using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.Extensions.Logging;
using LablabBean.AI.Core.Models;

namespace LablabBean.AI.Core.Interfaces;

/// <summary>
/// Loads and manages employee personality configurations from YAML files
/// </summary>
public sealed class EmployeePersonalityLoader
{
    private readonly ILogger<EmployeePersonalityLoader> _logger;
    private readonly IDeserializer _deserializer;
    private readonly Dictionary<string, EmployeePersonality> _cache;

    public EmployeePersonalityLoader(ILogger<EmployeePersonalityLoader> logger)
    {
        _logger = logger;
        _cache = new Dictionary<string, EmployeePersonality>();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// Load personality from YAML file
    /// </summary>
    public async Task<EmployeePersonality> LoadFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation($"Loading employee personality from: {filePath}");

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
            var personality = _deserializer.Deserialize<EmployeePersonality>(yaml);

            // Validate
            ValidatePersonality(personality);

            // Cache
            _cache[cacheKey] = personality;

            _logger.LogInformation($"Employee personality loaded successfully: {personality.Name} v{personality.Version}");

            return personality;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading employee personality from {filePath}");
            throw;
        }
    }

    /// <summary>
    /// Load personality from YAML string
    /// </summary>
    public EmployeePersonality LoadFromString(string yaml)
    {
        try
        {
            _logger.LogInformation("Loading employee personality from string");

            var personality = _deserializer.Deserialize<EmployeePersonality>(yaml);
            ValidatePersonality(personality);

            _logger.LogInformation($"Employee personality loaded: {personality.Name}");

            return personality;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading employee personality from string");
            throw;
        }
    }

    /// <summary>
    /// Load default employee personality
    /// </summary>
    public async Task<EmployeePersonality> LoadDefaultAsync(
        string personalitiesPath,
        CancellationToken cancellationToken = default)
    {
        var defaultPath = Path.Combine(personalitiesPath, "employee-default.yaml");
        return await LoadFromFileAsync(defaultPath, cancellationToken);
    }

    /// <summary>
    /// Create a default employee personality programmatically (no file required)
    /// </summary>
    public EmployeePersonality CreateDefault()
    {
        _logger.LogInformation("Creating default employee personality programmatically");

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
                ["customer_service"] = new LearningCurve { PlateauLevel = 0.85f, PracticeRequired = 80 }
            },
            TaskModifiers = new Dictionary<string, TaskModifier>
            {
                ["coffee_making"] = new TaskModifier { BaseTime = 1.0f, QualityBonus = 1.0f, EnergyImpact = 0.1f, BaseSatisfaction = 0.7f, FriendlinessBonus = 0.1f, StressPenalty = 0.05f, DetailBonus = 0.15f, FatiguePenalty = 0.1f }
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
                SystemPrompt = "You are a dedicated employee. Do good work and maintain positive relationships.",
                DecisionTemplate = "Context: {context}\nSituation: {situation}\nWhat should you do?",
                DialogueTemplate = "Speaking to: {listener_name}\nRelationship: {relationship_level}\nRespond naturally."
            },
            Emotions = new EmotionalStates
            {
                Default = "neutral",
                Available = new List<string> { "neutral", "happy", "content", "focused", "stressed", "tired", "frustrated", "proud", "anxious", "motivated" },
                Triggers = new Dictionary<string, List<string>>()
            },
            ResponseTemplates = new Dictionary<string, Dictionary<string, List<string>>>(),
            Metadata = new PersonalityMetadata
            {
                Author = "LablabBean",
                Created = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Tags = new List<string> { "employee", "default" },
                Description = "Default employee personality"
            }
        };
    }

    /// <summary>
    /// List available employee personality files
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

            var files = Directory.GetFiles(personalitiesPath, "employee-*.yaml")
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .Cast<string>()
                .ToList();

            _logger.LogInformation($"Found {files.Count} employee personality files");

            return await Task.FromResult(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing employee personalities");
            return new List<string>();
        }
    }

    /// <summary>
    /// Clear personality cache
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
        _logger.LogInformation("Employee personality cache cleared");
    }

    /// <summary>
    /// Validate personality configuration
    /// </summary>
    private void ValidatePersonality(EmployeePersonality personality)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(personality.Name))
            errors.Add("Personality name is required");

        if (string.IsNullOrWhiteSpace(personality.Version))
            errors.Add("Personality version is required");

        // Validate trait ranges (0-1)
        ValidateRange(personality.Traits.Diligence, nameof(personality.Traits.Diligence), errors);
        ValidateRange(personality.Traits.Friendliness, nameof(personality.Traits.Friendliness), errors);
        ValidateRange(personality.Traits.Adaptability, nameof(personality.Traits.Adaptability), errors);
        ValidateRange(personality.Traits.Creativity, nameof(personality.Traits.Creativity), errors);
        ValidateRange(personality.Traits.Teamwork, nameof(personality.Traits.Teamwork), errors);
        ValidateRange(personality.Traits.AttentionToDetail, nameof(personality.Traits.AttentionToDetail), errors);
        ValidateRange(personality.Traits.Enthusiasm, nameof(personality.Traits.Enthusiasm), errors);
        ValidateRange(personality.Traits.Resilience, nameof(personality.Traits.Resilience), errors);

        // Validate behavior ranges
        ValidateRange(personality.Behavior.TaskCompletionSpeed, nameof(personality.Behavior.TaskCompletionSpeed), errors);
        ValidateRange(personality.Behavior.InitiativeLevel, nameof(personality.Behavior.InitiativeLevel), errors);
        ValidateRange(personality.Behavior.CustomerFocus, nameof(personality.Behavior.CustomerFocus), errors);
        ValidateRange(personality.Behavior.LearningRate, nameof(personality.Behavior.LearningRate), errors);
        ValidateRange(personality.Behavior.StressTolerance, nameof(personality.Behavior.StressTolerance), errors);
        ValidateRange(personality.Behavior.MistakeRecovery, nameof(personality.Behavior.MistakeRecovery), errors);

        // Validate skill ranges
        ValidateRange(personality.Skills.CoffeeMaking, nameof(personality.Skills.CoffeeMaking), errors);
        ValidateRange(personality.Skills.CustomerService, nameof(personality.Skills.CustomerService), errors);
        ValidateRange(personality.Skills.CashHandling, nameof(personality.Skills.CashHandling), errors);
        ValidateRange(personality.Skills.Cleaning, nameof(personality.Skills.Cleaning), errors);
        ValidateRange(personality.Skills.Multitasking, nameof(personality.Skills.Multitasking), errors);
        ValidateRange(personality.Skills.ProblemSolving, nameof(personality.Skills.ProblemSolving), errors);

        // Validate performance factors
        ValidateRange(personality.Performance.BaseEfficiency, nameof(personality.Performance.BaseEfficiency), errors);
        ValidateRange(personality.Performance.QualityFocus, nameof(personality.Performance.QualityFocus), errors);
        ValidateRange(personality.Performance.Consistency, nameof(personality.Performance.Consistency), errors);

        // Validate growth parameters
        if (personality.Growth.SkillImprovementRate < 0 || personality.Growth.SkillImprovementRate > 1)
            errors.Add($"SkillImprovementRate must be between 0 and 1 (current: {personality.Growth.SkillImprovementRate})");

        if (personality.Growth.BurnoutThreshold < 0 || personality.Growth.BurnoutThreshold > 1)
            errors.Add($"BurnoutThreshold must be between 0 and 1 (current: {personality.Growth.BurnoutThreshold})");

        // Validate learning curves
        foreach (var (skill, curve) in personality.Learning)
        {
            if (curve.PlateauLevel < 0 || curve.PlateauLevel > 1)
                errors.Add($"Learning curve plateau for {skill} must be between 0 and 1 (current: {curve.PlateauLevel})");

            if (curve.PracticeRequired <= 0)
                errors.Add($"Learning curve practice required for {skill} must be positive (current: {curve.PracticeRequired})");
        }

        // Validate required prompts
        if (string.IsNullOrWhiteSpace(personality.Prompts.SystemPrompt))
            errors.Add("System prompt is required");

        if (string.IsNullOrWhiteSpace(personality.Prompts.DecisionTemplate))
            errors.Add("Decision template is required");

        if (string.IsNullOrWhiteSpace(personality.Prompts.DialogueTemplate))
            errors.Add("Dialogue template is required");

        // Validate emotions
        if (personality.Emotions.Available == null || !personality.Emotions.Available.Any())
        {
            errors.Add("At least one emotion must be available");
        }
        else if (!personality.Emotions.Available.Contains(personality.Emotions.Default))
        {
            errors.Add($"Default emotion '{personality.Emotions.Default}' must be in available emotions list");
        }

        if (errors.Any())
        {
            var errorMessage = $"Employee personality validation failed:\n{string.Join("\n", errors)}";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        _logger.LogDebug($"Employee personality validation successful: {personality.Name}");
    }

    private void ValidateRange(float value, string name, List<string> errors)
    {
        if (value < 0f || value > 1f)
            errors.Add($"{name} must be between 0 and 1 (current: {value})");
    }

    /// <summary>
    /// Create a personality variant by modifying traits
    /// </summary>
    public EmployeePersonality CreateVariant(
        EmployeePersonality basePersonality,
        string variantName,
        Dictionary<string, float> traitModifiers)
    {
        var variant = ClonePersonality(basePersonality);

        // Create a new instance with the modified name (since Name is init-only)
        // We can't modify variant.Name directly, so we need to work with what we have
        // or serialize/deserialize to create a new instance

        // Apply trait modifications - Traits may also be init-only, need to check
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

        ValidatePersonality(variant);
        return variant;
    }

    private EmployeePersonality ClonePersonality(EmployeePersonality source)
    {
        // Simple clone via serialization
        var yaml = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build()
            .Serialize(source);

        return _deserializer.Deserialize<EmployeePersonality>(yaml);
    }
}
