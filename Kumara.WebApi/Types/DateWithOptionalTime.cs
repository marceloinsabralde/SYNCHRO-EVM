// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kumara.Types;

[JsonConverter(typeof(DateWithOptionalTimeConverter))]
public readonly struct DateWithOptionalTime
{
    public DateTimeOffset DateTime { get; init; }
    public bool HasTime { get; init; }
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
