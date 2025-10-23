using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using LablabBean.Contracts.Serialization.Configuration;

namespace LablabBean.Contracts.Serialization.Services;

public class MessagePackSerializationService : IService
{
    private readonly MessagePackSerializerOptions _options;

    public MessagePackSerializationService(MessagePackOptions? options = null)
    {
        var config = options ?? MessagePackOptions.CreateDefault();
        _options = config.ToSerializerOptions();
    }

    public MessagePackSerializationService(MessagePackSerializerOptions options)
    {
        _options = options;
    }

    public async Task<byte[]> SerializeAsync<T>(T obj, SerializationFormat format, CancellationToken cancellationToken)
    {
        if (format != SerializationFormat.MessagePack)
            throw new NotSupportedException($"Format {format} is not supported by MessagePackSerializationService");

        if (obj == null)
            return Array.Empty<byte>();

        return await Task.Run(() => MessagePackSerializer.Serialize(obj, _options, cancellationToken), cancellationToken);
    }

    public async Task<T> DeserializeAsync<T>(byte[] data, SerializationFormat format, CancellationToken cancellationToken)
    {
        if (format != SerializationFormat.MessagePack)
            throw new NotSupportedException($"Format {format} is not supported by MessagePackSerializationService");

        if (data == null || data.Length == 0)
            return default(T)!;

        return await Task.Run(() => MessagePackSerializer.Deserialize<T>(data, _options, cancellationToken), cancellationToken);
    }
  public async Task<string> SerializeToStringAsync<T>(T obj, SerializationFormat format, CancellationToken cancellationToken)
    {
        if (format != SerializationFormat.MessagePack)
            throw new NotSupportedException($"Format {format} is not supported by MessagePackSerializationService");

        var bytes = await SerializeAsync(obj, format, cancellationToken);
        return Convert.ToBase64String(bytes);
    }

    public async Task<T> DeserializeFromStringAsync<T>(string data, SerializationFormat format, CancellationToken cancellationToken)
    {
        if (format != SerializationFormat.MessagePack)
            throw new NotSupportedException($"Format {format} is not supported by MessagePackSerializationService");

        if (string.IsNullOrEmpty(data))
            return default(T)!;

        var bytes = Convert.FromBase64String(data);
        return await DeserializeAsync<T>(bytes, format, cancellationToken);
    }

    public async Task SerializeToStreamAsync<T>(T obj, Stream stream, SerializationFormat format, CancellationToken cancellationToken)
    {
        if (format != SerializationFormat.MessagePack)
            throw new NotSupportedException($"Format {format} is not supported by MessagePackSerializationService");

        if (obj == null)
            return;

        await MessagePackSerializer.SerializeAsync(stream, obj, _options, cancellationToken);
    }

    public async Task<T> DeserializeFromStreamAsync<T>(Stream stream, SerializationFormat format, CancellationToken cancellationToken)
    {
        if (format != SerializationFormat.MessagePack)
            throw new NotSupportedException($"Format {format} is not supported by MessagePackSerializationService");

        if (stream == null || stream.Length == 0)
            return default(T)!;

        return await MessagePackSerializer.DeserializeAsync<T>(stream, _options, cancellationToken);
    }    public
 bool IsFormatSupported(SerializationFormat format)
    {
        return format == SerializationFormat.MessagePack;
    }

    public SerializationFormat[] GetSupportedFormats()
    {
        return new[] { SerializationFormat.MessagePack };
    }

    public bool CanSerialize<T>(T obj, SerializationFormat format)
    {
        if (format != SerializationFormat.MessagePack)
            return false;

        if (obj == null)
            return true;

        try
        {
            // Quick check if type can be serialized by attempting to get formatter
            var formatter = _options.Resolver.GetFormatter<T>();
            return formatter != null;
        }
        catch
        {
            return false;
        }
    }

    public long GetEstimatedSize<T>(T obj, SerializationFormat format)
    {
        if (format != SerializationFormat.MessagePack)
            throw new NotSupportedException($"Format {format} is not supported by MessagePackSerializationService");

        if (obj == null)
            return 0;

        try
        {
            // For MessagePack, we need to actually serialize to get accurate size
            // This is a rough estimation - in production, you might want to cache this
            var bytes = MessagePackSerializer.Serialize(obj, _options);
            return bytes.Length;
        }
        catch
        {
            return -1; // Indicates unable to estimate
        }
    }
}
