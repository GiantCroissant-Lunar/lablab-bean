using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.NPC.Components;

namespace LablabBean.Plugins.NPC.Systems;

/// <summary>
/// System that applies personality traits to dialogue generation.
/// Modifies dialogue text based on NPC personality, emotional state, and conversation context.
/// </summary>
public class PersonalityDialogueSystem
{
    private readonly World _world;
    private readonly Random _random = new();

    public PersonalityDialogueSystem(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Generates dialogue for an NPC based on their personality and context.
    /// </summary>
    public string GenerateDialogue(
        Entity npcEntity,
        string baseText,
        DialogueType dialogueType,
        DialogueEmotion emotion = DialogueEmotion.Neutral)
    {
        if (!npcEntity.Has<NPCPersonality>())
            return baseText;

        var personality = npcEntity.Get<NPCPersonality>();
        var context = npcEntity.Has<DialogueContext>()
            ? npcEntity.Get<DialogueContext>()
            : DialogueContext.Create();

        // Apply personality transformations
        var text = ApplyPersonalityStyle(baseText, personality, emotion);
        text = AdjustLength(text, personality.Traits.Chattiness);
        text = AddPersonalityFlavor(text, personality, emotion);

        return text;
    }

    /// <summary>
    /// Applies dialogue style based on personality.
    /// </summary>
    private string ApplyPersonalityStyle(string text, NPCPersonality personality, DialogueEmotion emotion)
    {
        return personality.Style switch
        {
            DialogueStyle.Formal => MakeFormal(text, personality.Traits.Formality),
            DialogueStyle.Casual => MakeCasual(text, personality.Traits.Friendliness),
            DialogueStyle.Gruff => MakeGruff(text, personality.Traits.Courage),
            DialogueStyle.Flowery => MakeFlowery(text, personality.Traits.Formality),
            DialogueStyle.Minimal => MakeMinimal(text, personality.Traits.Chattiness),
            _ => text
        };
    }

    /// <summary>
    /// Makes text more formal.
    /// </summary>
    private string MakeFormal(string text, float formality)
    {
        if (formality < 0.5f) return text;

        var replacements = new Dictionary<string, string>
        {
            { "hey", "greetings" },
            { "hi", "hello" },
            { "yeah", "yes" },
            { "nope", "no" },
            { "wanna", "wish to" },
            { "gonna", "going to" },
            { "gotta", "must" },
            { "dunno", "do not know" },
            { "kinda", "somewhat" },
            { "sorta", "rather" }
        };

        foreach (var (informal, formal) in replacements)
        {
            if (_random.NextDouble() < formality)
            {
                text = text.Replace(informal, formal, StringComparison.OrdinalIgnoreCase);
            }
        }

        return text;
    }

    /// <summary>
    /// Makes text more casual.
    /// </summary>
    private string MakeCasual(string text, float friendliness)
    {
        if (friendliness < 0.5f) return text;

        var replacements = new Dictionary<string, string>
        {
            { "greetings", "hey" },
            { "hello", "hi" },
            { "farewell", "see ya" },
            { "certainly", "sure" },
            { "perhaps", "maybe" },
            { "indeed", "yep" }
        };

        foreach (var (formal, casual) in replacements)
        {
            if (_random.NextDouble() < friendliness)
            {
                text = text.Replace(formal, casual, StringComparison.OrdinalIgnoreCase);
            }
        }

        return text;
    }

    /// <summary>
    /// Makes text more gruff and abrupt.
    /// </summary>
    private string MakeGruff(string text, float courage)
    {
        if (courage < 0.7f) return text;

        // Remove pleasantries
        text = text.Replace("please", "", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("kindly", "", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("if you don't mind", "", StringComparison.OrdinalIgnoreCase);

        // Shorten sentences (remove some conjunctions)
        if (_random.NextDouble() < 0.5)
        {
            text = text.Replace(", and ", ". ", StringComparison.OrdinalIgnoreCase);
            text = text.Replace(", but ", ". ", StringComparison.OrdinalIgnoreCase);
        }

        return text.Trim();
    }

    /// <summary>
    /// Makes text more flowery and elaborate.
    /// </summary>
    private string MakeFlowery(string text, float formality)
    {
        if (formality < 0.7f) return text;

        var embellishments = new[]
        {
            ("hello", "most cordial greetings"),
            ("goodbye", "I bid you farewell"),
            ("yes", "indeed, most certainly"),
            ("no", "I regret to inform you, no"),
            ("thanks", "my deepest gratitude"),
            ("good", "most excellent"),
            ("bad", "most unfortunate"),
            ("help", "render assistance")
        };

        foreach (var (simple, elaborate) in embellishments)
        {
            if (_random.NextDouble() < formality && text.Contains(simple, StringComparison.OrdinalIgnoreCase))
            {
                text = text.Replace(simple, elaborate, StringComparison.OrdinalIgnoreCase);
                break; // Only apply one embellishment to avoid overdoing it
            }
        }

        return text;
    }

    /// <summary>
    /// Makes text more minimal and terse.
    /// </summary>
    private string MakeMinimal(string text, float chattiness)
    {
        if (chattiness > 0.3f) return text;

        // Split into sentences
        var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

        if (sentences.Length == 0) return text;

        // Keep only first sentence for very low chattiness
        if (chattiness < 0.2f)
        {
            return sentences[0].Trim() + ".";
        }

        // Keep first 2 sentences for low chattiness
        if (sentences.Length > 2)
        {
            return string.Join(". ", sentences.Take(2)) + ".";
        }

        return text;
    }

    /// <summary>
    /// Adjusts dialogue length based on chattiness trait.
    /// </summary>
    private string AdjustLength(string text, float chattiness)
    {
        var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        if (sentences.Count == 0) return text;

        // Very terse (0.0 - 0.2): 1 sentence
        if (chattiness < 0.2f)
            return sentences[0] + GetPunctuation(text);

        // Terse (0.2 - 0.4): 1-2 sentences
        if (chattiness < 0.4f)
            return string.Join(". ", sentences.Take(Math.Min(2, sentences.Count))) + GetPunctuation(text);

        // Normal (0.4 - 0.6): keep as is
        if (chattiness < 0.6f)
            return text;

        // Chatty (0.6 - 0.8): keep all sentences
        if (chattiness < 0.8f)
            return text;

        // Very chatty (0.8 - 1.0): add extra elaboration
        return AddElaboration(text, sentences);
    }

    /// <summary>
    /// Gets appropriate punctuation from original text.
    /// </summary>
    private static string GetPunctuation(string text)
    {
        if (text.EndsWith('!')) return "!";
        if (text.EndsWith('?')) return "?";
        return ".";
    }

    /// <summary>
    /// Adds extra elaboration for very chatty NPCs.
    /// </summary>
    private string AddElaboration(string text, List<string> sentences)
    {
        var elaborations = new[]
        {
            "You know what I mean?",
            "If you ask me.",
            "That's what I always say.",
            "Between you and me.",
            "Just thought I'd mention it.",
            "Mind you.",
            "As it were."
        };

        if (_random.NextDouble() < 0.5)
        {
            var elaboration = elaborations[_random.Next(elaborations.Length)];
            return text + " " + elaboration;
        }

        return text;
    }

    /// <summary>
    /// Adds personality-specific flavor based on traits and emotion.
    /// </summary>
    private string AddPersonalityFlavor(string text, NPCPersonality personality, DialogueEmotion emotion)
    {
        var sb = new StringBuilder(text);

        // Add emotional indicators based on emotional range
        if (personality.EmotionalRange == EmotionalRange.Expressive)
        {
            text = AddEmotionalIndicators(text, emotion);
        }

        // Add personality-based interjections
        if (_random.NextDouble() < 0.3) // 30% chance
        {
            text = AddInterjection(text, personality, emotion);
        }

        return text;
    }

    /// <summary>
    /// Adds emotional indicators for expressive NPCs.
    /// </summary>
    private string AddEmotionalIndicators(string text, DialogueEmotion emotion)
    {
        var indicators = emotion switch
        {
            DialogueEmotion.Happy => new[] { "*smiles*", "*grins*", "*chuckles*" },
            DialogueEmotion.Angry => new[] { "*frowns*", "*scowls*", "*glares*" },
            DialogueEmotion.Sad => new[] { "*sighs*", "*looks down*", "*frowns sadly*" },
            DialogueEmotion.Fearful => new[] { "*nervous*", "*looks around*", "*trembles*" },
            DialogueEmotion.Surprised => new[] { "*eyes widen*", "*gasps*", "*blinks*" },
            DialogueEmotion.Excited => new[] { "*beams*", "*bounces*", "*claps hands*" },
            _ => Array.Empty<string>()
        };

        if (indicators.Length > 0 && _random.NextDouble() < 0.4)
        {
            var indicator = indicators[_random.Next(indicators.Length)];
            return $"{indicator} {text}";
        }

        return text;
    }

    /// <summary>
    /// Adds personality-based interjections.
    /// </summary>
    private string AddInterjection(string text, NPCPersonality personality, DialogueEmotion emotion)
    {
        List<string> interjections = new();

        // Friendly interjections
        if (personality.Traits.Friendliness > 0.7f)
        {
            interjections.AddRange(new[] { "Friend", "Pal", "Buddy" });
        }

        // Gruff interjections
        if (personality.Traits.Courage > 0.7f && personality.Traits.Friendliness < 0.4f)
        {
            interjections.AddRange(new[] { "Bah", "Hmph", "Aye" });
        }

        // Formal interjections
        if (personality.Traits.Formality > 0.7f)
        {
            interjections.AddRange(new[] { "Indeed", "Certainly", "I say" });
        }

        // Dishonest interjections
        if (personality.Traits.Honesty < 0.3f)
        {
            interjections.AddRange(new[] { "Trust me", "Believe me", "Honestly" });
        }

        if (interjections.Count == 0) return text;

        var interjection = interjections[_random.Next(interjections.Count)];

        // Add to beginning or end
        return _random.NextDouble() < 0.5
            ? $"{interjection}, {text}"
            : $"{text}, {interjection.ToLower()}.";
    }

    /// <summary>
    /// Determines appropriate greeting based on personality and context.
    /// </summary>
    public string GenerateGreeting(Entity npcEntity, Entity playerEntity)
    {
        if (!npcEntity.Has<NPCPersonality>())
            return "Hello.";

        var personality = npcEntity.Get<NPCPersonality>();
        var context = npcEntity.Has<DialogueContext>()
            ? npcEntity.Get<DialogueContext>()
            : DialogueContext.Create();

        // First meeting
        if (context.IsFirstConversation())
        {
            return GenerateFirstGreeting(personality);
        }

        // Recent conversation (within 5 minutes)
        if (context.TimeSinceLastDialogue().TotalMinutes < 5)
        {
            return GenerateRecentGreeting(personality);
        }

        // Normal greeting
        return GenerateNormalGreeting(personality);
    }

    private string GenerateFirstGreeting(NPCPersonality personality)
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

    private string GenerateRecentGreeting(NPCPersonality personality)
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

    private string GenerateNormalGreeting(NPCPersonality personality)
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

    /// <summary>
    /// Determines appropriate farewell based on personality.
    /// </summary>
    public string GenerateFarewell(Entity npcEntity)
    {
        if (!npcEntity.Has<NPCPersonality>())
            return "Goodbye.";

        var personality = npcEntity.Get<NPCPersonality>();
        var friendliness = personality.Traits.Friendliness;
        var formality = personality.Traits.Formality;

        if (friendliness > 0.7f && formality < 0.3f)
            return "See ya later!";

        if (friendliness > 0.7f && formality > 0.7f)
            return "Farewell, my friend. Safe travels.";

        if (friendliness < 0.3f && formality < 0.3f)
            return "Later.";

        if (friendliness < 0.3f && formality > 0.7f)
            return "I bid you good day.";

        return "Goodbye.";
    }

    /// <summary>
    /// Updates dialogue context after an exchange.
    /// </summary>
    public void RecordDialogueExchange(
        Entity npcEntity,
        Entity speakerEntity,
        string text,
        DialogueType type,
        string? topic = null)
    {
        if (!npcEntity.Has<DialogueContext>())
        {
            npcEntity.Add(DialogueContext.Create());
        }

        ref var context = ref npcEntity.Get<DialogueContext>();
        context.RecordExchange(speakerEntity.Id, text, type, topic);
    }
}
