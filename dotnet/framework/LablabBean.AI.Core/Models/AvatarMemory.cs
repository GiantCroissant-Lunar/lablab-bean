namespace LablabBean.AI.Core.Models;

/// <summary>
/// Memory entry for an avatar
/// </summary>
public class MemoryEntry
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public float Importance { get; set; } = 1.0f;
}

/// <summary>
/// Memory storage for an avatar
/// </summary>
public class AvatarMemory
{
    public string EntityId { get; set; } = string.Empty;
    public List<MemoryEntry> ShortTermMemory { get; set; } = new();
    public List<MemoryEntry> LongTermMemory { get; set; } = new();
    public Dictionary<string, int> InteractionCounts { get; set; } = new();
    public int MaxShortTermMemories { get; set; } = 10;
    public int MaxLongTermMemories { get; set; } = 50;

    public void AddMemory(MemoryEntry entry)
    {
        ShortTermMemory.Insert(0, entry);

        if (ShortTermMemory.Count > MaxShortTermMemories)
        {
            var oldest = ShortTermMemory.Last();
            ShortTermMemory.RemoveAt(ShortTermMemory.Count - 1);

            if (oldest.Importance >= 0.7f)
            {
                LongTermMemory.Insert(0, oldest);
                if (LongTermMemory.Count > MaxLongTermMemories)
                {
                    LongTermMemory.RemoveAt(LongTermMemory.Count - 1);
                }
            }
        }
    }

    public void RecordInteraction(string entityId)
    {
        if (!InteractionCounts.ContainsKey(entityId))
        {
            InteractionCounts[entityId] = 0;
        }
        InteractionCounts[entityId]++;
    }

    public List<MemoryEntry> GetRecentMemories(int count = 5)
    {
        return ShortTermMemory.Take(count).ToList();
    }
}
