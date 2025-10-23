using System;

namespace LablabBean.Contracts.Audio;

/// <summary>
/// Handle to a playing audio instance for control and management
/// </summary>
public readonly struct AudioHandle : IEquatable<AudioHandle>
{
    /// <summary>
    /// Unique identifier for this audio instance
    /// </summary>
    public readonly int Id;

    /// <summary>
    /// The audio source type that created this handle
    /// </summary>
    public readonly AudioSourceType SourceType;

    /// <summary>
    /// The audio category this handle belongs to
    /// </summary>
    public readonly AudioCategory Category;

    /// <summary>
    /// Path to the original audio asset
    /// </summary>
    public readonly string AudioPath;

    /// <summary>
    /// Invalid/null handle
    /// </summary>
    public static readonly AudioHandle Invalid = new(0, AudioSourceType.Native, AudioCategory.SFX, string.Empty);

    /// <summary>
    /// Whether this handle is valid
    /// </summary>
    public bool IsValid => Id != 0 && !string.IsNullOrEmpty(AudioPath);

    public AudioHandle(int id, AudioSourceType sourceType, AudioCategory category, string audioPath)
    {
        Id = id;
        SourceType = sourceType;
        Category = category;
        AudioPath = audioPath;
    }

    public bool Equals(AudioHandle other)
    {
        return Id == other.Id && SourceType == other.SourceType;
    }

    public override bool Equals(object? obj)
    {
        return obj is AudioHandle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, (int)SourceType);
    }

    public static bool operator ==(AudioHandle left, AudioHandle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AudioHandle left, AudioHandle right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"AudioHandle(Id: {Id}, Source: {SourceType}, Category: {Category}, Path: {AudioPath})";
    }
}
