using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using LablabBean.Contracts.Game.UI.Models;
using LablabBean.Contracts.Game.UI.Services;
using Old = LablabBean.Contracts.UI.Services;
using OldModels = LablabBean.Contracts.UI.Models;

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
    public IObservable<ActivityEntryDto> OnLogAdded => _inner.OnLogAdded.Select(MapToGameDto);
    public event Action<long>? Changed
    {
        add { _inner.Changed += value; }
        remove { _inner.Changed -= value; }
    }

    public IReadOnlyList<ActivityEntryDto> GetLast(int count) => MapToGameDtoList(_inner.GetLast(count));
    public IReadOnlyList<ActivityEntryDto> GetRecentEntries(int count) => MapToGameDtoList(_inner.GetRecentEntries(count));
    public IReadOnlyList<ActivityEntryDto> GetSince(long sequence, int maxCount = 200) => MapToGameDtoList(_inner.GetSince(sequence, maxCount));
    public IReadOnlyList<ActivityEntryDto> GetByCategory(ActivityCategory category, int maxCount = 50) => MapToGameDtoList(_inner.GetByCategory(MapToOldCategory(category), maxCount));
    public IReadOnlyList<ActivityEntryDto> GetBySeverity(ActivitySeverity severity, int maxCount = 50) => MapToGameDtoList(_inner.GetBySeverity(MapToOldSeverity(severity), maxCount));
    public IReadOnlyList<ActivityEntryDto> Search(string searchTerm, int maxCount = 50) => MapToGameDtoList(_inner.Search(searchTerm, maxCount));

    public void Append(string message, ActivitySeverity severity, int? originId = null, string[]? tags = null, char? icon = null)
        => _inner.Append(message, MapToOldSeverity(severity), originId, tags, icon);

    public void Info(string message) => _inner.Info(message);
    public void Success(string message) => _inner.Success(message);
    public void Warning(string message) => _inner.Warning(message);
    public void Error(string message) => _inner.Error(message);
    public void Combat(string message) => _inner.Combat(message);
    public void Loot(string message) => _inner.Loot(message);
    public void ClearLog() => _inner.ClearLog();

    // Mapping helpers
    private static ActivityEntryDto MapToGameDto(OldModels.ActivityEntryDto old)
    {
        return new ActivityEntryDto
        {
            Timestamp = old.Timestamp,
            Message = old.Message,
            Severity = MapToGameSeverity(old.Severity),
            Category = MapToGameCategory(old.Category),
            OriginEntityId = old.OriginEntityId,
            Tags = old.Tags,
            Metadata = old.Metadata,
            Icon = old.Icon,
            Color = old.Color
        };
    }

    private static IReadOnlyList<ActivityEntryDto> MapToGameDtoList(IReadOnlyList<OldModels.ActivityEntryDto> oldList)
    {
        return oldList.Select(MapToGameDto).ToList();
    }

    private static ActivitySeverity MapToGameSeverity(OldModels.ActivitySeverity severity)
    {
        return severity switch
        {
            OldModels.ActivitySeverity.Info => ActivitySeverity.Info,
            OldModels.ActivitySeverity.Success => ActivitySeverity.Success,
            OldModels.ActivitySeverity.Warning => ActivitySeverity.Warning,
            OldModels.ActivitySeverity.Error => ActivitySeverity.Error,
            OldModels.ActivitySeverity.Combat => ActivitySeverity.Combat,
            OldModels.ActivitySeverity.Loot => ActivitySeverity.Loot,
            OldModels.ActivitySeverity.System => ActivitySeverity.System,
            _ => ActivitySeverity.Info
        };
    }

    private static OldModels.ActivitySeverity MapToOldSeverity(ActivitySeverity severity)
    {
        return severity switch
        {
            ActivitySeverity.Info => OldModels.ActivitySeverity.Info,
            ActivitySeverity.Success => OldModels.ActivitySeverity.Success,
            ActivitySeverity.Warning => OldModels.ActivitySeverity.Warning,
            ActivitySeverity.Error => OldModels.ActivitySeverity.Error,
            ActivitySeverity.Combat => OldModels.ActivitySeverity.Combat,
            ActivitySeverity.Loot => OldModels.ActivitySeverity.Loot,
            ActivitySeverity.System => OldModels.ActivitySeverity.System,
            _ => OldModels.ActivitySeverity.Info
        };
    }

    private static ActivityCategory MapToGameCategory(OldModels.ActivityCategory category)
    {
        return category switch
        {
            OldModels.ActivityCategory.System => ActivityCategory.System,
            OldModels.ActivityCategory.Combat => ActivityCategory.Combat,
            OldModels.ActivityCategory.Movement => ActivityCategory.Movement,
            OldModels.ActivityCategory.Items => ActivityCategory.Items,
            OldModels.ActivityCategory.Level => ActivityCategory.Level,
            OldModels.ActivityCategory.Quest => ActivityCategory.Quest,
            OldModels.ActivityCategory.Dialogue => ActivityCategory.Dialogue,
            OldModels.ActivityCategory.Analytics => ActivityCategory.Analytics,
            OldModels.ActivityCategory.UI => ActivityCategory.UI,
            OldModels.ActivityCategory.Misc => ActivityCategory.Misc,
            _ => ActivityCategory.System
        };
    }

    private static OldModels.ActivityCategory MapToOldCategory(ActivityCategory category)
    {
        return category switch
        {
            ActivityCategory.System => OldModels.ActivityCategory.System,
            ActivityCategory.Combat => OldModels.ActivityCategory.Combat,
            ActivityCategory.Movement => OldModels.ActivityCategory.Movement,
            ActivityCategory.Items => OldModels.ActivityCategory.Items,
            ActivityCategory.Level => OldModels.ActivityCategory.Level,
            ActivityCategory.Quest => OldModels.ActivityCategory.Quest,
            ActivityCategory.Dialogue => OldModels.ActivityCategory.Dialogue,
            ActivityCategory.Analytics => OldModels.ActivityCategory.Analytics,
            ActivityCategory.UI => OldModels.ActivityCategory.UI,
            ActivityCategory.Misc => OldModels.ActivityCategory.Misc,
            _ => OldModels.ActivityCategory.System
        };
    }
}
