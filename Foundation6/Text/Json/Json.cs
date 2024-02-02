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
﻿using Foundation.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foundation.Text.Json;

public static class Json
{
    public static string Properties(IEnumerable<(string name, object? value)> properties)
    {
        var sb = new StringBuilder();
        foreach (var (name, value) in properties.AfterEach(() => sb.Append(',')))
        {
            sb.Append(Property(name, value));
        }
        return sb.ToString();
    }

    public static string Properties(IEnumerable<KeyValuePair<string, object?>> properties)
    {
        var sb = new StringBuilder();

        foreach (var (name, value) in properties.AfterEach(() => sb.Append(',')))
        {
            sb.Append(Property(name, value));
        }
        return sb.ToString();
    }

    public static string Property<T>(string name, T? value) => $"{name.ToJson()}:{ToJson(value)}";

    public static string ToJson(this string str) => $@"""{str}""";

    public static string ToJson<T>(this T? value)
    {
        if (value is null) return "null";

        var type = value.GetType();

        var scalarType = TypeHelper.GetScalarType(type);
        if (scalarType is null) return "null";

        return scalarType switch
        {
            { IsPrimitive: true } => $"{value}",
            Type _ when scalarType == typeof(DateTime) => $"{value:yyyy-MM-ddTHH:mm:ss}",
            Type _ when scalarType == typeof(DateOnly) => $"{value:yyyy-MM-dd}",
            Type _ when scalarType == typeof(decimal) => string.Create(CultureInfo.InvariantCulture, $"{value}"),
            Type _ when scalarType == typeof(Guid) => $"\"{value}\"",
            Type _ when scalarType == typeof(string) => $"\"{value}\"",
            Type _ when scalarType == typeof(TimeOnly) => $"\"{value:HH:mm:ss}\"",
            Type _ when scalarType == typeof(TimeSpan) => $"\"{value}\"",
            _ => "null"
        };
    }
}
