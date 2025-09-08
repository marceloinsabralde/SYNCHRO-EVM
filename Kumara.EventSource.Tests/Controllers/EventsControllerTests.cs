// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Kumara.Common.Controllers.Responses;
using Kumara.EventSource.Controllers.Requests;
using Kumara.EventSource.Controllers.Responses;
using Kumara.EventSource.Tests.Factories;
using Kumara.TestCommon.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Kumara.EventSource.Tests.Controllers;

public class EventsControllerTests : DatabaseTestBase
{
    [Fact]
    public async Task Create_WithValidEventPayload_Created()
    {
        var iTwinId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();
        var eventType = "activity.created.v1";
        var eventData = JsonSerializer.SerializeToDocument(
            new
            {
                Id = Guid.CreateVersion7(),
                Name = "Test Activity",
                ReferenceCode = "ACT001",
            },
            JsonSerializerOptions.Web
        );

        var response = await _client.PostAsyncJson(
            GetPathByName("CreateEvents"),
            new
            {
                Events = new[]
                {
                    new
                    {
                        Id = eventId,
                        ITwinId = iTwinId,
                        AccountId = accountId,
                        EventType = eventType,
                        Data = eventData,
                    },
                },
            },
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(
            HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)
        );

        var createdEvent = await _dbContext.Events.FirstAsync(
            @event => @event.ITwinId == iTwinId && @event.AccountId == accountId,
            cancellationToken: TestContext.Current.CancellationToken
        );
        createdEvent.ShouldNotBeNull();
        createdEvent.ShouldSatisfyAllConditions(
            () => createdEvent.Id.ShouldNotBe(Guid.Empty),
            () => createdEvent.ITwinId.ShouldBe(iTwinId),
            () => createdEvent.EventType.ShouldBe(eventType),
            () =>
                JsonElement
                    .DeepEquals(eventData.RootElement, createdEvent.Data.RootElement)
                    .ShouldBeTrue(),
            () => createdEvent.CorrelationId.ShouldBeNull(),
            () => createdEvent.TriggeredByUserAt.ShouldBeNull(),
            () => createdEvent.TriggeredByUserSubject.ShouldBeNull()
        );
    }

    [Fact]
    public async Task Create_WithMultipleValidEvents_Created()
    {
        var iTwinId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var eventType = "activity.created.v1";
        var eventsToCreate = Enumerable
            .Range(0, 5)
            .Select(index => new
            {
                ITwinId = iTwinId,
                AccountId = accountId,
                EventType = eventType,
                Data = JsonSerializer.SerializeToDocument(
                    new
                    {
                        Id = Guid.CreateVersion7(),
                        Name = $"Test Activity 0{index + 1}",
                        ReferenceCode = $"ACT00{index}",
                    },
                    JsonSerializerOptions.Web
                ),
            });

        var response = await _client.PostAsyncJson(
            GetPathByName("CreateEvents"),
            new { Events = eventsToCreate },
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(
            HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)
        );

        _dbContext.Events.Count().ShouldBe(5);
    }

