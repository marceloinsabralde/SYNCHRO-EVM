// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Text.Json;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Tests.Common;
using Kumara.EventSource.Tests.Factories;

namespace Kumara.EventSource.Tests.Controllers;

public class EventsControllerActivityEventsTests : EventsControllerTestBase
{
    [Fact]
    public async Task PostEvents_WithActivityCreatedV1_ReturnsSuccessAndStoresEvent()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        string referenceCode = $"ACT-{Guid.NewGuid().ToString()[..6]}";
        Guid activityId = Guid.NewGuid();
        Guid controlAccountId = Guid.NewGuid();
        Guid itwinId = Guid.NewGuid();
        Guid accountId = Guid.NewGuid();

        ActivityCreatedV1 activityData = ActivityCreatedV1Factory.Create(
            id: activityId,
            referenceCode: referenceCode,
            controlAccountId: controlAccountId,
            plannedStart: now,
            plannedFinish: now.AddDays(10)
        );

        List<Event> eventsPayload = new()
        {
            new Event
            {
                Type = "activity.created.v1",
                Source = new Uri("https://example.com/TestSource"),
                SpecVersion = "1.0",
                ITwinId = itwinId,
                AccountId = accountId,
                CorrelationId = Guid.NewGuid().ToString(),
                Time = now,
                DataJson = JsonSerializer.SerializeToDocument(activityData),
            },
        };

        StringContent content = new(
            JsonSerializer.Serialize(eventsPayload),
            System.Text.Encoding.UTF8,
            "application/json"
        );
        HttpResponseMessage postResponse = await _client.PostAsync(
            GetEventsEndpoint(),
            content,
            TestContext.Current.CancellationToken
        );
        postResponse.EnsureSuccessStatusCode();
        string postResponseString = await postResponse.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        postResponseString.ShouldNotBeNull();
        postResponseString.ShouldContain("\"count\":1");

        // Get the event back to verify it was stored
        HttpResponseMessage getResponse = await _client.GetAsync(
            GetEventsEndpoint(new { itwinId, type = "activity.created.v1" }),
            TestContext.Current.CancellationToken
        );
        getResponse.EnsureSuccessStatusCode();

        string getResponseString = await getResponse.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        getResponseString.ShouldNotBeNull();
        getResponseString.ShouldContain("\"referenceCode\":\"" + referenceCode + "\"");
        getResponseString.ShouldContain(activityId.ToString());
        getResponseString.ShouldContain("\"data\":");
    }

    [Fact]
    public async Task PostEvents_WithActivityUpdatedV1_ReturnsSuccessAndStoresEvent()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        string referenceCode = $"ACT-{Guid.NewGuid().ToString()[..6]}";
        Guid activityId = Guid.NewGuid();
        Guid controlAccountId = Guid.NewGuid();
        Guid itwinId = Guid.NewGuid();
        Guid accountId = Guid.NewGuid();

        ActivityUpdatedV1 activityData = ActivityUpdatedV1Factory.Create(
            id: activityId,
            referenceCode: referenceCode,
            controlAccountId: controlAccountId,
            plannedStart: now,
            plannedFinish: now.AddDays(10),
            actualStart: now,
            actualFinish: now.AddDays(9)
        );

        List<Event> eventsPayload = new()
        {
            new Event
            {
                Type = "activity.updated.v1",
                Source = new Uri("https://example.com/TestSource"),
                SpecVersion = "1.0",
                ITwinId = itwinId,
                AccountId = accountId,
                CorrelationId = Guid.NewGuid().ToString(),
                Time = now,
                DataJson = JsonSerializer.SerializeToDocument(activityData),
            },
        };

        StringContent content = new(
            JsonSerializer.Serialize(eventsPayload),
            System.Text.Encoding.UTF8,
            "application/json"
        );
        HttpResponseMessage postResponse = await _client.PostAsync(
            GetEventsEndpoint(),
            content,
            TestContext.Current.CancellationToken
        );
        postResponse.EnsureSuccessStatusCode();
        string postResponseString = await postResponse.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        postResponseString.ShouldNotBeNull();
        postResponseString.ShouldContain("\"count\":1");

        // Get the event back to verify it was stored
        HttpResponseMessage getResponse = await _client.GetAsync(
            GetEventsEndpoint(new { itwinId, type = "activity.updated.v1" }),
            TestContext.Current.CancellationToken
        );
        getResponse.EnsureSuccessStatusCode();

        string getResponseString = await getResponse.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        getResponseString.ShouldNotBeNull();
        getResponseString.ShouldContain("\"referenceCode\":\"" + referenceCode + "\"");
        getResponseString.ShouldContain(activityId.ToString());
        getResponseString.ShouldContain("\"data\":");
    }

    [Fact]
    public async Task PostEvents_WithActivityCreatedV1MissingReferenceCode_ReturnsBadRequest()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();

        // Creating an invalid activity without ReferenceCode
        var invalidActivity = new
        {
            Id = Guid.NewGuid(),
            Name = "Test Activity Without ReferenceCode",
            ControlAccountId = Guid.NewGuid(),
            PlannedStart = now,
            PlannedFinish = now.AddDays(10),
        };

        List<Event> eventsPayload = new()
        {
            new Event
            {
                Type = "activity.created.v1",
                Source = new Uri("https://example.com/TestSource"),
                SpecVersion = "1.0",
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                Time = now,
                DataJson = JsonSerializer.SerializeToDocument(invalidActivity),
            },
        };

        StringContent content = new(
            JsonSerializer.Serialize(eventsPayload),
            System.Text.Encoding.UTF8,
            "application/json"
        );
        HttpResponseMessage response = await _client.PostAsync(
            GetEventsEndpoint(),
            content,
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string responseString = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        responseString.ShouldContain("referenceCode");
    }
}
