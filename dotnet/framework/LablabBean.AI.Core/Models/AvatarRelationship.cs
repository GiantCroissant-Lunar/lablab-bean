namespace LablabBean.AI.Core.Models;

/// <summary>
/// Relationship between avatars
/// </summary>
public class AvatarRelationship
{
    public string EntityId { get; set; } = string.Empty;
    public string TargetEntityId { get; set; } = string.Empty;
    public float Affinity { get; set; } = 0f;
    public string RelationshipType { get; set; } = "Neutral";
    public List<string> SharedHistory { get; set; } = new();
    public DateTime LastInteraction { get; set; } = DateTime.UtcNow;

    public void AdjustAffinity(float delta)
    {
        Affinity = Math.Clamp(Affinity + delta, -100f, 100f);
        UpdateRelationshipType();
    }

    private void UpdateRelationshipType()
    {
        RelationshipType = Affinity switch
        {
            >= 50f => "Ally",
            >= 20f => "Friendly",
            >= -20f => "Neutral",
            >= -50f => "Hostile",
            _ => "Enemy"
        };
    }

    public void RecordInteraction(string eventDescription)
    {
        SharedHistory.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {eventDescription}");
        LastInteraction = DateTime.UtcNow;

        if (SharedHistory.Count > 20)
        {
            SharedHistory.RemoveAt(0);
        }
    }
}
