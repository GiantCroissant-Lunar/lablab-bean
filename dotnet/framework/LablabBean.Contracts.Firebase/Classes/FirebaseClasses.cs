using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Firebase;

public class FirebaseInitializationResult
{
    public FirebaseInitializationStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime InitializedAt { get; set; }
    public TimeSpan InitializationDuration { get; set; }
}

public class FirebaseConfig
{
    public string ProjectId { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public Dictionary<string, object> AdditionalSettings { get; set; } = new();
}

public class FirebaseStatistics
{
    public bool IsInitialized { get; set; }
    public FirebaseDependencyStatus DependencyStatus { get; set; }
    public DateTime? InitializationTime { get; set; }
    public TimeSpan Uptime { get; set; }
    public int ErrorCount { get; set; }
}

public class FirebaseDebugInfo
{
    public bool IsInitialized { get; set; }
    public FirebaseDependencyStatus DependencyStatus { get; set; }
    public string? ProjectId { get; set; }
    public string? AppName { get; set; }
    public List<string> RecentErrors { get; set; } = new();
    public DateTime? LastErrorTime { get; set; }
}

public class FirebaseError
{
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
    public DateTime Timestamp { get; set; }
    public Exception? Exception { get; set; }
}
