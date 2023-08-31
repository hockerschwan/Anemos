using System.Collections;
using System.Diagnostics;
using Collections.Pooled;

namespace Anemos;

public delegate void QueueChangedEventHandler(object? sender, EventArgs e);

[Serializable]
[DebuggerDisplay("Capacity = {Capacity}")]
public class LimitedPooledQueue<T> : IEnumerable<T>, ICollection, IReadOnlyList<T>, IDisposable
{
    [field: NonSerialized]
    public event QueueChangedEventHandler? QueueChanged;

    private bool _hasDisposed;

    private PooledQueue<T> _queue;

    private int _capacity;
    public int Capacity
    {
        get => _capacity;
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Capacity), _zeroCapacityErrorString);
            }

            if (value == _capacity) { return; }

            if (value > _capacity)
            {
                _capacity = value;
                var newQueue = new PooledQueue<T>(_capacity);
                while (_queue.Count > 0)
                {
                    newQueue.Enqueue(_queue.Dequeue());
                }
                _queue.Dispose();
                _queue = newQueue;
            }
            else
            {
                _capacity = value;
                while (_queue.Count > _capacity)
                {
                    _queue.Dequeue();
                }
                var newQueue = new PooledQueue<T>(_capacity);
                while (_queue.Count > 0)
                {
                    newQueue.Enqueue(_queue.Dequeue());
                }
                _queue.Dispose();
                _queue = newQueue;
            }

            OnQueueChanged();
        }
    }

    public int Count => _queue.Count;

    public bool IsSynchronized => false;

    public object SyncRoot => this;

    public T this[int index] => _queue.ElementAt(index);

    private const string _zeroCapacityErrorString = "Capacity must be greater than 0.";

    public LimitedPooledQueue(int capacity)
    {
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), _zeroCapacityErrorString);
        }
        _capacity = capacity;
        _queue = new(capacity);
    }

    public void Clear()
    {
        _queue.Clear();
        OnQueueChanged();
    }

    public bool Contains(T item) => _queue.Contains(item);

    public void CopyTo(Array array, int index)
    {
        var arr = array as T[];
        _queue.CopyTo(arr, index);
    }

    public T Dequeue()
    {
        var ret = _queue.Dequeue();
        OnQueueChanged();
        return ret;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_hasDisposed)
        {
            if (disposing)
            {
                _queue.Dispose();
            }

            _hasDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Enqueue(T item)
    {
        while (_queue.Count >= Capacity)
        {
            _queue.Dequeue();
        }

        _queue.Enqueue(item);
        OnQueueChanged();
    }

    public void EnqueueRange(IEnumerable<T> items)
    {
        if (!items.Any()) { return; }

        while (_queue.Any() && _queue.Count >= Capacity - items.Count())
        {
            _queue.Dequeue();
        }

        foreach (var item in items)
        {
            _queue.Enqueue(item);
        }
        OnQueueChanged();
    }

    public override bool Equals(object? obj) => this == obj;

    public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override int GetHashCode() => _queue.GetHashCode();

    private void OnQueueChanged()
    {
        QueueChanged?.Invoke(this, EventArgs.Empty);
    }

    public T Peek() => _queue.Peek();

    public int RemoveWhere(Func<T, bool> match) => _queue.RemoveWhere(match);

    public T[] ToArray() => _queue.ToArray();

    public override string? ToString() => _queue.ToString();

    public bool TryDequeue(out T result)
    {
        var ret = _queue.TryDequeue(out result);
        if (ret)
        {
            OnQueueChanged();
        }
        return ret;
    }

    public bool TryPeek(out T result) => _queue.TryPeek(out result);
}
