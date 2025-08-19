// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.Common.Utilities;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;

namespace Kumara.EventSource.Tests.Repositories;

/// <summary>
/// Base class for event repository tests providing common test methods.
/// Concrete implementations should provide their specific initialization logic.
/// </summary>
public abstract class EventRepositoryTestsBase
{
    /// <summary>
    /// Gets the event repository to test.
    /// </summary>
    protected abstract IEventRepository EventRepository { get; }

    [Fact]
    public async Task RoundtripEventsAsync_ShouldStoreAndRetrieveEvents()
    {
        List<Event> events = EventRepositoryTestUtilities.GetTestEvents();

        await EventRepository.AddEventsAsync(events, TestContext.Current.CancellationToken);
        IQueryable<Event> retrievedEvents = await EventRepository.QueryEventsAsync(
            new EventQueryBuilder(),
            TestContext.Current.CancellationToken
        );

        events.Select(e => e.ITwinId).ShouldBeSubsetOf(retrievedEvents.Select(e => e.ITwinId));
    }

    [Fact]
    public async Task QueryEventsByITwinId_ShouldReturnMatchingEvents()
    {
        List<Event> events = EventRepositoryTestUtilities.GetTestEvents();
        await EventRepository.AddEventsAsync(events, TestContext.Current.CancellationToken);
        Guid targetITwinId = events.First().ITwinId;

        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereITwinId(targetITwinId);
        IQueryable<Event> retrievedEvents = await EventRepository.QueryEventsAsync(
            queryBuilder,
            TestContext.Current.CancellationToken
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.ITwinId == targetITwinId).ShouldBeTrue();
    }

    [Fact]
    public async Task QueryEventsByAccountId_ShouldReturnMatchingEvents()
    {
        List<Event> events = EventRepositoryTestUtilities.GetTestEvents();
        await EventRepository.AddEventsAsync(events, TestContext.Current.CancellationToken);
        Guid targetAccountId = events.First().AccountId;

        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereAccountId(targetAccountId);
        IQueryable<Event> retrievedEvents = await EventRepository.QueryEventsAsync(
            queryBuilder,
            TestContext.Current.CancellationToken
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.AccountId == targetAccountId).ShouldBeTrue();
    }

    [Fact]
    public async Task QueryEventsByCorrelationId_ShouldReturnMatchingEvents()
    {
        List<Event> events = EventRepositoryTestUtilities.GetTestEvents();
        await EventRepository.AddEventsAsync(events, TestContext.Current.CancellationToken);
        string targetCorrelationId = events.First().CorrelationId;

        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereCorrelationId(
            targetCorrelationId
        );
        IQueryable<Event> retrievedEvents = await EventRepository.QueryEventsAsync(
            queryBuilder,
            TestContext.Current.CancellationToken
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.CorrelationId == targetCorrelationId).ShouldBeTrue();
    }

    [Fact]
    public async Task QueryEventsByType_ShouldReturnMatchingEvents()
    {
        List<Event> events = EventRepositoryTestUtilities.GetTestEvents();
        await EventRepository.AddEventsAsync(events, TestContext.Current.CancellationToken);
        string targetType = events.First().Type;

        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereType(targetType);
        IQueryable<Event> retrievedEvents = await EventRepository.QueryEventsAsync(
            queryBuilder,
            TestContext.Current.CancellationToken
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.Type == targetType).ShouldBeTrue();
    }

    [Fact]
    public async Task QueryEventsWithBuilder_ShouldReturnMatchingEvents()
    {
        List<Event> events = EventRepositoryTestUtilities.GetTestEvents();
        await EventRepository.AddEventsAsync(events, TestContext.Current.CancellationToken);
        string targetType = events.First().Type;
        Guid targetITwinId = events.First().ITwinId;

        EventQueryBuilder queryBuilder = new EventQueryBuilder()
            .WhereType(targetType)
            .WhereITwinId(targetITwinId);

        IQueryable<Event> retrievedEvents = await EventRepository.QueryEventsAsync(
            queryBuilder,
            TestContext.Current.CancellationToken
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.Type == targetType && e.ITwinId == targetITwinId).ShouldBeTrue();
    }

