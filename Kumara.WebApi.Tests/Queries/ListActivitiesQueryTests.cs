// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Models;
using Kumara.WebApi.Queries;
using NodaTime;

namespace Kumara.WebApi.Tests.Queries;

public sealed class ListActivitiesQueryTests : DatabaseTestBase
{
    public static Guid ITwinId = Guid.CreateVersion7();
    public static List<Activity> Activities = Enumerable
        .Range(0, 10)
        .Select(index =>
        {
            var timestamp = DateTimeOffset.UtcNow.AddDays(-index);
            return Factories.Activity(
                id: Guid.CreateVersion7(timestamp),
                iTwinId: ITwinId,
                plannedStart: OffsetDateTime.FromDateTimeOffset(timestamp)
            );
        })
        .OrderBy(activity => activity.Id)
        .ToList();

    public static Activity OtherITwinActivity = Factories.Activity();

    private async Task Setup()
    {
        await _dbContext.Activities.AddRangeAsync(
            Activities.Concat([OtherITwinActivity]),
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptyFilter_Test()
    {
        await Setup();

        var queryResult = new ListActivitiesQuery(
            query: _dbContext.Activities.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new ListActivitiesQueryFilter())
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(activity => activity.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(Activities);
    }

    [Fact]
    public async Task ControlAccountFilter_Test()
    {
        var newControlAccount = Factories.ControlAccount(iTwinId: ITwinId);
        Activities.GetRange(0, 5).ForEach(activity => activity.ControlAccount = newControlAccount);

        await Setup();

        var queryResult = new ListActivitiesQuery(
            query: _dbContext.Activities.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(
                new ListActivitiesQueryFilter() { ControlAccountId = newControlAccount.Id }
            )
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(activity => activity.ITwinId == ITwinId);
        queryResult.Items.ShouldAllBe(activity =>
            activity.ControlAccountId == newControlAccount.Id
        );
        queryResult.Items.ShouldBeEquivalentTo(Activities.GetRange(0, 5));
    }

    [Fact]
    public async Task ContinueFromId_Test()
    {
        await Setup();
        var continueFromId = Activities.ElementAt(4).Id;

        var queryResult = new ListActivitiesQuery(
            query: _dbContext.Activities.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new ListActivitiesQueryFilter() { ContinueFromId = continueFromId })
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(activity => activity.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(Activities.GetRange(5, 5));
    }

    [Fact]
    public async Task ContinueFromIdWithControlAccountFilter_Test()
    {
        var newControlAccount = Factories.ControlAccount(iTwinId: ITwinId);
        foreach (var (index, activity) in Activities.Index())
        {
            if (index % 2 == 0)
                activity.ControlAccount = newControlAccount;
        }
        var continueFromId = Activities.ElementAt(4).Id;

        await Setup();

        var queryResult = new ListActivitiesQuery(
            query: _dbContext.Activities.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(
                new ListActivitiesQueryFilter()
                {
                    ContinueFromId = continueFromId,
                    ControlAccountId = newControlAccount.Id,
                }
            )
            .ExecuteQuery();

        var expectedActivities = new List<Activity>
        {
            Activities.ElementAt(6),
            Activities.ElementAt(8),
        };
        queryResult.Items.ShouldAllBe(activity => activity.ITwinId == ITwinId);
        queryResult.Items.ShouldAllBe(activity => activity.ControlAccount == newControlAccount);
        queryResult.Items.ShouldBeEquivalentTo(expectedActivities);
    }

    [Fact]
    public async Task Limit_Test()
    {
        await Setup();

        var queryResult = new ListActivitiesQuery(
            query: _dbContext.Activities.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new ListActivitiesQueryFilter())
            .WithLimit(5)
            .ExecuteQuery();

        queryResult.Items.Count.ShouldBe(5);
        queryResult.HasMore.ShouldBeTrue();
        queryResult.Items.ShouldAllBe(activity => activity.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(Activities.GetRange(0, 5));

        queryResult = new ListActivitiesQuery(
            query: _dbContext.Activities.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(
                new ListActivitiesQueryFilter() { ContinueFromId = queryResult.LastReadId }
            )
            .WithLimit(5)
            .ExecuteQuery();

        queryResult.HasMore.ShouldBeFalse();
        queryResult.Items.ShouldAllBe(activity => activity.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(Activities.GetRange(5, 5));
    }
}
