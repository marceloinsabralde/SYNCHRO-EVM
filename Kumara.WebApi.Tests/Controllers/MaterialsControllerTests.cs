// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Json;
using Kumara.Common.Controllers.Responses;
using Kumara.TestCommon.Extensions;
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
            GetPathByName("ListMaterials", new { iTwinId }),
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<ListResponse<MaterialResponse>>();
        var materials = apiResponse.Items.ToList();

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
    public async ValueTask Index_PaginationTest()
    {
        var iTwinId = Guid.CreateVersion7();

        var materials = Enumerable
            .Range(0, 15)
            .Select(index =>
            {
                var timestamp = DateTimeOffset.UtcNow.AddDays(-index);
                return Factories.Material(id: Guid.CreateVersion7(timestamp), iTwinId: iTwinId);
            })
            .OrderBy(material => material.Id)
            .ToList();

        await _dbContext.Materials.AddRangeAsync(materials, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var requestPath = GetPathByName("ListMaterials", new { iTwinId, _top = 5 });
        var response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        var apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<MaterialResponse>
        >();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: true);
        var materialsFromResponse = apiResponse.Items.ToList();

        materialsFromResponse.ShouldNotBeNull();
        materialsFromResponse.ShouldAllBe(activity => activity.ITwinId == iTwinId);
        var expectedControlAccounts = materials
            .GetRange(0, 5)
            .Select(MaterialResponse.FromMaterial)
            .ToList();
        materialsFromResponse.ShouldBeEquivalentTo(expectedControlAccounts);

        requestPath = apiResponse.Links.Next!.Href;
        response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        apiResponse = await response.ShouldBeApiResponse<PaginatedListResponse<MaterialResponse>>();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: true);
        materialsFromResponse = apiResponse.Items.ToList();

        materialsFromResponse.ShouldNotBeNull();
        materialsFromResponse.ShouldAllBe(material => material.ITwinId == iTwinId);
        expectedControlAccounts = materials
            .GetRange(5, 5)
            .Select(MaterialResponse.FromMaterial)
            .ToList();
        materialsFromResponse.ShouldBeEquivalentTo(expectedControlAccounts);

        requestPath = apiResponse.Links.Next!.Href;
        response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        apiResponse = await response.ShouldBeApiResponse<PaginatedListResponse<MaterialResponse>>();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: false);
        materialsFromResponse = apiResponse.Items.ToList();

        materialsFromResponse.ShouldNotBeNull();
        materialsFromResponse.ShouldAllBe(material => material.ITwinId == iTwinId);
        expectedControlAccounts = materials
            .GetRange(10, 5)
            .Select(MaterialResponse.FromMaterial)
            .ToList();
        materialsFromResponse.ShouldBeEquivalentTo(expectedControlAccounts);
    }

    [Fact]
    public async Task Index_WhenITwinIdMissing_BadRequest()
    {
        var response = await _client.GetAsync(
            GetPathByName("ListMaterials"),
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
            GetPathByName("ListMaterials", new { iTwinId }),
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
            GetPathByName("GetMaterial", new { expected.Id }),
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<ShowResponse<MaterialResponse>>();
        var material = apiResponse.Item;
        material.ShouldBeEquivalentTo(MaterialResponse.FromMaterial(expected));
    }

    [Fact]
    public async Task Show_WhenMaterialNotFound_NotFound()
    {
        var response = await _client.GetAsync(
            GetPathByName("GetMaterial", new { Id = Guid.NewGuid() }),
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }
}
