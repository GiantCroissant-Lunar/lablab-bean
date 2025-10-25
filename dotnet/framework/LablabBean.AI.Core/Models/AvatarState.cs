namespace LablabBean.AI.Core.Models;

/// <summary>
/// Current state of an avatar
/// </summary>
public class AvatarState
{
    public string EntityId { get; set; } = string.Empty;
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public string CurrentBehavior { get; set; } = "Idle";
    public string EmotionalState { get; set; } = "Neutral";
    public Dictionary<string, float> Stats { get; set; } = new();
    public List<string> ActiveEffects { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public float HealthPercentage => MaxHealth > 0 ? Health / MaxHealth : 0f;

    public bool IsAlive => Health > 0;

    public void UpdateHealth(float delta)
    {
        Health = Math.Clamp(Health + delta, 0, MaxHealth);
        LastUpdated = DateTime.UtcNow;
    }
}
