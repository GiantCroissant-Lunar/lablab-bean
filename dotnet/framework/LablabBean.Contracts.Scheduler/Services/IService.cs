using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LablabBean.Contracts.Scheduler.Interfaces;

namespace LablabBean.Contracts.Scheduler.Services;

public interface IService
{
    IScheduledTask ScheduleDelayed(Action action, TimeSpan delay);
    IScheduledTask ScheduleRepeating(Action action, TimeSpan interval);
    IScheduledTask ScheduleDelayedAsync(Func<Task> taskFunc, TimeSpan delay);
    IScheduledTask ScheduleRepeatingAsync(Func<Task> taskFunc, TimeSpan interval);
    IScheduledTask Schedule(ScheduledTaskRequest request);
    void CancelTask(IScheduledTask task);
    void CancelAllTasks();
    IEnumerable<IScheduledTask> GetActiveTasks();
    SchedulerStats GetStats();
    void PauseAllTasks();
    void ResumeAllTasks();
}
