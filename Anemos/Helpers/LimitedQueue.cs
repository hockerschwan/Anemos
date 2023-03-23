// https://stackoverflow.com/questions/1292/

namespace Anemos;

public class LimitedQueue<T> : Queue<T>
{
    private int _limit;
    public int Limit
    {
        get => _limit;
        set
        {
            _limit = value;
            TrimToLimit();
        }
    }

    public LimitedQueue(int limit) : base(limit)
    {
        Limit = limit;
    }

    public new void Enqueue(T item)
    {
        base.Enqueue(item);
        TrimToLimit();
    }

    private void TrimToLimit()
    {
        while (Count > Limit)
        {
            Dequeue();
        }
    }
}
