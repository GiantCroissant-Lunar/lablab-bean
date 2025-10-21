namespace LablabBean.Plugins.Core;

using LablabBean.Plugins.Contracts;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// In-memory plugin registry with thread-safe operations.
/// </summary>
public sealed class PluginRegistry : IPluginRegistry
{
    private readonly object _lock = new object();
    private readonly Dictionary<string, PluginDescriptor> _plugins = new();

    public IReadOnlyCollection<PluginDescriptor> GetAll()
    {
        lock (_lock)
        {
            return _plugins.Values.ToList();
        }
    }

    public PluginDescriptor? GetById(string id)
    {
        lock (_lock)
        {
            return _plugins.TryGetValue(id, out var descriptor) ? descriptor : null;
        }
    }

    public void Add(PluginDescriptor descriptor)
    {
        lock (_lock)
        {
            _plugins[descriptor.Id] = descriptor;
        }
    }

    public void Remove(string id)
    {
        lock (_lock)
        {
            _plugins.Remove(id);
        }
    }

    public void UpdateState(string id, PluginState state, string? failureReason = null)
    {
        lock (_lock)
        {
            if (_plugins.TryGetValue(id, out var descriptor))
            {
                descriptor.State = state;
                if (failureReason != null)
                {
                    descriptor.FailureReason = failureReason;
                }
            }
        }
    }
}
