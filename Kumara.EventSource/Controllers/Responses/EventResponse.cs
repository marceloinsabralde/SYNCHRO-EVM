// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;
using NodaTime;

namespace Kumara.EventSource.Controllers.Responses;

public class EventResponse : IDisposable
{
    public Guid Id { get; set; }

    public required Guid ITwinId { get; set; }

    public required Guid AccountId { get; set; }

    public string? CorrelationId { get; set; }

    public required string EventType { get; set; }

    public Guid? TriggeredByUserSubject { get; set; }

    public Instant? TriggeredByUserAt { get; set; }

    public required JsonDocument Data { get; set; }

    public Instant CreatedAt { get; set; }

    public void Dispose()
    {
        Data.Dispose();
    }

    public static EventResponse FromEvent(Event @event)
    {
        return new EventResponse
        {
            Id = @event.Id,
            ITwinId = @event.ITwinId,
            AccountId = @event.AccountId,
            CorrelationId = @event.CorrelationId,
            EventType = @event.EventType,
            TriggeredByUserSubject = @event.TriggeredByUserSubject,
            TriggeredByUserAt = @event.TriggeredByUserAt,
            Data = @event.Data,
            CreatedAt = @event.CreatedAt,
        };
    }
}
