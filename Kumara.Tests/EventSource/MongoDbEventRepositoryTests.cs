// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.DbContext;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Repositories;
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
        string testDatabaseName = $"{s_databaseName}_{Guid.NewGuid():N}";

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
        IQueryable<EventEntity> retrievedEvents = await _eventRepository.GetAllEventsAsync();

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

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder()
            .WhereITwinGuid(targetITwinGuid);
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

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder()
            .WhereAccountGuid(targetAccountGuid);
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

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder()
            .WhereCorrelationId(targetCorrelationId);
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

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder()
            .WhereType(targetType);
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
}
