using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Serialization.Json.Providers;

public class JsonSerializationProvider
{
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _options;

    public JsonSerializationProvider(ILogger logger)
    {
        _logger = logger;
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<byte[]> SerializeAsync<T>(T obj, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, obj, _options, cancellationToken);
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize object to JSON");
            throw;
        }
    }

    public async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new MemoryStream(data);
            var result = await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken);
            return result ?? throw new InvalidOperationException("Deserialization resulted in null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize JSON to object");
            throw;
        }
    }

    public Task<string> SerializeToStringAsync<T>(T obj, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(obj, _options);
            return Task.FromResult(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize object to JSON string");
            throw;
        }
    }

    public Task<T> DeserializeFromStringAsync<T>(string data, CancellationToken cancellationToken)
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(data, _options);
            return Task.FromResult(result ?? throw new InvalidOperationException("Deserialization resulted in null"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize JSON string to object");
            throw;
        }
    }

    public async Task SerializeToStreamAsync<T>(T obj, Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            await JsonSerializer.SerializeAsync(stream, obj, _options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize object to JSON stream");
            throw;
        }
    }

    public async Task<T> DeserializeFromStreamAsync<T>(Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            var result = await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken);
            return result ?? throw new InvalidOperationException("Deserialization resulted in null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize JSON stream to object");
            throw;
        }
    }

    public bool CanSerialize<T>(T obj)
    {
        if (obj == null) return false;

        try
        {
            JsonSerializer.Serialize(obj, _options);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public long GetEstimatedSize<T>(T obj)
    {
        if (obj == null) return 0;

        try
        {
            var json = JsonSerializer.Serialize(obj, _options);
            return Encoding.UTF8.GetByteCount(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to estimate size");
            return 0;
        }
    }
}
