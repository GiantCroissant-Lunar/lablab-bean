using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Contracts.Scheduler;
using LablabBean.Contracts.Scheduler.Interfaces;
using LablabBean.Contracts.Scheduler.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Scheduler.Standard.Services;

public class SchedulerService : IService, IDisposable
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, ScheduledTaskItem> _tasks = new();
    private readonly object _statsLock = new();
    
    private int _completedCount;
    private int _cancelledCount;
    private int _failedCount;
    private DateTime _lastExecution = DateTime.MinValue;
    private TimeSpan _totalExecutionTime = TimeSpan.Zero;
    private bool _isPaused;
    private bool _disposed;

    public SchedulerService(ILogger logger)
    {
        _logger = logger;
    }

    public IScheduledTask ScheduleDelayed(Action action, TimeSpan delay)
    {
        var request = new ScheduledTaskRequest
        {
            Action = action,
            Delay = delay,
            Repeating = false
        };
        return Schedule(request);
    }

    public IScheduledTask ScheduleRepeating(Action action, TimeSpan interval)
    {
        var request = new ScheduledTaskRequest
        {
            Action = action,
            Delay = interval,
            Interval = interval,
            Repeating = true
        };
        return Schedule(request);
    }

    public IScheduledTask ScheduleDelayedAsync(Func<Task> taskFunc, TimeSpan delay)
    {
        var request = new ScheduledTaskRequest
        {
            AsyncAction = taskFunc,
            Delay = delay,
            Repeating = false
        };
        return Schedule(request);
    }

    public IScheduledTask ScheduleRepeatingAsync(Func<Task> taskFunc, TimeSpan interval)
    {
        var request = new ScheduledTaskRequest
        {
            AsyncAction = taskFunc,
            Delay = interval,
            Interval = interval,
            Repeating = true
        };
        return Schedule(request);
    }

    public IScheduledTask Schedule(ScheduledTaskRequest request)
    {
        var task = new ScheduledTaskItem(request, this);
        _tasks[task.Id] = task;
        
        _logger.LogInformation("Scheduled task {TaskId} ({Name}) with delay {Delay}", 
            task.Id, task.Name ?? "Unnamed", request.Delay);
        
        task.Start();
        return task;
    }

    public void CancelTask(IScheduledTask task)
    {
        if (_tasks.TryRemove(task.Id, out var item))
        {
            item.Cancel();
            Interlocked.Increment(ref _cancelledCount);
            _logger.LogInformation("Cancelled task {TaskId}", task.Id);
        }
    }

    public void CancelAllTasks()
    {
        _logger.LogInformation("Cancelling all tasks ({Count})", _tasks.Count);
        
        foreach (var task in _tasks.Values.ToList())
        {
            task.Cancel();
        }
        
        _tasks.Clear();
    }

    public IEnumerable<IScheduledTask> GetActiveTasks()
    {
        return _tasks.Values.Where(t => t.State == ScheduledTaskState.Pending || t.State == ScheduledTaskState.Running);
    }

    public SchedulerStats GetStats()
    {
        lock (_statsLock)
        {
            var activeCount = _tasks.Count;
            var avgTime = _completedCount > 0 
                ? TimeSpan.FromTicks(_totalExecutionTime.Ticks / _completedCount) 
                : TimeSpan.Zero;

            return new SchedulerStats
            {
                ActiveTasks = activeCount,
                CompletedTasks = _completedCount,
                CancelledTasks = _cancelledCount,
                FailedTasks = _failedCount,
                AverageExecutionTime = avgTime,
                LastExecution = _lastExecution
            };
        }
    }

    public void PauseAllTasks()
    {
        _isPaused = true;
        _logger.LogInformation("Paused all tasks");
    }

    public void ResumeAllTasks()
    {
        _isPaused = false;
        _logger.LogInformation("Resumed all tasks");
    }

    private void OnTaskCompleted(ScheduledTaskItem task, TimeSpan executionTime)
    {
        lock (_statsLock)
        {
            Interlocked.Increment(ref _completedCount);
            _lastExecution = DateTime.UtcNow;
            _totalExecutionTime = _totalExecutionTime.Add(executionTime);
        }

        if (!task.IsRepeating)
        {
            _tasks.TryRemove(task.Id, out _);
        }
    }

    private void OnTaskFailed(ScheduledTaskItem task)
    {
        Interlocked.Increment(ref _failedCount);
        _tasks.TryRemove(task.Id, out _);
    }

    private bool IsPaused => _isPaused;

    public void Dispose()
    {
        if (_disposed) return;
        
        CancelAllTasks();
        _disposed = true;
    }

    private class ScheduledTaskItem : IScheduledTask
    {
        private readonly ScheduledTaskRequest _request;
        private readonly SchedulerService _scheduler;
        private CancellationTokenSource? _cts;
        private Timer? _timer;
        private ScheduledTaskState _state = ScheduledTaskState.Pending;

        public string Id { get; } = Guid.NewGuid().ToString();
        public string? Name => _request.Name;
        public ScheduledTaskState State => _state;
        public DateTime ScheduledTime { get; }
        public DateTime? ExecutedTime { get; private set; }
        public TimeSpan? Delay => _request.Delay;
        public TimeSpan? Interval => _request.Interval;
        public bool IsRepeating => _request.Repeating;

        public ScheduledTaskItem(ScheduledTaskRequest request, SchedulerService scheduler)
        {
            _request = request;
            _scheduler = scheduler;
            ScheduledTime = DateTime.UtcNow.Add(request.Delay);
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            var dueTime = (int)_request.Delay.TotalMilliseconds;
            var period = _request.Repeating && _request.Interval.HasValue 
                ? (int)_request.Interval.Value.TotalMilliseconds 
                : Timeout.Infinite;

            _timer = new Timer(ExecuteCallback, null, dueTime, period);
        }

        private async void ExecuteCallback(object? state)
        {
            if (_cts?.Token.IsCancellationRequested ?? true) return;
            if (_scheduler.IsPaused) return;

            _state = ScheduledTaskState.Running;
            ExecutedTime = DateTime.UtcNow;
            var startTime = DateTime.UtcNow;

            try
            {
                if (_request.AsyncAction != null)
                {
                    await _request.AsyncAction();
                }
                else if (_request.Action != null)
                {
                    _request.Action();
                }

                var executionTime = DateTime.UtcNow - startTime;
                _state = ScheduledTaskState.Completed;
                _scheduler.OnTaskCompleted(this, executionTime);
            }
            catch (Exception ex)
            {
                _state = ScheduledTaskState.Failed;
                _scheduler._logger.LogError(ex, "Task {TaskId} failed", Id);
                _scheduler.OnTaskFailed(this);
                
                if (!IsRepeating)
                {
                    Cancel();
                }
            }
        }

        public void Cancel()
        {
            _state = ScheduledTaskState.Cancelled;
            _cts?.Cancel();
            _timer?.Dispose();
            _cts?.Dispose();
        }
    }
}
