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
﻿using System.Diagnostics.CodeAnalysis;

namespace Foundation.Collections.Generic;

public static class RingEnumerable
{
    public static RingEnumerable<T> Create<T>(IEnumerable<T> enumerable, bool infinite = false, int index = 0)
    {
        return new RingEnumerable<T>(new RingEnumerator<T>(enumerable, infinite, index));
    }

    public static RingEnumerable<T> Create<T>(RingEnumerator<T> enumerator)
    {
        return new RingEnumerable<T>(enumerator);
    }
}

public class RingEnumerable<T> : IEnumerable<T?>
{
    private readonly RingEnumerator<T> _enumerator;

    public RingEnumerable(RingEnumerator<T> enumerator)
    {
        _enumerator = enumerator.ThrowIfNull();
    }

    public IEnumerator<T?> GetEnumerator() => _enumerator;

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

