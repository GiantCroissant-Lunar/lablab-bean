using LablabBean.Contracts.UI.Models;
using SadRogue.Primitives;

namespace LablabBean.Game.Core.Components;

public struct ActivityLog
{
    public int Capacity { get; set; }
    public List<ActivityEntry> Entries { get; set; }
    public long Sequence { get; set; }

    public ActivityLog(int capacity = 200)
    {
        Capacity = capacity;
        Entries = new List<ActivityEntry>(capacity);
        Sequence = 0;
    }

    public void Add(ActivityEntry entry)
    {
        Entries.Add(entry);
        if (Entries.Count > Capacity)
        {
            var remove = Entries.Count - Capacity;
            Entries.RemoveRange(0, remove);
        }
        Sequence++;
    }
}

