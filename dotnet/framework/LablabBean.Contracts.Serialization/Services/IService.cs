using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LablabBean.Contracts.Serialization.Services;

public interface IService
{
    Task<byte[]> SerializeAsync<T>(T obj, SerializationFormat format, CancellationToken cancellationToken);
    Task<T> DeserializeAsync<T>(byte[] data, SerializationFormat format, CancellationToken cancellationToken);
    Task<string> SerializeToStringAsync<T>(T obj, SerializationFormat format, CancellationToken cancellationToken);
    Task<T> DeserializeFromStringAsync<T>(string data, SerializationFormat format, CancellationToken cancellationToken);
    Task SerializeToStreamAsync<T>(T obj, Stream stream, SerializationFormat format, CancellationToken cancellationToken);
    Task<T> DeserializeFromStreamAsync<T>(Stream stream, SerializationFormat format, CancellationToken cancellationToken);
    bool IsFormatSupported(SerializationFormat format);
    SerializationFormat[] GetSupportedFormats();
    bool CanSerialize<T>(T obj, SerializationFormat format);
    long GetEstimatedSize<T>(T obj, SerializationFormat format);
}
