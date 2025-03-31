// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using CloudNative.CloudEvents;

using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Repositories;

using MongoDB.Driver;

using Shouldly;

using Testcontainers.MongoDb;

namespace Kumara.Tests.EventSource;

[TestClass]
public class EventRepositoryTests
{
    private static MongoDbContainer? s_mongoDbContainer;
    private static IMongoDatabase? s_database;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        s_mongoDbContainer = new MongoDbBuilder().Build();
        await s_mongoDbContainer.StartAsync();
        s_database = new MongoClient(s_mongoDbContainer.GetConnectionString()).GetDatabase("testdb");
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
        if (s_database == null)
        {
            throw new InvalidOperationException("MongoDB container is not initialized.");
        }

        return new EventRepositoryMongoDb(s_database);
    }

    private static IEventRepository CreateInMemoryRepository() => new EventRepositoryInMemoryList();

    public static IEnumerable<object[]> GetRepositories()
    {
        yield return ["InMemory", CreateInMemoryRepository()];
        yield return ["MongoDb", CreateMongoDbRepository()];
    }

    private List<CloudEvent> GetTestCloudEvents() =>
    [
        new CloudEvent(CloudEventsSpecVersion.V1_0)
        {
            Type = "UserLogin",
            Source = new Uri("/source/user"),
            Id = "A234-1234-1234",
            Time = new DateTimeOffset(2023, 10, 1, 12, 0, 0, TimeSpan.Zero),
            Data = new { userId = "12345", userName = "arun.malik" }
        },

        new CloudEvent(CloudEventsSpecVersion.V1_0)
        {
            Type = "FileUpload",
            Source = new Uri("/source/file"),
            Id = "B234-1234-1234",
            Time = new DateTimeOffset(2023, 10, 1, 12, 5, 0, TimeSpan.Zero),
            Data = new { userId = "12345", fileName = "report.pdf", fileSize = 102400 }
        }
    ];

    [DataTestMethod]
    [DynamicData(nameof(GetRepositories), DynamicDataSourceType.Method)]
    public async Task RoundtripEventsAsync_ShouldStoreAndRetrieveCloudEvents(string repositoryType, IEventRepository eventRepository)
    {
        // Arrange
        List<CloudEvent> cloudEvents = GetTestCloudEvents();

        // Act
        await eventRepository.AddEventsAsync(cloudEvents);
        IQueryable<CloudEvent> retrievedEvents = await eventRepository.GetAllEventsAsync();

        // Assert
        cloudEvents.Select(e => e.Id).ShouldBeSubsetOf(retrievedEvents.Select(e => e.Id));
    }
}
