using System.Collections;
using System.Diagnostics;

namespace Anemos;

public delegate void QueueChangedEventHandler(object? sender, EventArgs e);

[Serializable]
[DebuggerDisplay("Capacity = {Capacity}")]
public class LimitedQueue : IReadOnlyList<double>
{
    public struct Enumerator : IEnumerator<double>, IEnumerator, IDisposable
    {
        private readonly LimitedQueue _q;

        private int _index;

        private double _currentElement;

        public double Current
        {
            get
            {
                if (_index < 0)
                {
                    throw new InvalidOperationException();
                }

                return _currentElement;
            }
        }

        object IEnumerator.Current => Current;

        internal Enumerator(LimitedQueue q)
        {
            _q = q;
            _index = -1;
            _currentElement = default;
        }

        public void Dispose()
        {
            _index = -2;
            _currentElement = default;
        }

        public bool MoveNext()
        {
            if (_index == -2)
            {
                return false;
            }

            _index++;
            if (_index == _q._capacity)
            {
                _index = -2;
                _currentElement = default;
                return false;
            }

            _currentElement = _q._items[_index];
            return true;
        }

        void IEnumerator.Reset()
        {
            _index = -1;
            _currentElement = default;
        }
    }

    [field: NonSerialized]
    public event QueueChangedEventHandler? QueueChanged;

    private double[] _items;

    private int _capacity;
    public int Capacity
    {
        get => _capacity;
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Capacity), "Capacity must be greater than 0.");
            }

            if (value == _capacity) { return; }

            var newItems = new double[value];
            var newSpan = newItems.AsSpan();
            var oldSpan = _items.AsSpan();
            if (value > _capacity)
            {
                oldSpan.CopyTo(newSpan[(value - _capacity)..]);
            }
            else
            {
                oldSpan[(_capacity - value)..].CopyTo(newSpan);
            }
            _items = newItems;
            _capacity = value;

            OnQueueChanged();
        }
    }

    public LimitedQueue(int capacity = 1, double fillValue = 0.0)
    {
        _capacity = capacity;

        _items = new double[capacity];
        var span = _items.AsSpan();
        span.Fill(fillValue);
    }

    public double Average()
    {
        var sum = 0.0;
        for (var i = 0; i < _items.Length; ++i)
        {
            sum += _items[i];
        }
        return sum / _items.Length;
    }

    public void Clear()
    {
        var span = _items.AsSpan();
        span.Clear();

        OnQueueChanged();
    }

    public double Dequeue()
    {
        var ret = _items[0];

        var span = _items.AsSpan();
        if (!span.IsEmpty)
        {
            Shift(ref span);
            span[^1] = 0.0;
        }

        OnQueueChanged();
        return ret;
    }

    public void Enqueue(double value)
    {
        var span = _items.AsSpan();
        Shift(ref span);
        span[^1] = value;

        OnQueueChanged();
    }

    public double Max()
    {
        var max = double.NegativeInfinity;
        for (var i = 0; i < _items.Length; ++i)
        {
            if (_items[i] > max) { max = _items[i]; }
        }
        return max;
    }

    public double Min()
    {
        var min = double.PositiveInfinity;
        for (var i = 0; i < _items.Length; ++i)
        {
            if (_items[i] < min) { min = _items[i]; }
        }
        return min;
    }

    private void OnQueueChanged()
    {
        QueueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Shift(ref Span<double> span)
    {
        for (var i = 0; i < _capacity - 1; ++i)
        {
            span[i] = span[i + 1];
        }
        span[^1] = 0d;
    }

    public double this[int index] => _items[index];
    public int Count => Capacity;
    IEnumerator<double> IEnumerable<double>.GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
