﻿using System.Reflection;
using System.Text.Json;

namespace Foundation.Text.Json;

public static class Utf8JsonReaderExtensions
{
    public static Result<KeyValuePair<string, object?>, Error> GetProperty(this ref Utf8JsonReader reader, Type type)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            var error = new Error($"{nameof(JsonTokenType)}", $"expected {nameof(JsonTokenType.PropertyName)}");
            return Result.Error<KeyValuePair<string, object?>>(error);
        }

        var name = reader.GetString();
        if (null == name)
        {
            return Result.Error<KeyValuePair<string, object?>>(new Error("property name", "property has no name"));
        }
        
        if (!reader.Read() || !reader.TokenType.IsValue())
        {
            return Result.Error<KeyValuePair<string, object?>>(new Error("property value", $"property {name} has no value"));
        }

        var value = reader.GetValue(type);
        return Result.Ok(new KeyValuePair<string, object?>(name, value));
    }

    public static object? GetValue(this Utf8JsonReader reader, Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean: return reader.GetBoolean();
            case TypeCode.Byte: return reader.GetByte();
            case TypeCode.Char: return reader.GetString();
            case TypeCode.DateTime: return reader.GetDateTime();
            case TypeCode.Decimal: return reader.GetDecimal();
            case TypeCode.Double: return reader.GetDouble();
            case TypeCode.Int16: return reader.GetInt16();
            case TypeCode.Int32: return reader.GetInt32();
            case TypeCode.Int64: return reader.GetInt64();
            case TypeCode.UInt16: return reader.GetUInt16();
            case TypeCode.UInt32: return reader.GetUInt32();
            case TypeCode.UInt64: return reader.GetUInt64();
            case TypeCode.SByte: return reader.GetSByte();
            case TypeCode.Single: return reader.GetSingle();
            case TypeCode.String: return reader.GetString();
        }

        switch(type)
        {
            case Type _ when type == typeof(DateOnly):
                {
                    var str = reader.GetString();
                    if(str is null) return null;

                    if (DateTime.TryParse(str, out var dt)) return dt.ToDateOnly();
                    return null;
                }
            case Type _ when type == typeof(Guid):
                {
                    var guid = reader.GetGuid();
                    return guid;
                }
            case Type _ when type == typeof(TimeOnly):
                {
                    var str = reader.GetString();
                    if (str is null) return null;

                    if (DateTime.TryParse(str, out var dt)) return dt.ToTimeOnly();
                    return null;
                }
        };

        return reader.GetString();
    }

    public static object? GetValue(this Utf8JsonReader reader, string typeName)
    {
        switch (typeName)
        {
            case "System.Boolean": return reader.GetBoolean();
            case "System.Byte": return reader.GetByte();
            case "System.Char": return reader.GetString();
            case "System.DateOnly":
                {
                    var str = reader.GetString();
                    if (str is null) return null;

                    if (DateTime.TryParse(str, out var dt)) return dt.ToDateOnly();
                    return null;
                }
            case "System.DateTime": return reader.GetDateTime();
            case "System.Decimal": return reader.GetDecimal();
            case "System.Double": return reader.GetDouble();
            case "System.Guid": return reader.GetGuid();
            case "System.Int16": return reader.GetInt16();
            case "System.Int32": return reader.GetInt32();
            case "System.Int64": return reader.GetInt64();
            case "System.UInt16": return reader.GetUInt16();
            case "System.UInt32": return reader.GetUInt32();
            case "System.UInt64": return reader.GetUInt64();
            case "System.SByte": return reader.GetSByte();
            case "System.Single": return reader.GetSingle();
            case "System.String": return reader.GetString();
            case "System.TimeOnly":
                {
                    var str = reader.GetString();
                    if (str is null) return null;

                    if (DateTime.TryParse(str, out var dt)) return dt.ToTimeOnly();
                    return null;
                }
        }

        return reader.GetString();
    }
}
