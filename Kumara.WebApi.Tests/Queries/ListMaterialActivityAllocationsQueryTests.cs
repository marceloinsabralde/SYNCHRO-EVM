// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Models;
using Kumara.WebApi.Queries;

namespace Kumara.WebApi.Tests.Queries;

public sealed class ListMaterialActivityAllocationsQueryTests : ApplicationTestBase
{
    public static Guid ITwinId = Guid.CreateVersion7();
    public static List<MaterialActivityAllocation> Allocations = Enumerable
        .Range(0, 10)
        .Select(index =>
        {
            var timestamp = DateTimeOffset.UtcNow.AddDays(-index);
            return Factories.MaterialActivityAllocation(
                id: Guid.CreateVersion7(timestamp),
                iTwinId: ITwinId
            );
        })
        .OrderBy(allocation => allocation.Id)
        .ToList();

    public static MaterialActivityAllocation OtherITwinAllocation =
        Factories.MaterialActivityAllocation();

    private async Task Setup()
    {
        await _dbContext.MaterialActivityAllocations.AddRangeAsync(
            Allocations.Concat([OtherITwinAllocation]),
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptyFilter_Test()
    {
        await Setup();

        var queryResult = new ListMaterialActivityAllocationsQuery(
            query: _dbContext.MaterialActivityAllocations.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new ListMaterialActivityAllocationsQueryFilter())
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(allocation => allocation.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(Allocations);
    }

    [Fact]
    public async Task ActivityFilter_Test()
    {
        var newActivity = Factories.Activity(iTwinId: ITwinId);
        Allocations.GetRange(0, 5).ForEach(allocation => allocation.Activity = newActivity);

        await Setup();

        var queryResult = new ListMaterialActivityAllocationsQuery(
            query: _dbContext.MaterialActivityAllocations.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(
                new ListMaterialActivityAllocationsQueryFilter() { ActivityId = newActivity.Id }
            )
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(allocation => allocation.ITwinId == ITwinId);
        queryResult.Items.ShouldAllBe(allocation => allocation.ActivityId == newActivity.Id);
        queryResult.Items.ShouldBeEquivalentTo(Allocations.GetRange(0, 5));
    }

    [Fact]
    public async Task MaterialFilter_Test()
    {
        var newMaterial = Factories.Material(iTwinId: ITwinId);
        Allocations.GetRange(0, 5).ForEach(allocation => allocation.Material = newMaterial);

        await Setup();

        var queryResult = new ListMaterialActivityAllocationsQuery(
            query: _dbContext.MaterialActivityAllocations.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(
                new ListMaterialActivityAllocationsQueryFilter() { MaterialId = newMaterial.Id }
            )
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(allocation => allocation.ITwinId == ITwinId);
        queryResult.Items.ShouldAllBe(allocation => allocation.MaterialId == newMaterial.Id);
        queryResult.Items.ShouldBeEquivalentTo(Allocations.GetRange(0, 5));
    }

    [Fact]
    public async Task ContinueFromId_Test()
    {
        await Setup();
        var continueFromId = Allocations.ElementAt(4).Id;

        var queryResult = new ListMaterialActivityAllocationsQuery(
            query: _dbContext.MaterialActivityAllocations.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(
                new ListMaterialActivityAllocationsQueryFilter() { ContinueFromId = continueFromId }
            )
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(allocation => allocation.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(Allocations.GetRange(5, 5));
    }

    [Fact]
    public async Task ContinueFromIdWithAllFilters_Test()
    {
        var newActivity = Factories.Activity(iTwinId: ITwinId);
        var newMaterial = Factories.Material(iTwinId: ITwinId);

        foreach (var (index, allocation) in Allocations.Index())
        {
            if (index % 2 == 0)
                allocation.Activity = newActivity;
            if (index % 3 == 0)
                allocation.Material = newMaterial;
        }
        var continueFromId = Allocations.ElementAt(4).Id;

        await Setup();

        var queryResult = new ListMaterialActivityAllocationsQuery(
            query: _dbContext.MaterialActivityAllocations.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(
                new ListMaterialActivityAllocationsQueryFilter()
                {
                    ContinueFromId = continueFromId,
                    ActivityId = newActivity.Id,
                    MaterialId = newMaterial.Id,
                }
            )
            .ExecuteQuery();

        var expectedAllocations = new List<MaterialActivityAllocation> { Allocations.ElementAt(6) };
        queryResult.Items.ShouldAllBe(allocation => allocation.ITwinId == ITwinId);
        queryResult.Items.ShouldAllBe(allocation => allocation.Activity == newActivity);
        queryResult.Items.ShouldAllBe(allocation => allocation.Material == newMaterial);
        queryResult.Items.ShouldBeEquivalentTo(expectedAllocations);
    }

    [Fact]
    public async Task Limit_Test()
    {
        await Setup();

        var queryResult = new ListMaterialActivityAllocationsQuery(
            query: _dbContext.MaterialActivityAllocations.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new ListMaterialActivityAllocationsQueryFilter())
            .WithLimit(5)
            .ExecuteQuery();

        queryResult.Items.Count.ShouldBe(5);
        queryResult.HasMore.ShouldBeTrue();
        queryResult.Items.ShouldAllBe(allocation => allocation.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(Allocations.GetRange(0, 5));

        queryResult = new ListMaterialActivityAllocationsQuery(
            query: _dbContext.MaterialActivityAllocations.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(
                new ListMaterialActivityAllocationsQueryFilter()
                {
                    ContinueFromId = queryResult.LastReadId,
                }
            )
            .WithLimit(5)
            .ExecuteQuery();

        queryResult.HasMore.ShouldBeFalse();
        queryResult.Items.ShouldAllBe(allocation => allocation.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(Allocations.GetRange(5, 5));
    }
}
