// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Utilities;
using Microsoft.AspNetCore.Mvc;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Kumara.EventSource.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventValidator _eventValidator;

    public EventsController(IEventRepository eventRepository, IEventValidator eventValidator)
    {
        _eventRepository = eventRepository;
        _eventValidator = eventValidator;
    }

    [HttpPost]
    public async Task<IActionResult> PostEvents([FromBody] JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Array)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid Payload",
                    Detail = "The payload must be an array of JSON objects.",
                    Instance = HttpContext.Request.Path,
                }
            );
        }

        List<EventEntity> events =
            JsonSerializer.Deserialize<List<EventEntity>>(payload.GetRawText()) ?? [];

        foreach (EventEntity eventEntity in events)
        {
            ValidationResult validationResult = _eventValidator.ValidateEvent(eventEntity);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
        }

        if (events.Count != 0)
        {
            await _eventRepository.AddEventsAsync(events);
        }

        return Ok(new { count = events.Count });
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        IQueryable<EventEntity> events = await _eventRepository.GetAllEventsAsync();
        return Ok(events);
    }
}
