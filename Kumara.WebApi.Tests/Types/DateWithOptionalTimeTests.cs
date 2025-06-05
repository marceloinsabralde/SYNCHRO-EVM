// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Text.Json;
using Kumara.Types;

namespace Kumara.WebApi.Tests.Types;

public class DateWithOptionalTimeTests
{
    [Theory]
    [MemberData(nameof(TestData))]
    public void DeserializeTest(string input, DateWithOptionalTime expected)
    {
        DateWithOptionalTime actual = JsonSerializer.Deserialize<DateWithOptionalTime>(input);
        actual.HasTime.ShouldBe(expected.HasTime);
        actual.DateTime.ShouldBe(expected.DateTime);
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void SerializeTest(string expected, DateWithOptionalTime input)
    {
        string actual = JsonSerializer.Serialize(input);
        actual.ShouldBe(expected);
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void ToStringTest(string expectedJson, DateWithOptionalTime input)
    {
        string expected = JsonSerializer.Deserialize<string>(expectedJson)!;
        string actual = input.ToString();
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData("2025/05/05")]
    [InlineData("foo")]
    [InlineData("2025-05-05 11:55 am +10:00")]
    public void InvalidDeserializationInputTest(string input)
    {
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<DateWithOptionalTime>(input));
    }

    public static TheoryData<string, DateWithOptionalTime> TestData =>
        new TheoryData<string, DateWithOptionalTime>
        {
            {
                "\"2025-05-05\"",
                new DateWithOptionalTime
                {
                    DateTime = new DateTimeOffset(
                        year: 2025,
                        month: 05,
                        day: 05,
                        hour: 00,
                        minute: 00,
                        second: 00,
                        offset: TimeSpan.Zero
                    ),
                    HasTime = false,
                }
            },
            {
                // "2025-05-05T11:08:43.0000000+00:00" but UTF8
                "\"2025-05-05T11:08:43.0000000\\u002B00:00\"",
                new DateWithOptionalTime
                {
                    DateTime = new DateTimeOffset(
                        year: 2025,
                        month: 05,
                        day: 05,
                        hour: 11,
                        minute: 08,
                        second: 43,
                        offset: TimeSpan.Zero
                    ),
                    HasTime = true,
                }
            },
            {
                "\"2025-05-05T11:08:43.0000000\\u002B09:30\"",
                new DateWithOptionalTime
                {
                    DateTime = new DateTimeOffset(
                        year: 2025,
                        month: 05,
                        day: 05,
                        hour: 11,
                        minute: 08,
                        second: 43,
                        offset: new TimeSpan(hours: 9, minutes: 30, seconds: 0)
                    ),
                    HasTime = true,
                }
            },
        };
}
