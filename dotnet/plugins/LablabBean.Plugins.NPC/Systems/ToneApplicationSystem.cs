using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.NPC.Components;

namespace LablabBean.Plugins.NPC.Systems;

/// <summary>
/// Advanced tone application system that modifies dialogue based on:
/// - Current emotional state
/// - Relationship with speaker
/// - Recent conversation context
/// - Personality traits
/// </summary>
public class ToneApplicationSystem
{
    private readonly World _world;
    private readonly Random _random = new();

    public ToneApplicationSystem(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Applies emotional tone to dialogue text.
    /// </summary>
    public string ApplyEmotionalTone(
        string text,
        DialogueEmotion emotion,
        NPCPersonality personality,
        float intensity = 1.0f)
    {
        intensity = Math.Clamp(intensity, 0.0f, 1.0f);

        // Stoic NPCs show less emotion
        if (personality.EmotionalRange == EmotionalRange.Stoic)
        {
            intensity *= 0.3f;
        }
        // Expressive NPCs show more emotion
        else if (personality.EmotionalRange == EmotionalRange.Expressive)
        {
            intensity *= 1.5f;
        }
        // Volatile NPCs have extreme emotions
        else if (personality.EmotionalRange == EmotionalRange.Volatile)
        {
            intensity *= 1.8f;
        }

        intensity = Math.Clamp(intensity, 0.0f, 2.0f);

        return emotion switch
        {
            DialogueEmotion.Happy => ApplyHappyTone(text, intensity),
            DialogueEmotion.Angry => ApplyAngryTone(text, intensity),
            DialogueEmotion.Sad => ApplySadTone(text, intensity),
            DialogueEmotion.Fearful => ApplyFearfulTone(text, intensity),
            DialogueEmotion.Surprised => ApplySurprisedTone(text, intensity),
            DialogueEmotion.Contemptuous => ApplyContemptuousTone(text, intensity),
            DialogueEmotion.Excited => ApplyExcitedTone(text, intensity),
            DialogueEmotion.Cautious => ApplyCautiousTone(text, intensity),
            DialogueEmotion.Friendly => ApplyFriendlyTone(text, intensity),
            _ => text
        };
    }

    /// <summary>
    /// Applies relationship-aware tone modifications.
    /// </summary>
    public string ApplyRelationshipTone(
        string text,
        NPCPersonality personality,
        float relationshipValue,
        int conversationCount)
    {
        // relationshipValue: -1.0 (enemy) to 1.0 (friend)
        relationshipValue = Math.Clamp(relationshipValue, -1.0f, 1.0f);

        // Very friendly NPCs are nicer even to enemies
        var friendlinessAdjustment = (personality.Traits.Friendliness - 0.5f) * 0.4f;
        var adjustedRelationship = Math.Clamp(relationshipValue + friendlinessAdjustment, -1.0f, 1.0f);

        // First conversation is always more formal/cautious
        if (conversationCount <= 1)
        {
            adjustedRelationship *= 0.7f;
        }
        // Long relationship leads to more casual tone
        else if (conversationCount > 10)
        {
            adjustedRelationship *= 1.2f;
            adjustedRelationship = Math.Clamp(adjustedRelationship, -1.0f, 1.0f);
        }

        if (adjustedRelationship > 0.7f)
        {
            return ApplyFriendlyRelationshipTone(text, personality);
        }
        else if (adjustedRelationship > 0.3f)
        {
            return ApplyNeutralRelationshipTone(text, personality);
        }
        else if (adjustedRelationship > -0.3f)
        {
            return ApplyColdRelationshipTone(text, personality);
        }
        else
        {
            return ApplyHostileRelationshipTone(text, personality);
        }
    }

    /// <summary>
    /// Modifies personality slightly based on recent context.
    /// Returns a context-adjusted personality (doesn't modify original).
    /// </summary>
    public NPCPersonality ApplyContextualShift(
        NPCPersonality basePersonality,
        DialogueContext context,
        DialogueType currentDialogueType)
    {
        var shifted = basePersonality;
        var traits = shifted.Traits;

        // Combat makes everyone braver (temporarily)
        if (currentDialogueType == DialogueType.Combat)
        {
            traits.Courage = Math.Min(traits.Courage + 0.2f, 1.0f);
            traits.Chattiness = Math.Max(traits.Chattiness - 0.3f, 0.0f);
        }
        // Trading makes people more greedy
        else if (currentDialogueType == DialogueType.Trading)
        {
            traits.Greed = Math.Min(traits.Greed + 0.1f, 1.0f);
            traits.Friendliness = Math.Max(traits.Friendliness - 0.05f, 0.0f);
        }
        // Asking for help makes people more humble
        else if (currentDialogueType == DialogueType.RequestHelp)
        {
            traits.Formality = Math.Min(traits.Formality + 0.2f, 1.0f);
            traits.Friendliness = Math.Min(traits.Friendliness + 0.15f, 1.0f);
        }
        // Repeated conversations increase familiarity
        if (context.TotalDialogueCount > 5)
        {
            traits.Formality = Math.Max(traits.Formality - 0.1f, 0.0f);
            traits.Chattiness = Math.Min(traits.Chattiness + 0.1f, 1.0f);
        }

        shifted.Traits = traits;
        return shifted;
    }

    #region Emotional Tone Application

    private string ApplyHappyTone(string text, float intensity)
    {
        if (intensity < 0.3f) return text;

        var modifiers = new List<string>();

        if (intensity > 0.8f)
        {
            modifiers.AddRange(new[] { "Ha!", "Wonderful!", "Excellent!" });
        }
        else if (intensity > 0.5f)
        {
            modifiers.AddRange(new[] { "Good!", "Nice!", "Great!" });
        }
        else
        {
            modifiers.AddRange(new[] { "Oh", "Well" });
        }

        if (_random.NextDouble() < intensity * 0.5)
        {
            var modifier = modifiers[_random.Next(modifiers.Count)];
            text = $"{modifier} {text}";
        }

        // Add enthusiasm punctuation
        if (intensity > 0.7f && !text.EndsWith('!'))
        {
            text = text.TrimEnd('.', '?') + "!";
        }

        return text;
    }

    private string ApplyAngryTone(string text, float intensity)
    {
        if (intensity < 0.3f) return text;

        var modifiers = new List<string>();

        if (intensity > 0.8f)
        {
            modifiers.AddRange(new[] { "Damn it!", "Curse you!", "Enough!" });
        }
        else if (intensity > 0.5f)
        {
            modifiers.AddRange(new[] { "Listen here!", "Now look!", "Hey!" });
        }
        else
        {
            modifiers.AddRange(new[] { "Look", "Now" });
        }

        if (_random.NextDouble() < intensity * 0.6)
        {
            var modifier = modifiers[_random.Next(modifiers.Count)];
            text = $"{modifier} {text}";
        }

        // Emphasize with exclamation marks
        if (intensity > 0.6f)
        {
            text = text.TrimEnd('.', '?', '!') + "!";
        }

        return text;
    }

    private string ApplySadTone(string text, float intensity)
    {
        if (intensity < 0.3f) return text;

        var modifiers = new List<string>();

        if (intensity > 0.8f)
        {
            modifiers.AddRange(new[] { "*sighs heavily*", "Alas...", "Unfortunately..." });
        }
        else if (intensity > 0.5f)
        {
            modifiers.AddRange(new[] { "*sighs*", "I'm afraid...", "Sadly..." });
        }
        else
        {
            modifiers.AddRange(new[] { "Well...", "I suppose..." });
        }

        if (_random.NextDouble() < intensity * 0.5)
        {
            var modifier = modifiers[_random.Next(modifiers.Count)];
            text = $"{modifier} {text}";
        }

        return text;
    }

    private string ApplyFearfulTone(string text, float intensity)
    {
        if (intensity < 0.3f) return text;

        var modifiers = new List<string>();

        if (intensity > 0.8f)
        {
            modifiers.AddRange(new[] { "Oh no!", "Please, no!", "Help!" });
        }
        else if (intensity > 0.5f)
        {
            modifiers.AddRange(new[] { "Oh dear...", "I... I don't know...", "Please..." });
        }
        else
        {
            modifiers.AddRange(new[] { "Um...", "Well..." });
        }

        if (_random.NextDouble() < intensity * 0.6)
        {
            var modifier = modifiers[_random.Next(modifiers.Count)];
            text = $"{modifier} {text}";
        }

        // Add hesitation
        if (intensity > 0.5f && _random.NextDouble() < 0.3)
        {
            text = text.Replace(". ", "... ");
        }

        return text;
    }

    private string ApplySurprisedTone(string text, float intensity)
    {
        if (intensity < 0.3f) return text;

        var modifiers = new List<string>();

        if (intensity > 0.8f)
        {
            modifiers.AddRange(new[] { "What?!", "By the gods!", "Incredible!" });
        }
        else if (intensity > 0.5f)
        {
            modifiers.AddRange(new[] { "Oh!", "Really?", "What?" });
        }
        else
        {
            modifiers.AddRange(new[] { "Hm", "Oh" });
        }

        if (_random.NextDouble() < intensity * 0.6)
        {
            var modifier = modifiers[_random.Next(modifiers.Count)];
            text = $"{modifier} {text}";
        }

        if (intensity > 0.6f && !text.Contains('!'))
        {
            text = text.TrimEnd('.') + "!";
        }

        return text;
    }

    private string ApplyContemptuousTone(string text, float intensity)
    {
        if (intensity < 0.3f) return text;

        var modifiers = new List<string>();

        if (intensity > 0.8f)
        {
            modifiers.AddRange(new[] { "Pathetic.", "Ridiculous.", "How absurd." });
        }
        else if (intensity > 0.5f)
        {
            modifiers.AddRange(new[] { "Hmph.", "Really.", "I see." });
        }
        else
        {
            modifiers.AddRange(new[] { "Well", "Indeed" });
        }

        if (_random.NextDouble() < intensity * 0.5)
        {
            var modifier = modifiers[_random.Next(modifiers.Count)];
            text = $"{modifier} {text}";
        }

        return text;
    }

    private string ApplyExcitedTone(string text, float intensity)
    {
        if (intensity < 0.3f) return text;

        var modifiers = new List<string>();

        if (intensity > 0.8f)
        {
            modifiers.AddRange(new[] { "Amazing!", "Fantastic!", "Yes!" });
        }
        else if (intensity > 0.5f)
        {
            modifiers.AddRange(new[] { "Great!", "Perfect!", "Wonderful!" });
        }
        else
        {
            modifiers.AddRange(new[] { "Good", "Nice" });
        }

        if (_random.NextDouble() < intensity * 0.6)
        {
            var modifier = modifiers[_random.Next(modifiers.Count)];
            text = $"{modifier} {text}";
        }

        if (intensity > 0.7f)
        {
            text = text.TrimEnd('.', '?') + "!";
        }

        return text;
    }

    private string ApplyCautiousTone(string text, float intensity)
    {
        if (intensity < 0.3f) return text;

        var modifiers = new List<string>();

        if (intensity > 0.8f)
        {
            modifiers.AddRange(new[] { "Wait...", "Hold on...", "Careful..." });
        }
        else if (intensity > 0.5f)
        {
            modifiers.AddRange(new[] { "Perhaps...", "Maybe...", "Let's see..." });
        }
        else
        {
            modifiers.AddRange(new[] { "Well...", "Hmm..." });
        }

        if (_random.NextDouble() < intensity * 0.5)
        {
            var modifier = modifiers[_random.Next(modifiers.Count)];
            text = $"{modifier} {text}";
        }

        return text;
    }

    private string ApplyFriendlyTone(string text, float intensity)
    {
        if (intensity < 0.3f) return text;

        var modifiers = new List<string>();

        if (intensity > 0.8f)
        {
            modifiers.AddRange(new[] { "Friend!", "My friend!", "Dear friend!" });
        }
        else if (intensity > 0.5f)
        {
            modifiers.AddRange(new[] { "Hello friend!", "Hey there!", "Good to see you!" });
        }
        else
        {
            modifiers.AddRange(new[] { "Hello", "Hey" });
        }

        if (_random.NextDouble() < intensity * 0.4)
        {
            var modifier = modifiers[_random.Next(modifiers.Count)];
            text = $"{modifier} {text}";
        }

        return text;
    }

    #endregion

    #region Relationship Tone Application

    private string ApplyFriendlyRelationshipTone(string text, NPCPersonality personality)
    {
        // Close friends get more casual language
        if (personality.Traits.Friendliness > 0.6f)
        {
            var friendlyAdditions = new[] { ", my friend", ", pal", ", buddy" };
            if (_random.NextDouble() < 0.3)
            {
                text = text.TrimEnd('.', '!', '?') + friendlyAdditions[_random.Next(friendlyAdditions.Length)] + ".";
            }
        }

        return text;
    }

    private string ApplyNeutralRelationshipTone(string text, NPCPersonality personality)
    {
        // Neutral tone - no major modifications
        return text;
    }

    private string ApplyColdRelationshipTone(string text, NPCPersonality personality)
    {
        // Remove warmth
        text = text.Replace("my friend", "");
        text = text.Replace("friend", "");
        text = text.Replace("!", ".");

        // Make more formal
        if (personality.Traits.Formality < 0.5f)
        {
            text = text.Replace("yeah", "yes");
            text = text.Replace("nope", "no");
        }

        return text.Trim();
    }

    private string ApplyHostileRelationshipTone(string text, NPCPersonality personality)
    {
        // Hostile tone - curt and unfriendly
        text = text.Replace("please", "");
        text = text.Replace("my friend", "");
        text = text.Replace("friend", "you");

        // Shorten sentences
        var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        if (sentences.Length > 1)
        {
            text = sentences[0].Trim() + ".";
        }

        return text;
    }

    #endregion

    /// <summary>
    /// Combines all tone modifications for comprehensive dialogue generation.
    /// </summary>
    public string ApplyCompleteTone(
        Entity npcEntity,
        string baseText,
        DialogueEmotion emotion,
        float emotionIntensity,
        float relationshipValue,
        DialogueType dialogueType)
    {
        if (!npcEntity.Has<NPCPersonality>())
            return baseText;

        var personality = npcEntity.Get<NPCPersonality>();
        var context = npcEntity.Has<DialogueContext>()
            ? npcEntity.Get<DialogueContext>()
            : DialogueContext.Create();

        // Apply contextual personality shifts
        var adjustedPersonality = ApplyContextualShift(personality, context, dialogueType);

        // Apply emotional tone
        var text = ApplyEmotionalTone(baseText, emotion, adjustedPersonality, emotionIntensity);

        // Apply relationship tone
        text = ApplyRelationshipTone(text, adjustedPersonality, relationshipValue, context.TotalDialogueCount);

        return text;
    }
}
