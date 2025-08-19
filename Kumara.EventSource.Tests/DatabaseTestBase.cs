// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Tests;
using Kumara.TestCommon.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Testcontainers.MongoDb;

[assembly: AssemblyFixture(typeof(DatabaseTestFixture))]

namespace Kumara.EventSource.Tests;

public class DatabaseTestFixture : IAsyncLifetime
{
    public MongoDbContainer mongo = null!;

    public static async ValueTask<DatabaseTestFixture> GetInstanceAsync()
    {
        return (await TestContext.Current.GetFixture<DatabaseTestFixture>())!;
    }

    public async ValueTask InitializeAsync()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });
        var logger = loggerFactory.CreateLogger<DatabaseTestFixture>();

        mongo = new MongoDbBuilder()
            .WithImage(Environment.GetEnvironmentVariable("MONGO_IMAGE"))
            .WithUsername(GenerateRandomString())
            .WithPassword(GenerateRandomString())
            .WithCleanUp(true)
            .WithLogger(logger)
            .Build();

        await mongo.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (mongo is not null)
            await mongo.DisposeAsync();
    }

    private static string GenerateRandomString()
    {
        return Guid.NewGuid().ToString("N");
    }

    public string GenerateMongoConnectionString()
    {
        var urlBuilder = new MongoUrlBuilder(mongo.GetConnectionString());
        urlBuilder.AuthenticationSource = "admin";
        urlBuilder.DatabaseName = GenerateRandomString();
        return urlBuilder.ToString();
    }
}

public abstract class DatabaseTestBase : IAsyncLifetime
{
    protected AppServicesHelper.AppFactory _factory = null!;
    protected HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        var dbFixture = await DatabaseTestFixture.GetInstanceAsync();
        var connectionString = dbFixture.GenerateMongoConnectionString();

        _factory = AppServicesHelper.CreateWebApplicationFactory(builder =>
        {
            builder.UseSetting($"ConnectionStrings:KumaraEventSourceDB", connectionString);
        });
        _client = _factory.CreateClient();
    }

    public ValueTask DisposeAsync()
    {
        _factory?.Dispose();

        return ValueTask.CompletedTask;
    }
}
