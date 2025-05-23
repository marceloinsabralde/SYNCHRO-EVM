// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.DbContext;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Repositories;
using Kumara.EventSource.Utilities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Shouldly;
using Testcontainers.MongoDb;

namespace Kumara.EventSource.Tests;

[TestClass]
public class MongoDbEventRepositoryTests
{
    private static readonly MongoDbContainer s_mongoDbContainer = new MongoDbBuilder()
        .WithCleanUp(true)
        .Build();

    private static string s_connectionString = string.Empty;
    private static string s_databaseName = string.Empty;

    private MongoDbContext? _dbContext;
    private IEventRepository? _eventRepository;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext testContext)
    {
        await s_mongoDbContainer.StartAsync();

        s_connectionString = s_mongoDbContainer.GetConnectionString();
        s_databaseName = "event_test_db";
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await s_mongoDbContainer.StopAsync();
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
        string testDatabaseName = $"{s_databaseName}_{GuidUtility.CreateGuid():N}";

        MongoClient mongoClient = new(s_connectionString);

        DbContextOptions<MongoDbContext> dbContextOptions =
            new DbContextOptionsBuilder<MongoDbContext>()
                .UseMongoDB(mongoClient, testDatabaseName)
                .Options;

        _dbContext = new MongoDbContext(dbContextOptions);

        await _dbContext.Database.EnsureCreatedAsync();

        _eventRepository = new EventRepositoryMongo(_dbContext);
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        if (_dbContext != null)
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
            _dbContext = null;
        }
    }

    [TestMethod]
    public async Task RoundtripEventsAsync_ShouldStoreAndRetrieveEvents()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<Event> events = EventRepositoryTestUtils.GetTestEvents();

        await _eventRepository.AddEventsAsync(events);
        IQueryable<Event> retrievedEvents = await _eventRepository.QueryEventsAsync(
            new EventQueryBuilder()
        );

        events.Select(e => e.ITwinGuid).ShouldBeSubsetOf(retrievedEvents.Select(e => e.ITwinGuid));
    }

    [TestMethod]
    public async Task QueryEventsByITwinGuid_ShouldReturnMatchingEvents()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<Event> events = EventRepositoryTestUtils.GetTestEvents();
        await _eventRepository.AddEventsAsync(events);
        Guid targetITwinGuid = events.First().ITwinGuid;

        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereITwinGuid(targetITwinGuid);
        IQueryable<Event> retrievedEvents = await _eventRepository.QueryEventsAsync(queryBuilder);

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.ITwinGuid == targetITwinGuid).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsByAccountGuid_ShouldReturnMatchingEvents()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<Event> events = EventRepositoryTestUtils.GetTestEvents();
        await _eventRepository.AddEventsAsync(events);
        Guid targetAccountGuid = events.First().AccountGuid;

        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereAccountGuid(
            targetAccountGuid
        );
        IQueryable<Event> retrievedEvents = await _eventRepository.QueryEventsAsync(queryBuilder);

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.AccountGuid == targetAccountGuid).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsByCorrelationId_ShouldReturnMatchingEvents()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<Event> events = EventRepositoryTestUtils.GetTestEvents();
        await _eventRepository.AddEventsAsync(events);
        string targetCorrelationId = events.First().CorrelationId;

        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereCorrelationId(
            targetCorrelationId
        );
        IQueryable<Event> retrievedEvents = await _eventRepository.QueryEventsAsync(queryBuilder);

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.CorrelationId == targetCorrelationId).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsByType_ShouldReturnMatchingEvents()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<Event> events = EventRepositoryTestUtils.GetTestEvents();
        await _eventRepository.AddEventsAsync(events);
        string targetType = events.First().Type;

        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereType(targetType);
        IQueryable<Event> retrievedEvents = await _eventRepository.QueryEventsAsync(queryBuilder);

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.Type == targetType).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsWithBuilder_ShouldReturnMatchingEvents()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<Event> events = EventRepositoryTestUtils.GetTestEvents();
        await _eventRepository.AddEventsAsync(events);
        string targetType = events.First().Type;
        Guid targetITwinGuid = events.First().ITwinGuid;

        EventQueryBuilder queryBuilder = new EventQueryBuilder()
            .WhereType(targetType)
            .WhereITwinGuid(targetITwinGuid);

        IQueryable<Event> retrievedEvents = await _eventRepository.QueryEventsAsync(queryBuilder);

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents
            .All(e => e.Type == targetType && e.ITwinGuid == targetITwinGuid)
            .ShouldBeTrue();
    }

    [TestMethod]
    public async Task GetPaginatedEvents_ShouldReturnPaginatedResult()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);

        List<Event> testEvents = [];

        for (int i = 0; i < 25; i++)
        {
            await Task.Delay(5);

            testEvents.Add(
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

        await _eventRepository.AddEventsAsync(testEvents);

        // Create a query builder for pagination
        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereType("test.pagination.event");

        int pageSize = 10;
        PaginatedList<Event> paginatedResult = await _eventRepository.GetPaginatedEventsAsync(
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

        List<Event> events = paginatedResult.Items.ToList();
        for (int i = 0; i < events.Count - 1; i++)
        {
            events[i].Id.ShouldBeLessThan(events[i + 1].Id);
        }
    }

    [TestMethod]
    public async Task GetPaginatedEvents_WithContinuationToken_ShouldReturnNextPage()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);

        List<Event> events = new();

        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(10);

            events.Add(
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

        await _eventRepository.AddEventsAsync(events);

        EventQueryBuilder firstPageQueryBuilder = new EventQueryBuilder().WhereType(
            "test.pagination.continuation"
        );
        int pageSize = 10;
        PaginatedList<Event> firstPage = await _eventRepository.GetPaginatedEventsAsync(
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

        EventQueryBuilder secondQueryBuilder = new EventQueryBuilder()
            .WhereType("test.pagination.continuation")
            .WithContinuationToken(realContinuationToken); // Use real token

        PaginatedList<Event> secondPage = await _eventRepository.GetPaginatedEventsAsync(
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
        minSecondPageId.ShouldBeGreaterThan(
            maxFirstPageId,
            "Second page IDs should all be greater than first page"
        );

        List<Event> secondPageResults = secondPage.Items.ToList();
        Guid lastSecondPageEventId = secondPageResults.Last().Id;
        string thirdPageToken = Pagination.CreateContinuationToken(lastSecondPageEventId);

        EventQueryBuilder thirdQueryBuilder = new EventQueryBuilder()
            .WhereType("test.pagination.continuation")
            .WithContinuationToken(thirdPageToken);

        PaginatedList<Event> thirdPage = await _eventRepository.GetPaginatedEventsAsync(
            thirdQueryBuilder,
            pageSize
        );

        EventRepositoryTestUtils.BuildPaginationLinks(thirdPage, "test.pagination.continuation");

        thirdPage.ShouldNotBeNull();
        thirdPage.Items.Count.ShouldBe(10); // Last 10 of our 30 events
        thirdPage.Links.Next.ShouldBeNull(); // No next link on last page

        IEnumerable<Guid> thirdPageIds = thirdPage.Items.Select(e => e.Id);
        firstPageIds.Intersect(thirdPageIds).ShouldBeEmpty();
        secondPageIds.Intersect(thirdPageIds).ShouldBeEmpty();
    }

    [TestMethod]
    public async Task GetAllEvents_ShouldReturnEventsOrderedById()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<Event> events = new()
        {
            new Event
            {
                Id = GuidUtility.CreateGuid(),
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event",
            },
            new Event
            {
                Id = GuidUtility.CreateGuid(),
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event",
            },
            new Event
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

        events = events.OrderBy(_ => GuidUtility.CreateGuid()).ToList();

        await _eventRepository.AddEventsAsync(events);
        IQueryable<Event> retrievedEvents = await _eventRepository.QueryEventsAsync(
            new EventQueryBuilder()
        );

        retrievedEvents
            .Select(e => e.Id)
            .ShouldBe(retrievedEvents.Select(e => e.Id).OrderBy(id => id));
    }

    [TestMethod]
    public async Task GetAllEvents_ShouldHandleEmptyRepository()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);

        IQueryable<Event> retrievedEvents = await _eventRepository.QueryEventsAsync(
            new EventQueryBuilder()
        );

        retrievedEvents.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task GetAllEvents_ShouldHandleSingleEvent()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        Event singleEvent = new()
        {
            Id = GuidUtility.CreateGuid(),
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://testsource.com"),
            Type = "test.event",
        };

        await _eventRepository.AddEventsAsync(new List<Event> { singleEvent });
        IQueryable<Event> retrievedEvents = await _eventRepository.QueryEventsAsync(
            new EventQueryBuilder()
        );

        retrievedEvents.Count().ShouldBe(1);
        retrievedEvents.First().ShouldBe(singleEvent);
    }
}
