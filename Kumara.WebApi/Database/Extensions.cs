// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
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
        if (app.Environment.IsDevelopment())
        {
            ExternalScript.SchemaDump();
        }
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
}
