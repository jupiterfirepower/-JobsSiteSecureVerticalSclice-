using System.Collections;
using System.Collections.Concurrent;

namespace Jobs.YarpGateway.Helpers;

public class ConcurrentList<T> : ICollection<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, object> _store;

    public ConcurrentList(IEnumerable<T> items)
    {
        var prime = (items ?? []).Select(x => new KeyValuePair<T, object>(x, new object()));
        _store = new ConcurrentDictionary<T, object>(prime);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _store.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        if(!_store.TryAdd(item, new object()))
            throw new ApplicationException("Unable to concurrently add item to list");
    }

    public void Clear()
    {
        _store.Clear();
    }

    public bool Contains(T item)
    {
        return _store.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _store.Keys.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        _store.Remove(item, out _);
        return true;
    }

    public int Count => _store.Count;

    public bool IsReadOnly => _store.Keys.IsReadOnly;
}