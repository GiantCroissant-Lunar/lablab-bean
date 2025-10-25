using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LablabBean.Plugins.NPC.Components;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LablabBean.Plugins.NPC.Services;

/// <summary>
/// Service for loading and managing personality templates from YAML configuration.
/// </summary>
public class PersonalityTemplateService
{
    private readonly Dictionary<string, PersonalityTemplate> _templates = new();
    private readonly IDeserializer _deserializer;

    public PersonalityTemplateService()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// Loads personality templates from a YAML file.
    /// </summary>
    public void LoadTemplates(string yamlFilePath)
    {
        if (!File.Exists(yamlFilePath))
            throw new FileNotFoundException($"Personality template file not found: {yamlFilePath}");

        var yaml = File.ReadAllText(yamlFilePath);
        var config = _deserializer.Deserialize<PersonalityConfig>(yaml);

        if (config?.Personalities == null)
            return;

        foreach (var (key, template) in config.Personalities)
        {
            _templates[key] = template;
        }
    }

    /// <summary>
    /// Loads personality templates from embedded resources.
    /// </summary>
    public void LoadEmbeddedTemplates()
    {
        var assembly = typeof(PersonalityTemplateService).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("personality-templates.yaml"));

        if (resourceName == null)
            return;

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            return;

        using var reader = new StreamReader(stream);
        var yaml = reader.ReadToEnd();

        var config = _deserializer.Deserialize<PersonalityConfig>(yaml);

        if (config?.Personalities == null)
            return;

        foreach (var (key, template) in config.Personalities)
        {
            _templates[key] = template;
        }
    }

    /// <summary>
    /// Gets a personality template by key.
    /// </summary>
    public PersonalityTemplate? GetTemplate(string key)
    {
        return _templates.TryGetValue(key, out var template) ? template : null;
    }

    /// <summary>
    /// Checks if a template exists.
    /// </summary>
    public bool HasTemplate(string key)
    {
        return _templates.ContainsKey(key);
    }

    /// <summary>
    /// Gets all available template keys.
    /// </summary>
    public IEnumerable<string> GetAllTemplateKeys()
    {
        return _templates.Keys;
    }

    /// <summary>
    /// Creates an NPCPersonality from a template.
    /// </summary>
    public NPCPersonality CreateFromTemplate(string templateKey)
    {
        var template = GetTemplate(templateKey);
        if (template == null)
            throw new ArgumentException($"Personality template not found: {templateKey}");

        return template.ToNPCPersonality(templateKey);
    }

    /// <summary>
    /// Creates a random personality by selecting a random template.
    /// </summary>
    public NPCPersonality CreateRandom(Random? random = null)
    {
        random ??= new Random();

        if (_templates.Count == 0)
            return NPCPersonality.CreateDefault();

        var keys = _templates.Keys.ToArray();
        var key = keys[random.Next(keys.Length)];
        return CreateFromTemplate(key);
    }

    /// <summary>
    /// Creates a personality with random trait variations from a template.
    /// Applies small random adjustments (+/- variance) to each trait.
    /// </summary>
    public NPCPersonality CreateVariation(string templateKey, float variance = 0.15f, Random? random = null)
    {
        random ??= new Random();
        var basePersonality = CreateFromTemplate(templateKey);

        // Apply random variance to traits
        var traits = basePersonality.Traits;
        traits.Friendliness = ClampVariation(traits.Friendliness, variance, random);
        traits.Courage = ClampVariation(traits.Courage, variance, random);
        traits.Formality = ClampVariation(traits.Formality, variance, random);
        traits.Honesty = ClampVariation(traits.Honesty, variance, random);
        traits.Chattiness = ClampVariation(traits.Chattiness, variance, random);
        traits.Greed = ClampVariation(traits.Greed, variance, random);

        basePersonality.Traits = traits;
        return basePersonality;
    }

    private static float ClampVariation(float value, float variance, Random random)
    {
        var adjustment = (float)(random.NextDouble() * 2 - 1) * variance; // -variance to +variance
        return Math.Clamp(value + adjustment, 0.0f, 1.0f);
    }
}

/// <summary>
/// Root configuration object for YAML deserialization.
/// </summary>
public class PersonalityConfig
{
    public Dictionary<string, PersonalityTemplate>? Personalities { get; set; }
}

/// <summary>
/// Personality template loaded from YAML configuration.
/// </summary>
public class PersonalityTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PersonalityTraitsTemplate Traits { get; set; } = new();
    public string Style { get; set; } = "casual";
    public string EmotionalRange { get; set; } = "stable";
    public Dictionary<string, float>? CustomTraits { get; set; }

    /// <summary>
    /// Converts this template to an NPCPersonality component.
    /// </summary>
    public NPCPersonality ToNPCPersonality(string templateKey)
    {
        return new NPCPersonality
        {
            Traits = new PersonalityTraits
            {
                Friendliness = Traits.Friendliness,
                Courage = Traits.Courage,
                Formality = Traits.Formality,
                Honesty = Traits.Honesty,
                Chattiness = Traits.Chattiness,
                Greed = Traits.Greed
            },
            Style = ParseDialogueStyle(Style),
            EmotionalRange = ParseEmotionalRange(EmotionalRange),
            CustomTraits = CustomTraits != null ? new Dictionary<string, float>(CustomTraits) : new Dictionary<string, float>(),
            TemplateName = templateKey
        };
    }

    private static DialogueStyle ParseDialogueStyle(string style)
    {
        return style.ToLowerInvariant() switch
        {
            "formal" => DialogueStyle.Formal,
            "casual" => DialogueStyle.Casual,
            "gruff" => DialogueStyle.Gruff,
            "flowery" => DialogueStyle.Flowery,
            "minimal" => DialogueStyle.Minimal,
            _ => DialogueStyle.Casual
        };
    }

    private static Components.EmotionalRange ParseEmotionalRange(string range)
    {
        return range.ToLowerInvariant() switch
        {
            "stable" => Components.EmotionalRange.Stable,
            "volatile" => Components.EmotionalRange.Volatile,
            "stoic" => Components.EmotionalRange.Stoic,
            "expressive" => Components.EmotionalRange.Expressive,
            _ => Components.EmotionalRange.Stable
        };
    }
}

/// <summary>
/// Personality traits template for YAML deserialization.
/// </summary>
public class PersonalityTraitsTemplate
{
    public float Friendliness { get; set; } = 0.5f;
    public float Courage { get; set; } = 0.5f;
    public float Formality { get; set; } = 0.5f;
    public float Honesty { get; set; } = 0.5f;
    public float Chattiness { get; set; } = 0.5f;
    public float Greed { get; set; } = 0.5f;
}
