// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;

namespace Kumara.Search.Tests.HealthChecks;

public class HealthCheckTests : ApplicationTestBase
{
    [Fact]
    public async Task ReturnsHealthyResponse()
    {
        var response = await _client.GetAsync("/healthz", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        content.ShouldBe("Healthy");
    }
}
