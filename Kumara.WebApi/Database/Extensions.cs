// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Reflection;
using Kumara.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Kumara.Database;

public static class Extensions
{
    public static async Task MigrateDbAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        await EnsureViewsPopulated(dbContext);
    }

    public static void SeedDevelopmentData(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!app.Environment.IsDevelopment())
        {
            throw new Exception(
                "SeedDevelopmentData should only be executed in a development environment."
            );
        }

        DbSeeder.SeedDevelopmentData(dbContext);
    }

    private static async Task EnsureViewsPopulated(ApplicationDbContext dbContext)
    {
        var sqlViewAttrs = dbContext
            .GetType()
            .Assembly.GetTypes()
            .Select(type => type.GetCustomAttribute<SqlViewAttribute>())
            .Where(attr => attr is not null);

        foreach (var attr in sqlViewAttrs)
        {
            var viewName = attr!.Name;

            using StreamReader reader = new(Path.Combine("Models", attr.SqlFileName));
            string viewSql = reader.ReadToEnd();

#pragma warning disable EF1002
            // Using ExecuteSqlAsync instead of ExecuteSqlRawAsync here results in invalid SQL
            await dbContext.Database.ExecuteSqlRawAsync(
                $"""
                CREATE OR REPLACE VIEW public.{viewName} AS
                {viewSql};
                """
            );
#pragma warning restore EF1002
        }
    }
}
