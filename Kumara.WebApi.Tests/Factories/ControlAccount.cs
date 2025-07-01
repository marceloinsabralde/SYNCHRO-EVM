// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Bogus;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Tests;

public static partial class Factories
{
    private static int _controlAccountCount = 0;

    public static ControlAccount ControlAccount(
        Guid? id = null,
        Guid? iTwinId = null,
        string? referenceCode = null,
        string? name = null,
        decimal percentComplete = default,
        DateOnly? actualStart = null,
        DateOnly? actualFinish = null,
        DateOnly? plannedStart = null,
        DateOnly? plannedFinish = null
    )
    {
        _controlAccountCount++;

        return new Faker<ControlAccount>()
            .RuleFor(ca => ca.Id, id ?? Guid.CreateVersion7())
            .RuleFor(ca => ca.ITwinId, iTwinId ?? Guid.CreateVersion7())
            .RuleFor(ca => ca.ReferenceCode, referenceCode ?? $"CON{_controlAccountCount:D3}")
            .RuleFor(ca => ca.Name, name ?? $"Control Account {_controlAccountCount:D3}")
            .RuleFor(ca => ca.PercentComplete, percentComplete)
            .RuleFor(ca => ca.ActualStart, actualStart)
            .RuleFor(ca => ca.ActualFinish, actualFinish)
            .RuleFor(ca => ca.PlannedStart, plannedStart)
            .RuleFor(ca => ca.PlannedFinish, plannedFinish)
            .Generate();
    }
}
