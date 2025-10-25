using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LablabBean.Plugins.NPC.Components;

namespace LablabBean.Plugins.NPC.Utilities;

/// <summary>
/// Utility for testing and debugging personality-driven dialogue generation.
/// Helps visualize how different personalities affect dialogue output.
/// </summary>
public static class PersonalityDialogueTestUtility
{
    /// <summary>
    /// Generates a test report showing how a single text is transformed by different personalities.
    /// </summary>
    public static string GeneratePersonalityComparisonReport(
        string baseText,
        IEnumerable<(string name, NPCPersonality personality)> personalities)
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine("   PERSONALITY DIALOGUE COMPARISON REPORT");
        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine($"Base Text: \"{baseText}\"");
        sb.AppendLine();
        sb.AppendLine("───────────────────────────────────────────────────────────");

        foreach (var (name, personality) in personalities)
        {
            sb.AppendLine();
            sb.AppendLine($"🎭 {name}");
            sb.AppendLine($"   Style: {personality.Style} | Range: {personality.EmotionalRange}");
            sb.AppendLine($"   Traits: F={personality.Traits.Friendliness:F1} " +
                         $"C={personality.Traits.Courage:F1} " +
                         $"Fm={personality.Traits.Formality:F1} " +
                         $"H={personality.Traits.Honesty:F1} " +
                         $"Ch={personality.Traits.Chattiness:F1} " +
                         $"G={personality.Traits.Greed:F1}");
            sb.AppendLine($"   Output: \"{TransformText(baseText, personality)}\"");
        }

        sb.AppendLine();
        sb.AppendLine("═══════════════════════════════════════════════════════════");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a greeting comparison report for different personalities.
    /// </summary>
    public static string GenerateGreetingComparisonReport(
        IEnumerable<(string name, NPCPersonality personality)> personalities)
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine("   PERSONALITY GREETING COMPARISON");
        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine();

        foreach (var (name, personality) in personalities)
        {
            var firstGreeting = GenerateFirstGreeting(personality);
            var recentGreeting = GenerateRecentGreeting(personality);
            var normalGreeting = GenerateNormalGreeting(personality);

            sb.AppendLine($"🎭 {name}");
            sb.AppendLine($"   First Meeting:  \"{firstGreeting}\"");
            sb.AppendLine($"   Recent Return:  \"{recentGreeting}\"");
            sb.AppendLine($"   Normal Meeting: \"{normalGreeting}\"");
            sb.AppendLine();
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════");
        return sb.ToString();
    }

    /// <summary>
    /// Generates an emotion comparison report for a single personality.
    /// </summary>
    public static string GenerateEmotionComparisonReport(
        string baseText,
        NPCPersonality personality,
        string personalityName = "NPC")
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine($"   EMOTION COMPARISON: {personalityName}");
        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine($"Base Text: \"{baseText}\"");
        sb.AppendLine($"Emotional Range: {personality.EmotionalRange}");
        sb.AppendLine();

        var emotions = Enum.GetValues<DialogueEmotion>();
        foreach (var emotion in emotions)
        {
            if (emotion == DialogueEmotion.Neutral) continue;

            sb.AppendLine($"😊 {emotion}:");
            sb.AppendLine($"   \"{ApplyEmotionToText(baseText, emotion, personality)}\"");
            sb.AppendLine();
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════");
        return sb.ToString();
    }

