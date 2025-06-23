// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kumara.Common.Converters;

public class EnumToJsonStringValueConverter<T> : ValueConverter<T, string>
    where T : struct, Enum
{
    public EnumToJsonStringValueConverter()
        : base(v => ToString(v), v => ToEnum(v)) { }

    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, false) },
    };

    public static string ToString(T enumValue)
    {
        string jsonValue = JsonSerializer.Serialize(enumValue, _options);
        string stringValue = JsonSerializer.Deserialize<string>(jsonValue)!;

        return stringValue;
    }

    public static T ToEnum(string stringValue)
    {
        try
        {
            string jsonValue = JsonSerializer.Serialize(stringValue);

            T enumValue = JsonSerializer.Deserialize<T>(jsonValue, _options);

            if (stringValue != ToString(enumValue))
                throw new JsonException();

            return enumValue;
        }
        catch (JsonException)
        {
            throw new FormatException(
                $"Cannot convert {JsonSerializer.Serialize(stringValue)} to {typeof(T).Name}"
            );
        }
    }
}
