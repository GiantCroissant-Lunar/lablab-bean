using System;

namespace LablabBean.Contracts.ObjectPool;

/// <summary>
/// Information about an active object pool
/// </summary>
[Serializable]
public class PoolInfo
{
    /// <summary>
    /// Unique identifier of the pool
    /// </summary>
    public string Identifier { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of objects managed by this pool
    /// </summary>
    public Type? ObjectType { get; set; }
    
    /// <summary>
    /// Current pool statistics
    /// </summary>
    public PoolStatistics? Statistics { get; set; }
    
    /// <summary>
    /// When this pool was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Last time this pool was accessed
    /// </summary>
    public DateTime LastAccessedAt { get; set; }
    
    /// <summary>
    /// Whether this pool is currently active and accepting operations
    /// </summary>
    public bool IsActive { get; set; }

    public PoolInfo()
    {
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        LastAccessedAt = DateTime.UtcNow;
    }

    public PoolInfo(string identifier, Type objectType)
        : this()
    {
        Identifier = identifier;
        ObjectType = objectType;
    }

    /// <summary>
    /// Age of this pool
    /// </summary>
    public TimeSpan Age => DateTime.UtcNow - CreatedAt;
    
    /// <summary>
    /// Time since last access
    /// </summary>
    public TimeSpan IdleTime => DateTime.UtcNow - LastAccessedAt;
    
    /// <summary>
    /// Update the last accessed timestamp
    /// </summary>
    public void MarkAccessed()
    {
        LastAccessedAt = DateTime.UtcNow;
    }

    public override string ToString()
    {
        var objectName = ObjectType?.Name ?? "Unknown";
        return $"PoolInfo[{Identifier}]: {objectName}, Age: {Age.TotalMinutes:F1}min, Idle: {IdleTime.TotalSeconds:F0}s";
    }
}
