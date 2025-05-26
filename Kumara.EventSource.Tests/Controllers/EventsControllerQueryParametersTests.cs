// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Text.Json;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Tests.Common;
using Kumara.EventSource.Tests.Controllers.Helpers;

namespace Kumara.EventSource.Tests.Controllers;

public class EventsControllerQueryParametersTests : EventsControllerTestBase
{
    [Fact]
    public async Task GetEvents_WithIdParameter_ReturnsOnlyMatchingEvent()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        Guid targetId = Guid.NewGuid();

        Event matchingEvent = new()
        {
            Id = targetId,
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

    [Fact]
    public async Task GetEvents_WithITwinIdParameter_ReturnsOnlyMatchingEvents()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        Guid targetITwinId = Guid.NewGuid();
        Guid differentITwinId = Guid.NewGuid();

        Event matchingEvent = new()
        {
            ITwinId = targetITwinId,
            AccountId = Guid.NewGuid(),
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
            ITwinId = differentITwinId,
            AccountId = Guid.NewGuid(),
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

        HttpResponseMessage response = await _client.GetAsync($"/events?iTwinId={targetITwinId}");

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, options);

        paginatedResponse.ShouldNotBeNull();
        List<Event> events = paginatedResponse.GetEvents();
        events.ShouldNotBeNull();
        events[0].ITwinId.ShouldBe(targetITwinId);
    }

    [Fact]
    public async Task GetEvents_WithTypeParameter_ReturnsOnlyMatchingEvents()
    {
        string targetType = "test.type.filter";
        string differentType = "different.type";

        Event matchingEvent = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = targetType,
            DataJson = JsonSerializer.SerializeToDocument(new { Message = "Matching Type Event" }),
        };

        Event nonMatchingEvent = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
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

    [Fact]
    public async Task GetEvents_WithCorrelationIdParameter_ReturnsOnlyMatchingEvents()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        string targetCorrelationId = Guid.NewGuid().ToString();
        string differentCorrelationId = Guid.NewGuid().ToString();

        Event matchingEvent = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
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
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
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

    [Fact]
    public async Task GetEvents_WithAccountIdParameter_ReturnsOnlyMatchingEvents()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        Guid targetAccountId = Guid.NewGuid();
        Guid differentAccountId = Guid.NewGuid();

        Event matchingEvent = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = targetAccountId,
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
            ITwinId = Guid.NewGuid(),
            AccountId = differentAccountId,
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
            $"/events?accountId={targetAccountId}"
        );

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, options);

        paginatedResponse.ShouldNotBeNull();
        List<Event> events = paginatedResponse.GetEvents();
        events.ShouldNotBeNull();
        events[0].AccountId.ShouldBe(targetAccountId);
    }

    [Fact]
    public async Task GetEvents_WithMultipleParameters_ReturnsOnlyEventsMatchingAllCriteria()
    {
        Guid targetITwinId = Guid.NewGuid();
        string targetType = "test.combined.filter";

        Event matchingEvent = new()
        {
            ITwinId = targetITwinId,
            AccountId = Guid.NewGuid(),
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
            ITwinId = targetITwinId,
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "different.type",
            DataJson = JsonSerializer.SerializeToDocument(new { Message = "Matching iTwin only" }),
        };

        Event matchingTypeOnly = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
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
            $"/events?iTwinId={targetITwinId}&type={targetType}"
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
                e => e.ITwinId.ShouldBe(targetITwinId),
                e => e.Type.ShouldBe(targetType)
            );
    }

    [Fact]
    public async Task GetEvents_WithInvalidGuid_ReturnsBadRequest()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        Event event1 = new()
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

        HttpResponseMessage response = await _client.GetAsync("/events?iTwinId=not-a-valid-guid");

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
}
