// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kumara.EventSource.Models.Events;

public enum TestOptions
{
    OptionA,
    OptionB,
    OptionC,
    OptionD,
    OptionE,
}

public class TestCreatedV1
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

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] // Don't serialize if default
    [JsonPropertyName("event_type_version")]
    public string EventTypeVersion => "1.0"; // Readonly property identifies version
}
