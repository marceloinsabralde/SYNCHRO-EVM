// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.WebApi.Database;

public static class Extensions
{
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
