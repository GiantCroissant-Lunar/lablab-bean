using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Contracts.UI.Models;
using LablabBean.Contracts.UI.Services;
using LablabBean.Game.Core.Components;
using LablabBean.Game.Core.Systems;
using LablabBean.Game.Core.Worlds;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SadRogue.Primitives;

namespace LablabBean.Game.Core.Services;

/// <summary>
/// Platform-agnostic service that wraps ActivityLogSystem and exposes push notifications.
/// </summary>
public sealed class ActivityLogService : IActivityLogService
{
    private readonly ILogger<ActivityLogService> _logger;
    private readonly GameWorldManager _worldManager;
    private readonly ActivityLogSystem _logSystem;
    private readonly ActivityLogOptions _options;
    private readonly object _lock = new();
    private readonly List<IObserver<ActivityEntryDto>> _observers = new();

    private sealed class ObservableDispatcher : IObservable<ActivityEntryDto>
    {
        private readonly ActivityLogService _owner;
        public ObservableDispatcher(ActivityLogService owner) => _owner = owner;
        public IDisposable Subscribe(IObserver<ActivityEntryDto> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            lock (_owner._lock)
            {
                _owner._observers.Add(observer);
            }
            return new Unsubscriber(_owner, observer);
        }

        private sealed class Unsubscriber : IDisposable
        {
            private readonly ActivityLogService _owner;
            private readonly IObserver<ActivityEntryDto> _observer;
            public Unsubscriber(ActivityLogService owner, IObserver<ActivityEntryDto> observer)
            { _owner = owner; _observer = observer; }
            public void Dispose()
            {
                lock (_owner._lock)
                {
                    _owner._observers.Remove(_observer);
                }
            }
        }
    }

    public event Action<long>? Changed;

    public ActivityLogService(
        ILogger<ActivityLogService> logger,
        GameWorldManager worldManager,
        ActivityLogSystem logSystem,
        IOptions<ActivityLogOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _logSystem = logSystem ?? throw new ArgumentNullException(nameof(logSystem));
        _options = options?.Value ?? new ActivityLogOptions();
    }

    public long Sequence => GetLog(out _).Sequence;
    public int Capacity => GetLog(out _).Capacity;
    public IObservable<ActivityEntryDto> OnLogAdded => new ObservableDispatcher(this);

    public IReadOnlyList<ActivityEntryDto> GetLast(int count)
    {
        var log = GetLog(out _);
        var ct = log.Entries.Count;
        if (ct == 0) return Array.Empty<ActivityEntryDto>();
        var start = Math.Max(0, ct - Math.Max(1, count));
        var list = new List<ActivityEntryDto>(ct - start);
        for (int i = start; i < ct; i++) list.Add(Map(log.Entries[i]));
        return list;
    }

    public IReadOnlyList<ActivityEntryDto> GetSince(long sequence, int maxCount = 200)
    {
        var log = GetLog(out _);
        if (log.Sequence <= sequence || log.Entries.Count == 0)
            return Array.Empty<ActivityEntryDto>();

        // Since we don't store per-entry sequence, approximate by taking last maxCount
        // Consumers should rely on Changed(sequence) to re-render fully when needed.
        var take = Math.Min(maxCount, log.Entries.Count);
        var start = log.Entries.Count - take;
        var list = new List<ActivityEntryDto>(take);
        for (int i = start; i < log.Entries.Count; i++) list.Add(Map(log.Entries[i]));
        return list;
    }

    public IReadOnlyList<ActivityEntryDto> GetRecentEntries(int count)
        => GetLast(count);

    public IReadOnlyList<ActivityEntryDto> GetByCategory(ActivityCategory category, int maxCount = 50)
    {
        var log = GetLog(out _);
        if (log.Entries.Count == 0) return Array.Empty<ActivityEntryDto>();

        var filtered = log.Entries
            .Where(e => e.Category == category)
            .TakeLast(maxCount)
            .Select(Map)
            .ToList();

        return filtered;
    }

    public IReadOnlyList<ActivityEntryDto> GetBySeverity(ActivitySeverity severity, int maxCount = 50)
    {
        var log = GetLog(out _);
        if (log.Entries.Count == 0) return Array.Empty<ActivityEntryDto>();

        var filtered = log.Entries
            .Where(e => e.Severity == severity)
            .TakeLast(maxCount)
            .Select(Map)
            .ToList();

        return filtered;
    }

    public IReadOnlyList<ActivityEntryDto> Search(string searchTerm, int maxCount = 50)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<ActivityEntryDto>();

