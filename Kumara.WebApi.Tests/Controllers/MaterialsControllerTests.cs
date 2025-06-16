// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Json;
using Kumara.Common.Controllers.Responses;
using Kumara.WebApi.Controllers.Responses;

namespace Kumara.WebApi.Tests.Controllers;

public sealed class MaterialsControllerTests : DatabaseTestBase
{
    [Fact]
    public async Task Index_Success()
    {
        var iTwinId = Guid.CreateVersion7();
        var otherITwinId = Guid.CreateVersion7();

        // We're supplying a timestamp when we generate our UUIDs so we can control order
        var timestamp = DateTimeOffset.UtcNow.AddDays(-7);

        var material1 = Factories.Material(
            id: Guid.CreateVersion7(timestamp.AddDays(0)),
            iTwinId: iTwinId
        );
        var material2 = Factories.Material(
            id: Guid.CreateVersion7(timestamp.AddDays(1)),
            iTwinId: iTwinId
        );
        var otherITwinMaterial = Factories.Material(
            id: Guid.CreateVersion7(timestamp.AddDays(2)),
            iTwinId: otherITwinId
        );
        await _dbContext.Materials.AddRangeAsync(
            [material1, material2, otherITwinMaterial],
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            $"/api/v1/materials?iTwinId={iTwinId}",
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<ListResponse<MaterialResponse>>();
        var materials = apiResponse?.items.ToList();

        materials.ShouldNotBeNull();
        materials.ShouldAllBe(activity => activity.ITwinId == iTwinId);
        materials.Count().ShouldBe(2);
        materials.ShouldBeEquivalentTo(
            new List<MaterialResponse>
            {
                MaterialResponse.FromMaterial(material1),
                MaterialResponse.FromMaterial(material2),
            }
        );
    }

    [Fact]
    public async Task Index_WhenITwinIdMissing_BadRequest()
    {
        var response = await _client.GetAsync(
            "/api/v1/materials",
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
            $"/api/v1/materials?iTwinId={iTwinId}",
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }

    [Fact]
    public async Task Show_Success()
    {
        var expected = Factories.Material();
        await _dbContext.Materials.AddAsync(expected, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            $"/api/v1/materials/{expected.Id}",
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<ShowResponse<MaterialResponse>>();
        var material = apiResponse?.item;
        material.ShouldBeEquivalentTo(MaterialResponse.FromMaterial(expected));
    }

    [Fact]
    public async Task Show_WhenMaterialNotFound_NotFound()
    {
        var response = await _client.GetAsync(
            $"/api/v1/materials/{Guid.NewGuid()}",
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }
}
