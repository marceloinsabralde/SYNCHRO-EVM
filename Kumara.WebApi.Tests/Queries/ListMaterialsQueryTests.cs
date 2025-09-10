// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Models;
using Kumara.WebApi.Queries;

namespace Kumara.WebApi.Tests.Queries;

public class ListMaterialsQueryTests : ApplicationTestBase
{
    public static Guid ITwinId = Guid.CreateVersion7();
    public static List<Material> Materials = Enumerable
        .Range(0, 10)
        .Select(index =>
        {
            var timestamp = DateTimeOffset.UtcNow.AddDays(-index);
            return Factories.Material(id: Guid.CreateVersion7(timestamp), iTwinId: ITwinId);
        })
        .OrderBy(material => material.Id)
        .ToList();

    public static Material OtherITwinMaterial = Factories.Material();

    private async Task Setup()
    {
        await _dbContext.Materials.AddRangeAsync(
            Materials.Concat([OtherITwinMaterial]),
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptyFilter_Test()
    {
        await Setup();

        var queryResult = new ListMaterialsQuery(
            query: _dbContext.Materials.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new ListMaterialsQueryFilter())
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(material => material.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(Materials);
    }

    [Fact]
    public async Task ContinueFromId_Test()
    {
        await Setup();
        var continueFromId = Materials.ElementAt(4).Id;

        var queryResult = new ListMaterialsQuery(
            query: _dbContext.Materials.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new() { ContinueFromId = continueFromId })
            .ExecuteQuery();

        queryResult.Items.ShouldAllBe(material => material.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(Materials.GetRange(5, 5));
    }

    [Fact]
    public async Task Limit_Test()
    {
        await Setup();

        var queryResult = new ListMaterialsQuery(
            query: _dbContext.Materials.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new())
            .WithLimit(5)
            .ExecuteQuery();

        queryResult.Items.Count.ShouldBe(5);
        queryResult.HasMore.ShouldBeTrue();
        queryResult.Items.ShouldAllBe(material => material.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(Materials.GetRange(0, 5));

        queryResult = new ListMaterialsQuery(
            query: _dbContext.Materials.AsQueryable(),
            iTwinId: ITwinId
        )
            .ApplyFilter(new() { ContinueFromId = queryResult.LastReadId })
            .WithLimit(5)
            .ExecuteQuery();

        queryResult.HasMore.ShouldBeFalse();
        queryResult.Items.ShouldAllBe(material => material.ITwinId == ITwinId);
        queryResult.Items.ShouldBeEquivalentTo(Materials.GetRange(5, 5));
    }
}
