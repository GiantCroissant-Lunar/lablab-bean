using LablabBean.Contracts.Config;
using LablabBean.Contracts.Config.Services;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace LablabBean.Plugins.ConfigManager;

/// <summary>
/// Simple in-memory configuration service.
/// </summary>
public class InMemoryConfigService : IService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly Dictionary<string, string> _config = new();

    public event EventHandler<ConfigChangedEventArgs>? ConfigChanged;

    public InMemoryConfigService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string? Get(string key)
    {
        return _config.TryGetValue(key, out var value) ? value : null;
    }

    public T? Get<T>(string key)
    {
        var value = Get(key);
        if (value == null) return default;

        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T?)converter.ConvertFromString(value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to convert config value {Key} to type {Type}", key, typeof(T).Name);
            return default;
        }
    }

    public IConfigSection GetSection(string key)
    {
        return new ConfigSection(this, key);
    }

    public void Set(string key, string value)
    {
        var oldValue = Get(key);
        _config[key] = value;

        _logger.LogDebug("Config set: {Key} = {Value}", key, value);

        // Fire event
        ConfigChanged?.Invoke(this, new ConfigChangedEventArgs
        {
            Key = key,
            OldValue = oldValue,
            NewValue = value
        });

        // Publish event
        _eventBus.PublishAsync(new ConfigChangedEvent(key, oldValue, value)).GetAwaiter().GetResult();
    }

    public bool Exists(string key)
    {
        return _config.ContainsKey(key);
    }

    public Task ReloadAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Config reload requested (no-op for in-memory config)");
        _eventBus.PublishAsync(new ConfigReloadedEvent()).GetAwaiter().GetResult();
        return Task.CompletedTask;
    }

    private class ConfigSection : IConfigSection
    {
        private readonly InMemoryConfigService _service;
        private readonly string _path;

        public string Path => _path;

        public ConfigSection(InMemoryConfigService service, string path)
        {
            _service = service;
            _path = path;
        }

        public string? Get(string key)
        {
            var fullKey = $"{_path}:{key}";
            return _service.Get(fullKey);
        }

        public T? Get<T>(string key)
        {
            var fullKey = $"{_path}:{key}";
            return _service.Get<T>(fullKey);
        }

        public IConfigSection GetSection(string key)
        {
            var fullPath = $"{_path}:{key}";
            return new ConfigSection(_service, fullPath);
        }

        public IReadOnlyCollection<string> GetKeys()
        {
            var prefix = $"{_path}:";
            return _service._config.Keys
                .Where(k => k.StartsWith(prefix))
                .Select(k => k.Substring(prefix.Length).Split(':')[0])
                .Distinct()
                .ToList()
                .AsReadOnly();
        }
    }
}
