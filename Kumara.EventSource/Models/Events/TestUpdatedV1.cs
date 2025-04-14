// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kumara.EventSource.Models.Events;

public class TestUpdatedV1
{
    [Required(AllowEmptyStrings = false)]
    [JsonPropertyName("test_string")]
    public required string TestString { get; set; }

    [Required]
    [EnumDataType(typeof(TestOptions))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("test_enum")]
    public required TestOptions TestEnum { get; set; }

    [Range(0, 1000)]
    [JsonPropertyName("test_integer")]
    public int TestInteger { get; set; }

    [Required]
    [JsonPropertyName("updated_time")]
    public required DateTime UpdatedTime { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("event_type_version")]
    public string EventTypeVersion => "1.0";
}
