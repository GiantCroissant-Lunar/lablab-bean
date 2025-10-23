using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Contracts.Diagnostic;
using LablabBean.Contracts.Diagnostic.Interfaces;
using LablabBean.Contracts.Diagnostic.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Diagnostic.Console.Services;

public class DiagnosticService : IService
{
    private readonly ILogger _logger;
    private readonly ConcurrentBag<DiagnosticEvent> _events = new();
    private readonly ConcurrentBag<DiagnosticData> _collectedData = new();
    private readonly Dictionary<string, bool> _providers = new();
    private readonly Dictionary<string, string> _globalTags = new();
    private readonly List<DiagnosticSpan> _activeSpans = new();
    
    private Timer? _collectionTimer;
    private bool _isCollecting;
    private int _eventCount;
    private int _errorCount;
    private int _warningCount;
    private PerformanceThresholds? _thresholds;
    private string? _userId;
    private Dictionary<string, string>? _userProperties;
    private SystemHealth _currentHealth = SystemHealth.Healthy;

    public SystemHealth CurrentHealth => _currentHealth;
    public bool IsCollecting => _isCollecting;
    public int ProviderCount => _providers.Count;

    public DiagnosticService(ILogger logger)
    {
        _logger = logger;
        InitializeProviders();
    }

    private void InitializeProviders()
    {
        _providers["System"] = true;
        _providers["Performance"] = true;
        _providers["Memory"] = true;
        _providers["Console"] = true;
    }

    public async Task StartCollectionAsync(TimeSpan interval, CancellationToken cancellationToken)
    {
        if (_isCollecting)
        {
            _logger.LogWarning("Diagnostic collection already started");
            return;
        }

        _logger.LogInformation("Starting diagnostic collection with interval {Interval}", interval);
        _isCollecting = true;

        _collectionTimer = new Timer(
            async _ => await CollectDataAsync(null, CancellationToken.None),
            null,
            TimeSpan.Zero,
            interval
        );

        await Task.CompletedTask;
    }

    public async Task StopCollectionAsync()
    {
        if (!_isCollecting)
        {
            _logger.LogWarning("Diagnostic collection not started");
            return;
        }

        _logger.LogInformation("Stopping diagnostic collection");
        _isCollecting = false;
        _collectionTimer?.Dispose();
        _collectionTimer = null;

        await Task.CompletedTask;
    }

    public async Task<DiagnosticData[]> CollectDataAsync(string[]? providers, CancellationToken cancellationToken)
    {
        var results = new List<DiagnosticData>();
        var timestamp = DateTime.UtcNow;

        var activeProviders = providers ?? _providers.Keys.ToArray();

        foreach (var provider in activeProviders)
        {
            if (!_providers.ContainsKey(provider) || !_providers[provider])
                continue;

            var data = new DiagnosticData
            {
                Timestamp = timestamp,
                Provider = provider,
                Data = CollectProviderData(provider)
            };

            results.Add(data);
            _collectedData.Add(data);
        }

        _logger.LogDebug("Collected {Count} diagnostic data points", results.Count);
        return await Task.FromResult(results.ToArray());
    }

    private Dictionary<string, object> CollectProviderData(string provider)
    {
        return provider switch
        {
            "System" => CollectSystemData(),
            "Performance" => CollectPerformanceData(),
            "Memory" => CollectMemoryData(),
            "Console" => CollectConsoleData(),
            _ => new Dictionary<string, object>()
        };
    }

    private Dictionary<string, object> CollectSystemData()
    {
        return new Dictionary<string, object>
        {
            { "MachineName", Environment.MachineName },
            { "OSVersion", Environment.OSVersion.ToString() },
            { "ProcessorCount", Environment.ProcessorCount },
            { "Is64BitOS", Environment.Is64BitOperatingSystem },
            { "CLRVersion", Environment.Version.ToString() },
            { "Uptime", (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds }
        };
    }

    private Dictionary<string, object> CollectPerformanceData()
    {
        var process = Process.GetCurrentProcess();
        return new Dictionary<string, object>
        {
            { "CpuTime", process.TotalProcessorTime.TotalMilliseconds },
            { "ThreadCount", process.Threads.Count },
            { "HandleCount", process.HandleCount }
        };
    }

    private Dictionary<string, object> CollectMemoryData()
    {
        var process = Process.GetCurrentProcess();
        return new Dictionary<string, object>
        {
            { "WorkingSet", process.WorkingSet64 },
            { "PrivateMemory", process.PrivateMemorySize64 },
            { "GCMemory", GC.GetTotalMemory(false) },
            { "Gen0Collections", GC.CollectionCount(0) },
            { "Gen1Collections", GC.CollectionCount(1) },
            { "Gen2Collections", GC.CollectionCount(2) }
        };
    }

    private Dictionary<string, object> CollectConsoleData()
    {
        return new Dictionary<string, object>
        {
            { "EventCount", _eventCount },
            { "ErrorCount", _errorCount },
            { "WarningCount", _warningCount }
        };
    }

    public async Task LogEventAsync(DiagnosticEvent @event)
    {
        _events.Add(@event);
        Interlocked.Increment(ref _eventCount);

        if (@event.Level == DiagnosticLevel.Error || @event.Level == DiagnosticLevel.Critical)
        {
            Interlocked.Increment(ref _errorCount);
        }
        else if (@event.Level == DiagnosticLevel.Warning)
        {
            Interlocked.Increment(ref _warningCount);
        }

        WriteToConsole(@event);
        await Task.CompletedTask;
    }

    public Task LogEventAsync(DiagnosticLevel level, string message, string category, string? source)
    {
        var @event = new DiagnosticEvent
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = message,
            Category = category,
            Source = source ?? "Unknown"
        };

        return LogEventAsync(@event);
    }

