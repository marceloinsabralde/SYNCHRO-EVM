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
            RunScript.SchemaDump();
        }
    }
}
