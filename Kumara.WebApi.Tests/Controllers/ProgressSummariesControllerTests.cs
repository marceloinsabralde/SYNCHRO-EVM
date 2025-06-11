// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Json;
using Kumara.WebApi.Controllers.Responses;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Tests.Controllers;

public sealed class ProgressSummariesControllerTests : DatabaseTestBase
{
    [Fact]
    public async Task Index_Success()
    {
        var iTwinId = Guid.CreateVersion7();

        var uom = Factories.UnitOfMeasure(iTwinId: iTwinId);
        _dbContext.UnitsOfMeasure.Add(uom);

        var activity = Factories.Activity(iTwinId: iTwinId);
        _dbContext.Activities.AddRange(activity);

        var material = Factories.Material(iTwinId: iTwinId, quantityUnitOfMeasure: uom);
        _dbContext.Materials.Add(material);

        var materialActivityAllocation = Factories.MaterialActivityAllocation(
            iTwinId: iTwinId,
            material: material,
            activity: activity,
            quantityUnitOfMeasure: uom
        );
        _dbContext.MaterialActivityAllocations.Add(materialActivityAllocation);

        ProgressEntry progressEntry = Factories.ProgressEntry(
            materialActivityAllocation: materialActivityAllocation,
            progressDate: new(2025, 04, 08),
            quantityDelta: 5m
        );

        var otherITwinMaterialActivityAllocation = Factories.MaterialActivityAllocation();
        var otherITwinProgressEntry = Factories.ProgressEntry(otherITwinMaterialActivityAllocation);
        _dbContext.ProgressEntries.AddRange([progressEntry, otherITwinProgressEntry]);
        _dbContext.SaveChanges();

        var response = await _client.GetAsync(
            $"/api/v1/progress-summaries?iTwinId={iTwinId}",
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<ListResponse<ProgressSummary>>();
        var progressSummaries = apiResponse?.items.ToList();

        progressSummaries.ShouldNotBeNull();
        progressSummaries.Count().ShouldBe(1);
        progressSummaries.ShouldBeEquivalentTo(
            new List<ProgressSummary>
            {
                new()
                {
                    ITwinId = iTwinId,
                    ActivityId = activity.Id,
                    MaterialId = material.Id,
                    QuantityUnitOfMeasureId = uom.Id,
                    QuantityAtComplete = 110.0m,
                    QuantityToDate = 5m,
                    QuantityToComplete = 110.0m - 5m,
                    RecentProgressEntries =
                    [
                        new RecentProgressEntry
                        {
                            Id = progressEntry.Id,
                            QuantityDelta = 5m,
                            ProgressDate = new(2025, 04, 08),
                            CreatedAt = progressEntry.CreatedAt,
                            UpdatedAt = progressEntry.UpdatedAt,
                        },
                    ],
                },
            }
        );
    }

    [Fact]
    public async Task Index_WhenITwinIdMissing_BadRequest()
    {
        var response = await _client.GetAsync(
            "/api/v1/progress-summaries",
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
            $"/api/v1/progress-summaries?iTwinId={iTwinId}",
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }

    [Fact]
    public async Task Index_WhenActivityNotFound_NotFound()
    {
        var allocation = Factories.MaterialActivityAllocation();
        await _dbContext.MaterialActivityAllocations.AddAsync(
            allocation,
            TestContext.Current.CancellationToken
        );
        var progressEntry = Factories.ProgressEntry(allocation);
        await _dbContext.ProgressEntries.AddAsync(
            progressEntry,
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            $"/api/v1/progress-summaries?iTwinId={progressEntry.ITwinId}&activityId={Guid.CreateVersion7()}&materialId={progressEntry.MaterialId}",
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }

    [Fact]
    public async Task Index_WhenMaterialNotFound_NotFound()
    {
        var allocation = Factories.MaterialActivityAllocation();
        await _dbContext.MaterialActivityAllocations.AddAsync(
            allocation,
            TestContext.Current.CancellationToken
        );
        var progressEntry = Factories.ProgressEntry(allocation);
        await _dbContext.ProgressEntries.AddAsync(
            progressEntry,
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            $"/api/v1/progress-summaries?iTwinId={progressEntry.ITwinId}&activityId={progressEntry.ActivityId}&materialId={Guid.CreateVersion7()}",
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }
}
