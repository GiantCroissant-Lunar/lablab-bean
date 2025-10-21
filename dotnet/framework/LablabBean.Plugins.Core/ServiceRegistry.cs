namespace LablabBean.Plugins.Core;

using LablabBean.Plugins.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Cross-ALC service registry with priority-based selection.
/// Thread-safe implementation for concurrent plugin loading.
/// </summary>
public sealed class ServiceRegistry : IRegistry
{
    private readonly object _lock = new object();
    private readonly Dictionary<Type, List<ServiceRegistration>> _services = new();

    public void Register<TService>(TService implementation, ServiceMetadata metadata) where TService : class
    {
        if (implementation == null) throw new ArgumentNullException(nameof(implementation));
        if (metadata == null) throw new ArgumentNullException(nameof(metadata));

        lock (_lock)
        {
            var serviceType = typeof(TService);
            if (!_services.ContainsKey(serviceType))
            {
                _services[serviceType] = new List<ServiceRegistration>();
            }

            var registration = new ServiceRegistration
            {
                Implementation = implementation,
                Metadata = metadata
            };

            _services[serviceType].Add(registration);
        }
    }

    public void Register<TService>(TService implementation, int priority = 100) where TService : class
    {
        Register(implementation, new ServiceMetadata { Priority = priority });
    }

    public TService Get<TService>(SelectionMode mode = SelectionMode.HighestPriority) where TService : class
    {
        lock (_lock)
        {
            var serviceType = typeof(TService);
            if (!_services.TryGetValue(serviceType, out var registrations) || registrations.Count == 0)
            {
                throw new InvalidOperationException($"No implementations registered for service type {serviceType.Name}");
            }

            if (mode == SelectionMode.One && registrations.Count > 1)
            {
                throw new InvalidOperationException($"Multiple implementations registered for service type {serviceType.Name}. Use SelectionMode.HighestPriority or GetAll() instead.");
            }

            if (mode == SelectionMode.All)
            {
                throw new InvalidOperationException("SelectionMode.All is not valid for Get<T>. Use GetAll<T>() instead.");
            }

            var selected = registrations.OrderByDescending(r => r.Metadata.Priority).First();
            return (TService)selected.Implementation;
        }
    }

    public IEnumerable<TService> GetAll<TService>() where TService : class
    {
        lock (_lock)
        {
            var serviceType = typeof(TService);
            if (!_services.TryGetValue(serviceType, out var registrations))
            {
                return Enumerable.Empty<TService>();
            }

            return registrations
                .OrderByDescending(r => r.Metadata.Priority)
                .Select(r => (TService)r.Implementation)
                .ToList();
        }
    }

    public bool IsRegistered<TService>() where TService : class
    {
        lock (_lock)
        {
            var serviceType = typeof(TService);
            return _services.TryGetValue(serviceType, out var registrations) && registrations.Count > 0;
        }
    }

    public bool Unregister<TService>(TService implementation) where TService : class
    {
        lock (_lock)
        {
            var serviceType = typeof(TService);
            if (!_services.TryGetValue(serviceType, out var registrations))
            {
                return false;
            }

            var removed = registrations.RemoveAll(r => ReferenceEquals(r.Implementation, implementation));
            return removed > 0;
        }
    }

    private sealed class ServiceRegistration
    {
        public required object Implementation { get; init; }
        public required ServiceMetadata Metadata { get; init; }
    }
}
