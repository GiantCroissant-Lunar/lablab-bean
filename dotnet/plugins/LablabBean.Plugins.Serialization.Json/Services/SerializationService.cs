using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Contracts.Serialization;
using LablabBean.Contracts.Serialization.Services;
using LablabBean.Plugins.Serialization.Json.Providers;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Serialization.Json.Services;

public class SerializationService : IService
{
    private readonly ILogger _logger;
    private readonly JsonSerializationProvider _jsonProvider;

    public SerializationService(ILogger logger)
    {
        _logger = logger;
        _jsonProvider = new JsonSerializationProvider(logger);
    }

    public async Task<byte[]> SerializeAsync<T>(T obj, SerializationFormat format, CancellationToken cancellationToken)
    {
        if (format != SerializationFormat.Json)
        {
            throw new NotSupportedException($"Format {format} is not supported");
        }

        return await _jsonProvider.SerializeAsync(obj, cancellationToken);
    }

    public async Task<T> DeserializeAsync<T>(byte[] data, SerializationFormat format, CancellationToken cancellationToken)
    {
        if (format != SerializationFormat.Json)
        {
            throw new NotSupportedException($"Format {format} is not supported");
        }

        return await _jsonProvider.DeserializeAsync<T>(data, cancellationToken);
    }

    public async Task<string> SerializeToStringAsync<T>(T obj, SerializationFormat format, CancellationToken cancellationToken)
    {
        if (format != SerializationFormat.Json)
        {
            throw new NotSupportedException($"Format {format} is not supported");
        }

        return await _jsonProvider.SerializeToStringAsync(obj, cancellationToken);
    }

    public async Task<T> DeserializeFromStringAsync<T>(string data, SerializationFormat format, CancellationToken cancellationToken)
    {
        if (format != SerializationFormat.Json)
        {
            throw new NotSupportedException($"Format {format} is not supported");
        }

        return await _jsonProvider.DeserializeFromStringAsync<T>(data, cancellationToken);
    }

    public async Task SerializeToStreamAsync<T>(T obj, Stream stream, SerializationFormat format, CancellationToken cancellationToken)
    {
        if (format != SerializationFormat.Json)
        {
            throw new NotSupportedException($"Format {format} is not supported");
        }

        await _jsonProvider.SerializeToStreamAsync(obj, stream, cancellationToken);
    }

    public async Task<T> DeserializeFromStreamAsync<T>(Stream stream, SerializationFormat format, CancellationToken cancellationToken)
    {
        if (format != SerializationFormat.Json)
        {
            throw new NotSupportedException($"Format {format} is not supported");
        }

        return await _jsonProvider.DeserializeFromStreamAsync<T>(stream, cancellationToken);
    }

    public bool IsFormatSupported(SerializationFormat format)
    {
        return format == SerializationFormat.Json;
    }

    public SerializationFormat[] GetSupportedFormats()
    {
        return new[] { SerializationFormat.Json };
    }

    public bool CanSerialize<T>(T obj, SerializationFormat format)
    {
        if (format != SerializationFormat.Json)
        {
            return false;
        }

        return _jsonProvider.CanSerialize(obj);
    }

    public long GetEstimatedSize<T>(T obj, SerializationFormat format)
    {
        if (format != SerializationFormat.Json)
        {
            throw new NotSupportedException($"Format {format} is not supported");
        }

        return _jsonProvider.GetEstimatedSize(obj);
    }
}
