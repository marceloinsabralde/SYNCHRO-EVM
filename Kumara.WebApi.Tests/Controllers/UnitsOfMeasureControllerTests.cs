// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Json;
using Kumara.Common.Controllers.Responses;
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
            $"/api/v1/units-of-measure?iTwinId={iTwinId}",
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<ListResponse<UnitOfMeasureResponse>>();
        var unitsOfMeasure = apiResponse?.items.ToList();

        unitsOfMeasure.ShouldNotBeNull();
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
    public async Task Index_WhenITwinIdMissing_BadRequest()
    {
        var response = await _client.GetAsync(
            "/api/v1/units-of-measure",
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
            $"/api/v1/units-of-measure?iTwinId={iTwinId}",
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }
}
