# MessagePack Serialization Integration

High-performance binary serialization for the LablabBean plugin system using MessagePack-CSharp.

## Overview

This implementation provides MessagePack serialization as a first-class option in the existing serialization contracts, offering significant performance improvements for high-frequency operations and large data sets.

## Key Benefits

- **2-5x faster** serialization for large objects compared to JSON
- **30-60% smaller** payload sizes
- **High throughput**: 166,000+ events/sec for typical game events
- **Memory efficient**: Lower allocation overhead
- **Cross-platform**: Works with TypeScript frontend via MessagePack.js

## Quick Start

### Basic Usage

```csharp
using LablabBean.Contracts.Serialization.Services;
using LablabBean.Contracts.Serialization;

// Create service with default configuration
var serializer = new MessagePackSerializationService();

// Serialize to bytes
var bytes = await serializer.SerializeAsync(gameEvent, SerializationFormat.MessagePack, cancellationToken);

// Deserialize from bytes
var restored = await serializer.DeserializeAsync<GameEvent>(bytes, SerializationFormat.MessagePack, cancellationToken);
```

### With Custom Configuration

```csharp
using LablabBean.Contracts.Serialization.Configuration;

// High performance configuration (no compression, trusted data)
var highPerfOptions = MessagePackOptions.CreateHighPerformance();
var serializer = new MessagePackSerializationService(highPerfOptions);

// Compact configuration (maximum compression)
var compactOptions = MessagePackOptions.CreateCompact();
var compactSerializer = new MessagePackSerializationService(compactOptions);
```

## Configuration Options

### Predefined Configurations

| Configuration | Use Case | Compression | Performance | Size |
|---------------|----------|-------------|-------------|------|
| `CreateDefault()` | General purpose | LZ4 | Balanced | Medium |
| `CreateHighPerformance()` | Real-time events | None | Fastest | Larger |
| `CreateCompact()` | Network transmission | LZ4 | Good | Smallest |

### Custom Configuration

```csharp
var options = new MessagePackOptions
{
    Compression = MessagePackCompression.Lz4BlockArray,
    Resolver = ContractlessStandardResolver.Instance,
    Security = MessagePackSecurity.UntrustedData,
    AllowAssemblyVersionMismatch = false,
    OmitAssemblyVersion = false
};

var serializer = new MessagePackSerializationService(options);
```

## Data Model Attributes

### Attributed Approach (Recommended)

```csharp
[MessagePackObject]
public class GameEvent
{
    [Key(0)] public string EventType { get; set; }
    [Key(1)] public DateTime Timestamp { get; set; }
    [Key(2)] public string PlayerId { get; set; }
    [Key(3)] public Dictionary<string, object> Data { get; set; }
}
```

### Contractless Approach

```csharp
// No attributes needed - uses property names
public class SimpleEvent
{
    public string Type { get; set; }
    public DateTime When { get; set; }
}
```

## Performance Characteristics

### Benchmark Results

| Scenario | MessagePack | JSON | Improvement |
|----------|-------------|------|-------------|
| Small objects (< 1KB) | ~0.73x | 1.0x | JSON faster |
| Large objects (> 10KB) | 5.21x | 1.0x | MessagePack faster |
| Payload size | 1.6-4.6x smaller | 1.0x | MessagePack smaller |
| High-frequency ops | 166K ops/sec | 72K ops/sec | 2.3x faster |

### Memory Usage

- **Lower allocation**: ~40% less memory allocation during serialization
- **Streaming support**: Efficient handling of large objects (>10MB)
- **Compression**: LZ4 compression reduces network/storage overhead

## Integration Patterns

### Plugin Event Serialization

```csharp
public class EventBusPlugin
{
    private readonly IRegistry _registry;

    public async Task PublishEventAsync<T>(T eventData)
    {
        var serializer = _registry.Get<ISerializationService>();
        var bytes = await serializer.SerializeAsync(eventData, SerializationFormat.MessagePack, cancellationToken);

        // Transmit or store bytes
        await TransmitEventAsync(bytes);
    }
}
```

### Analytics Data Batching

```csharp
public class AnalyticsPlugin
{
    public async Task<byte[]> CreateBatchAsync(List<AnalyticsData> sessions)
    {
        var serializer = _registry.Get<ISerializationService>();
        return await serializer.SerializeAsync(sessions, SerializationFormat.MessagePack, cancellationToken);
    }
}
```

