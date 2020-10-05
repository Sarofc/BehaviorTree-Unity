using System;
using System.Collections.Generic;

public class UpdateList<T>
{
    private readonly List<T> data = new List<T>();
    private readonly List<T> addQueue = new List<T>();
    private readonly List<T> removeQueue = new List<T>();

    private readonly Predicate<T> isInRemovealQueue;

    public IReadOnlyList<T> Data => data;

    public UpdateList()
    {
        isInRemovealQueue = delegate (T val)
        {
            return removeQueue.Contains(val);
        };
    }

    public void Add(T item)
    {
        addQueue.Add(item);
    }

    public void Remove(T item)
    {
        removeQueue.Add(item);
    }

    public void AddAndRemoveQueued()
    {
        if(removeQueue.Count != 0)
        {
            data.RemoveAll(isInRemovealQueue);
            removeQueue.Clear();
        }

        if(addQueue.Count != 0)
        {
            data.AddRange(addQueue);
            addQueue.Clear();
        }
    }
}

