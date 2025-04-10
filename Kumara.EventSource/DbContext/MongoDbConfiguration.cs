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
        string mongoConnectionString =
            configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";
        string mongoDatabase = configuration["MongoDB:DatabaseName"] ?? "KumaraEvents";

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
