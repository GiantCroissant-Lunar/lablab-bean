using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Contracts.Diagnostic;
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

        foreach (var providerName in activeProviders)
        {
            if (!_providers.ContainsKey(providerName) || !_providers[providerName])
                continue;

            var data = new DiagnosticData
            {
                ProviderName = providerName,
                ProviderType = ProviderType.Custom,
                Timestamp = timestamp,
                Performance = GetCurrentPerformanceMetrics(),
                Memory = GetCurrentMemoryInfo()
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

    public async Task LogEventAsync(DiagnosticEvent diagnosticEvent)
    {
        _events.Add(diagnosticEvent);
        Interlocked.Increment(ref _eventCount);

        if (diagnosticEvent.Level == DiagnosticLevel.Error || diagnosticEvent.Level == DiagnosticLevel.Critical)
        {
            Interlocked.Increment(ref _errorCount);
        }
        else if (diagnosticEvent.Level == DiagnosticLevel.Warning)
        {
            Interlocked.Increment(ref _warningCount);
        }

        WriteToConsole(diagnosticEvent);
        await Task.CompletedTask;
    }

    public Task LogEventAsync(DiagnosticLevel level, string message, string category, string? source)
    {
        var diagnosticEvent = new DiagnosticEvent
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = message,
            Category = category,
            Source = source ?? "Unknown"
        };

        return LogEventAsync(diagnosticEvent);
    }

    public Task LogExceptionAsync(Exception exception, DiagnosticLevel level, string category, string? source)
    {
        var diagnosticEvent = new DiagnosticEvent
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = exception.Message,
            Category = category,
            Source = source ?? "Unknown",
            Exception = exception
        };

        return LogEventAsync(diagnosticEvent);
    }

    private void WriteToConsole(DiagnosticEvent diagnosticEvent)
    {
        var color = diagnosticEvent.Level switch
        {
            DiagnosticLevel.Critical => ConsoleColor.Magenta,
            DiagnosticLevel.Error => ConsoleColor.Red,
            DiagnosticLevel.Warning => ConsoleColor.Yellow,
            DiagnosticLevel.Information => ConsoleColor.Cyan,
            DiagnosticLevel.Debug => ConsoleColor.Gray,
            _ => ConsoleColor.White
        };

        var originalColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = color;

        var timestamp = diagnosticEvent.Timestamp.ToString("HH:mm:ss.fff");
        var level = diagnosticEvent.Level.ToString().ToUpper().PadRight(8);
        var category = diagnosticEvent.Category.PadRight(15);

        System.Console.WriteLine($"[{timestamp}] [{level}] [{category}] {diagnosticEvent.Message}");

        if (diagnosticEvent.Exception != null)
        {
            System.Console.WriteLine($"  Exception: {diagnosticEvent.Exception.GetType().Name}: {diagnosticEvent.Exception.Message}");
        }

        System.Console.ForegroundColor = originalColor;
    }

    public PerformanceMetrics GetCurrentPerformanceMetrics()
    {
        var process = Process.GetCurrentProcess();

        // Map to current contracts: use available fields only
        return new PerformanceMetrics
        {
            CpuUsage = 0, // Placeholder without sampling window
            MemoryUsage = process.WorkingSet64,
            Uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime()
        };
    }

    public MemoryInfo GetCurrentMemoryInfo()
    {
        var process = Process.GetCurrentProcess();

        return new MemoryInfo
        {
            // Total/Available/Used may require OS APIs; populate managed/GC info
            ManagedHeapSize = GC.GetTotalMemory(false),
            GCGen0Collections = GC.CollectionCount(0),
            GCGen1Collections = GC.CollectionCount(1),
            GCGen2Collections = GC.CollectionCount(2),
            Timestamp = DateTime.UtcNow
        };
    }

    public SystemInfo GetCurrentSystemInfo()
    {
        return new SystemInfo
        {
            Device = new DeviceInfo
            {
                Name = Environment.MachineName,
                OperatingSystem = Environment.OSVersion.ToString()
            },
            Cpu = new CpuInfo
            {
                ProcessorCount = Environment.ProcessorCount,
                Architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86"
            },
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<string> ExportDataAsync(DiagnosticExportFormat format, DateTime? startTime, DateTime? endTime,
        string[]? providers, CancellationToken cancellationToken)
    {
        var data = _collectedData.Where(d =>
            (!startTime.HasValue || d.Timestamp >= startTime.Value) &&
            (!endTime.HasValue || d.Timestamp <= endTime.Value) &&
            (providers == null || providers.Contains(d.ProviderName))
        ).ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"Diagnostic Export - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total Data Points: {data.Count}");
        sb.AppendLine();

        foreach (var item in data)
        {
            sb.AppendLine($"[{item.Timestamp:HH:mm:ss.fff}] Provider: {item.ProviderName} ({item.ProviderType})");

            if (item.Performance != null)
            {
                sb.AppendLine($"  Performance: CpuUsage={item.Performance.CpuUsage:F1}%, MemoryUsage={item.Performance.MemoryUsage} bytes, Uptime={item.Performance.Uptime}");
            }
            if (item.Memory != null)
            {
                sb.AppendLine($"  Memory: ManagedHeap={item.Memory.ManagedHeapSize} bytes, GC0={item.Memory.GCGen0Collections}, GC1={item.Memory.GCGen1Collections}, GC2={item.Memory.GCGen2Collections}");
            }
            if (item.Cpu != null)
            {
                sb.AppendLine($"  CPU: Arch={item.Cpu.Architecture}, Logical={item.Cpu.ProcessorCount}");
            }
            if (item.Device != null)
            {
                sb.AppendLine($"  Device: {item.Device.Name} / {item.Device.OperatingSystem}");
            }
            if (item.CustomMetrics?.Count > 0)
            {
                foreach (var kv in item.CustomMetrics)
                {
                    sb.AppendLine($"  Metric {kv.Key}: {kv.Value}");
                }
            }
            if (item.Metadata?.Count > 0)
            {
                foreach (var kv in item.Metadata)
                {
                    sb.AppendLine($"  Meta {kv.Key}: {kv.Value}");
                }
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
            Type = ProviderType.Custom,
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
            Health = kv.Value ? SystemHealth.Healthy : SystemHealth.Degraded,
            Timestamp = DateTime.UtcNow
        }).ToArray();
    }

    public async Task<HealthCheckResult> PerformHealthCheckAsync(CancellationToken cancellationToken)
    {
        var allHealthy = _providers.Values.All(v => v);
        _currentHealth = allHealthy ? SystemHealth.Healthy : SystemHealth.Degraded;

        return await Task.FromResult(new HealthCheckResult
        {
            OverallHealth = _currentHealth,
            Timestamp = DateTime.UtcNow,
            IsSuccessful = allHealthy
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
            SessionName = "Diagnostic Session",
            StartTime = DateTime.UtcNow.AddMinutes(-5),
            EndTime = DateTime.UtcNow,
            TotalDataPoints = _collectedData.Count,
            TotalEvents = _eventCount,
            TotalAlerts = 0,
            ParticipatingProviders = _providers.Keys.ToList()
        });
    }

    public DiagnosticServiceStats GetServiceStatistics()
    {
        return new DiagnosticServiceStats
        {
            ServiceStartTime = DateTime.UtcNow.AddMinutes(-5), // Approximate
            IsCollecting = _isCollecting,
            TotalDataCollections = _collectedData.Count,
            TotalEventsLogged = _eventCount,
            TotalAlertsTriggered = 0,
            ActiveSessions = 0,
            TotalSessionsStarted = 0,
            TotalSessionsCompleted = 0,
            RegisteredProviders = _providers.Count,
            EnabledProviders = _providers.Count(kv => kv.Value),
            HealthyProviders = _providers.Count(kv => kv.Value),
            DiagnosticDataMemoryUsage = _collectedData.Count * 1024, // Estimate
            StoredDataRecords = _collectedData.Count,
            LastCollectionTime = _collectedData.Any() ? _collectedData.Last().Timestamp : null,
            ServiceHealth = _currentHealth,
            RecentServiceErrors = 0,
            Performance = new ServicePerformanceMetrics
            {
                CpuUsage = 0,
                MemoryUsage = _collectedData.Count * 1024,
                ThreadCount = Process.GetCurrentProcess().Threads.Count,
                AverageResponseTime = 0,
                OperationsPerSecond = 0
            }
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
        var span = new DiagnosticSpan(this, operationName, tags);
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
        private readonly DiagnosticService _service;
        private readonly Dictionary<string, string> _tags = new();
        private readonly List<SpanEvent> _events = new();
        private SpanStatus _status = SpanStatus.Unknown;
        private string? _statusDescription;

        public string SpanId { get; } = Guid.NewGuid().ToString("N");
        public string? ParentSpanId { get; }
        public string OperationName { get; }
        public DateTime StartTime { get; }
        public DateTime? EndTime { get; private set; }
        public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
        public bool IsActive => !EndTime.HasValue;
        public SpanStatus Status => _status;
        public IReadOnlyDictionary<string, string> Tags => _tags;
        public IReadOnlyList<SpanEvent> Events => _events;

        public DiagnosticSpan(DiagnosticService service, string operationName, Dictionary<string, string>? tags, string? parentSpanId = null)
        {
            _service = service;
            OperationName = operationName;
            ParentSpanId = parentSpanId;
            StartTime = DateTime.UtcNow;

            if (tags != null)
            {
                foreach (var kv in tags)
                {
                    _tags[kv.Key] = kv.Value;
                }
            }
        }

        public void SetTag(string key, string value)
        {
            _tags[key] = value;
        }

        public void SetTags(Dictionary<string, string> tags)
        {
            foreach (var kv in tags)
            {
                _tags[kv.Key] = kv.Value;
            }
        }

        public void AddEvent(string name, Dictionary<string, object>? attributes = null)
        {
            _events.Add(SpanEvent.Create(name, attributes));
        }

        public void AddException(Exception exception)
        {
            _events.Add(SpanEvent.FromException(exception));
            _status = SpanStatus.Error;
        }

        public void SetStatus(SpanStatus status, string? description = null)
        {
            _status = status;
            _statusDescription = description;
        }

        public void Finish()
        {
            if (!EndTime.HasValue)
            {
                EndTime = DateTime.UtcNow;

                if (_status == SpanStatus.Unknown)
                {
                    _status = SpanStatus.Ok;
                }
            }
        }

        public IDiagnosticSpan CreateChild(string operationName, Dictionary<string, string>? tags = null)
        {
            var child = new DiagnosticSpan(_service, operationName, tags, SpanId);
            _service._activeSpans.Add(child);
            return child;
        }

        public void Dispose()
        {
            Finish();
        }
    }
}
