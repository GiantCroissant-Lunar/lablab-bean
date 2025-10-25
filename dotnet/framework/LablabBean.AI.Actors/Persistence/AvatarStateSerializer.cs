using System.Text.Json;
using LablabBean.AI.Core.Models;

namespace LablabBean.AI.Actors.Persistence;

/// <summary>
/// Serializes and deserializes avatar state for persistence
/// </summary>
public class AvatarStateSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static string Serialize(AvatarState state)
    {
        return JsonSerializer.Serialize(state, Options);
    }

    public static AvatarState? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<AvatarState>(json, Options);
    }

    public static byte[] SerializeToBytes(AvatarState state)
    {
        return JsonSerializer.SerializeToUtf8Bytes(state, Options);
    }

    public static AvatarState? DeserializeFromBytes(byte[] bytes)
    {
        return JsonSerializer.Deserialize<AvatarState>(bytes, Options);
    }
}

/// <summary>
/// Snapshot for avatar state persistence
/// </summary>
public class AvatarStateSnapshot
{
    public string EntityId { get; set; } = string.Empty;
    public string StateJson { get; set; } = string.Empty;
    public string MemoryJson { get; set; } = string.Empty;
    public DateTime SnapshotTime { get; set; } = DateTime.UtcNow;

    public static AvatarStateSnapshot Create(AvatarState state, AvatarMemory memory)
    {
        return new AvatarStateSnapshot
        {
            EntityId = state.EntityId,
            StateJson = AvatarStateSerializer.Serialize(state),
            MemoryJson = JsonSerializer.Serialize(memory, new JsonSerializerOptions { WriteIndented = true }),
            SnapshotTime = DateTime.UtcNow
        };
    }

    public (AvatarState?, AvatarMemory?) Restore()
    {
        var state = AvatarStateSerializer.Deserialize(StateJson);
        var memory = JsonSerializer.Deserialize<AvatarMemory>(MemoryJson);
        return (state, memory);
    }
}
