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
