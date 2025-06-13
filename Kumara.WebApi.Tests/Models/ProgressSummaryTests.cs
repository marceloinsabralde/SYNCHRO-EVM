// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.TestCommon.Extensions;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Tests.Models;

public sealed class ProgressSummaryTests : DatabaseTestBase
{
    [Fact]
    public void ContainsSummaryData()
    {
        var iTwinId = Guid.CreateVersion7();

        var uom = Factories.UnitOfMeasure(iTwinId: iTwinId);
        _dbContext.UnitsOfMeasure.Add(uom);

        var activity = Factories.Activity(iTwinId: iTwinId);
        var activity2 = Factories.Activity(iTwinId: iTwinId);
        _dbContext.Activities.AddRange([activity, activity2]);

        var material = Factories.Material(iTwinId: iTwinId, quantityUnitOfMeasure: uom);
        _dbContext.Materials.Add(material);

        var materialActivityAllocation = Factories.MaterialActivityAllocation(
            iTwinId: iTwinId,
            activity: activity,
            material: material,
            quantityUnitOfMeasure: uom,
            quantityAtComplete: 110m
        );
        var materialActivityAllocation2 = Factories.MaterialActivityAllocation(
            iTwinId: iTwinId,
            activity: activity,
            quantityUnitOfMeasure: uom,
            quantityAtComplete: 140m
        );
        var materialActivity2Allocation = Factories.MaterialActivityAllocation(
            iTwinId: iTwinId,
            activity: activity2,
            material: material,
            quantityUnitOfMeasure: uom,
            quantityAtComplete: 328m
        );
        var otherITwinMaterialActivityAllocation = Factories.MaterialActivityAllocation();
        _dbContext.MaterialActivityAllocations.AddRange(
            [
                materialActivityAllocation,
                materialActivityAllocation2,
                materialActivity2Allocation,
                otherITwinMaterialActivityAllocation,
            ]
        );

        ProgressEntry[] activityProgressEntries =
        [
            Factories.ProgressEntry(
                materialActivityAllocation: materialActivityAllocation,
                progressDate: new(2025, 04, 08),
                quantityDelta: 5m
            ),
            Factories.ProgressEntry(
                materialActivityAllocation: materialActivityAllocation,
                progressDate: new(2025, 04, 09),
                quantityDelta: 7m
            ),
            Factories.ProgressEntry(
                materialActivityAllocation: materialActivityAllocation2,
                progressDate: new(2025, 04, 08),
                quantityDelta: 10m
            ),
            Factories.ProgressEntry(
                materialActivityAllocation: materialActivityAllocation2,
                progressDate: new(2025, 04, 09),
                quantityDelta: 20m
            ),
        ];
        ProgressEntry[] activity2ProgressEntries =
        [
            Factories.ProgressEntry(
                materialActivityAllocation: materialActivity2Allocation,
                progressDate: new(2025, 04, 06),
                quantityDelta: 8m
            ),
            Factories.ProgressEntry(
                materialActivityAllocation: materialActivity2Allocation,
                progressDate: new(2025, 04, 07),
                quantityDelta: 5m
            ),
        ];
        var otherITwinProgressEntry = Factories.ProgressEntry(
            materialActivityAllocation: otherITwinMaterialActivityAllocation
        );
        _dbContext.ProgressEntries.AddRange(
            [
                activityProgressEntries[0],
                activityProgressEntries[1],
                activityProgressEntries[2],
                activityProgressEntries[3],
                activity2ProgressEntries[0],
                activity2ProgressEntries[1],
                otherITwinProgressEntry,
            ]
        );
        _dbContext.SaveChanges();

        var progressSummaries = _dbContext.ProgressSummaries.ToList();
        progressSummaries.ShouldNotBeNull();
        progressSummaries.Count().ShouldBe(4);
        progressSummaries.ShouldBeEquivalentTo(
            new List<ProgressSummary>
            {
                new ProgressSummary
                {
                    ITwinId = iTwinId,
                    ActivityId = activity.Id,
                    MaterialId = material.Id,
                    QuantityUnitOfMeasureId = uom.Id,
                    QuantityAtComplete = 110.0m,
                    QuantityToDate = 12m,
                    QuantityToComplete = 110.0m - 12m,
                    RecentProgressEntries =
                    [
                        new RecentProgressEntry
                        {
                            Id = activityProgressEntries[1].Id,
                            QuantityDelta = 7m,
                            ProgressDate = new(2025, 04, 09),
                            CreatedAt = activityProgressEntries[1].CreatedAt,
                            UpdatedAt = activityProgressEntries[1].UpdatedAt,
                        },
                        new RecentProgressEntry
                        {
                            Id = activityProgressEntries[0].Id,
                            QuantityDelta = 5m,
                            ProgressDate = new(2025, 04, 08),
                            CreatedAt = activityProgressEntries[0].CreatedAt,
                            UpdatedAt = activityProgressEntries[0].UpdatedAt,
                        },
                    ],
                },
                new ProgressSummary
                {
                    ITwinId = iTwinId,
                    ActivityId = activity.Id,
                    MaterialId = materialActivityAllocation2.MaterialId,
                    QuantityUnitOfMeasureId = uom.Id,
                    QuantityAtComplete = 140.0m,
                    QuantityToDate = 30m,
                    QuantityToComplete = 140.0m - 30m,
                    RecentProgressEntries =
                    [
                        new RecentProgressEntry
                        {
                            Id = activityProgressEntries[3].Id,
                            QuantityDelta = 20m,
                            ProgressDate = new(2025, 04, 09),
                            CreatedAt = activityProgressEntries[3].CreatedAt,
                            UpdatedAt = activityProgressEntries[3].UpdatedAt,
                        },
                        new RecentProgressEntry
                        {
                            Id = activityProgressEntries[2].Id,
                            QuantityDelta = 10m,
                            ProgressDate = new(2025, 04, 08),
                            CreatedAt = activityProgressEntries[2].CreatedAt,
                            UpdatedAt = activityProgressEntries[2].UpdatedAt,
                        },
                    ],
                },
                new ProgressSummary
                {
                    ITwinId = iTwinId,
                    ActivityId = activity2.Id,
                    MaterialId = material.Id,
                    QuantityUnitOfMeasureId = uom.Id,
                    QuantityAtComplete = 328.0m,
                    QuantityToDate = 13m,
                    QuantityToComplete = 328.0m - 13m,
                    RecentProgressEntries =
                    [
                        new RecentProgressEntry
                        {
                            Id = activity2ProgressEntries[1].Id,
                            QuantityDelta = 5m,
                            ProgressDate = new(2025, 04, 07),
                            CreatedAt = activity2ProgressEntries[1].CreatedAt,
                            UpdatedAt = activity2ProgressEntries[1].UpdatedAt,
                        },
                        new RecentProgressEntry
                        {
                            Id = activity2ProgressEntries[0].Id,
                            QuantityDelta = 8m,
                            ProgressDate = new(2025, 04, 06),
                            CreatedAt = activity2ProgressEntries[0].CreatedAt,
                            UpdatedAt = activity2ProgressEntries[0].UpdatedAt,
                        },
                    ],
                },
                new ProgressSummary
                {
                    ITwinId = otherITwinProgressEntry.ITwinId,
                    ActivityId = otherITwinProgressEntry.Activity.Id,
                    MaterialId = otherITwinProgressEntry.Material.Id,
                    QuantityUnitOfMeasureId = otherITwinProgressEntry.QuantityUnitOfMeasure.Id,
                    QuantityAtComplete = otherITwinMaterialActivityAllocation.QuantityAtComplete,
                    QuantityToDate = otherITwinProgressEntry.QuantityDelta,
                    QuantityToComplete =
                        otherITwinMaterialActivityAllocation.QuantityAtComplete
                        - otherITwinProgressEntry.QuantityDelta,
                    RecentProgressEntries =
                    [
                        new RecentProgressEntry
                        {
                            Id = otherITwinProgressEntry.Id,
                            QuantityDelta = otherITwinProgressEntry.QuantityDelta,
                            ProgressDate = otherITwinProgressEntry.ProgressDate,
                            CreatedAt = otherITwinProgressEntry.CreatedAt,
                            UpdatedAt = otherITwinProgressEntry.UpdatedAt,
                        },
                    ],
                },
            }
        );
    }

