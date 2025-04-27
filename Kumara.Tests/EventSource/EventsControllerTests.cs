// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Kumara.EventSource.Controllers;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kumara.Tests.EventSource;

[TestClass]
public class EventsControllerTests
{
    private readonly HttpClient _client;

    private readonly IEventRepository _eventRepository = new EventRepositoryInMemoryList();

    public EventsControllerTests()
    {
        WebApplicationFactory<EventsController> factory =
            new WebApplicationFactory<EventsController>().WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => services.AddSingleton(_eventRepository))
            );

        _client = factory.CreateClient();
    }

    #region Routing Tests

    [TestMethod]
    public async Task GetEvents_EndpointIsActive()
    {
        HttpResponseMessage? response = await _client.GetAsync("/events");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task PostEvents_EndpointIsActive()
    {
        HttpResponseMessage response = await _client.PostAsync(
            "/events",
            new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    #endregion

    #region ContentType Tests

    [TestMethod]
    public async Task GetEvents_ReturnsCorrectContentType()
    {
        string expectedContentType = "application/json";
        EventEntity expectedEvent = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "test.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new TestCreatedV1
                {
                    TestString = "Expected Test String",
                    TestEnum = TestOptions.OptionB,
                    TestInteger = 100,
                }
            ),
        };

        await _eventRepository.AddEventsAsync(new[] { expectedEvent });

        HttpResponseMessage response = await _client.GetAsync("/events");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        string? contentType = response.Content.Headers.ContentType?.MediaType;
        contentType.ShouldBe(expectedContentType);

        string responseContent = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, options);

        paginatedResponse.ShouldNotBeNull();
        List<EventEntity> events = paginatedResponse.GetEvents();
        events.ShouldNotBeNull();
        events.Count.ShouldBeGreaterThan(0);

        EventEntity firstEvent = events[0];
        firstEvent.ShouldSatisfyAllConditions(
            e => e.ITwinGuid.ShouldBe(expectedEvent.ITwinGuid),
            e => e.AccountGuid.ShouldBe(expectedEvent.AccountGuid),
            e => e.CorrelationId.ShouldBe(expectedEvent.CorrelationId),
            e => e.Id.ShouldBe(expectedEvent.Id),
            e => e.SpecVersion.ShouldBe(expectedEvent.SpecVersion),
            e => e.Source.ShouldBe(expectedEvent.Source),
            e => e.Type.ShouldBe(expectedEvent.Type),
            e =>
                JsonSerializer
                    .Serialize(e.DataJson)
                    .ShouldBe(JsonSerializer.Serialize(expectedEvent.DataJson))
        );
    }

    [TestMethod]
    public async Task PostEvents_AcceptsCorrectContentType()
    {
        List<EventEntity> eventsPayload = new()
        {
            new EventEntity
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://example.com/TestSource"),
                Type = "test.created.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Payload Test String",
                        TestEnum = TestOptions.OptionC,
                        TestInteger = 200,
                    }
                ),
            },
        };

        string serialized = JsonSerializer.Serialize(eventsPayload);
        StringContent content = new(serialized, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync("/events", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task PostEvents_RejectsIncorrectContentType()
    {
        StringContent content = new("Invalid content", System.Text.Encoding.UTF8, "text/plain");

        HttpResponseMessage response = await _client.PostAsync("/events", content);

        response.StatusCode.ShouldBe(HttpStatusCode.UnsupportedMediaType);
    }

    #endregion

    #region Content Tests

    [TestMethod]
    public async Task PostEvents_WithZeroEvents_ReturnsSuccessAndCountZero()
    {
        StringContent content = new("[]", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("/events", content);

        response.EnsureSuccessStatusCode();
        string responseString = await response.Content.ReadAsStringAsync();
        responseString.ShouldNotBeNull();
        responseString.ShouldContain("\"count\":0");
    }

    [TestMethod]
    public async Task PostEvents_WithMultipleEvents_ReturnsSuccessAndCorrectCount()
    {
        List<EventEntity> eventsPayload = new()
        {
            new EventEntity
            {
                Type = "test.created.v1",
                Source = new Uri("/events/test"),
                SpecVersion = "1.0",
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Sample Test String",
                        TestEnum = TestOptions.OptionA,
                        TestInteger = 42,
                    }
                ),
            },
            new EventEntity
            {
                Type = "test.created.v1",
                Source = new Uri("/events/test"),
                SpecVersion = "1.0",
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Sample Test String",
                        TestEnum = TestOptions.OptionA,
                        TestInteger = 42,
                    }
                ),
            },
        };

        string serialized = JsonSerializer.Serialize(eventsPayload);
        StringContent content = new(serialized, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("/events", content);

        response.EnsureSuccessStatusCode();
        string responseString = await response.Content.ReadAsStringAsync();
        responseString.ShouldNotBeNull();
        responseString.ShouldContain("\"count\":2");
    }

    [TestMethod]
    public async Task GetEvents_ReturnsEventEntityBatch()
    {
        IQueryable<EventEntity> eventEntities = new List<EventEntity>
        {
            new()
            {
                Type = "UserLogin",
                Source = new Uri("/source/user"),
                Id = Guid.NewGuid(),
                SpecVersion = "1.0",
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
            },
            new()
            {
                Type = "FileUpload",
                Source = new Uri("/source/file"),
                Id = Guid.NewGuid(),
                SpecVersion = "1.0",
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
            },
        }.AsQueryable();

        await _eventRepository.AddEventsAsync(eventEntities);

        HttpResponseMessage response = await _client.GetAsync("/events");

        response.EnsureSuccessStatusCode();
        response
            .Content.Headers.ContentType?.ToString()
            .ShouldBe("application/json; charset=utf-8");

        string responseString = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseString, options);

        paginatedResponse.ShouldNotBeNull();
        List<EventEntity> events = paginatedResponse.GetEvents();
        events.ShouldNotBeNull();
        events.Count.ShouldBe(2);
        paginatedResponse.Links.ShouldNotBeNull();
        paginatedResponse.Links.Self.ShouldNotBeNull();
        paginatedResponse.Links.Next.ShouldBeNull(
            "No next link expected when all results fit on one page"
        );

        // Verify we have events of both expected types
        events.Any(e => e.Type == "UserLogin").ShouldBeTrue();
        events.Any(e => e.Type == "FileUpload").ShouldBeTrue();
    }

    #endregion

    #region Query Parameter Error Tests

    [TestMethod]
    public async Task GetEvents_UnknownQueryParameter_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync("/events?unknownParam=value");

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

    [TestMethod]
    public async Task GetEvents_InvalidIdFormat_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync("/events?id=invalid-guid-format");

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

    [TestMethod]
    public async Task GetEvents_InvalidITwinGuidFormat_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync("/events?itwinguid=not-a-valid-guid");

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
        detail.ShouldContain("'itwinguid'");

        problemDetails.GetProperty("status").GetInt32().ShouldBe(400);

        string? invalidParam = problemDetails.GetProperty("invalidParameter").GetString();
        invalidParam.ShouldNotBeNull();
        invalidParam.ShouldBe("itwinguid");
    }

    [TestMethod]
    public async Task GetEvents_InvalidAccountGuidFormat_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync("/events?accountguid=123-invalid");

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
        detail.ShouldContain("'accountguid'");

        problemDetails.GetProperty("status").GetInt32().ShouldBe(400);

        string? invalidParam = problemDetails.GetProperty("invalidParameter").GetString();
        invalidParam.ShouldNotBeNull();
        invalidParam.ShouldBe("accountguid");
    }

    #endregion

    [TestMethod]
    public async Task PostEvents_ValidEventEntity_ReturnsOk()
    {
        EventEntity eventEntity = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "test.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new TestCreatedV1
                {
                    TestString = "Controller Test String",
                    TestEnum = TestOptions.OptionE,
                    TestInteger = 500,
                }
            ),
        };

        string serialized = JsonSerializer.Serialize(new[] { eventEntity });
        StringContent content = new(serialized, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync("/events", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task PostEvents_ValidUpdatedEventEntity_ReturnsOk()
    {
        // Arrange
        var eventEntity = new EventEntity
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "test.updated.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new TestUpdatedV1
                {
                    TestString = "Updated Controller Test String",
                    TestEnum = TestOptions.OptionD,
                    TestInteger = 750,
                    UpdatedTime = DateTime.UtcNow,
                }
            ),
        };

        string serialized = JsonSerializer.Serialize(new[] { eventEntity });
        StringContent content = new(serialized, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync("/events", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task PostEvents_InvalidMediaType_ReturnsUnsupportedMediaType()
    {
        StringContent content = new("Invalid content");
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "text/plain"
        );

        // Submit POST request with invalid media type
        HttpResponseMessage response = await _client.PostAsync("/events", content);

        response.StatusCode.ShouldBe(HttpStatusCode.UnsupportedMediaType);
    }

    [TestMethod]
    public async Task RoundTripEvents_ValidEvents_ReturnsEventEntities()
    {
        List<EventEntity> eventEntities = new()
        {
            new EventEntity
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://example.com/TestSource1"),
                Type = "test.created.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Controller Test String",
                        TestEnum = TestOptions.OptionE,
                        TestInteger = 50,
                    }
                ),
            },
            new EventEntity
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://example.com/TestSource2"),
                Type = "test.updated.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestUpdatedV1
                    {
                        TestString = "Updated Controller Test String",
                        TestEnum = TestOptions.OptionB,
                        TestInteger = 100,
                        UpdatedTime = DateTime.UtcNow,
                    }
                ),
            },
        };

        string serialized = JsonSerializer.Serialize(eventEntities);
        StringContent content = new(serialized, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage postResponse = await _client.PostAsync("/events", content);

        postResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage response = await _client.GetAsync("/Events");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("TestSource1");
        responseContent.ShouldContain("TestSource2");
        responseContent.ShouldContain("test.created.v1");
        responseContent.ShouldContain("test.updated.v1");
    }
}
