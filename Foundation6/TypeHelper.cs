﻿namespace Foundation;

public static class TypeHelper
{
    /// <summary>
    /// Returns the type of a nullable primitive type from type name.
    /// </summary>
    /// <param name="shortTypeName"></param>
    /// <returns></returns>
    public static Type? GetNullablePrimitveType(string shortTypeName)
    {
        var fullName = GetNullablePrimitiveTypeFullName(shortTypeName);
        if (null == fullName) return null;

        return Type.GetType(fullName);
    }

    /// <summary>
    /// Returns the full name of the nullable primitive type from the short name.
    /// </summary>
    /// <param name="shortTypeName"></param>
    /// <returns></returns>
    public static string? GetNullablePrimitiveTypeFullName(string shortTypeName) => shortTypeName switch
    {
        "bool?" => typeof(Boolean?).FullName,
        "byte?" => typeof(Byte?).FullName,
        "char?" => typeof(Char?).FullName,
        "double?" => typeof(Double?).FullName,
        "float?" => typeof(Single?).FullName,
        "int?" => typeof(Int32?).FullName,
        "long?" => typeof(Int64?).FullName,
        "sbyte?" => typeof(SByte?).FullName,
        "short?" => typeof(Int16?).FullName,
        "uint?" => typeof(UInt32?).FullName,
        "ulong?" => typeof(UInt64?).FullName,
        "ushort?" => typeof(UInt16?).FullName,
        _ => null,
    };
    /// <summary>
    /// returns the type from the short name. e.g. int?, string?.
    /// </summary>
    /// <param name="shortTypeName">The name of the type.</param>
    /// <returns></returns>
    public static Type? GetNullableScalarType(string shortTypeName, bool withNullablePrimitives = true)
    {
        var fullName = GetNullableScalarTypeFullName(shortTypeName, withNullablePrimitives);
        if (null == fullName) return null;

        return Type.GetType(fullName);
    }

    /// <summary>
    /// returns the fullname of the type.
    /// </summary>
    /// <param name="shortTypeName">e.g. DateTime?, decimal?, string.</param>
    /// <returns></returns>
    public static string? GetNullableScalarTypeFullName(string shortTypeName, bool withNullablePrimitives = true)
    {
        if (withNullablePrimitives)
        {
            var fullName = GetNullablePrimitiveTypeFullName(shortTypeName);
            if (null != fullName) return fullName;
        }

        return shortTypeName switch
        {
            $"{nameof(DateOnly)}?" => typeof(DateOnly?).FullName,
            $"{nameof(DateTime)}?" => typeof(DateTime?).FullName,
            "decimal?" => typeof(Decimal?).FullName,
            $"{nameof(Guid)}?" => typeof(Guid?).FullName,
            "string" => typeof(String).FullName,
            $"{nameof(TimeOnly)}?" => typeof(TimeOnly?).FullName,
            _ => null,
        };
    }

    /// <summary>
    /// returns the type from the short name. e.g. bool, int.
    /// </summary>
    /// <param name="shortTypeName"></param>
    /// <returns></returns>
    public static Type? GetPrimitveType(string shortTypeName)
    {
        var fullName = GetPrimitiveTypeFullName(shortTypeName);
        if (null == fullName) return null;

        return Type.GetType(fullName);
    }

    /// <summary>
    /// returns the fullname of the type. e.g. int => System.Int32.
    /// </summary>
    /// <param name="shortTypeName">e.g. bool, byte int, long, ...</param>
    /// <returns></returns>
    public static string? GetPrimitiveTypeFullName(string shortTypeName) => shortTypeName switch
    {
        "bool" => typeof(Boolean).FullName,
        "byte" => typeof(Byte).FullName,
        "char" => typeof(Char).FullName,
        "double" => typeof(Double).FullName,
        "float" => typeof(Single).FullName,
        "int" => typeof(Int32).FullName,
        "long" => typeof(Int64).FullName,
        "sbyte" => typeof(SByte).FullName,
        "short" => typeof(Int16).FullName,
        "uint" => typeof(UInt32).FullName,
        "ulong" => typeof (UInt64).FullName,
        "ushort" => typeof(UInt16).FullName,
        _ => null,
    };

