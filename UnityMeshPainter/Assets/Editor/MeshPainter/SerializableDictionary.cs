using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable()]
public class SerializableKeyValuePair<K, V>
{
    [SerializeField]
    private K _key;
    private V _value;

    public K Key
    {
        get
        {
            return (this._key);
        }
        set
        {
            this._key = value;
        }
    }

    public V Value
    {
        get
        {
            return (this._value);
        }
        set
        {
            this._value = value;
        }
    }

    public SerializableKeyValuePair()
    {
        this._key = default(K);
        this._value = default(V);
    }

    public SerializableKeyValuePair(K key, V value)
    {
        this._key = key;
        this._value = value;
    }

    public SerializableKeyValuePair(KeyValuePair<K, V> kvp)
    {
        this._key = kvp.Key;
        this._value = kvp.Value;
    }
}

[Serializable()]
public class SerializableDictionary<TKey, TValue>
{
    [SerializeField]
    private List<SerializableKeyValuePair<TKey, TValue>> _elements;

    public SerializableDictionary()
    {
        this._elements = new List<SerializableKeyValuePair<TKey, TValue>>();
    }

    public TValue this[TKey key]
    {
        get
        {
            for (int i = 0, size = this._elements.Count; i < size; ++i)
            {
                if (this._elements[i].Key.Equals(key) == true)
                {
                    return (this._elements[i].Value);
                }
            }
            Debug.LogWarning("Key " + key.ToString() + " not found in dictionnary.");
            return (default(TValue));
        }
        set
        {
            for (int i = 0, size = this._elements.Count; i < size; ++i)
            {
                if (this._elements[i].Key.Equals(key) == true)
                {
                    //this._elements[i] = new KeyValuePair<TKey, TValue>(key, value);
                    this._elements[i].Value = value;
                    return;
                }
            }
            this.Add(new SerializableKeyValuePair<TKey, TValue>(key, value));
        }
    }

    public void Add(KeyValuePair<TKey, TValue> kvp)
    {
        this._elements.Add(new SerializableKeyValuePair<TKey, TValue>(kvp));
    }

    public void Add(SerializableKeyValuePair<TKey, TValue> kvp)
    {
        this._elements.Add(kvp);
    }

    public void Remove(TKey key)
    {
        for (int i = 0, size = this._elements.Count; i < size; ++i)
        {
            if (this._elements[i].Equals(key) == true)
            {
                this._elements.RemoveAt(i);
                return;
            }
        }
    }
}
