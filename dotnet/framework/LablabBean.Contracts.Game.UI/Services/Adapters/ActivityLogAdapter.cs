using System;
using System.Collections.Generic;
using LablabBean.Contracts.Game.UI.Models;
using LablabBean.Contracts.Game.UI.Services;
using Old = LablabBean.Contracts.UI.Services;

namespace LablabBean.Contracts.Game.UI.Services.Adapters;

/// <summary>
/// Adapts the existing UI-layer IActivityLogService to the game-specific IActivityLog contract.
/// Enables incremental migration without breaking current implementations.
/// </summary>
public sealed class ActivityLogAdapter : IActivityLog
{
    private readonly Old.IActivityLogService _inner;

    public ActivityLogAdapter(Old.IActivityLogService inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public long Sequence => _inner.Sequence;
    public int Capacity => _inner.Capacity;
    public IObservable<ActivityEntryDto> OnLogAdded => _inner.OnLogAdded;
    public event Action<long>? Changed
    {
        add { _inner.Changed += value; }
        remove { _inner.Changed -= value; }
    }

    public IReadOnlyList<ActivityEntryDto> GetLast(int count) => _inner.GetLast(count);
    public IReadOnlyList<ActivityEntryDto> GetRecentEntries(int count) => _inner.GetRecentEntries(count);
    public IReadOnlyList<ActivityEntryDto> GetSince(long sequence, int maxCount = 200) => _inner.GetSince(sequence, maxCount);
    public IReadOnlyList<ActivityEntryDto> GetByCategory(ActivityCategory category, int maxCount = 50) => _inner.GetByCategory(category, maxCount);
    public IReadOnlyList<ActivityEntryDto> GetBySeverity(ActivitySeverity severity, int maxCount = 50) => _inner.GetBySeverity(severity, maxCount);
    public IReadOnlyList<ActivityEntryDto> Search(string searchTerm, int maxCount = 50) => _inner.Search(searchTerm, maxCount);

    public void Append(string message, ActivitySeverity severity, int? originId = null, string[]? tags = null, char? icon = null)
        => _inner.Append(message, severity, originId, tags, icon);

    public void Info(string message) => _inner.Info(message);
    public void Success(string message) => _inner.Success(message);
    public void Warning(string message) => _inner.Warning(message);
    public void Error(string message) => _inner.Error(message);
    public void Combat(string message) => _inner.Combat(message);
    public void Loot(string message) => _inner.Loot(message);
    public void ClearLog() => _inner.ClearLog();
}
