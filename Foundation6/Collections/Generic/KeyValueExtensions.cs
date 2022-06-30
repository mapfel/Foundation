﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Foundation;

public static class KeyValueExtensions
{
    public static bool IsEmpty<TKey, TValue>(this KeyValue<TKey, TValue> keyValue, bool valueNotNull = false)
        where TKey : notnull
    {
        return null == keyValue.Key || (valueNotNull && null == keyValue.Value);
    }

    [return: NotNull]
    public static KeyValue<TKey, TValue> ThrowIfEmpty<TKey, TValue>(
        this KeyValue<TKey, TValue> keyValue,
        bool valueNotNull = false,
        [CallerArgumentExpression("keyValue")] string name = "")
        where TKey : notnull
    {
        if (keyValue.IsEmpty(valueNotNull)) throw new ArgumentNullException(name);

        return keyValue;
    }

    public static KeyValuePair<TKey, TValue> ToKeyValuePair<TKey, TValue>(this KeyValue<TKey, TValue> keyValue)
        where TKey : notnull  => new(keyValue.Key, keyValue.Value);
}