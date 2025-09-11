// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Controllers.Requests;
using Kumara.EventSource.Models;

namespace Kumara.EventSource.Tests.Extensions;

public static class EventExtensions
{
    public static EventCreateRequest ToEventCreateRequest(
        this Event @event,
        Guid? idempotencyKey = null
    )
    {
        return new EventCreateRequest()
        {
            ITwinId = @event.ITwinId,
            AccountId = @event.AccountId,
            CorrelationId = @event.CorrelationId,
            EventType = @event.EventType,
            EntityType = @event.EntityType,
            EntityId = @event.EntityId,
            Data = @event.Data,
            TriggeredByUserSubject = @event.TriggeredByUserSubject,
            TriggeredByUserAt = @event.TriggeredByUserAt,
            IdempotencyKey = idempotencyKey,
        };
    }
}
