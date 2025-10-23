using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Contracts.Performance.Interfaces;

namespace LablabBean.Contracts.Performance.Services;

public interface IService
{
    void RecordMetric(string metricName, TimeSpan duration, IDictionary<string, object>? metadata);
    void RecordCounter(string counterName, long value, IDictionary<string, object>? metadata);
    void RecordGauge(string gaugeName, double value, IDictionary<string, object>? metadata);
    IPerformanceActivity StartActivity(string activityName, IDictionary<string, object>? metadata);
    void RecordException(string source, Exception exception, IDictionary<string, object>? metadata);
    Task<PerformanceStatistics> GetStatisticsAsync();
    Task<PerformanceStatistics> GetStatisticsAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken);
    PerformanceHealthStatus GetHealthStatus();
    Task FlushAsync(CancellationToken cancellationToken);
    Task<IEnumerable<PerformanceRecommendation>> GetRecommendationsAsync();
    void ResetStatistics();
}
