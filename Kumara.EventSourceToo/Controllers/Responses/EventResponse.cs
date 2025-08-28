// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSourceToo.Models;
using NodaTime;

namespace Kumara.EventSourceToo.Controllers.Responses;

public class EventResponse
{
    public Guid Id { get; set; }

    public required Guid ITwinId { get; set; }

    public required Guid AccountId { get; set; }

    public string? CorrelationId { get; set; }

    public required string Type { get; set; }

    public Guid? TriggeredByUserSubject { get; set; }

    public Instant? TriggeredByUserAt { get; set; }

    public required JsonDocument Data { get; set; }

    public Instant CreatedAt { get; set; }

    public static EventResponse FromEvent(Event @event)
    {
        return new EventResponse
        {
            Id = @event.Id,
            ITwinId = @event.ITwinId,
            AccountId = @event.AccountId,
            CorrelationId = @event.CorrelationId,
            Type = @event.Type,
            TriggeredByUserSubject = @event.TriggeredByUserSubject,
            TriggeredByUserAt = @event.TriggeredByUserAt,
            Data = @event.Data,
            CreatedAt = @event.CreatedAt,
        };
    }
}
