// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Utilities;

using Microsoft.EntityFrameworkCore;

namespace Kumara.Database;

public static class Extensions
{
    public static void RunMigrations(this IHost host)
    {
        ExecuteDatabaseAction(host, context => context.Database.Migrate());
        ExecuteDatabaseAction(host, context =>
        {
            var runner = new RunScript();
            runner.Execute(["../script/dump-schema"]);
        });
    }

    private static void ExecuteDatabaseAction(IHost host, Action<ApplicationDbContext> action)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        action(context);
    }
}