### Session State Persistence

```csharp
public class SessionManager
{
    public async Task SaveSessionAsync(SessionState state)
    {
        var serializer = _registry.Get<ISerializationService>();

        using var fileStream = File.Create($"session_{state.PlayerId}.msgpack");
        await serializer.SerializeToStreamAsync(state, fileStream, SerializationFormat.MessagePack, cancellationToken);
    }
}
```

## When to Use MessagePack vs JSON

### Use MessagePack For

- ✅ High-frequency events (player movement, combat actions)
- ✅ Large data sets (analytics batches, session state)
- ✅ Network transmission (bandwidth optimization)
- ✅ Performance-critical paths (real-time systems)
- ✅ Binary storage (save files, caches)

### Use JSON For

- ✅ Configuration files (human-readable)
- ✅ API responses (web compatibility)
- ✅ Debugging scenarios (easy inspection)
- ✅ Small objects (< 1KB, JSON is faster)
- ✅ Development/testing (easier to debug)

## Error Handling

### Common Exceptions

```csharp
try
{
    var result = await serializer.SerializeAsync(data, SerializationFormat.MessagePack, cancellationToken);
}
catch (MessagePackSerializationException ex)
{
    // Handle serialization-specific errors
    logger.LogError("MessagePack serialization failed: {Error}", ex.Message);
}
catch (NotSupportedException ex)
{
    // Handle unsupported format
    logger.LogError("Serialization format not supported: {Error}", ex.Message);
}
```

### Validation

```csharp
// Check if format is supported
if (serializer.IsFormatSupported(SerializationFormat.MessagePack))
{
    // Check if object can be serialized
    if (serializer.CanSerialize(myObject, SerializationFormat.MessagePack))
    {
        var bytes = await serializer.SerializeAsync(myObject, SerializationFormat.MessagePack, cancellationToken);
    }
}
```

## Migration Guide

### From JSON to MessagePack

1. **Add MessagePack attributes** to your data models
2. **Update serialization calls** to use `SerializationFormat.MessagePack`
3. **Test performance** with your specific data patterns
4. **Keep JSON fallback** for debugging and configuration

### Gradual Migration

```csharp
public async Task<byte[]> SerializeEventAsync<T>(T eventData, bool useMessagePack = true)
{
    var format = useMessagePack ? SerializationFormat.MessagePack : SerializationFormat.Json;
    return await serializer.SerializeAsync(eventData, format, cancellationToken);
}
```

## Troubleshooting

### Common Issues

1. **Missing attributes**: Use `ContractlessStandardResolver` or add `[MessagePackObject]` attributes
2. **Version compatibility**: Enable `AllowAssemblyVersionMismatch` for cross-version scenarios
3. **Circular references**: MessagePack doesn't support circular references by default
4. **Large objects**: Use streaming serialization for objects > 10MB

### Debugging Tips

1. **Use JSON for development**: Switch to JSON format during debugging
2. **Enable diagnostics**: Use `MessagePackSecurity.UntrustedData` for better error messages
3. **Size estimation**: Use `GetEstimatedSize()` to predict payload sizes
4. **Performance profiling**: Benchmark with your actual data patterns

## Examples

See the following demo projects for complete examples:

- **[MessagePackDemo](../../examples/MessagePackDemo/)** - Basic usage and features
- **[SerializationBenchmark](../../examples/SerializationBenchmark/)** - Performance comparisons
- **[MessagePackIntegrationDemo](../../examples/MessagePackIntegrationDemo/)** - Real-world scenarios

## Dependencies

- **MessagePack**: 2.5.187 (Cysharp)
- **MessagePack.Annotations**: 2.5.187 (Cysharp)
- **.NET**: 8.0+

## Security Considerations

- Use `MessagePackSecurity.UntrustedData` for external data
- Validate data size limits for network scenarios
- Consider compression overhead for small objects
- Monitor memory usage with large object graphs

---

**Performance**: 2-5x faster than JSON for large objects
**Size**: 30-60% smaller payloads
**Throughput**: 166K+ operations/second
**Integration**: Seamless with existing serialization contracts
