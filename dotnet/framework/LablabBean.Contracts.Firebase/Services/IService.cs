using System;
using System.Threading;
using System.Threading.Tasks;

namespace LablabBean.Contracts.Firebase.Services;

public interface IService
{
    bool IsInitialized { get; }
    FirebaseDependencyStatus DependencyStatus { get; }
    string AppName { get; }
    string ProjectId { get; }
    Task<FirebaseInitializationResult> InitializeAsync(CancellationToken cancellationToken);
    Task<FirebaseDependencyStatus> CheckDependenciesAsync(CancellationToken cancellationToken);
    FirebaseConfig GetConfiguration();
    void SetConfiguration(FirebaseConfig config);
    Task WaitForInitializationAsync(CancellationToken cancellationToken);
    FirebaseStatistics GetStatistics();
    FirebaseDebugInfo GetDebugInfo();
    event EventHandler<FirebaseInitializationResult>? InitializationStatusChanged;
    event EventHandler<FirebaseDependencyStatus>? DependencyStatusChanged;
    event EventHandler<FirebaseError>? ErrorOccurred;
}
