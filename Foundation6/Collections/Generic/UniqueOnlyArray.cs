﻿namespace Foundation.Collections.Generic;

using System.Collections;
using System.Diagnostics.CodeAnalysis;

public static class UniqueOnlyArray
{
    public static UniqueOnlyArray<T> New<T>(params T[] values)
    {
        return new UniqueOnlyArray<T>(values);
    }
}

public struct UniqueOnlyArray<T>
    : ICloneable
    , IEnumerable<T>
    , IEquatable<UniqueOnlyArray<T>>
    , IEquatable<T[]>
{
    private readonly int _hashCode;
    private string? _valuesAsString;
    private readonly HashSet<T>? _values;

    public UniqueOnlyArray(T[] values)
    {
        values.ThrowIfNull();
        
        _values = new HashSet<T>();
        foreach (var value in values)
        {
            if (!_values.Add(value)) throw new ArgumentNullException($"{value} exists");
        }

        _hashCode = HashCode.FromObjects(_values);
        _valuesAsString = "";
    }

    public static implicit operator UniqueOnlyArray<T>(T[] array) => UniqueOnlyArray.New(array);

    public static implicit operator T[](UniqueOnlyArray<T> array) 
        => null == array._values ? Array.Empty<T>() : array._values.ToArray();

    public static bool operator ==(UniqueOnlyArray<T> left, UniqueOnlyArray<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(UniqueOnlyArray<T> left, UniqueOnlyArray<T> right)
    {
        return !(left == right);
    }

    public T this[int index] => this.ElementAt(index);

    public object Clone()
    {
        return IsEmpty
            ? new UniqueOnlyArray<T>(Array.Empty<T>())
            : new UniqueOnlyArray<T>(this);
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is UniqueOnlyArray<T> other && Equals(other);

    public bool Equals(T[]? other)
    {
        if (IsEmpty) return null == other || 0 == other.Length;

        return null != other && _values!.SequenceEqual(other);
    }

    public bool Equals(UniqueOnlyArray<T> other)
    {
        if (IsEmpty) return other.IsEmpty;
        return !other.IsEmpty && _values!.SequenceEqual(other._values!);
    }

    public IEnumerator<T> GetEnumerator() => IsEmpty 
                                            ? Enumerable.Empty<T>().GetEnumerator()
                                            : _values!.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => IsEmpty
                                            ? Enumerable.Empty<T>().GetEnumerator()
                                            : _values!.GetEnumerator();

    public override int GetHashCode() => _hashCode;

    public bool IsEmpty => null == _values || 0 == _values.Count;

    public int Length => IsEmpty ? 0 : _values!.Count;

    public override string? ToString()
    {
        if (IsEmpty || 0 == _values!.Count) return "";
        
        if(null == _valuesAsString)
            _valuesAsString = string.Join(", ", _values);

        return _valuesAsString;
    }
}
