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
public class EventRepositoryTests
{
    private static MongoDbContainer? s_mongoDbContainer;
    private static MongoDbContext? s_mongoDbContext;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        s_mongoDbContainer = new MongoDbBuilder().WithPortBinding(27017, true).Build();
        await s_mongoDbContainer.StartAsync();
        IMongoDatabase mongoDatabase = new MongoClient(
            s_mongoDbContainer.GetConnectionString()
        ).GetDatabase("testdb");
        s_mongoDbContext = new MongoDbContext(
            new DbContextOptionsBuilder<MongoDbContext>()
                .UseMongoDB(mongoDatabase.Client, mongoDatabase.DatabaseNamespace.DatabaseName)
                .Options
        );
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if (s_mongoDbContainer != null)
        {
            await s_mongoDbContainer.StopAsync();
        }
    }

    private static IEventRepository CreateMongoDbRepository()
    {
        if (s_mongoDbContext == null)
        {
            throw new InvalidOperationException("MongoDB container is not initialized.");
        }

        return new EventRepositoryMongo(s_mongoDbContext);
    }

    private static IEventRepository CreateInMemoryRepository()
    {
        return new EventRepositoryInMemoryList();
    }

    public static IEnumerable<object[]> GetRepositories()
    {
        yield return new object[] { "InMemory", CreateInMemoryRepository() };
        yield return new object[] { "MongoDb", CreateMongoDbRepository() };
    }

    private List<EventEntity> GetTestEventEntities()
    {
        return new List<EventEntity>
        {
            new()
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.created.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Repository Test String",
                        TestEnum = TestOptions.OptionD,
                        TestInteger = 300,
                    }
                ),
            },
            new()
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.created.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Repository Test String",
                        TestEnum = TestOptions.OptionD,
                        TestInteger = 300,
                    }
                ),
            },
        };
    }

    [DataTestMethod]
    [DynamicData(nameof(GetRepositories), DynamicDataSourceType.Method)]
    public async Task RoundtripEventsAsync_ShouldStoreAndRetrieveEventEntities(
        string repositoryType,
        IEventRepository eventRepository
    )
    {
        List<EventEntity> eventEntities = GetTestEventEntities();

        await eventRepository.AddEventsAsync(eventEntities);
        IQueryable<EventEntity> retrievedEvents = await eventRepository.GetAllEventsAsync();

        eventEntities
            .Select(e => e.ITwinGuid)
            .ShouldBeSubsetOf(retrievedEvents.Select(e => e.ITwinGuid));
    }
}
