// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;

namespace Kumara.EventSource.Tests.Controllers;

public class EventsControllerRoutingTests : EventsControllerTestBase
{
    [Fact]
    public async Task GetEvents_EndpointIsActive()
    {
        HttpResponseMessage? response = await _client.GetAsync("/events", CancellationToken.None);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostEvents_EndpointIsActive()
    {
        HttpResponseMessage response = await _client.PostAsync(
            "/events",
            new StringContent("[]", System.Text.Encoding.UTF8, "application/json"),
            CancellationToken.None
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
