using System;
using System.Collections.Generic;
using System.Threading;

namespace LablabBean.Contracts.Audio;

/// <summary>
/// Complete audio playback request with all parameters
/// </summary>
public readonly struct AudioRequest : IEquatable<AudioRequest>
{
    /// <summary>
    /// Path to the audio asset to play
    /// </summary>
    public readonly string AudioPath;

    /// <summary>
    /// Audio source type to use for playback
    /// </summary>
    public readonly AudioSourceType SourceType;

    /// <summary>
    /// Audio category for volume mixing
    /// </summary>
    public readonly AudioCategory Category;

    /// <summary>
    /// Playback priority
    /// </summary>
    public readonly AudioPriority Priority;

    /// <summary>
    /// Volume (0.0 to 1.0)
    /// </summary>
    public readonly float Volume;

    /// <summary>
    /// Pitch multiplier (default 1.0)
    /// </summary>
    public readonly float Pitch;

    /// <summary>
    /// Whether audio should loop
    /// </summary>
    public readonly bool Loop;

    /// <summary>
    /// 3D position for spatial audio (null for 2D audio)
    /// </summary>
    public readonly AudioPosition? Position;

    /// <summary>
    /// Fade in duration in seconds
    /// </summary>
    public readonly float FadeInDuration;

    /// <summary>
    /// Cancellation token for the request
    /// </summary>
    public readonly CancellationToken CancellationToken;

    /// <summary>
    /// Additional metadata for provider-specific parameters
    /// </summary>
    public readonly IReadOnlyDictionary<string, object> Metadata;

    public AudioRequest(
        string audioPath,
        AudioSourceType sourceType,
        AudioCategory category = AudioCategory.SFX,
        AudioPriority priority = AudioPriority.Normal,
        float volume = 1.0f,
        float pitch = 1.0f,
        bool loop = false,
        AudioPosition? position = null,
        float fadeInDuration = 0.0f,
        CancellationToken cancellationToken = default,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        AudioPath = audioPath;
        SourceType = sourceType;
        Category = category;
        Priority = priority;
        Volume = volume;
        Pitch = pitch;
        Loop = loop;
        Position = position;
        FadeInDuration = fadeInDuration;
        CancellationToken = cancellationToken;
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Create a simple 2D audio request
    /// </summary>
    public static AudioRequest Simple(string audioPath, AudioSourceType sourceType, AudioCategory category = AudioCategory.SFX)
    {
        return new AudioRequest(audioPath, sourceType, category);
    }

    /// <summary>
    /// Create a 3D spatial audio request
    /// </summary>
    public static AudioRequest Spatial(string audioPath, AudioSourceType sourceType, AudioPosition position, AudioCategory category = AudioCategory.SFX)
    {
        return new AudioRequest(audioPath, sourceType, category, position: position);
    }

    /// <summary>
    /// Create a looping audio request
    /// </summary>
    public static AudioRequest Looping(string audioPath, AudioSourceType sourceType, AudioCategory category = AudioCategory.Music)
    {
        return new AudioRequest(audioPath, sourceType, category, loop: true);
    }

    public bool Equals(AudioRequest other)
    {
        return AudioPath == other.AudioPath &&
               SourceType == other.SourceType &&
               Category == other.Category &&
               Priority == other.Priority &&
               Math.Abs(Volume - other.Volume) < 0.001f &&
               Math.Abs(Pitch - other.Pitch) < 0.001f &&
               Loop == other.Loop &&
               Nullable.Equals(Position, other.Position);
    }

    public override bool Equals(object? obj)
    {
        return obj is AudioRequest other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AudioPath, (int)SourceType, (int)Category, (int)Priority);
    }

    public override string ToString()
    {
        return $"AudioRequest(Path: {AudioPath}, Source: {SourceType}, Category: {Category}, Priority: {Priority})";
    }
}
