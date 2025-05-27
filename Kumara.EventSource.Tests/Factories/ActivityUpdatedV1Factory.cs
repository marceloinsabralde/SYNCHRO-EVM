// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models.Events;

namespace Kumara.EventSource.Tests.Factories;

public class ActivityUpdatedV1Factory
{
    public static ActivityUpdatedV1 Create(
        Guid? id = null,
        string? name = null,
        string? referenceCode = null,
        Guid? controlAccountId = null,
        DateTimeOffset? plannedStart = null,
        DateTimeOffset? plannedFinish = null,
        DateTimeOffset? actualStart = null,
        DateTimeOffset? actualFinish = null
    )
    {
        return new ActivityUpdatedV1
        {
            Id = id ?? Guid.NewGuid(),
            Name = name ?? $"Test Activity {Guid.NewGuid().ToString()[..8]}",
            ReferenceCode = referenceCode ?? $"ACT-{Guid.NewGuid().ToString()[..6]}",
            ControlAccountId = controlAccountId ?? Guid.NewGuid(),
            PlannedStart = plannedStart,
            PlannedFinish = plannedFinish,
            ActualStart = actualStart,
            ActualFinish = actualFinish,
        };
    }
}
