using System;
using System.Collections.Generic;

namespace LablabBean.Plugins.NPC.Components;

/// <summary>
/// Tracks dialogue context and conversation history for an NPC.
/// Used to maintain conversation flow and avoid repetition.
/// </summary>
public struct DialogueContext
{
    /// <summary>
    /// Current type of dialogue being conducted.
    /// </summary>
    public DialogueType CurrentType { get; set; }

    /// <summary>
    /// Current topic being discussed (if any).
    /// </summary>
    public string? CurrentTopic { get; set; }

    /// <summary>
    /// Last time this NPC engaged in dialogue.
    /// </summary>
    public DateTime LastDialogue { get; set; }

    /// <summary>
    /// Total number of dialogue exchanges with all players.
    /// </summary>
    public int TotalDialogueCount { get; set; }

    /// <summary>
    /// Number of times each topic has been mentioned.
    /// </summary>
    public Dictionary<string, int> TopicMentions { get; set; }

    /// <summary>
    /// Recent dialogue history (last N exchanges).
    /// </summary>
    public List<DialogueExchange> RecentHistory { get; set; }

    /// <summary>
    /// Maximum number of recent exchanges to keep.
    /// </summary>
    public int MaxHistorySize { get; set; }

    /// <summary>
    /// Creates a new DialogueContext with default values.
    /// </summary>
    public static DialogueContext Create()
    {
        return new DialogueContext
        {
            CurrentType = DialogueType.Greeting,
            CurrentTopic = null,
            LastDialogue = DateTime.MinValue,
            TotalDialogueCount = 0,
            TopicMentions = new Dictionary<string, int>(),
            RecentHistory = new List<DialogueExchange>(),
            MaxHistorySize = 10
        };
    }

    /// <summary>
    /// Records a new dialogue exchange.
    /// </summary>
    public void RecordExchange(int speakerId, string text, DialogueType type, string? topic = null)
    {
        var exchange = new DialogueExchange
        {
            Timestamp = DateTime.UtcNow,
            SpeakerId = speakerId,
            Text = text,
            Type = type,
            Topic = topic
        };

        RecentHistory ??= new List<DialogueExchange>();
        RecentHistory.Add(exchange);

        // Trim history if too long
        if (RecentHistory.Count > MaxHistorySize)
        {
            RecentHistory.RemoveAt(0);
        }

        // Update topic mentions
        if (!string.IsNullOrEmpty(topic))
        {
            TopicMentions ??= new Dictionary<string, int>();
            TopicMentions[topic] = TopicMentions.GetValueOrDefault(topic, 0) + 1;
        }

        // Update current state
        CurrentType = type;
        CurrentTopic = topic;
        LastDialogue = DateTime.UtcNow;
        TotalDialogueCount++;
    }

    /// <summary>
    /// Gets the most discussed topics (sorted by mention count).
    /// </summary>
    public readonly List<string> GetTopTopics(int count = 3)
    {
        if (TopicMentions == null || TopicMentions.Count == 0)
            return new List<string>();

        var sortedTopics = new List<KeyValuePair<string, int>>(TopicMentions);
        sortedTopics.Sort((a, b) => b.Value.CompareTo(a.Value));

        var result = new List<string>();
        for (int i = 0; i < Math.Min(count, sortedTopics.Count); i++)
        {
            result.Add(sortedTopics[i].Key);
        }
        return result;
    }

    /// <summary>
    /// Checks if a topic was recently discussed (within last N exchanges).
    /// </summary>
    public readonly bool WasRecentlyDiscussed(string topic, int withinLastN = 5)
    {
        if (RecentHistory == null || RecentHistory.Count == 0)
            return false;

        int checkCount = Math.Min(withinLastN, RecentHistory.Count);
        for (int i = RecentHistory.Count - 1; i >= RecentHistory.Count - checkCount; i--)
        {
            if (RecentHistory[i].Topic == topic)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Gets time since last dialogue.
    /// </summary>
    public readonly TimeSpan TimeSinceLastDialogue()
    {
        if (LastDialogue == DateTime.MinValue)
            return TimeSpan.MaxValue;

        return DateTime.UtcNow - LastDialogue;
    }

    /// <summary>
    /// Determines if this is the first conversation.
    /// </summary>
    public readonly bool IsFirstConversation()
    {
        return TotalDialogueCount == 0;
    }

    /// <summary>
    /// Gets recent exchanges with a specific speaker.
    /// </summary>
    public readonly List<DialogueExchange> GetRecentExchangesWith(int speakerId, int count = 5)
    {
        if (RecentHistory == null)
            return new List<DialogueExchange>();

        var result = new List<DialogueExchange>();
        for (int i = RecentHistory.Count - 1; i >= 0 && result.Count < count; i--)
        {
            if (RecentHistory[i].SpeakerId == speakerId)
            {
                result.Add(RecentHistory[i]);
            }
        }
        result.Reverse();
        return result;
    }
}

/// <summary>
/// Records a single dialogue exchange.
/// </summary>
public struct DialogueExchange
{
    /// <summary>
    /// When the exchange occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// ID of the speaker (NPC or player).
    /// </summary>
    public int SpeakerId { get; set; }

    /// <summary>
    /// The dialogue text.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Type of dialogue.
    /// </summary>
    public DialogueType Type { get; set; }

    /// <summary>
    /// Topic discussed (if applicable).
    /// </summary>
    public string? Topic { get; set; }
}

/// <summary>
/// Type of dialogue being conducted.
/// </summary>
public enum DialogueType
{
    /// <summary>
    /// Initial greeting when meeting.
    /// </summary>
    Greeting,

    /// <summary>
    /// Parting words when leaving.
    /// </summary>
    Farewell,

    /// <summary>
    /// Combat-related dialogue (taunts, threats, etc.).
    /// </summary>
    Combat,

    /// <summary>
    /// Trading/merchant dialogue.
    /// </summary>
    Trading,

    /// <summary>
    /// Quest-related dialogue.
    /// </summary>
    Quest,

    /// <summary>
    /// General conversation.
    /// </summary>
    Casual,

    /// <summary>
    /// Intimidation or threats.
    /// </summary>
    Intimidation,

    /// <summary>
    /// Persuasion or diplomacy.
    /// </summary>
    Persuasion,

    /// <summary>
    /// Sharing information or lore.
    /// </summary>
    Information,

    /// <summary>
    /// Asking for help or assistance.
    /// </summary>
    RequestHelp,

    /// <summary>
    /// Offering help or assistance.
    /// </summary>
    OfferHelp
}

/// <summary>
/// Emotional tone of dialogue.
/// </summary>
public enum DialogueEmotion
{
    /// <summary>
    /// Neutral, calm tone.
    /// </summary>
    Neutral,

    /// <summary>
    /// Happy, cheerful tone.
    /// </summary>
    Happy,

    /// <summary>
    /// Angry, hostile tone.
    /// </summary>
    Angry,

    /// <summary>
    /// Sad, melancholy tone.
    /// </summary>
    Sad,

    /// <summary>
    /// Fearful, scared tone.
    /// </summary>
    Fearful,

    /// <summary>
    /// Surprised, shocked tone.
    /// </summary>
    Surprised,

    /// <summary>
    /// Contemptuous, disdainful tone.
    /// </summary>
    Contemptuous,

    /// <summary>
    /// Excited, enthusiastic tone.
    /// </summary>
    Excited,

    /// <summary>
    /// Cautious, wary tone.
    /// </summary>
    Cautious,

    /// <summary>
    /// Friendly, warm tone.
    /// </summary>
    Friendly
}
