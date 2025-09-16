// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource;
using Kumara.EventSource.Models;
using Kumara.EventSource.Services;

namespace Kumara.EventSource.Tests;

public class IdempotencyKeyCleanupServiceTest : ApplicationTestBase
{
    [Fact]
    public async Task CleansUpOldIdempotencyKeys()
    {
        var keys = Enumerable.Range(0, 15).Select(_ => new IdempotencyKey(Guid.CreateVersion7()));
        _dbContext.IdempotencyKeys.AddRange(keys);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await IdempotencyKeyCleanupService.RunCleanup(
            _dbContext,
            CancellationToken.None,
            maxKeys: 10
        );

        _dbContext.IdempotencyKeys.Count().ShouldBe(5);
    }
}