        var log = GetLog(out _);
        if (log.Entries.Count == 0) return Array.Empty<ActivityEntryDto>();

        var searchLower = searchTerm.ToLowerInvariant();
        var filtered = log.Entries
            .Where(e => e.Message.ToLowerInvariant().Contains(searchLower))
            .TakeLast(maxCount)
            .Select(Map)
            .ToList();

        return filtered;
    }

    public void Append(string message, ActivitySeverity severity, int? originId = null, string[]? tags = null, char? icon = null)
    {
        // Check if severity meets minimum threshold
        if (severity < _options.MinimumSeverity)
            return;

        lock (_lock)
        {
            var world = _worldManager.CurrentWorld;
            _logSystem.Append(world, message, severity, originId, null, tags, icon);

            // Mirror to Microsoft.Extensions.Logging if enabled
            if (_options.MirrorToLogger)
            {
                switch (severity)
                {
                    case ActivitySeverity.Error:
                        _logger.LogError("{Message}", message);
                        break;
                    case ActivitySeverity.Warning:
                        _logger.LogWarning("{Message}", message);
                        break;
                    case ActivitySeverity.Success:
                    case ActivitySeverity.Loot:
                    case ActivitySeverity.Combat:
                    case ActivitySeverity.System:
                    case ActivitySeverity.Info:
                    default:
                        _logger.LogInformation("{Message}", message);
                        break;
                }
            }

            var log = GetLog(out var entity, world);
            var seq = log.Sequence;
            Changed?.Invoke(seq);
            // Notify observers with the last entry
            if (log.Entries.Count > 0)
            {
                var last = log.Entries[^1];
                var dto = Map(last);
                foreach (var obs in _observers.ToArray())
                {
                    try { obs.OnNext(dto); } catch { /* ignore observer errors */ }
                }
            }
        }
    }

    public void Info(string message) => Append(message, ActivitySeverity.Info);
    public void Success(string message) => Append(message, ActivitySeverity.Success);
    public void Warning(string message) => Append(message, ActivitySeverity.Warning);
    public void Error(string message) => Append(message, ActivitySeverity.Error);
    public void Combat(string message) => Append(message, ActivitySeverity.Combat);
    public void Loot(string message) => Append(message, ActivitySeverity.Loot);

    public void ClearLog()
    {
        lock (_lock)
        {
            var world = _worldManager.CurrentWorld;
            var entity = _logSystem.EnsureLogEntity(world);
            var log = world.Get<ActivityLog>(entity);
            if (log.Entries.Count > 0)
            {
                log.Entries.Clear();
                // Advance sequence so subscribers detect change
                log.Sequence++;
                world.Set(entity, log);
                _logger.LogInformation("Activity log cleared");
                Changed?.Invoke(log.Sequence);
            }
        }
    }

    private ActivityLog GetLog(out Entity entity) => GetLog(out entity, _worldManager.CurrentWorld);

    private ActivityLog GetLog(out Entity entity, World world)
    {
        entity = _logSystem.EnsureLogEntity(world);
        return world.Get<ActivityLog>(entity);
    }

    private static ActivityEntryDto Map(ActivityEntry entry)
        => new ActivityEntryDto
        {
            Timestamp = entry.Timestamp,
            Message = entry.Message,
            Severity = entry.Severity,
            Category = entry.Category,
            OriginEntityId = entry.OriginEntityId,
            Tags = entry.Tags,
            Metadata = entry.Metadata != null ? new System.Collections.Generic.Dictionary<string, object>(entry.Metadata) : null,
            Icon = GetIconForSeverity(entry.Severity),
            Color = GetColorForSeverity(entry.Severity)
        };

    private static string GetIconForSeverity(ActivitySeverity severity)
        => severity switch
        {
            ActivitySeverity.Success => "+",
            ActivitySeverity.Warning => "!",
            ActivitySeverity.Error => "×",
            ActivitySeverity.Combat => "⚔",
            ActivitySeverity.Loot => "$",
            ActivitySeverity.System => "·",
            ActivitySeverity.Info => "·",
            _ => "·"
        };

    private static string GetColorForSeverity(ActivitySeverity severity)
        => severity switch
        {
            ActivitySeverity.Success => "Green",
            ActivitySeverity.Warning => "Yellow",
            ActivitySeverity.Error => "Red",
            ActivitySeverity.Combat => "Red",
            ActivitySeverity.Loot => "Gold",
            ActivitySeverity.System => "Gray",
            ActivitySeverity.Info => "White",
            _ => "White"
        };
}
