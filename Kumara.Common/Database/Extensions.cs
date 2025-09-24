// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kumara.Common.Database;

public static class Extensions
{
    public static async Task MigrateDbAsync<TContext>(this WebApplication app)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        await dbContext.Database.MigrateAsync();
        await EnsureViewsPopulated(dbContext);
    }

    public static async Task EnsureViewsPopulated(DbContext dbContext)
    {
        var sqlViewAttrs = dbContext
            .GetType()
            .Assembly.GetTypes()
            .SelectMany(type => type.GetCustomAttributes<SqlViewAttribute>(false));

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
