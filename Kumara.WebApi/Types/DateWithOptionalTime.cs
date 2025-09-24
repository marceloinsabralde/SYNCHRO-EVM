// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kumara.Common.Utilities;
using Microsoft.OpenApi.Models;
using NodaTime;
using NodaTime.Text;
using Swashbuckle.AspNetCore.Annotations;

namespace Kumara.WebApi.Types;

[JsonConverter(typeof(DateWithOptionalTimeConverter))]
[SwaggerSchemaFilter(typeof(DateWithOptionalTimeSchemaPatcher))]
[OpenApiSchemaTransformer(typeof(DateWithOptionalTimeSchemaPatcher))]
public readonly struct DateWithOptionalTime : IParsable<DateWithOptionalTime>
{
    public required OffsetDateTime DateTime { get; init; }
    public required bool HasTime { get; init; }

    public override string ToString()
    {
        return JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(this))!;
    }

    public static DateWithOptionalTime Parse(string str) => Parse(str, CultureInfo.CurrentCulture);

    public static DateWithOptionalTime Parse(string s, IFormatProvider? provider)
    {
        // Try date-only first (YYYY-MM-DD)
        var dateResult = LocalDatePattern.Iso.Parse(s);
        if (dateResult.Success)
        {
            return new DateWithOptionalTime
            {
                DateTime = dateResult.Value.AtStartOfDayInZone(DateTimeZone.Utc).ToOffsetDateTime(),
                HasTime = false,
            };
        }

        // Try date-time with offset
        var parseResult = OffsetDateTimePattern.ExtendedIso.Parse(s);
        return new DateWithOptionalTime
        {
            DateTime = parseResult.GetValueOrThrow(),
            HasTime = true,
        };
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out DateWithOptionalTime result
    )
    {
        result = default;

        try
        {
            if (string.IsNullOrEmpty(s))
                return false;

            result = Parse(s);

            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class DateWithOptionalTimeConverter : JsonConverter<DateWithOptionalTime>
{
    public override DateWithOptionalTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var stringValue = reader.GetString();
        if (string.IsNullOrEmpty(stringValue))
            throw new JsonException("Date string cannot be null or empty");

        return DateWithOptionalTime.Parse(stringValue);
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateWithOptionalTime value,
        JsonSerializerOptions options
    )
    {
        if (value.HasTime)
        {
            writer.WriteStringValue(OffsetDateTimePattern.ExtendedIso.Format(value.DateTime));
        }
        else
        {
            writer.WriteStringValue(LocalDatePattern.Iso.Format(value.DateTime.Date));
        }
    }
}

public class DateWithOptionalTimeSchemaPatcher : SchemaPatcher
{
    protected override void Patch(OpenApiSchema schema, Type type)
    {
        var nullable = schema.Nullable;
        Clear(schema);
        schema.Nullable = nullable;

        schema.AnyOf = new List<OpenApiSchema>
        {
            new OpenApiSchema { Type = "string", Format = "date" },
            new OpenApiSchema { Type = "string", Format = "date-time" },
        };

        // hack to get Swagger UI to generate a date-time example
        // if this causes problems we can set Example instead
        schema.Format = "date-time";
    }
}
