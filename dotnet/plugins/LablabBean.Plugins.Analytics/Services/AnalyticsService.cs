using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LablabBean.Contracts.Analytics;
using LablabBean.Contracts.Analytics.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Analytics.Services;

public class AnalyticsService : LablabBean.Contracts.Analytics.Services.IService
{
    private readonly ILogger _logger;
    private readonly ConcurrentBag<AnalyticsEvent> _events = new();
    private readonly ConcurrentDictionary<string, object> _userProperties = new();
    private readonly Dictionary<string, ActionHandler> _actions = new();
    
    private string? _userId;
    private string? _currentScreen;
    private int _eventCount;
    private int _screenTrackCount;

    public AnalyticsService(ILogger logger)
    {
        _logger = logger;
        RegisterDefaultActions();
    }

    public void TrackEvent(string eventName, object? parameters)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));

        var evt = new AnalyticsEvent
        {
            Name = eventName,
            Timestamp = DateTime.UtcNow,
            UserId = _userId,
            Screen = _currentScreen,
            Parameters = parameters
        };

        _events.Add(evt);
        _eventCount++;

        _logger.LogInformation("Analytics: Event tracked - {EventName} (Total: {Count})", 
            eventName, _eventCount);
    }

    public void TrackScreen(string screenName)
    {
        if (string.IsNullOrWhiteSpace(screenName))
            throw new ArgumentException("Screen name cannot be null or empty", nameof(screenName));

        _currentScreen = screenName;
        _screenTrackCount++;

        TrackEvent("screen_view", new { screen_name = screenName });
        
        _logger.LogInformation("Analytics: Screen tracked - {ScreenName} (Total: {Count})", 
            screenName, _screenTrackCount);
    }

    public void SetUserProperty(string propertyName, object value)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

        _userProperties[propertyName] = value;
        
        _logger.LogDebug("Analytics: User property set - {Property} = {Value}", 
            propertyName, value);
    }

    public void SetUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        _userId = userId;
        
        _logger.LogInformation("Analytics: User ID set - {UserId}", userId);
    }

    public void FlushEvents()
    {
        var count = _events.Count;
        
        _logger.LogInformation("Analytics: Flushing {Count} events", count);
        
        // In a real implementation, this would send events to analytics backend
        // For now, just log summary
        var eventSummary = _events
            .GroupBy(e => e.Name)
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();

        foreach (var summary in eventSummary)
        {
            _logger.LogDebug("  {Summary}", summary);
        }
    }

    public TResult ExecuteAction<TResult>(string actionName, params object[] parameters)
    {
        if (!SupportsAction(actionName))
            throw new InvalidOperationException($"Action '{actionName}' is not supported");

        var handler = _actions[actionName];
        
        if (!handler.HasReturnValue)
            throw new InvalidOperationException($"Action '{actionName}' does not return a value");

        try
        {
            var result = handler.Handler(parameters);
            _logger.LogDebug("Analytics: Executed action '{Action}' with result", actionName);
            return (TResult)result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analytics: Failed to execute action '{Action}'", actionName);
            throw;
        }
    }

    public void ExecuteAction(string actionName, params object[] parameters)
    {
        if (!SupportsAction(actionName))
            throw new InvalidOperationException($"Action '{actionName}' is not supported");

        var handler = _actions[actionName];

        try
        {
            handler.Handler(parameters);
            _logger.LogDebug("Analytics: Executed action '{Action}'", actionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analytics: Failed to execute action '{Action}'", actionName);
            throw;
        }
    }

    public bool SupportsAction(string actionName)
    {
        return _actions.ContainsKey(actionName);
    }

    public IEnumerable<ActionInfo> GetSupportedActions()
    {
        return _actions.Values.Select(a => a.Info);
    }

    private void RegisterDefaultActions()
    {
        RegisterAction("GetEventCount", "Get total number of tracked events", 
            _ => _eventCount, hasReturnValue: true);
        
        RegisterAction("GetScreenTrackCount", "Get total number of screen views tracked", 
            _ => _screenTrackCount, hasReturnValue: true);
        
        RegisterAction("GetCurrentUserId", "Get current user ID", 
            _ => _userId ?? "Not set", hasReturnValue: true);
        
        RegisterAction("GetCurrentScreen", "Get current screen name", 
            _ => _currentScreen ?? "Not set", hasReturnValue: true);
        
        RegisterAction("GetUserProperty", "Get a user property value", 
            args =>
            {
                var propName = args[0]?.ToString() ?? throw new ArgumentException("Property name required");
                return _userProperties.TryGetValue(propName, out var value) ? value : null;
            }, 
            hasReturnValue: true, 
            parameterNames: new[] { "propertyName" });
        
        RegisterAction("GetEventsByName", "Get all events with a specific name", 
            args =>
            {
                var eventName = args[0]?.ToString() ?? throw new ArgumentException("Event name required");
                return _events.Where(e => e.Name == eventName).ToList();
            },
            hasReturnValue: true,
            parameterNames: new[] { "eventName" });
        
        RegisterAction("ClearEvents", "Clear all tracked events", 
            _ =>
            {
                _events.Clear();
                _eventCount = 0;
                return null;
            });
        
        RegisterAction("GetAnalyticsSummary", "Get analytics summary", 
            _ => new
            {
                TotalEvents = _eventCount,
                TotalScreenViews = _screenTrackCount,
                UserId = _userId,
                CurrentScreen = _currentScreen,
                UserPropertyCount = _userProperties.Count,
                UniqueEvents = _events.Select(e => e.Name).Distinct().Count()
            },
            hasReturnValue: true);
    }

    private void RegisterAction(string name, string description, Func<object[], object?> handler, 
        bool hasReturnValue = false, string[]? parameterNames = null)
    {
        _actions[name] = new ActionHandler
        {
            Info = new ActionInfo
            {
                ActionName = name,
                Description = description,
                HasReturnValue = hasReturnValue,
                ParameterNames = parameterNames ?? Array.Empty<string>()
            },
            Handler = handler
        };
    }

    private class ActionHandler
    {
        public ActionInfo Info { get; set; } = null!;
        public Func<object[], object?> Handler { get; set; } = null!;
        public bool HasReturnValue => Info.HasReturnValue;
    }

    private class AnalyticsEvent
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? UserId { get; set; }
        public string? Screen { get; set; }
        public object? Parameters { get; set; }
    }
}
