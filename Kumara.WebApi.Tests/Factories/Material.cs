// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Bogus;
using Kumara.Models;

namespace Kumara.WebApi.Tests;

public static partial class Factories
{
    public static Material Material(
        Guid? id = null,
        Guid? iTwinId = null,
        string? name = null,
        Guid? resourceRoleId = null,
        UnitOfMeasure? quantityUnitOfMeasure = null
    )
    {
        return new Faker<Material>()
            .RuleFor(m => m.Id, id ?? Guid.CreateVersion7())
            .RuleFor(m => m.ITwinId, iTwinId ?? Guid.CreateVersion7())
            .RuleFor(m => m.Name, faker => name ?? faker.Name.FullName())
            .RuleFor(m => m.ResourceRoleId, resourceRoleId ?? Guid.CreateVersion7())
            .RuleFor(
                m => m.QuantityUnitOfMeasure,
                (_faker, m) => quantityUnitOfMeasure ?? UnitOfMeasure(iTwinId: m.ITwinId)
            )
            .Generate();
    }
}
