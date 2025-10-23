namespace LablabBean.Contracts.Firebase;

public enum FirebaseDependencyStatus
{
    Unknown,
    Available,
    Unavailable,
    UnavailableUpdating,
    UnavailableDisabled,
    UnavailablePermissionDenied
}

public enum FirebaseInitializationStatus
{
    NotStarted,
    InProgress,
    Success,
    Failed
}
