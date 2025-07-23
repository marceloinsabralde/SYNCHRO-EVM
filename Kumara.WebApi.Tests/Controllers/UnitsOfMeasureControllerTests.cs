// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Json;
using Kumara.Common.Controllers.Responses;
using Kumara.TestCommon.Extensions;
using Kumara.WebApi.Controllers.Responses;

namespace Kumara.WebApi.Tests.Controllers;

public sealed class UnitsOfMeasureControllerTests : DatabaseTestBase
{
    [Fact]
    public async Task Index_Success()
    {
        var iTwinId = Guid.CreateVersion7();
        var otherITwinId = Guid.CreateVersion7();

        // We're supplying a timestamp when we generate our UUIDs so we can control order
        var timestamp = DateTimeOffset.UtcNow.AddDays(-7);

        var unitOfMeasure1 = Factories.UnitOfMeasure(
            id: Guid.CreateVersion7(timestamp.AddDays(0)),
            iTwinId: iTwinId
        );
        var unitOfMeasure2 = Factories.UnitOfMeasure(
            id: Guid.CreateVersion7(timestamp.AddDays(1)),
            iTwinId: iTwinId
        );
        var otherITwinUnitOfMeasure = Factories.UnitOfMeasure(
            id: Guid.CreateVersion7(timestamp.AddDays(2)),
            iTwinId: otherITwinId
        );
        await _dbContext.UnitsOfMeasure.AddRangeAsync(
            [unitOfMeasure1, unitOfMeasure2, otherITwinUnitOfMeasure],
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            GetPathByName("ListUnitsOfMeasure", new { iTwinId }),
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<ListResponse<UnitOfMeasureResponse>>();
        var unitsOfMeasure = apiResponse.Items.ToList();

        unitsOfMeasure.ShouldAllBe(uom => uom.ITwinId == iTwinId);
        unitsOfMeasure.Count().ShouldBe(2);
        unitsOfMeasure.ShouldBeEquivalentTo(
            new List<UnitOfMeasureResponse>
            {
                UnitOfMeasureResponse.FromUnitOfMeasure(unitOfMeasure1),
                UnitOfMeasureResponse.FromUnitOfMeasure(unitOfMeasure2),
            }
        );
    }

    [Fact]
    public async ValueTask Index_PaginationTest()
    {
        var iTwinId = Guid.CreateVersion7();

        var unitsOfMeasure = Enumerable
            .Range(0, 10)
            .Select(index =>
            {
                var timestamp = DateTimeOffset.UtcNow.AddDays(-index);
                return Factories.UnitOfMeasure(
                    id: Guid.CreateVersion7(timestamp),
                    iTwinId: iTwinId
                );
            })
            .OrderBy(uom => uom.Id)
            .ToList();

        await _dbContext.UnitsOfMeasure.AddRangeAsync(
            unitsOfMeasure,
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var requestPath = GetPathByName("ListUnitsOfMeasure", new { iTwinId, _top = 5 });
        var response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        var apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<UnitOfMeasureResponse>
        >();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: true);
        var uomsFromResponse = apiResponse.Items.ToList();

        uomsFromResponse.ShouldNotBeNull();
        uomsFromResponse.ShouldAllBe(uom => uom.ITwinId == iTwinId);
        var expectedUoms = unitsOfMeasure
            .GetRange(0, 5)
            .Select(UnitOfMeasureResponse.FromUnitOfMeasure)
            .ToList();
        uomsFromResponse.ShouldBeEquivalentTo(expectedUoms);

        requestPath = apiResponse.Links.Next!.Href;
        response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<UnitOfMeasureResponse>
        >();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: false);
        uomsFromResponse = apiResponse.Items.ToList();

        uomsFromResponse.ShouldNotBeNull();
        uomsFromResponse.ShouldAllBe(uom => uom.ITwinId == iTwinId);
        expectedUoms = unitsOfMeasure
            .GetRange(5, 5)
            .Select(UnitOfMeasureResponse.FromUnitOfMeasure)
            .ToList();
        uomsFromResponse.ShouldBeEquivalentTo(expectedUoms);
    }

    [Fact]
    public async Task Index_WhenITwinIdMissing_BadRequest()
    {
        var response = await _client.GetAsync(
            GetPathByName("ListUnitsOfMeasure"),
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorBadRequest(
            errorsPattern: @"{""iTwinId"":\[""The iTwinId field is required.""\]}"
        );
    }

    [Fact]
    public async Task Index_WhenITwinNotFound_NotFound()
    {
        var iTwinId = Guid.CreateVersion7();
        var response = await _client.GetAsync(
            GetPathByName("ListUnitsOfMeasure", new { iTwinId }),
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }
}
