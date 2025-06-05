// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Text.Json;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using Kumara.Utilities;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Kumara.Types;

[JsonConverter(typeof(DateWithOptionalTimeConverter))]
[SwaggerSchemaFilter(typeof(DateWithOptionalTimeSchemaPatcher))]
[OpenApiSchemaTransformer(typeof(DateWithOptionalTimeSchemaPatcher))]
public readonly struct DateWithOptionalTime
{
    public required DateTimeOffset DateTime { get; init; }
    public required bool HasTime { get; init; }

    public override string ToString()
    {
        return JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(this))!;
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
        if (DateOnly.TryParseExact(reader.GetString(), "o", out DateOnly date))
            return new DateWithOptionalTime
            {
                DateTime = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                HasTime = false,
            };

        return new DateWithOptionalTime { DateTime = reader.GetDateTimeOffset(), HasTime = true };
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateWithOptionalTime value,
        JsonSerializerOptions options
    )
    {
        if (value.HasTime)
            writer.WriteStringValue(value.DateTime.ToString("O"));
        else
            writer.WriteStringValue(DateOnly.FromDateTime(value.DateTime.Date).ToString("O"));
    }
}

public class DateWithOptionalTimeSchemaPatcher : SchemaPatcher
{
    protected override void Patch(OpenApiSchema schema)
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
