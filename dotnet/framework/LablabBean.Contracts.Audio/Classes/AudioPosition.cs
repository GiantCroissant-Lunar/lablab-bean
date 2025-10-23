using System;

namespace LablabBean.Contracts.Audio;

/// <summary>
/// Represents a 3D position for spatial audio (replaces Unity Vector3)
/// </summary>
public readonly struct AudioPosition : IEquatable<AudioPosition>
{
    public readonly float X;
    public readonly float Y;
    public readonly float Z;

    public AudioPosition(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static readonly AudioPosition Zero = new(0, 0, 0);

    public bool Equals(AudioPosition other)
    {
        return Math.Abs(X - other.X) < 0.001f &&
               Math.Abs(Y - other.Y) < 0.001f &&
               Math.Abs(Z - other.Z) < 0.001f;
    }

    public override bool Equals(object? obj)
    {
        return obj is AudioPosition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public static bool operator ==(AudioPosition left, AudioPosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AudioPosition left, AudioPosition right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"({X:F2}, {Y:F2}, {Z:F2})";
    }
}
