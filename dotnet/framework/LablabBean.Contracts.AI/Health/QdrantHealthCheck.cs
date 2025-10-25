using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace LablabBean.Contracts.AI.Health;

/// <summary>
/// Health check for Qdrant vector database connectivity
/// </summary>
public class QdrantHealthCheck : IHealthCheck
{
    private readonly string _qdrantEndpoint;
    private readonly ILogger<QdrantHealthCheck> _logger;
    private readonly HttpClient _httpClient;

    public QdrantHealthCheck(
        string qdrantEndpoint,
        ILogger<QdrantHealthCheck> logger,
        IHttpClientFactory httpClientFactory)
    {
        _qdrantEndpoint = qdrantEndpoint ?? throw new ArgumentNullException(nameof(qdrantEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClientFactory.CreateClient("QdrantHealthCheck");
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var healthEndpoint = $"{_qdrantEndpoint.TrimEnd('/')}/health";

            _logger.LogDebug("Checking Qdrant health at {Endpoint}", healthEndpoint);

            var response = await _httpClient.GetAsync(healthEndpoint, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Qdrant health check succeeded");

                return HealthCheckResult.Healthy(
                    description: $"Qdrant is available at {_qdrantEndpoint}",
                    data: new Dictionary<string, object>
                    {
                        { "endpoint", _qdrantEndpoint },
                        { "status_code", (int)response.StatusCode }
                    });
            }

            _logger.LogWarning(
                "Qdrant health check returned non-success status: {StatusCode}",
                response.StatusCode);

            return HealthCheckResult.Degraded(
                description: $"Qdrant returned status code {response.StatusCode}",
                data: new Dictionary<string, object>
                {
                    { "endpoint", _qdrantEndpoint },
                    { "status_code", (int)response.StatusCode }
                });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Qdrant health check failed with HTTP error");

            return HealthCheckResult.Unhealthy(
                description: $"Cannot connect to Qdrant at {_qdrantEndpoint}",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "endpoint", _qdrantEndpoint },
                    { "error", ex.Message }
                });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Qdrant health check timed out");

            return HealthCheckResult.Unhealthy(
                description: $"Qdrant health check timed out for {_qdrantEndpoint}",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "endpoint", _qdrantEndpoint },
                    { "error", "Timeout" }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Qdrant health check");

            return HealthCheckResult.Unhealthy(
                description: "Unexpected error checking Qdrant health",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "endpoint", _qdrantEndpoint },
                    { "error", ex.Message }
                });
        }
    }
}
