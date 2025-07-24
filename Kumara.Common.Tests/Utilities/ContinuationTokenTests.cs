// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text;
using Kumara.Common.Utilities;

namespace Kumara.Common.Tests.Utilities;

public class ContinuationTokenTests
{
    static readonly Guid TestId = Guid.CreateVersion7();

    [Theory]
    [MemberData(nameof(ContinuationTokenTestData))]
    public void ToBase64String(string expected, ContinuationToken token)
    {
        token.ToBase64String().ShouldBe(expected);
    }

    [Theory]
    [MemberData(nameof(ContinuationTokenTestData))]
    public void ParseTest(string input, ContinuationToken expected)
    {
        var actual = ContinuationToken.Parse(input);
        actual.ShouldBeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(ContinuationTokenTestData))]
    public void TryParseTest(string input, ContinuationToken expected)
    {
        var success = ContinuationToken.TryParse(input, out var result);
        success.ShouldBeTrue();
        result.ShouldBeEquivalentTo(expected);
    }

    public static TheoryData<string, ContinuationToken> ContinuationTokenTestData =>
        new TheoryData<string, ContinuationToken>
        {
            {
                ConvertToBase64String($$$"""{"Id":"{{{TestId}}}","QueryParameters":{}}"""),
                new ContinuationToken() { Id = TestId }
            },
            {
                ConvertToBase64String(
                    $$$"""{"Id":"{{{TestId}}}","QueryParameters":{"foo":"bar","baz":"qux","1":"true"}}"""
                ),
                new ContinuationToken()
                {
                    Id = TestId,
                    QueryParameters = new()
                    {
                        { "foo", "bar" },
                        { "baz", "qux" },
                        { "1", "true" },
                    },
                }
            },
        };

    static string ConvertToBase64String(string input) =>
        Convert.ToBase64String(Encoding.Default.GetBytes(input));

    public sealed class TestClass
    {
        public Guid? GuidField { get; set; }
        public bool? BooleanField { get; set; }
        public string? StringField { get; set; }
    }

    [Fact]
    public void GenericContinuationTokenTest()
    {
        var input = ConvertToBase64String(
            $$$"""{"GuidField":"{{{TestId}}}","BooleanField":true,"StringField":"foo"}"""
        );
        ContinuationToken<TestClass> expected = new ContinuationToken<TestClass>(
            new()
            {
                GuidField = TestId,
                BooleanField = true,
                StringField = "foo",
            }
        );

        var result = ContinuationToken<TestClass>.Parse(input);
        result.ShouldBeEquivalentTo(expected);
    }
}
