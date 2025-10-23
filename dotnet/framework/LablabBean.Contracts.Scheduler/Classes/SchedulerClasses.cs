using System;

namespace LablabBean.Contracts.Scheduler;

public class ScheduledTaskRequest
{
    public string? Name { get; set; }
    public Action? Action { get; set; }
    public Func<System.Threading.Tasks.Task>? AsyncAction { get; set; }
    public TimeSpan Delay { get; set; }
    public TimeSpan? Interval { get; set; }
    public bool Repeating { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;
    public System.Threading.CancellationToken CancellationToken { get; set; }
}

public class SchedulerStats
{
    public int ActiveTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int CancelledTasks { get; set; }
    public int FailedTasks { get; set; }
    public TimeSpan AverageExecutionTime { get; set; }
    public DateTime LastExecution { get; set; }
}
