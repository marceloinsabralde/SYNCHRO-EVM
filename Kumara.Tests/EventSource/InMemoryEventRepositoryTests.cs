// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Repositories;
using Kumara.EventSource.Utilities;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kumara.Tests.EventSource;

[TestClass]
public class InMemoryEventRepositoryTests
{
    private static IEventRepository CreateInMemoryRepository()
    {
        return new EventRepositoryInMemoryList();
    }

    [TestMethod]
    public async Task RoundtripEventsAsync_ShouldStoreAndRetrieveEventEntities()
    {
        IEventRepository eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();

        await eventRepository.AddEventsAsync(eventEntities);
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            new EventEntityQueryBuilder()
        );

        eventEntities
            .Select(e => e.ITwinGuid)
            .ShouldBeSubsetOf(retrievedEvents.Select(e => e.ITwinGuid));
    }

    [TestMethod]
    public async Task QueryEventsByITwinGuid_ShouldReturnMatchingEvents()
    {
        IEventRepository? eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await eventRepository.AddEventsAsync(eventEntities);
        Guid targetITwinGuid = eventEntities.First().ITwinGuid;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder().WhereITwinGuid(
            targetITwinGuid
        );
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.ITwinGuid == targetITwinGuid).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsByAccountGuid_ShouldReturnMatchingEvents()
    {
        IEventRepository? eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await eventRepository.AddEventsAsync(eventEntities);
        Guid targetAccountGuid = eventEntities.First().AccountGuid;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder().WhereAccountGuid(
            targetAccountGuid
        );
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.AccountGuid == targetAccountGuid).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsByCorrelationId_ShouldReturnMatchingEvents()
    {
        IEventRepository? eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await eventRepository.AddEventsAsync(eventEntities);
        string? targetCorrelationId = eventEntities.First().CorrelationId;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder().WhereCorrelationId(
            targetCorrelationId
        );
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.CorrelationId == targetCorrelationId).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsByType_ShouldReturnMatchingEvents()
    {
        IEventRepository? eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await eventRepository.AddEventsAsync(eventEntities);
        string? targetType = eventEntities.First().Type;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder().WhereType(targetType);
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.Type == targetType).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsWithBuilder_ShouldReturnMatchingEvents()
    {
        IEventRepository eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await eventRepository.AddEventsAsync(eventEntities);
        string targetType = eventEntities.First().Type;
        Guid targetITwinGuid = eventEntities.First().ITwinGuid;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder()
            .WhereType(targetType)
            .WhereITwinGuid(targetITwinGuid);
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents
            .All(e => e.Type == targetType && e.ITwinGuid == targetITwinGuid)
            .ShouldBeTrue();
    }

    [TestMethod]
    public async Task GetAllEvents_ShouldReturnEventsOrderedById()
    {
        IEventRepository eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = new()
        {
            new EventEntity
            {
                Id = GuidUtility.CreateGuid(),
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event",
            },
            new EventEntity
            {
                Id = GuidUtility.CreateGuid(),
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event",
            },
            new EventEntity
            {
                Id = GuidUtility.CreateGuid(),
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event",
            },
        };

        eventEntities = eventEntities.OrderBy(_ => GuidUtility.CreateGuid()).ToList();

        await eventRepository.AddEventsAsync(eventEntities);
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            new EventEntityQueryBuilder()
        );

        retrievedEvents
            .Select(e => e.Id)
            .ShouldBe(retrievedEvents.Select(e => e.Id).OrderBy(id => id));
    }

    [TestMethod]
    public async Task GetAllEvents_ShouldHandleEmptyRepository()
    {
        IEventRepository eventRepository = CreateInMemoryRepository();

        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            new EventEntityQueryBuilder()
        );

        retrievedEvents.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task GetAllEvents_ShouldHandleSingleEvent()
    {
        IEventRepository eventRepository = CreateInMemoryRepository();
        EventEntity singleEvent = new()
        {
            Id = GuidUtility.CreateGuid(),
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://testsource.com"),
            Type = "test.event",
        };

        await eventRepository.AddEventsAsync(new List<EventEntity> { singleEvent });
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            new EventEntityQueryBuilder()
        );

        retrievedEvents.Count().ShouldBe(1);
        retrievedEvents.First().ShouldBe(singleEvent);
    }

    [TestMethod]
    public async Task GetPaginatedEvents_ShouldReturnPaginatedResult()
    {
        IEventRepository eventRepository = CreateInMemoryRepository();

        List<EventEntity> eventEntities = new();

        for (int i = 0; i < 25; i++)
        {
            await Task.Delay(5);

            eventEntities.Add(
                new Event
                {
                    ITwinGuid = Guid.NewGuid(),
                    AccountGuid = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    SpecVersion = "1.0",
                    Source = new Uri("http://testsource.com"),
                    Type = "test.pagination.event",
                    Id = Guid.CreateVersion7(),
                    DataJson = JsonSerializer.SerializeToDocument(
                        new { Index = i, Message = $"Pagination test event {i}" }
                    ),
                }
            );
        }

        await eventRepository.AddEventsAsync(eventEntities);

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder().WhereType(
            "test.pagination.event"
        );

        int pageSize = 10;
        PaginatedList<Event> paginatedResult = await eventRepository.GetPaginatedEventsAsync(
            queryBuilder,
            pageSize
        );

        const string testContinuationToken = "test-continuation-token-123";
        EventRepositoryTestUtils.BuildPaginationLinks(
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

        List<EventEntity> events = paginatedResult.Items.ToList();
        for (int i = 0; i < events.Count - 1; i++)
        {
            events[i].Id.ShouldBeLessThan(events[i + 1].Id);
        }
    }

    [TestMethod]
    public async Task GetPaginatedEvents_WithContinuationToken_ShouldReturnNextPage()
    {
        IEventRepository eventRepository = CreateInMemoryRepository();

        List<Event> eventEntities = new();
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(10);
            eventEntities.Add(
                new Event
                {
                    ITwinGuid = Guid.NewGuid(),
                    AccountGuid = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    SpecVersion = "1.0",
                    Source = new Uri("http://testsource.com"),
                    Type = "test.pagination.continuation",
                    Id = Guid.CreateVersion7(),
                    DataJson = JsonSerializer.SerializeToDocument(
                        new { Index = i, Message = $"Pagination continuation test event {i}" }
                    ),
                }
            );
        }

        await eventRepository.AddEventsAsync(eventEntities);

        EventEntityQueryBuilder firstPageQueryBuilder = new EventEntityQueryBuilder().WhereType(
            "test.pagination.continuation"
        );

        int pageSize = 10;
        PaginatedList<Event> firstPage = await eventRepository.GetPaginatedEventsAsync(
            firstPageQueryBuilder,
            pageSize
        );

        List<Event> firstPageResults = firstPage.Items.ToList();
        Guid lastEventId = firstPageResults.Last().Id;
        string realContinuationToken = Pagination.CreateContinuationToken(lastEventId);

        EventRepositoryTestUtils.BuildPaginationLinks(
            firstPage,
            "test.pagination.continuation",
            realContinuationToken
        );

        EventEntityQueryBuilder secondQueryBuilder = new EventEntityQueryBuilder()
            .WhereType("test.pagination.continuation")
            .WithContinuationToken(realContinuationToken);

        PaginatedList<Event> secondPage = await eventRepository.GetPaginatedEventsAsync(
            secondQueryBuilder,
            pageSize
        );

        EventRepositoryTestUtils.BuildPaginationLinks(
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
        minSecondPageId.ShouldBeGreaterThan(maxFirstPageId);

        List<Event> secondPageResults = secondPage.Items.ToList();
        Guid lastSecondPageEventId = secondPageResults.Last().Id;
        string thirdPageToken = Pagination.CreateContinuationToken(lastSecondPageEventId);

        EventEntityQueryBuilder thirdQueryBuilder = new EventEntityQueryBuilder()
            .WhereType("test.pagination.continuation")
            .WithContinuationToken(thirdPageToken);

        PaginatedList<Event> thirdPage = await eventRepository.GetPaginatedEventsAsync(
            thirdQueryBuilder,
            pageSize
        );

        EventRepositoryTestUtils.BuildPaginationLinks(thirdPage, "test.pagination.continuation");

        thirdPage.ShouldNotBeNull();
        thirdPage.Items.Count.ShouldBe(10);
        thirdPage.Links.Next.ShouldBeNull();

        IEnumerable<Guid> thirdPageIds = thirdPage.Items.Select(e => e.Id);
        firstPageIds.Intersect(thirdPageIds).ShouldBeEmpty();
        secondPageIds.Intersect(thirdPageIds).ShouldBeEmpty();
    }
}
