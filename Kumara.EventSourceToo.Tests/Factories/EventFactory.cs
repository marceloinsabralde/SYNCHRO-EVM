// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using System.Text.Json;
using Bogus;
using Kumara.Common.Attributes;
using Kumara.Common.EventTypes;
using Kumara.EventSourceToo.Models;
using NodaTime;

namespace Kumara.EventSourceToo.Tests.Factories;

public static class EventFactory
{
    private static int _activityCount = 0;

    public static Event CreateEvent(
        string eventType,
        JsonDocument eventData,
        Guid? id = null,
        Guid? iTwinId = null,
        Guid? accountId = null,
        string? correlationId = null,
        Guid? triggeredByUserSubject = null,
        Instant? triggeredByUserAt = null
    )
    {
        return new Faker<Event>()
            .RuleFor(e => e.Id, id ?? Guid.CreateVersion7())
            .RuleFor(e => e.ITwinId, iTwinId ?? Guid.CreateVersion7())
            .RuleFor(e => e.AccountId, accountId ?? Guid.CreateVersion7())
            .RuleFor(e => e.CorrelationId, correlationId)
            .RuleFor(e => e.Type, eventType)
            .RuleFor(e => e.TriggeredByUserSubject, triggeredByUserSubject)
            .RuleFor(e => e.TriggeredByUserAt, triggeredByUserAt)
            .RuleFor(e => e.Data, eventData)
            .Generate();
    }

    public static Event CreateActivityCreatedV1Event(
        Guid? activityId = null,
        Guid? controlAccountId = null,
        string? referenceCode = null,
        string? name = null,
        DateTimeOffset? plannedStart = null,
        DateTimeOffset? plannedFinish = null,
        DateTimeOffset? actualStart = null,
        DateTimeOffset? actualFinish = null,
        Guid? eventId = null,
        Guid? iTwinId = null,
        Guid? accountId = null,
        string? correlationId = null,
        Guid? triggeredByUserSubject = null,
        Instant? triggeredByUserAt = null
    )
    {
        _activityCount++;

        var activityCreatedV1Event = new Faker<ActivityCreatedV1>()
            .RuleFor(a => a.Id, activityId ?? Guid.CreateVersion7())
            .RuleFor(a => a.ControlAccountId, controlAccountId ?? Guid.CreateVersion7())
            .RuleFor(a => a.ReferenceCode, referenceCode ?? $"ACT{_activityCount:D3}")
            .RuleFor(a => a.Name, name ?? $"Activity {_activityCount:D3}")
            .RuleFor(a => a.ActualStart, actualStart)
            .RuleFor(a => a.ActualFinish, actualFinish)
            .RuleFor(a => a.PlannedStart, plannedStart)
            .RuleFor(a => a.PlannedFinish, plannedFinish)
            .Generate();

        var eventTypeName = GetEventTypeName(activityCreatedV1Event);
        var eventData = JsonSerializer.SerializeToDocument(activityCreatedV1Event);

        return CreateEvent(
            eventTypeName,
            eventData,
            eventId,
            iTwinId,
            accountId,
            correlationId,
            triggeredByUserSubject,
            triggeredByUserAt
        );
    }

    public static Event CreateActivityDeletedV1Event(
        Guid? id = null,
        Guid? eventId = null,
        Guid? iTwinId = null,
        Guid? accountId = null,
        string? correlationId = null,
        Guid? triggeredByUserSubject = null,
        Instant? triggeredByUserAt = null
    )
    {
        var activityDeletedV1Event = new Faker<ActivityDeletedV1>()
            .RuleFor(a => a.Id, id ?? Guid.CreateVersion7())
            .Generate();

        var eventTypeName = GetEventTypeName(activityDeletedV1Event);
        var eventData = JsonSerializer.SerializeToDocument(activityDeletedV1Event);

        return CreateEvent(
            eventTypeName,
            eventData,
            eventId,
            iTwinId,
            accountId,
            correlationId,
            triggeredByUserSubject,
            triggeredByUserAt
        );
    }

    private static string GetEventTypeName(object eventTypeInstance) =>
        eventTypeInstance.GetType().GetCustomAttribute<EventTypeAttribute>()!.Name;
}
