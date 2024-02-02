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
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foundation.Text.Json.Serialization;

public class TypeJsonConverter : JsonConverter<Type>
{
    private static readonly string FullName = nameof(Type.FullName);
    private static readonly string Name = nameof(Type.Name);

    private readonly Assembly[] _assemblies;
    private readonly string[] _propertyNames;

    public TypeJsonConverter() : this(GetAssemblies(Assembly.GetExecutingAssembly().Location), new[] {nameof(Type.FullName)})
    {
    }

    public TypeJsonConverter(IEnumerable<Assembly> assemblies, IEnumerable<string> propertyNames)
    {
        _assemblies = assemblies.ThrowIfNull().ToArray();
        _propertyNames = propertyNames.ThrowIfNull().ToArray();
    }

    private static IEnumerable<Assembly> GetAssemblies(string location)
    {
        var dir = Path.GetDirectoryName(location);
        if (dir is null) yield break;

        var dlls = Directory.GetFiles(dir, "*.dll");
        foreach(var dll in dlls)
        {
            yield return Assembly.LoadFile(dll);
        }
    }

    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject) return null;

        var typeInstance = typeToConvert.GetType();
        if (null == typeInstance) return null;
        Type? type = null;

        string? propertyName = null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (null == type && reader.TokenType == JsonTokenType.PropertyName)
            {
                propertyName = reader.GetString();
                continue;
            }
            if (null == type && propertyName == FullName || propertyName == Name)
            {
                var typeName = reader.GetString();
                if (null == typeName) continue;

                type = getType(typeName);
            }
        }

        return type;

        Type? getType(string name)
        {
            var type = Type.GetType(name);
            if (type is null)
            {
                var span = name.AsSpan();
                var index = span.IndexOf('+');
                var typeName = span[(index + 1)..].ToString();

                type = _assemblies.SelectMany(x => x.GetTypes()).Where(x => x.Name == typeName).FirstOrDefault();
            }
            return type;
        }
    }

    public override void Write(Utf8JsonWriter writer, Type type, JsonSerializerOptions options)
    {
        var typeInstance = type.GetType();
        if (null == typeInstance) return;

        writer.WriteStartObject();

        foreach (var propertyName in _propertyNames)
        {
            var property = typeInstance.GetProperty(propertyName);
            if (null == property) continue;

            writer.WritePropertyName(propertyName);

            var value = property.GetValue(type, null);
            writer.WriteValue(value);
        }

        writer.WriteEndObject();
    }
}
