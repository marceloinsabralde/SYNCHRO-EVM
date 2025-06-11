// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Bogus;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Tests;

public static partial class Factories
{
    public static ProgressEntry ProgressEntry(
        Guid? id = null,
        Guid? iTwinId = null,
        Activity? activity = null,
        Material? material = null,
        UnitOfMeasure? quantityUnitOfMeasure = null,
        decimal? quantityDelta = null,
        DateOnly? progressDate = null
    )
    {
        return new Faker<ProgressEntry>()
            .RuleFor(pe => pe.Id, id ?? Guid.CreateVersion7())
            .RuleFor(pe => pe.ITwinId, iTwinId ?? Guid.CreateVersion7())
            .RuleFor(pe => pe.Activity, (faker, pe) => activity ?? Activity(iTwinId: pe.ITwinId))
            .RuleFor(pe => pe.Material, (faker, pe) => material ?? Material(iTwinId: pe.ITwinId))
            .RuleFor(
                pe => pe.QuantityUnitOfMeasure,
                (faker, pe) => quantityUnitOfMeasure ?? UnitOfMeasure(iTwinId: pe.ITwinId)
            )
            .RuleFor(pe => pe.QuantityDelta, quantityDelta ?? 1m)
            .RuleFor(pe => pe.ProgressDate, progressDate ?? DateOnly.FromDateTime(DateTime.Today))
            .Generate();
    }

    public static ProgressEntry ProgressEntry(
        MaterialActivityAllocation materialActivityAllocation,
        Guid? id = null,
        decimal? quantityDelta = null,
        DateOnly? progressDate = null
    )
    {
        return ProgressEntry(
            id: id,
            iTwinId: materialActivityAllocation.ITwinId,
            material: materialActivityAllocation.Material,
            activity: materialActivityAllocation.Activity,
            quantityUnitOfMeasure: materialActivityAllocation.QuantityUnitOfMeasure,
            quantityDelta: quantityDelta,
            progressDate: progressDate
        );
    }
}
