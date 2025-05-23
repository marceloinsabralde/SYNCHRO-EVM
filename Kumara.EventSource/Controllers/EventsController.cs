// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Extensions;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
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
    public async Task<IActionResult> PostEvents(
        [FromBody] JsonElement payload,
        CancellationToken cancellationToken = default
    )
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

        List<Event> events = JsonSerializer.Deserialize<List<Event>>(payload.GetRawText()) ?? [];

        foreach (Event @event in events)
        {
            ValidationResult validationResult = _eventValidator.ValidateEvent(@event);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
        }

        if (events.Count != 0)
        {
            await _eventRepository.AddEventsAsync(events, cancellationToken);
        }

        return Ok(new { count = events.Count });
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents(CancellationToken cancellationToken = default)
    {
        IQueryCollection query = HttpContext.Request.Query;

        string? continuationToken = null;

        if (
            query.TryGetValue("continuationtoken", out StringValues tokenValues)
            && !string.IsNullOrWhiteSpace(tokenValues)
        )
        {
            continuationToken = tokenValues.ToString();
        }

        QueryParsingResult queryParsingResult;

        if (!string.IsNullOrEmpty(continuationToken))
        {
            Pagination.ContinuationToken? token = Pagination.ParseContinuationToken(
                continuationToken
            );
            if (token != null)
            {
                QueryCollection tokenParams = new(
                    token.QueryParameters.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new StringValues(kvp.Value)
                    )
                );

                Dictionary<string, StringValues> paramsWithToken = new(
                    tokenParams.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                );
                paramsWithToken["continuationtoken"] = continuationToken;

                queryParsingResult = new QueryCollection(paramsWithToken).ToEventQueryBuilder();
            }
            else
            {
                return BadRequest(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Invalid Continuation Token",
                        Detail = "The provided continuation token is invalid or malformed.",
                        Instance = HttpContext.Request.Path,
                        Extensions = { ["invalidParameter"] = "continuationtoken" },
                    }
                );
            }
        }
        else
        {
            queryParsingResult = query.ToEventQueryBuilder();
        }

        if (!queryParsingResult.IsSuccess)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = queryParsingResult.ErrorTitle,
                    Detail = queryParsingResult.ErrorDetail,
                    Instance = HttpContext.Request.Path,
                    Extensions = { ["invalidParameter"] = queryParsingResult.InvalidParameterName },
                }
            );
        }

        PaginatedList<Event> paginatedList = await _eventRepository.GetPaginatedEventsAsync(
            queryParsingResult.QueryBuilder!,
            queryParsingResult.PageSize,
            cancellationToken
        );

        BuildPaginationLinks(paginatedList, query, queryParsingResult.QueryBuilder!);

        return Ok(paginatedList);
    }

    private void BuildPaginationLinks<T>(
        PaginatedList<T> paginatedList,
        IQueryCollection query,
        EventQueryBuilder queryBuilder
    )
    {
        HttpRequest request = HttpContext.Request;
        string baseUrl = $"{request.Scheme}://{request.Host}{request.Path}";

        Dictionary<string, string> queryParams = new();

        if (queryBuilder.TokenQueryParameters.Count > 0)
        {
            foreach (KeyValuePair<string, string> param in queryBuilder.TokenQueryParameters)
            {
                queryParams[param.Key] = param.Value;
            }
        }
        else
        {
            foreach (KeyValuePair<string, StringValues> param in query)
            {
                if (
                    param.Key.ToLowerInvariant() != "continuationtoken"
                    && !string.IsNullOrEmpty(param.Value.ToString())
                )
                {
                    string value = param.Value.ToString() ?? string.Empty;
                    queryParams[param.Key] = value;
                }
            }
        }

        string? continuationToken = null;
        if (paginatedList.Items.Count > 0 && paginatedList.HasMoreItems)
        {
            T? lastItem = paginatedList.Items.LastOrDefault();
            if (lastItem != null)
            {
                Guid lastItemId = ((dynamic)lastItem).Id;

                continuationToken = Pagination.CreateContinuationToken(lastItemId, queryParams);
            }
        }

        paginatedList.SetPaginationLinks(
            paginatedList.HasMoreItems,
            continuationToken,
            baseUrl,
            queryParams
        );
    }
}

public class QueryCollection(Dictionary<string, StringValues> dictionary)
    : Dictionary<string, StringValues>(dictionary, StringComparer.OrdinalIgnoreCase),
        IQueryCollection
{
    ICollection<string> IQueryCollection.Keys => base.Keys.ToList();
}
