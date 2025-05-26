// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Text.Json;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Tests.Common;
using Kumara.EventSource.Tests.Controllers.Helpers;

namespace Kumara.EventSource.Tests.Controllers;

public class EventsControllerContentTypeTests : EventsControllerTestBase
{
    [Fact]
    public async Task GetEvents_ReturnsCorrectContentType()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        string expectedContentType = "application/json";
        Event expectedEvent = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
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
            e => e.ITwinId.ShouldBe(expectedEvent.ITwinId),
            e => e.AccountId.ShouldBe(expectedEvent.AccountId),
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

    [Fact]
    public async Task PostEvents_AcceptsCorrectContentType()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        List<Event> eventsPayload = new()
        {
            new Event
            {
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
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

    [Fact]
    public async Task PostEvents_RejectsIncorrectContentType()
    {
        StringContent content = new("Invalid content", System.Text.Encoding.UTF8, "text/plain");

        HttpResponseMessage response = await _client.PostAsync("/events", content);

        response.StatusCode.ShouldBe(HttpStatusCode.UnsupportedMediaType);
    }
}
