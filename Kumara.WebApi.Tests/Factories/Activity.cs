// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Bogus;
using Kumara.WebApi.Enums;
using Kumara.WebApi.Models;
using Kumara.WebApi.Types;
using NodaTime;

namespace Kumara.WebApi.Tests;

public static partial class Factories
{
    private static int _activityCount = 0;

    public static Activity Activity(
        Guid? id = null,
        Guid? iTwinId = null,
        ControlAccount? controlAccount = null,
        string? referenceCode = null,
        string? name = null,
        decimal percentComplete = default,
        ActivityProgressType progressType = ActivityProgressType.Manual,
        DateWithOptionalTime? actualStart = null,
        DateWithOptionalTime? actualFinish = null,
        DateWithOptionalTime? plannedStart = null,
        DateWithOptionalTime? plannedFinish = null
    )
    {
        _activityCount++;

        return new Faker<Activity>()
            .RuleFor(a => a.Id, id ?? Guid.CreateVersion7())
            .RuleFor(a => a.ITwinId, iTwinId ?? Guid.CreateVersion7())
            .RuleFor(
                a => a.ControlAccount,
                (faker, a) => controlAccount ?? ControlAccount(iTwinId: a.ITwinId)
            )
            .RuleFor(a => a.ReferenceCode, referenceCode ?? $"ACT{_activityCount:D3}")
            .RuleFor(a => a.Name, name ?? $"Activity {_activityCount:D3}")
            .RuleFor(a => a.PercentComplete, percentComplete)
            .RuleFor(a => a.ProgressType, progressType)
            .RuleFor(a => a.ActualStart, actualStart)
            .RuleFor(a => a.ActualFinish, actualFinish)
            .RuleFor(a => a.PlannedStart, plannedStart)
            .RuleFor(a => a.PlannedFinish, plannedFinish)
            .Generate();
    }
}
