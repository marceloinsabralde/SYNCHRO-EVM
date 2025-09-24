// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;

namespace Kumara.Search.Tests.Controllers;

public class SearchControllerTests : ApplicationTestBase
{
    [Fact]
    public async Task Search_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/v1/search?q=test", CancellationToken.None);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
