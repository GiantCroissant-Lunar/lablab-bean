using System;

namespace LablabBean.Contracts.ObjectPool;

/// <summary>
/// Generic object pool interface for efficient object reuse
/// </summary>
public interface IObjectPool<T> : IDisposable where T : class
{
    /// <summary>
    /// Get an object from the pool, creating a new one if necessary
    /// </summary>
    T Get();

    /// <summary>
    /// Return an object to the pool for reuse
    /// </summary>
    void Return(T item);

    /// <summary>
    /// Clear all objects from the pool
    /// </summary>
    void Clear();

    /// <summary>
    /// The maximum number of objects this pool can hold
    /// </summary>
    int MaxSize { get; }

    /// <summary>
    /// The current number of objects in the pool
    /// </summary>
    int CountAll { get; }

    /// <summary>
    /// The number of objects currently available in the pool
    /// </summary>
    int CountInactive { get; }

    /// <summary>
    /// The number of objects currently checked out from the pool
    /// </summary>
    int CountActive { get; }

    /// <summary>
    /// Unique identifier for this pool
    /// </summary>
    string Identifier { get; }

    /// <summary>
    /// The type of objects managed by this pool
    /// </summary>
    Type ObjectType { get; }
}