    /// <summary>
    /// returns the type from the short name. e.g. int, string.
    /// </summary>
    /// <param name="shortTypeName">The name of the type.</param>
    /// <returns></returns>
    public static Type? GetScalarType(string shortTypeName)
    {
        return GetScalarType(shortTypeName, true);
    }

    /// <summary>
    /// returns a type if it matches with a scalar or primitive type.
    /// </summary>
    /// <param name="type">Type to check if it is scalar or primitive.</param>
    /// <param name="whithoutPrimitives">If true primitive types are not considered.</param>
    /// <returns></returns>
    public static Type? GetScalarType(Type type, bool whithoutPrimitives = false)
    {
        return ScalarTypes(whithoutPrimitives).FirstOrDefault(x => x == type);
    }

    /// <summary>
    /// returns the type from the short name. e.g. int, string.
    /// </summary>
    /// <param name="shortTypeName">The name of the type.</param>
    /// <param name="whithPrimitives">Including primitive types.</param>
    /// <returns></returns>
    public static Type? GetScalarType(string shortTypeName, bool whithPrimitives)
    {
        var fullName = GetScalarTypeFullName(shortTypeName, whithPrimitives);
        if (null == fullName) return null;

        return Type.GetType(fullName);
    }

    /// <summary>
    /// returns the fullname of the type. e.g. int => System.Int32.
    /// </summary>
    /// <param name="shortTypeName">e.g. DateTime, decimal, string.</param>
    /// <returns></returns>
    public static string? GetScalarTypeFullName(string shortTypeName, bool whithPrimitives = true)
    {
        if(whithPrimitives)
        {
            var fullName = GetPrimitiveTypeFullName(shortTypeName);
            if (null != fullName) return fullName;
        }

        return shortTypeName switch
        {
            nameof(DateOnly) => typeof(DateOnly).FullName,
            nameof(DateTime) => typeof(DateTime).FullName,
            "decimal"  => typeof(Decimal).FullName,
            nameof(Guid) => typeof(Guid).FullName,
            "string" => typeof(String).FullName,
            nameof(TimeOnly) => typeof(TimeOnly).FullName,
            _ => null,
        };
    }

    public static IEnumerable<Type> NullablePrimitiveTypes()
    {
        yield return typeof(Boolean?);
        yield return typeof(Byte?);
        yield return typeof(Char?);
        yield return typeof(Double?);
        yield return typeof(Int16?);
        yield return typeof(Int32?);
        yield return typeof(Int64?);
        yield return typeof(SByte?);
        yield return typeof(Single?);
        yield return typeof(UInt16?);
        yield return typeof(UInt32?);
        yield return typeof(UInt64?);
    }

    public static IEnumerable<string> NullablePrimitiveTypeShortNames()
    {
        yield return "bool?";
        yield return "byte?";
        yield return "char?";
        yield return "double?";
        yield return "float?";
        yield return "int?";
        yield return "long?";
        yield return "sbyte?";
        yield return "short?";
        yield return "uint?";
        yield return "ulong?";
        yield return "ushort?";
    }

    public static IEnumerable<Type> NullableScalarTypes(bool whithoutPrimitives = false)
    {
        if (!whithoutPrimitives)
        {
            foreach (var primitive in NullablePrimitiveTypes())
            {
                yield return primitive;
            }
        }

        yield return typeof(DateOnly?);
        yield return typeof(DateTime?);
        yield return typeof(decimal?);
        yield return typeof(Guid?);
        yield return typeof(string);
        yield return typeof(TimeOnly?);
        yield return typeof(TimeSpan?);
    }

    public static IEnumerable<Type> PrimitiveArrayTypes()
    {
        yield return typeof(Boolean[]);
        yield return typeof(Byte[]);
        yield return typeof(Char[]);
        yield return typeof(Double[]);
        yield return typeof(Int16[]);
        yield return typeof(Int32[]);
        yield return typeof(Int64[]);
        yield return typeof(SByte[]);
        yield return typeof(Single[]);
        yield return typeof(UInt16[]);
        yield return typeof(UInt32[]);
        yield return typeof(UInt64[]);
    }

