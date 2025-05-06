// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Kumara.EventSource.DbContext;

public static class MongoDbConfiguration
{
    public static void AddMongoDbContext(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string? mongoConnectionString = configuration.GetConnectionString("KumaraEventSource");

        ArgumentException.ThrowIfNullOrEmpty(
            mongoConnectionString,
            "ConnectionStrings__KumaraEventSource"
        );

        MongoUrl mongoUrl = MongoUrl.Create(mongoConnectionString);
        if (string.IsNullOrWhiteSpace(mongoUrl.DatabaseName))
        {
            throw new InvalidOperationException(
                "The MongoDB connection string must contain a database name"
            );
        }

        string mongoDatabase = mongoUrl.DatabaseName;

        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            return new MongoClient(mongoConnectionString);
        });

        services.AddDbContext<MongoDbContext>(
            (serviceProvider, options) =>
            {
                IMongoClient mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
                IMongoDatabase? database = mongoClient.GetDatabase(mongoDatabase);

                options.UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName);
            }
        );
    }
}
