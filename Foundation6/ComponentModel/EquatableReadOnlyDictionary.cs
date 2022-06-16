﻿namespace Foundation.ComponentModel;

using Foundation;
using Foundation.Collections.Generic;

using System.Collections;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// This dictionary considers the equality of all keys and values <see cref="Equals"/>.
/// The position of the elements are ignored.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class EquatableReadOnlyDictionary<TKey, TValue>
    : IReadOnlyDictionary<TKey, TValue>
    , IEquatable<EquatableReadOnlyDictionary<TKey, TValue>>
    where TKey : notnull
{
    private readonly IDictionary<TKey, TValue> _dictionary;
    private readonly int _hashCode;

    public EquatableReadOnlyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> keyValues)
        : this(keyValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
    {
    }

    public EquatableReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
    {
        _dictionary = dictionary.ThrowIfNull();
        _hashCode = HashCode.FromObjects(_dictionary);
    }

    public TValue this[TKey key] => _dictionary[key];

    public int Count => _dictionary.Count;

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    protected static int DefaultHashCode { get; } = typeof(EquatableReadOnlyDictionary<TKey, TValue>).GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as EquatableReadOnlyDictionary<TKey, TValue>);

    public bool Equals(EquatableReadOnlyDictionary<TKey, TValue>? other)
    {
        if (other is null) return false;
        if (_hashCode != other._hashCode) return false;

        return _dictionary.IsEqualTo(other._dictionary);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

    public override int GetHashCode() => _hashCode;

    public IEnumerable<TKey> Keys => _dictionary.Keys;

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public IEnumerable<TValue> Values => _dictionary.Values;
}