    /// <summary>
    /// Generates a trait spectrum report showing how changing a single trait affects output.
    /// </summary>
    public static string GenerateTraitSpectrumReport(
        string baseText,
        NPCPersonality basePersonality,
        string traitName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine($"   TRAIT SPECTRUM: {traitName.ToUpper()}");
        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine($"Base Text: \"{baseText}\"");
        sb.AppendLine();

        var values = new[] { 0.0f, 0.25f, 0.5f, 0.75f, 1.0f };
        foreach (var value in values)
        {
            var personality = basePersonality;
            var traits = personality.Traits;

            // Modify the specified trait
            switch (traitName.ToLowerInvariant())
            {
                case "friendliness":
                    traits.Friendliness = value;
                    break;
                case "courage":
                    traits.Courage = value;
                    break;
                case "formality":
                    traits.Formality = value;
                    break;
                case "honesty":
                    traits.Honesty = value;
                    break;
                case "chattiness":
                    traits.Chattiness = value;
                    break;
                case "greed":
                    traits.Greed = value;
                    break;
                default:
                    continue;
            }

            personality.Traits = traits;
            var transformed = TransformText(baseText, personality);

            sb.AppendLine($"{traitName} = {value:F2}:");
            sb.AppendLine($"   \"{transformed}\"");
            sb.AppendLine();
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════");
        return sb.ToString();
    }

    /// <summary>
    /// Validates that a personality's traits are within valid ranges.
    /// </summary>
    public static (bool isValid, List<string> errors) ValidatePersonality(NPCPersonality personality)
    {
        var errors = new List<string>();

        if (!personality.Traits.IsValid())
        {
            errors.Add("One or more traits are outside valid range (0.0-1.0)");

            if (personality.Traits.Friendliness < 0 || personality.Traits.Friendliness > 1)
                errors.Add($"Friendliness: {personality.Traits.Friendliness}");
            if (personality.Traits.Courage < 0 || personality.Traits.Courage > 1)
                errors.Add($"Courage: {personality.Traits.Courage}");
            if (personality.Traits.Formality < 0 || personality.Traits.Formality > 1)
                errors.Add($"Formality: {personality.Traits.Formality}");
            if (personality.Traits.Honesty < 0 || personality.Traits.Honesty > 1)
                errors.Add($"Honesty: {personality.Traits.Honesty}");
            if (personality.Traits.Chattiness < 0 || personality.Traits.Chattiness > 1)
                errors.Add($"Chattiness: {personality.Traits.Chattiness}");
            if (personality.Traits.Greed < 0 || personality.Traits.Greed > 1)
                errors.Add($"Greed: {personality.Traits.Greed}");
        }

        if (personality.CustomTraits != null)
        {
            foreach (var (name, value) in personality.CustomTraits)
            {
                if (value < 0 || value > 1)
                {
                    errors.Add($"Custom trait '{name}': {value} (outside 0.0-1.0 range)");
                }
            }
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Creates a summary string describing a personality's key characteristics.
    /// </summary>
    public static string GetPersonalitySummary(NPCPersonality personality)
    {
        var traits = personality.Traits;
        var descriptions = new List<string>();

        // Analyze each trait
        if (traits.Friendliness > 0.7f) descriptions.Add("very friendly");
        else if (traits.Friendliness < 0.3f) descriptions.Add("unfriendly");

        if (traits.Courage > 0.7f) descriptions.Add("brave");
        else if (traits.Courage < 0.3f) descriptions.Add("cowardly");

        if (traits.Formality > 0.7f) descriptions.Add("formal");
        else if (traits.Formality < 0.3f) descriptions.Add("casual");

        if (traits.Honesty > 0.7f) descriptions.Add("honest");
        else if (traits.Honesty < 0.3f) descriptions.Add("dishonest");

        if (traits.Chattiness > 0.7f) descriptions.Add("talkative");
        else if (traits.Chattiness < 0.3f) descriptions.Add("terse");

        if (traits.Greed > 0.7f) descriptions.Add("greedy");
        else if (traits.Greed < 0.3f) descriptions.Add("generous");

        var summary = descriptions.Count > 0
            ? string.Join(", ", descriptions)
            : "neutral";

        return $"{personality.Style} style, {personality.EmotionalRange.ToString().ToLower()} emotions, {summary}";
    }

    #region Helper Methods

    private static string TransformText(string text, NPCPersonality personality)
    {
        // Simplified transformation for testing
        var result = text;

        // Apply style
        if (personality.Style == DialogueStyle.Formal && personality.Traits.Formality > 0.5f)
        {
            result = result.Replace("hey", "greetings")
                          .Replace("yeah", "yes")
                          .Replace("nope", "no");
        }
        else if (personality.Style == DialogueStyle.Casual && personality.Traits.Friendliness > 0.5f)
        {
            result = result.Replace("hello", "hey")
                          .Replace("yes", "yeah");
        }
        else if (personality.Style == DialogueStyle.Gruff)
        {
            result = result.Replace("please", "").Trim();
        }

        // Apply chattiness
        if (personality.Traits.Chattiness < 0.3f)
        {
            var sentences = result.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (sentences.Length > 0)
            {
                result = sentences[0].Trim() + ".";
            }
        }

        return result;
    }

    private static string GenerateFirstGreeting(NPCPersonality personality)
    {
        var friendliness = personality.Traits.Friendliness;
        var formality = personality.Traits.Formality;

        if (friendliness > 0.7f && formality < 0.3f)
            return "Hey there! Don't think we've met before.";
        if (friendliness > 0.7f && formality > 0.7f)
            return "Greetings, traveler. A pleasure to make your acquaintance.";
        if (friendliness < 0.3f && formality < 0.3f)
            return "What do you want?";
        if (friendliness < 0.3f && formality > 0.7f)
            return "State your business.";

        return "Hello. Can I help you?";
    }

    private static string GenerateRecentGreeting(NPCPersonality personality)
    {
        var friendliness = personality.Traits.Friendliness;
        var chattiness = personality.Traits.Chattiness;

        if (friendliness > 0.7f && chattiness > 0.7f)
            return "Back so soon? What can I do for you?";
        if (friendliness > 0.7f)
            return "Hello again!";
        if (friendliness < 0.3f && chattiness < 0.3f)
            return "You again?";
        if (friendliness < 0.3f)
            return "Back already?";

        return "Yes?";
    }

    private static string GenerateNormalGreeting(NPCPersonality personality)
    {
        var friendliness = personality.Traits.Friendliness;
        var formality = personality.Traits.Formality;

        if (friendliness > 0.7f && formality < 0.3f)
            return "Hey! Good to see you again!";
        if (friendliness > 0.7f && formality > 0.7f)
            return "Welcome back, friend. How may I assist you?";
        if (friendliness < 0.3f && formality < 0.3f)
            return "Yeah?";
        if (friendliness < 0.3f && formality > 0.7f)
            return "You have returned.";

        return "Hello again.";
    }

    private static string ApplyEmotionToText(string text, DialogueEmotion emotion, NPCPersonality personality)
    {
        var intensity = personality.EmotionalRange switch
        {
            EmotionalRange.Stoic => 0.3f,
            EmotionalRange.Stable => 0.6f,
            EmotionalRange.Volatile => 1.2f,
            EmotionalRange.Expressive => 1.0f,
            _ => 0.6f
        };

        var prefix = emotion switch
        {
            DialogueEmotion.Happy => "Good! ",
            DialogueEmotion.Angry => "Listen here! ",
            DialogueEmotion.Sad => "*sighs* ",
            DialogueEmotion.Fearful => "Oh dear... ",
            DialogueEmotion.Surprised => "What?! ",
            DialogueEmotion.Contemptuous => "Hmph. ",
            DialogueEmotion.Excited => "Amazing! ",
            DialogueEmotion.Cautious => "Perhaps... ",
            DialogueEmotion.Friendly => "Friend! ",
            _ => ""
        };

        if (intensity < 0.5f)
        {
            return text;
        }

        return prefix + text;
    }

    #endregion
}
