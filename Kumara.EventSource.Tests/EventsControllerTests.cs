// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Collections.Specialized;
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

namespace Kumara.EventSource.Tests;

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
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();
        string expectedContentType = "application/json";
        Event expectedEvent = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Expected Account",
                    WbsPath = "1.2.3",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = DateTimeOffset.Now,
                    PlannedFinish = DateTimeOffset.Now.AddDays(10),
                    ActualStart = DateTimeOffset.Now,
                    ActualFinish = DateTimeOffset.Now.AddDays(9),
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
        List<Event> events = paginatedResponse.GetEvents();
        events.ShouldNotBeNull();
        events.Count.ShouldBeGreaterThan(0);

        Event firstEvent = events[0];
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
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();
        List<Event> eventsPayload = new()
        {
            new Event
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://example.com/TestSource"),
                Type = "control.account.created.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new ControlAccountCreatedV1
                    {
                        Id = Guid.NewGuid(),
                        Name = "Payload Account",
                        WbsPath = "1.2.3",
                        TaskId = Guid.NewGuid(),
                        PlannedStart = DateTimeOffset.Now,
                        PlannedFinish = DateTimeOffset.Now.AddDays(10),
                        ActualStart = DateTimeOffset.Now,
                        ActualFinish = DateTimeOffset.Now.AddDays(9),
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
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();

        List<Event> eventsPayload = new()
        {
            new Event
            {
                Type = "control.account.created.v1",
                Source = new Uri("/events/test"),
                SpecVersion = "1.0",
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                DataJson = JsonSerializer.SerializeToDocument(
                    new ControlAccountCreatedV1
                    {
                        Id = Guid.NewGuid(),
                        Name = "Sample Account 1",
                        WbsPath = "21.22.23",
                        TaskId = Guid.NewGuid(),
                        PlannedStart = now,
                        PlannedFinish = now.AddDays(2),
                        ActualStart = now,
                        ActualFinish = now.AddDays(1),
                    }
                ),
            },
            new Event
            {
                Type = "control.account.created.v1",
                Source = new Uri("/events/test"),
                SpecVersion = "1.0",
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                DataJson = JsonSerializer.SerializeToDocument(
                    new ControlAccountCreatedV1
                    {
                        Id = Guid.NewGuid(),
                        Name = "Sample Account 2",
                        WbsPath = "22.23.24",
                        TaskId = Guid.NewGuid(),
                        PlannedStart = now,
                        PlannedFinish = now.AddDays(3),
                        ActualStart = now,
                        ActualFinish = now.AddDays(2),
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
    public async Task GetEvents_ReturnsEventBatch()
    {
        IQueryable<Event> testEvents = new List<Event>
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

        await _eventRepository.AddEventsAsync(testEvents);

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
        List<Event> events = paginatedResponse.GetEvents();
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
    public async Task PostEvents_ValidEvent_ReturnsOk()
    {
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();
        Event @event = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Controller Account",
                    WbsPath = "1.2.3",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(10),
                    ActualStart = now,
                    ActualFinish = now.AddDays(9),
                }
            ),
        };

        string serialized = JsonSerializer.Serialize(new[] { @event });
        StringContent content = new(serialized, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync("/events", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task PostEvents_ValidUpdatedEvent_ReturnsOk()
    {
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();
        Event @event = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.updated.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountUpdatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Updated Controller Account",
                    WbsPath = "1.2.3",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(10),
                    ActualStart = now,
                    ActualFinish = now.AddDays(9),
                }
            ),
        };

        string serialized = JsonSerializer.Serialize(new[] { @event });
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
    public async Task RoundTripEvents_ValidEvents_ReturnsEvents()
    {
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();
        List<Event> events = new()
        {
            new Event
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://example.com/TestSource1"),
                Type = "control.account.created.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new ControlAccountCreatedV1
                    {
                        Id = Guid.NewGuid(),
                        Name = "Controller Account",
                        WbsPath = "1.2.3",
                        TaskId = Guid.NewGuid(),
                        PlannedStart = now,
                        PlannedFinish = now.AddDays(10),
                        ActualStart = now,
                        ActualFinish = now.AddDays(9),
                    }
                ),
            },
            new Event
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://example.com/TestSource2"),
                Type = "control.account.updated.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new ControlAccountUpdatedV1
                    {
                        Id = Guid.NewGuid(),
                        Name = "Updated Controller Account",
                        WbsPath = "24.25.26",
                        TaskId = Guid.NewGuid(),
                        PlannedStart = now,
                        PlannedFinish = now.AddDays(4),
                        ActualStart = now,
                        ActualFinish = now.AddDays(3),
                    }
                ),
            },
        };

        string serialized = JsonSerializer.Serialize(events);
        StringContent content = new(serialized, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage postResponse = await _client.PostAsync("/events", content);

        postResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage response = await _client.GetAsync("/Events");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("TestSource1");
        responseContent.ShouldContain("TestSource2");
        responseContent.ShouldContain("control.account.created.v1");
        responseContent.ShouldContain("control.account.updated.v1");
    }

    #region QueryParameters Tests

    [TestMethod]
    public async Task GetEvents_WithIdParameter_ReturnsOnlyMatchingEvent()
    {
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();
        Guid targetId = Guid.NewGuid();

        Event matchingEvent = new()
        {
            Id = targetId,
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Matching ID Event",
                    WbsPath = "27.28.29",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(5),
                    ActualStart = now,
                    ActualFinish = now.AddDays(4),
                }
            ),
        };

        Event nonMatchingEvent = new()
        {
            Id = Guid.NewGuid(),
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Non-matching ID Event",
                    WbsPath = "28.29.30",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(6),
                    ActualStart = now,
                    ActualFinish = now.AddDays(5),
                }
            ),
        };

        await _eventRepository.AddEventsAsync(new[] { matchingEvent, nonMatchingEvent });

        HttpResponseMessage response = await _client.GetAsync($"/events?id={targetId}");

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, options);

        paginatedResponse.ShouldNotBeNull();
        List<Event> events = paginatedResponse.GetEvents();
        events.ShouldNotBeNull();
        events[0].Id.ShouldBe(targetId);
    }

    [TestMethod]
    public async Task GetEvents_WithITwinGuidParameter_ReturnsOnlyMatchingEvents()
    {
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();
        Guid targetITwinGuid = Guid.NewGuid();
        Guid differentITwinGuid = Guid.NewGuid();

        Event matchingEvent = new()
        {
            ITwinGuid = targetITwinGuid,
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Matching iTwin Event",
                    WbsPath = "31.32.33",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(7),
                    ActualStart = now,
                    ActualFinish = now.AddDays(6),
                }
            ),
        };

        Event nonMatchingEvent = new()
        {
            ITwinGuid = differentITwinGuid,
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Non-matching iTwin Event",
                    WbsPath = "32.33.34",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(8),
                    ActualStart = now,
                    ActualFinish = now.AddDays(7),
                }
            ),
        };

        await _eventRepository.AddEventsAsync(new[] { matchingEvent, nonMatchingEvent });

        HttpResponseMessage response = await _client.GetAsync(
            $"/events?iTwinGuid={targetITwinGuid}"
        );

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, options);

        paginatedResponse.ShouldNotBeNull();
        List<Event> events = paginatedResponse.GetEvents();
        events.ShouldNotBeNull();
        events[0].ITwinGuid.ShouldBe(targetITwinGuid);
    }

    [TestMethod]
    public async Task GetEvents_WithTypeParameter_ReturnsOnlyMatchingEvents()
    {
        string targetType = "test.type.filter";
        string differentType = "different.type";

        Event matchingEvent = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = targetType,
            DataJson = JsonSerializer.SerializeToDocument(new { Message = "Matching Type Event" }),
        };

        Event nonMatchingEvent = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = differentType,
            DataJson = JsonSerializer.SerializeToDocument(
                new { Message = "Non-matching Type Event" }
            ),
        };

        await _eventRepository.AddEventsAsync(new[] { matchingEvent, nonMatchingEvent });

        HttpResponseMessage response = await _client.GetAsync($"/events?type={targetType}");

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, options);

        paginatedResponse.ShouldNotBeNull();
        List<Event> events = paginatedResponse.GetEvents();
        events.ShouldNotBeNull();
        events[0].Type.ShouldBe(targetType);
    }

    [TestMethod]
    public async Task GetEvents_WithCorrelationIdParameter_ReturnsOnlyMatchingEvents()
    {
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();
        string targetCorrelationId = Guid.NewGuid().ToString();
        string differentCorrelationId = Guid.NewGuid().ToString();

        Event matchingEvent = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = targetCorrelationId,
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Matching Correlation Event",
                    WbsPath = "35.36.37",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(9),
                    ActualStart = now,
                    ActualFinish = now.AddDays(8),
                }
            ),
        };

        Event nonMatchingEvent = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = differentCorrelationId,
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Non-matching Correlation Event",
                    WbsPath = "36.37.38",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(10),
                    ActualStart = now,
                    ActualFinish = now.AddDays(9),
                }
            ),
        };

        await _eventRepository.AddEventsAsync(new[] { matchingEvent, nonMatchingEvent });

        HttpResponseMessage response = await _client.GetAsync(
            $"/events?correlationId={targetCorrelationId}"
        );

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, options);

        paginatedResponse.ShouldNotBeNull();
        List<Event> events = paginatedResponse.GetEvents();
        events.ShouldNotBeNull();
        events[0].CorrelationId.ShouldBe(targetCorrelationId);
    }

    [TestMethod]
    public async Task GetEvents_WithAccountGuidParameter_ReturnsOnlyMatchingEvents()
    {
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();
        Guid targetAccountGuid = Guid.NewGuid();
        Guid differentAccountGuid = Guid.NewGuid();

        Event matchingEvent = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = targetAccountGuid,
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Matching Account Event",
                    WbsPath = "39.40.41",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(11),
                    ActualStart = now,
                    ActualFinish = now.AddDays(10),
                }
            ),
        };

        Event nonMatchingEvent = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = differentAccountGuid,
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Non-matching Account Event",
                    WbsPath = "40.41.42",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(12),
                    ActualStart = now,
                    ActualFinish = now.AddDays(11),
                }
            ),
        };

        await _eventRepository.AddEventsAsync(new[] { matchingEvent, nonMatchingEvent });

        HttpResponseMessage response = await _client.GetAsync(
            $"/events?accountGuid={targetAccountGuid}"
        );

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, options);

        paginatedResponse.ShouldNotBeNull();
        List<Event> events = paginatedResponse.GetEvents();
        events.ShouldNotBeNull();
        events[0].AccountGuid.ShouldBe(targetAccountGuid);
    }

    [TestMethod]
    public async Task GetEvents_WithMultipleParameters_ReturnsOnlyEventsMatchingAllCriteria()
    {
        Guid targetITwinGuid = Guid.NewGuid();
        string targetType = "test.combined.filter";

        Event matchingEvent = new()
        {
            ITwinGuid = targetITwinGuid,
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = targetType,
            DataJson = JsonSerializer.SerializeToDocument(
                new { Message = "Matching All Criteria" }
            ),
        };

        Event matchingITwinOnly = new()
        {
            ITwinGuid = targetITwinGuid,
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "different.type",
            DataJson = JsonSerializer.SerializeToDocument(new { Message = "Matching iTwin only" }),
        };

        Event matchingTypeOnly = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = targetType,
            DataJson = JsonSerializer.SerializeToDocument(new { Message = "Matching type only" }),
        };

        await _eventRepository.AddEventsAsync(
            new[] { matchingEvent, matchingITwinOnly, matchingTypeOnly }
        );

        HttpResponseMessage response = await _client.GetAsync(
            $"/events?iTwinGuid={targetITwinGuid}&type={targetType}"
        );

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, options);

        paginatedResponse.ShouldNotBeNull();
        List<Event> events = paginatedResponse.GetEvents();
        events.ShouldNotBeNull();
        events[0]
            .ShouldSatisfyAllConditions(
                e => e.ITwinGuid.ShouldBe(targetITwinGuid),
                e => e.Type.ShouldBe(targetType)
            );
    }

    [TestMethod]
    public async Task GetEvents_WithInvalidGuid_ReturnsBadRequest()
    {
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();
        Event event1 = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Event 1",
                    WbsPath = "14.15.16",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(19),
                    ActualStart = now,
                    ActualFinish = now.AddDays(18),
                }
            ),
        };

        Event event2 = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Event 2",
                    WbsPath = "15.16.17",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(20),
                    ActualStart = now,
                    ActualFinish = now.AddDays(19),
                }
            ),
        };

        await _eventRepository.AddEventsAsync(new[] { event1, event2 });

        HttpResponseMessage response = await _client.GetAsync("/events?iTwinGuid=not-a-valid-guid");

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
        detail.ShouldContain("'iTwinGuid'");

        problemDetails.GetProperty("status").GetInt32().ShouldBe(400);

        string? invalidParam = problemDetails.GetProperty("invalidParameter").GetString();
        invalidParam.ShouldNotBeNull();
        invalidParam.ShouldBe("iTwinGuid");
    }

    #endregion

    #region Pagination Tests

    [TestMethod]
    public async Task GetEvents_WithNoPaginationParameters_ReturnsDefaultPaginatedEvents()
    {
        // Create enough events to trigger pagination (more than default page size)
        List<Event> events = new();
        for (int i = 0; i < 60; i++)
        {
            // Add delay to ensure different UUID v7 IDs (time-ordered)
            await Task.Delay(5);

            events.Add(
                new Event
                {
                    ITwinGuid = Guid.NewGuid(),
                    AccountGuid = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    SpecVersion = "1.0",
                    Source = new Uri($"http://example.com/TestSource{i}"),
                    Type = "test.pagination.default",
                    Id = Guid.CreateVersion7(), // UUID v7 for time ordering
                    DataJson = JsonSerializer.SerializeToDocument(
                        new { Index = i, Message = $"Default Pagination Test {i}" }
                    ),
                }
            );
        }

        await _eventRepository.AddEventsAsync(events);

        HttpResponseMessage response = await _client.GetAsync("/events");

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();

        // Deserialize as PaginatedEvents
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, options);

        // Verify pagination structure
        paginatedResponse.ShouldNotBeNull();
        paginatedResponse.Items.ShouldNotBeNull();
        paginatedResponse.Links.ShouldNotBeNull();
        paginatedResponse.Links.Self.ShouldNotBeNull();
        paginatedResponse.Links.Next.ShouldNotBeNull(
            "Should have a Next link when more pages exist"
        );
        paginatedResponse.Items.Count.ShouldBe(50); // Default page size
    }

    [TestMethod]
    public async Task GetEvents_WithPageSize_ReturnsPaginatedEventsWithSpecifiedSize()
    {
        List<Event> events = new();
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(5);
            events.Add(
                new Event
                {
                    ITwinGuid = Guid.NewGuid(),
                    AccountGuid = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    SpecVersion = "1.0",
                    Source = new Uri($"http://example.com/TestSource{i}"),
                    Type = "test.pagination.pagesize",
                    Id = Guid.CreateVersion7(), // UUID v7 for time ordering
                    DataJson = JsonSerializer.SerializeToDocument(
                        new { Index = i, Message = $"PageSize Test {i}" }
                    ),
                }
            );
        }

        await _eventRepository.AddEventsAsync(events);

        // Set custom page size
        int customPageSize = 15;

        HttpResponseMessage response = await _client.GetAsync($"/events?top={customPageSize}");

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();

        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, options);

        paginatedResponse.ShouldNotBeNull();
        paginatedResponse.Items.ShouldNotBeNull();
        paginatedResponse.Links.ShouldNotBeNull();
        paginatedResponse.Links.Self.ShouldNotBeNull();
        paginatedResponse.Links.Next.ShouldNotBeNull(
            "Should have a Next link when more pages exist"
        );
    }

    [TestMethod]
    public async Task GetEvents_WithContinuationToken_ReturnsNextPage()
    {
        List<Event> events = new();
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(5);
            events.Add(
                new Event
                {
                    ITwinGuid = Guid.NewGuid(),
                    AccountGuid = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    SpecVersion = "1.0",
                    Source = new Uri($"http://example.com/TestSource{i}"),
                    Type = "test.pagination.continuation",
                    Id = Guid.CreateVersion7(), // UUID v7 for time ordering
                    DataJson = JsonSerializer.SerializeToDocument(
                        new { Index = i, Message = $"Continuation Test {i}" }
                    ),
                }
            );
        }

        await _eventRepository.AddEventsAsync(events);

        // Get first page
        int pageSize = 10;
        HttpResponseMessage firstResponse = await _client.GetAsync(
            $"/events?type=test.pagination.continuation&top={pageSize}"
        );
        firstResponse.EnsureSuccessStatusCode();

        string firstResponseContent = await firstResponse.Content.ReadAsStringAsync();

        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? firstPage = JsonSerializer.Deserialize<PaginatedResponseWrapper>(
            firstResponseContent,
            options
        );

        firstPage.ShouldNotBeNull();
        firstPage.Links.Next.ShouldNotBeNull("First page should have a next link");

        // Get the Next URL which should contain the continuation token
        string nextUrl = firstPage.Links.Next.Href;

        // Extract continuation token from the next link URL
        NameValueCollection queryParams = System.Web.HttpUtility.ParseQueryString(
            new Uri(nextUrl).Query
        );
        string continuationToken = queryParams["continuationtoken"] ?? "";

        continuationToken.ShouldNotBeNullOrEmpty(
            "Should be able to extract continuation token from Next link"
        );

        HttpResponseMessage secondResponse = await _client.GetAsync(
            $"/events?top={pageSize}&continuationtoken={continuationToken}"
        );

        secondResponse.EnsureSuccessStatusCode();
        string secondResponseContent = await secondResponse.Content.ReadAsStringAsync();
        PaginatedResponseWrapper? secondPage = JsonSerializer.Deserialize<PaginatedResponseWrapper>(
            secondResponseContent,
            options
        );

        secondPage.ShouldNotBeNull();
        List<Event> secondPageEvents = secondPage.GetEvents();
        secondPageEvents.ShouldNotBeNull();
        secondPageEvents.Count.ShouldBe(pageSize);

        // Ensure no duplicate events between pages
        List<Guid> firstPageIds = firstPage.GetEvents().Select(e => e.Id).ToList();
        List<Guid> secondPageIds = secondPageEvents.Select(e => e.Id).ToList();
        firstPageIds.Intersect(secondPageIds).ShouldBeEmpty();

        // Verify that type filter is preserved from the token
        secondPage
            .Items.All(e => e.Type == "test.pagination.continuation")
            .ShouldBeTrue("The type filter should be preserved from the continuation token");
    }

    [TestMethod]
    public async Task GetEvents_WithContinuationTokenAndFilters_ReturnsCombinedResult()
    {
        Guid targetITwinGuid = Guid.NewGuid();
        string eventType = "test.pagination.combined";

        List<Event> events = new();
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(5);
            events.Add(
                new Event
                {
                    ITwinGuid = targetITwinGuid, // Use the same ITwinGuid for all events
                    AccountGuid = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    SpecVersion = "1.0",
                    Source = new Uri($"http://example.com/TestSource{i}"),
                    Type = eventType,
                    Id = Guid.CreateVersion7(), // UUID v7 for time ordering
                    DataJson = JsonSerializer.SerializeToDocument(
                        new { Index = i, Message = $"Combined Filter Test {i}" }
                    ),
                }
            );
        }

        await _eventRepository.AddEventsAsync(events);

        // Get first page with filters
        int pageSize = 10;
        HttpResponseMessage firstResponse = await _client.GetAsync(
            $"/events?iTwinGuid={targetITwinGuid}&type={eventType}&top={pageSize}"
        );

        firstResponse.EnsureSuccessStatusCode();
        string firstResponseContent = await firstResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"First page response: {firstResponseContent}");

        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? firstPage = JsonSerializer.Deserialize<PaginatedResponseWrapper>(
            firstResponseContent,
            options
        );

        firstPage.ShouldNotBeNull();
        firstPage.Links.Next.ShouldNotBeNull("First page should have a next link");

        // Debug the Next URL
        string nextUrl = firstPage.Links.Next.Href;
        Console.WriteLine($"Next URL: {nextUrl}");

        // Extract continuation token from the next link URL using more robust parsing
        NameValueCollection queryParams = System.Web.HttpUtility.ParseQueryString(
            new Uri(nextUrl).Query
        );
        string continuationToken =
            queryParams["continuationtoken"] ?? queryParams["continuationToken"] ?? "";

        continuationToken.ShouldNotBeNullOrEmpty(
            "Should be able to extract continuation token from Next link"
        );

        HttpResponseMessage secondResponse = await _client.GetAsync(
            $"/events?iTwinGuid={targetITwinGuid}&type={eventType}&top={pageSize}&continuationtoken={continuationToken}"
        );

        secondResponse.EnsureSuccessStatusCode();
        string secondResponseContent = await secondResponse.Content.ReadAsStringAsync();
        PaginatedResponseWrapper? secondPage = JsonSerializer.Deserialize<PaginatedResponseWrapper>(
            secondResponseContent,
            options
        );

        secondPage.ShouldNotBeNull();
        secondPage.Items.ShouldNotBeNull();

        // Verify all events match the filter criteria
        foreach (Event evt in secondPage.Items)
        {
            evt.ITwinGuid.ShouldBe(targetITwinGuid);
            evt.Type.ShouldBe(eventType);
        }
    }

    #endregion

    // Helper class used for deserialization in tests
    private class PaginatedResponseWrapper
    {
        public List<Event> Items { get; set; } = new();
        public PaginationLinksResponse Links { get; set; } = new();

        public List<Event> GetEvents() => Items;
    }

    private class PaginationLinksResponse
    {
        public PaginationLinkResponse Self { get; set; } = new();
        public PaginationLinkResponse? Next { get; set; }
    }

    private class PaginationLinkResponse
    {
        public string Href { get; set; } = string.Empty;
    }
}
