// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Net;
using System.Net.Http.Json;
using Kumara.WebApi.Controllers.Responses;

namespace Kumara.WebApi.Tests.Controllers;

public sealed class MaterialActivityAllocationsControllerTests : ControllerTestsBase
{
    [Fact]
    public async ValueTask Index_Success()
    {
        var iTwinId = Guid.CreateVersion7();
        var otherITwinId = Guid.CreateVersion7();

        // We're supplying a timestamp when we generate our UUIDs so we can control order
        var timestamp = DateTimeOffset.UtcNow.AddDays(-7);

        var allocation1 = Factories.MaterialActivityAllocation(
            id: Guid.CreateVersion7(timestamp.AddDays(0)),
            iTwinId: iTwinId
        );
        var allocation2 = Factories.MaterialActivityAllocation(
            id: Guid.CreateVersion7(timestamp.AddDays(1)),
            iTwinId: iTwinId
        );
        var otherITwinAllocation = Factories.MaterialActivityAllocation(
            id: Guid.CreateVersion7(timestamp.AddDays(2)),
            iTwinId: otherITwinId
        );
        await _dbContext.MaterialActivityAllocations.AddRangeAsync(
            [allocation1, allocation2, otherITwinAllocation],
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            $"/api/v1/material-activity-allocations?iTwinId={iTwinId}",
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var apiResponse = await response.Content.ReadFromJsonAsync<
            ListResponse<MaterialActivityAllocationResponse>
        >(TestContext.Current.CancellationToken);
        var allocations = apiResponse?.items;

        allocations.ShouldNotBeNull();
        allocations.ShouldAllBe(allocation => allocation.ITwinId == iTwinId);
        allocations.Count().ShouldBe(2);
        allocations.ShouldBeEquivalentTo(
            new List<MaterialActivityAllocationResponse>
            {
                MaterialActivityAllocationResponse.FromMaterialActivityAllocation(allocation1),
                MaterialActivityAllocationResponse.FromMaterialActivityAllocation(allocation2),
            }
        );
    }

    [Fact]
    public async Task Index_WhenITwinIdMissing_BadRequest()
    {
        var response = await _client.GetAsync(
            "/api/v1/material-activity-allocations",
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
            $"/api/v1/material-activity-allocations?iTwinId={iTwinId}",
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }
}
