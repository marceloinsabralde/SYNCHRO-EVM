// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Bogus;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Tests;

public static partial class Factories
{
    private static int _unitOfMeasureCount = 0;

    public static UnitOfMeasure UnitOfMeasure(
        Guid? id = null,
        Guid? iTwinId = null,
        string? name = null,
        string? symbol = null
    )
    {
        _unitOfMeasureCount++;

        return new Faker<UnitOfMeasure>()
            .RuleFor(uom => uom.Id, id ?? Guid.CreateVersion7())
            .RuleFor(uom => uom.ITwinId, iTwinId ?? Guid.CreateVersion7())
            .RuleFor(uom => uom.Name, name ?? $"UoM {_unitOfMeasureCount:D3}")
            .RuleFor(uom => uom.Symbol, symbol ?? $"uom{_unitOfMeasureCount}")
            .Generate();
    }
}
