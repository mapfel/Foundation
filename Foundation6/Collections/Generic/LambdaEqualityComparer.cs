﻿using System.Diagnostics.CodeAnalysis;

namespace Foundation.Collections.Generic;

public class LambdaEqualityComparer<T> : EqualityComparer<T>
{
    private readonly Func<T?, int> _hashCodeFunc;
    private readonly Func<T?, T?, bool> _predicate;

    public override bool Equals(T? x, T? y)
    {
        return _predicate(x, y);
    }

    public override int GetHashCode([DisallowNull] T obj)
    {
        return _hashCodeFunc(obj);
    }

    /// <summary>
    /// Default hashCodeFunc is DefaultHashCodeFunc.
    /// </summary>
    /// <param name="predicate"></param>
    public LambdaEqualityComparer(Func<T?, T?, bool> predicate) : this(predicate, DefaultHashCodeFunc)
    {
    }

    /// <summary>
    /// Default predicate uses Equals method.
    /// </summary>
    /// <param name="hashCodeFunc"></param>
    public LambdaEqualityComparer(Func<T?, int>? hashCodeFunc) : this(DefaultPredicate, hashCodeFunc)
    {
    }

    public LambdaEqualityComparer(Func<T?, T?, bool> predicate, Func<T?, int>? hashCodeFunc)
    {
        _predicate = predicate.ThrowIfNull();
        _hashCodeFunc = hashCodeFunc.ThrowIfNull();
    }

    public static Func<T?, int> DefaultHashCodeFunc { get; } = (t => null == t ? 0 : t.GetHashCode());

    public static Func<T?, T?, bool> DefaultPredicate { get; } =
        (x, y) =>
        {
            if (null == x) return null == y;
            if (null == y) return false;

            return x.Equals(y);
        };
}


