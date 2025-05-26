// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.DbContext;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Repositories;
using Kumara.EventSource.Utilities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Kumara.EventSource.Tests.Repositories;

public class MongoDbEventRepositoryTests : EventRepositoryTestsBase, IDisposable
{
    private static readonly MongoDbContainer SMongoDbContainer;
    private static readonly string s_connectionString;
    private static readonly string s_databaseName;

    private MongoDbContext? _dbContext;
    private readonly IEventRepository? _eventRepository;

    static MongoDbEventRepositoryTests()
    {
        SMongoDbContainer = new MongoDbBuilder()
            .WithImage(Environment.GetEnvironmentVariable("MONGO_IMAGE"))
            .WithCleanUp(true)
            .Build();

        SMongoDbContainer.StartAsync().GetAwaiter().GetResult();

        s_connectionString = SMongoDbContainer.GetConnectionString();
        s_databaseName = "event_test_db";
    }

    public MongoDbEventRepositoryTests()
    {
        string testDatabaseName = $"{s_databaseName}_{GuidUtility.CreateGuid():N}";

        MongoClient mongoClient = new(s_connectionString);

        DbContextOptions<MongoDbContext> dbContextOptions =
            new DbContextOptionsBuilder<MongoDbContext>()
                .UseMongoDB(mongoClient, testDatabaseName)
                .Options;

        _dbContext = new MongoDbContext(dbContextOptions);

        _dbContext.Database.EnsureCreatedAsync().GetAwaiter().GetResult();

        _eventRepository = new EventRepositoryMongo(_dbContext);
    }

    protected override IEventRepository EventRepository =>
        _eventRepository ?? throw new InvalidOperationException("EventRepository not initialized");

    public void Dispose()
    {
        if (_dbContext != null)
        {
            _dbContext.Database.EnsureDeletedAsync().GetAwaiter().GetResult();
            _dbContext.DisposeAsync().GetAwaiter().GetResult();
            _dbContext = null;
        }
    }

    // No need to implement the common test methods as they are inherited from the base class
    // Add any specialized tests specific to MongoDbEventRepository here
}
