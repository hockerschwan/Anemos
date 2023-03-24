using System.Collections.ObjectModel;

namespace Anemos.Helpers;

public class LimitedObservableCollection<T> : RangeObservableCollection<T>
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

    public LimitedObservableCollection(int limit = 1)
    {
        Limit = limit;
    }

    public LimitedObservableCollection(IEnumerable<T> collection, int limit = 1) : base(collection)
    {
        Limit = limit;
    }

    public LimitedObservableCollection(List<T> list, int limit = 1) : base(list)
    {
        Limit = limit;
    }

    public new void Add(T item)
    {
        base.Add(item);
        TrimToLimit();
    }

    private void TrimToLimit()
    {
        if (Count > 0 && Count > Limit)
        {
            RemoveRange(0, Count - Limit);
        }
    }
}
