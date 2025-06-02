// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Text.Json;

namespace Kumara.EventSource.Tests.Controllers;

public class EventsControllerQueryParameterErrorTests : EventsControllerTestBase
{
    [Fact]
    public async Task GetEvents_UnknownQueryParameter_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync(
            GetEventsEndpoint("unknownParam=value")
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        string content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNull();
        JsonElement problemDetails = JsonSerializer.Deserialize<JsonElement>(content);

        string? title = problemDetails.GetProperty("title").GetString();
        title.ShouldNotBeNull();
        title.ShouldBe("Unknown Query Parameter");

        string? detail = problemDetails.GetProperty("detail").GetString();
        detail.ShouldNotBeNull();
        detail.ShouldContain("unknownParam");

        problemDetails.GetProperty("status").GetInt32().ShouldBe(400);

        string? invalidParam = problemDetails.GetProperty("invalidParameter").GetString();
        invalidParam.ShouldNotBeNull();
        invalidParam.ShouldBe("unknownParam");
    }

    [Fact]
    public async Task GetEvents_InvalidIdFormat_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync(
            GetEventsEndpoint("id=invalid-guid-format")
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        string content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNull();
        JsonElement problemDetails = JsonSerializer.Deserialize<JsonElement>(content);

        string? title = problemDetails.GetProperty("title").GetString();
        title.ShouldNotBeNull();
        title.ShouldBe("Invalid Parameter Value");

        string? detail = problemDetails.GetProperty("detail").GetString();
        detail.ShouldNotBeNull();
        detail.ShouldContain("'invalid-guid-format'");
        detail.ShouldContain("'id'");

        problemDetails.GetProperty("status").GetInt32().ShouldBe(400);

        string? invalidParam = problemDetails.GetProperty("invalidParameter").GetString();
        invalidParam.ShouldNotBeNull();
        invalidParam.ShouldBe("id");
    }

    [Fact]
    public async Task GetEvents_InvalidITwinIdFormat_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync(
            GetEventsEndpoint("iTwinId=not-a-valid-guid")
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        string content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNull();
        JsonElement problemDetails = JsonSerializer.Deserialize<JsonElement>(content);

        string? title = problemDetails.GetProperty("title").GetString();
        title.ShouldNotBeNull();
        title.ShouldBe("Invalid Parameter Value");

        string? detail = problemDetails.GetProperty("detail").GetString();
        detail.ShouldNotBeNull();
        detail.ShouldContain("'not-a-valid-guid'");
        detail.ShouldContain("'iTwinId'");

        problemDetails.GetProperty("status").GetInt32().ShouldBe(400);

        string? invalidParam = problemDetails.GetProperty("invalidParameter").GetString();
        invalidParam.ShouldNotBeNull();
        invalidParam.ShouldBe("iTwinId");
    }

    [Fact]
    public async Task GetEvents_InvalidAccountIdFormat_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync(
            GetEventsEndpoint("accountId=123-invalid")
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        string content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNull();
        JsonElement problemDetails = JsonSerializer.Deserialize<JsonElement>(content);

        string? title = problemDetails.GetProperty("title").GetString();
        title.ShouldNotBeNull();
        title.ShouldBe("Invalid Parameter Value");

        string? detail = problemDetails.GetProperty("detail").GetString();
        detail.ShouldNotBeNull();
        detail.ShouldContain("'123-invalid'");
        detail.ShouldContain("'accountId'");

        problemDetails.GetProperty("status").GetInt32().ShouldBe(400);

        string? invalidParam = problemDetails.GetProperty("invalidParameter").GetString();
        invalidParam.ShouldNotBeNull();
        invalidParam.ShouldBe("accountId");
    }
}
