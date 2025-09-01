// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;
using Kumara.EventSource.Tests.Factories;
using Microsoft.EntityFrameworkCore;

namespace Kumara.EventSource.Tests.Database;

public sealed class CitusTests : DatabaseTestBase
{
    [Fact]
    public async Task DistributesEventsAcrossShardsByAccountId()
    {
        var accountId1 = Guid.CreateVersion7();
        var event1 = EventFactory.CreateActivityCreatedV1Event(accountId: accountId1);
        var event2 = EventFactory.CreateActivityCreatedV1Event(accountId: accountId1);

        var accountId2 = Guid.CreateVersion7();
        var event3 = EventFactory.CreateActivityCreatedV1Event(accountId: accountId2);

        await _dbContext.Events.AddRangeAsync(
            [event1, event2, event3],
            TestContext.Current.CancellationToken
        );

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var shardId1 = await GetShardId(accountId1);
        var shard1Events = await GetEventsForShard(shardId1);

        shard1Events.ShouldContain(event1);
        shard1Events.ShouldContain(event2);

        var shardId2 = await GetShardId(accountId2);
        var shard2Events = await GetEventsForShard(shardId2);

        shard2Events.ShouldContain(event3);
    }

    private record CitusShard(int Id);

    private async Task<int> GetShardId(Guid distributionValue, string table = "events")
    {
        var row = await _dbContext
            .Database.SqlQuery<CitusShard>(
                $"SELECT get_shard_id_for_distribution_column({table}, {distributionValue}) AS id"
            )
            .FirstAsync(TestContext.Current.CancellationToken);
        return row.Id;
    }

    private async Task<List<Event>> GetEventsForShard(int shardId)
    {
        var sql = $"SELECT * FROM events_{shardId}";
        var rows = await _dbContext
            .Events.FromSqlRaw(sql)
            .ToListAsync(TestContext.Current.CancellationToken);
        return rows;
    }
}
