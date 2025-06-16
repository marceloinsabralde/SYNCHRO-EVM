using System.Text;
using Kumara.Common.Utilities;

namespace Kumara.Common.Tests.Utilities;

public class PaginationTests
{
    [Fact]
    public void CreateContinuationTokenTest()
    {
        var id = Guid.CreateVersion7();
        var jsonString = $$$"""
            {"Id":"{{{id}}}","QueryParameters":{}}
            """;

        var expected = Convert.ToBase64String(Encoding.Default.GetBytes(jsonString));
        var actual = Pagination.CreateContinuationToken(id);
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
        var expected = new Pagination.ContinuationToken() { Id = id };
        var actual = Pagination.ParseContinuationToken(tokenString);
        actual.ShouldBeEquivalentTo(expected);
    }
}
