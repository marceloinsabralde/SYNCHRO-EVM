// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Database;
using Microsoft.EntityFrameworkCore;

namespace Kumara.EventSource.Services;

public class IdempotencyKeyCleanupService : BackgroundService
{
    private readonly TimeSpan _frequency = TimeSpan.FromHours(8);
    private readonly IServiceProvider _serviceProvider;

    public IdempotencyKeyCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static async Task RunCleanup(
        ApplicationDbContext dbContext,
        CancellationToken stoppingToken,
        long maxKeys = 10_000
    )
    {
        await dbContext
            .IdempotencyKeys.Where(key =>
                dbContext.IdempotencyKeys.Max(idempotencyKey => idempotencyKey.Id) - maxKeys
                < key.Id
            )
            .ExecuteDeleteAsync(cancellationToken: stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_frequency);

        while (
            !stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken)
        )
        {
            using var scope = _serviceProvider.CreateScope();
            await using var dbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await RunCleanup(dbContext, stoppingToken);
        }
    }
}