    [Fact]
    public async Task GetAllEvents_ShouldReturnEventsOrderedById()
    {
        List<Event> events = new()
        {
            new Event
            {
                Id = GuidUtility.CreateGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event",
                Time = null,
            },
            new Event
            {
                Id = GuidUtility.CreateGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event",
                Time = null,
            },
            new Event
            {
                Id = GuidUtility.CreateGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event",
                Time = null,
            },
        };

        events = events.OrderBy(_ => GuidUtility.CreateGuid()).ToList();

        await EventRepository.AddEventsAsync(events, TestContext.Current.CancellationToken);
        IQueryable<Event> retrievedEvents = await EventRepository.QueryEventsAsync(
            new EventQueryBuilder(),
            TestContext.Current.CancellationToken
        );

        retrievedEvents
            .Select(e => e.Id)
            .ShouldBe(retrievedEvents.Select(e => e.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task GetAllEvents_ShouldHandleEmptyRepository()
    {
        IQueryable<Event> retrievedEvents = await EventRepository.QueryEventsAsync(
            new EventQueryBuilder(),
            TestContext.Current.CancellationToken
        );

        retrievedEvents.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllEvents_ShouldHandleSingleEvent()
    {
        Event singleEvent = new()
        {
            Id = GuidUtility.CreateGuid(),
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://testsource.com"),
            Type = "test.event",
            Time = null,
        };

        await EventRepository.AddEventsAsync(
            new List<Event> { singleEvent },
            TestContext.Current.CancellationToken
        );
        IQueryable<Event> retrievedEvents = await EventRepository.QueryEventsAsync(
            new EventQueryBuilder(),
            TestContext.Current.CancellationToken
        );

        retrievedEvents.Count().ShouldBe(1);
        retrievedEvents.First().ShouldBe(singleEvent);
    }

    [Fact]
    public async Task GetPaginatedEvents_ShouldReturnPaginatedResult()
    {
        List<Event> testEvents = await CreatePaginatedTestEvents("test.pagination.event", 25);
        await EventRepository.AddEventsAsync(testEvents, TestContext.Current.CancellationToken);

        // Create a query builder for pagination
        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereType("test.pagination.event");

        int pageSize = 10;
        PaginatedList<Event> paginatedResult = await EventRepository.GetPaginatedEventsAsync(
            queryBuilder,
            pageSize,
            TestContext.Current.CancellationToken
        );

        const string testContinuationToken = "test-continuation-token-123";
        EventRepositoryTestUtilities.BuildPaginationLinks(
            paginatedResult,
            "test.pagination.event",
            testContinuationToken
        );

        paginatedResult.ShouldNotBeNull();
        paginatedResult.Items.Count.ShouldBe(pageSize);
        paginatedResult.Links.ShouldNotBeNull();
        paginatedResult.Links.Self.ShouldNotBeNull();
        paginatedResult.Links.Next.ShouldNotBeNull(
            "Should have a next link since more events exist"
        );

        List<Event> events = paginatedResult.Items.ToList();
        for (int i = 0; i < events.Count - 1; i++)
        {
            events[i].Id.ShouldBeLessThan(events[i + 1].Id);
        }
    }

    [Fact]
    public async Task GetPaginatedEvents_WithContinuationToken_ShouldReturnNextPage()
    {
        List<Event> events = await CreatePaginatedTestEvents("test.pagination.continuation", 30);
        await EventRepository.AddEventsAsync(events, TestContext.Current.CancellationToken);

        EventQueryBuilder firstPageQueryBuilder = new EventQueryBuilder().WhereType(
            "test.pagination.continuation"
        );
        int pageSize = 10;
        PaginatedList<Event> firstPage = await EventRepository.GetPaginatedEventsAsync(
            firstPageQueryBuilder,
            pageSize,
            TestContext.Current.CancellationToken
        );

        List<Event> firstPageResults = firstPage.Items.ToList();

        Guid lastEventId = firstPageResults.Last().Id;
        string realContinuationToken = new ContinuationToken()
        {
            Id = lastEventId,
        }.ToBase64String();

        EventRepositoryTestUtilities.BuildPaginationLinks(
            firstPage,
            "test.pagination.continuation",
            realContinuationToken
        );

        EventQueryBuilder secondQueryBuilder = new EventQueryBuilder()
            .WhereType("test.pagination.continuation")
            .WithContinuationToken(realContinuationToken); // Use real token

        PaginatedList<Event> secondPage = await EventRepository.GetPaginatedEventsAsync(
            secondQueryBuilder,
            pageSize,
            TestContext.Current.CancellationToken
        );

        EventRepositoryTestUtilities.BuildPaginationLinks(
            secondPage,
            "test.pagination.continuation",
            "second-page-token"
        );

        secondPage.ShouldNotBeNull();
        secondPage.Items.Count.ShouldBe(pageSize);
        secondPage.Links.Next.ShouldNotBeNull("Second page should have a Next link");

        IEnumerable<Guid> firstPageIds = firstPage.Items.Select(e => e.Id);
        IEnumerable<Guid> secondPageIds = secondPage.Items.Select(e => e.Id);
        firstPageIds.Intersect(secondPageIds).ShouldBeEmpty();

        Guid maxFirstPageId = firstPageIds.Max();
        Guid minSecondPageId = secondPageIds.Min();
        minSecondPageId.ShouldBeGreaterThan(
            maxFirstPageId,
            "Second page IDs should all be greater than first page"
        );

        List<Event> secondPageResults = secondPage.Items.ToList();
        Guid lastSecondPageEventId = secondPageResults.Last().Id;
        string thirdPageToken = new ContinuationToken()
        {
            Id = lastSecondPageEventId,
        }.ToBase64String();

        EventQueryBuilder thirdQueryBuilder = new EventQueryBuilder()
            .WhereType("test.pagination.continuation")
            .WithContinuationToken(thirdPageToken);

        PaginatedList<Event> thirdPage = await EventRepository.GetPaginatedEventsAsync(
            thirdQueryBuilder,
            pageSize,
            TestContext.Current.CancellationToken
        );

        EventRepositoryTestUtilities.BuildPaginationLinks(
            thirdPage,
            "test.pagination.continuation"
        );

        thirdPage.ShouldNotBeNull();
        thirdPage.Items.Count.ShouldBe(10); // Last 10 of our 30 events
        thirdPage.Links.Next.ShouldBeNull(); // No next link on last page

        IEnumerable<Guid> thirdPageIds = thirdPage.Items.Select(e => e.Id);
        firstPageIds.Intersect(thirdPageIds).ShouldBeEmpty();
        secondPageIds.Intersect(thirdPageIds).ShouldBeEmpty();
    }

    /// <summary>
    /// Helper method to create test events for pagination tests.
    /// </summary>
    protected async Task<List<Event>> CreatePaginatedTestEvents(string eventType, int count)
    {
        List<Event> testEvents = new();

        for (int i = 0; i < count; i++)
        {
            await Task.Delay(5);

            testEvents.Add(
                new Event
                {
                    ITwinId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    SpecVersion = "1.0",
                    Source = new Uri("http://testsource.com"),
                    Type = eventType,
                    Id = Guid.CreateVersion7(),
                    Time = null, // Optional timestamp set to null by default
                    DataJson = JsonSerializer.SerializeToDocument(
                        new { Index = i, Message = $"{eventType} test event {i}" }
                    ),
                }
            );
        }

        return testEvents;
    }
}
