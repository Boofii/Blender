using System;
using System.Collections.Generic;

namespace Blender.Utility;

public class ListeningDictionary<TKey, TValue>(Action<TKey, TValue> addedEvent) : Dictionary<TKey, TValue>
{
    private readonly Action<TKey, TValue> addedEvent = addedEvent;

    public new void Add(TKey key, TValue value)
    {
        base.Add(key, value);
        this.addedEvent?.Invoke(key, value);
    }
}