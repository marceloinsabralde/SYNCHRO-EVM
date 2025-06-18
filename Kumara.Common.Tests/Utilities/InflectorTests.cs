// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Utilities;

namespace Kumara.Common.Tests.Utilities;

public class InflectorTests
{
    [Theory]
    [MemberData(nameof(PluralizeTestData))]
    public void PluralizeTest(string singular, string plural)
    {
        Inflector.Pluralize(singular).ShouldBe(plural);
        Inflector.Pluralize(plural).ShouldBe(plural);
    }

    [Theory]
    [MemberData(nameof(PluralizeTestData))]
    public void SingularizeTest(string singular, string plural)
    {
        Inflector.Singularize(singular).ShouldBe(singular);
        Inflector.Singularize(plural).ShouldBe(singular);
    }

    [Theory]
    [MemberData(nameof(CamelizeTestData))]
    public void CamelizeTest(string input, string expected)
    {
        Inflector.Camelize(input).ShouldBe(expected);
        Inflector.Camelize(expected).ShouldBe(expected);
    }

    public static TheoryData<string, string> PluralizeTestData =>
        new TheoryData<string, string>
        {
            { "activity", "activities" },
            { "Activity", "Activities" },
            { "ControlAccount", "ControlAccounts" },
            { "controlAccount", "controlAccounts" },
            { "control account", "control accounts" },
            { "UnitOfMeasure", "UnitsOfMeasure" },
            { "unitOfMeasure", "unitsOfMeasure" },
            { "Unit of Measure", "Units of Measure" },
        };

    public static TheoryData<string, string> CamelizeTestData =>
        new TheoryData<string, string>
        {
            { "Activity", "activity" },
            { "ControlAccount", "controlAccount" },
            { "Unit of Measure", "unitOfMeasure" },
        };
}
