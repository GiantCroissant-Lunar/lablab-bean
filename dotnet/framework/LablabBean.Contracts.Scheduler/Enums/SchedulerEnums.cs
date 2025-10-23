namespace LablabBean.Contracts.Scheduler;

public enum TaskPriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum ScheduledTaskState
{
    Pending,
    Running,
    Completed,
    Cancelled,
    Failed
}
