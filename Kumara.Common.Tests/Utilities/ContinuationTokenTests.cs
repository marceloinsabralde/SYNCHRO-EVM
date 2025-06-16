// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text;
using Kumara.Common.Utilities;

namespace Kumara.Common.Tests.Utilities;

public class PaginationTests
{
    [Fact]
    public void ToBase64String()
    {
        var id = Guid.CreateVersion7();
        var jsonString = $$$"""
            {"Id":"{{{id}}}","QueryParameters":{}}
            """;

        var expected = Convert.ToBase64String(Encoding.Default.GetBytes(jsonString));
        var actual = new ContinuationToken() { Id = id }.ToBase64String();
        actual.ShouldBe(expected);
    }

    [Fact]
    public void ParseContinuationTokenTest()
    {
        var id = Guid.CreateVersion7();
        var jsonString = $$$"""
            {"Id":"{{{id}}}","QueryParameters":{}}
            """;
        var tokenString = Convert.ToBase64String(Encoding.Default.GetBytes(jsonString));
        var expected = new ContinuationToken() { Id = id };
        var actual = ContinuationToken.Parse(tokenString);
        actual.ShouldBeEquivalentTo(expected);
    }
}
