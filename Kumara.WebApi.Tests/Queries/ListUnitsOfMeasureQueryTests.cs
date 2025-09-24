// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Models;
using Kumara.WebApi.Queries;

namespace Kumara.WebApi.Tests.Queries;

public class ListUnitsOfMeasureQueryTests : ApplicationTestBase
{
    public static Guid ITwinId = Guid.CreateVersion7();
    public static List<UnitOfMeasure> UnitsOfMeasure = Enumerable
        .Range(0, 10)
        .Select(index =>
        {
            var timestamp = DateTimeOffset.UtcNow.AddDays(-index);
            return Factories.UnitOfMeasure(id: Guid.CreateVersion7(timestamp), iTwinId: ITwinId);
        })
        .OrderBy(uom => uom.Id)
        .ToList();

    public static UnitOfMeasure OtherITwinUnitOfMeasure = Factories.UnitOfMeasure();

    private async Task Setup()
    {
        await _dbContext.UnitsOfMeasure.AddRangeAsync(
            UnitsOfMeasure.Concat([OtherITwinUnitOfMeasure]),
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptyFilter_Test()
    {
        await Setup();

        var queryResult = new ListUnitsOfMeasureQuery(
            query: _dbContext.UnitsOfMeasure.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new ListUnitsOfMeasureQueryFilter())
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(uom => uom.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(UnitsOfMeasure);
    }

    [Fact]
    public async Task ContinueFromId_Test()
    {
        await Setup();
        var continueFromId = UnitsOfMeasure.ElementAt(4).Id;

        var queryResult = new ListUnitsOfMeasureQuery(
            query: _dbContext.UnitsOfMeasure.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new() { ContinueFromId = continueFromId })
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(uom => uom.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(UnitsOfMeasure.GetRange(5, 5));
    }

    [Fact]
    public async Task Limit_Test()
    {
        await Setup();

        var queryResult = new ListUnitsOfMeasureQuery(
            query: _dbContext.UnitsOfMeasure.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new())
            .WithLimit(5)
            .ExecuteQuery();

        queryResult.Items.Count.ShouldBe(5);
        queryResult.HasMore.ShouldBeTrue();
        queryResult.Items.ShouldAllBe(uom => uom.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(UnitsOfMeasure.GetRange(0, 5));

        queryResult = new ListUnitsOfMeasureQuery(
            query: _dbContext.UnitsOfMeasure.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new() { ContinueFromId = queryResult.LastReadId })
            .WithLimit(5)
            .ExecuteQuery();

        queryResult.HasMore.ShouldBeFalse();
        queryResult.Items.ShouldAllBe(uom => uom.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(UnitsOfMeasure.GetRange(5, 5));
    }
}
