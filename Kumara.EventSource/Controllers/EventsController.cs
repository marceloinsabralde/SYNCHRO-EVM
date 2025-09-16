// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.Common.Controllers.Extensions;
using Kumara.Common.Controllers.Responses;
using Kumara.Common.Utilities;
using Kumara.EventSource.Controllers.Requests;
using Kumara.EventSource.Controllers.Responses;
using Kumara.EventSource.Database;
using Kumara.EventSource.Models;
using Kumara.EventSource.Queries;
using Kumara.EventSource.Validations;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.EventSource.Controllers;

[Route("api/events")]
[ApiController]
[Produces("application/json")]
public class EventsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListEvents")]
    public ActionResult<PaginatedListResponse<EventResponse>> Index(
        [FromQuery] Guid? iTwinId,
        [FromQuery] Guid? accountId,
        [FromQuery] [ValidEventType] string? eventType,
        [FromQuery] string? entityType,
        [FromQuery(Name = "$continuationToken")]
            ContinuationToken<ListEventsQueryFilter>? continuationToken,
        [FromQuery(Name = "$top")] int limit = 50
    )
    {
        ListEventsQuery query = new(dbContext.Events.AsQueryable());
        ListEventsQueryFilter filter;

        if (continuationToken is not null)
            filter = continuationToken.Value;
        else
            filter = new()
            {
                ITwinId = iTwinId,
                AccountId = accountId,
                EventType = eventType,
                EntityType = entityType,
            };

        var result = query.ApplyFilter(filter).WithLimit(limit).ExecuteQuery();
        var events = result.Items;

        if (events.Count == 0)
            return NotFound();

        return Ok(
            this.BuildPaginatedResponse(events.Select(EventResponse.FromEvent), result, filter)
        );
    }

    [HttpPost]
    [EndpointName("CreateEvents")]
    public async Task<IActionResult> Create([FromBody] EventsCreateRequest eventsCreateRequest)
    {
        var idempotencyKeys = dbContext.IdempotencyKeys.Select(key => key.Key).ToHashSet();
        var newIdempotencyKeys = new List<IdempotencyKey>();

        var newEvents = eventsCreateRequest
            .Events.Where(@event =>
            {
                if (@event.IdempotencyKey is null)
                    return true;

                var isExistingKey = idempotencyKeys.Contains(@event.IdempotencyKey.Value);

                if (isExistingKey)
                    return false;

                newIdempotencyKeys.Add(new(@event.IdempotencyKey.Value));

                return true;
            })
            .Select(@event => new Event
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
            })
            .ToList();

        await Task.WhenAll(
            dbContext.Events.AddRangeAsync(newEvents),
            dbContext.IdempotencyKeys.AddRangeAsync(newIdempotencyKeys)
        );
        await dbContext.SaveChangesAsync();
        newEvents.ForEach(@event => @event.Dispose());
        return Created();
    }
}
