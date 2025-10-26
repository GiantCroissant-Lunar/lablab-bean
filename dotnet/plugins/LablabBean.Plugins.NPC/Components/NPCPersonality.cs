using System;
using System.Collections.Generic;

namespace LablabBean.Plugins.NPC.Components;

/// <summary>
/// Defines the personality traits and dialogue style for an NPC.
/// Influences how the NPC speaks, reacts, and interacts with players.
/// </summary>
public struct NPCPersonality
{
    /// <summary>
    /// Core personality traits that define the NPC's character.
    /// </summary>
    public PersonalityTraits Traits { get; set; }

    /// <summary>
    /// Dialogue style determines word choice and sentence structure.
    /// </summary>
    public DialogueStyle Style { get; set; }

    /// <summary>
    /// How quickly and intensely the NPC's emotions change.
    /// </summary>
    public EmotionalRange EmotionalRange { get; set; }

    /// <summary>
    /// Additional custom trait values (0.0 to 1.0).
    /// Allows extending personality system without code changes.
    /// </summary>
    public Dictionary<string, float>? CustomTraits { get; set; }

    /// <summary>
    /// Name of the personality template (for loading from config).
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Creates a default neutral personality.
    /// </summary>
    public static NPCPersonality CreateDefault()
    {
        return new NPCPersonality
        {
            Traits = PersonalityTraits.CreateNeutral(),
            Style = DialogueStyle.Casual,
            EmotionalRange = EmotionalRange.Stable,
            CustomTraits = new Dictionary<string, float>(),
            TemplateName = "neutral"
        };
    }

    /// <summary>
    /// Creates a friendly merchant personality.
    /// </summary>
    public static NPCPersonality CreateFriendlyMerchant()
    {
        return new NPCPersonality
        {
            Traits = new PersonalityTraits
            {
                Friendliness = 0.9f,
                Courage = 0.5f,
                Formality = 0.3f,
                Honesty = 0.8f,
                Chattiness = 0.9f,
                Greed = 0.4f
            },
            Style = DialogueStyle.Casual,
            EmotionalRange = EmotionalRange.Expressive,
            CustomTraits = new Dictionary<string, float>(),
            TemplateName = "friendly_merchant"
        };
    }

    /// <summary>
    /// Creates a gruff warrior personality.
    /// </summary>
    public static NPCPersonality CreateGruffWarrior()
    {
        return new NPCPersonality
        {
            Traits = new PersonalityTraits
            {
                Friendliness = 0.3f,
                Courage = 0.95f,
                Formality = 0.4f,
                Honesty = 0.9f,
                Chattiness = 0.2f,
                Greed = 0.1f
            },
            Style = DialogueStyle.Gruff,
            EmotionalRange = EmotionalRange.Stoic,
            CustomTraits = new Dictionary<string, float>(),
            TemplateName = "gruff_warrior"
        };
    }

    /// <summary>
    /// Creates a scheming noble personality.
    /// </summary>
    public static NPCPersonality CreateSchemingNoble()
    {
        return new NPCPersonality
        {
            Traits = new PersonalityTraits
            {
                Friendliness = 0.6f,
                Courage = 0.4f,
                Formality = 0.95f,
                Honesty = 0.3f,
                Chattiness = 0.7f,
                Greed = 0.8f
            },
            Style = DialogueStyle.Flowery,
            EmotionalRange = EmotionalRange.Stable,
            CustomTraits = new Dictionary<string, float>(),
            TemplateName = "scheming_noble"
        };
    }

