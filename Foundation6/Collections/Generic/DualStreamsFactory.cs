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
﻿namespace Foundation.Collections.Generic;

public static class DualStreamsFactory
{
    /// <summary>
    /// Splits a stream into two streams and returns it as DualStreams. Matching items are added to the right stream.
    /// If isExhaustive is false, all items are added to the left stream.
    /// </summary>
    /// <typeparam name="TLeft"></typeparam>
    /// <typeparam name="TRight"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate">If predicate it true the item is added to the right stream.</param>
    /// <param name="project"></param>
    /// <param name="isExhaustive"></param>
    /// <returns></returns>
    public static DualStreams<TLeft, TRight> ToDualStreams<TLeft, TRight>(
        this IEnumerable<TLeft> items,
        Func<TLeft, bool> predicate,
        Func<TLeft, TRight> project,
        bool isExhaustive)
    {
        items.ThrowIfEnumerableIsNull();
        predicate.ThrowIfNull();
        project.ThrowIfNull();

        var streams = new DualStreams<TLeft, TRight>();

        foreach (var item in items)
        {
            if (predicate(item))
            {
                var right = project(item);

                streams.Right = streams.Right.Append(right);

                if (isExhaustive) continue;
            }

            streams.Left = streams.Left.Append(item);
        }

        return streams;
    }
}
