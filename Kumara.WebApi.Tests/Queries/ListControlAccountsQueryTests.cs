// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Models;
using Kumara.WebApi.Queries;

namespace Kumara.WebApi.Tests.Queries;

public class ListControlAccountsQueryTests : ApplicationTestBase
{
    public static Guid ITwinId = Guid.CreateVersion7();
    public static List<ControlAccount> ControlAccounts = Enumerable
        .Range(0, 10)
        .Select(index =>
        {
            var timestamp = DateTimeOffset.UtcNow.AddDays(-index);
            return Factories.ControlAccount(id: Guid.CreateVersion7(timestamp), iTwinId: ITwinId);
        })
        .OrderBy(controlAccount => controlAccount.Id)
        .ToList();

    public static ControlAccount OtherITwinControlAccount = Factories.ControlAccount();

    private async Task Setup()
    {
        await _dbContext.ControlAccounts.AddRangeAsync(
            ControlAccounts.Concat([OtherITwinControlAccount]),
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptyFilter_Test()
    {
        await Setup();

        var queryResult = new ListControlAccountsQuery(
            query: _dbContext.ControlAccounts.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new ListControlAccountsQueryFilter())
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(controlAccount => controlAccount.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(ControlAccounts);
    }

    [Fact]
    public async Task ContinueFromId_Test()
    {
        await Setup();
        var continueFromId = ControlAccounts.ElementAt(4).Id;

        var queryResult = new ListControlAccountsQuery(
            query: _dbContext.ControlAccounts.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new ListControlAccountsQueryFilter() { ContinueFromId = continueFromId })
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(controlAccount => controlAccount.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(ControlAccounts.GetRange(5, 5));
    }

    [Fact]
    public async Task Limit_Test()
    {
        await Setup();

        var queryResult = new ListControlAccountsQuery(
            query: _dbContext.ControlAccounts.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new ListControlAccountsQueryFilter())
            .WithLimit(5)
            .ExecuteQuery();

        queryResult.Items.Count.ShouldBe(5);
        queryResult.HasMore.ShouldBeTrue();
        queryResult.Items.ShouldAllBe(controlAccount => controlAccount.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(ControlAccounts.GetRange(0, 5));

        queryResult = new ListControlAccountsQuery(
            query: _dbContext.ControlAccounts.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(
                new ListControlAccountsQueryFilter() { ContinueFromId = queryResult.LastReadId }
            )
            .WithLimit(5)
            .ExecuteQuery();

        queryResult.HasMore.ShouldBeFalse();
        queryResult.Items.ShouldAllBe(controlAccount => controlAccount.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(ControlAccounts.GetRange(5, 5));
    }
}