    /// <summary>
    /// Gets a custom trait value, or 0.5 (neutral) if not set.
    /// </summary>
    public float GetCustomTrait(string traitName, float defaultValue = 0.5f)
    {
        if (CustomTraits == null) return defaultValue;
        return CustomTraits.TryGetValue(traitName, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Sets a custom trait value (clamped to 0.0-1.0).
    /// </summary>
    public void SetCustomTrait(string traitName, float value)
    {
        CustomTraits ??= new Dictionary<string, float>();
        CustomTraits[traitName] = Math.Clamp(value, 0.0f, 1.0f);
    }
}

/// <summary>
/// Core personality traits that define an NPC's character.
/// All values range from 0.0 to 1.0.
/// </summary>
public struct PersonalityTraits
{
    /// <summary>
    /// Friendliness: 0.0 (hostile/cold) to 1.0 (warm/friendly).
    /// Affects greeting warmth, willingness to help, and overall demeanor.
    /// </summary>
    public float Friendliness { get; set; }

    /// <summary>
    /// Courage: 0.0 (coward) to 1.0 (brave/foolhardy).
    /// Affects combat behavior, willingness to take risks, and fear responses.
    /// </summary>
    public float Courage { get; set; }

    /// <summary>
    /// Formality: 0.0 (casual/slang) to 1.0 (formal/proper).
    /// Affects word choice, grammar, and overall speech patterns.
    /// </summary>
    public float Formality { get; set; }

    /// <summary>
    /// Honesty: 0.0 (deceptive) to 1.0 (truthful/direct).
    /// Affects trustworthiness, trading fairness, and dialogue truthfulness.
    /// </summary>
    public float Honesty { get; set; }

    /// <summary>
    /// Chattiness: 0.0 (terse/silent) to 1.0 (verbose/talkative).
    /// Affects dialogue length and willingness to engage in conversation.
    /// </summary>
    public float Chattiness { get; set; }

    /// <summary>
    /// Greed: 0.0 (generous) to 1.0 (greedy/selfish).
    /// Affects trading prices, quest rewards, and willingness to share.
    /// </summary>
    public float Greed { get; set; }

    /// <summary>
    /// Creates neutral personality traits (all values at 0.5).
    /// </summary>
    public static PersonalityTraits CreateNeutral()
    {
        return new PersonalityTraits
        {
            Friendliness = 0.5f,
            Courage = 0.5f,
            Formality = 0.5f,
            Honesty = 0.5f,
            Chattiness = 0.5f,
            Greed = 0.5f
        };
    }

    /// <summary>
    /// Creates random personality traits.
    /// </summary>
    public static PersonalityTraits CreateRandom(Random? random = null)
    {
        random ??= new Random();
        return new PersonalityTraits
        {
            Friendliness = (float)random.NextDouble(),
            Courage = (float)random.NextDouble(),
            Formality = (float)random.NextDouble(),
            Honesty = (float)random.NextDouble(),
            Chattiness = (float)random.NextDouble(),
            Greed = (float)random.NextDouble()
        };
    }

    /// <summary>
    /// Validates all traits are in valid range (0.0-1.0).
    /// </summary>
    public readonly bool IsValid()
    {
        return Friendliness >= 0.0f && Friendliness <= 1.0f &&
               Courage >= 0.0f && Courage <= 1.0f &&
               Formality >= 0.0f && Formality <= 1.0f &&
               Honesty >= 0.0f && Honesty <= 1.0f &&
               Chattiness >= 0.0f && Chattiness <= 1.0f &&
               Greed >= 0.0f && Greed <= 1.0f;
    }

    /// <summary>
    /// Clamps all traits to valid range (0.0-1.0).
    /// </summary>
    public void Normalize()
    {
        Friendliness = Math.Clamp(Friendliness, 0.0f, 1.0f);
        Courage = Math.Clamp(Courage, 0.0f, 1.0f);
        Formality = Math.Clamp(Formality, 0.0f, 1.0f);
        Honesty = Math.Clamp(Honesty, 0.0f, 1.0f);
        Chattiness = Math.Clamp(Chattiness, 0.0f, 1.0f);
        Greed = Math.Clamp(Greed, 0.0f, 1.0f);
    }
}

/// <summary>
/// Dialogue style determines word choice, sentence structure, and overall speech patterns.
/// </summary>
public enum DialogueStyle
{
    /// <summary>
    /// Formal, proper language.
    /// Example: "Good day to you, esteemed traveler."
    /// </summary>
    Formal,

    /// <summary>
    /// Relaxed, conversational language.
    /// Example: "Hey there! What's up?"
    /// </summary>
    Casual,

    /// <summary>
    /// Rough, abrupt language.
    /// Example: "What do you want?"
    /// </summary>
    Gruff,

    /// <summary>
    /// Elaborate, ornate language.
    /// Example: "Greetings, most noble and distinguished adventurer!"
    /// </summary>
    Flowery,

    /// <summary>
    /// Brief, terse responses.
    /// Example: "Yeah." "Nope." "Fine."
    /// </summary>
    Minimal
}

/// <summary>
/// Emotional range determines how quickly and intensely NPC emotions change.
/// </summary>
public enum EmotionalRange
{
    /// <summary>
    /// Emotions change gradually and predictably.
    /// Takes multiple interactions to shift mood significantly.
    /// </summary>
    Stable,

    /// <summary>
    /// Emotions change quickly and dramatically.
    /// Can go from happy to angry in one interaction.
    /// </summary>
    Volatile,

    /// <summary>
    /// Shows little to no emotional response.
    /// Maintains neutral demeanor regardless of situation.
    /// </summary>
    Stoic,

    /// <summary>
    /// Shows emotions clearly and intensely.
    /// Feelings are obvious in dialogue and behavior.
    /// </summary>
    Expressive
}
