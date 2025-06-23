// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using System.Text.Json.Serialization;
using Kumara.Common.Converters;
using Kumara.TestCommon.Helpers;

namespace Kumara.Common.Tests.Converters;

public class EnumToJsonStringValueConverterTests
{
    readonly EnumToJsonStringValueConverter<TestEnum> _converter = new();

    [Theory]
    [InlineData(TestEnum.FirstValue, "first_value")]
    [InlineData(TestEnum.SecondValue, "custom_second_value")]
    public void ConvertsEnumsToJsonStringValues(TestEnum expectedEnum, string expectedString)
    {
        var appSerializedString = JsonSerializer.Deserialize<string>(
            JsonSerializer.Serialize(expectedEnum, AppServicesHelper.JsonSerializerOptions)
        );
        expectedString.ShouldBe(appSerializedString);

        var actualString = _converter.ConvertToProvider(expectedEnum);
        actualString.ShouldBe(expectedString);

        var actualEnum = _converter.ConvertFromProvider(expectedString);
        actualEnum.ShouldBe(expectedEnum);
    }

    [Theory]
    [InlineData("invalid_value")] // not in list
    [InlineData("\"first_value\"")] // JSON string
    [InlineData("null")] // JSON null
    [InlineData("")] // empty
    [InlineData("firstValue")] // camel case
    [InlineData("FirstValue")] // title case
    [InlineData("0")] // integer index
    public void ThrowsOnInvalidStringValues(string invalidString)
    {
        var ex = Should.Throw<FormatException>(() =>
        {
            _converter.ConvertFromProvider(invalidString);
        });
        ex.Message.ShouldContain(JsonSerializer.Serialize(invalidString));
        ex.Message.ShouldContain("TestEnum");
    }

    public enum TestEnum
    {
        FirstValue,

        [JsonStringEnumMemberName("custom_second_value")]
        SecondValue,
    }
}
