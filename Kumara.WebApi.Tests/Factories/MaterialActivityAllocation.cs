// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Bogus;
using Kumara.Models;

namespace Kumara.WebApi.Tests;

public static partial class Factories
{
    public static MaterialActivityAllocation MaterialActivityAllocation(
        Guid? id = null,
        Guid? iTwinId = null,
        Activity? activity = null,
        Material? material = null,
        UnitOfMeasure? quantityUnitOfMeasure = null,
        decimal? quantityAtComplete = null
    )
    {
        return new Faker<MaterialActivityAllocation>()
            .RuleFor(maa => maa.Id, id ?? Guid.CreateVersion7())
            .RuleFor(maa => maa.ITwinId, iTwinId ?? Guid.CreateVersion7())
            .RuleFor(
                maa => maa.Activity,
                (faker, maa) => activity ?? Activity(iTwinId: maa.ITwinId)
            )
            .RuleFor(
                maa => maa.Material,
                (faker, maa) => material ?? Material(iTwinId: maa.ITwinId)
            )
            .RuleFor(
                maa => maa.QuantityUnitOfMeasure,
                (faker, maa) => quantityUnitOfMeasure ?? UnitOfMeasure(iTwinId: maa.ITwinId)
            )
            .RuleFor(maa => maa.QuantityAtComplete, quantityAtComplete ?? 110m)
            .Generate();
    }
}
