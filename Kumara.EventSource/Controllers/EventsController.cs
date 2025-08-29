// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

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
        [FromQuery] [ValidEventType] string? type,
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
                Type = type,
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
    [EndpointName("CreateEvent")]
    public async Task<IActionResult> Create([FromBody] EventCreateRequest eventCreateRequest)
    {
        using (eventCreateRequest)
        {
            using var newEvent = new Event
            {
                ITwinId = eventCreateRequest.ITwinId,
                AccountId = eventCreateRequest.AccountId,
                CorrelationId = eventCreateRequest.CorrelationId,
                Type = eventCreateRequest.Type,
                Data = eventCreateRequest.Data,
                TriggeredByUserSubject = eventCreateRequest.TriggeredByUserSubject,
                TriggeredByUserAt = eventCreateRequest.TriggeredByUserAt,
            };

            if (eventCreateRequest.Id.HasValue)
                newEvent.Id = eventCreateRequest.Id.Value;

            await dbContext.Events.AddAsync(newEvent);
            await dbContext.SaveChangesAsync();

            return Created();
        }
    }
}
