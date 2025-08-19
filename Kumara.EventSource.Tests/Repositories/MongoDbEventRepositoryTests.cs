// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.DbContext;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Repositories;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Kumara.EventSource.Tests.Repositories;

public class MongoDbEventRepositoryTests : EventRepositoryTestsBase, IAsyncLifetime
{
    private MongoDbContext? _dbContext;
    private IEventRepository? _eventRepository;

    public async ValueTask InitializeAsync()
    {
        var dbFixture = await DatabaseTestFixture.GetInstanceAsync();
        var mongoUrl = new MongoUrl(dbFixture.GenerateMongoConnectionString());

        MongoClient mongoClient = new(mongoUrl);

        DbContextOptions<MongoDbContext> dbContextOptions =
            new DbContextOptionsBuilder<MongoDbContext>()
                .UseMongoDB(mongoClient, mongoUrl.DatabaseName)
                .Options;

        _dbContext = new MongoDbContext(dbContextOptions);

        await _dbContext.Database.EnsureCreatedAsync();

        _eventRepository = new EventRepositoryMongo(_dbContext);
    }

    protected override IEventRepository EventRepository =>
        _eventRepository ?? throw new InvalidOperationException("EventRepository not initialized");

    public async ValueTask DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
            _dbContext = null;
        }
    }

    // No need to implement the common test methods as they are inherited from the base class
    // Add any specialized tests specific to MongoDbEventRepository here
}
