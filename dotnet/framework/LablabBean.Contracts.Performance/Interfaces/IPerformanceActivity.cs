using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Performance.Interfaces;

public interface IPerformanceActivity : IDisposable
{
    string ActivityName { get; }
    DateTime StartTime { get; }
    TimeSpan Duration { get; }
    void AddMetadata(string key, object value);
    void MarkFailed(Exception exception);
}
