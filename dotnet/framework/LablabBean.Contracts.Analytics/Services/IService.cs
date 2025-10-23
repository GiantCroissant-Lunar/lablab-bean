using System.Collections.Generic;

namespace LablabBean.Contracts.Analytics.Services;

public interface IService
{
    void TrackEvent(string eventName, object? parameters);
    void TrackScreen(string screenName);
    void SetUserProperty(string propertyName, object value);
    void SetUserId(string userId);
    void FlushEvents();
    TResult ExecuteAction<TResult>(string actionName, params object[] parameters);
    void ExecuteAction(string actionName, params object[] parameters);
    bool SupportsAction(string actionName);
    IEnumerable<ActionInfo> GetSupportedActions();
}
