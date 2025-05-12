// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.DbContext;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Repositories;
using Kumara.EventSource.Utilities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Shouldly;
using Testcontainers.MongoDb;

namespace Kumara.Tests.EventSource;

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
    public async Task RoundtripEventsAsync_ShouldStoreAndRetrieveEventEntities()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();

        await _eventRepository.AddEventsAsync(eventEntities);
        IQueryable<EventEntity> retrievedEvents = await _eventRepository.QueryEventsAsync(
            new EventEntityQueryBuilder()
        );

        eventEntities
            .Select(e => e.ITwinGuid)
            .ShouldBeSubsetOf(retrievedEvents.Select(e => e.ITwinGuid));
    }

    [TestMethod]
    public async Task QueryEventsByITwinGuid_ShouldReturnMatchingEvents()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await _eventRepository.AddEventsAsync(eventEntities);
        Guid targetITwinGuid = eventEntities.First().ITwinGuid;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder().WhereITwinGuid(
            targetITwinGuid
        );
        IQueryable<EventEntity> retrievedEvents = await _eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.ITwinGuid == targetITwinGuid).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsByAccountGuid_ShouldReturnMatchingEvents()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await _eventRepository.AddEventsAsync(eventEntities);
        Guid targetAccountGuid = eventEntities.First().AccountGuid;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder().WhereAccountGuid(
            targetAccountGuid
        );
        IQueryable<EventEntity> retrievedEvents = await _eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.AccountGuid == targetAccountGuid).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsByCorrelationId_ShouldReturnMatchingEvents()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await _eventRepository.AddEventsAsync(eventEntities);
        string targetCorrelationId = eventEntities.First().CorrelationId;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder().WhereCorrelationId(
            targetCorrelationId
        );
        IQueryable<EventEntity> retrievedEvents = await _eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.CorrelationId == targetCorrelationId).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsByType_ShouldReturnMatchingEvents()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await _eventRepository.AddEventsAsync(eventEntities);
        string targetType = eventEntities.First().Type;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder().WhereType(targetType);
        IQueryable<EventEntity> retrievedEvents = await _eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.Type == targetType).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsWithBuilder_ShouldReturnMatchingEvents()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await _eventRepository.AddEventsAsync(eventEntities);
        string targetType = eventEntities.First().Type;
        Guid targetITwinGuid = eventEntities.First().ITwinGuid;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder()
            .WhereType(targetType)
            .WhereITwinGuid(targetITwinGuid);

        IQueryable<EventEntity> retrievedEvents = await _eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents
            .All(e => e.Type == targetType && e.ITwinGuid == targetITwinGuid)
            .ShouldBeTrue();
    }

    [TestMethod]
    public async Task GetPaginatedEvents_ShouldReturnPaginatedResult()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);

        List<EventEntity> eventEntities = [];

        for (int i = 0; i < 25; i++)
        {
            await Task.Delay(5);

            eventEntities.Add(
                new EventEntity
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

        await _eventRepository.AddEventsAsync(eventEntities);

        // Create a query builder for pagination
        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder().WhereType(
            "test.pagination.event"
        );

        int pageSize = 10;
        PaginatedList<EventEntity> paginatedResult = await _eventRepository.GetPaginatedEventsAsync(
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
        ArgumentNullException.ThrowIfNull(_eventRepository);

        List<EventEntity> eventEntities = new();

        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(10);

            eventEntities.Add(
                new EventEntity
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

        await _eventRepository.AddEventsAsync(eventEntities);

        EventEntityQueryBuilder firstPageQueryBuilder = new EventEntityQueryBuilder().WhereType(
            "test.pagination.continuation"
        );
        int pageSize = 10;
        PaginatedList<EventEntity> firstPage = await _eventRepository.GetPaginatedEventsAsync(
            firstPageQueryBuilder,
            pageSize
        );

        List<EventEntity> firstPageResults = firstPage.Items.ToList();

        Guid lastEventId = firstPageResults.Last().Id;
        string realContinuationToken = Pagination.CreateContinuationToken(lastEventId);

        EventRepositoryTestUtils.BuildPaginationLinks(
            firstPage,
            "test.pagination.continuation",
            realContinuationToken
        );

        EventEntityQueryBuilder secondQueryBuilder = new EventEntityQueryBuilder()
            .WhereType("test.pagination.continuation")
            .WithContinuationToken(realContinuationToken); // Use real token

        PaginatedList<EventEntity> secondPage = await _eventRepository.GetPaginatedEventsAsync(
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

        List<EventEntity> secondPageResults = secondPage.Items.ToList();
        Guid lastSecondPageEventId = secondPageResults.Last().Id;
        string thirdPageToken = Pagination.CreateContinuationToken(lastSecondPageEventId);

        EventEntityQueryBuilder thirdQueryBuilder = new EventEntityQueryBuilder()
            .WhereType("test.pagination.continuation")
            .WithContinuationToken(thirdPageToken);

        PaginatedList<EventEntity> thirdPage = await _eventRepository.GetPaginatedEventsAsync(
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

        await _eventRepository.AddEventsAsync(eventEntities);
        IQueryable<EventEntity> retrievedEvents = await _eventRepository.QueryEventsAsync(
            new EventEntityQueryBuilder()
        );

        retrievedEvents
            .Select(e => e.Id)
            .ShouldBe(retrievedEvents.Select(e => e.Id).OrderBy(id => id));
    }

    [TestMethod]
    public async Task GetAllEvents_ShouldHandleEmptyRepository()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);

        IQueryable<EventEntity> retrievedEvents = await _eventRepository.QueryEventsAsync(
            new EventEntityQueryBuilder()
        );

        retrievedEvents.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task GetAllEvents_ShouldHandleSingleEvent()
    {
        ArgumentNullException.ThrowIfNull(_eventRepository);
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

        await _eventRepository.AddEventsAsync(new List<EventEntity> { singleEvent });
        IQueryable<EventEntity> retrievedEvents = await _eventRepository.QueryEventsAsync(
            new EventEntityQueryBuilder()
        );

        retrievedEvents.Count().ShouldBe(1);
        retrievedEvents.First().ShouldBe(singleEvent);
    }
}