    public static IEnumerable<Type> PrimitiveEnumerableTypes()
    {
        yield return typeof(IEnumerable<Boolean>);
        yield return typeof(IEnumerable<Byte>);
        yield return typeof(IEnumerable<Char>);
        yield return typeof(IEnumerable<Double>);
        yield return typeof(IEnumerable<Int16>);
        yield return typeof(IEnumerable<Int32>);
        yield return typeof(IEnumerable<Int64>);
        yield return typeof(IEnumerable<SByte>);
        yield return typeof(IEnumerable<Single>);
        yield return typeof(IEnumerable<UInt16>);
        yield return typeof(IEnumerable<UInt32>);
        yield return typeof(IEnumerable<UInt64>);
    }

    public static IEnumerable<Type> PrimitiveTypes()
    {
        yield return typeof(Boolean);
        yield return typeof(Byte);
        yield return typeof(Char);
        yield return typeof(Double);
        yield return typeof(Int16);
        yield return typeof(Int32);
        yield return typeof(Int64);
        yield return typeof(SByte);
        yield return typeof(Single);
        yield return typeof(UInt16);
        yield return typeof(UInt32);
        yield return typeof(UInt64);
    }

    public static IEnumerable<string> PrimitiveTypeShortNames()
    {
        yield return "bool";
        yield return "byte";
        yield return "char";
        yield return "double";
        yield return "float";
        yield return "int";
        yield return "long";
        yield return "sbyte";
        yield return "short";
        yield return "uint";
        yield return "ulong";
        yield return "ushort";
    }

    /// <summary>
    /// Real number types.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Type> RealNumberTypes()
    {
        yield return typeof(Decimal);
        yield return typeof(Double);
        yield return typeof(Single);
    }

    public static IEnumerable<Type> ScalarArrayTypes(bool whithoutPrimitives = false)
    {
        if(!whithoutPrimitives)
        {
            foreach (var primitive in PrimitiveArrayTypes())
            {
                yield return primitive;
            }
        }
        yield return typeof(DateOnly[]);
        yield return typeof(DateTime[]);
        yield return typeof(decimal[]);
        yield return typeof(Guid[]);
        yield return typeof(string[]);
        yield return typeof(TimeOnly[]);
    }

    public static IEnumerable<Type> ScalarEnumerableTypes(bool whithoutPrimitives = false)
    {
        if(!whithoutPrimitives)
        {
            foreach (var primitive in PrimitiveEnumerableTypes())
            {
                yield return primitive;
            }
        }
        yield return typeof(IEnumerable<DateOnly>);
        yield return typeof(IEnumerable<DateTime>);
        yield return typeof(IEnumerable<decimal>);
        yield return typeof(IEnumerable<Guid>);
        yield return typeof(IEnumerable<string>);
        yield return typeof(IEnumerable<TimeOnly>);
        yield return typeof(IEnumerable<TimeSpan>);
    }

    public static IEnumerable<Type> ScalarTypes(bool whithoutPrimitives = false)
    {
        if(!whithoutPrimitives)
        {
            foreach (var primitive in PrimitiveTypes())
            {
                yield return primitive;
            }
        }

        yield return typeof(DateOnly);
        yield return typeof(DateTime);
        yield return typeof(decimal);
        yield return typeof(Guid);
        yield return typeof(string);
        yield return typeof(TimeOnly);
        yield return typeof(TimeSpan);
    }

    public static IEnumerable<string> ScalarTypeShortNames(bool whithoutPrimitives = false)
    {
        if(!whithoutPrimitives)
        {
            foreach (var typeName in PrimitiveTypeShortNames())
                yield return typeName;
        }

        yield return nameof(DateOnly);
        yield return nameof(DateTime);
        yield return "decimal";
        yield return nameof(Guid);
        yield return "string";
        yield return nameof(TimeOnly);
        yield return nameof(TimeSpan);
    }
}

