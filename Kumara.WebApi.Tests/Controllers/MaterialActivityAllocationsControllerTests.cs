// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Json;
using Kumara.Common.Controllers.Responses;
using Kumara.TestCommon.Extensions;
using Kumara.WebApi.Controllers.Responses;

namespace Kumara.WebApi.Tests.Controllers;

public sealed class MaterialActivityAllocationsControllerTests : DatabaseTestBase
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
            GetPathByName("ListMaterialActivityAllocations", new { iTwinId }),
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<
            ListResponse<MaterialActivityAllocationResponse>
        >();
        var allocations = apiResponse.Items.ToList();

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
    public async ValueTask Index_WithActivityFilter()
    {
        var iTwinId = Guid.CreateVersion7();
        var otherITwinId = Guid.CreateVersion7();

        // We're supplying a timestamp when we generate our UUIDs so we can control order
        var timestamp = DateTimeOffset.UtcNow.AddDays(-7);
        var activity = Factories.Activity(iTwinId: iTwinId);

        var allocation1 = Factories.MaterialActivityAllocation(
            id: Guid.CreateVersion7(timestamp.AddDays(0)),
            iTwinId: iTwinId,
            activity: activity
        );
        var allocation2 = Factories.MaterialActivityAllocation(
            id: Guid.CreateVersion7(timestamp.AddDays(1)),
            iTwinId: iTwinId,
            activity: activity
        );

        var otherActivityAllocation = Factories.MaterialActivityAllocation(
            id: Guid.CreateVersion7(timestamp.AddDays(2)),
            iTwinId: iTwinId
        );
        var otherITwinAllocation = Factories.MaterialActivityAllocation(
            id: Guid.CreateVersion7(timestamp.AddDays(3)),
            iTwinId: otherITwinId
        );
        await _dbContext.MaterialActivityAllocations.AddRangeAsync(
            [allocation1, allocation2, otherActivityAllocation, otherITwinAllocation],
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            GetPathByName(
                "ListMaterialActivityAllocations",
                new { iTwinId, activityId = activity.Id }
            ),
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<
            ListResponse<MaterialActivityAllocationResponse>
        >();
        var allocations = apiResponse.Items.ToList();

        allocations.ShouldAllBe(allocation => allocation.ITwinId == iTwinId);
        allocations.ShouldAllBe(allocation => allocation.ActivityId == activity.Id);
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
    public async ValueTask Index_WithMaterialFilter()
    {
        var iTwinId = Guid.CreateVersion7();
        var otherITwinId = Guid.CreateVersion7();

        // We're supplying a timestamp when we generate our UUIDs so we can control order
        var timestamp = DateTimeOffset.UtcNow.AddDays(-7);
        var material = Factories.Material(iTwinId: iTwinId);

        var allocation1 = Factories.MaterialActivityAllocation(
            id: Guid.CreateVersion7(timestamp.AddDays(0)),
            iTwinId: iTwinId,
            material: material
        );
        var allocation2 = Factories.MaterialActivityAllocation(
            id: Guid.CreateVersion7(timestamp.AddDays(1)),
            iTwinId: iTwinId,
            material: material
        );

        var otherMaterialAllocation = Factories.MaterialActivityAllocation(
            id: Guid.CreateVersion7(timestamp.AddDays(2)),
            iTwinId: iTwinId
        );
        var otherITwinAllocation = Factories.MaterialActivityAllocation(
            id: Guid.CreateVersion7(timestamp.AddDays(3)),
            iTwinId: otherITwinId
        );
        await _dbContext.MaterialActivityAllocations.AddRangeAsync(
            [allocation1, allocation2, otherMaterialAllocation, otherITwinAllocation],
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            GetPathByName(
                "ListMaterialActivityAllocations",
                new { iTwinId, materialId = material.Id }
            ),
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<
            ListResponse<MaterialActivityAllocationResponse>
        >();
        var allocations = apiResponse.Items.ToList();

        allocations.ShouldAllBe(allocation => allocation.ITwinId == iTwinId);
        allocations.ShouldAllBe(allocation => allocation.MaterialId == material.Id);
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
    public async ValueTask Index_PaginationTest()
    {
        var iTwinId = Guid.CreateVersion7();

        var allocations = Enumerable
            .Range(0, 10)
            .Select(index =>
            {
                var timestamp = DateTimeOffset.UtcNow.AddDays(-index);
                return Factories.MaterialActivityAllocation(
                    id: Guid.CreateVersion7(timestamp),
                    iTwinId: iTwinId
                );
            })
            .OrderBy(allocation => allocation.Id)
            .ToList();

        await _dbContext.MaterialActivityAllocations.AddRangeAsync(
            allocations,
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var requestPath = GetPathByName(
            "ListMaterialActivityAllocations",
            new { iTwinId, _top = 5 }
        );
        var response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        var apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<MaterialActivityAllocationResponse>
        >();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: true);
        var allocationsFromResponse = apiResponse.Items.ToList();

        allocationsFromResponse.ShouldNotBeNull();
        allocationsFromResponse.ShouldAllBe(allocation => allocation.ITwinId == iTwinId);
        var expectedAllocations = allocations
            .GetRange(0, 5)
            .Select(MaterialActivityAllocationResponse.FromMaterialActivityAllocation)
            .ToList();
        allocationsFromResponse.ShouldBeEquivalentTo(expectedAllocations);

        requestPath = apiResponse.Links.Next!.Href;
        response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<MaterialActivityAllocationResponse>
        >();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: false);
        allocationsFromResponse = apiResponse.Items.ToList();

        allocationsFromResponse.ShouldNotBeNull();
        allocationsFromResponse.ShouldAllBe(allocation => allocation.ITwinId == iTwinId);
        expectedAllocations = allocations
            .GetRange(5, 5)
            .Select(MaterialActivityAllocationResponse.FromMaterialActivityAllocation)
            .ToList();
        allocationsFromResponse.ShouldBeEquivalentTo(expectedAllocations);
    }

    [Fact]
    public async Task Index_WhenITwinIdMissing_BadRequest()
    {
        var response = await _client.GetAsync(
            GetPathByName("ListMaterialActivityAllocations"),
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
            GetPathByName("ListMaterialActivityAllocations", new { iTwinId }),
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }
}
