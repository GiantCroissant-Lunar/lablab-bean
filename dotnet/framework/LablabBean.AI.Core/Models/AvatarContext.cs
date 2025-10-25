namespace LablabBean.AI.Core.Models;

/// <summary>
/// Context information about an avatar for AI decision-making
/// </summary>
public class AvatarContext
{
    public string EntityId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PersonalityProfile { get; set; } = string.Empty;
    public Dictionary<string, object> CurrentState { get; set; } = new();
    public List<string> NearbyEntities { get; set; } = new();
    public Dictionary<string, float> EnvironmentFactors { get; set; } = new();

    public void AddStateValue(string key, object value)
    {
        CurrentState[key] = value;
    }

    public T? GetStateValue<T>(string key)
    {
        if (CurrentState.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }
}
