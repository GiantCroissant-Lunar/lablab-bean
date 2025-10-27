using LablabBean.Contracts.Game.UI.Models;

namespace LablabBean.Contracts.Game.UI.Services;

/// <summary>
/// Platform-agnostic façade over the ECS ActivityLog.
/// UIs should subscribe to Changed and query via GetLast/GetSince.
/// </summary>
public interface IActivityLog
{
    long Sequence { get; }
    int Capacity { get; }
    IObservable<ActivityEntryDto> OnLogAdded { get; }

    event Action<long>? Changed;

    IReadOnlyList<ActivityEntryDto> GetLast(int count);
    IReadOnlyList<ActivityEntryDto> GetRecentEntries(int count);
    IReadOnlyList<ActivityEntryDto> GetSince(long sequence, int maxCount = 200);

    /// <summary>
    /// Get recent entries filtered by category
    /// </summary>
    IReadOnlyList<ActivityEntryDto> GetByCategory(ActivityCategory category, int maxCount = 50);

    /// <summary>
    /// Get recent entries filtered by severity
    /// </summary>
    IReadOnlyList<ActivityEntryDto> GetBySeverity(ActivitySeverity severity, int maxCount = 50);

    /// <summary>
    /// Search entries containing the specified text (case-insensitive)
    /// </summary>
    IReadOnlyList<ActivityEntryDto> Search(string searchTerm, int maxCount = 50);

    void Append(string message, ActivitySeverity severity, int? originId = null, string[]? tags = null, char? icon = null);

    void Info(string message);
    void Success(string message);
    void Warning(string message);
    void Error(string message);
    void Combat(string message);
    void Loot(string message);

    // Maintenance
    void ClearLog();
}
