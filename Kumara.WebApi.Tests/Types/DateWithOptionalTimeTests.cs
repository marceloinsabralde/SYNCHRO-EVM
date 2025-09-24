// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Globalization;
using System.Text.Json;
using Kumara.WebApi.Types;
using NodaTime;

namespace Kumara.WebApi.Tests.Types;

public class DateWithOptionalTimeTests
{
    [Theory]
    [MemberData(nameof(TestData))]
    public void ParseTest(string input, DateWithOptionalTime expected)
    {
        DateWithOptionalTime
            .Parse(input)
            .ShouldSatisfyAllConditions(
                actual => actual.HasTime.ShouldBe(expected.HasTime),
                actual => actual.DateTime.ShouldBe(expected.DateTime)
            );
    }

    [Fact]
    public void TryParse_Valid_Test()
    {
        var validInput = "2025-05-07";
        DateWithOptionalTime expected = new DateWithOptionalTime()
        {
            HasTime = false,
            DateTime = OffsetDateTime.FromDateTimeOffset(
                new DateTimeOffset(2025, 05, 07, 0, 0, 0, TimeSpan.Zero)
            ),
        };

        DateWithOptionalTime
            .TryParse(validInput, CultureInfo.CurrentCulture, out DateWithOptionalTime result)
            .ShouldBeTrue();
        result.ShouldBe(expected);
    }

    [Fact]
    public void TryParse_Invalid_Test()
    {
        var invalidInput = "invalid";

        DateWithOptionalTime
            .TryParse(invalidInput, CultureInfo.CurrentCulture, out DateWithOptionalTime result)
            .ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void DeserializeTest(string input, DateWithOptionalTime expected)
    {
        var serializedString = JsonSerializer.Serialize<string>(input);
        DateWithOptionalTime actual = JsonSerializer.Deserialize<DateWithOptionalTime>(
            serializedString
        );
        actual.HasTime.ShouldBe(expected.HasTime);
        actual.DateTime.ShouldBe(expected.DateTime);
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void SerializeTest(string expected, DateWithOptionalTime input)
    {
        string actual = JsonSerializer.Serialize(input);
        actual.ShouldBe(JsonSerializer.Serialize<string>(expected));
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void ToStringTest(string expected, DateWithOptionalTime input)
    {
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
                "2025-05-05",
                new DateWithOptionalTime
                {
                    DateTime = OffsetDateTime.FromDateTimeOffset(
                        new DateTimeOffset(
                            year: 2025,
                            month: 05,
                            day: 05,
                            hour: 00,
                            minute: 00,
                            second: 00,
                            offset: TimeSpan.Zero
                        )
                    ),
                    HasTime = false,
                }
            },
            {
                "2025-05-05T11:08:43Z",
                new DateWithOptionalTime
                {
                    DateTime = OffsetDateTime.FromDateTimeOffset(
                        new DateTimeOffset(
                            year: 2025,
                            month: 05,
                            day: 05,
                            hour: 11,
                            minute: 08,
                            second: 43,
                            offset: TimeSpan.Zero
                        )
                    ),
                    HasTime = true,
                }
            },
            {
                "2025-05-05T11:08:43+09:30",
                new DateWithOptionalTime
                {
                    DateTime = OffsetDateTime.FromDateTimeOffset(
                        new DateTimeOffset(
                            year: 2025,
                            month: 05,
                            day: 05,
                            hour: 11,
                            minute: 08,
                            second: 43,
                            offset: new TimeSpan(hours: 9, minutes: 30, seconds: 0)
                        )
                    ),
                    HasTime = true,
                }
            },
        };
}
