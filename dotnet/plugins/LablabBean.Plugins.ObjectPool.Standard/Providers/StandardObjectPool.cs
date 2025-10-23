using System;
using System.Collections.Concurrent;
using LablabBean.Contracts.ObjectPool;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.ObjectPool.Standard.Providers;

public class StandardObjectPool<T> : IObjectPool<T> where T : class
{
    private readonly ConcurrentBag<T> _items = new();
    private readonly Func<T> _createFunc;
    private readonly Action<T>? _resetAction;
    private readonly Action<T>? _destroyAction;
    private readonly ILogger _logger;
    private int _countAll;
    private int _countActive;

    public string Identifier { get; }
    public Type ObjectType => typeof(T);
    public int MaxSize { get; }
    public int CountAll => _countAll;
    public int CountActive => _countActive;
    public int CountInactive => _items.Count;

    public StandardObjectPool(
        string identifier,
        Func<T> createFunc,
        Action<T>? resetAction,
        Action<T>? destroyAction,
        int maxSize,
        int preallocateCount,
        ILogger logger)
    {
        Identifier = identifier;
        _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
        _resetAction = resetAction;
        _destroyAction = destroyAction;
        MaxSize = maxSize;
        _logger = logger;

        // Preallocate objects
        for (int i = 0; i < preallocateCount; i++)
        {
            var item = _createFunc();
            _items.Add(item);
            System.Threading.Interlocked.Increment(ref _countAll);
        }
    }

    public T Get()
    {
        if (_items.TryTake(out var item))
        {
            System.Threading.Interlocked.Increment(ref _countActive);
            return item;
        }

        // Create new object
        var newItem = _createFunc();
        System.Threading.Interlocked.Increment(ref _countAll);
        System.Threading.Interlocked.Increment(ref _countActive);
        return newItem;
    }

    public void Return(T item)
    {
        if (item == null) return;

        _resetAction?.Invoke(item);

        if (MaxSize > 0 && _items.Count >= MaxSize)
        {
            _destroyAction?.Invoke(item);
            System.Threading.Interlocked.Decrement(ref _countAll);
            System.Threading.Interlocked.Decrement(ref _countActive);
            return;
        }

        _items.Add(item);
        System.Threading.Interlocked.Decrement(ref _countActive);
    }

    public void Clear()
    {
        while (_items.TryTake(out var item))
        {
            _destroyAction?.Invoke(item);
            System.Threading.Interlocked.Decrement(ref _countAll);
        }
    }

    public void Dispose()
    {
        Clear();
    }
}
