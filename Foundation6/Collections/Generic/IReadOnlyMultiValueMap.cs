// The MIT License (MIT)
//
// Copyright (c) 2020 Markus Raufer
//
// All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.Diagnostics.CodeAnalysis;

namespace Foundation.Collections.Generic;

public interface IReadOnlyMultiValueMap<TKey, TValue> : IReadOnlyMultiValueMap<TKey, TValue, ICollection<TValue>>
{
}

public interface IReadOnlyMultiValueMap<TKey, TValue, TValueCollection> 
    : IEnumerable<KeyValuePair<TKey, TValue>>
    where TValueCollection : IEnumerable<TValue>
{
    /// <summary>
    /// Checks if key value pair exists.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool Contains(TKey key, TValue value);

    /// <summary>
    /// Checks if value exists for a key.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    bool ContainsValue(TValue value);

    /// <summary>
    /// Returns key values as flat list of the specified keys. If keys is empty it returns all key values.
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    IEnumerable<KeyValuePair<TKey, TValue>> GetFlattenedKeyValues(IEnumerable<TKey>? keys = default);

    /// <summary>
    /// Returns values as flat list of the specified keys.
    /// </summary>
    /// <returns></returns>
    IEnumerable<TValue> GetFlattenedValues(IEnumerable<TKey>? keys = default);

    /// <summary>
    /// Returns the keys containing the value. If values is empty all keys are returned.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    IEnumerable<TKey> GetKeys(IEnumerable<TValue> values);

    /// <summary>
    /// Gets all keys with their values.
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> GetKeyValues(IEnumerable<TKey>? keys = default);

    /// <summary>
    /// Returns the values of a specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IEnumerable<TValue> GetValues(TKey key);

    /// <summary>
    /// Returns the values of the specified keys.
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    IEnumerable<TValue> GetValues(IEnumerable<TKey> keys);

    /// <summary>
    /// Returns the number of values of the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    int GetValuesCount(TKey key);

    /// <summary>
    /// Returns the number of keys.
    /// </summary>
    public int KeyCount { get; }

    bool TryGetValues(TKey key, [NotNullWhen(true)] out TValueCollection? values);
}
