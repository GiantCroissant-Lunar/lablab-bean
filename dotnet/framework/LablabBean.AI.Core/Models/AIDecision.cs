namespace LablabBean.AI.Core.Models;

/// <summary>
/// AI decision result
/// </summary>
public class AIDecision
{
    public string DecisionType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string Reasoning { get; set; } = string.Empty;
    public float Confidence { get; set; } = 1.0f;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public T? GetParameter<T>(string key)
    {
        if (Parameters.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    public void SetParameter(string key, object value)
    {
        Parameters[key] = value;
    }
}
