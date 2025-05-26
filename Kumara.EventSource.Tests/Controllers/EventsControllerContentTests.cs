// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Text.Json;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Tests.Common;
using Kumara.EventSource.Tests.Controllers.Helpers;

namespace Kumara.EventSource.Tests.Controllers;

public class EventsControllerContentTests : EventsControllerTestBase
{
    [Fact]
    public async Task PostEvents_WithZeroEvents_ReturnsSuccessAndCountZero()
    {
        StringContent content = new("[]", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync(ApiBasePath, content);

        response.EnsureSuccessStatusCode();
        string responseString = await response.Content.ReadAsStringAsync();
        responseString.ShouldNotBeNull();
        responseString.ShouldContain("\"count\":0");
    }

    [Fact]
    public async Task PostEvents_WithMultipleEvents_ReturnsSuccessAndCorrectCount()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();

        List<Event> eventsPayload = new()
        {
            new Event
            {
                Type = "control.account.created.v1",
                Source = new Uri("/events/test"),
                SpecVersion = "1.0",
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
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
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
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
        HttpResponseMessage response = await _client.PostAsync(ApiBasePath, content);

        response.EnsureSuccessStatusCode();
        string responseString = await response.Content.ReadAsStringAsync();
        responseString.ShouldNotBeNull();
        responseString.ShouldContain("\"count\":2");
    }

    [Fact]
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
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
            },
            new()
            {
                Type = "FileUpload",
                Source = new Uri("/source/file"),
                Id = Guid.NewGuid(),
                SpecVersion = "1.0",
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
            },
        }.AsQueryable();

        await _eventRepository.AddEventsAsync(testEvents);

        HttpResponseMessage response = await _client.GetAsync(GetEventsEndpoint());

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

    [Fact]
    public async Task PostEvents_ValidEvent_ReturnsOk()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        Event @event = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.created.v1",
            Time = now,
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

        HttpResponseMessage response = await _client.PostAsync(ApiBasePath, content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostEvents_ValidUpdatedEvent_ReturnsOk()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        Event @event = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "control.account.updated.v1",
            Time = now,
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

        HttpResponseMessage response = await _client.PostAsync(ApiBasePath, content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostEvents_InvalidMediaType_ReturnsUnsupportedMediaType()
    {
        StringContent content = new("Invalid content");
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "text/plain"
        );

        // Submit POST request with invalid media type
        HttpResponseMessage response = await _client.PostAsync(ApiBasePath, content);

        response.StatusCode.ShouldBe(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task RoundTripEvents_ValidEvents_ReturnsEvents()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        List<Event> events = new()
        {
            new Event
            {
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
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
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
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
        HttpResponseMessage postResponse = await _client.PostAsync(ApiBasePath, content);

        postResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage response = await _client.GetAsync(ApiBasePath);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("TestSource1");
        responseContent.ShouldContain("TestSource2");
        responseContent.ShouldContain("control.account.created.v1");
        responseContent.ShouldContain("control.account.updated.v1");
    }
}
