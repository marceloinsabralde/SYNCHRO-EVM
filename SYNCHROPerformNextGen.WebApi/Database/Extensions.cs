// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Microsoft.EntityFrameworkCore;

namespace SYNCHROPerformNextGen.Database;

public static class Extensions
{
    public static void RunMigrations(this IHost host)
    {
        ExecuteDatabaseAction(host, context => context.Database.Migrate());
    }

    private static void ExecuteDatabaseAction(IHost host, Action<ApplicationDbContext> action)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        action(context);
    }
}
