namespace LablabBean.AI.Core.Models;

/// <summary>
/// Context for dialogue generation
/// </summary>
public class DialogueContext
{
    public string SpeakerId { get; set; } = string.Empty;
    public string ListenerId { get; set; } = string.Empty;
    public string SpeakerPersonality { get; set; } = string.Empty;
    public string ConversationTopic { get; set; } = string.Empty;
    public List<string> ConversationHistory { get; set; } = new();
    public string SpeakerEmotionalState { get; set; } = "Neutral";
    public Dictionary<string, object> ContextVariables { get; set; } = new();
    public DateTime ConversationStartTime { get; set; } = DateTime.UtcNow;

    public void AddMessage(string speaker, string message)
    {
        ConversationHistory.Add($"{speaker}: {message}");

        if (ConversationHistory.Count > 10)
        {
            ConversationHistory.RemoveAt(0);
        }
    }

    public string GetConversationSummary()
    {
        return string.Join("\n", ConversationHistory);
    }
}
