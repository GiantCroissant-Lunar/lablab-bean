using System;

namespace LablabBean.Contracts.Scheduler.Interfaces;

public interface IScheduledTask
{
    string Id { get; }
    string? Name { get; }
    ScheduledTaskState State { get; }
    DateTime ScheduledTime { get; }
    DateTime? ExecutedTime { get; }
    TimeSpan? Delay { get; }
    TimeSpan? Interval { get; }
    bool IsRepeating { get; }
    void Cancel();
}