    [Fact]
    public async Task Create_WithInvalidType_BadRequest()
    {
        var iTwinId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();

        var response = await _client.PostAsyncJson(
            GetPathByName("CreateEvents"),
            new
            {
                Events = new[]
                {
                    new EventCreateRequest
                    {
                        ITwinId = iTwinId,
                        AccountId = accountId,
                        EventType = "invalid.type.v1",
                        Data = JsonSerializer.SerializeToDocument(
                            new { },
                            JsonSerializerOptions.Web
                        ),
                    },
                },
            },
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorBadRequest(
            new Dictionary<string, string[]>()
            {
                { "Events[0].EventType", ["\"invalid.type.v1\" is not a valid Event Type."] },
            }
        );
    }

    [Fact]
    public async Task Create_ValidTypeWithInvalidData_BadRequest()
    {
        var iTwinId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();

        var response = await _client.PostAsyncJson(
            GetPathByName("CreateEvents"),
            new
            {
                Events = new[]
                {
                    new EventCreateRequest
                    {
                        ITwinId = iTwinId,
                        AccountId = accountId,
                        EventType = "activity.updated.v1",
                        Data = JsonSerializer.SerializeToDocument(
                            new { },
                            JsonSerializerOptions.Web
                        ),
                    },
                },
            },
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorBadRequest(
            new Dictionary<string, string[]>()
            {
                {
                    "Events[0].Data",
                    ["The Data field does not conform to the \"activity.updated.v1\" Event Type."]
                },
            }
        );
    }

    [Fact]
    public async Task Create_EventMissingRequiredFields_BadRequest()
    {
        var iTwinId = Guid.CreateVersion7();

        var response = await _client.PostAsyncJson(
            GetPathByName("CreateEvents"),
            new
            {
                Events = new[]
                {
                    new EventCreateRequest
                    {
                        ITwinId = iTwinId,
                        EventType = "activity.deleted.v1",
                        Data = JsonSerializer.SerializeToDocument(
                            new { Id = Guid.CreateVersion7() },
                            JsonSerializerOptions.Web
                        ),
                    },
                },
            },
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorBadRequest(
            new Dictionary<string, string[]>()
            {
                { "Events[0].AccountId", ["AccountId must not be empty."] },
            }
        );
    }

    [Fact]
    public async Task Index_Success()
    {
        var accountId = Guid.CreateVersion7();
        var iTwinId = Guid.CreateVersion7();
        var events = Enumerable
            .Repeat(0, 10)
            .Select(index =>
            {
                var timestamp = DateTimeOffset.UtcNow.AddDays(-index);
                return EventFactory.CreateActivityCreatedV1Event(
                    eventId: Guid.CreateVersion7(timestamp),
                    accountId: accountId,
                    iTwinId: iTwinId
                );
            })
            .OrderBy(@event => @event.Id)
            .ToList();
        await _dbContext.Events.AddRangeAsync(events, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            GetPathByName("ListEvents"),
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<EventResponse>
        >();
        apiResponse.Items.ShouldNotBeNull();
        var expectedEventIds = events.Select(e => e.Id);
        apiResponse.Items.Select(e => e.Id).ShouldBe(expectedEventIds);

        Assert.EquivalentWithExclusions(
            events.Select(EventResponse.FromEvent).First(),
            apiResponse.Items.First(),
            "Data"
        );

        JsonElement
            .DeepEquals(apiResponse.Items.First().Data.RootElement, events.First().Data.RootElement)
            .ShouldBeTrue();
    }

    [Fact]
    public async Task Index_PaginationTest()
    {
        var accountId = Guid.CreateVersion7();
        var iTwinId = Guid.CreateVersion7();
        var events = Enumerable
            .Repeat(0, 15)
            .Select(index =>
            {
                var timestamp = DateTimeOffset.UtcNow.AddDays(-index);
                return EventFactory.CreateActivityCreatedV1Event(
                    eventId: Guid.CreateVersion7(timestamp),
                    accountId: accountId,
                    iTwinId: iTwinId
                );
            })
            .OrderBy(@event => @event.Id)
            .ToList();
        await _dbContext.Events.AddRangeAsync(events, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var requestPath = GetPathByName("ListEvents", new { iTwinId, _top = 5 });
        var response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        var apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<EventResponse>
        >();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: true);
        var eventsFromResponse = apiResponse.Items.ToList();

        eventsFromResponse.ShouldNotBeNull();
        var expectedEventIds = events.GetRange(0, 5).Select(e => e.Id);
        eventsFromResponse.Select(e => e.Id).ShouldBe(expectedEventIds);

        requestPath = apiResponse.Links.Next!.Href;
        response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        apiResponse = await response.ShouldBeApiResponse<PaginatedListResponse<EventResponse>>();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: true);
        eventsFromResponse = apiResponse.Items.ToList();

        eventsFromResponse.ShouldNotBeNull();
        expectedEventIds = events.GetRange(5, 5).Select(e => e.Id);
        eventsFromResponse.Select(e => e.Id).ShouldBe(expectedEventIds);

        requestPath = apiResponse.Links.Next!.Href;
        response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        apiResponse = await response.ShouldBeApiResponse<PaginatedListResponse<EventResponse>>();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: false);
        eventsFromResponse = apiResponse.Items.ToList();

        eventsFromResponse.ShouldNotBeNull();
        expectedEventIds = events.GetRange(10, 5).Select(e => e.Id);
        eventsFromResponse.Select(e => e.Id).ShouldBe(expectedEventIds);
    }

    [Fact]
    public async ValueTask Index_WithITwinFilter()
    {
        var iTwinId = Guid.CreateVersion7();
        var event1 = EventFactory.CreateActivityCreatedV1Event(iTwinId: iTwinId);
        var event2 = EventFactory.CreateActivityCreatedV1Event(iTwinId: iTwinId);

        var otherITwinId = Guid.CreateVersion7();
        var otherITwinEvent = EventFactory.CreateActivityCreatedV1Event(iTwinId: otherITwinId);

        await _dbContext.Events.AddRangeAsync(
            [event1, event2, otherITwinEvent],
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            GetPathByName("ListEvents", new { iTwinId }),
            TestContext.Current.CancellationToken
        );
        var apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<EventResponse>
        >();
        var eventsFromResponse = apiResponse.Items.ToList();

        eventsFromResponse.ShouldAllBe(e => e.ITwinId == iTwinId);
        eventsFromResponse.Select(e => e.Id).ShouldBe([event1.Id, event2.Id]);
    }

    [Fact]
    public async ValueTask Index_WithAccountFilter()
    {
        var accountId = Guid.CreateVersion7();
        var event1 = EventFactory.CreateActivityCreatedV1Event(accountId: accountId);
        var event2 = EventFactory.CreateActivityCreatedV1Event(accountId: accountId);

        var otherAccountId = Guid.CreateVersion7();
        var otherAccountEvent = EventFactory.CreateActivityCreatedV1Event(
            accountId: otherAccountId
        );

        await _dbContext.Events.AddRangeAsync(
            [event1, event2, otherAccountEvent],
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            GetPathByName("ListEvents", new { accountId }),
            TestContext.Current.CancellationToken
        );
        var apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<EventResponse>
        >();
        var eventsFromResponse = apiResponse.Items.ToList();

        eventsFromResponse.ShouldAllBe(e => e.AccountId == accountId);
        eventsFromResponse.Select(e => e.Id).ShouldBe([event1.Id, event2.Id]);
    }

    [Fact]
    public async ValueTask Index_WithTypeFilter()
    {
        var event1 = EventFactory.CreateActivityCreatedV1Event();
        var event2 = EventFactory.CreateActivityCreatedV1Event();
        var otherTypeEvent = EventFactory.CreateActivityDeletedV1Event();

        await _dbContext.Events.AddRangeAsync(
            [event1, event2, otherTypeEvent],
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            GetPathByName("ListEvents", new { type = "activity.created.v1" }),
            TestContext.Current.CancellationToken
        );
        var apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<EventResponse>
        >();
        var eventsFromResponse = apiResponse.Items.ToList();

        eventsFromResponse.ShouldAllBe(e => e.Type == "activity.created.v1");
        eventsFromResponse.Select(e => e.Id).ShouldBe([event1.Id, event2.Id]);
    }

    [Fact]
    public async ValueTask Index_WithInvalidTypeFilter_BadRequest()
    {
        var response = await _client.GetAsync(
            GetPathByName("ListEvents", new { type = "invalid.event.type" }),
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorBadRequest(
            new Dictionary<string, string[]>
            {
                { "type", ["\"invalid.event.type\" is not a valid Event Type."] },
            }
        );
    }
}