    [Fact]
    public void LimitsRecentProgressEntriesTo10MostRecent()
    {
        var iTwinId = Guid.CreateVersion7();
        var materialActivityAllocation = Factories.MaterialActivityAllocation(iTwinId: iTwinId);

        _dbContext.MaterialActivityAllocations.Add(materialActivityAllocation);

        int progressEntryCount = 15;
        var progressEntries = Enumerable
            .Range(0, progressEntryCount)
            .Select(i =>
            {
                var progressDate = DateOnly.FromDateTime(
                    DateTime.Today.SubtractDays(progressEntryCount - i)
                );
                return Factories.ProgressEntry(
                    materialActivityAllocation: materialActivityAllocation,
                    progressDate: progressDate
                );
            })
            .ToList();

        _dbContext.ProgressEntries.AddRange(progressEntries);
        _dbContext.SaveChanges();

        var progressSummary = _dbContext.ProgressSummaries.Single();
        progressSummary.ShouldNotBeNull();
        progressSummary.RecentProgressEntries.ShouldBeEquivalentTo(
            progressEntries
                .GetRange(5, 10)
                .Select(pe => new RecentProgressEntry
                {
                    Id = pe.Id,
                    QuantityDelta = pe.QuantityDelta,
                    ProgressDate = pe.ProgressDate,
                    CreatedAt = pe.CreatedAt,
                    UpdatedAt = pe.UpdatedAt,
                })
                .Reverse()
                .ToList()
        );
    }
}
