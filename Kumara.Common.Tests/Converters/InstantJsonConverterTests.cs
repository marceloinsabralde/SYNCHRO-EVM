// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.RegularExpressions;
using Kumara.Common.Converters;
using NodaTime.Text;

namespace Kumara.Common.Tests.Converters;

public class InstantJsonConverterTests
{
    [Theory]
    [InlineData("2025-06-18T04:15:06.630135Z")]
    [InlineData("2025-06-18T04:15:06.630135+00:00")]
    public void ConvertsUTCStringsToInstants(string testString)
    {
        var converter = new InstantJsonConverter();

        var expectedInstant = OffsetDateTimePattern.ExtendedIso.Parse(testString).Value.ToInstant();
        var actualInstant = converter.ConvertFromProvider(testString);
        actualInstant.ShouldBe(expectedInstant);

        var expectedString = Regex.Replace(testString, @"\+00:00$", "Z");
        var actualString = converter.ConvertToProvider(actualInstant);
        actualString.ShouldBe(expectedString);
    }
}