    public Task LogExceptionAsync(Exception exception, DiagnosticLevel level, string category, string? source)
    {
        var @event = new DiagnosticEvent
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = exception.Message,
            Category = category,
            Source = source ?? "Unknown",
            Exception = exception
        };

        return LogEventAsync(@event);
    }

    private void WriteToConsole(DiagnosticEvent @event)
    {
        var color = @event.Level switch
        {
            DiagnosticLevel.Critical => ConsoleColor.Magenta,
            DiagnosticLevel.Error => ConsoleColor.Red,
            DiagnosticLevel.Warning => ConsoleColor.Yellow,
            DiagnosticLevel.Info => ConsoleColor.Cyan,
            DiagnosticLevel.Debug => ConsoleColor.Gray,
            _ => ConsoleColor.White
        };

        var originalColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = color;
        
        var timestamp = @event.Timestamp.ToString("HH:mm:ss.fff");
        var level = @event.Level.ToString().ToUpper().PadRight(8);
        var category = @event.Category.PadRight(15);
        
        System.Console.WriteLine($"[{timestamp}] [{level}] [{category}] {@event.Message}");
        
        if (@event.Exception != null)
        {
            System.Console.WriteLine($"  Exception: {@event.Exception.GetType().Name}: {@event.Exception.Message}");
        }
        
        System.Console.ForegroundColor = originalColor;
    }

    public PerformanceMetrics GetCurrentPerformanceMetrics()
    {
        var process = Process.GetCurrentProcess();
        
        return new PerformanceMetrics
        {
            CpuUsagePercent = 0,
            MemoryUsageMB = process.WorkingSet64 / 1024.0 / 1024.0,
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount,
            Timestamp = DateTime.UtcNow
        };
    }

    public MemoryInfo GetCurrentMemoryInfo()
    {
        var process = Process.GetCurrentProcess();
        
        return new MemoryInfo
        {
            WorkingSetBytes = process.WorkingSet64,
            PrivateMemoryBytes = process.PrivateMemorySize64,
            ManagedMemoryBytes = GC.GetTotalMemory(false),
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            Timestamp = DateTime.UtcNow
        };
    }

    public SystemInfo GetCurrentSystemInfo()
    {
        return new SystemInfo
        {
            MachineName = Environment.MachineName,
            OSVersion = Environment.OSVersion.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            Is64BitOS = Environment.Is64BitOperatingSystem,
            CLRVersion = Environment.Version.ToString(),
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<string> ExportDataAsync(DiagnosticExportFormat format, DateTime? startTime, DateTime? endTime, 
        string[]? providers, CancellationToken cancellationToken)
    {
        var data = _collectedData.Where(d =>
            (!startTime.HasValue || d.Timestamp >= startTime.Value) &&
            (!endTime.HasValue || d.Timestamp <= endTime.Value) &&
            (providers == null || providers.Contains(d.Provider))
        ).ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"Diagnostic Export - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total Data Points: {data.Count}");
        sb.AppendLine();

        foreach (var item in data)
        {
            sb.AppendLine($"[{item.Timestamp:HH:mm:ss.fff}] Provider: {item.Provider}");
            foreach (var kv in item.Data)
            {
                sb.AppendLine($"  {kv.Key}: {kv.Value}");
            }
            sb.AppendLine();
        }

        return await Task.FromResult(sb.ToString());
    }

    public void ConfigureAlertThresholds(PerformanceThresholds thresholds)
    {
        _thresholds = thresholds;
        _logger.LogInformation("Alert thresholds configured");
    }

    public ProviderInfo[] GetAvailableProviders()
    {
        return _providers.Select(kv => new ProviderInfo
        {
            Name = kv.Key,
            Enabled = kv.Value,
            Description = $"{kv.Key} diagnostic provider"
        }).ToArray();
    }

    public async Task ConfigureProviderAsync(string providerName, bool enabled)
    {
        if (_providers.ContainsKey(providerName))
        {
            _providers[providerName] = enabled;
            _logger.LogInformation("Provider {Provider} {Status}", providerName, enabled ? "enabled" : "disabled");
        }
        await Task.CompletedTask;
    }

    public async Task ConfigureProviderAsync(string providerName, Dictionary<string, object> configuration)
    {
        _logger.LogInformation("Configuring provider {Provider} with {Count} settings", providerName, configuration.Count);
        await Task.CompletedTask;
    }

    public ProviderHealthResult[] GetProviderHealth(string? providerName)
    {
        var providers = providerName != null 
            ? _providers.Where(kv => kv.Key == providerName) 
            : _providers;

        return providers.Select(kv => new ProviderHealthResult
        {
            ProviderName = kv.Key,
            IsHealthy = kv.Value,
            LastCheck = DateTime.UtcNow
        }).ToArray();
    }

    public async Task<HealthCheckResult> PerformHealthCheckAsync(CancellationToken cancellationToken)
    {
        var allHealthy = _providers.Values.All(v => v);
        _currentHealth = allHealthy ? SystemHealth.Healthy : SystemHealth.Degraded;

        return await Task.FromResult(new HealthCheckResult
        {
            Health = _currentHealth,
            Timestamp = DateTime.UtcNow,
            Message = allHealthy ? "All systems operational" : "Some providers unhealthy"
        });
    }

    public async Task<string> StartSessionAsync(string sessionName, DiagnosticSessionConfig? configuration, 
        CancellationToken cancellationToken)
    {
        var sessionId = Guid.NewGuid().ToString();
        _logger.LogInformation("Started diagnostic session {SessionId} ({Name})", sessionId, sessionName);
        return await Task.FromResult(sessionId);
    }

    public async Task<DiagnosticSessionSummary> StopSessionAsync(string sessionId)
    {
        _logger.LogInformation("Stopped diagnostic session {SessionId}", sessionId);
        
        return await Task.FromResult(new DiagnosticSessionSummary
        {
            SessionId = sessionId,
            StartTime = DateTime.UtcNow.AddMinutes(-5),
            EndTime = DateTime.UtcNow,
            EventCount = _eventCount
        });
    }

    public DiagnosticServiceStats GetServiceStatistics()
    {
        return new DiagnosticServiceStats
        {
            TotalEvents = _eventCount,
            TotalErrors = _errorCount,
            TotalWarnings = _warningCount,
            ActiveProviders = _providers.Count(kv => kv.Value),
            CollectedDataPoints = _collectedData.Count,
            IsCollecting = _isCollecting
        };
    }

    public async Task<int> ClearDataAsync(DateTime? olderThan)
    {
        var beforeCount = _collectedData.Count;
        
        if (olderThan.HasValue)
        {
            var toRemove = _collectedData.Where(d => d.Timestamp < olderThan.Value).ToList();
            foreach (var item in toRemove)
            {
                _collectedData.TryTake(out _);
            }
        }
        else
        {
            _collectedData.Clear();
        }

        var removed = beforeCount - _collectedData.Count;
        _logger.LogInformation("Cleared {Count} diagnostic data points", removed);
        
        return await Task.FromResult(removed);
    }

    public void SubscribeToEvents(DiagnosticEventFilter filter)
    {
        _logger.LogInformation("Subscribed to diagnostic events with filter");
    }

    public IDiagnosticSpan CreateSpan(string operationName, Dictionary<string, string>? tags)
    {
        var span = new DiagnosticSpan(operationName, tags);
        _activeSpans.Add(span);
        return span;
    }

    public void AddBreadcrumb(string message, string category, DiagnosticLevel level, Dictionary<string, object>? data)
    {
        _logger.LogDebug("Breadcrumb: [{Category}] {Message}", category, message);
    }

    public void SetUserContext(string userId, Dictionary<string, string>? properties)
    {
        _userId = userId;
        _userProperties = properties;
        _logger.LogInformation("User context set for {UserId}", userId);
    }

    public void SetGlobalTags(Dictionary<string, string> tags)
    {
        foreach (var kv in tags)
        {
            _globalTags[kv.Key] = kv.Value;
        }
        _logger.LogInformation("Set {Count} global tags", tags.Count);
    }

    public void RemoveGlobalTags(string[]? keys)
    {
        if (keys == null)
        {
            _globalTags.Clear();
        }
        else
        {
            foreach (var key in keys)
            {
                _globalTags.Remove(key);
            }
        }
    }

    private class DiagnosticSpan : IDiagnosticSpan
    {
        public string OperationName { get; }
        public Dictionary<string, string>? Tags { get; }
        public DateTime StartTime { get; }
        public DateTime? EndTime { get; private set; }
        public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;

        public DiagnosticSpan(string operationName, Dictionary<string, string>? tags)
        {
            OperationName = operationName;
            Tags = tags;
            StartTime = DateTime.UtcNow;
        }

        public void End()
        {
            EndTime = DateTime.UtcNow;
        }

        public void Dispose()
        {
            if (!EndTime.HasValue)
            {
                End();
            }
        }
    }
}
